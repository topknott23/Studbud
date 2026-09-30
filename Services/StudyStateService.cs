using System.Text.Json;
using Microsoft.JSInterop;
using StudyPomodoroDashboard.Models;

namespace StudyPomodoroDashboard.Services;

public class StudyStateService : IDisposable
{
    private readonly IJSRuntime _js;
    private System.Timers.Timer? _timer;

    // State properties
    public UserSettings Settings { get; private set; } = new();
    public List<TaskItem> Tasks { get; private set; } = new();
    public List<SessionRecord> SessionHistory { get; private set; } = new();
    public List<SubjectDef> Subjects { get; private set; } = SubjectDef.DefaultSubjects;

    // Timer State
    public TimerMode CurrentMode { get; private set; } = TimerMode.Work;
    public int RemainingSeconds { get; private set; } = 25 * 60;
    public int TotalModeSeconds { get; private set; } = 25 * 60;
    public bool IsRunning { get; private set; } = false;
    public int CompletedPomodoroCount { get; private set; } = 0;
    public string? ActiveTaskId { get; set; }

    public event Action? OnChange;

    public StudyStateService(IJSRuntime js)
    {
        _js = js;
        InitTimer();
    }

    private void InitTimer()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerTick;
        _timer.AutoReset = true;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var settingsJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_settings");
            if (!string.IsNullOrEmpty(settingsJson))
            {
                var loadedSettings = JsonSerializer.Deserialize<UserSettings>(settingsJson);
                if (loadedSettings != null) Settings = loadedSettings;
            }

            var tasksJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_tasks");
            if (!string.IsNullOrEmpty(tasksJson))
            {
                var loadedTasks = JsonSerializer.Deserialize<List<TaskItem>>(tasksJson);
                if (loadedTasks != null && loadedTasks.Count > 0)
                {
                    Tasks = loadedTasks;
                }
                else
                {
                    SeedDefaultTasks();
                }
            }
            else
            {
                SeedDefaultTasks();
            }

            var historyJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_history");
            if (!string.IsNullOrEmpty(historyJson))
            {
                var loadedHistory = JsonSerializer.Deserialize<List<SessionRecord>>(historyJson);
                if (loadedHistory != null)
                {
                    SessionHistory = loadedHistory;
                    // Calculate today's completed pomodoros
                    CompletedPomodoroCount = SessionHistory.Count(s => s.Mode == TimerMode.Work && s.CompletedAt.Date == DateTime.Today);
                }
            }
            else
            {
                SeedDefaultHistory();
            }
        }
        catch
        {
            // Fallback to in-memory defaults if storage is unavailable
            if (Tasks.Count == 0) SeedDefaultTasks();
            if (SessionHistory.Count == 0) SeedDefaultHistory();
        }

        SetMode(TimerMode.Work);
        NotifyStateChanged();
    }

    private void SeedDefaultTasks()
    {
        Tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Implement Finite State Machine Diagram", Subject = "Automata Theory", EstimatedPomodoros = 3, CompletedPomodoros = 2, IsCompleted = false },
            new TaskItem { Title = "Configure Blazor State Management & UI Components", Subject = "Application Development", EstimatedPomodoros = 4, CompletedPomodoros = 3, IsCompleted = false },
            new TaskItem { Title = "Train Neural Network Classification Model", Subject = "Intelligent Systems", EstimatedPomodoros = 2, CompletedPomodoros = 2, IsCompleted = true, CompletedAt = DateTime.Now.AddHours(-1) },
            new TaskItem { Title = "Prepare Business Model Canvas & Pitch Deck", Subject = "Technopreneurship", EstimatedPomodoros = 2, CompletedPomodoros = 0, IsCompleted = false },
            new TaskItem { Title = "Solve Linear Programming Simplex Method", Subject = "Quantitative Methods", EstimatedPomodoros = 3, CompletedPomodoros = 1, IsCompleted = false },
            new TaskItem { Title = "Design Normalized Relational Schema & Indexes", Subject = "Information Management 2", EstimatedPomodoros = 2, CompletedPomodoros = 0, IsCompleted = false }
        };
    }

    private void SeedDefaultHistory()
    {
        SessionHistory = new List<SessionRecord>
        {
            new SessionRecord { Mode = TimerMode.Work, Subject = "Application Development", TaskTitle = "Configure Blazor State Management & UI Components", DurationMinutes = 25, CompletedAt = DateTime.Now.AddHours(-3) },
            new SessionRecord { Mode = TimerMode.Work, Subject = "Intelligent Systems", TaskTitle = "Train Neural Network Classification Model", DurationMinutes = 25, CompletedAt = DateTime.Now.AddHours(-2) },
            new SessionRecord { Mode = TimerMode.Work, Subject = "Automata Theory", TaskTitle = "Implement Finite State Machine Diagram", DurationMinutes = 25, CompletedAt = DateTime.Now.AddHours(-1) },
            new SessionRecord { Mode = TimerMode.ShortBreak, Subject = "Break", TaskTitle = "Hydration & Rest", DurationMinutes = 5, CompletedAt = DateTime.Now.AddMinutes(-35) }
        };
        CompletedPomodoroCount = SessionHistory.Count(s => s.Mode == TimerMode.Work && s.CompletedAt.Date == DateTime.Today);
    }

    // Timer Controls
    public void StartTimer()
    {
        if (IsRunning) return;
        IsRunning = true;
        _timer?.Start();
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void PauseTimer()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _timer?.Stop();
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void ResetTimer()
    {
        _timer?.Stop();
        IsRunning = false;
        RemainingSeconds = TotalModeSeconds;
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void SetMode(TimerMode mode)
    {
        _timer?.Stop();
        IsRunning = false;
        CurrentMode = mode;
        TotalModeSeconds = mode switch
        {
            TimerMode.Work => Settings.WorkDurationMinutes * 60,
            TimerMode.ShortBreak => Settings.ShortBreakMinutes * 60,
            TimerMode.LongBreak => Settings.LongBreakMinutes * 60,
            _ => 25 * 60
        };
        RemainingSeconds = TotalModeSeconds;
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void SkipTimer()
    {
        _timer?.Stop();
        IsRunning = false;
        AdvanceCycle(false);
    }

    private async void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (RemainingSeconds > 0)
        {
            RemainingSeconds--;
            NotifyStateChanged();
            UpdateDocumentTitle();
        }
        else
        {
            _timer?.Stop();
            IsRunning = false;
            await OnSessionFinished();
        }
    }

    private async Task OnSessionFinished()
    {
        // Play notification sound
        if (Settings.SoundEnabled)
        {
            try
            {
                await _js.InvokeVoidAsync("studyApp.playChime");
            }
            catch { }
        }

        // Record session history
        var activeTask = Tasks.FirstOrDefault(t => t.Id == ActiveTaskId);
        var record = new SessionRecord
        {
            Mode = CurrentMode,
            Subject = activeTask?.Subject ?? (CurrentMode == TimerMode.Work ? "General Study" : "Break"),
            TaskTitle = activeTask?.Title ?? (CurrentMode == TimerMode.Work ? "Focus Session" : "Rest & Recharge"),
            DurationMinutes = TotalModeSeconds / 60,
            CompletedAt = DateTime.Now
        };

        SessionHistory.Insert(0, record);

        if (CurrentMode == TimerMode.Work)
        {
            CompletedPomodoroCount++;
            if (activeTask != null)
            {
                activeTask.CompletedPomodoros++;
                if (activeTask.CompletedPomodoros >= activeTask.EstimatedPomodoros)
                {
                    activeTask.IsCompleted = true;
                    activeTask.CompletedAt = DateTime.Now;
                }
            }
        }

        await SaveDataAsync();
        AdvanceCycle(true);
    }

    private void AdvanceCycle(bool autoTransition)
    {
        if (CurrentMode == TimerMode.Work)
        {
            if (CompletedPomodoroCount > 0 && CompletedPomodoroCount % Settings.LongBreakInterval == 0)
            {
                SetMode(TimerMode.LongBreak);
            }
            else
            {
                SetMode(TimerMode.ShortBreak);
            }

            if (autoTransition && Settings.AutoStartBreaks)
            {
                StartTimer();
            }
        }
        else
        {
            SetMode(TimerMode.Work);
            if (autoTransition && Settings.AutoStartPomodoros)
            {
                StartTimer();
            }
        }

        NotifyStateChanged();
    }

    // Task Management
    public async Task AddTaskAsync(string title, string subject, int estimatedPomodoros = 1)
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        var task = new TaskItem
        {
            Title = title.Trim(),
            Subject = subject,
            EstimatedPomodoros = Math.Max(1, estimatedPomodoros),
            CompletedPomodoros = 0,
            IsCompleted = false
        };

        Tasks.Insert(0, task);
        if (string.IsNullOrEmpty(ActiveTaskId))
        {
            ActiveTaskId = task.Id;
        }

        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task ToggleTaskCompletionAsync(string taskId)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task DeleteTaskAsync(string taskId)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            Tasks.Remove(task);
            if (ActiveTaskId == taskId)
            {
                ActiveTaskId = Tasks.FirstOrDefault(t => !t.IsCompleted)?.Id;
            }
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task UpdateSettingsAsync(UserSettings newSettings)
    {
        Settings = newSettings;
        // Adjust current timer duration if not running
        if (!IsRunning)
        {
            SetMode(CurrentMode);
        }
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task ClearHistoryAsync()
    {
        SessionHistory.Clear();
        CompletedPomodoroCount = 0;
        await SaveDataAsync();
        NotifyStateChanged();
    }

    private async Task SaveDataAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "study_tasks", JsonSerializer.Serialize(Tasks));
            await _js.InvokeVoidAsync("localStorage.setItem", "study_history", JsonSerializer.Serialize(SessionHistory));
            await _js.InvokeVoidAsync("localStorage.setItem", "study_settings", JsonSerializer.Serialize(Settings));
        }
        catch { }
    }

    private void UpdateDocumentTitle()
    {
        try
        {
            int mins = RemainingSeconds / 60;
            int secs = RemainingSeconds % 60;
            string timeStr = $"{mins:D2}:{secs:D2}";
            string modeName = CurrentMode == TimerMode.Work ? "Focus" : "Break";
            string title = $"{timeStr} - {modeName} | StudyPlanner";
            _ = _js.InvokeVoidAsync("studyApp.setTitle", title);
        }
        catch { }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
