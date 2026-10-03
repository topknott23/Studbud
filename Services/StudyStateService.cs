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
    public List<FlashcardDeck> FlashcardDecks { get; private set; } = new();

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
                if (loadedTasks != null)
                {
                    Tasks = loadedTasks;
                }
            }

            var historyJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_history");
            if (!string.IsNullOrEmpty(historyJson))
            {
                var loadedHistory = JsonSerializer.Deserialize<List<SessionRecord>>(historyJson);
                if (loadedHistory != null)
                {
                    SessionHistory = loadedHistory;
                    CompletedPomodoroCount = SessionHistory.Count(s => s.Mode == TimerMode.Work && s.CompletedAt.Date == DateTime.Today);
                }
            }

            var decksJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_flashcards");
            if (!string.IsNullOrEmpty(decksJson))
            {
                var loadedDecks = JsonSerializer.Deserialize<List<FlashcardDeck>>(decksJson);
                if (loadedDecks != null)
                {
                    FlashcardDecks = loadedDecks;
                }
            }
        }
        catch { }

        SetMode(TimerMode.Work);
        NotifyStateChanged();
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
        if (Settings.SoundEnabled)
        {
            try
            {
                await _js.InvokeVoidAsync("studyApp.playChime");
            }
            catch { }
        }

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
                activeTask.Status = StudyTaskStatus.InProgress;
                if (activeTask.CompletedPomodoros >= activeTask.EstimatedPomodoros)
                {
                    activeTask.IsCompleted = true;
                    activeTask.Status = StudyTaskStatus.Done;
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
    public async Task AddTaskAsync(string title, string subject, int estimatedPomodoros = 1, TaskPriority priority = TaskPriority.Medium, string description = "", DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        var task = new TaskItem
        {
            Title = title.Trim(),
            Subject = subject,
            Priority = priority,
            Description = description,
            EstimatedPomodoros = Math.Max(1, estimatedPomodoros),
            CompletedPomodoros = 0,
            Status = StudyTaskStatus.Todo,
            IsCompleted = false,
            DueDate = dueDate
        };

        Tasks.Insert(0, task);
        if (string.IsNullOrEmpty(ActiveTaskId))
        {
            ActiveTaskId = task.Id;
        }

        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task UpdateTaskAsync(TaskItem updatedTask)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);
        if (task != null)
        {
            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Subject = updatedTask.Subject;
            task.Priority = updatedTask.Priority;
            task.EstimatedPomodoros = updatedTask.EstimatedPomodoros;
            task.DueDate = updatedTask.DueDate;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task MoveTaskStatusAsync(string taskId, StudyTaskStatus newStatus)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            task.Status = newStatus;
            task.IsCompleted = newStatus == StudyTaskStatus.Done;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task ToggleTaskCompletionAsync(string taskId)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            task.Status = task.IsCompleted ? StudyTaskStatus.Done : StudyTaskStatus.Todo;
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

    // Flashcards Management
    public async Task ToggleFlashcardMasteryAsync(string deckId, string cardId)
    {
        var deck = FlashcardDecks.FirstOrDefault(d => d.Id == deckId);
        var card = deck?.Cards.FirstOrDefault(c => c.Id == cardId);
        if (card != null)
        {
            card.IsMastered = !card.IsMastered;
            card.ReviewCount++;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task AddFlashcardDeckAsync(string title, string subject, string description, string color)
    {
        var deck = new FlashcardDeck
        {
            Title = title,
            Subject = subject,
            Description = description,
            Color = color
        };
        FlashcardDecks.Add(deck);
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task AddFlashcardToDeckAsync(string deckId, string question, string answer)
    {
        var deck = FlashcardDecks.FirstOrDefault(d => d.Id == deckId);
        if (deck != null)
        {
            deck.Cards.Add(new FlashcardItem
            {
                Question = question,
                Answer = answer,
                Subject = deck.Subject
            });
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task UpdateSettingsAsync(UserSettings newSettings)
    {
        Settings = newSettings;
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

    public async Task ClearAllTasksAsync()
    {
        Tasks.Clear();
        ActiveTaskId = null;
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task ResetAllDataAsync()
    {
        Tasks.Clear();
        SessionHistory.Clear();
        ActiveTaskId = null;
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
            await _js.InvokeVoidAsync("localStorage.setItem", "study_flashcards", JsonSerializer.Serialize(FlashcardDecks));
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
            string title = $"{timeStr} - {modeName} | Studbud";
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
