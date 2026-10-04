namespace StudyPomodoroDashboard.Models;

public enum TimerMode
{
    Work,
    ShortBreak,
    LongBreak
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public enum StudyTaskStatus
{
    Todo,
    InProgress,
    Done
}

public class SubjectDef
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366F1";
    public string Description { get; set; } = string.Empty;

    public static List<SubjectDef> DefaultSubjects => new()
    {
        new SubjectDef { Name = "Automata Theory", Color = "#6366F1", Description = "DFAs, NFAs, Turing machines, and formal grammars" },
        new SubjectDef { Name = "Application Development", Color = "#3B82F6", Description = "Blazor WebAssembly, UI/UX, and software architecture" },
        new SubjectDef { Name = "Intelligent Systems", Color = "#10B981", Description = "Neural networks, classification, heuristics & search" },
        new SubjectDef { Name = "Applied AI", Color = "#EC4899", Description = "Deep learning, LLMs, computer vision & applied neural systems" },
        new SubjectDef { Name = "Technopreneurship", Color = "#F59E0B", Description = "Business models, startup feasibility, and product design" },
        new SubjectDef { Name = "Quantitative Methods", Color = "#8B5CF6", Description = "Linear programming, simplex method, and optimization" },
        new SubjectDef { Name = "Information Management 2", Color = "#06B6D4", Description = "Database normalization, relational schemas & indexing" }
    };
}

public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = "Application Development";
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public StudyTaskStatus Status { get; set; } = StudyTaskStatus.Todo;
    public int EstimatedPomodoros { get; set; } = 1;
    public int CompletedPomodoros { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? DueDate { get; set; }
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
    public string Subject { get; set; } = "Application Development";
    public string TaskTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 25;
    public DateTime CompletedAt { get; set; } = DateTime.Now;
}

public class FlashcardItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Subject { get; set; } = "Application Development";
    public bool IsMastered { get; set; } = false;
    public int ReviewCount { get; set; } = 0;
}

public class FlashcardDeck
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = "Application Development";
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366F1";
    public List<FlashcardItem> Cards { get; set; } = new();
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
    public string ActivePreset { get; set; } = "Classic (25/5)";
}
