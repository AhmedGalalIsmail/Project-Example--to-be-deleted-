---
aliases:
  - 🧱 CoNSoL Production Layers
doc_id: 600
status: active
audience: AI coding agents + solo developer
---
# CoNSoL-TakeOff — Production Layers Guide

> **Purpose:** This is the vibe-coding entry point.
> An AI agent dropped into this project reads this doc first, then picks a task from `1_Task_Backlog.md`.
> All layer definitions are specific to CoNSoL-TakeOff — no generic infrastructure that doesn't apply.

---

## How to Use This Document

1. **Orient:** Read the layer map below (2 min)
2. **Pick a layer** matching the current work area
3. **Read the layer section** — it tells you purpose, what already exists, what is missing, and what to produce
4. **Cross-reference** the SDLC IDs listed — look them up in `05_Mega-File.md` if you need full detail
5. **Pick a task** from `1_Task_Backlog.md` and execute

---

## Layer Map

```
CoNSoL-TakeOff Production Layers
│
├── L01  Canvas & Drawing Engine          ← Core visual surface
├── L02  Business Logic & Calculation     ← Quantities, costs, formulas
├── L03  Data Model & Persistence         ← Entities, DB, JSON, files
├── L04  UI/UX & Presentation             ← Panels, tools, interaction
├── L05  Architecture & Code Quality      ← Patterns, DI, standards
├── L06  Testing & Verification           ← Unit tests, acceptance, perf
├── L07  Build, Package & Deployment      ← Installer, CI, release
└── L08  Observability & Logging          ← Logging, debugging, tracing
```

**Not included (not applicable to standalone WinForms v1):**
Cloud infrastructure, CDN, rate limiting, horizontal scaling, distributed caching.
These reappear if/when the Integrated or cloud-hosted mode becomes a target.

---
# L01 — Canvas & Drawing Engine

## Purpose

Implement the 2D visual canvas that accepts user input, renders geometry, manages coordinate systems, and handles all drawing tool interactions.

## SDLC References

`FR-DT-001..FR-DT-052`, `FR-CV-001..FR-CV-007`, `0209 Canvas Engine Spec`, `UC-001`, `UC-010`, `UC-011`

## What Exists

- Line, Rectangle, Circle, Ellipse, Polyline tools — working
- Zoom (0.1x–10x), pan — working
- Grid rendering, double-buffering — working
- Tool state machine (SelectTool, LineTool, RectangleTool, CircleTool, PanTool) — working
- Shape selection with visual highlight — working

## What Is Missing

| **Gap**        | **Description**                                                              |
| -------------- | ---------------------------------------------------------------------------- |
| G-0301-06      | HitTest algorithm per shape type not documented or implemented               |
| G-0301-07      | Window vs crossing selection logic not documented                            |
| G-0301-08      | Physical↔logical coordinate conversion not implemented (only formula exists) |
| G-0209-02      | Canvas Engine Spec not linked to formal FRs in SRS                           |
| G-0209-03      | Mouse-wheel zoom trigger not specified                                       |
| FR-CV-010..030 | Canvas FRs missing from SRS (gap G-0104-02)                                  |

## AI Agent Mission

Implement missing canvas capabilities following this priority:

1. Coordinate conversion (physical px ↔ logical units using ScaleFactor)
2. HitTest per shape type (point-in-rect, point-on-line with tolerance, point-in-circle)
3. Window vs crossing selection (L→R = window/fully-inside; R→L = crossing/intersects)
4. Mouse-wheel zoom binding

## Required Deliverables

- `CoordinateConverter` service (physical ↔ logical, tested)
- HitTest implementation per shape type in the Shape base class hierarchy
- Selection mode logic documented and coded
- All new FRs registered in SRS §5.2 (FR-CV-010 onwards)

## Validation Rules

- Coordinate round-trip test: convert point to logical, convert back, delta < 0.001 units
- HitTest: clicking 1px outside a shape boundary returns false
- Window select: shape partially outside window = not selected
- Crossing select: shape partially inside crossing area = selected
- Zoom: logical coordinates unchanged after zoom in/out cycle

## Anti-Patterns

- Mixing logical and physical coordinates in the same variable
- HitTest that only checks bounding box (fails for Line, Circle, etc.)
- Redrawing the entire canvas for a single object change (use dirty-region invalidation)

---

# L02 — Business Logic & Calculation

## Purpose

Implement the take-off calculation engine: quantity extraction from geometry, formula application, cost rollup, and aggregated output.

## SDLC References

`FR-DT-040..FR-DT-052`, `UC-003`, `UC-004`, `UC-014`, `020103 §3 Dimension Model`, `0201 §6 Calculation Architecture`

## What Exists

- `TakeOffCalculator` class — skeleton only, `Calculate()` not implemented
- `TakeOffService`, `MaterialService` — skeleton only
- `BusinessDefinition` entity — material, quantity, pricing fields exist
- Dimension mode enum (D0/D1/D2/D3) — defined

## What Is Missing

| **Item**    | **Description**                                                                |
| ----------- | ------------------------------------------------------------------------------ |
| BUS-004     | `Calculator.Calculate()` — core dispatch by dimension mode                     |
| BUS-005/006 | Dimension extraction: D1 from line length, D2 from width×height, D3 from H×W×L |
| BUS-007     | Formula resolution: apply `FormulaCode` expression to calculated quantity      |
| BUS-008     | Cost aggregation: quantity × unit_price per material                           |
| BUS-009     | Nested object subtraction: child object area/volume subtracted from parent     |
| UC-003      | Smart Tag engine (define tag, attach to object, set value, aggregate)          |
| UC-004      | Take-off summary: group-by layer/type/tag, export CSV/XLSX                     |

## AI Agent Mission

Implement in this order:

1. `Calculator.Calculate(shape, dimensionMode)` — switch on D0/D1/D2/D3
2. Nested object traversal — subtract child quantities from parent
3. Formula expression evaluator — resolve `FormulaCode` against calculated value
4. `TakeOffService.GetSummary()` — aggregate by group, return table rows
5. Export to CSV (UC-014) — tab-delimited, header row, one row per group

## Required Deliverables

- `Calculator.Calculate()` passing unit tests for all 4 dimension modes
- Nested object subtraction tested with door-in-wall example
- `TakeOffService.GetSummary()` returning correct aggregated table
- CSV export producing valid file
- All implemented items traced to FRs in the RTM

## Validation Rules

- D0 result = object count (integer, ≥ 1)
- D1 result = line length in logical units (matches geometry)
- D2 result = width × height, door subtracted if nested
- D3 result = H × W × L from Logical3D properties
- Cost = quantity × unit_price, rounded to 2 decimal places
- Export file: UTF-8, comma-delimited, first row = headers

## Anti-Patterns

- Hardcoding formula expressions in Calculate() — must resolve from FormulaCode lookup
- Calculating cost inside the drawing engine (belongs in Application layer only)
- Mutating the DrawingObject during calculation (calculation is read-only)

---

# L03 — Data Model & Persistence

## Purpose

Define and implement all data structures, database schema, serialization format, and file persistence for the CoNSoL-TakeOff drawing and business data.

## SDLC References

`020103 Data Model`, `0005_Gap_Analysis.md §020103`, `UC-013`, `UC-008`

## What Exists

- 25-entity ERD (Mermaid) — complete and authoritative (see `0005_Gap_Analysis.md`)
- `CanvasElement`, `CanvasLayout`, `BusinessDefinition`, `BlockModels`, `ElementRelationship` — Domain entities exist
- `.takeoff` file format (JSON serialization via Newtonsoft.Json) — working
- File encryption/decryption — working
- SQLite adapter (standalone mode) — partial

## What Is Missing

| **Gap**     | **Description**                                                   |
| ----------- | ----------------------------------------------------------------- |
| G-020103-02 | No formal JSON schema (draft-07 or OpenAPI) per entity            |
| G-020103-03 | No DB migration strategy (schema versioning)                      |
| G-020103-04 | No DB index strategy (FK columns, frequent queries)               |
| G-020103-05 | No seed data (default layers, default tag defs, built-in symbols) |
| FND-003     | CanvasLayout validation module not implemented                    |
| FND-007     | CanvasElement validation module not implemented                   |
| FND-013     | Layer entity not created in code                                  |
| FND-014     | Layer management logic not implemented                            |

## AI Agent Mission

1. Create `Layer` entity matching ERD (layer_id, canvas_id, name, visible, locked, printable, color, line_style, line_weight, sort_order)
2. Create `LayerGroup` entity
3. Implement `CanvasLayoutValidator` and `CanvasElementValidator`
4. Define DB indexes: all FK columns + `object_type`, `layer_id` on DRAWING_OBJECT
5. Define seed data set: 1 default layer ("Layer 1"), default config values
6. Define migration approach (use a version field in the `.takeoff` JSON header)

## Required Deliverables

- `Layer.vb` and `LayerGroup.vb` entities, matching ERD exactly
- Validation modules with unit tests
- Index definitions documented in `020103`
- Seed data list documented in `020103 §5 Appendix`

## Validation Rules

- Layer entity must have exactly the fields in the ERD — no additions without updating the ERD first
- CanvasLayout validation: scale_factor > 0, unit in valid enum set
- CanvasElement validation: object_type in valid enum, layer_id must reference existing layer
- Seed data test: new project always has at least 1 layer named "Layer 1"

## Anti-Patterns

- Adding fields to Domain entities that aren't in the ERD without updating 020103
- Bypassing the Repository interface with direct DB calls
- Storing geometry in the same JSON blob as business data (they are separate by design)

---

# L04 — UI/UX & Presentation

## Purpose

Implement the WinForms presentation layer: canvas control, property panel, layer panel, toolbars, menus, and dialogs — following the UX spec in `0208`.

## SDLC References

`0208 UX & UI Design`, `UC-001..UC-008`, `FR-PP-001..FR-PP-008`, `FR-LP-001..FR-LP-004`, `FR-UI-020..FR-UI-023`

## What Exists

- `MainForm` — main window, skeleton
- `ProductionMainForm` — production shell, working
- `MainWPFform` — WPF shell clone, wired to the same workspace actions
- `CanvasControl` — interactive drawing surface (working)
- `PropertiesPanel` — exists but not wired to selection state
- `BlockAssignmentForm` — block/material dialog (exists)
- `MaterialCrudForm` — material management (exists)
- `MaterialCrudFormWPF` — WPF CRUD clone, wired to the same catalog actions
- Tool selection UI — working

## What Is Missing

| **Item**                                    | **SDLC Ref**              | **Effort** |
| ------------------------------------------- | ------------------------- | ---------- |
| Layer panel (`LayerPanel`)                  | UC-002, UC-007, FR-LP-001 | 8h         |
| Property panel wired to selection           | UC-006, FR-PP-001         | 3h         |
| Context sensitivity: None/Single/Multi/Tool | FR-PP-001, 0208 §4.1      | 2h         |
| `(mixed)` placeholder for differing values  | FR-PP-004                 | 1h         |
| Logical 3D fields in property panel         | FR-PP-008                 | 2h         |
| Main shell responsive layout                | FR-UI-024..FR-UI-029      | 5h         |
| Main shell button behaviors + 0.0 axes     | FR-UI-024..FR-UI-029, FR-UI-036 | 4h |
| Materials & Blocks CRUD form                | FR-UI-030..FR-UI-035      | 8h         |
| Toolbox layout + grouping                   | G-0208-08                 | 2h         |
| Status bar                                  | G-0208-10                 | 2h         |
| Main menu items (File, Edit, View, Tools)   | IGN-012/013               | 4h         |
| Keyboard shortcuts map                      | G-0208-06                 | 2h         |

## AI Agent Mission

Implement in this order:

1. `LayerPanel` control: list of layers, visibility toggle, lock toggle, active layer indicator, Delete button triggering UC-007 flow
2. Wire `PropertiesPanel` to `CanvasControl.SelectionChanged` event
3. Context-sensitivity: panel reads selection type and shows correct fields
4. `(mixed)` — when multi-selection has differing values, show placeholder string
5. Status bar: cursor coordinates (logical), active layer name, zoom %, object count
6. Main shell: keep fixed chrome around a fluid canvas and right inspector
7. Materials & Blocks CRUD form: keep tree, editor, composition rows, and save-state aligned

## Required Deliverables

- `LayerPanel.vb` implementing FR-LP-001 through FR-LP-004
- `PropertiesPanel.vb` context-sensitivity for all 5 states (0208 §4.1 table)
- Status bar showing cursor position in logical units
- All menu items wired or stubbed (no dead menus)
- Responsive shell layout matching the main UI mockup
- Searchable materials/blocks CRUD form matching the CRUD mockup

## Validation Rules

- Layer panel: deleting the active layer shows error, does not delete
- Layer panel: deleting last layer shows error, does not delete
- Property panel: selecting 0 objects shows canvas properties
- Property panel: selecting 1 object shows all properties for that type
- Property panel: selecting mixed types shows universal properties only
- `(mixed)` field: editing it applies the new value to all selected objects

## Anti-Patterns

- Business logic inside Form/Control event handlers (delegate to Application layer)
- Direct DB calls from UI (all persistence goes through Repository)
- Thread-unsafe UI updates (use `Control.Invoke()` for async operations)

---

# L05 — Architecture & Code Quality

## Purpose

Maintain the layered architecture, dependency injection, coding standards, and design patterns that keep the codebase maintainable as it grows.

## SDLC References

`0201 Design Documentation`, `0301 Development Documentation`, `0205 ADRs`

## What Exists

- 4-layer architecture (Domain / Application / Infrastructure / Desktop) — implemented
- DI container (`Microsoft.Extensions.DependencyInjection`) — working
- Layering enforced (no direct DB calls from UI)
- JSON serialization (`Newtonsoft.Json`) — working
- `ILogger` interface — defined (not widely used yet)

## What Is Missing

| **Gap**   | **Description**                                                                |
| --------- | ------------------------------------------------------------------------------ |
| G-0301-09 | Command pattern (ICommand with Execute/Undo/Redo) not documented               |
| G-0302-01 | IDrawingEngine, ILayerService, ITagService, ITakeOffService not defined        |
| G-0301-10 | Error handling strategy: canvas errors vs data errors vs UI errors not defined |
| G-0205-01 | No ADRs authored yet                                                           |
| G-0301-11 | No unit test coverage targets defined per layer                                |

## AI Agent Mission

1. Define `ICommand` interface with `Execute()`, `Undo()`, `Redo()`, `Description As String`
2. Implement `CommandHistory` (stack-based, bounded to N entries per `UNDO_STACK` ERD design)
3. Define service interfaces: `IDrawingEngine`, `ILayerService`, `ITagService`, `ITakeOffService`
4. Register all services in `CompositionRoot.vb`
5. Author ADR-001: choice of WinForms over WPF for v1
6. Author ADR-002: SQLite for standalone mode

## Required Deliverables

- `ICommand.vb` interface + `CommandHistory.vb`
- `AddShapeCommand.vb` as the first concrete implementation (covers UC-001 undo)
- All 4 service interfaces defined (even if not fully implemented)
- ADR-001 and ADR-002 in `05_SDLC_Library/02_Design/0205_ADR.md`

## Validation Rules

- Execute → Undo → state equals pre-Execute state (tested)
- Execute → Undo → Redo → state equals post-Execute state (tested)
- CommandHistory bounded: adding beyond limit drops oldest entry
- No circular dependencies between layers (Domain has zero references to Application, Infrastructure, Desktop)

## Anti-Patterns

- God classes (classes with >300 lines or >10 public methods)
- Singleton state shared across layers instead of DI
- Catching and swallowing exceptions without logging

---

# L06 — Testing & Verification

## Purpose

Build and maintain test coverage to ensure correctness of the calculation engine, domain logic, and use case flows.

## SDLC References

`0401 Testing Documentation`, `G-0401-01 through G-0401-07`

## What Exists

- NUnit referenced as planned testing framework
- No test projects exist yet

## What Is Missing

| **Gap**   | **Description**                                  | **Severity** |
| --------- | ------------------------------------------------ | ------------ |
| G-0401-01 | Test strategy not authored                       | 🔴           |
| G-0401-02 | No test cases for FR-DT-xxx                      | 🔴           |
| G-0401-03 | No test cases for FR-CV-xxx                      | 🔴           |
| G-0401-04 | No negative test cases for validation rules      | 🟠           |
| G-0401-05 | No acceptance tests per UC                       | 🟠           |
| G-0401-06 | No performance test plan (NFR-001: <16ms redraw) | 🟡           |

## AI Agent Mission

1. Create 4 test projects: `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`, `Desktop.Tests`
2. Write unit tests for `Calculator.Calculate()` — all 4 dimension modes + nested subtraction
3. Write unit tests for `CoordinateConverter` — round-trip precision
4. Write unit tests for `CanvasLayoutValidator` and `CanvasElementValidator`
5. Write acceptance test for UC-001: simulate mouse events, assert Line object in drawing state
6. Write acceptance test for UC-004: load fixture with 3 objects, run take-off, assert output

## Coverage Targets (from G-0301-11 resolution)

| **Layer**      | **Target** |
| -------------- | ---------- |
| Domain         | 80%        |
| Application    | 70%        |
| Infrastructure | 60%        |
| Desktop (UI)   | 40%        |

## Validation Rules

- All tests pass on `dotnet test` before any PR is merged
- No test may touch the real file system (use temp folders or mocks)
- Test method names: `MethodName_Scenario_ExpectedResult`
- Each UC must have at least 1 acceptance test before that UC is marked Done

## Anti-Patterns

- Tests that only test the happy path (write at least 1 negative case per method)
- Tests that depend on each other's execution order
- Asserting exact floating-point equality (use tolerance: `Math.Abs(actual - expected) < 0.001`)

---

# L07 — Build, Package & Deployment

## Purpose

Define the build pipeline, packaging format, and installer strategy for standalone Windows deployment.

## SDLC References

`0304 DevSecOps & CI/CD`, `0501 Deployment Documentation`, `OQ-NEW-03 (installer format)`

## What Exists

- Project builds with `dotnet build` (no errors)
- `dotnet publish -c Release -r win-x64 --self-contained` produces executable

## What Is Missing

| **Gap**   | **Description**                                        |
| --------- | ------------------------------------------------------ |
| G-0304-01 | CI/CD pipeline not defined                             |
| G-0501-01 | Deployment runbook not authored                        |
| OQ-NEW-03 | Installer format undecided (.msi vs MSIX vs ClickOnce) |
| G-0305-01 | Environment strategy (Dev/QA/Prod) not defined         |

## AI Agent Mission

1. Resolve OQ-NEW-03: recommend installer format with rationale (ADR candidate)
2. Define build pipeline stages: Restore → Build → Test → Publish → Package
3. Write PowerShell build script (`build.ps1`) — ASCII-safe, no Unicode box chars
4. Write deployment runbook: install steps for end user (non-technical)
5. Define 3 environments: Dev (local), QA (test machine), Prod (client install)

## Required Deliverables

- `build.ps1` script that runs Restore → Build → Test → Publish
- `0501_Deployment_Documentation.md` with step-by-step installer instructions
- Installer format decision recorded as ADR-003
- Environment matrix table in `0305_Environment_Strategy.md`

## Validation Rules

- `build.ps1` exits 0 on success, non-zero on any failure
- Build script is idempotent (run twice = same result)
- Installer tested on a clean Windows machine with no .NET pre-installed
- PowerShell script: ASCII-only, no Unicode arrows or box-drawing characters

---

# L08 — Observability & Logging

## Purpose

Instrument the application with strategic logging to enable debugging, performance monitoring, and issue tracing.

## SDLC References

`0606 Observability`, `0301 §13 Logging & Debugging`

## What Exists

- `ILogger` interface — defined
- `FileLogger` — implementation exists
- Logging present: ~5% coverage (minimal)

## What Is Missing

- Strategic logging points across all 4 layers
- Performance timing around redraw cycle (NFR-001: <16ms)
- Error boundary logging (exceptions caught and logged with context)
- Log format standardization (timestamp, level, layer, message)

## AI Agent Mission

1. Define log format: `[YYYY-MM-DD HH:mm:ss.fff] [LEVEL] [LAYER] [CLASS.Method] message`
2. Add `Logger.LogInfo()` at: tool activation, shape commit, layer operation, file save/load, calculator run
3. Add `Logger.LogDebug()` at: coordinate conversion, HitTest calls, render cycle start/end
4. Add `Logger.LogError()` at: all caught exceptions, with stack trace and context
5. Add performance timing: wrap `CanvasControl.OnPaint()` with stopwatch, log if > 16ms

## Required Deliverables

- Log format spec documented in `0606_Observability.md`
- Logging added to at least: `LineTool.OnMouseUp`, `Calculator.Calculate`, `TakeOffFileStore.Save`, `TakeOffFileStore.Load`
- Performance log: any render exceeding 16ms emits a `LogWarning` entry
- Log file location documented in `AppConfig`

## Validation Rules

- Run app, draw 3 shapes, save file → log file contains at least 6 Info entries
- Trigger a validation error → log file contains 1 Error entry with exception type
- Log file encoding: UTF-8, ASCII-safe log messages (no emoji in log output)

## Anti-Patterns

- Logging at every property getter/setter (too verbose, kills performance)
- Logging sensitive data (file paths are OK; user content is not)
- Using `Console.WriteLine` instead of `ILogger` anywhere in the codebase

---

## AI Intake Overlay - Draft 0601 Promotion

This overlay captures the remaining 0601 items that still need promotion into the live SDLC library and backlog.

| **Category** | **Item**                                     | **Status** | **Where It Belongs**                                           | **Notes**                                                    |
| ------------ | -------------------------------------------- | ---------- | -------------------------------------------------------------- | ------------------------------------------------------------ |
| Data         | Import session + source artifact persistence | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | Needed before accepted AI objects can live in the file model |
| AI           | OCR result storage                           | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | Keep extracted text reviewable and auditable                 |
| AI           | Geometry candidate detection                 | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | Produces reviewed candidates, not final objects              |
| AI           | Scale confirmation workflow                  | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | User must confirm scale before conversion                    |
| AI           | Classification + confidence + review         | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | Needed for accept/reject/edit flow                           |
| UI           | AI review surface in the clean form          | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md`          | Add overlays/panels without disturbing the dev form          |
| Testing      | AI intake fixtures and acceptance tests      | Planned    | `1_Task_Backlog.md` / `0401`                                   | Cover intake -> review -> export                             |
| Ops          | AI tracing and packaging fallback            | Planned    | `0_CoNSoL_Production_Layers.md` / `1_Task_Backlog.md` / `0304` | Capture logs and pick offline OCR/CV strategy                |

# Final Checklist — Production Readiness Gate

## Core (L01–L04)


- [ ] Canvas coordinate conversion implemented and tested
- [ ] HitTest implemented per shape type
- [ ] UC-001 end-to-end: draw → save → reload shows correct shape
- [ ] UC-004 end-to-end: draw objects → run take-off → export CSV
- [ ] UC-002 end-to-end: assign object to layer, layer visible/locked toggle
- [ ] UC-007 end-to-end: delete layer, objects reassigned
- [ ] Layer panel implemented (FR-LP-001 through FR-LP-004)
- [ ] Property panel context-sensitive (0208 §4.1 table)

## Quality (L05–L06)


- [ ] ICommand pattern implemented (UC-012 Undo/Redo working)
- [ ] 40+ unit tests passing (Domain + Application layer)
- [ ] Acceptance test per UC-001 and UC-004 passing
- [ ] No circular layer dependencies
- [ ] All public APIs have XML doc comments

## Delivery (L07–L08)


- [ ] build.ps1 runs clean (Restore → Build → Test → Publish)
- [ ] Standalone installer tested on clean Windows machine
- [ ] Log file produced on first run
- [ ] Performance: redraw time logged, within 16ms for <100 objects
- [ ] ADR-001, ADR-002, ADR-003 authored

---
> See `1_Task_Backlog.md` for the concrete task list with UC/FR/GAP traceability.
