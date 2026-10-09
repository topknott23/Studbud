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
    public List<SubjectDef> Subjects { get; private set; } = new(SubjectDef.DefaultSubjects);
    public List<SubjectNoteItem> SubjectNotes { get; private set; } = new(SubjectNoteItem.DefaultNotes);
    public List<SubjectResourceItem> SubjectResources { get; private set; } = new();
    public List<FlashcardDeck> FlashcardDecks { get; private set; } = new();

    // Timer State
    public TimerMode CurrentMode { get; private set; } = TimerMode.Work;
    public int RemainingSeconds { get; private set; } = 25 * 60;
    public int TotalModeSeconds { get; private set; } = 25 * 60;
    public bool IsRunning { get; private set; } = false;
    public int CompletedPomodoroCount { get; private set; } = 0;
    public string? ActiveTaskId { get; set; }

    // Alarm State
    public bool IsAlarmRinging { get; private set; } = false;
    public TimerMode AlarmTriggeredMode { get; private set; } = TimerMode.Work;
    public string AlarmSoundPath { get; set; } = "alarm/Chinese Meme Ringtone Download.mp3";

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

            var subjectsJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_subjects");
            if (!string.IsNullOrEmpty(subjectsJson))
            {
                var loadedSubjects = JsonSerializer.Deserialize<List<SubjectDef>>(subjectsJson);
                if (loadedSubjects != null && loadedSubjects.Any())
                {
                    Subjects = loadedSubjects;
                }
            }

            var notesJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_subject_notes");
            if (!string.IsNullOrEmpty(notesJson))
            {
                var loadedNotes = JsonSerializer.Deserialize<List<SubjectNoteItem>>(notesJson);
                if (loadedNotes != null && loadedNotes.Any())
                {
                    SubjectNotes = loadedNotes;
                }
            }

            var resourcesJson = await _js.InvokeAsync<string?>("localStorage.getItem", "study_subject_resources");
            if (!string.IsNullOrEmpty(resourcesJson))
            {
                var loadedResources = JsonSerializer.Deserialize<List<SubjectResourceItem>>(resourcesJson);
                if (loadedResources != null)
                {
                    SubjectResources = loadedResources;
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
        if (IsAlarmRinging)
        {
            _ = StopAlarmOnlyAsync();
        }
        if (IsRunning) return;
        IsRunning = true;
        _timer?.Start();
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void PauseTimer()
    {
        if (IsAlarmRinging)
        {
            _ = StopAlarmOnlyAsync();
        }
        if (!IsRunning) return;
        IsRunning = false;
        _timer?.Stop();
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void ResetTimer()
    {
        if (IsAlarmRinging)
        {
            _ = StopAlarmOnlyAsync();
        }
        _timer?.Stop();
        IsRunning = false;
        RemainingSeconds = TotalModeSeconds;
        NotifyStateChanged();
        UpdateDocumentTitle();
    }

    public void SetMode(TimerMode mode)
    {
        if (IsAlarmRinging)
        {
            _ = StopAlarmOnlyAsync();
        }
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
        if (IsAlarmRinging)
        {
            _ = StopAlarmOnlyAsync();
        }
        _timer?.Stop();
        IsRunning = false;
        AdvanceCycle(false);
    }

    public async Task StopAlarmOnlyAsync()
    {
        if (IsAlarmRinging)
        {
            IsAlarmRinging = false;
            try
            {
                await _js.InvokeVoidAsync("studyApp.stopAlarm");
            }
            catch { }
            NotifyStateChanged();
            UpdateDocumentTitle();
        }
    }

    public async Task StopAlarmAsync()
    {
        await StopAlarmOnlyAsync();
        AdvanceCycle(true);
    }

    public async Task ExtendTimerAsync(int additionalMinutes = 5)
    {
        await StopAlarmOnlyAsync();
        CurrentMode = AlarmTriggeredMode;
        RemainingSeconds = Math.Max(1, additionalMinutes) * 60;
        TotalModeSeconds = RemainingSeconds;
        IsRunning = true;
        _timer?.Start();
        NotifyStateChanged();
        UpdateDocumentTitle();
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
        IsAlarmRinging = true;
        AlarmTriggeredMode = CurrentMode;

        if (Settings.SoundEnabled)
        {
            try
            {
                await _js.InvokeVoidAsync("studyApp.startAlarm", AlarmSoundPath);
            }
            catch { }
        }

        var activeTask = Tasks.FirstOrDefault(t => t.Id == ActiveTaskId);
        var record = new SessionRecord
        {
            Mode = CurrentMode,
            Subject = activeTask?.Subject ?? (CurrentMode == TimerMode.Work ? "General Study" : "Break"),
            TaskTitle = activeTask?.Title ?? (CurrentMode == TimerMode.Work ? "Focus Session" : "Rest & Recharge"),
            DurationMinutes = Math.Max(1, TotalModeSeconds / 60),
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
        NotifyStateChanged();
        UpdateDocumentTitle();
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

    // Subject & Notes Management
    public async Task AddSubjectAsync(string name, string color, string description, string code = "", string instructor = "", string schedule = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var sub = new SubjectDef
        {
            Name = name.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? "#6366F1" : color,
            Description = description.Trim(),
            Code = code.Trim(),
            Instructor = instructor.Trim(),
            Schedule = schedule.Trim()
        };
        Subjects.Add(sub);
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task UpdateSubjectAsync(SubjectDef updated)
    {
        var existing = Subjects.FirstOrDefault(s => s.Id == updated.Id);
        if (existing != null)
        {
            var oldName = existing.Name;
            existing.Name = updated.Name.Trim();
            existing.Color = updated.Color;
            existing.Description = updated.Description.Trim();
            existing.Code = updated.Code.Trim();
            existing.Instructor = updated.Instructor.Trim();
            existing.Schedule = updated.Schedule.Trim();

            // Update associated tasks and notes if name changed
            if (oldName != existing.Name)
            {
                foreach (var note in SubjectNotes.Where(n => n.SubjectId == existing.Id || n.SubjectName == oldName))
                {
                    note.SubjectName = existing.Name;
                    note.SubjectId = existing.Id;
                }
                foreach (var task in Tasks.Where(t => t.Subject == oldName))
                {
                    task.Subject = existing.Name;
                }
            }

            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task DeleteSubjectAsync(string subjectId)
    {
        var sub = Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (sub != null)
        {
            Subjects.Remove(sub);
            SubjectNotes.RemoveAll(n => n.SubjectId == subjectId || n.SubjectName == sub.Name);
            SubjectResources.RemoveAll(r => r.SubjectId == subjectId);
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task AddSubjectNoteAsync(string subjectId, string title, string topic, string content, string tags = "", bool isPinned = false)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        var sub = Subjects.FirstOrDefault(s => s.Id == subjectId) ?? Subjects.FirstOrDefault();
        var note = new SubjectNoteItem
        {
            SubjectId = sub?.Id ?? subjectId,
            SubjectName = sub?.Name ?? "General",
            Title = title.Trim(),
            Topic = topic.Trim(),
            Content = content,
            Tags = tags.Trim(),
            IsPinned = isPinned,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        SubjectNotes.Insert(0, note);
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task UpdateSubjectNoteAsync(SubjectNoteItem updated)
    {
        var note = SubjectNotes.FirstOrDefault(n => n.Id == updated.Id);
        if (note != null)
        {
            note.Title = updated.Title.Trim();
            note.Topic = updated.Topic.Trim();
            note.Content = updated.Content;
            note.Tags = updated.Tags.Trim();
            note.IsPinned = updated.IsPinned;
            note.SubjectId = updated.SubjectId;
            var sub = Subjects.FirstOrDefault(s => s.Id == updated.SubjectId);
            if (sub != null) note.SubjectName = sub.Name;
            note.UpdatedAt = DateTime.Now;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task DeleteSubjectNoteAsync(string noteId)
    {
        var note = SubjectNotes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            SubjectNotes.Remove(note);
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task TogglePinNoteAsync(string noteId)
    {
        var note = SubjectNotes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            note.IsPinned = !note.IsPinned;
            note.UpdatedAt = DateTime.Now;
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task AddSubjectResourceAsync(string subjectId, string title, string urlOrRef, string type = "Cheatsheet")
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        SubjectResources.Add(new SubjectResourceItem
        {
            SubjectId = subjectId,
            Title = title.Trim(),
            UrlOrReference = urlOrRef.Trim(),
            Type = type
        });
        await SaveDataAsync();
        NotifyStateChanged();
    }

    public async Task DeleteSubjectResourceAsync(string resId)
    {
        var res = SubjectResources.FirstOrDefault(r => r.Id == resId);
        if (res != null)
        {
            SubjectResources.Remove(res);
            await SaveDataAsync();
            NotifyStateChanged();
        }
    }

    public async Task ResetAllDataAsync()
    {
        Tasks.Clear();
        SessionHistory.Clear();
        Subjects = new(SubjectDef.DefaultSubjects);
        SubjectNotes = new(SubjectNoteItem.DefaultNotes);
        SubjectResources.Clear();
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
            await _js.InvokeVoidAsync("localStorage.setItem", "study_subjects", JsonSerializer.Serialize(Subjects));
            await _js.InvokeVoidAsync("localStorage.setItem", "study_subject_notes", JsonSerializer.Serialize(SubjectNotes));
            await _js.InvokeVoidAsync("localStorage.setItem", "study_subject_resources", JsonSerializer.Serialize(SubjectResources));
        }
        catch { }
    }

    private void UpdateDocumentTitle()
    {
        try
        {
            if (IsAlarmRinging)
            {
                string label = AlarmTriggeredMode == TimerMode.Work ? "Focus Completed!" : "Break Over!";
                _ = _js.InvokeVoidAsync("studyApp.setTitle", $"⏰ {label} (Alarm Ringing...) | Studbud");
                return;
            }

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
        try
        {
            _ = _js.InvokeVoidAsync("studyApp.stopAlarm");
        }
        catch { }
    }
}
