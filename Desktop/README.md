# Desktop Layer (WinForms UI)

The **Desktop** layer is the **presentation layer** containing the WinForms user interface for CoNSoL-TakeOff.

This layer provides the visual interface for drawing, editing, and managing construction take-off projects.

---

## 📋 Overview

### Purpose

The Desktop layer provides:
- **2D Drawing Canvas** — Interactive drawing surface with geometric shapes
- **Tool System** — Select, Line, Rectangle, Circle, Polyline, Pan, Zoom tools
- **Property Inspector** — Context-sensitive property editing
- **Forms & Dialogs** — Material management, block assignment, settings
- **Event Handling** — User interaction orchestration

### Design Principle

> **The Desktop layer is thin and orchestration-focused. Business logic belongs in Application and Domain.**

### Architecture

```
WinForms UI
    ↓
Desktop Layer (MainForm, CanvasControl, PropertiesPanel)
    ↓
Application Layer (Services, Orchestration)
    ↓
Domain Layer (Entities, Calculations)
    ↓
Infrastructure Layer (IO, Config, Logging)
```

---

## 🏗️ Project Structure

```
Desktop/
├── Forms/
│   ├── MainForm.vb                    # Main application window
│   ├── MainForm.Designer.vb           # Designer-generated (DO NOT EDIT)
│   ├── BlockAssignmentForm.vb         # Block/Material assignment dialog
│   ├── BlockAssignmentForm.Designer.vb
│   ├── BlockAssignmentModel.vb        # MVVM model for block assignment
│   └── MaterialCrudForm.vb            # Material CRUD dialog
├── Controls/
│   ├── CanvasControl.vb               # 2D drawing canvas
│   ├── PropertiesPanel.vb             # Property inspector
│   └── LineShape.vb                   # Line shape implementation
├── CompositionRoot.vb                 # Dependency injection setup
├── ApplicationEvents.vb               # VB Application Framework events
├── Program.vb                         # Entry point (Main Sub)
├── PublicTypes.vb                     # Enums and public types
└── README.md
```

---

## 📊 Core Components

### 1. MainForm

**Purpose:** Main application window — primary UI container.

**Key Responsibilities:**

- **Window Layout** — Menu, toolbars, canvas, panels, statusbar
- **Tool Activation** — Respond to toolbar button clicks
- **File Operations** — Open, save, new drawing
- **Event Coordination** — Wire up control events

**Key Properties:**

```vb
Public Class MainForm
    Inherits Form

    Private ReadOnly _canvas As CanvasControl           ' 2D drawing surface
    Private ReadOnly _left As Panel                      ' Left sidebar (tools)
    Private ReadOnly _propertiesPanel As PropertiesPanel ' Property inspector
    Private ReadOnly _status As StatusStrip              ' Status bar

    Private CurrentLayout As CanvasLayout                ' Current drawing state
End Class
```

**Key Methods:**

```vb
' Toolbar button handlers
Private Sub BtnSelect_Click() Handles btnSelect.Click
    _canvas.SetTool(ToolType.SelectTool)
End Sub

Private Sub BtnLine_Click() Handles btnLine.Click
    _canvas.SetTool(ToolType.Line)
End Sub

' File operations
Private Sub BtnNew_Click() Handles btnNew.Click
    CurrentLayout = New CanvasLayout()
    _canvas.SetLayout(CurrentLayout)
End Sub

Private Async Sub BtnOpen_Click() Handles btnOpen.Click
    Dim dialog = New OpenFileDialog With {.Filter = "TakeOff files (*.takeoff)|*.takeoff"}
    If dialog.ShowDialog() = DialogResult.OK Then
        Dim fileStore = CompositionRoot.ResolveFileStore()
        CurrentLayout = Await fileStore.LoadAsync(dialog.FileName)
        _canvas.SetLayout(CurrentLayout)
    End If
End Sub

Private Async Sub BtnSave_Click() Handles btnSave.Click
    Dim fileStore = CompositionRoot.ResolveFileStore()
    Await fileStore.SaveAsync(CurrentLayout, "current.takeoff")
    _status.Items(0).Text = "Saved successfully"
End Sub
```

**Related Use Cases:**
- All use cases (central hub)

**Key Design Patterns:**
- **Composition** — Aggregates CanvasControl, PropertiesPanel, toolbars
- **Dependency Injection** — Gets services from CompositionRoot
- **Event Propagation** — Receives events from child controls

---

### 2. CanvasControl

**Purpose:** The interactive 2D drawing canvas where users create shapes.

**Key Responsibilities:**

- **Shape Rendering** — Draw all CanvasElements to the surface
- **Tool Processing** — Handle mouse events per active tool
- **Coordinate Mapping** — Convert physical (pixels) ↔ logical (units) coordinates
- **Grid & Snapping** — Optional grid rendering and snap-to-grid
- **Selection** — Track selected objects for property editing

**Key Properties:**

```vb
Public Class CanvasControl
    Inherits Control

    Private _layout As CanvasLayout                      ' Current drawing state
    Private _currentTool As ToolType                     ' Active tool
    Private _selectedElements As List(Of CanvasElement)  ' Currently selected
    Private _grid As GridSettings                        ' Grid configuration
    Private _zoomLevel As Double                         ' Zoom factor (1.0 = 100%)
    Private _panX As Double, _panY As Double             ' Pan offset
End Class
```

**Key Methods:**

```vb
''' <summary>Set the drawing layout</summary>
Public Sub SetLayout(layout As CanvasLayout)
    _layout = layout
    Invalidate()  ' Request redraw
End Sub

''' <summary>Set the active drawing tool</summary>
Public Sub SetTool(tool As ToolType)
    _currentTool = tool
End Sub

''' <summary>Add a shape to the canvas</summary>
Public Sub AddElement(element As CanvasElement)
    _layout.Elements.Add(element)
    Invalidate()
End Sub

''' <summary>Get elements at a point (for selection)</summary>
Public Function ElementsAtPoint(point As Point) As List(Of CanvasElement)
    ' Hit testing using Geometry utilities
End Function

''' <summary>Convert physical to logical coordinates</summary>
Public Function ScreenToLogical(screenPoint As Point) As Point
    ' Map pixel coordinates to drawing units
End Function

''' <summary>Convert logical to physical coordinates</summary>
Public Function LogicalToScreen(logicalPoint As Point) As Point
    ' Map drawing units to pixel coordinates
End Function
```

**Event Handling:**

```vb
Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
    ' Start tool operation
    Select Case _currentTool
        Case ToolType.Line
            ' Capture start point, enter line drawing mode
        Case ToolType.Rectangle
            ' Capture top-left corner
        Case ToolType.SelectTool
            ' Hit test, select element
    End Select
End Sub

Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
    ' Update tool preview (rubber-band, etc.)
    ' Refresh cursor style based on element under cursor
End Sub

Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
    ' Commit shape, exit tool mode
End Sub

Protected Overrides Sub OnPaint(e As PaintEventArgs)
    ' Render canvas background
    ' Render grid (if enabled)
    ' Render all elements
    ' Render selection indicators (highlights)
    ' Render tool preview (if active)
End Sub
```

**Tool Interaction Model (from Mega-File.md §6.2):**

```
MouseDown → start
  ↓
MouseMove → preview (rubber-band)
  ↓
MouseUp → commit
  ↓
Escape → cancel
```

**Related Use Cases:**
- UC-001: Draw a Line on the Canvas
- UC-005: Insert a symbol from the library
- UC-006: Edit properties of a multi-selection

**Key Design Patterns:**
- **Separation of Concerns** — Canvas handles rendering/events, logic in Application
- **Double Buffering** — Reduces flicker during redraw
- **Coordinate Abstraction** — Logical/physical separation enables zoom/pan

---

### 3. PropertiesPanel

**Purpose:** Context-sensitive property inspector for editing selected objects.

**Key Responsibilities:**

- **Context Detection** — Show appropriate properties based on selection
- **Property Binding** — Bind UI controls to CanvasElement properties
- **Multi-Selection** — Show `(mixed)` for differing values
- **Validation** — Validate input before saving to model

**Key Properties:**

```vb
Public Class PropertiesPanel
    Inherits UserControl

    Private _selectedElements As List(Of CanvasElement)   ' Current selection
    Private _propertyGrid As PropertyGrid                 ' Native WinForms control
    Private _logicalPropertiesPanel As Panel              ' 3D logical properties
End Class
```

**Key Methods:**

```vb
''' <summary>Set the selected elements to display properties for</summary>
Public Sub SetSelection(elements As List(Of CanvasElement))
    _selectedElements = elements
    UpdatePanel()
End Sub

''' <summary>Update panel based on current selection</summary>
Private Sub UpdatePanel()
    ' Clear existing controls
    ' Determine selection type (single, multi same-type, multi mixed-type)
    ' Generate appropriate property fields
    ' Bind to data
    ' Set validation rules
End Sub

''' <summary>Handle property change</summary>
Private Sub OnPropertyChanged(propertyName As String, newValue As Object)
    ' Validate new value
    ' Apply to all selected elements
    ' Trigger canvas redraw
    ' Mark drawing as modified
End Sub
```

**Display Modes:**

```
Selection State          | Properties Shown
─────────────────────────┼──────────────────────────────────────
None (canvas)            | Canvas properties (Unit, ScaleFactor)
Single object            | All properties for that type + universal
Multi same-type          | Shared properties + (mixed) placeholders
Multi mixed-type         | Universal properties only
                         |   (Layer, Color, LineStyle, Tags, Marks)
Active tool              | Tool defaults (e.g., line width for Line tool)
```

**Universal Properties** (all objects):

```
Layer                    ' Layer assignment
Color                    ' Line color
LineStyle                ' Solid, Dashed, Dotted
LineWeight               ' Thickness
Visibility               ' Visible/Hidden
Lock                     ' Editable/Locked
Notes                    ' User comments
Tags                     ' Smart Tags (Material, Quantity, etc.)
Marks                    ' Custom Marks
```

**Type-Specific Properties** (example Rectangle):

```
X, Y                     ' Position
Width, Height            ' Dimensions
Rotation                 ' Angle (0-360)
CornerRadius             ' Rounded corners
Fill                     ' Fill color/pattern
```

**Logical 3D Properties** (when applicable):

```
Height (H)               ' Vertical dimension
Width (W)                ' Horizontal dimension
Length (L)               ' Depth dimension
Unit System              ' m, ft, etc.
Quantity                 ' Multiplier
UnitPrice                ' Cost per unit
TotalCost                ' (calculated)
```

**Related Use Cases:**
- UC-003: Attach a Smart Tag to an object
- UC-006: Edit properties of a multi-selection

**Key Design Patterns:**
- **Property Binding** — Two-way data binding to selected elements
- **Context Adaptation** — Show/hide fields based on selection type
- **Validation** — Inline error display on invalid input

---

### 4. BlockAssignmentForm & Model

**Purpose:** Dialog for assigning blocks/materials to shapes.

**Components:**

```vb
' Form (Dialog window)
Public Class BlockAssignmentForm
    Inherits Form

    Private _model As BlockAssignmentModel
    Private _blockComboBox As ComboBox
    Private _materialComboBox As ComboBox
    Private _formulaTextBox As TextBox
    Private _okButton As Button, _cancelButton As Button
End Class

' MVVM Model (VB recommended approach)
Public Class BlockAssignmentModel
    Public Property SelectedBlock As String
    Public Property SelectedMaterial As String
    Public Property SelectedFormula As String

    Public Function Validate() As Boolean
        Return Not String.IsNullOrWhiteSpace(SelectedBlock)
    End Function
End Class
```

**Related Use Cases:**
- UC-003: Attach a Smart Tag to an object (material assignment)

---

### 5. MaterialCrudForm

**Purpose:** Dialog for managing materials (Create, Read, Update, Delete).

**Key Features:**

- **Material List** — DataGridView showing all materials
- **CRUD Operations** — Add, Edit, Delete buttons
- **Property Editor** — Edit name, unit, price
- **Validation** — Ensure required fields are filled

**Related Use Cases:**
- UC-004: Run a take-off quantity summary (material lookup)

---

### 6. Supporting Types (PublicTypes.vb)

**Purpose:** Enums and types used across the Desktop layer.

**Key Enums:**

```vb
Public Enum ToolType
    SelectTool
    Line
    Rectangle
    Circle
    Polyline
    Ellipse
    Pan
    ZoomIn
    ZoomOut
    Text
    Dimension
    Symbol
End Enum

Public Enum GridSettings
    None
    ShowGrid
    SnapToGrid
    ShowAndSnap
End Enum

Public Enum SelectionMode
    Single
    Multiple
    Window
    Crossing
End Enum
```

---

## 🔄 Data Flow

### User Creates a Rectangle

```
User clicks Rectangle tool button
    ↓
BtnRectangle_Click() 
    ↓
_canvas.SetTool(ToolType.Rectangle)
    ↓
CanvasControl.OnMouseDown (rectangle drawing starts)
    ├─ ScreenToLogical() convert pixel to units
    ├─ Store start point
    └─ Enter preview mode
    ↓
CanvasControl.OnMouseMove (for each mouse move)
    ├─ Calculate rubber-band preview
    ├─ OnPaint() renders preview
    ↓
CanvasControl.OnMouseUp (user finishes)
    ├─ Create new CanvasElement
    ├─ Set geometry JSON
    ├─ Add to _layout.Elements
    ├─ Invalidate() triggers redraw
    └─ Exit tool mode
    ↓
Canvas redraws
    ↓
PropertiesPanel.SetSelection([new rectangle])
    ↓
Panel displays rectangle properties (X, Y, Width, Height, etc.)
```

### User Assigns Material to Rectangle

```
User selects rectangle (already on canvas)
    ↓
CanvasControl.SelectTool processes MouseUp
    ├─ ElementsAtPoint() finds rectangle
    ├─ Select it
    └─ Fire SelectionChanged event
    ↓
PropertiesPanel.SetSelection([rectangle])
    ↓
Panel displays properties
    ↓
User opens "Assign Material" dialog
    ↓
BlockAssignmentForm shows available materials
    ↓
User selects material, clicks OK
    ↓
Form returns BlockAssignmentModel
    ↓
MainForm.OnBlockAssignmentComplete()
    ├─ Update selected rectangle's BusinessJson
    ├─ Trigger calculation
    └─ Update statusbar with quantity
    ↓
Canvas redraws with updated appearance
```

---

## 🧵 Event Flow

### Single Selection → Property Display

```
Canvas.SelectionChanged event
    ↓
MainForm catches event
    ↓
PropertiesPanel.SetSelection(selectedElements)
    ↓
Panel creates property controls
    ↓
Bind to selected element
    ↓
Display in UI
```

### Property Changed → Model Update

```
User edits property (e.g., Width = 10)
    ↓
PropertiesPanel.OnPropertyChanged()
    ↓
Validate input (must be > 0)
    ↓
Update selected element
    ├─ Modify GeometryJson
    ├─ Trigger recalculation
    └─ Mark drawing modified
    ↓
Canvas.Invalidate()
    ↓
OnPaint() redraws canvas
```

---

## 🎨 UI/UX Considerations (from Mega-File.md §0208)

### Canvas Guidelines

✅ **Do:**
- Use adequate margins/padding around drawing area
- Support keyboard shortcuts (Ctrl+Z for undo, Ctrl+S for save)
- Provide visual feedback (cursor changes, status messages)
- Support multiple selection (Ctrl+Click, window select)

❌ **Don't:**
- Use absolute positioning (use anchors/docking)
- Block user input on validation errors (warn instead)
- Perform heavy calculations on every mouse move (debounce)

### Property Panel Guidelines

✅ **Do:**
- Show `(mixed)` for multi-selection with differing values
- Disable fields when all objects are locked
- Provide inline validation feedback
- Update real-time (no "Apply" button needed)

❌ **Don't:**
- Show too many fields at once (group logically)
- Allow invalid values (validate before commit)
- Update canvas on every keystroke (wait for enter/blur)

---

## 🔌 Dependency Injection (CompositionRoot.vb)

**Purpose:** Central DI container setup.

**Typical Setup:**

```vb
Public Class CompositionRoot
    Private Shared _serviceProvider As IServiceProvider

    Public Shared Sub Initialize()
        Dim services = New ServiceCollection()

        ' Infrastructure
        Dim config = AppConfig.LoadFromEnvironment()
        services.AddSingleton(config)
        services.AddSingleton(Of ILogger)(New FileLogger(config.LogFilePath))

        ' Persistence
        services.AddSingleton(New TakeOffFileStore())
        services.AddSingleton(New MaterialJsonStore())

        ' Application services
        services.AddScoped(Of IMaterialService)(New MaterialService(...))
        services.AddScoped(Of ITakeOffService)(New TakeOffService(...))

        ' Build provider
        _serviceProvider = services.BuildServiceProvider()
    End Sub

    Public Shared Function Resolve(Of T)() As T
        Return _serviceProvider.GetRequiredService(Of T)()
    End Function
End Class
```

---

## 🧪 Testing Considerations

### Unit Tests

- **MainForm** — Test toolbar button wiring, file operations
- **CanvasControl** — Test coordinate mapping, tool switching
- **PropertiesPanel** — Test property binding, multi-selection display

### Integration Tests

- **End-to-End Drawing** — Create shape → Set properties → Save/load
- **Tool Workflow** — Activate tool → Draw → Commit → Verify in model

### UI Testing (Manual)

- Grid rendering and snapping
- Zoom and pan operations
- Multi-selection highlighting
- Property panel update speed

---

## 📝 WinForms Best Practices

### Designer Files

✅ **Do:**
- Keep `*.Designer.vb` auto-generated (don't edit manually)
- Use `InitializeComponent()` only (Designer responsibility)
- Pin complex controls in constructor

❌ **Don't:**
- Add event handlers in Designer file
- Add business logic in `InitializeComponent()`
- Use `Handles` clause in Designer file

### Control Naming

- Prefix with control type: `btn` (Button), `txt` (TextBox), `dgv` (DataGridView)
- Example: `btnSave`, `txtFilePath`, `dgvMaterials`

### Double Buffering

```vb
Protected Overrides ReadOnly Property DoubleBuffered As Boolean
    Get
        Return True
    End Get
End Property
```

---

## 🔗 References

### Mega-File Documentation

- [0208-UX_UI_Design](../Mega-File.md#-0208--ux--ui-design) — Interaction model, validation rules
- [0301-Development_Documentation](../Mega-File.md#-0301--development-documentation) — Coding standards
- [UC-001-007](../Mega-File.md#-use-cases) — Use case workflows

### Related Layers

- **Application** — Provides services (TakeOffService, MaterialService)
- **Domain** — Provides entities (CanvasElement, CanvasLayout)
- **Infrastructure** — Provides config, logging, persistence

---

## 📝 Conventions

### Form Naming

- `*Form.vb` for main windows
- `*Dialog.vb` for modal dialogs
- `*Panel.vb` for user controls

### Control Naming

- Buttons: `btn*` (btnSave, btnCancel)
- TextBoxes: `txt*` (txtFilePath, txtQuantity)
- ComboBoxes: `cbx*` (cbxMaterial, cbxLayer)
- Labels: `lbl*` (lblMaterial, lblQuantity)
- DataGridView: `dgv*` (dgvMaterials)

### Event Handler Naming

- `On*` or `Btn*_Click` pattern (BtnSave_Click, BtnClose_Click)
- Private handlers (not Public Sub)

---

## ⚠️ Important Notes

### No Business Logic

❌ Do NOT add:
- Calculation logic (belongs in Application)
- Data access (belongs in Infrastructure)
- Entity manipulation (minimal, prefer service calls)

✅ Keep Desktop layer:
- Pure UI rendering and event handling
- Delegation to Application services
- Thin orchestration only

### Thread Safety

- All UI updates **must** occur on the UI thread
- Long-running operations → Use Task with ConfigureAwait(False)
- Use `Invoke` for cross-thread updates

```vb
Private Async Sub BtnCalculate_Click() Handles btnCalculate.Click
    Dim result = Await Task.Run(Function() DoExpensiveCalculation())
    ' Update UI (automatically on UI thread with Async/Await)
    lblResult.Text = result.GrandTotalCost.ToString()
End Sub
```

---

## 🚀 Quick Reference

### Add a Toolbar Button

```vb
Dim btnCustom As New Button With {
    .Text = "Custom Tool",
    .Dock = DockStyle.Top,
    .Height = 34
}
AddHandler btnCustom.Click, Sub() _canvas.SetTool(ToolType.CustomTool)
```

### Set Canvas Layout

```vb
Dim layout = New CanvasLayout()
_canvas.SetLayout(layout)
```

### Get Selected Elements

```vb
Dim selected = _canvas.GetSelectedElements()
_propertiesPanel.SetSelection(selected)
```

### Update Statusbar

```vb
_status.Items(0).Text = "Drawing modified"
```

---

**Last Updated:** January 2025  
**Layer Responsibility:** Presentation (WinForms UI)  
**Platform:** Windows 10+ with .NET 8.0+  
**Maintainer:** Development Team
