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
            foreach (var schema in new[] { "hrm-mvp.sql", "remote-attendance.sql", "face-enrollment.sql", "opencv-face-trial.sql", "profile-recruitment-enhancements.sql" })
                hrm.EnsureSchema(Path.GetFullPath(Path.Combine("NHIGIA.Modern", "App_Data", schema)));
            using var db = new SqlConnection(connectionString);
            db.Open();

            int AddUser(string username, string role, string department = "IT") => db.ExecuteScalar<int>(@"INSERT dbo.HrmUserAccount(Username,DisplayName,PasswordHash,PasswordSalt,RoleCode,DepartmentId)
                OUTPUT INSERTED.Id VALUES(@username,@username,'unused','unused',@role,(SELECT Id FROM dbo.HrmDepartment WHERE Code=@department))", new { username, role, department });
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

            // --- people whose pay depends on leave kind, waivers, overtime rates and leaving mid-period
            var manager = hrm.FindUser(AddUser("payroll_manager", HrmRoles.Manager));
            bool Weekday(DateTime day) => day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
            var periodDays = Enumerable.Range(0, (ended.ToDate - ended.FromDate).Days + 1).Select(x => ended.FromDate.AddDays(x)).ToList();
            var standardDays = periodDays.Count(Weekday);
            var plain = periodDays.Where(x => Weekday(x) && !OvertimePolicy.IsFixedHoliday(x)).ToList();
            var (unpaidDay, lateDay, overtimeDay, lastDay) = (plain[2], plain[3], plain[4], plain[9]);
            var saturday = periodDays.First(x => x.DayOfWeek == DayOfWeek.Saturday && !OvertimePolicy.IsFixedHoliday(x));
            HrmUserAccountModel Worker(string username, string code, Func<DateTime, (double In, double Out)?> punches)
            {
                var user = hrm.FindUser(AddUser(username, HrmRoles.Employee));
                db.Execute(@"INSERT dbo.HrmEmployeeProfile(UserId,EmployeeCode,HireDate,BasicSalary) VALUES(@Id,@code,'2025-03-04','24000000')", new { user.Id, code });
                db.Execute(@"INSERT dbo.HrmEmployeeSchedule(UserId,ShiftName,StartTime,EndTime,BreakMinutes,GraceMinutes,WorkDaysMask,EffectiveFrom,CreatedByUserId)
                    VALUES(@Id,N'Ca hành chính','08:00','17:30',90,0,62,'2026-01-01',@Id)", new { user.Id });
                foreach (var day in periodDays)
                    if (punches(day) is { } punch)
                        foreach (var time in new[] { day.AddHours(punch.In), day.AddHours(punch.Out) })
                            db.Execute(@"INSERT dbo.HrmAttendanceEvent(EventKey,UserId,CheckTime,PayloadJson,ReceivedAt)
                                VALUES(@Key,@Id,@time,'{}',@time)", new { Key = $"{username}-{time:yyyyMMddHHmm}", user.Id, time });
                return user;
            }
            var unpaid = Worker("payroll_unpaid", "PT002", d => Weekday(d) && d != unpaidDay ? (8, 17.5) : null);
            var waived = Worker("payroll_waived", "PT003", d => Weekday(d) ? (d == lateDay ? 8.5 : 8, 17.5) : null);
            var late = Worker("payroll_late", "PT004", d => Weekday(d) ? (d == lateDay ? 8.5 : 8, 17.5) : null);
            var overtime = Worker("payroll_overtime", "PT005", d => d == saturday ? (8, 11) : Weekday(d) ? (8, d == overtimeDay ? 21 : 17.5) : null);
            var leaver = Worker("payroll_leaver", "PT006", d => Weekday(d) && d <= lastDay ? (8, 17.5) : null);

            CreateLeaveRequest Ask(string type, DateTime day, string session = "Cả ngày") => new() { LeaveType = type, StartDate = day, EndDate = day, SessionCode = session, Reason = "Kiểm thử" };
            var unpaidAsk = Ask("Nghỉ phép", unpaidDay);
            unpaidAsk.LeaveSubtype = "Nghỉ không hưởng lương";
            var unpaidLeave = hrm.CreateLeave(unpaidAsk, unpaid, "127.0.0.1");
            Check(unpaidLeave.LeaveType == "Nghỉ không lương", "unpaid choice is stored as unpaid leave");
            Check(hrm.ApproveLeave(new ApprovalRequest { Id = unpaidLeave.Id, Approve = true }, manager, "127.0.0.1"), "manager approves the unpaid day");
            Reject(() => hrm.CreateLeave(Ask("Đi muộn về sớm", lateDay, "Đi muộn 30 phút"), waived, "127.0.0.1"), "đi muộn hoặc về sớm", "late request needs its minutes");
            var lateAsk = Ask("Đi muộn về sớm", lateDay, "Đi muộn 30 phút");
            (lateAsk.AdjustKind, lateAsk.AdjustMinutes) = ("LATE", 30);
            Check(hrm.ApproveLeave(new ApprovalRequest { Id = hrm.CreateLeave(lateAsk, waived, "127.0.0.1").Id, Approve = true }, manager, "127.0.0.1"), "manager approves the late arrival");
            var waiting = hrm.CreateLeave(Ask("Nghỉ ốm", plain[6]), late, "127.0.0.1");
            foreach (var (day, from, to, category) in new[] { (saturday, 8d, 11d, OvertimePolicy.WeekendCategory), (overtimeDay, 18d, 21d, OvertimePolicy.EveningCategory) })
                db.Execute(@"INSERT dbo.HrmWorkItem(Kind,Title,Category,EmployeeId,DueDate,StartAt,EndAt,Status,CreatedBy)
                    VALUES('overtime',N'Tăng ca',@category,@Id,@day,@StartAt,@EndAt,'APPROVED',@Id)",
                    new { category, overtime.Id, day, StartAt = day.AddHours(from), EndAt = day.AddHours(to) });

            // --- completing a resignation keeps the days already worked
            var seed = new WorkItemStore(configuration, hrm);
            int Resignation(DateTime due) => db.ExecuteScalar<int>(@"INSERT dbo.HrmWorkItem(Kind,Title,EmployeeId,DueDate,Status,CreatedBy)
                OUTPUT INSERTED.Id VALUES('resignation',N'Nghỉ việc',@Id,@due,'APPROVED',@Id)", new { leaver.Id, due });
            var tooEarly = Resignation(HrmDataStore.CurrentVietnamTime().Date.AddDays(10));
            Reject(() => seed.UpdateResignationStatus(tooEarly, "RESOLVED", null, hr.Id, "127.0.0.1"), "Ngày làm việc cuối cùng", "resignation cannot be completed before the last day");
            Check(seed.HasActiveResignation(leaver.Id), "a second resignation can be refused");
            db.Execute("DELETE dbo.HrmWorkItem WHERE Id=@tooEarly", new { tooEarly });
            Check(seed.UpdateResignationStatus(Resignation(lastDay), "RESOLVED", null, hr.Id, "127.0.0.1"), "resignation is completed after the last day");
            var leaverSchedule = db.QuerySingle<ScheduleModel>("SELECT * FROM dbo.HrmEmployeeSchedule WHERE UserId=@Id", new { leaver.Id });
            Check(leaverSchedule.StatusCode == "ACTIVE" && leaverSchedule.EffectiveTo == lastDay && !hrm.FindUser(leaver.Id).IsActive, "schedule ends on the last day and the account is locked");

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
            foreach (var user in new[] { employee, accountant, hr, manager, unpaid, waived, late, overtime }) hrm.ConfirmAttendancePeriod(ended.Period, user, "127.0.0.1");
            Check(hrm.GetAttendancePeriod(ended.Period, employee).IsConfirmed, "ended period can be confirmed");

            // --- payroll
            var payroll = new WorkItemStore(configuration, hrm);
            Reject(() => payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1"), "chưa được khóa", "payroll needs a locked period");
            hrm.SetAttendancePeriodLock(ended.Period, true, null, false, hr, "127.0.0.1");
            var created = payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1");
            Check(created.Created == 9 && created.Updated == 0, "payroll is created for the locked period, including a leaver who worked in it");
            decimal Net(HrmUserAccountModel user) => db.ExecuteScalar<decimal>("SELECT Actual FROM dbo.HrmWorkItem WHERE Kind='payroll' AND Quarter=@Period AND EmployeeId=@Id", new { ended.Period, user.Id });
            void Pays(HrmUserAccountModel user, decimal expected, string label) => Check(Math.Abs(Net(user) - expected) < 5, $"{label}: {Net(user):0} (expected {expected:0})");
            const decimal salary = 24_000_000m;
            var perDay = salary / standardDays;
            Pays(unpaid, salary - perDay, "one unpaid day is deducted");
            Pays(waived, salary, "an approved late arrival costs nothing");
            Pays(late, salary - perDay * 30 / 480, "the same late arrival without approval is deducted");
            Pays(overtime, salary + perDay / 480 * (180 * 2m + 180 * 1.5m), "weekend overtime pays 200% and an evening 150%");
            Pays(leaver, perDay * periodDays.Count(x => Weekday(x) && x <= lastDay), "a leaver is paid up to the last working day");
            Reject(() => hrm.CreateLeave(Ask("Nghỉ ốm", plain[7]), late, "127.0.0.1"), "đã khóa", "leave in a locked period is refused");
            Reject(() => hrm.ApproveLeave(new ApprovalRequest { Id = waiting.Id, Approve = true }, manager, "127.0.0.1"), "đã khóa", "approving leave in a locked period is refused");
            Reject(() => hrm.DeleteLeave(unpaidLeave.Id, manager, "127.0.0.1"), "đã khóa", "deleting approved leave of a locked period is refused");
            Check(hrm.ApproveLeave(new ApprovalRequest { Id = waiting.Id, Approve = false, Note = "Đã khóa công" }, manager, "127.0.0.1"), "rejecting it stays possible");
            WorkItem Row() => db.QuerySingle<WorkItem>("SELECT * FROM dbo.HrmWorkItem WHERE Kind='payroll' AND Quarter=@Period AND EmployeeId=@Id", new { ended.Period, employee.Id });
            var row = Row();
            Check(row.JobLevel == "Chuyên viên kiểm thử" && row.StartDate == new DateTime(2025, 3, 4), "job title and hire date are stored");
            Check(row.Target == 24_000_000m && row.Actual == 24_000_000m, $"full attendance earns the full salary (target {row.Target}, actual {row.Actual})");

            db.Execute("UPDATE dbo.HrmEmployeeProfile SET JobTitle=N'Trưởng nhóm',BasicSalary='30000000' WHERE UserId=@Id", new { employee.Id });
            var updated = payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1");
            Check(updated.Created == 0 && updated.Updated == 9, "recalculation updates the existing drafts");
            row = Row();
            Check(row.JobLevel == "Trưởng nhóm" && row.StartDate == new DateTime(2025, 3, 4) && row.Actual == 30_000_000m, "recalculation refreshes title and salary");
            db.Execute("UPDATE dbo.HrmWorkItem SET Status='APPROVED' WHERE Id=@Id", new { row.Id });
            Check(payroll.GeneratePayrollPeriod(ended.Period, accountant, "127.0.0.1").SkippedLocked == 1, "approved payroll is kept");
            Reject(() => hrm.SetAttendancePeriodLock(ended.Period, false, "Sửa công", false, hr, "127.0.0.1"), "Giám đốc", "attendance cannot be unlocked under an approved payroll");

            // --- personal payslip and its feedback
            WorkPage Payslips(HrmUserAccountModel user, string period = null)
            {
                var page = new WorkPage { Kind = "payroll", PayrollPeriod = period, PayrollDeptFilter = "ALL", PayrollStatusFilter = "ALL" };
                payroll.Load(page, user);
                return page;
            }
            Check(Payslips(employee).PayslipPeriods.Count == 0, "an unpublished payslip is not listed");
            db.Execute("UPDATE dbo.HrmWorkItem SET Status='PUBLISHED' WHERE Id=@Id", new { row.Id });
            var mine = Payslips(employee);
            Check(mine.PayrollPeriod == ended.Period && mine.PayslipPeriods.Single().Period == ended.Period && mine.PayrollItems.Single().EmployeeId == employee.Id, "the latest own payslip opens first");
            Check(Payslips(unpaid).PayrollItems.Count == 0 && Payslips(manager).PayrollItems.Count == 0, "nobody sees someone else's payslip");
            Check(!payroll.TransitionPayroll(row.Id, "PUBLISHED", "DISPUTED", "DISPUTE", "Sai", unpaid.Id, unpaid.Id, "127.0.0.1"), "feedback on someone else's payslip is refused");
            Check(payroll.TransitionPayroll(row.Id, "PUBLISHED", "DISPUTED", "DISPUTE", "Thiếu phụ cấp", employee.Id, employee.Id, "127.0.0.1"), "employee sends payslip feedback");
            Check(db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmNotification WHERE UserId=@Id AND Title LIKE N'Phản hồi sai lệch%'", new { accountant.Id }) == 1, "the accountant is told once");
            Check(payroll.TransitionPayroll(row.Id, "DISPUTED", "RESOLVED", "RESOLVE", "Bù kỳ sau", accountant.Id, null, "127.0.0.1") && Payslips(employee).PayrollItems.Single().LastActionNote == "Bù kỳ sau", "the answer reaches the employee");

            // --- salary advance becomes a payroll deduction
            var advanceAsk = Ask("Tạm ứng lương", current.FromDate, "Chuyển khoản");
            Reject(() => hrm.CreateLeave(advanceAsk, late, "127.0.0.1"), "số tiền", "an advance needs an amount");
            advanceAsk.Amount = 2_000_000m;
            var advance = hrm.CreateLeave(advanceAsk, late, "127.0.0.1");
            Check(hrm.ApproveLeave(new ApprovalRequest { Id = advance.Id, Approve = true }, manager, "127.0.0.1"), "manager approves the advance");
            Check(db.ExecuteScalar<decimal>("SELECT SUM(Target) FROM dbo.HrmWorkItem WHERE Kind='payroll-advance' AND EmployeeId=@Id AND Quarter=@Period AND Status='APPROVED'", new { late.Id, current.Period }) == 2_000_000m, "the advance is booked into the period of its date");
            Reject(() => hrm.DeleteLeave(advance.Id, manager, "127.0.0.1"), "khoản trừ lương", "a booked advance cannot be deleted");

            // --- bulk approval keeps going past a refused request
            var bulk = new[] { plain[0], plain[1] }.Select(x => x.AddDays(70)).Select(x => hrm.CreateLeave(Ask("Suất ăn", x, "Cơm trưa"), late, "127.0.0.1").Id).ToList();
            Check(hrm.BulkApproveLeave(new BulkApprovalRequest { Ids = bulk, Approve = true }, manager, "127.0.0.1") == 2, "bulk approval handles every request");

            // --- schedules
            SaveScheduleRequest Shift(string start, string end, DateTime from) => new() { UserId = late.Id, ShiftName = "Ca thử", StartTime = start, EndTime = end, BreakMinutes = 90, WorkDaysMask = 62, EffectiveFrom = from };
            Reject(() => hrm.SaveSchedule(Shift("08:00", "17:30", current.FromDate), hr, "127.0.0.1"), "đã có lịch", "the same shift cannot be assigned twice");
            Reject(() => hrm.SaveSchedule(Shift("09:00", "18:30", new DateTime(2026, 1, 1)), hr, "127.0.0.1"), "đã có lịch", "two shifts cannot start on the same day");
            Check(hrm.SaveSchedule(Shift("09:00", "18:30", current.FromDate), hr, "127.0.0.1") > 0, "a later change of shift is allowed");

            // --- IT helpdesk
            var requester = hrm.FindUser(AddUser("payroll_requester", HrmRoles.Employee, "CSKH"));
            var ticket = payroll.Create(new WorkItem { Kind = "helpdesk", Title = "Máy in", Description = "Kẹt giấy", Priority = "NORMAL", EmployeeId = requester.Id, CreatedBy = requester.Id, Status = "OPEN" });
            string Ticket() => db.ExecuteScalar<string>("SELECT Status FROM dbo.HrmWorkItem WHERE Id=@ticket", new { ticket });
            Check(db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmNotification WHERE UserId=@Id AND LinkUrl='/Work?kind=helpdesk'", new { manager.Id }) == 1, "IT is told about a new ticket");
            Check(!payroll.HelpdeskAction(ticket, "accept", null, requester, "127.0.0.1"), "the requester cannot accept a ticket");
            Check(payroll.HelpdeskAction(ticket, "accept", null, manager, "127.0.0.1") && Ticket() == "IN_PROGRESS", "IT accepts the ticket");
            Reject(() => payroll.HelpdeskAction(ticket, "resolve", " ", manager, "127.0.0.1"), "phản hồi", "resolving needs an answer");
            Check(payroll.HelpdeskAction(ticket, "resolve", "Đã thay trục", manager, "127.0.0.1") && Ticket() == "RESOLVED", "IT resolves the ticket");
            Check(payroll.HelpdeskAction(ticket, "reopen", "Vẫn kẹt", requester, "127.0.0.1") && Ticket() == "OPEN", "the requester reopens it");
            Check(!payroll.HelpdeskAction(ticket, "cancel", null, hr, "127.0.0.1"), "people outside IT cannot act on it");

            // --- accounts
            CreateAccountRequest NewAccount(string username, string role, string code) => new() { DisplayName = "Nhân viên mới", Username = username, EmployeeCode = code, RoleCode = role, EmploymentStatus = EmployeeProfileOptions.EmploymentStatuses[0] };
            var issued = hrm.CreateAccount(NewAccount("Nhan.Vien", HrmRoles.Employee, "PT100"), hr, "127.0.0.1");
            var account = hrm.FindUser("nhan.vien");
            Check(account.MustChangePassword && HrmPasswordHasher.Verify(issued, account.PasswordSalt, account.PasswordHash), "a new account starts with a one-time password");
            Reject(() => hrm.CreateAccount(NewAccount("nhan.vien", HrmRoles.Employee, "PT101"), hr, "127.0.0.1"), "đã tồn tại", "a username is used once");
            Reject(() => hrm.CreateAccount(NewAccount("nhan.vien2", HrmRoles.Employee, "PT100"), hr, "127.0.0.1"), "đã được sử dụng", "an employee code is used once");
            Reject(() => hrm.CreateAccount(NewAccount("giam.doc", HrmRoles.Director, "PT102"), hr, "127.0.0.1"), "vai trò", "HR cannot hand out director rights");
            Reject(() => hrm.ChangePassword(account.Id, "sai", "Mat-khau-moi-1", "127.0.0.1"), "không đúng", "changing a password needs the current one");
            hrm.ChangePassword(account.Id, issued, "Mat-khau-moi-1", "127.0.0.1");
            account = hrm.FindUser("nhan.vien");
            Check(!account.MustChangePassword && HrmPasswordHasher.Verify("Mat-khau-moi-1", account.PasswordSalt, account.PasswordHash), "the new password works");
            Check(hrm.ResetPassword(account.Id, hr, "127.0.0.1").Username == "nhan.vien" && hrm.FindUser("nhan.vien").MustChangePassword, "HR issues a new one-time password");
            var refused = false;
            try { hrm.ResetPassword(hr.Id, manager, "127.0.0.1"); } catch (UnauthorizedAccessException) { refused = true; }
            Check(refused, "a manager cannot reset passwords");

            // --- leave preview and routing share one rule
            var monday = current.FromDate;
            while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(1);
            foreach (var (end, session, days, status, label) in new[]
            {
                (0, "Buổi sáng", 0.5m, "PENDING_MANAGER", "Trưởng phòng"),
                (2, "Cả ngày", 3m, "PENDING_MANAGER", "Trưởng phòng"),
                (3, "Cả ngày", 4m, "PENDING_HR", "HR/Giám đốc"),
                (6, "Buổi chiều", 2.5m, "PENDING_MANAGER", "Trưởng phòng")
            })
            {
                // Each request has its own week: overlapping ones are refused.
                var request = new CreateLeaveRequest { LeaveType = "Nghỉ phép", StartDate = monday, EndDate = monday.AddDays(end), SessionCode = session, Reason = "Kiểm thử" };
                monday = monday.AddDays(7);
                var preview = hrm.PreviewLeave(request, employee);
                var saved = hrm.CreateLeave(request, employee, "127.0.0.1");
                Check(preview.RequestedDays == days && preview.StatusCode == status && preview.ApproverLabel == label && saved.StatusCode == status,
                    $"preview matches the saved request: {days} days, {label}");
                Check(hrm.GetLeaveRequests(employee).Single(x => x.Id == saved.Id).RequestedDays == days, $"list shows {days} days");
            }
            Reject(() => hrm.CreateLeave(Ask("Nghỉ phép", monday.AddDays(-7)), employee, "127.0.0.1"), "trùng thời gian", "an overlapping request is refused");
            Check(hrm.CreateLeave(Ask("Nghỉ phép", monday.AddDays(-7), "Buổi sáng"), employee, "127.0.0.1").Id > 0, "the free half of a day can still be requested");
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
