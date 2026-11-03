using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static partial class DocxCvParser
{
    private const int MaxFileBytes = 10 * 1024 * 1024;
    private const long MaxExpandedBytes = 24L * 1024 * 1024;
    private const int MaxEntries = 2_000;
    private static readonly XNamespace WordNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public static RecruitmentCvImportPreviewModel Parse(byte[] content, string fileName)
    {
        if (content == null || content.Length == 0) throw new InvalidDataException("Tệp CV trống.");
        if (content.Length > MaxFileBytes) throw new InvalidDataException("Tệp CV vượt quá giới hạn 10 MB.");
        if (!string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Chức năng tự điền hiện chỉ hỗ trợ tệp Word .docx.");
        if (content.Length < 4 || content[0] != 0x50 || content[1] != 0x4B)
            throw new InvalidDataException("Tệp Word không hợp lệ.");

        using var stream = new MemoryStream(content, writable: false);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        if (archive.Entries.Count > MaxEntries) throw new InvalidDataException("Tệp Word chứa quá nhiều thành phần.");
        if (archive.Entries.Sum(entry => entry.Length) > MaxExpandedBytes)
            throw new InvalidDataException("Tệp Word có kích thước giải nén vượt giới hạn.");

        var documentEntry = archive.GetEntry("word/document.xml")
            ?? throw new InvalidDataException("Không tìm thấy nội dung chính trong tệp Word.");
        if (documentEntry.Length > 8 * 1024 * 1024)
            throw new InvalidDataException("Nội dung văn bản trong CV vượt giới hạn.");

        XDocument document;
        using (var xmlStream = documentEntry.Open())
        {
            document = XDocument.Load(xmlStream, LoadOptions.None);
        }

        var lines = document.Descendants(WordNamespace + "p")
            .Select(paragraph => NormalizeWhitespace(string.Concat(paragraph.Descendants(WordNamespace + "t").Select(node => node.Value))))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(2_000)
            .ToList();
        if (lines.Count == 0) throw new InvalidDataException("Không đọc được nội dung văn bản trong CV.");

        var text = string.Join("\n", lines);
        return new RecruitmentCvImportPreviewModel
        {
            FullName = FindName(lines),
            Email = EmailRegex().Match(text) is { Success: true } email ? email.Value.Trim() : null,
            Phone = FindPhone(text),
            DateOfBirth = FindDate(text, "ngày sinh", "date of birth", "dob"),
            Address = FindLabelValue(lines, 500, "địa chỉ", "address"),
            CurrentPosition = FindLabelValue(lines, 200, "vị trí ứng tuyển", "vị trí mong muốn", "chức danh", "position"),
            Education = FindSection(lines, 2_000, "học vấn", "trình độ học vấn", "education"),
            ExperienceSummary = FindSection(lines, 4_000, "kinh nghiệm", "kinh nghiệm làm việc", "work experience", "experience"),
            Skills = FindSection(lines, 2_000, "kỹ năng", "skills"),
            ExpectedSalary = FindLabelValue(lines, 100, "mức lương mong muốn", "lương mong muốn", "expected salary"),
            CvFileName = Path.GetFileName(fileName),
            ExtractedTextPreview = text.Length <= 3_000 ? text : text[..3_000] + "…"
        };
    }

    private static string FindName(IReadOnlyList<string> lines)
    {
        var labelled = FindLabelValue(lines, 200, "họ và tên", "họ tên", "full name", "name");
        if (!string.IsNullOrWhiteSpace(labelled)) return ToTitleCase(labelled);
        foreach (var line in lines.Take(12))
        {
            var candidate = NormalizeWhitespace(line);
            if (candidate.Length is < 4 or > 80 || candidate.Contains('@') || candidate.Any(char.IsDigit)) continue;
            var normalized = Fold(candidate);
            if (normalized is "curriculum vitae" or "resume" or "so yeu ly lich" or "thong tin ca nhan") continue;
            if (candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length is >= 2 and <= 7)
                return ToTitleCase(candidate);
        }
        return null;
    }

    private static string FindPhone(string text)
    {
        foreach (Match match in PhoneRegex().Matches(text))
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            if (digits.Length is >= 9 and <= 12) return NormalizeWhitespace(match.Value);
        }
        return null;
    }

    private static DateTime? FindDate(string text, params string[] labels)
    {
        foreach (var label in labels)
        {
            var match = Regex.Match(text, $@"(?im){Regex.Escape(label)}\s*[:\-]?\s*(?<date>\d{{1,2}}[\/\.\-]\d{{1,2}}[\/\.\-]\d{{4}})");
            if (!match.Success) continue;
            var value = match.Groups["date"].Value.Replace('.', '/').Replace('-', '/');
            if (DateTime.TryParseExact(value, ["d/M/yyyy", "dd/MM/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                && date >= new DateTime(1940, 1, 1) && date <= DateTime.Today)
                return date;
        }
        return null;
    }

    private static string FindLabelValue(IReadOnlyList<string> lines, int maxLength, params string[] labels)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var folded = Fold(line);
            foreach (var label in labels)
            {
                var foldedLabel = Fold(label);
                if (!folded.StartsWith(foldedLabel, StringComparison.OrdinalIgnoreCase)) continue;
                var separator = line.IndexOfAny([':', '-']);
                var value = separator >= 0 ? line[(separator + 1)..].Trim() : null;
                if (string.IsNullOrWhiteSpace(value) && index + 1 < lines.Count) value = lines[index + 1].Trim();
                return Truncate(value, maxLength);
            }
        }
        return null;
    }

    private static string FindSection(IReadOnlyList<string> lines, int maxLength, params string[] headings)
    {
        var start = -1;
        for (var index = 0; index < lines.Count; index++)
        {
            var folded = Fold(lines[index]).Trim(':', '-', ' ');
            if (headings.Any(heading => folded.Equals(Fold(heading), StringComparison.OrdinalIgnoreCase)))
            {
                start = index + 1;
                break;
            }
        }
        if (start < 0) return null;
        var selected = new List<string>();
        for (var index = start; index < lines.Count && selected.Count < 12; index++)
        {
            var line = lines[index];
            if (LooksLikeSectionHeading(line)) break;
            selected.Add(line);
            if (string.Join("\n", selected).Length >= maxLength) break;
        }
        return Truncate(string.Join("\n", selected), maxLength);
    }

    private static bool LooksLikeSectionHeading(string line)
    {
        if (line.Length > 60 || line.Contains(':')) return false;
        var folded = Fold(line).Trim();
        var known = new[] { "thong tin ca nhan", "muc tieu nghe nghiep", "hoc van", "kinh nghiem", "ky nang", "chung chi", "hoat dong", "nguoi tham chieu", "education", "experience", "skills", "certificates", "projects" };
        return known.Contains(folded) || (line.Length >= 4 && line.All(character => !char.IsLetter(character) || char.IsUpper(character)));
    }

    private static string Fold(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character == 'đ' ? 'd' : character == 'Đ' ? 'D' : char.ToLowerInvariant(character));
        return NormalizeWhitespace(builder.ToString().Normalize(NormalizationForm.FormC));
    }

    private static string ToTitleCase(string value) => CultureInfo.GetCultureInfo("vi-VN").TextInfo.ToTitleCase(NormalizeWhitespace(value).ToLowerInvariant());
    private static string NormalizeWhitespace(string value) => string.IsNullOrWhiteSpace(value) ? null : WhitespaceRegex().Replace(value, " ").Trim();
    private static string Truncate(string value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maxLength)];

    [GeneratedRegex(@"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
    [GeneratedRegex(@"(?<!\d)(?:\+?84|0)(?:[\s.\-]?\d){8,10}(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
