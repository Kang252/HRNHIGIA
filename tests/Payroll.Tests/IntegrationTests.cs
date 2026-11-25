using Dapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

/// <summary>
/// SQL-backed checks against a disposable LocalDB database. Never reads application connection settings.
/// </summary>
internal static class IntegrationTests
{
    public static void Run()
    {
        var instance = Environment.GetEnvironmentVariable("HRM_TEST_LOCALDB") ?? "NHIGIA";
        var database = "NHIGIA_PayrollTests_" + Guid.NewGuid().ToString("N");
        var masterString = $"Server=(localdb)\\{instance};Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=30";
        using var master = new SqlConnection(masterString);
        master.Open();
        master.Execute($"CREATE DATABASE [{database}]");
        var keys = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), database));
        try
        {
            var connectionString = new SqlConnectionStringBuilder(masterString) { InitialCatalog = database }.ConnectionString;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HRM_CONNECTION_STRING"] = connectionString
            }).Build();
            var hrm = new HrmDataStore(configuration, DataProtectionProvider.Create(keys));
            foreach (var schema in new[] { "hrm-mvp.sql", "remote-attendance.sql", "face-enrollment.sql" })
                hrm.EnsureSchema(Path.GetFullPath(Path.Combine("NHIGIA.Modern", "App_Data", schema)));
            using var db = new SqlConnection(connectionString);
            db.Open();

            int AddUser(string username, string role) => db.ExecuteScalar<int>(@"INSERT dbo.HrmUserAccount(Username,DisplayName,PasswordHash,PasswordSalt,RoleCode,DepartmentId)
                OUTPUT INSERTED.Id VALUES(@username,@username,'unused','unused',@role,(SELECT Id FROM dbo.HrmDepartment WHERE Code='IT'))", new { username, role });
            var employee = hrm.FindUser(AddUser("payroll_employee", HrmRoles.Employee));
            var accountant = hrm.FindUser(AddUser("payroll_accountant", HrmRoles.Accountant));
            var hr = hrm.FindUser(AddUser("payroll_hr", HrmRoles.Hr));
            db.Execute(@"INSERT dbo.HrmEmployeeProfile(UserId,EmployeeCode,JobTitle,HireDate,WorkLocation,CompanyEmail,BasicSalary)
                VALUES(@Id,'PT001',N'Chuyên viên kiểm thử','2025-03-04',N'Hà Nội','pt001@example.test','24000000')", new { employee.Id });
            db.Execute(@"INSERT dbo.HrmEmployeeSchedule(UserId,ShiftName,StartTime,EndTime,BreakMinutes,GraceMinutes,WorkDaysMask,EffectiveFrom,CreatedByUserId)
                VALUES(@Id,N'Ca hành chính','08:00','17:30',90,0,62,'2026-01-01',@Id)", new { employee.Id });

            // Last fully ended period: the one before the period containing today.
            var current = AttendancePeriodRange.Current();
            var ended = AttendancePeriodRange.ForDate(current.FromDate.AddDays(-1));
            for (var day = ended.FromDate; day <= ended.ToDate; day = day.AddDays(1))
            {
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                foreach (var time in new[] { day.AddHours(8), day.AddHours(17.5) })
                    db.Execute(@"INSERT dbo.HrmAttendanceEvent(EventKey,UserId,CheckTime,PayloadJson,ReceivedAt)
                        VALUES(@Key,@Id,@time,'{}',@time)", new { Key = $"payroll-{time:yyyyMMddHHmm}", employee.Id, time });
            }

            // --- attendance period guards
            Reject(() => hrm.ConfirmAttendancePeriod(current.Period, employee, "127.0.0.1"), "chưa kết thúc", "confirming the running period is refused");
            Reject(() => hrm.SetAttendancePeriodLock(current.Period, true, null, false, hr, "127.0.0.1"), "chưa kết thúc", "locking the running period is refused");
            Reject(() => hrm.SetAttendancePeriodLock(current.Period, true, "ngoại lệ", true, hr, "127.0.0.1"), "chưa kết thúc", "exception lock of the running period is refused");
            Reject(() => hrm.RemindAttendanceConfirmations(current.Period, hr, "127.0.0.1"), "chưa kết thúc", "reminding for the running period is refused");
            Check(db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmAttendancePeriod") == 0 &&
                  db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmAttendancePeriodConfirmation") == 0, "refused requests leave no rows");
            var running = hrm.GetAttendancePeriod(current.Period, employee);
            Check(!running.HasEnded && running.ClosableFrom == current.ToDate.AddDays(1), "running period exposes its opening date");
            db.Execute("INSERT dbo.HrmAttendancePeriodConfirmation(Period,UserId,SubmittedAt) VALUES(@Period,@Id,@At)",
                new { current.Period, employee.Id, At = current.FromDate });
            Check(!hrm.GetAttendancePeriod(current.Period, employee).IsConfirmed && hrm.GetAttendancePeriod(current.Period, hr).ConfirmedCount == 0,
                "a confirmation stored before the period ended does not count");

            Check(hrm.GetAttendancePeriod(ended.Period, employee).HasEnded, "previous period can be closed");
            Reject(() => hrm.SetAttendancePeriodLock(ended.Period, true, null, false, hr, "127.0.0.1"), "Chưa đủ xác nhận", "normal lock still needs every confirmation");
            foreach (var user in new[] { employee, accountant, hr }) hrm.ConfirmAttendancePeriod(ended.Period, user, "127.0.0.1");
            Check(hrm.GetAttendancePeriod(ended.Period, employee).IsConfirmed, "ended period can be confirmed");

            // --- payroll
            var payroll = new WorkItemStore(configuration, hrm);
            Reject(() => payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1"), "chưa được khóa", "payroll needs a locked period");
            hrm.SetAttendancePeriodLock(ended.Period, true, null, false, hr, "127.0.0.1");
            var created = payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1");
            Check(created.Created == 3 && created.Updated == 0, "payroll is created for the locked period");
            WorkItem Row() => db.QuerySingle<WorkItem>("SELECT * FROM dbo.HrmWorkItem WHERE Kind='payroll' AND Quarter=@Period AND EmployeeId=@Id", new { ended.Period, employee.Id });
            var row = Row();
            Check(row.JobLevel == "Chuyên viên kiểm thử" && row.StartDate == new DateTime(2025, 3, 4), "job title and hire date are stored");
            Check(row.Target == 24_000_000m && row.Actual == 24_000_000m, $"full attendance earns the full salary (target {row.Target}, actual {row.Actual})");

            db.Execute("UPDATE dbo.HrmEmployeeProfile SET JobTitle=N'Trưởng nhóm',BasicSalary='30000000' WHERE UserId=@Id", new { employee.Id });
            var updated = payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1");
            Check(updated.Created == 0 && updated.Updated == 3, "recalculation updates the existing drafts");
            row = Row();
            Check(row.JobLevel == "Trưởng nhóm" && row.StartDate == new DateTime(2025, 3, 4) && row.Actual == 30_000_000m, "recalculation refreshes title and salary");
            db.Execute("UPDATE dbo.HrmWorkItem SET Status='APPROVED' WHERE Id=@Id", new { row.Id });
            Check(payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1").SkippedLocked == 1, "approved payroll is kept");

            // --- leave preview and routing share one rule
            var monday = current.FromDate;
            while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(1);
            foreach (var (end, session, days, status, label) in new[]
            {
                (monday, "Buổi sáng", 0.5m, "PENDING_MANAGER", "Trưởng phòng"),
                (monday.AddDays(2), "Cả ngày", 3m, "PENDING_MANAGER", "Trưởng phòng"),
                (monday.AddDays(3), "Cả ngày", 4m, "PENDING_HR", "HR/Giám đốc"),
                (monday.AddDays(6), "Buổi chiều", 2.5m, "PENDING_MANAGER", "Trưởng phòng")
            })
            {
                var request = new CreateLeaveRequest { LeaveType = "Nghỉ phép", StartDate = monday, EndDate = end, SessionCode = session, Reason = "Kiểm thử" };
                var preview = hrm.PreviewLeave(request, employee);
                var saved = hrm.CreateLeave(request, employee, "127.0.0.1");
                Check(preview.RequestedDays == days && preview.StatusCode == status && preview.ApproverLabel == label && saved.StatusCode == status,
                    $"preview matches the saved request: {days} days, {label}");
                Check(hrm.GetLeaveRequests(employee).Single(x => x.Id == saved.Id).RequestedDays == days, $"list shows {days} days");
            }
            Reject(() => hrm.PreviewLeave(new CreateLeaveRequest { EmployeeId = hr.Id }, employee), "phạm vi", "preview cannot be used for someone else");
            Check(hrm.PreviewLeave(new CreateLeaveRequest(), employee).StatusCode == "PENDING_MANAGER", "non-leave request of an employee goes to the manager");
            Check(hrm.PreviewLeave(new CreateLeaveRequest(), accountant).StatusCode == "PENDING_HR", "non-leave request of other roles goes to HR/director");
        }
        finally
        {
            SqlConnection.ClearAllPools();
            master.Execute($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]");
            keys.Delete(true);
        }
        Console.WriteLine("Payroll, attendance and leave integration checks passed");
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("FAIL " + label);
        Console.WriteLine("PASS " + label);
    }

    private static void Reject(Action action, string expected, string label)
    {
        try { action(); }
        catch (InvalidOperationException exception)
        {
            Check(exception.Message.Contains(expected, StringComparison.OrdinalIgnoreCase), $"{label} ({exception.Message})");
            return;
        }
        throw new InvalidOperationException("FAIL expected rejection: " + label);
    }
}
