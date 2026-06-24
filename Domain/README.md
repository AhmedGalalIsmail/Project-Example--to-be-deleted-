# Domain Layer

The **Domain** layer contains the **core business logic, data entities, and utilities** for CoNSoL-TakeOff.

This is the **heart of the application** — all other layers depend on it.

---

## 📋 Overview

### Purpose

The Domain layer provides:
- **Data Entities** - Shape geometry, business definitions, relationships
- **Business Logic** - Calculation rules, validation, aggregation
- **Utilities** - Geometric calculations, helper functions
- **Independence** - No dependencies on UI or infrastructure frameworks

### Design Principle

> **The Domain must be reusable, testable, and framework-agnostic.**

---

## 🏗️ Project Structure

```
Domain/
├── Entities/
│   ├── CanvasElement.vb              # Shape + metadata container
│   ├── CanvasLayout.vb               # Canvas state (collection of elements)
│   ├── BusinessDefinition.vb         # Business metadata (Material, Quantity, Price)
│   ├── BlockModels.vb                # Block/Symbol definitions
│   └── ElementRelationship.vb        # Nested object relationships (Parent-Child)
├── Utilities/
│   └── Geometry.vb                   # Geometric calculations (distance, area, etc.)
└── README.md
```

---

## 📊 Core Entities

### 1. CanvasElement

**Purpose:** Represents a single drawn object (shape) on the canvas.

**Structure:**
```vb
Public Class CanvasElement
    Public Property Id As Guid                          ' Unique identifier
    Public Property Type As String                      ' Shape type (Line, Rectangle, Circle, etc.)
    Public Property Layer As String                     ' Layer assignment
    Public Property GeometryJson As String              ' Visual geometry (serialized)
    Public Property BusinessJson As String              ' Business metadata (serialized)
    Public Property RelationshipType As ElementRelationshipType  ' Relationship to parent
    Public Property ChildElementId As String            ' Link to child element
    Public Property ParentElementId As String           ' Link to parent element
End Class
```

**Key Points:**
- Geometry and business data **separated** (different JSON fields)
- Supports **nested objects** (parent-child relationships)
- **Type-agnostic** — shape type determined by Type property
- **Layer-aware** — can query by layer for batch operations

**Related Use Cases:**
- UC-001: Draw a Line on the Canvas
- UC-002: Assign an object to a layer
- UC-006: Edit properties of a multi-selection

---

### 2. CanvasLayout

**Purpose:** Represents the entire drawing canvas state.

**Structure:**
```vb
Public Class CanvasLayout
    Public Property Id As Guid                          ' Canvas/drawing ID
    Public Property Name As String                      ' Drawing name
    Public Property Unit As String                      ' Unit system (m, ft, etc.)
    Public Property ScaleFactor As Double               ' Logical-to-physical scale
    Public Property Elements As List(Of CanvasElement)  ' All drawn objects
    Public Property Layers As List(Of Layer)            ' Layer definitions
    Public Property CreatedAt As DateTime               ' Creation timestamp
End Class
```

**Key Points:**
- Contains **all elements** in the drawing
- Defines **unit system** and **scale factor** for coordinate mapping
- Tracks **layers** for visibility, locking, styling
- **Serializable** to JSON/database

**Related Use Cases:**
- UC-004: Run a take-off quantity summary
- UC-008: Switch between standalone and integrated mode

---

### 3. BusinessDefinition

**Purpose:** Metadata that assigns business meaning to a shape.

**Structure:**
```vb
Public Class BusinessDefinition
    Public Property BlockRef As String                  ' Reference to block/material definition
    Public Property DimensionMode As String             ' D0 (count), D1 (length), D2 (area), D3 (volume)
    Public Property FormulaCode As String               ' Formula to apply for calculation
    Public Property Quantity As Double                  ' Calculated quantity
    Public Property Unit As String                      ' Unit of measurement (m, m², m³, count)
    Public Property UnitPrice As Double                 ' Price per unit
    Public Property TotalCost As Double                 ' Quantity × UnitPrice (calculated)
End Class
```

**Dimension Modes (from Mega-File.md §7.1):**

| Mode | Meaning | Calculation | Example |
|------|---------|-------------|---------|
| D0 | Count | 1 per shape | Door count |
| D1 | Length | Derived from geometry | Wall length (m) |
| D2 | Area | Width × Height or derived | Room area (m²) |
| D3 | Volume | Area × Depth | Concrete volume (m³) |

**Key Points:**
- **Separates** visual (geometry) from business (this class)
- Drives **calculation engine** logic
- Supports **formula-based** quantity calculations
- **Extensible** — new dimension modes can be added

**Related Use Cases:**
- UC-003: Attach a Smart Tag to an object
- UC-004: Run a take-off quantity summary

---

### 4. BlockModels

**Purpose:** Defines a reusable block/symbol template.

**Structure:**
```vb
Public Class BlockDefinition
    Public Property Id As Guid                          ' Block unique ID
    Public Property Name As String                      ' Block name (e.g., "Wall-Standard")
    Public Property Components As List(Of BlockComponent)  ' Child geometry
    Public Property BasePoint As Point                  ' Insertion point
    Public Property Attributes As Dictionary(Of String, Object)  ' Custom attributes
End Class
```

**Key Points:**
- Used for **symbol libraries** (windows, doors, repeating elements)
- Supports **attribute definitions** (editable per instance)
- **Reusable** across multiple drawings
- Can contain **nested geometry**

**Related Use Cases:**
- UC-005: Insert a symbol from the library

---

### 5. BlockComponent

**Purpose:** A single geometry element within a block definition.

**Structure:**
```vb
Public Class BlockComponent
    Public Property Id As Guid                          ' Component ID
    Public Property Geometry As String                  ' Geometry data (JSON)
    Public Property BlockId As Guid                     ' Parent block ID
    Public Property Order As Integer                    ' Draw order
End Class
```

**Key Points:**
- Represents **primitive shapes** within a block
- Defines **draw order** for layering
- Part of **block composition** (one block may have many components)

---

### 6. ElementRelationship

**Purpose:** Models parent-child relationships for nested objects.

**Structure:**
```vb
Public Class ElementRelationship
    Public Property ParentElementId As Guid             ' Parent shape ID
    Public Property ChildElementId As Guid              ' Child shape ID
    Public Property RelationshipType As ElementRelationshipType  ' Contains, Subtracts, etc.
End Class

Public Enum ElementRelationshipType
    Contains                                            ' Child is inside parent
    Subtracts                                           ' Child area subtracts from parent
    Adjacent                                            ' Child is next to parent (future)
End Enum
```

**Key Points:**
- Enables **nested object modeling** (doors inside walls)
- Supports **calculation rules** (e.g., door subtracts from wall area)
- **Prevents circular references** (validation rule in engine)

**Example:**
```
Wall (area = 50 m²)
  ├─ Door 1 (subtracts 2 m²)
  └─ Door 2 (subtracts 2 m²)
  Result: Net wall area = 46 m²
```

**Related Use Cases:**
- UC-001 (implicitly, through hierarchy)
- UC-004: Run a take-off quantity summary

---

## 🧮 Utilities

### Geometry.vb

**Purpose:** Geometric calculations and helper functions.

**Key Methods:**

| Method | Purpose | Returns |
|--------|---------|---------|
| `CalculateDistance(p1, p2)` | Distance between two points | Double (length) |
| `CalculateArea(points)` | Area of a polygon | Double (area) |
| `CalculatePolygonCentroid(points)` | Center point of polygon | Point |
| `IsPointInside(point, polygon)` | Hit testing | Boolean |
| `SnapToGrid(point, gridSize)` | Grid snapping | Point |
| `RotatePoint(point, angle, center)` | Rotation transformation | Point |

**Key Points:**
- **No UI dependencies** — pure math
- **Reusable** across desktop and future web implementations
- **Testable** — geometric logic isolated

**Related Use Cases:**
- UC-001: Draw a Line on the Canvas (distance calculation)
- UC-004: Run a take-off quantity summary (area/volume calculation)

---

## 🔄 Data Flow

### From Drawing to Calculation

```
CanvasElement (visual)
  ↓ GeometryJson
  └─→ Geometry.vb calculates length/area/volume

CanvasElement (business)
  ↓ BusinessJson
  └─→ BusinessDefinition extracted
      ↓ Quantity = calculated geometry × DimensionMode
      └─→ TakeOffResult (quantity × price = cost)
```

### Example: Room with Doors

```
Drawing Canvas:
  ├─ Room Rectangle (D2, area = 50 m²)
  │   └─ BusinessDefinition: Material="Concrete", Unit Price = 20 €/m²
  │
  ├─ Door 1 Rectangle (D2, area = 2 m²)
  │   ├─ RelationshipType: Subtracts (from Room)
  │   └─ BusinessDefinition: Material="Wood", Unit Price = 100 €/unit
  │
  └─ Door 2 Rectangle (D2, area = 2 m²)
      ├─ RelationshipType: Subtracts (from Room)
      └─ BusinessDefinition: Material="Wood", Unit Price = 100 €/unit

Calculation:
  Room net area = 50 - 2 - 2 = 46 m²
  Room cost = 46 m² × 20 €/m² = 920 €
  Door cost = 2 × 100 € = 200 €
  Total = 1120 €
```

---

## 🧪 Testing Considerations

### Unit Tests

- **GeometryUtilities** — Test geometric calculations independently
- **BusinessDefinition** — Test quantity calculations per dimension mode
- **ElementRelationship** — Test nested object logic and circular reference prevention
- **Serialization** — Test JSON round-trip integrity

### Integration Tests

- **CanvasLayout** + **CanvasElement** — End-to-end drawing state
- **Relationships** + **Calculation** — Parent-child quantity rules

---

## 🔗 References

### Mega-File Documentation

- [020103-Data_Model](../Mega-File.md#-020103--data-model) — Full entity relationships and schema
- [0201-Design_Documentation](../Mega-File.md#-0201--design-documentation) — Architecture context
- [0104-SRS §5.2](../Mega-File.md#-drawing-tools) — Functional requirements for drawing tools

### Related Layers

- **Application** — Consumes Domain entities for use case orchestration
- **Desktop** — Uses Domain entities in UI binding and serialization
- **Infrastructure** — Persists Domain entities via repositories

---

## 📝 Conventions

### Naming

- Entity classes use **PascalCase** (CanvasElement, BusinessDefinition)
- Properties use **PascalCase** (Id, Type, Layer)
- Enums use **PascalCase** with **Singular name** (ElementRelationshipType)

### Immutability

- Entities are **mutable** (properties have setters for data binding)
- **Validation** occurs at the Application/Infrastructure layer
- Domain entities trust the caller (principle of "fast fail")

### Serialization

- All entities have **parameterless constructors** (JSON deserialization)
- Properties are **public** with default setters
- Complex objects stored as **JSON strings** (GeometryJson, BusinessJson)

---

## ⚠️ Important Notes

### No UI Dependencies

❌ Do NOT add:
- Windows.Forms references
- WPF references
- Any UI framework imports

✅ Keep domain layer:
- Pure .NET Framework APIs
- JSON serialization only
- Geometric calculations only

### Framework Independence

This layer is designed to be **host-agnostic**:
- ✅ Desktop (WinForms)
- ✅ Web (Blazor/HTML5 Canvas)
- ✅ Mobile (Xamarin/MAUI future)

---

## 🚀 Quick Reference

### Create a Canvas Element

```vb
Dim element As New CanvasElement With {
    .Id = Guid.NewGuid(),
    .Type = "Rectangle",
    .Layer = "Walls",
    .GeometryJson = "{""width"": 5.0, ""height"": 3.0}",
    .BusinessJson = "{""blockRef"": ""BLOCK_WALL"", ""dimensionMode"": ""D2""}"
}
```

### Calculate Area

```vb
Dim area = Geometry.CalculateArea(points)
```

### Access Nested Objects

```vb
Dim parentElement = layout.Elements.FirstOrDefault(Function(e) e.Id = parentId)
Dim childElements = layout.Elements.Where(Function(e) e.ParentElementId = parentId)
```

---

**Last Updated:** January 2025  
**Layer Responsibility:** Business Logic & Data Entities  
**Maintainer:** Development Team
