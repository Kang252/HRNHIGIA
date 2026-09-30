namespace NHIGIA.Modern.Models;

public sealed class FaceMatchResult
{
    public string Status { get; set; }
    public double? Score { get; set; }
    public double? Threshold { get; set; }
    public string ModelVersion { get; set; }
    public string DetailCode { get; set; }

    public static bool IsTerminal(string status) => status is
        "MATCH" or "NO_MATCH" or "NO_FACE" or "MULTIPLE_FACES" or "LOW_QUALITY" or "INVALID_IMAGE";
}

public sealed class RemotePunchReceipt
{
    public long Id { get; set; }
    public string StatusCode { get; set; }
    public string ReviewReason { get; set; }
    public FaceMatchResult FaceMatch { get; set; }
}
