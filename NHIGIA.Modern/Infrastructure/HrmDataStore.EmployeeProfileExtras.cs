using Dapper;
using Microsoft.AspNetCore.DataProtection;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    public EmployeeProfileExtrasModel GetEmployeeProfileExtras(int userId, bool canManage, bool canRevealPasswords)
    {
        using var connection = OpenConnection();
        var result = new EmployeeProfileExtrasModel
        {
            UserId = userId,
            CanManage = canManage,
            CanRevealPasswords = canRevealPasswords
        };
        result.Assets = connection.Query<EmployeeAssignedAssetModel>(@"
            SELECT Id,Reference,Title,Category,Status,SalaryRange Condition,DueDate IssuedAt,LastActionNote Notes
            FROM dbo.HrmWorkItem
            WHERE Kind='assets' AND EmployeeId=@UserId AND Status IN ('ALLOCATED','ALLOCATING','RECOVERING')
            ORDER BY COALESCE(DueDate,CreatedAt) DESC,Id DESC", new { UserId = userId }).ToList();
        result.Uniforms = connection.Query<EmployeeUniformModel>(@"
            SELECT Id,UserId,ItemName,Size,Quantity,IssuedDate,ReturnedDate,StatusCode,Notes
            FROM dbo.HrmEmployeeUniform WHERE UserId=@UserId
            ORDER BY IssuedDate DESC,Id DESC", new { UserId = userId }).ToList();
        result.SoftwareAccounts = connection.Query<EmployeeSoftwareAccountModel>(@"
            SELECT Id,UserId,SoftwareName,LoginUrl,LoginMethod,Username,PermissionScope,IsActive,
                   CONVERT(bit,CASE WHEN ProtectedPassword IS NULL OR LEN(ProtectedPassword)=0 THEN 0 ELSE 1 END) HasPassword,
                   UpdatedAt
            FROM dbo.HrmEmployeeSoftwareAccount WHERE UserId=@UserId
            ORDER BY IsActive DESC,SoftwareName,Id", new { UserId = userId }).ToList();
        return result;
    }

    public int SaveEmployeeUniform(EmployeeUniformModel input, HrmUserAccountModel actor, string ipAddress)
    {
        if (input == null || actor == null) throw new InvalidOperationException("Dữ liệu trang phục không hợp lệ.");
        input.ItemName = CleanProfileText(input.ItemName, 150);
        input.Size = CleanProfileText(input.Size, 50);
        input.StatusCode = NormalizeUniformStatus(input.StatusCode);
        input.Notes = CleanProfileText(input.Notes, 500);
        if (string.IsNullOrWhiteSpace(input.ItemName)) throw new InvalidOperationException("Vui lòng nhập tên trang phục.");
        if (input.UserId <= 0 || input.Quantity is < 1 or > 1000) throw new InvalidOperationException("Nhân viên hoặc số lượng trang phục không hợp lệ.");
        if (input.IssuedDate == default) input.IssuedDate = CurrentVietnamTime().Date;
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var userExists = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmUserAccount WHERE Id=@Id", new { Id = input.UserId }, transaction) > 0;
        if (!userExists) throw new InvalidOperationException("Nhân viên không tồn tại.");
        int id;
        if (input.Id > 0)
        {
            id = connection.ExecuteScalar<int?>(@"
                UPDATE dbo.HrmEmployeeUniform SET ItemName=@ItemName,Size=@Size,Quantity=@Quantity,
                    IssuedDate=@IssuedDate,ReturnedDate=@ReturnedDate,StatusCode=@StatusCode,Notes=@Notes,
                    UpdatedAt=SYSDATETIME(),UpdatedByUserId=@ActorId
                OUTPUT INSERTED.Id WHERE Id=@Id AND UserId=@UserId", new
            {
                input.Id, input.UserId, input.ItemName, input.Size, input.Quantity, input.IssuedDate,
                input.ReturnedDate, input.StatusCode, input.Notes, ActorId = actor.Id
            }, transaction) ?? throw new InvalidOperationException("Bản ghi trang phục không tồn tại.");
        }
        else
        {
            id = connection.ExecuteScalar<int>(@"
                INSERT dbo.HrmEmployeeUniform(UserId,ItemName,Size,Quantity,IssuedDate,ReturnedDate,StatusCode,Notes,CreatedByUserId)
                OUTPUT INSERTED.Id VALUES(@UserId,@ItemName,@Size,@Quantity,@IssuedDate,@ReturnedDate,@StatusCode,@Notes,@ActorId)", new
            {
                input.UserId, input.ItemName, input.Size, input.Quantity, input.IssuedDate,
                input.ReturnedDate, input.StatusCode, input.Notes, ActorId = actor.Id
            }, transaction);
        }
        AddAudit(connection, actor.Id, input.Id > 0 ? "UPDATE" : "CREATE", "HrmEmployeeUniform", id.ToString(),
            $"{input.ItemName}, số lượng {input.Quantity}, nhân viên #{input.UserId}", ipAddress, transaction);
        transaction.Commit();
        return id;
    }

    public bool DeleteEmployeeUniform(int id, int userId, HrmUserAccountModel actor, string ipAddress)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var changed = connection.Execute(@"UPDATE dbo.HrmEmployeeUniform
            SET StatusCode='RETURNED',ReturnedDate=COALESCE(ReturnedDate,@ReturnedDate),
                UpdatedAt=SYSDATETIME(),UpdatedByUserId=@ActorId
            WHERE Id=@Id AND UserId=@UserId AND StatusCode<>'RETURNED'",
            new { Id = id, UserId = userId, ActorId = actor.Id, ReturnedDate = CurrentVietnamTime().Date }, transaction) > 0;
        if (changed) AddAudit(connection, actor.Id, "RETURN", "HrmEmployeeUniform", id.ToString(), $"Thu hồi trang phục của nhân viên #{userId}", ipAddress, transaction);
        transaction.Commit();
        return changed;
    }

    public int SaveEmployeeSoftwareAccount(EmployeeSoftwareAccountModel input, HrmUserAccountModel actor, string ipAddress)
    {
        if (input == null || actor == null) throw new InvalidOperationException("Dữ liệu tài khoản phần mềm không hợp lệ.");
        input.SoftwareName = CleanProfileText(input.SoftwareName, 150);
        input.LoginUrl = CleanProfileText(input.LoginUrl, 500);
        input.LoginMethod = CleanProfileText(input.LoginMethod, 100);
        input.Username = CleanProfileText(input.Username, 200);
        input.PermissionScope = CleanProfileText(input.PermissionScope, 500);
        if (string.IsNullOrWhiteSpace(input.SoftwareName)) throw new InvalidOperationException("Vui lòng nhập tên phần mềm.");
        if (!string.IsNullOrWhiteSpace(input.LoginUrl)
            && (!Uri.TryCreate(input.LoginUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http")))
            throw new InvalidOperationException("Link đăng nhập phải là địa chỉ HTTP/HTTPS hợp lệ.");
        if (input.UserId <= 0) throw new InvalidOperationException("Nhân viên không hợp lệ.");
        var protectedPassword = string.IsNullOrWhiteSpace(input.Password) ? null : _softwareAccountProtector.Protect(input.Password);
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var userExists = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmUserAccount WHERE Id=@Id", new { Id = input.UserId }, transaction) > 0;
        if (!userExists) throw new InvalidOperationException("Nhân viên không tồn tại.");
        int id;
        if (input.Id > 0)
        {
            id = connection.ExecuteScalar<int?>(@"
                UPDATE dbo.HrmEmployeeSoftwareAccount SET SoftwareName=@SoftwareName,LoginUrl=@LoginUrl,
                    LoginMethod=@LoginMethod,Username=@Username,
                    ProtectedPassword=COALESCE(@ProtectedPassword,ProtectedPassword),PermissionScope=@PermissionScope,
                    IsActive=@IsActive,UpdatedAt=SYSDATETIME(),UpdatedByUserId=@ActorId
                OUTPUT INSERTED.Id WHERE Id=@Id AND UserId=@UserId", new
            {
                input.Id, input.UserId, input.SoftwareName, input.LoginUrl, input.LoginMethod, input.Username,
                ProtectedPassword = protectedPassword, input.PermissionScope, input.IsActive, ActorId = actor.Id
            }, transaction) ?? throw new InvalidOperationException("Tài khoản phần mềm không tồn tại.");
        }
        else
        {
            id = connection.ExecuteScalar<int>(@"
                INSERT dbo.HrmEmployeeSoftwareAccount(UserId,SoftwareName,LoginUrl,LoginMethod,Username,ProtectedPassword,
                    PermissionScope,IsActive,CreatedByUserId)
                OUTPUT INSERTED.Id VALUES(@UserId,@SoftwareName,@LoginUrl,@LoginMethod,@Username,@ProtectedPassword,
                    @PermissionScope,@IsActive,@ActorId)", new
            {
                input.UserId, input.SoftwareName, input.LoginUrl, input.LoginMethod, input.Username,
                ProtectedPassword = protectedPassword, input.PermissionScope, input.IsActive, ActorId = actor.Id
            }, transaction);
        }
        AddAudit(connection, actor.Id, input.Id > 0 ? "UPDATE" : "CREATE", "HrmEmployeeSoftwareAccount", id.ToString(),
            $"{input.SoftwareName}, nhân viên #{input.UserId}; mật khẩu {(protectedPassword == null ? "không đổi" : "đã cập nhật")}", ipAddress, transaction);
        transaction.Commit();
        return id;
    }

    public string RevealEmployeeSoftwarePassword(int id, int userId, HrmUserAccountModel actor, string ipAddress)
    {
        if (actor == null || (actor.Id != userId && actor.RoleCode != HrmRoles.Admin))
            throw new UnauthorizedAccessException("Bạn không có quyền xem mật khẩu tài khoản phần mềm này.");

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var protectedPassword = connection.QuerySingleOrDefault<string>(@"
            SELECT ProtectedPassword FROM dbo.HrmEmployeeSoftwareAccount WHERE Id=@Id AND UserId=@UserId AND IsActive=1",
            new { Id = id, UserId = userId }, transaction);
        if (string.IsNullOrWhiteSpace(protectedPassword)) return null;
        string password;
        try { password = _softwareAccountProtector.Unprotect(protectedPassword); }
        catch { throw new InvalidOperationException("Không thể giải mã mật khẩu này. Vui lòng đặt lại mật khẩu trong hồ sơ."); }
        AddAudit(connection, actor.Id, "REVEAL_SECRET", "HrmEmployeeSoftwareAccount", id.ToString(),
            $"Xem mật khẩu tài khoản phần mềm của nhân viên #{userId}", ipAddress, transaction);
        transaction.Commit();
        return password;
    }

    public bool DeleteEmployeeSoftwareAccount(int id, int userId, HrmUserAccountModel actor, string ipAddress)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var changed = connection.Execute(@"UPDATE dbo.HrmEmployeeSoftwareAccount
            SET IsActive=0,UpdatedAt=SYSDATETIME(),UpdatedByUserId=@ActorId
            WHERE Id=@Id AND UserId=@UserId AND IsActive=1",
            new { Id = id, UserId = userId, ActorId = actor.Id }, transaction) > 0;
        if (changed) AddAudit(connection, actor.Id, "REVOKE", "HrmEmployeeSoftwareAccount", id.ToString(), $"Thu hồi tài khoản phần mềm của nhân viên #{userId}", ipAddress, transaction);
        transaction.Commit();
        return changed;
    }

    private static string NormalizeUniformStatus(string status) => (status ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "RETURNED" => "RETURNED",
        "LOST" => "LOST",
        _ => "ISSUED"
    };

    private static string CleanProfileText(string value, int maxLength)
    {
        value = value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, maxLength)];
    }
}
