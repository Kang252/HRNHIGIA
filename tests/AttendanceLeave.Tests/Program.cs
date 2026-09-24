using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

// Pure projection regressions. These tests never connect to HANET or a database.
var day = new DateTime(2026, 9, 24);
var tests = new List<(string Name, Action Run)>
{
    ("Approved full-day leave creates attendance without device events", () =>
    {
        var row = Single(Project([], [Schedule()], [Leave()]));
        Equal("ON_LEAVE", row.StatusCode);
        Equal("Nghỉ đã duyệt", row.Source);
        True(row.ApprovedLeave, "approved leave marker");
        True(!string.IsNullOrWhiteSpace(row.LeaveDescription), "leave label");
        Equal(0, row.EventCount);
        Equal<DateTime?>(null, row.CheckIn);
        Equal<DateTime?>(null, row.CheckOut);
        Equal(0, row.LateMinutes);
        Equal(0, row.EarlyMinutes);
        Equal(0, row.WorkedMinutes);
    }),
    ("Full-day leave suppresses attendance exceptions but preserves actual punches", () =>
    {
        var actual = Punch(1, day, 9, 16);
        actual.EventCount = 8;
        var row = Single(Project([actual], [Schedule()], [Leave()]));
        Equal("ON_LEAVE", row.StatusCode);
        Equal(0, row.LateMinutes);
        Equal(0, row.EarlyMinutes);
        Equal(day.AddHours(9), row.CheckIn);
        Equal(day.AddHours(16), row.CheckOut);
        Equal(day.AddHours(16), row.LastSeen);
        Equal(8, row.EventCount);
        Equal("HANET", row.Source);
    }),
    ("Morning leave excuses morning and lunch only", () =>
    {
        var row = Single(Project([Punch(1, day, 13.5, 17.5)], [Schedule()], [Leave(session: "Buổi sáng")]));
        True(row.ApprovedLeave, "approved leave marker");
        Equal(day.AddHours(13.5), row.ExpectedStartAt);
        Equal(day.AddHours(17.5), row.ExpectedEndAt);
        Equal("ON_TIME", row.StatusCode);
        Equal(0, row.LateMinutes);
        Equal(0, row.EarlyMinutes);
    }),
    ("Morning leave still detects late return", () =>
    {
        var row = Single(Project([Punch(1, day, 13.75, 17.5)], [Schedule()], [Leave(session: "MORNING")]));
        Equal("LATE", row.StatusCode);
        Equal(15, row.LateMinutes);
    }),
    ("Afternoon leave ends attendance expectation at noon", () =>
    {
        var row = Single(Project([Punch(1, day, 8, 12)], [Schedule()], [Leave(session: "Buổi chiều")]));
        Equal(day.AddHours(8), row.ExpectedStartAt);
        Equal(day.AddHours(12), row.ExpectedEndAt);
        Equal("ON_TIME", row.StatusCode);
        Equal(0, row.EarlyMinutes);
    }),
    ("Afternoon leave still detects early departure before noon", () =>
    {
        var row = Single(Project([Punch(1, day, 8, 11.5)], [Schedule()], [Leave(session: "AFTERNOON")]));
        Equal("EARLY", row.StatusCode);
        Equal(30, row.EarlyMinutes);
    }),
    ("Half-day leave without remaining shift punches is missing after the shift", () =>
    {
        var row = Single(Project([], [Schedule()], [Leave(session: "AM")]));
        Equal("MISSING_CHECK", row.StatusCode);
        True(row.ApprovedLeave, "approved leave marker");
        True(!row.IsProvisional, "remaining shift has ended");
    }),
    ("Half-day leave is provisional until the remaining shift ends", () =>
    {
        var row = Single(Project([], [Schedule()], [Leave(session: "PM")], now: day.AddHours(11)));
        Equal("IN_PROGRESS", row.StatusCode);
        True(row.IsProvisional, "remaining shift is still open");
        Equal(0, row.EarlyMinutes);
    }),
    ("Afternoon leave becomes final at noon instead of original shift end", () =>
    {
        var row = Single(Project([Punch(1, day, 8, 12)], [Schedule()], [Leave(session: "PM")], now: day.AddHours(12)));
        Equal("ON_TIME", row.StatusCode);
        True(!row.IsProvisional, "remaining shift is closed at noon");
    }),
    ("Single remaining punch does not become a valid checkout", () =>
    {
        var actual = Punch(1, day, 13, null);
        var row = Single(Project([actual], [Schedule()], [Leave(session: "AM")]));
        Equal("MISSING_CHECK", row.StatusCode);
        Equal(1, row.EventCount);
        Equal<DateTime?>(null, row.CheckOut);
        Equal(day.AddHours(13), row.LastSeen);
    }),
    ("Morning leave fully covers an assigned Saturday morning shift", () =>
    {
        var saturday = new DateTime(2026, 9, 26);
        var shift = Schedule(start: 8, end: 12, mask: 64, breakMinutes: 0);
        var row = Single(Project([], [shift], [Leave(start: saturday, session: "AM")], saturday, saturday));
        Equal("ON_LEAVE", row.StatusCode);
        Equal(0, row.LateMinutes);
        Equal(0, row.EarlyMinutes);
    }),
    ("Leave spans months but only produces assigned workdays", () =>
    {
        var from = new DateTime(2026, 9, 28);
        var to = new DateTime(2026, 10, 5);
        var rows = Project([], [Schedule()], [Leave(start: from, end: to)], from, to);
        Equal(6, rows.Count);
        True(rows.All(x => x.WorkDate.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday), "no unassigned weekend rows");
        True(rows.Any(x => x.WorkDate == new DateTime(2026, 10, 1)), "October rows included");
        True(rows.All(x => x.StatusCode == "ON_LEAVE"), "every working date is excused");
    }),
    ("Projection clips leave to requested reporting dates", () =>
    {
        var rows = Project([], [Schedule()], [Leave(start: day.AddDays(-7), end: day.AddDays(7))], day, day);
        Equal(1, rows.Count);
        Equal(day, rows[0].WorkDate);
    }),
    ("Employee leave does not change another employee attendance", () =>
    {
        var rows = Project([Punch(2, day, 9, 16)], [Schedule(1), Schedule(2)], [Leave(1)]);
        Equal(2, rows.Count);
        Equal("ON_LEAVE", rows.Single(x => x.UserId == 1).StatusCode);
        var second = rows.Single(x => x.UserId == 2);
        True(!second.ApprovedLeave, "second employee is not on leave");
        Equal("LATE_EARLY", second.StatusCode);
        Equal(60, second.LateMinutes);
        Equal(90, second.EarlyMinutes);
    }),
    ("Pending, rejected and cancelled requests do not excuse attendance", () =>
    {
        foreach (var status in new[] { "PENDING_MANAGER", "PENDING_HR", "REJECTED", "CANCELLED" })
        {
            var rows = Project([], [Schedule()], [Leave(status: status)]);
            Equal(0, rows.Count, status);
            var row = Single(Project([Punch(1, day, 9, 16)], [Schedule()], [Leave(status: status)]));
            Equal("LATE_EARLY", row.StatusCode, status);
            True(!row.ApprovedLeave, status);
        }
    }),
    ("Approved non-leave requests do not excuse attendance", () =>
    {
        foreach (var type in new[] { "Công tác", "Đi công tác", "Đi muộn", "Về sớm", "Làm việc từ xa", "Đổi ca" })
        {
            var rows = Project([], [Schedule()], [Leave(type: type)]);
            Equal(0, rows.Count, type);
        }
    }),
    ("Supported actual leave types excuse an assigned workday", () =>
    {
        foreach (var type in new[] { "Phép năm", "Nghỉ phép", "Nghỉ ốm" })
        {
            var row = Single(Project([], [Schedule()], [Leave(type: type)]));
            Equal("ON_LEAVE", row.StatusCode, type);
        }
    }),
    ("Overlapping morning and afternoon approvals produce one fully excused day", () =>
    {
        var rows = Project([], [Schedule()], [Leave(session: "AM"), Leave(session: "PM")]);
        var row = Single(rows);
        Equal("ON_LEAVE", row.StatusCode);
    }),
    ("Duplicate and full-day overlapping approvals do not duplicate attendance", () =>
    {
        var rows = Project([Punch(1, day, 9, 16)], [Schedule()],
            [Leave(session: "MORNING"), Leave(session: "FULL_DAY"), Leave(session: "FULL_DAY")]);
        var row = Single(rows);
        Equal("ON_LEAVE", row.StatusCode);
        Equal(2, row.EventCount);
        Equal(day.AddHours(9), row.CheckIn);
        Equal(day.AddHours(16), row.LastSeen);
    }),
    ("Revoking approval removes synthetic attendance on the next projection", () =>
    {
        Equal(1, Project([], [Schedule()], [Leave()]).Count);
        Equal(0, Project([], [Schedule()], []).Count);
        Equal(0, Project([], [Schedule()], [Leave(status: "REJECTED")]).Count);
    }),
    ("No assigned schedule means no synthetic leave attendance", () =>
    {
        Equal(0, Project([], [], [Leave()]).Count);
        var inactive = Schedule();
        inactive.StatusCode = "INACTIVE";
        Equal(0, Project([], [inactive], [Leave()]).Count);
        var future = Schedule();
        future.EffectiveFrom = day.AddDays(1);
        Equal(0, Project([], [future], [Leave()]).Count);
        var expired = Schedule();
        expired.EffectiveTo = day.AddDays(-1);
        Equal(0, Project([], [expired], [Leave()]).Count);
    }),
    ("Existing attendance without leave retains the normal calculation", () =>
    {
        var row = Single(Project([Punch(1, day, 8, 17.5)], [Schedule()], []));
        Equal("ON_TIME", row.StatusCode);
        True(!row.ApprovedLeave, "normal attendance");
        Equal(2, row.EventCount);
        Equal(day.AddHours(8), row.CheckIn);
        Equal(day.AddHours(17.5), row.CheckOut);
        Equal(day.AddHours(17.5), row.LastSeen);
        Equal("HANET", row.Source);
    }),
    ("Remainder shift honours its configured late grace", () =>
    {
        var shift = Schedule();
        shift.GraceMinutes = 5;
        var actual = Punch(1, day, 13.75, 17.5);
        actual.GraceMinutes = 5;
        var row = Single(Project([actual], [shift], [Leave(session: "AM")]));
        Equal("LATE", row.StatusCode);
        Equal(10, row.LateMinutes);
    }),
    ("Morning leave returns at 13:30 even with a legacy break duration", () =>
    {
        var actual = Punch(1, day, 13.5, 17.5);
        actual.BreakMinutes = 30;
        var row = Single(Project([actual], [Schedule(breakMinutes: 30)], [Leave(session: "AM")]));
        Equal(day.AddHours(13.5), row.ExpectedStartAt);
        Equal("ON_TIME", row.StatusCode);
    })
};

var failures = 0;
foreach (var (name, run) in tests)
{
    try
    {
        run();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception exception)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {exception.Message}");
    }
}
Console.WriteLine($"Attendance leave regressions: {tests.Count - failures}/{tests.Count} passed.");
return failures == 0 ? 0 : 1;

IList<AttendanceRecordModel> Project(IReadOnlyList<AttendanceRecordModel> attendance,
    IReadOnlyList<ScheduleModel> schedules, IReadOnlyList<LeaveRequestModel> leaves,
    DateTime? from = null, DateTime? to = null, DateTime? now = null) =>
    AttendanceLeavePolicy.Project(attendance, schedules, leaves, from ?? day, to ?? day,
        now ?? new DateTime(2026, 10, 10, 20, 0, 0));

ScheduleModel Schedule(int userId = 1, double start = 8, double end = 17.5, int mask = 62, int breakMinutes = 60) => new()
{
    Id = userId,
    UserId = userId,
    DisplayName = $"Employee {userId}",
    Username = $"employee{userId}",
    DepartmentName = "IT",
    ShiftName = "Ca hành chính",
    StartTime = TimeSpan.FromHours(start),
    EndTime = TimeSpan.FromHours(end),
    BreakMinutes = breakMinutes,
    GraceMinutes = 0,
    WorkDaysMask = mask,
    EffectiveFrom = new DateTime(2026, 1, 1),
    StatusCode = "ACTIVE"
};

LeaveRequestModel Leave(int userId = 1, DateTime? start = null, DateTime? end = null,
    string session = "Cả ngày", string status = "APPROVED", string type = "Phép năm") => new()
{
    Id = 100 + userId,
    RequestCode = $"NP{userId:D8}",
    UserId = userId,
    DisplayName = $"Employee {userId}",
    EmployeeCode = $"NV{userId:D3}",
    DepartmentName = "IT",
    StartDate = start ?? day,
    EndDate = end ?? start ?? day,
    SessionCode = session,
    StatusCode = status,
    LeaveType = type,
    Reason = "Approved absence"
};

AttendanceRecordModel Punch(int userId, DateTime date, double checkIn, double? checkOut) => new()
{
    UserId = userId,
    DisplayName = $"Employee {userId}",
    WorkDate = date,
    PersonId = $"hanet-{userId}",
    ScheduledStart = TimeSpan.FromHours(8),
    ScheduledEnd = TimeSpan.FromHours(17.5),
    BreakMinutes = 60,
    GraceMinutes = 0,
    CheckIn = date.AddHours(checkIn),
    CheckOut = checkOut.HasValue ? date.AddHours(checkOut.Value) : null,
    LastSeen = date.AddHours(checkOut ?? checkIn),
    EventCount = checkOut.HasValue ? 2 : 1,
    Source = "HANET"
};

static AttendanceRecordModel Single(IList<AttendanceRecordModel> rows)
{
    Equal(1, rows.Count, "row count");
    return rows[0];
}

static void Equal<T>(T expected, T actual, string? context = null)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{context ?? "value"}: expected '{expected}', got '{actual}'.");
}

static void True(bool condition, string context)
{
    if (!condition) throw new InvalidOperationException(context);
}
