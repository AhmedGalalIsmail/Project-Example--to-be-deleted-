---
aliases:
  - 🚀 AGENT BRIEFING
color: "#f43607"
doc_id: 000
---
# CoNSoL-TakeOff — Agent Briefing

> **Read this first. Every time. It takes 3 minutes.**
> This replaces START_HERE, DOCUMENTATION_INDEX, CODING_ASSISTANCE_PLAN and DELIVERY_SUMMARY.

---

## What Is This Project?

**CoNSoL-TakeOff** is a visual-first construction take-off and estimation tool.
Users draw construction elements (walls, slabs, rooms, columns) on a 2D canvas.
Each drawn object carries business meaning, geometry, and calculable quantities.
The system auto-computes quantities, costs, and material breakdowns.

> Core principle: **Drawing is data input, not decoration.**

**Platform:** VB.NET, .NET 8.0, WinForms (v1) → Blazor (future)
**Architecture:** 4 layers — Domain / Application / Infrastructure / Desktop
**Deployment v1:** Standalone (SQLite, single-user, offline, Windows)

---

## Library Structure

```
CoNSoL-TakeOff Documentation Library
│
├── 0000_AGENT_BRIEFING.md          ← You are here
├── 0001_MASTER_DASHBOARD.md        ← Current status, UC progress, blocking gaps
│
├── 05_SDLC_Library\                ← Canonical requirements (read-only reference)
│   ├── 05_Mega-File.md             ← All SDLC phases 00-06: FRs, UCs, design, data model
│   ├── 0005_Gap_Analysis.md        ← 67 gaps, severity, ERD (25 entities), resolution order
│   └── ...individual docs...
│
├── 06_VIBE_CODING_GUIDE\           ← Execution layer (read before coding)
│   ├── 0_CoNSoL_Production_Layers.md  ← 8-layer guide: what exists, what's missing, what to build
│   └── 1_Task_Backlog.md           ← 66 tasks with UC/FR/GAP IDs, status, effort estimates
│
└── 99_Archive\                     ← Historical docs (do not update)
    └── ...
```

---

## Key Vocabulary

| Term           | Definition                                                                             |
| -------------- | -------------------------------------------------------------------------------------- |
| UC-NNN         | Use Case — full user flow with actor, preconditions, steps, exceptions                 |
| FR-XX-NNN      | Functional Requirement (DT=drawing tool, CV=canvas, PP=property panel, LP=layer panel) |
| NFR-NNN        | Non-Functional Requirement (NFR-001 = <16ms redraw)                                    |
| GAP / G-XXXX   | A missing or incomplete doc section (in Gap Analysis)                                  |
| D0/D1/D2/D3    | Dimension modes: D0=count, D1=length, D2=area, D3=volume                               |
| Logical units  | Real-world units (mm/cm/m) used for business data                                      |
| Physical units | Screen pixels used for rendering                                                       |
| ScaleFactor    | Conversion ratio: logical ÷ physical                                                   |
| Smart Tag      | Data aggregator attached to objects (carries numeric or text value)                    |
| Custom Mark    | Visual marker attached to objects (countable, no value)                                |
| Block/Symbol   | Reusable drawing element with attribute definitions                                    |
| Nested object  | Child drawing object that subtracts from or adds to its parent's quantity              |

---

## What Works Right Now (can demo)

- Draw lines, rectangles, circles, ellipses, polylines on canvas
- Zoom (0.1x–10x), pan, grid
- Select and highlight shapes (click)
- Save and load `.takeoff` files (with encryption)
- DI container initialized, deployment mode config partial (UC-008)

---

## What Does NOT Work Yet

- Layer management (no Layer entity in code yet)
- Take-off calculation (Calculator.Calculate() not implemented)
- Smart Tags, Custom Marks, Symbol library
- Undo/Redo
- Layer panel UI, Property panel not wired to selection
- Unit tests (none exist yet)

---

## How to Choose What to Code Next

```
Step 1: Open 0001_MASTER_DASHBOARD.md
        → Check "Blocking Gaps" table
        → Find the highest-priority gap that is NOT already being worked

Step 2: Open 06_VIBE_CODING_GUIDE/1_Task_Backlog.md
        → Find the task row matching that gap
        → Check its Status and Blocked-by column

Step 3: Open 06_VIBE_CODING_GUIDE/0_CoNSoL_Production_Layers.md
        → Read the layer section for that task
        → Read: "What Exists", "What Is Missing", "AI Agent Mission"

Step 4: Open 05_SDLC_Library/05_Mega-File.md
        → Look up the UC or FR mentioned in the task
        → Read the full requirement before writing any code

Step 5: Code. Then update:
        → Task status in 1_Task_Backlog.md
        → UC % in 0001_MASTER_DASHBOARD.md
```

---

## Coding Rules (non-negotiable)

1. **No task without an ID.** Every code change traces to a T-xxx in the backlog, which traces to a UC/FR/GAP.
2. **No new entities without updating the ERD.** The 25-entity ERD in `0005_Gap_Analysis.md` is authoritative. Propose changes before coding.
3. **Code is truth, docs follow code.** When code and docs diverge, update the doc.
4. **Layers must not cross.** Domain has zero imports from Application, Infrastructure, or Desktop. Enforce this.
5. **ASCII-safe PowerShell.** No Unicode box-drawing or arrow characters in `.ps1` scripts.
6. **One test per task minimum.** No task is DONE without at least one passing test.
7. **Update the dashboard.** After every session, update UC % and task statuses.

---

## Do NOT Do

- Create new status documents (everything goes into the dashboard)
- Add tasks to a new list (add to `1_Task_Backlog.md` only)
- Reference generic framework concepts not in the 8 layers (no Cloud, CDN, Rate Limiting for v1)
- Mark a UC as Done without a passing acceptance test for that UC
- Accept the Full Tech Stack 17-layer generic guide as applicable — it is archived

---

## Immediate Next Tasks (start here)

```
T-001  Implement CoordinateConverter (2h)  → G-0301-08, FR-CV-001
T-008  Implement CanvasLayoutValidator (2h) → FND-003
T-006  Create Layer entity (3h)            → FND-013, ERD
T-010  Calculator.Calculate() D0 (1h)      → BUS-004, FR-DT-041
T-014  Define ICommand interface (1h)      → G-0301-09, UC-012
```

Total to unblock everything: ~9 hours of focused work.

---
> **Remember:** The SDLC Library is the source of truth for *what* to build.
> The Vibe Coding Guide is the source of truth for *how* to build it and *in what order*.
