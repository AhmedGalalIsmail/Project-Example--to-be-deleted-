---
aliases:
  - 📋 TASK BACKLOG
doc_id: 601
status: active
audience: AI coding agents + solo developer
---
# CoNSoL-TakeOff — Task Backlog

> **Single executable task list.**
> Every task here traces to a UC, FR, or GAP ID in the SDLC Library.
> Status: `TODO` | `IN-PROGRESS` | `DONE` | `BLOCKED`
> Update this file after each session. Do not create new task lists elsewhere.

---

## How to Pick a Task

1. Look at the 🔴 CRITICAL block — clear these before anything else
2. Within a block, pick the task at the top (dependencies flow downward)
3. If a task is BLOCKED, skip to the next unblocked task in the same layer
4. After completing a task, update its Status and update the UC % in `0001_MASTER_DASHBOARD.md`

---

## 🔴 CRITICAL — Unblock First

These tasks block entire feature chains. Nothing downstream can be completed until these are done.

| **ID** | **Task**                                                                                                                                   | **Layer** | **SDLC Ref**           | **Status** | **Effort** | **Unlocks**           |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------ | --------- | ---------------------- | ---------- | ---------- | --------------------- |
| T-001  | Implement `CoordinateConverter.ToLogical(px, scaleFactor)` and `.ToPhysical(logical, scaleFactor)`                                         | L01       | G-0301-08, FR-CV-001   | TODO       | 2h         | T-002, T-003, UC-001  |
| T-002  | Implement HitTest for Line (point-on-segment with tolerance)                                                                               | L01       | G-0301-06, FR-DT-001   | TODO       | 2h         | T-005, UC-001 full    |
| T-003  | Implement HitTest for Rectangle (point-in-rect)                                                                                            | L01       | G-0301-06, FR-DT-003   | TODO       | 1h         | UC-001 full           |
| T-004  | Implement HitTest for Circle (distance to center vs radius)                                                                                | L01       | G-0301-06, FR-DT-004   | TODO       | 1h         | UC-001 full           |
| T-005  | Document + implement Window vs Crossing selection logic                                                                                    | L01       | G-0301-07, UC-006      | TODO       | 2h         | UC-006                |
| T-006  | Create `Layer.vb` entity (matching ERD: layer_id, canvas_id, name, visible, locked, printable, color, line_style, line_weight, sort_order) | L03       | FND-013, ERD           | TODO       | 3h         | T-007, UC-002, UC-007 |
| T-007  | Create `LayerGroup.vb` entity (group_id, canvas_id, name, collapsed, sort_order)                                                           | L03       | FND-013, ERD           | BLOCKED    | 1h         | Needs T-006           |
| T-008  | Implement `CanvasLayoutValidator` (scale_factor > 0, unit in enum set)                                                                     | L03       | FND-003, G-0301-08     | TODO       | 2h         | Data integrity        |
| T-009  | Implement `CanvasElementValidator` (object_type in enum, layer_id exists)                                                                  | L03       | FND-007                | BLOCKED    | 2h         | Needs T-006           |
| T-010  | Implement `Calculator.Calculate(element, mode)` — D0 count                                                                                 | L02       | BUS-004, FR-DT-041     | TODO       | 1h         | UC-004                |
| T-011  | Implement `Calculator.Calculate` — D1 length from geometry                                                                                 | L02       | BUS-005, FR-DT-041     | TODO       | 1h         | UC-004                |
| T-012  | Implement `Calculator.Calculate` — D2 area (W × H, subtract nested children)                                                               | L02       | BUS-005, FR-DT-041     | TODO       | 2h         | UC-004                |
| T-013  | Implement `Calculator.Calculate` — D3 volume (H × W × L from Logical3D)                                                                    | L02       | BUS-006, FR-DT-041     | TODO       | 1h         | UC-004                |
| T-014  | Define `ICommand` interface (Execute, Undo, Redo, Description)                                                                             | L05       | G-0301-09, UC-012      | TODO       | 1h         | T-015, T-016, UC-012  |
| T-015  | Implement `CommandHistory` (bounded stack, N=50, LIFO for Undo)                                                                            | L05       | G-0301-09, UNDO_STACK  | BLOCKED    | 2h         | Needs T-014           |
| T-016  | Implement `AddShapeCommand` (Execute=add shape, Undo=remove shape)                                                                         | L05       | G-0301-09, UC-001 undo | BLOCKED    | 2h         | Needs T-015           |

---

## 🟠 HIGH — Do Before Feature Implementation

| **ID** | **Task**                                                                                      | **Layer** | **SDLC Ref**         | **Status** | **Effort** | **Unlocks**        |
| ------ | --------------------------------------------------------------------------------------------- | --------- | -------------------- | ---------- | ---------- | ------------------ |
| T-017  | Define `ILayerService` interface (GetAll, GetActive, Add, Delete, SetActive)                  | L05       | G-0302-01            | TODO       | 1h         | T-018, UC-002      |
| T-018  | Implement `LayerService` backed by Repository                                                 | L05       | G-0302-01            | BLOCKED    | 3h         | Needs T-006, T-017 |
| T-019  | Define `ITagService` interface (DefineTag, AttachTag, DetachTag, GetInstances)                | L05       | G-0302-01            | TODO       | 1h         | UC-003             |
| T-020  | Define `ITakeOffService` interface (GetSummary, ExportCsv)                                    | L05       | G-0302-01            | TODO       | 1h         | UC-004             |
| T-021  | Implement `TakeOffService.GetSummary(groupBy, filter)`                                        | L02       | BUS-008, UC-004      | BLOCKED    | 3h         | Needs T-010..013   |
| T-022  | Implement cost aggregation (quantity × unit_price per material)                               | L02       | BUS-009, UC-004      | BLOCKED    | 2h         | Needs T-021        |
| T-023  | Implement CSV export of take-off summary (UTF-8, comma-delimited, headers)                    | L02       | FR-DT-045, UC-014    | BLOCKED    | 2h         | Needs T-021        |
| T-024  | Create `LayerPanel` WinForms control (list, visibility toggle, lock toggle, active indicator) | L04       | FR-LP-001, UC-002    | BLOCKED    | 5h         | Needs T-006, T-018 |
| T-025  | Implement Layer Delete button in LayerPanel (UC-007 flow: prompt reassign or delete objects)  | L04       | FR-LP-003, UC-007    | BLOCKED    | 3h         | Needs T-024        |
| T-026  | Wire `PropertiesPanel` to `CanvasControl.SelectionChanged` event                              | L04       | FR-PP-001, UC-006    | TODO       | 2h         | UC-006             |
| T-027  | Property panel context sensitivity: None → canvas props                                       | L04       | FR-PP-001, 0208 §4.1 | BLOCKED    | 1h         | Needs T-026        |
| T-028  | Property panel context sensitivity: Single → full props for type                              | L04       | FR-PP-001            | BLOCKED    | 2h         | Needs T-026        |
| T-029  | Property panel context sensitivity: Multi-same-type → shared + (mixed)                        | L04       | FR-PP-004, UC-006    | BLOCKED    | 2h         | Needs T-026        |
| T-030  | Property panel context sensitivity: Multi-mixed-type → universal only                         | L04       | FR-PP-004, UC-006    | BLOCKED    | 2h         | Needs T-026        |
| T-031  | Logical 3D fields in property panel (H, W, L, Qty, UnitPrice, TotalCost auto)                 | L04       | FR-PP-008            | TODO       | 2h         | UC-004 display     |
| T-032  | Author `0401_Testing_Documentation.md` — test strategy section                                | L06       | G-0401-01            | TODO       | 2h         | All testing        |
| T-033  | Create Domain.Tests project, write 5 tests for `Calculator.Calculate` all modes               | L06       | G-0401-02            | BLOCKED    | 3h         | Needs T-010..013   |
| T-034  | Create Infrastructure.Tests project, write 5 tests for `CanvasLayoutValidator`                | L06       | G-0401-04            | BLOCKED    | 2h         | Needs T-008        |

---

## 🟡 MEDIUM — Implement During Feature Work

| **ID** | **Task**                                                                        | **Layer** | **SDLC Ref**         | **Status** | **Effort** |
| ------ | ------------------------------------------------------------------------------- | --------- | -------------------- | ---------- | ---------- |
| T-035  | Add mouse-wheel zoom binding to `CanvasControl`                                 | L01       | G-0209-03            | TODO       | 1h         |
| T-036  | Implement keyboard pan (arrow keys)                                             | L01       | UC-011               | TODO       | 1h         |
| T-037  | Add seed data: default Layer on new project                                     | L03       | G-020103-05          | BLOCKED    | 1h         |
| T-038  | Define DB index strategy and document in 020103                                 | L03       | G-020103-04          | TODO       | 1h         |
| T-039  | Implement `TagDefinition` and `TagInstance` entities (matching ERD)             | L02       | UC-003, ERD          | TODO       | 2h         |
| T-040  | Implement Smart Tag display modes (Hidden, Label, Badge) on canvas              | L01/L04   | FR-DT-040, 0208 §2.6 | BLOCKED    | 3h         |
| T-041  | Implement Undo shortcut: Ctrl+Z → `CommandHistory.Undo()`                       | L04       | UC-012               | BLOCKED    | 1h         |
| T-042  | Implement Redo shortcut: Ctrl+Y → `CommandHistory.Redo()`                       | L04       | UC-012               | BLOCKED    | 1h         |
| T-043  | Add status bar: cursor coordinates (logical units), active layer, zoom %, count | L04       | G-0208-10            | TODO       | 2h         |
| T-044  | Define keyboard shortcuts map and document in 0208 §Appendix                    | L04       | G-0208-06            | TODO       | 1h         |
| T-045  | Implement `DeleteShapeCommand` (Undo=restore shape)                             | L05       | G-0301-09, UC-001    | BLOCKED    | 1h         |
| T-046  | Implement `EditPropertyCommand` (Undo=restore previous value)                   | L05       | G-0301-09, UC-006    | BLOCKED    | 2h         |
| T-047  | Write acceptance test for UC-001 end-to-end                                     | L06       | G-0401-05            | BLOCKED    | 2h         |
| T-048  | Write acceptance test for UC-004 end-to-end                                     | L06       | G-0401-05            | BLOCKED    | 2h         |
| T-049  | Write acceptance test for UC-007 (delete layer)                                 | L06       | G-0401-05            | BLOCKED    | 1h         |
| T-050  | Resolve OQ-NEW-03: document installer format decision as ADR-003                | L07       | OQ-NEW-03, 0205      | TODO       | 1h         |
| T-051  | Write `build.ps1` (Restore → Build → Test → Publish, ASCII-safe)                | L07       | G-0304-01            | TODO       | 2h         |
| T-052  | Write deployment runbook in 0501 (standalone install, step-by-step)             | L07       | G-0501-01            | TODO       | 2h         |
| T-053  | Add strategic logging: `LineTool.OnMouseUp`, `Calculator.Calculate`             | L08       | 0606                 | TODO       | 2h         |
| T-054  | Add strategic logging: `TakeOffFileStore.Save`, `TakeOffFileStore.Load`         | L08       | 0606                 | TODO       | 1h         |
| T-055  | Add performance timing: wrap `OnPaint()` with stopwatch, warn if > 16ms         | L08       | NFR-001, 0606        | TODO       | 2h         |
| T-056  | Add XML doc comments to all Domain layer public classes                         | L05       | G-0301-11            | TODO       | 5h         |
| T-057  | Add XML doc comments to Application layer public classes                        | L05       | G-0301-11            | TODO       | 4h         |

---

## 🟢 LOW — Polish and Future

| **ID** | **Task**                                                   | **Layer** | **SDLC Ref**       | **Status** | **Effort** |
| ------ | ---------------------------------------------------------- | --------- | ------------------ | ---------- | ---------- |
| T-058  | Author ADR-001: WinForms vs WPF for v1                     | L05       | 0205               | TODO       | 1h         |
| T-059  | Author ADR-002: SQLite for standalone mode                 | L05       | 0205               | TODO       | 1h         |
| T-060  | Produce C4 Level 1 context diagram                         | L05       | G-020101-01        | TODO       | 2h         |
| T-061  | Populate RTM (0103) with first 10 FRs                      | L05       | G-0103-01          | TODO       | 2h         |
| T-062  | Produce C4 Level 2 container diagram                       | L05       | G-020102-02        | TODO       | 2h         |
| T-063  | Write user quick-start guide (non-technical)               | L07       | G-0604-01          | TODO       | 3h         |
| T-064  | Define performance test: 10k objects, measure redraw time  | L06       | G-0401-06, NFR-001 | TODO       | 2h         |
| T-065  | Implement `SymbolDefinition` and `SymbolInstance` entities | L02       | UC-005, ERD        | TODO       | 3h         |
| T-066  | Implement Symbol Library panel (UC-005)                    | L04       | UC-005, FR-DT-030  | TODO       | 8h         |

---

## 🧠 AI — Upcoming Tasks

| **ID** | **Category** | **Task**                                                       | **Layer** | **SDLC Ref**                                          | **Status** | **Effort** | **Depends On** | **Unlocks**                  |
| ------ | ------------ | -------------------------------------------------------------- | --------- | ----------------------------------------------------- | ---------- | ---------- | -------------- | ---------------------------- |
| T-067  | Data         | Define AI import session and source artifact entities          | L03       | UC-AI-001, FR-AI-001, FR-AI-002                       | TODO       | 4h         |                | T-071, UC-AI-001             |
| T-068  | AI           | Define OCR service contract and adapter boundary               | L05       | UC-AI-002, FR-AI-003                                  | TODO       | 3h         |                | T-072, UC-AI-002             |
| T-069  | AI           | Define geometry detection and candidate review contracts       | L05       | UC-AI-004, UC-AI-006, FR-AI-005..FR-AI-010            | TODO       | 5h         |                | T-070, T-071                 |
| T-070  | UI           | Add AI intake/review workflow to `ProductionMainForm`          | L04       | UC-AI-003, UC-AI-006, FR-AI-004, FR-AI-009, FR-AI-010 | TODO       | 8h         | T-026, T-043   | T-072, UC-AI-006             |
| T-071  | Data         | Persist OCR results, geometry candidates, and review decisions | L03       | UC-AI-001, UC-AI-002, UC-AI-004, UC-AI-006, FR-AI-012 | TODO       | 6h         | T-067, T-069   | UC-AI-001..UC-AI-006         |
| T-072  | Testing      | Add AI intake fixtures and acceptance tests                    | L06       | G-0401-05, G-0401-06, UC-AI-001..UC-AI-008            | TODO       | 6h         | T-070, T-071   | AI promotion gate            |
| T-073  | Ops          | Add AI logging and tracing points                              | L08       | UC-AI-001..UC-AI-008, 0606                            | TODO       | 3h         | T-070, T-071   | Auditability                 |
| T-074  | Build        | Decide OCR/CV packaging and fallback strategy                  | L07       | UC-AI-002, UC-AI-004, UC-AI-005, OQ-NEW-03            | TODO       | 3h         | T-068, T-069   | Standalone install readiness |
| T-075  | Business     | Wire accepted AI candidates into calculation/export flow       | L02       | UC-AI-006, UC-AI-007, UC-AI-008, FR-AI-011..FR-AI-013 | TODO       | 5h         | T-071          | AI take-off value            |
| T-076  | UI           | Draft main shell layout requirements from the main UI mockup   | L04       | FR-UI-024..FR-UI-029                                  | TODO       | 2h         | T-024, T-043   | Main shell UI spec           |
| T-077  | UI           | Draft layer panel and status bar requirements from mockup      | L04       | FR-LP-001..FR-LP-004, G-0208-10                       | TODO       | 2h         | T-024, T-043   | Layer/status polish          |
| T-078  | UI           | Draft Materials & Blocks CRUD form requirements                | L04       | FR-UI-030..FR-UI-035                                  | TODO       | 3h         | T-066          | CRUD form spec               |
| T-079  | UI           | Draft CRUD validation and save-state behavior                  | L04       | FR-UI-033..FR-UI-035                                  | TODO       | 2h         | T-078          | Editor workflow              |
| T-080  | Platform     | Draft application/user settings and configuration requirements | L04       | FR-CONF-010..FR-CONF-016                              | TODO       | 2h         | T-024, T-077   | Settings subsystem spec      |

## Summary Counts

| **Priority** | **Done** | **In Progress** | **Blocked** | **TODO** | **Total** |
| ------------ | -------- | --------------- | ----------- | -------- | --------- |
| 🔴 Critical  | 0        | 0               | 5           | 11       | 16        |
| 🟠 High      | 0        | 0               | 12          | 6        | 18        |
| 🟡 Medium    | 0        | 0               | 8           | 19       | 27        |
| 🟢 Low       | 0        | 0               | 0           | 9        | 9         |
| **Total**    | **0**    | **0**           | **25**      | **55**   | **80**    |

> Note: "Blocked" = dependency task not yet complete. As critical tasks complete, blocked tasks become available.
---
> Update Status column and summary counts after each session.
> Do not add tasks without assigning a UC, FR, or GAP ID.
