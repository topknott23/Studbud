# StudyFlow — Study Planner & Pomodoro Dashboard

A modern, clean web application built with **Blazor WebAssembly (.NET 10)**, **C#**, and **Pure CSS (Grid & Flexbox)**. Designed with zero CSS frameworks, sharp typography, minimalist SVG iconography, and sleek dark/light mode glassmorphic UI.

---

## 📚 Enrolled Courses / Subject Tags

- **Automata Theory** (`#6366F1`)
- **Application Development** (`#3B82F6`)
- **Intelligent Systems** (`#10B981`)
- **Technopreneurship** (`#F59E0B`)
- **Quantitative Methods** (`#8B5CF6`)
- **Information Management 2** (`#06B6D4`)

---

## Key Features

### 1. Interactive Pomodoro Timer
- **Interval Modes**: 25-minute study intervals, 5-minute short breaks, and 15-minute long breaks.
- **Precision Controls**: Real-time Start, Pause, Resume, Reset, and Skip cycle buttons.
- **Circular Animated SVG Progress**: Dynamic countdown ring showing remaining time ratio.
- **C# Architecture**: Driven by `System.Timers.Timer` with asynchronous UI state synchronization.
- **Audio Notifications**: Built-in sound notifications synthesized via the Web Audio API.
- **Browser Tab Sync**: Real-time timer countdown reflected on the browser page title.

### 2. Subject & Task Checklist
- **Quick-Entry Input**: Rapid task addition with Enter-key shortcuts.
- **Subject Tagging & Badges**: Color-coded badges for all 6 university subjects.
- **Cycle Estimator**: Set and track planned vs. completed pomodoro cycles per task.
- **Interactive Checkboxes**: Complete tasks with smooth strikethrough animations and dynamic count updates.
- **Filter Pills**: Filter tasks by *All*, *Active*, *Completed*, or specific subject tags.
- **Active Task Focus Binding**: Bind the current active timer session to a specific task.

### 3. Session Log & Daily Summary
- **Cycle Counter**: Minimalist cycle tracking (e.g., `3 / 8 Cycles`).
- **Stat Cards**: Real-time tracking of *Focus Cycles*, *Total Focus Time*, *Tasks Completed*, and *Productivity Rate*.
- **Daily Target Progress Bar**: Visual progress percentage towards your daily Pomodoro goal.
- **Session History Log**: Audit trail detailing completed session timestamps, subject tags, task names, and duration.
- **Persistence**: Automatic client-side `localStorage` caching for tasks, history, and user preferences.

---

## Pure CSS Architecture

- **Zero Frameworks**: 100% vanilla CSS using standard **CSS Grid** (12-column responsive layout) and **CSS Flexbox**.
- **Theming**: Seamless Dark Mode / Light Mode toggle with CSS custom properties.
- **Typography**: `Plus Jakarta Sans` for clean UI text and `JetBrains Mono` for timer digits.
- **Glassmorphism**: Backdrop blur filters, subtle borders, and smooth micro-transitions.

---

## Getting Started Locally

### Prerequisites
- [.NET 8, 9, or 10 SDK](https://dotnet.microsoft.com/download)

### Run the App
```bash
# Clone the repository
git clone <your-repo-url>
cd APPDEV

# Run the development server
dotnet run
```

Then open `http://localhost:5000` (or the URL displayed in your terminal) in your browser.

---

## Project Structure

```text
├── Components/
│   ├── DailySummary.razor     # 4 stat cards & daily goal progress
│   ├── SessionHistory.razor   # Session audit log & history list
│   ├── SettingsModal.razor    # Timer interval & sound settings modal
│   ├── TaskList.razor         # Task quick-add, filters & checkboxes
│   └── TimerCard.razor        # SVG circular timer & countdown controls
├── Layout/
│   ├── MainLayout.razor       # Top navbar, theme toggle & footer
├── Models/
│   └── StudyModels.cs         # Data structures (TaskItem, SessionRecord, SubjectDef)
├── Pages/
│   └── Home.razor             # Main Dashboard view
├── Services/
│   └── StudyStateService.cs   # State management, C# timer & localStorage sync
├── wwwroot/
│   ├── css/app.css            # Pure CSS Grid & Flexbox stylesheet
│   ├── js/study-helper.js     # Audio synth & theme helpers
│   └── index.html             # HTML5 entrypoint & typography
└── Program.cs                 # Blazor WebAssembly bootstrapping & DI
```

---

## Figma Design Specifications

A full UI design blueprint and layout structure is documented in [`FIGMA_DESIGN_GUIDE.md`](./FIGMA_DESIGN_GUIDE.md).
