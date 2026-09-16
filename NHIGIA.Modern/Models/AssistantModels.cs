namespace NHIGIA.Modern.Models;

public sealed class AssistantQuestionRequest
{
    public string Question { get; set; }
    public IReadOnlyList<AssistantChatMessage> History { get; set; } = Array.Empty<AssistantChatMessage>();
}

public sealed class AssistantChatMessage
{
    public string Role { get; set; }
    public string Text { get; set; }
}

public sealed class AssistantAnswer
{
    public string Answer { get; set; }
    public string Scope { get; set; }
    public string LinkUrl { get; set; }
    public string LinkLabel { get; set; }
    public bool UsedGemini { get; set; }
    public IReadOnlyList<string> Suggestions { get; set; } = Array.Empty<string>();
}

public sealed class AssistantWorkSummary
{
    public string Kind { get; set; }
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
}
