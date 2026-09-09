using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers
{
    [HrmAuthorize(HrmRoles.Director, HrmRoles.Admin, HrmRoles.Hr)]
    public sealed class AiReportController : BaseController
    {
        private readonly AiAnalyticsEngine _engine;
        private readonly ILogger<AiReportController> _logger;

        public AiReportController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            AiAnalyticsEngine engine,
            ILogger<AiReportController> logger)
            : base(store, userAccessor)
        {
            _engine = engine;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string period = "current_month")
        {
            ViewBag.Title = "AI Báo cáo Ban Giám Đốc";
            ViewBag.ActivePeriod = period;
            var dashboard = _engine.GenerateDashboard(period);
            return View(dashboard);
        }

        [HttpGet]
        public IActionResult GetData(string period = "current_month")
        {
            try
            {
                var data = _engine.GenerateDashboard(period);
                return Json(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nạp dữ liệu AI Analytics");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi phân tích dữ liệu AI: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Ask([FromBody] AiAskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
            {
                return Json(new AiAskResponse
                {
                    Success = false,
                    Answer = "Vui lòng nhập câu hỏi để Trợ lý AI có thể hỗ trợ phân tích số liệu."
                });
            }

            try
            {
                var context = _engine.GenerateDashboard(request.Period ?? "current_month");
                var response = _engine.AnswerExecutiveQuestion(request, context);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý câu hỏi AI");
                return Json(new AiAskResponse
                {
                    Success = false,
                    Answer = "Đã xảy ra sự cố khi xử lý câu hỏi: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Speak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest();

            try
            {
                // Chuẩn hóa văn bản tiếng Việt để đọc tự nhiên
                var clean = System.Text.RegularExpressions.Regex.Replace(text, @"[*#_`]", "")
                                 .Replace("VNĐ", " đồng ")
                                 .Replace("đ/người", " đồng một người ")
                                 .Replace("%", " phần trăm ")
                                 .Replace("/", " trên ")
                                 .Replace("OT", " ô ti ")
                                 .Replace("KPI", " ca bê i ")
                                 .Trim();

                var sentences = clean.Split(new[] { '.', ';', '\n', '!' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .Where(s => !string.IsNullOrWhiteSpace(s))
                                     .ToList();

                var chunks = new List<string>();
                var currentChunk = "";

                foreach (var s in sentences)
                {
                    if ((currentChunk + " " + s).Length > 150)
                    {
                        if (!string.IsNullOrWhiteSpace(currentChunk)) chunks.Add(currentChunk.Trim());
                        currentChunk = s;
                    }
                    else
                    {
                        currentChunk = string.IsNullOrWhiteSpace(currentChunk) ? s : currentChunk + ". " + s;
                    }
                }
                if (!string.IsNullOrWhiteSpace(currentChunk)) chunks.Add(currentChunk.Trim());
                if (chunks.Count == 0) chunks.Add(clean);

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Referer", "https://translate.google.com/");

                using var ms = new MemoryStream();
                foreach (var chunk in chunks.Take(6))
                {
                    var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl=vi&client=tw-ob&q={Uri.EscapeDataString(chunk)}";
                    var audioBytes = await client.GetByteArrayAsync(url);
                    if (audioBytes != null && audioBytes.Length > 0)
                    {
                        await ms.WriteAsync(audioBytes, 0, audioBytes.Length);
                    }
                }

                var combined = ms.ToArray();
                if (combined.Length > 0)
                {
                    return File(combined, "audio/mpeg");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sinh giọng đọc tiếng Việt");
            }

            return NotFound();
        }
    }
}

