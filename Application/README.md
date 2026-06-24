# Application Layer

The **Application** layer contains the **use case orchestration, business services, and calculation engines** for CoNSoL-TakeOff.

This layer bridges the **Domain** (data) and **Infrastructure** (persistence) layers, implementing the workflows defined in the SDLC requirements.

---

## 📋 Overview

### Purpose

The Application layer provides:
- **Use Case Orchestration** — Coordinates domain entities to implement business workflows
- **Calculation Engine** — Computes quantities, costs, aggregations
- **Business Services** — Material lookups, quantity services, pricing
- **Context & Results** — Contextual calculation parameters and aggregation results

### Design Principle

> **Application logic is orchestration, not computation. Domain contains computation.**

The layers work together as:
```
Desktop (UI) → Application (workflows) → Domain (calculations) → Infrastructure (persistence)
```

---

## 🏗️ Project Structure

```
Application/
├── Services/
│   ├── TakeOffService.vb              # Quantity & cost aggregation
│   └── MaterialService.vb             # Material management & lookup
├── TakeOffCalculator.vb               # Core calculation engine
├── TakeOffContext.vb                  # Calculation context parameters
├── TakeOffResult.vb                   # Result aggregation object
└── README.md
```

---

## 📊 Core Components

### 1. TakeOffCalculator

**Purpose:** Core calculation engine — computes quantities and costs from canvas state.

**Key Responsibilities:**

- **Quantity Calculation** — Apply dimension mode (D0-D3) to shapes
- **Nested Object Logic** — Handle parent-child relationships (doors in walls, etc.)
- **Cost Aggregation** — Multiply quantity × unit price
- **Formula Application** — Apply custom formulas to shapes

**Key Methods:**

```vb
Public Function Calculate(
    layout As CanvasLayout,
    ctx As TakeOffContext
) As TakeOffResult
    ' Compute quantities for all elements
    ' Apply formulas
    ' Handle nested objects
    ' Return aggregated results
End Function

Private Function CalculateElementQuantity(
    element As CanvasElement,
    def As BusinessDefinition,
    relationships As List(Of ElementRelationship)
) As Double
    ' Determine quantity based on dimension mode
    ' Apply relationship logic (subtract child quantities)
    ' Return net quantity
End Function
```

**Related Use Cases:**
- UC-004: Run a take-off quantity summary
- UC-008: Switch between standalone and integrated mode

**Example:**

```vb
' Usage
Dim calculator = New TakeOffCalculator()
Dim ctx = New TakeOffContext With {
    .UnitSystem = "metric",
    .ApplyFormulaOverrides = True
}
Dim result = calculator.Calculate(layout, ctx)

' Result contains:
' - Aggregated quantities by material
' - Costs by layer
' - Grand totals
' - Validation warnings (if any)
```

---

### 2. TakeOffService

**Purpose:** High-level service for quantity & cost aggregation operations.

**Key Responsibilities:**

- **Group Aggregation** — Group elements by tag, layer, or type
- **Sum/Average Functions** — Compute statistics on grouped quantities
- **Export Formatting** — Prepare results for CSV/Excel export
- **Caching** — Cache intermediate results for performance

**Key Methods:**

```vb
Public Function AggregateByTag(
    layout As CanvasLayout,
    tagName As String,
    aggregateFunc As AggregateFunction  ' Sum, Average, Count, Min, Max
) As Dictionary(Of String, Double)
    ' Group elements by tag value
    ' Apply aggregate function
    ' Return results
End Function

Public Function GetMaterialSummary(
    layout As CanvasLayout
) As MaterialSummary
    ' Sum quantities by material
    ' Apply unit prices
    ' Return cost breakdown
End Function

Public Function ExportToCsv(
    result As TakeOffResult
) As String
    ' Format result as CSV
    ' Include headers and data rows
End Function
```

**Related Use Cases:**
- UC-004: Run a take-off quantity summary (primary)
- UC-003: Attach a Smart Tag to an object

**Example:**

```vb
' Usage
Dim service = New TakeOffService()

' Aggregate by material tag
Dim summary = service.AggregateByTag(
    layout,
    "Material",
    AggregateFunction.Sum
)

' Export to CSV
Dim csv = service.ExportToCsv(summary)
```

---

### 3. MaterialService

**Purpose:** Material management and lookup service.

**Key Responsibilities:**

- **Material Lookup** — Find material by ID or name
- **Price Retrieval** — Get current unit price for material
- **Material List** — List all available materials (for dropdowns)
- **Price History** — Retrieve historical prices (future)

**Key Methods:**

```vb
Public Function GetMaterial(materialId As String) As Material
    ' Look up material from repository
    ' Return material object
End Function

Public Function GetUnitPrice(
    materialId As String,
    asOfDate As DateTime
) As Double
    ' Retrieve price as of date
    ' Handle price history
End Function

Public Function ListAllMaterials() As List(Of Material)
    ' Return all materials for UI binding
End Function
```

**Related Use Cases:**
- UC-003: Attach a Smart Tag to an object (material tag types)
- UC-004: Run a take-off quantity summary (pricing lookup)

---

### 4. TakeOffContext

**Purpose:** Contextual parameters for calculation execution.

**Structure:**

```vb
Public Class TakeOffContext
    ''' <summary>Unit system (metric, imperial)</summary>
    Public Property UnitSystem As String

    ''' <summary>Whether to apply formula code overrides</summary>
    Public Property ApplyFormulaOverrides As Boolean

    ''' <summary>Filters to apply during calculation</summary>
    Public Property LayerFilter As List(Of String)          ' Only calc on these layers
    Public Property ObjectTypeFilter As List(Of String)     ' Only calc these types

    ''' <summary>Rounding precision</summary>
    Public Property RoundingPrecision As Integer            ' Decimal places

    ''' <summary>Exchange rate for currency conversion (future)</summary>
    Public Property ExchangeRate As Double
End Class
```

**Key Points:**

- **Parameterizes** calculation behavior
- Enables **filtering** (by layer, object type)
- Supports **unit conversion** (metric ↔ imperial)
- **Reusable** across multiple calculation requests

**Example:**

```vb
' Calculate only walls on the "Exterior" layer
Dim ctx = New TakeOffContext With {
    .UnitSystem = "metric",
    .LayerFilter = New List(Of String) From {"Exterior"},
    .RoundingPrecision = 2
}
Dim result = calculator.Calculate(layout, ctx)
```

---

### 5. TakeOffResult

**Purpose:** Aggregation result object — holds calculation outputs.

**Structure:**

```vb
Public Class TakeOffResult
    ''' <summary>Aggregated quantities by material</summary>
    Public Property MaterialSummary As Dictionary(Of String, MaterialQuantity)

    ''' <summary>Costs by layer</summary>
    Public Property CostByLayer As Dictionary(Of String, Double)

    ''' <summary>Grand total cost</summary>
    Public Property GrandTotalCost As Double

    ''' <summary>Warnings or validation issues</summary>
    Public Property Warnings As List(Of String)

    ''' <summary>Calculation timestamp</summary>
    Public Property CalculatedAt As DateTime
End Class

Public Class MaterialQuantity
    Public Property Material As String
    Public Property Quantity As Double
    Public Property Unit As String
    Public Property UnitPrice As Double
    Public Property TotalCost As Double
End Class
```

**Key Points:**

- **Immutable result** — represents calculation snapshot
- Contains **detailed breakdown** — by material, layer, etc.
- **Warnings included** — validation issues don't block calculation
- **Exportable** — can be serialized to CSV/Excel

---

## 🔄 Data Flow

### Calculation Pipeline

```
CanvasLayout (from storage)
    ↓
TakeOffCalculator.Calculate(layout, context)
    ├─ For each CanvasElement:
    │   ├─ Extract BusinessDefinition
    │   ├─ Determine dimension mode (D0-D3)
    │   ├─ Get geometry dimensions (from Geometry utility)
    │   ├─ Check relationships (nested objects)
    │   ├─ Apply quantity formula
    │   ├─ Multiply by unit price
    │   └─ Accumulate in result
    │
    └─ Return TakeOffResult
        ├─ MaterialSummary (by tag)
        ├─ CostByLayer (by layer)
        ├─ GrandTotalCost
        └─ Warnings
```

### Example: Two-Room Layout

**Input:**

```
CanvasLayout:
  ├─ Room A (Rectangle, D2, area=50m², Material="Concrete", Price=20€/m²)
  ├─ Door A1 (Rectangle, D0, count=1, Subtracts from Room A, Material="Wood", Price=100€)
  ├─ Room B (Rectangle, D2, area=30m², Material="Tile", Price=15€/m²)
  └─ Door B1 (Rectangle, D0, count=1, Subtracts from Room B, Material="Wood", Price=100€)
```

**Calculation:**

```
Room A:
  - Geometry area = 50 m²
  - Doors subtract = 2 m² (typical door)
  - Net area = 48 m²
  - Cost = 48 m² × 20 €/m² = 960 €

Room B:
  - Geometry area = 30 m²
  - Doors subtract = 2 m²
  - Net area = 28 m²
  - Cost = 28 m² × 15 €/m² = 420 €

Doors (Wood):
  - Count = 2
  - Cost = 2 × 100 € = 200 €
```

**Output (TakeOffResult):**

```
MaterialSummary:
  "Concrete": { Qty=48, Unit="m²", Price=20, Total=960 }
  "Tile":     { Qty=28, Unit="m²", Price=15, Total=420 }
  "Wood":     { Qty=2,  Unit="count", Price=100, Total=200 }

CostByLayer:
  "Walls": 960 + 420 = 1380 €
  "Doors": 200 €

GrandTotalCost: 1580 €

Warnings: []
```

---

## 🧪 Service Interfaces

### ICalculationEngine (Interface Contract)

```vb
Public Interface ICalculationEngine
    Function Calculate(
        layout As CanvasLayout,
        context As TakeOffContext
    ) As TakeOffResult
End Interface
```

**Implementation:** TakeOffCalculator

### ITakeOffService (Interface Contract)

```vb
Public Interface ITakeOffService
    Function AggregateByTag(
        layout As CanvasLayout,
        tagName As String,
        aggregateFunc As AggregateFunction
    ) As Dictionary(Of String, Double)

    Function GetMaterialSummary(layout As CanvasLayout) As MaterialSummary

    Function ExportToCsv(result As TakeOffResult) As String
End Interface
```

**Implementation:** TakeOffService

---

## 🏗️ Layering Pattern

### Dependency Direction

```
Desktop (UI)
    ↓ uses
Application (Orchestration)
    ↓ uses
Domain (Entities, Calculation utilities)
    ↓ uses
Infrastructure (IO, Config, Logging)
```

**Key Rule:** Lower layers **never** depend on upper layers.

### Dependency Injection

Services are injected via constructor:

```vb
Public Class TakeOffService
    Private ReadOnly _materialService As IMaterialService
    Private ReadOnly _logger As ILogger

    Public Sub New(materialService As IMaterialService, logger As ILogger)
        _materialService = materialService
        _logger = logger
    End Sub
End Class
```

**Benefits:**
- ✅ Testable (mock dependencies)
- ✅ Loosely coupled (interface-based)
- ✅ Flexible (swap implementations)

---

## 🧪 Testing Considerations

### Unit Tests

- **TakeOffCalculator** — Test quantity calculations per dimension mode
- **TakeOffService** — Test aggregation and export formatting
- **MaterialService** — Test lookup and filtering

### Integration Tests

- **Full Pipeline** — CanvasLayout → Calculate → TakeOffResult
- **Nested Objects** — Verify parent-child quantity logic
- **CSV Export** — Verify formatting and data integrity

### Mock Dependencies

```vb
' Mock IMaterialService
Dim mockMaterialService = New Mock(Of IMaterialService)
mockMaterialService.Setup(
    Function(m) m.GetMaterial("MAT001")
).Returns(New Material With {.Id = "MAT001", .Name = "Concrete"})

' Inject into service
Dim service = New TakeOffService(mockMaterialService.Object)
```

---

## 🔗 References

### Mega-File Documentation

- [0301-Development_Documentation](../Mega-File.md#-0301--development-documentation) — Coding standards
- [020103-Data_Model](../Mega-File.md#-020103--data-model) — Entity schemas
- [0104-SRS §5](../Mega-File.md#-functional-requirements) — Functional requirements
- [UC-004: Run a take-off quantity summary](../Mega-File.md#uc004--run-a-take-off-quantity-summary-) — Main use case

### Related Layers

- **Domain** — Provides entities (CanvasLayout, BusinessDefinition, etc.)
- **Desktop** — Consumes services and results for UI binding
- **Infrastructure** — Provides persistence and configuration

---

## 📝 Conventions

### Naming

- Service classes use **Service suffix** (TakeOffService, MaterialService)
- Service interfaces use **IService prefix** (ITakeOffService, IMaterialService)
- Result classes use **Result suffix** (TakeOffResult, MaterialQuantity)
- Methods use **PascalCase** with **verb-first naming** (Calculate, Aggregate, Export)

### Exception Handling

- **ArgumentException** — Invalid parameters
- **InvalidOperationException** — Invalid state
- **ApplicationException** — Business logic errors

```vb
If layout Is Nothing Then
    Throw New ArgumentNullException(NameOf(layout))
End If

If Not layout.Elements.Any() Then
    Throw New InvalidOperationException("Canvas has no elements")
End If
```

### Logging

- Log **entry/exit** of main calculate methods (Info level)
- Log **warnings** when quantity calculations result in zero or negative values
- Log **exceptions** with full stack trace (Error level)

```vb
_logger.Log(LogLevel.Info, $"Calculating quantities for {layout.Elements.Count} elements")
_logger.Log(LogLevel.Warn, $"Zero quantity calculated for element {element.Id}")
_logger.Log(LogLevel.Error, $"Calculation failed: {ex.Message}")
```

---

## ⚠️ Important Notes

### No UI Logic

❌ Do NOT add:
- Windows.Forms references
- Display formatting (use Infrastructure/Wrappers)
- Event handlers

✅ Keep Application layer:
- Pure orchestration
- Business calculations
- Service interfaces

### Performance Considerations

- **Caching** — Cache material lookups within a calculate call
- **Lazy Loading** — Don't load all materials upfront
- **Batch Operations** — Process multiple elements in a single call

---

## 🚀 Quick Reference

### Create Context and Calculate

```vb
Dim ctx = New TakeOffContext With {
    .UnitSystem = "metric",
    .ApplyFormulaOverrides = True,
    .RoundingPrecision = 2
}

Dim calculator = New TakeOffCalculator()
Dim result = calculator.Calculate(layout, ctx)

Console.WriteLine($"Grand Total: {result.GrandTotalCost}")
```

### Aggregate and Export

```vb
Dim service = New TakeOffService()

' Get summary
Dim summary = service.AggregateByTag(layout, "Material", AggregateFunction.Sum)

' Export to CSV
Dim csv = service.ExportToCsv(result)
System.IO.File.WriteAllText("output.csv", csv)
```

---

**Last Updated:** January 2025  
**Layer Responsibility:** Use Case Orchestration & Calculation  
**Maintainer:** Development Team
