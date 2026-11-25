using Dapper;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    public IList<AccountListItem> GetAccounts(HrmUserAccountModel actor)
    {
        if (!AccountPolicy.CanManageAccounts(actor?.RoleCode)) throw new UnauthorizedAccessException();
        using var db = OpenConnection();
        return db.Query<AccountListItem>(@"SELECT u.Id,u.Username,u.DisplayName,p.EmployeeCode,u.RoleCode,d.Name DepartmentName,
                u.IsActive,u.MustChangePassword,u.LastLoginAt
            FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
            LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
            WHERE u.RoleCode IN @Roles OR u.Id=@ActorId
            ORDER BY u.IsActive DESC,d.Name,u.DisplayName",
            new { Roles = AccountPolicy.AssignableRoles(actor.RoleCode), ActorId = actor.Id }).ToList();
    }

    /// <summary>Creates the account and an empty profile, and returns the temporary password to hand over once.</summary>
    public string CreateAccount(CreateAccountRequest request, HrmUserAccountModel actor, string ip)
    {
        if (!AccountPolicy.CanManageAccounts(actor?.RoleCode)) throw new UnauthorizedAccessException();
        var username = request.Username?.Trim().ToLowerInvariant();
        var error = AccountPolicy.ValidateUsername(username);
        if (error != null) throw new InvalidOperationException(error);
        if (!AccountPolicy.AssignableRoles(actor.RoleCode).Contains(request.RoleCode))
            throw new InvalidOperationException("Bạn không được cấp vai trò này.");
        if (!EmployeeProfileOptions.EmploymentStatuses.Contains(request.EmploymentStatus))
            throw new InvalidOperationException("Hình thức lao động không hợp lệ.");
        var password = AccountPolicy.TemporaryPassword();
        var (hash, salt) = HrmPasswordHasher.Hash(password);
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(System.Data.IsolationLevel.Serializable);
        if (db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmUserAccount WHERE Username=@username", new { username }, tx) > 0)
            throw new InvalidOperationException("Tài khoản này đã tồn tại.");
        if (db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmEmployeeProfile WHERE EmployeeCode=@Code", new { Code = request.EmployeeCode.Trim() }, tx) > 0)
            throw new InvalidOperationException("Mã nhân viên này đã được sử dụng.");
        if (request.DepartmentId.HasValue && db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmDepartment WHERE Id=@Id AND IsActive=1", new { Id = request.DepartmentId }, tx) == 0)
            throw new InvalidOperationException("Phòng ban không hợp lệ.");
        var id = db.ExecuteScalar<int>(@"INSERT dbo.HrmUserAccount(Username,PasswordHash,PasswordSalt,DisplayName,RoleCode,DepartmentId,MustChangePassword)
            OUTPUT INSERTED.Id VALUES(@username,@hash,@salt,@DisplayName,@RoleCode,@DepartmentId,1)",
            new { username, hash, salt, DisplayName = request.DisplayName.Trim(), request.RoleCode, request.DepartmentId }, tx);
        db.Execute(@"INSERT dbo.HrmEmployeeProfile(UserId,EmployeeCode,JobTitle,HireDate,CompanyEmail,EmploymentStatus,AnnualLeaveDays)
            VALUES(@id,@EmployeeCode,@JobTitle,@HireDate,@CompanyEmail,@EmploymentStatus,12)",
            new { id, EmployeeCode = request.EmployeeCode.Trim(), JobTitle = request.JobTitle?.Trim(), request.HireDate,
                CompanyEmail = request.CompanyEmail?.Trim(), request.EmploymentStatus }, tx);
        AddAudit(db, actor.Id, "CREATE", "HrmUserAccount", id.ToString(), $"Tạo tài khoản {username} ({request.RoleCode})", ip, tx);
        tx.Commit();
        return password;
    }

    public (string Username, string Password) ResetPassword(int userId, HrmUserAccountModel actor, string ip)
    {
        var target = FindUser(userId);
        if (!AccountPolicy.CanManageAccounts(actor?.RoleCode) || !AccountPolicy.CanManage(actor, target)) throw new UnauthorizedAccessException();
        var password = AccountPolicy.TemporaryPassword();
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        StorePassword(db, tx, userId, password, mustChange: true);
        AddAudit(db, actor.Id, "RESET_PASSWORD", "HrmUserAccount", userId.ToString(), $"Cấp lại mật khẩu cho {target.Username}", ip, tx);
        tx.Commit();
        return (target.Username, password);
    }

    public void SetAccountActive(int userId, bool active, HrmUserAccountModel actor, string ip)
    {
        var target = FindUser(userId);
        if (!AccountPolicy.CanManageAccounts(actor?.RoleCode) || !AccountPolicy.CanManage(actor, target)) throw new UnauthorizedAccessException();
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        db.Execute("UPDATE dbo.HrmUserAccount SET IsActive=@active,UpdatedAt=SYSDATETIME() WHERE Id=@userId", new { userId, active }, tx);
        AddAudit(db, actor.Id, active ? "UNLOCK" : "LOCK", "HrmUserAccount", userId.ToString(), $"{(active ? "Mở khóa" : "Khóa")} tài khoản {target.Username}", ip, tx);
        tx.Commit();
    }

    public void SetAccountRole(int userId, string roleCode, HrmUserAccountModel actor, string ip)
    {
        var target = FindUser(userId);
        if (!AccountPolicy.CanManageAccounts(actor?.RoleCode) || !AccountPolicy.CanManage(actor, target) ||
            !AccountPolicy.AssignableRoles(actor.RoleCode).Contains(roleCode)) throw new UnauthorizedAccessException();
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        db.Execute("UPDATE dbo.HrmUserAccount SET RoleCode=@roleCode,UpdatedAt=SYSDATETIME() WHERE Id=@userId", new { userId, roleCode }, tx);
        AddAudit(db, actor.Id, "CHANGE_ROLE", "HrmUserAccount", userId.ToString(), $"Đổi vai trò {target.Username}: {target.RoleCode} -> {roleCode}", ip, tx);
        tx.Commit();
    }

    public void ChangePassword(int userId, string currentPassword, string newPassword, string ip)
    {
        var user = FindUser(FindUser(userId)?.Username);
        if (user?.IsActive != true || !HrmPasswordHasher.Verify(currentPassword, user.PasswordSalt, user.PasswordHash))
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
        var error = AccountPolicy.ValidatePassword(newPassword, user.Username);
        if (error != null) throw new InvalidOperationException(error);
        if (newPassword == currentPassword) throw new InvalidOperationException("Mật khẩu mới phải khác mật khẩu hiện tại.");
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        StorePassword(db, tx, userId, newPassword, mustChange: false);
        AddAudit(db, userId, "CHANGE_PASSWORD", "HrmUserAccount", userId.ToString(), "Đổi mật khẩu", ip, tx);
        tx.Commit();
    }

    private static void StorePassword(Microsoft.Data.SqlClient.SqlConnection db, Microsoft.Data.SqlClient.SqlTransaction tx, int userId, string password, bool mustChange)
    {
        var (hash, salt) = HrmPasswordHasher.Hash(password);
        // Whole seconds, because the sign-in ticket that is compared with it has the same precision.
        var now = DateTime.UtcNow;
        db.Execute(@"UPDATE dbo.HrmUserAccount SET PasswordHash=@hash,PasswordSalt=@salt,MustChangePassword=@mustChange,
                PasswordChangedAt=@ChangedAt,UpdatedAt=SYSDATETIME() WHERE Id=@userId",
            new { userId, hash, salt, mustChange, ChangedAt = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc) }, tx);
    }
}
