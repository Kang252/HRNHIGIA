using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[AllowAnonymous]
public sealed class HanetWebhookController : Controller
{
    private readonly HrmDataStore _store;
    private readonly ILogger<HanetWebhookController> _logger;

    public HanetWebhookController(HrmDataStore store, ILogger<HanetWebhookController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        try
        {
            var settings = _store.GetHanetSettings(true);
            var suppliedSecret = Request.Headers["X-HANET-SECRET"].FirstOrDefault() ?? Request.Query["secret"].FirstOrDefault();
            if (!settings.IsEnabled) return Unauthorized();

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            JsonObject payload;
            if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith('{')) payload = JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
            else
            {
                var form = await Request.ReadFormAsync();
                payload = new JsonObject(form.ToDictionary(x => x.Key, x => (JsonNode)x.Value.ToString()));
            }

            var dataType = Value(payload, "data_type", "dataType");
            if (!string.IsNullOrWhiteSpace(dataType) && !string.Equals(dataType, "log", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = true, ignored = true, reason = "unsupported_data_type" });

            var eventId = Value(payload, "id", "eventId", "eventID");
            var signature = Value(payload, "hash");
            var validOfficialSignature = !string.IsNullOrWhiteSpace(eventId)
                && !string.IsNullOrWhiteSpace(settings.ClientSecret)
                && !string.IsNullOrWhiteSpace(signature)
                && FixedEquals(Md5(settings.ClientSecret + eventId), signature);
            var validLegacySecret = !string.IsNullOrWhiteSpace(settings.WebhookSecret)
                && !string.IsNullOrWhiteSpace(suppliedSecret)
                && FixedEquals(settings.WebhookSecret, suppliedSecret);
            if (!validOfficialSignature && !validLegacySecret) return Unauthorized();

            var personId = Value(payload, "personID", "personId", "person_id");
            var aliasId = Value(payload, "aliasID", "aliasId", "alias_id");
            if (!TryParseTime(Value(payload, "date", "checkTime", "time", "timestamp", "check_time"), out var checkTime)) throw new InvalidOperationException("Thời gian sự kiện HANET không hợp lệ.");
            var placeId = Value(payload, "placeID", "placeId", "place_id");
            if (!string.IsNullOrWhiteSpace(settings.PlaceId) && !string.Equals(settings.PlaceId.Trim(), placeId?.Trim(), StringComparison.OrdinalIgnoreCase))
                return Json(new { success = true, ignored = true, reason = "different_place" });
            var eventKey = eventId ?? Sha256((body ?? payload.ToJsonString()) + "|" + personId + "|" + aliasId + "|" + checkTime.ToString("O"));
            var inserted = _store.SaveAttendanceEvent(new HanetWebhookEvent
            {
                EventKey = eventKey,
                PersonId = personId,
                AliasId = aliasId,
                PlaceId = placeId,
                DeviceId = Value(payload, "deviceID", "deviceId", "device_id"),
                CheckTime = checkTime,
                EventType = Value(payload, "type", "eventType", "event_type", "action_type") ?? "checkin",
                PayloadJson = payload.ToJsonString()
            });
            return Json(new { success = true, inserted });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Invalid HANET webhook payload");
            return BadRequest(new { success = false, message = exception.Message });
        }
    }

    private static string Value(JsonObject source, params string[] names)
    {
        foreach (var property in source)
            if (names.Any(x => string.Equals(x, property.Key, StringComparison.OrdinalIgnoreCase))) return property.Value?.ToString();
        return null;
    }

    private static bool TryParseTime(string value, out DateTime result)
    {
        if (long.TryParse(value, out var epoch))
        {
            if (epoch > 9999999999) epoch /= 1000;
            var vietnam = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");
            result = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(epoch), vietnam).DateTime;
            return true;
        }
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result) || DateTime.TryParse(value, new CultureInfo("vi-VN"), DateTimeStyles.AssumeLocal, out result);
    }

    private static string Sha256(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input ?? string.Empty))).ToLowerInvariant();

    private static string Md5(string input) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(input ?? string.Empty))).ToLowerInvariant();

    private static bool FixedEquals(string expected, string actual)
    {
        if (expected == null || actual == null) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));
    }
}
