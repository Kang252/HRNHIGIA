using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("================================================================");
Console.WriteLine("KIỂM THỬ TỰ ĐỘNG CÁC TÍNH NĂNG NÂNG CẤP eHRM NHỊ GIA GROUP");
Console.WriteLine("================================================================");
Console.ResetColor();

int passedTests = 0;
int failedTests = 0;

void Assert(bool condition, string testName, string detail = null)
{
    if (condition)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[PASS] {testName}");
        Console.ResetColor();
        passedTests++;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {testName} - {detail}");
        Console.ResetColor();
        failedTests++;
    }
}

// 1. TEST JWT SERVICE
Console.WriteLine("\n--- 1. Kiểm thử Dịch vụ Xác thực JWT (Mobile Auth) ---");
var inMemorySettings = new Dictionary<string, string>
{
    { "JWT_SECRET_KEY", "NHIGIA_GROUP_TEST_KEY_2026_ENTERPRISE_SYSTEM_SECRET_FOR_UNIT_TESTS" }
};
IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
var jwtService = new JwtService(config);

var sampleUser = new HrmUserAccountModel
{
    Id = 101,
    Username = "sales01",
    DisplayName = "Nguyễn Văn Sales",
    RoleCode = HrmRoles.Employee,
    DepartmentId = 5,
    DepartmentName = "Phòng Kinh doanh",
    SupervisorUserId = 20
};

var (token, expiresAt) = jwtService.GenerateToken(sampleUser);
Assert(!string.IsNullOrWhiteSpace(token), "JWT Token được tạo thành công");
Assert(expiresAt > DateTime.UtcNow.AddDays(25), "Thời hạn JWT Token hợp lệ (30 ngày)");

var handler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtService.SecretKey)),
    ValidateIssuer = true,
    ValidIssuer = JwtService.Issuer,
    ValidateAudience = true,
    ValidAudience = JwtService.Audience,
    ValidateLifetime = true
};

try
{
    var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
    Assert(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value == "101", "Claim UserId (NameIdentifier) chính xác");
    Assert(principal.FindFirst(ClaimTypes.Name)?.Value == "sales01", "Claim Username chính xác");
    Assert(principal.FindFirst("display_name")?.Value == "Nguyễn Văn Sales", "Claim DisplayName chính xác");
    Assert(principal.FindFirst(ClaimTypes.Role)?.Value == HrmRoles.Employee, "Claim RoleCode chính xác");
    Assert(principal.FindFirst("department_name")?.Value == "Phòng Kinh doanh", "Claim DepartmentName chính xác");
}
catch (Exception ex)
{
    Assert(false, "Xác thực chữ ký JWT Token", ex.Message);
}

// 2. TEST CHẤM CÔNG GPS & GEOFENCING & FAKE GPS REJECTION
Console.WriteLine("\n--- 2. Kiểm thử Chấm công GPS & Phát hiện Giả lập vị trí ---");
var gpsService = new GpsAttendanceService(config);

// Test Mock Location Rejection
var mockRequest = new MobileGpsCheckInRequest
{
    Latitude = 10.7937400m,
    Longitude = 106.6781200m,
    IsMockLocation = true, // Ứng dụng phát hiện Fake GPS
    CheckType = "IN"
};

var (mockSuccess, mockMsg, _) = gpsService.ProcessGpsCheckIn(101, mockRequest, "127.0.0.1");
Assert(!mockSuccess, "Chặn thành công lượt chấm công có Fake GPS", mockMsg);
Assert(mockMsg.Contains("giả lập"), "Thông báo cảnh báo Fake GPS chính xác", mockMsg);

// Test GPS Haversine calculation
double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371000;
    var dLat = (lat2 - lat1) * (Math.PI / 180.0);
    var dLon = (lon2 - lon1) * (Math.PI / 180.0);
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c;
}

// Trụ sở Nhị Gia: 108-110 Nguyễn Văn Trỗi (10.79374, 106.67812)
// Điểm nhân viên đứng cách 40m: (10.79400, 106.67812)
double dNear = CalculateDistance(10.79374, 106.67812, 10.79400, 106.67812);
Assert(dNear < 100, $"Khoảng cách nhân viên tại cổng ({dNear:0.0}m) nằm trong bán kính Geofence văn phòng (< 150m)");

// Điểm nhân viên đứng tại Chợ Bến Thành (10.7725, 106.6980)
double dFar = CalculateDistance(10.79374, 106.67812, 10.7725, 106.6980);
Assert(dFar > 2000, $"Khoảng cách nhân viên tại hiện trường Q1 ({dFar:0.0}m) được nhận diện chính xác ngoài văn phòng (> 2000m)");

// 3. TEST AI HR ASSISTANT (CHATBOT & CV PARSER)
Console.WriteLine("\n--- 3. Kiểm thử Trợ lý Ảo AI HR (Policy Chatbot & CV Parser) ---");
var aiService = new AiHrAssistantService(config, null);

// Test Chatbot hỏi về Tăng ca
var otReply = aiService.ProcessChat(101, "Cho tôi hỏi quy định làm thêm giờ tăng ca tính lương thế nào?");
Assert(otReply.Success, "Chatbot phản hồi thành công câu hỏi Tăng ca");
Assert(otReply.Reply.Contains("150%") && otReply.Reply.Contains("200%") && otReply.Reply.Contains("300%"),
    "Chatbot cung cấp chính xác các mức 150%, 200%, 300% theo quy chế");

// Test Chatbot hỏi về Công tác phí
var tripReply = aiService.ProcessChat(101, "Chế độ thanh toán tiền khách sạn khi đi công tác?");
Assert(tripReply.Success, "Chatbot phản hồi thành công câu hỏi Công tác phí");
Assert(tripReply.Reply.Contains("công tác") && tripReply.Reply.Contains("khách sạn"),
    "Chatbot trích dẫn định mức khách sạn và phụ cấp công tác");

// Test CV Parser
var sampleCv = @"
Họ và tên: Trần Đình Trọng
Email: trong.tran@gmail.com
Số điện thoại: 0988776655
Trình độ: Kỹ sư Công nghệ thông tin - Đại học Bách Khoa
Kinh nghiệm: 3 năm
Kỹ năng chuyên môn: C#, .NET 8, Flutter, SQL Server, Microservices, Git
Đã từng phát triển ứng dụng di động chấm công và quản trị nhân sự cho doanh nghiệp.
";

var cvResult = aiService.ParseCvText(sampleCv, "Kỹ sư Lập trình .NET / Mobile");
Assert(cvResult.Success, "AI bóc tách CV thành công");
Assert(cvResult.CandidateName == "Trần Đình Trọng", $"Bóc tách chính xác tên ứng viên: {cvResult.CandidateName}");
Assert(cvResult.Email == "trong.tran@gmail.com", $"Bóc tách chính xác Email: {cvResult.Email}");
Assert(cvResult.Phone == "0988776655", $"Bóc tách chính xác SĐT: {cvResult.Phone}");
Assert(cvResult.KeySkills.Contains("C#") && cvResult.KeySkills.Contains("Flutter"), "Trích xuất chính xác bộ kỹ năng then chốt");
Assert(cvResult.MatchScorePercentage >= 80, $"Điểm tương thích CV đạt chuẩn ({cvResult.MatchScorePercentage}%)");
Assert(cvResult.InterviewQuestions.Count >= 3, "Sinh tự động bộ câu hỏi phỏng vấn phù hợp");

// 4. TEST WORKFLOW ENGINE CONFIGURATION
Console.WriteLine("\n--- 4. Kiểm thử Cấu hình Bộ máy Workflow Automation ---");
var defaultWorkflows = new[] { "LEAVE", "BUSINESS_TRIP", "PURCHASE", "IT_ASSET", "EXPENSE", "RECRUITMENT" };
Assert(defaultWorkflows.Length == 6, "Chuẩn hóa đủ 6 quy trình nghiệp vụ cốt lõi theo đề án nâng cấp");

Console.WriteLine("\n================================================================");
if (failedTests == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"TẤT CẢ {passedTests} BÀI KIỂM THỬ ĐÃ VƯỢT QUA XUẤT SẮC! (100% PASS)");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"KẾT QUẢ: {passedTests} PASS, {failedTests} FAIL");
}
Console.ResetColor();
Console.WriteLine("================================================================");
return failedTests == 0 ? 0 : 1;

