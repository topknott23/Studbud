# Studbud

A study planner and Pomodoro focus application built with Blazor WebAssembly (.NET 10) and Tailwind CSS.

## Courses

- Automata Theory
- Application Development
- Intelligent Systems
- Applied AI
- Technopreneurship
- Quantitative Methods
- Information Management 2

## Features

- **Pomodoro Timer**: Work and break interval countdown with audio chime alerts.
- **Task Planner**: Kanban board and list views with priority levels and course tags.
- **Analytics**: Focus time tracking, course study distribution, and session history logs.
- **Flashcards**: Active recall study module (to be connected with database backend).
- **Settings**: Focus duration presets (Classic, Deep Work, Sprint) and theme preferences.

## Tech Stack

- **Framework**: Blazor WebAssembly (.NET 10)
- **Styling**: Tailwind CSS
- **State Management**: C# Singleton Service with LocalStorage persistence

## Getting Started

### Prerequisites
- .NET 10 SDK

### Development
```bash
# Clone the repository
git clone https://github.com/topknott23/Studbud.git
cd Studbud

# Run development server
dotnet run
```

Open `http://localhost:5102` in your browser.

## Project Structure

```text
├── Components/          # Reusable UI components (Timer, Tasks, Summary, History)
├── Layout/              # Main layout and navigation
├── Models/              # Data models (TaskItem, SessionRecord, SubjectDef)
├── Pages/               # Application routes (Home, Planner, Analytics, Flashcards, Settings)
├── Services/            # State management service and timer engine
└── wwwroot/             # Static assets, Tailwind configuration, and entry HTML
```
