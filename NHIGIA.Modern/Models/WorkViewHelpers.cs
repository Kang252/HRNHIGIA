using System;
using System.Collections.Generic;

namespace NHIGIA.Modern.Models;

public static class WorkViewHelpers
{
    public static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "NV";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "NV";
        var firstWord = parts[0];
        return firstWord.Length >= 2 ? firstWord.Substring(0, 2).ToUpperInvariant() : firstWord.ToUpperInvariant();
    }

    public static (string Gradient, List<(string Icon, string Text, string Color)> Pills) GetCourseBannerInfo(string reference, string title, string category)
    {
        var t = (title ?? "").ToLowerInvariant();
        var r = (reference ?? "").ToUpperInvariant();

        if (r.Contains("DEVOPS") || t.Contains("devops") || t.Contains("ci/cd") || t.Contains("docker") || t.Contains("kubernetes"))
        {
            return ("background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);", new List<(string, string, string)>
            {
                ("cloud_queue", "CI/CD", "#38bdf8"),
                ("dns", "Docker/K8s", "#34d399")
            });
        }
        if (r.Contains("WEB") || t.Contains("react") || t.Contains("vue") || t.Contains("angular") || t.Contains("frontend"))
        {
            return ("background: linear-gradient(135deg, #1e1b4b 0%, #312e81 100%);", new List<(string, string, string)>
            {
                ("code", "React", "#61dafb"),
                ("web", "Vue", "#42b883"),
                ("layers", "Angular", "#f87171")
            });
        }
        if (r.Contains("DEV") || t.Contains("phần mềm") || t.Contains("công nghệ phần mềm") || t.Contains("software") || t.Contains("lập trình") || t.Contains("coding") || t.Contains("developer"))
        {
            return ("background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 50%, #2563eb 100%);", new List<(string, string, string)>
            {
                ("code", "Công nghệ phần mềm", "#60a5fa"),
                ("terminal", "Software Dev", "#38bdf8"),
                ("developer_mode", "Kiến trúc hệ thống", "#a5f3fc")
            });
        }
        if (r.Contains("AGILE") || t.Contains("agile") || t.Contains("scrum") || t.Contains("sprint"))
        {
            return ("background: linear-gradient(135deg, #064e3b 0%, #059669 100%);", new List<(string, string, string)>
            {
                ("sync", "Agile Sprint", "#86efac"),
                ("groups", "Scrum", "#bbf7d0")
            });
        }
        if (r.Contains("ENG") || t.Contains("tiếng anh") || t.Contains("english") || t.Contains("ngoại ngữ"))
        {
            return ("background: linear-gradient(135deg, #4c1d95 0%, #6d28d9 100%);", new List<(string, string, string)>
            {
                ("record_voice_over", "English Speaking", "#e9d5ff")
            });
        }
        if (r.Contains("TIME") || t.Contains("thời gian") || t.Contains("kỹ năng"))
        {
            return ("background: linear-gradient(135deg, #7c2d12 0%, #c2410c 100%);", new List<(string, string, string)>
            {
                ("schedule", "Quản lý thời gian", "#fed7aa")
            });
        }
        if (r.Contains("LEAD") || t.Contains("lãnh đạo") || t.Contains("quản trị") || t.Contains("leader") || t.Contains("manager"))
        {
            return ("background: linear-gradient(135deg, #312e81 0%, #4338ca 100%);", new List<(string, string, string)>
            {
                ("star", "Leaders & Managers", "#c7d2fe")
            });
        }
        if (r.Contains("PM") || t.Contains("dự án") || t.Contains("project") || t.Contains("jira") || t.Contains("trello"))
        {
            return ("background: linear-gradient(135deg, #0c4a6e 0%, #0369a1 100%);", new List<(string, string, string)>
            {
                ("view_kanban", "Jira", "#60a5fa"),
                ("dashboard_customize", "Trello", "#93c5fd")
            });
        }

        var defGrad = category == "Offline"
            ? "background: linear-gradient(135deg, #0f172a 0%, #334155 100%);"
            : "background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%);";
        return (defGrad, new List<(string, string, string)>
        {
            ("school", string.IsNullOrWhiteSpace(title) ? "Đào tạo nghiệp vụ" : title, "#93c5fd")
        });
    }
}

