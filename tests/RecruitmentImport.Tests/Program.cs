using System.IO.Compression;
using System.Security;
using System.Text;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

var passed = 0;
void Check(string name, bool condition)
{
    if (!condition) throw new Exception("FAIL " + name);
    Console.WriteLine("PASS " + name);
    passed++;
}

static byte[] CreateDocx(params string[] paragraphs)
{
    using var stream = new MemoryStream();
    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
    {
        var entry = archive.CreateEntry("word/document.xml", CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        var body = string.Join(string.Empty, paragraphs.Select(line =>
            $"<w:p><w:r><w:t>{SecurityElement.Escape(line)}</w:t></w:r></w:p>"));
        writer.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>{body}</w:body></w:document>");
    }
    return stream.ToArray();
}

var cv = CreateDocx(
    "NGUYỄN VĂN AN",
    "Email: an.nguyen@example.com",
    "Số điện thoại: 0901 234 567",
    "Ngày sinh: 15/06/1996",
    "Địa chỉ: Quận 1, TP. Hồ Chí Minh",
    "Vị trí ứng tuyển: Nhân viên kinh doanh",
    "HỌC VẤN", "Đại học Kinh tế TP.HCM",
    "KINH NGHIỆM", "03 năm bán hàng B2B",
    "KỸ NĂNG", "Đàm phán, CRM");
var parsed = DocxCvParser.Parse(cv, "nguyen-van-an.docx");
Check("Extracts candidate name", parsed.FullName == "Nguyễn Văn An");
Check("Extracts email", parsed.Email == "an.nguyen@example.com");
Check("Extracts phone", parsed.Phone?.Contains("0901") == true);
Check("Extracts date of birth", parsed.DateOfBirth == new DateTime(1996, 6, 15));
Check("Extracts current position", parsed.CurrentPosition == "Nhân viên kinh doanh");
Check("Extracts education and experience", parsed.Education?.Contains("Đại học") == true && parsed.ExperienceSummary?.Contains("B2B") == true);

var wrongExtension = false;
try { DocxCvParser.Parse(cv, "cv.doc"); } catch (InvalidDataException) { wrongExtension = true; }
Check("Rejects legacy or renamed Word file", wrongExtension);
var malformed = false;
try { DocxCvParser.Parse(Encoding.UTF8.GetBytes("not a zip"), "cv.docx"); } catch (InvalidDataException) { malformed = true; }
Check("Rejects malformed DOCX", malformed);
Check("Employment statuses are exact", EmployeeProfileOptions.EmploymentStatuses.SequenceEqual(["Nhân viên chính thức", "Thử việc", "Thực tập sinh", "Đã thôi việc"]));

Console.WriteLine($"{passed} recruitment/profile checks passed.");
