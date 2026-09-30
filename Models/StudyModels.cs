namespace StudyPomodoroDashboard.Models;

public enum TimerMode
{
    Work,
    ShortBreak,
    LongBreak
}

public class SubjectDef
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366F1"; // Default Indigo
    public string Icon { get; set; } = "📚";

    public static List<SubjectDef> DefaultSubjects => new()
    {
        new SubjectDef { Name = "Data Structures", Color = "#6366F1", Icon = "💻" },
        new SubjectDef { Name = "Calculus", Color = "#10B981", Icon = "📐" },
        new SubjectDef { Name = "Web Dev", Color = "#F59E0B", Icon = "🌐" },
        new SubjectDef { Name = "Physics", Color = "#EC4899", Icon = "⚡" },
        new SubjectDef { Name = "General Study", Color = "#8B5CF6", Icon = "📖" }
    };
}

public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = "General Study";
    public int EstimatedPomodoros { get; set; } = 1;
    public int CompletedPomodoros { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}

public class SessionRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TimerMode Mode { get; set; }
    public string ModeDisplay => Mode switch
    {
        TimerMode.Work => "Focus Session",
        TimerMode.ShortBreak => "Short Break",
        TimerMode.LongBreak => "Long Break",
        _ => "Session"
    };
    public string Subject { get; set; } = "General Study";
    public string TaskTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 25;
    public DateTime CompletedAt { get; set; } = DateTime.Now;
}

public class UserSettings
{
    public int WorkDurationMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakInterval { get; set; } = 4;
    public int DailyPomodoroGoal { get; set; } = 8;
    public bool SoundEnabled { get; set; } = true;
    public bool AutoStartBreaks { get; set; } = false;
    public bool AutoStartPomodoros { get; set; } = false;
    public string Theme { get; set; } = "dark";
}
