using NHIGIA.Web.Infrastructure;
using NHIGIA.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    [AllowAnonymous]
    public class HanetWebhookController : Controller
    {
        [HttpPost]
        public ActionResult Receive()
        {
            try
            {
                var settings = HrmDataStore.Instance.GetHanetSettings(true);
                var suppliedSecret = Request.Headers["X-HANET-SECRET"] ?? Request.QueryString["secret"];
                if (!settings.IsEnabled || string.IsNullOrWhiteSpace(settings.WebhookSecret) || !FixedEquals(settings.WebhookSecret, suppliedSecret)) return new HttpStatusCodeResult(401);

                string body;
                using (var reader = new StreamReader(Request.InputStream)) body = reader.ReadToEnd();
                JObject payload;
                if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith("{")) payload = JObject.Parse(body);
                else payload = JObject.FromObject(Request.Form.AllKeys.Where(x => x != null).ToDictionary(x => x, x => Request.Form[x]));

                var personId = Value(payload, "personID", "personId", "person_id");
                var aliasId = Value(payload, "aliasID", "aliasId", "alias_id");
                var rawTime = Value(payload, "date", "checkTime", "time", "timestamp", "check_time");
                DateTime checkTime;
                if (!TryParseTime(rawTime, out checkTime)) throw new InvalidOperationException("Thời gian sự kiện HANET không hợp lệ.");
                var eventKey = Value(payload, "id", "eventId", "eventID") ?? Sha256((body ?? payload.ToString(Formatting.None)) + "|" + personId + "|" + aliasId + "|" + checkTime.ToString("O"));

                var inserted = HrmDataStore.Instance.SaveAttendanceEvent(new HanetWebhookEvent
                {
                    EventKey = eventKey,
                    PersonId = personId,
                    AliasId = aliasId,
                    PlaceId = Value(payload, "placeID", "placeId", "place_id"),
                    DeviceId = Value(payload, "deviceID", "deviceId", "device_id"),
                    CheckTime = checkTime,
                    EventType = Value(payload, "type", "eventType", "event_type") ?? "checkin",
                    PayloadJson = payload.ToString(Formatting.None)
                });
                return Json(new { success = true, inserted });
            }
            catch (JsonException exception)
            {
                return Json(new { success = false, message = exception.Message });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message });
            }
        }

        private static string Value(JObject source, params string[] names)
        {
            foreach (var property in source.Properties()) if (names.Any(x => string.Equals(x, property.Name, StringComparison.OrdinalIgnoreCase))) return Convert.ToString(property.Value, CultureInfo.InvariantCulture);
            return null;
        }

        private static bool TryParseTime(string value, out DateTime result)
        {
            long epoch;
            if (long.TryParse(value, out epoch))
            {
                if (epoch > 9999999999) epoch /= 1000;
                result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToLocalTime();
                return true;
            }
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result) || DateTime.TryParse(value, new CultureInfo("vi-VN"), DateTimeStyles.AssumeLocal, out result);
        }

        private static string Sha256(string input)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty))).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool FixedEquals(string expected, string actual)
        {
            if (expected == null || actual == null) return false;
            var a = Encoding.UTF8.GetBytes(expected);
            var b = Encoding.UTF8.GetBytes(actual);
            if (a.Length != b.Length) return false;
            var difference = 0;
            for (var i = 0; i < a.Length; i++) difference |= a[i] ^ b[i];
            return difference == 0;
        }
    }
}
