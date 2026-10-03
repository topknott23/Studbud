# Figma Design Guide & Blueprint — Studbud Dashboard

This guide provides the exact design specifications, frame layouts, typography scales, color palettes, and component hierarchy to design the **Studbud** (Study Planner & Pomodoro Dashboard) in Figma with a clean, minimal aesthetic.

---

## 1. Canvas & Frame Setup

### Primary Desktop Frame (1440px)
- **Frame Preset**: Desktop / MacBook Pro 16" (`1440 × 1024 px`)
- **Background Fill**: `#0B0F19` (Dark Theme) or `#F3F4F6` (Light Theme)
- **Grid / Layout Grid**:
  - **Type**: Columns
  - **Count**: 12 Columns
  - **Margin**: `80px`
  - **Gutter**: `24px`

---

## 2. Color Styles & Course Palette

| Course / Subject | Hex Code | Usage |
| :--- | :--- | :--- |
| **Automata Theory** | `#6366F1` (Indigo) | Course Tag / Badge |
| **Application Development** | `#3B82F6` (Blue) | Course Tag / Badge |
| **Intelligent Systems** | `#10B981` (Emerald) | Course Tag / Badge |
| **Technopreneurship** | `#F59E0B` (Amber) | Course Tag / Badge |
| **Quantitative Methods** | `#8B5CF6` (Purple) | Course Tag / Badge |
| **Information Management 2** | `#06B6D4` (Cyan) | Course Tag / Badge |

### System UI Colors

| Token Name | Hex Code | Usage |
| :--- | :--- | :--- |
| **Background / Canvas** | `#0B0F19` | Main screen background |
| **Card / Surface (Glass)** | `#121826` (85% Opacity) | Panels, Timer Card, Task List |
| **Border / Subtle** | `rgba(255, 255, 255, 0.08)` | Card borders, dividers |
| **Primary Accent** | `#6366F1` | Primary action buttons |
| **Focus Mode (Work)** | `#EF4444` | Active countdown arc & focus states |
| **Short Break** | `#10B981` | Break mode & completed states |
| **Long Break** | `#3B82F6` | Long break mode |
| **Text Primary** | `#F9FAFB` | Headings, timer countdown numbers |
| **Text Muted** | `#9CA3AF` | Subtitles, labels, descriptions |
| **Text Subtle** | `#6B7280` | Placeholders, timestamps, borders |

---

## 3. Typography Styles

| Style Name | Font Family | Size / Weight | Line Height |
| :--- | :--- | :--- | :--- |
| **Timer Display** | `JetBrains Mono` | 56px Bold (700) | 56px |
| **Heading 1 (Brand)** | `Plus Jakarta Sans` | 22px ExtraBold (800) | 28px |
| **Heading 2 (Panel Title)**| `Plus Jakarta Sans` | 18px Bold (700) | 24px |
| **Stat Numbers** | `Plus Jakarta Sans` | 24px ExtraBold (800) | 30px |
| **Body / Task Title** | `Plus Jakarta Sans` | 14px SemiBold (600) | 20px |
| **Caption / Badges** | `Plus Jakarta Sans` | 12px Bold (700) | 16px |

---

## 4. Component Breakdown for Figma

### Component 1: Top Navigation Bar
- **Dimensions**: `1280 × 60 px` (Auto Layout: Horizontal, Space Between)
- **Left Group**:
  - Logo Icon (`44 × 44 px`, Corner Radius `14px`, Fill: Linear Gradient `#6366F1` to `#8B5CF6`, clean minimal vector icon)
  - Text Stack: "StudyFlow" (22px 800) + "Study Planner & Pomodoro Dashboard" (13px 500 `#9CA3AF`)
- **Right Group**:
  - Light/Dark Mode Button (`Corner Radius: 8px`, Stroke: 1px `rgba(255,255,255,0.08)`)
  - Settings Button (`Corner Radius: 8px`, Stroke: 1px `rgba(255,255,255,0.08)`)

---

### Component 2: Daily Summary Stat Cards (4 Cards)
- **Container**: Auto Layout Horizontal, `4 Columns`, Gap `20px`
- **Card Specs**:
  - Corner Radius `14px`, Fill `#121826`, Stroke 1px, Padding `20px`
  - **Card 1 (Focus Cycles)**: Minimal Clock Icon, "3 / 8", "38% of daily target"
  - **Card 2 (Total Focus Time)**: Monitor Icon, "1h 15m", "3 logged sessions"
  - **Card 3 (Tasks Completed)**: Check Icon, "1 / 6", "5 pending tasks"
  - **Card 4 (Productivity Rate)**: Lightning Icon, "47%", "Active session efficiency"

---

### Component 3: Pomodoro Timer Card (Left Column)
- **Frame**: Auto Layout Vertical, Padding `28px`, Corner Radius `20px`
- **Mode Pill Switcher**:
  - 3 Tabs: `Pomodoro (25m)` (Active Red Fill), `Short Break (5m)`, `Long Break (15m)`
- **Timer Circular Ring**:
  - Outer Circle (`280 × 280 px`, Stroke Width `10px`, Track: `#1E293B`, Progress Arc: `#EF4444`)
  - Center Content: `25:00` (56px Mono font) + Active Task Tag (`Configure Blazor State Management`)
- **Control Buttons**:
  - Reset Button (Circle `44 × 44 px`, Vector reload icon)
  - Start / Pause Main Action (`180 × 48 px`, Pill Radius `999px`, Solid/Gradient Fill, Text: "START FOCUS")
  - Skip Button (Circle `44 × 44 px`, Vector next icon)
- **Cycle Indicators**: 8 small minimalist round dots (3 filled red, 5 empty gray)

---

### Component 4: Subject & Task Checklist (Right Column)
- **Header**: Title "Tasks & Subject Checklist" + "1 / 6 completed"
- **Quick-Entry Input Bar**:
  - Text input field placeholder: "Enter task description..."
  - Subject Dropdown Selector (`Automata Theory`, `Application Development`, `Intelligent Systems`, `Technopreneurship`, `Quantitative Methods`, `Information Management 2`)
  - Estimated Pomodoros Dropdown (`1 Cycle (25m)`)
  - Add Task Button (`+ Add Task`, Fill `#6366F1`)
- **Filter Tabs**: Pills for `All`, `Active`, `Completed`, and each course tag
- **Task Item Row (Repeated Frame)**:
  - Custom Checkbox (`22 × 22 px`, Rounded `6px`)
  - Task Title & Subject Badge pill (`Automata Theory` in Indigo tint)
  - Pomodoro Progress Badge (`2 / 3 cycles`)
  - Action buttons: Focus Button (`Focus`) + Delete Icon

---

### Component 5: Session History Log (Below Timer)
- **Header**: "Session Log & Activity" + "Clear Log"
- **Daily Progress Bar**: Height `8px`, Fill gradient, showing percentage width
- **List Items**:
  - Row 1: `Configure Blazor State Management` • `Application Development` • `10:30 AM` • `+25 min`
  - Row 2: `Train Neural Network Classification Model` • `Intelligent Systems` • `11:15 AM` • `+25 min`
  - Row 3: `Hydration & Rest` • `Break` • `11:45 AM` • `+5 min`

---

## 5. Submission Checklist

1. Open [Figma](https://figma.com) and create a **New Design File**.
2. Name your file: `StudyFlow - Study Planner & Pomodoro Dashboard`.
3. Create the desktop artboard frame (`1440 × 1024 px`) and assemble the 5 components using the specs above.
4. Click **Share** (top-right) -> set to **"Anyone with the link can view"** -> click **"Copy link"**.
5. Submit your **Figma Link** alongside your **GitHub Repository URL**.
