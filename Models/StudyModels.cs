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
    public string Code { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;

    public static List<SubjectDef> DefaultSubjects => new()
    {
        new SubjectDef { Id = "sub-automata", Name = "Automata Theory", Color = "#6366F1", Code = "CS-311", Description = "DFAs, NFAs, Turing machines, and formal grammars", Instructor = "Prof. Alan Turing", Schedule = "Mon/Wed 10:00 - 11:30 AM" },
        new SubjectDef { Id = "sub-appdev", Name = "Application Development", Color = "#3B82F6", Code = "CS-312", Description = "Blazor WebAssembly, UI/UX, and software architecture", Instructor = "Dr. Grace Hopper", Schedule = "Tue/Thu 1:00 - 2:30 PM" },
        new SubjectDef { Id = "sub-intsys", Name = "Intelligent Systems", Color = "#10B981", Code = "CS-313", Description = "Neural networks, classification, heuristics & search", Instructor = "Dr. Marvin Minsky", Schedule = "Mon/Wed 1:30 - 3:00 PM" },
        new SubjectDef { Id = "sub-appliedai", Name = "Applied AI", Color = "#EC4899", Code = "AI-401", Description = "Deep learning, LLMs, computer vision & applied neural systems", Instructor = "Dr. Yann LeCun", Schedule = "Tue/Thu 3:00 - 4:30 PM" },
        new SubjectDef { Id = "sub-techno", Name = "Technopreneurship", Color = "#F59E0B", Code = "ENT-201", Description = "Business models, startup feasibility, and product design", Instructor = "Prof. Steve Blank", Schedule = "Fri 9:00 - 12:00 PM" },
        new SubjectDef { Id = "sub-quant", Name = "Quantitative Methods", Color = "#8B5CF6", Code = "MATH-205", Description = "Linear programming, simplex method, and optimization", Instructor = "Dr. John von Neumann", Schedule = "Wed/Fri 8:30 - 10:00 AM" },
        new SubjectDef { Id = "sub-im2", Name = "Information Management 2", Color = "#06B6D4", Code = "IM-202", Description = "Database normalization, relational schemas & indexing", Instructor = "Dr. Edgar Codd", Schedule = "Thu 9:00 - 12:00 PM" }
    };
}

public class SubjectNoteItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SubjectId { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public static List<SubjectNoteItem> DefaultNotes => new()
    {
        new SubjectNoteItem
        {
            Id = "note-ai-clustering",
            SubjectId = "sub-appliedai",
            SubjectName = "Applied AI",
            Title = "Clustering Notes: K-Means, DBSCAN & Hierarchical",
            Topic = "Clustering & Unsupervised Learning",
            IsPinned = true,
            Tags = "Clustering, Machine Learning, K-Means, DBSCAN",
            CreatedAt = DateTime.Now.AddDays(-3),
            UpdatedAt = DateTime.Now.AddDays(-1),
            Content = @"### Unsupervised Learning: Clustering Cheat Sheet

#### 1. K-Means Clustering
- **Objective:** Partition $N$ observations into $K$ clusters by minimizing Within-Cluster Sum of Squares (WCSS / Inertia):
$$\text{WCSS} = \sum_{k=1}^{K} \sum_{x_i \in C_k} ||x_i - \mu_k||^2$$

- **Algorithm Steps:**
  1. Initialize $K$ centroids randomly (or via K-Means++ for better spread).
  2. **Assignment Step:** Assign each data point $x_i$ to nearest centroid $\mu_k$ using Euclidean distance.
  3. **Update Step:** Recalculate centroids as the mean of all points assigned to that cluster: $\mu_k = \frac{1}{|C_k|} \sum_{x \in C_k} x$.
  4. Repeat until centroids converge (no change) or maximum iterations reached.

- **Finding Optimal K:**
  - **Elbow Method:** Plot WCSS vs $K$, pick the 'elbow point' where reduction rate sharply flattens.
  - **Silhouette Coefficient:** Range $[-1, 1]$. Score near $+1$ indicates well-separated clusters.

---

#### 2. DBSCAN (Density-Based Spatial Clustering)
- **Key Parameters:**
  - $\epsilon$ (eps): Radius of neighborhood around a point.
  - $\text{min\_samples}$: Minimum points required inside $\epsilon$-radius to form a dense region.
- **Point Categories:**
  - **Core Point:** Has $\ge \text{min\_samples}$ within distance $\epsilon$.
  - **Border Point:** Within $\epsilon$ of a core point but has $<\text{min\_samples}$ itself.
  - **Noise Point / Outlier:** Neither core nor border.
- **Pros:** Can discover arbitrary shape clusters (non-spherical) and automatically isolates noise/outliers!

---

#### 3. Hierarchical Clustering (Dendrograms)
- **Agglomerative (Bottom-Up):** Start with each point as its own cluster, iteratively merge closest pairs.
- **Linkage Criteria:** Single (minimum distance), Complete (maximum distance), Average, Ward's Minimum Variance."
        },
        new SubjectNoteItem
        {
            Id = "note-automata-nfa-dfa",
            SubjectId = "sub-automata",
            SubjectName = "Automata Theory",
            Title = "How to do NFA to DFA Conversion (Subset Construction)",
            Topic = "Finite Automata & Regular Languages",
            IsPinned = true,
            Tags = "NFA, DFA, Subset Construction, Epsilon Closure",
            CreatedAt = DateTime.Now.AddDays(-4),
            UpdatedAt = DateTime.Now.AddDays(-2),
            Content = @"### Step-by-Step Guide: NFA to DFA Conversion

#### Overview (Subset Construction / Powerset Method)
Given an NFA $M = (Q, \Sigma, \delta, q_0, F)$, we construct an equivalent DFA $M' = (Q', \Sigma, \delta', q_0', F')$ where each state in $Q'$ is a **subset** of $Q$.

---

#### Step 1: Compute $\epsilon$-Closure of Initial State
- Find $\epsilon\text{-closure}(q_0)$ = set of all states reachable from $q_0$ using zero or more $\epsilon$-transitions.
- This subset forms the **initial state** of the DFA: $q_0' = \epsilon\text{-closure}(q_0)$.

---

#### Step 2: Build the DFA Transition Table
For every discovered state subset $S \subseteq Q$ and for every input symbol $a \in \Sigma$:
$$\delta'(S, a) = \bigcup_{p \in S} \epsilon\text{-closure}(\delta(p, a))$$

1. Take the active subset $S$.
2. For each symbol $a \in \Sigma$, look at all states reachable on $a$ from any state in $S$.
3. Compute the $\epsilon$-closure of that destination set.
4. The resulting set is a new state in DFA. If not already in the transition table, add it as a new row to explore!

---

#### Step 3: Identify Accepting States ($F'$)
- Any DFA state (subset) that contains **at least one** accepting state from the original NFA ($q_f \in F$) is marked as an **accepting/final state** in the DFA!

---

#### Step 4: Draw the DFA Diagram
- Rename subsets with clean labels (e.g., $A = \{q_0, q_1\}$, $B = \{q_2\}$, etc.)
- Connect directed edges labeled with input symbols.
- Remove unreachable states (dead ends)."
        },
        new SubjectNoteItem
        {
            Id = "note-automata-pumping",
            SubjectId = "sub-automata",
            SubjectName = "Automata Theory",
            Title = "Pumping Lemma for Regular Languages",
            Topic = "Non-Regular Proofs",
            IsPinned = false,
            Tags = "Automata, Pumping Lemma, Proofs",
            CreatedAt = DateTime.Now.AddDays(-5),
            UpdatedAt = DateTime.Now.AddDays(-5),
            Content = @"### Pumping Lemma Statement
If $L$ is a regular language, then there exists a pumping length $p \ge 1$ such that any string $w \in L$ with $|w| \ge p$ can be divided into three parts $w = xyz$ satisfying:
1. $|y| \ge 1$ ($y$ is not empty)
2. $|xy| \le p$
3. For all $i \ge 0$, $x y^i z \in L$.

#### Proof by Contradiction Strategy:
1. Assume $L$ is regular (so pumping length $p$ exists).
2. Pick a clever string $w \in L$ with length $\ge p$ (e.g., $0^p 1^p$).
3. Consider all possible splits $w = xyz$ constrained by $|xy| \le p$.
4. Find an $i$ (such as $i=0$ or $i=2$) where $x y^i z \notin L$, establishing a contradiction!"
        },
        new SubjectNoteItem
        {
            Id = "note-appdev-blazor",
            SubjectId = "sub-appdev",
            SubjectName = "Application Development",
            Title = "Blazor WebAssembly Architecture & State Management",
            Topic = "Frontend & C# WebAssembly",
            IsPinned = false,
            Tags = "Blazor, WebAssembly, C#, State Pattern",
            CreatedAt = DateTime.Now.AddDays(-2),
            UpdatedAt = DateTime.Now.AddDays(-1),
            Content = @"### Blazor WASM Core Concepts

#### 1. Lifecycle Methods
- `OnInitializedAsync()`: Component initialization & initial data fetch.
- `OnParametersSetAsync()`: Called when component receives updated `[Parameter]`.
- `OnAfterRenderAsync(bool firstRender)`: Safe place for JavaScript Interop (`IJSRuntime`).

#### 2. Centralized State Container Pattern
- Register singleton service: `builder.Services.AddSingleton<StudyStateService>();`
- In components: inject service and subscribe to `State.OnChange += StateHasChanged;`
- Clean up subscriptions in `IDisposable.Dispose()` to avoid memory leaks!"
        }
    };
}

public class SubjectResourceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SubjectId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UrlOrReference { get; set; } = string.Empty;
    public string Type { get; set; } = "Cheatsheet"; // Link, Cheatsheet, Book, Slide
    public DateTime CreatedAt { get; set; } = DateTime.Now;
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
