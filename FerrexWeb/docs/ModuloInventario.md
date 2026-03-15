# Módulo de Inventario por Ciudades y Puntos

## Descripción General

Sistema completo de inventario para el panel de administración que permite gestionar **Ciudades**, **Puntos de Inventario** por ciudad, y el **stock de productos** en cada punto. Incluye seguimiento de movimientos (entradas, salidas, ajustes) y transferencias entre puntos.

**Convención del proyecto:** Variables, propiedades, tablas y columnas en **inglés**. Etiquetas de UI en **español**.

---

## Arquitectura

```
Admin.razor (/admin)
  └── Card "Inventario"
        └── AdminInventoryDashboard.razor (/admin/inventory)
              ├── AdminCities.razor (/admin/cities)
              │     └── Ver puntos por ciudad → AdminInventoryPoints
              ├── AdminInventoryPoints.razor (/admin/inventorypoints)
              │     ├── /admin/inventorypoints          (todos)
              │     └── /admin/inventorypoints/{CityId}  (filtrado)
              │           └── Ver inventario → AdminInventory
              └── AdminInventory.razor (/admin/inventory/{PointId})
                    ├── Agregar producto
                    ├── Editar stock (entrada/salida/ajuste)
                    ├── Transferir entre puntos
                    └── Historial de movimientos
```

---

## Modelos de Datos

### City (`Models/City.cs`)

Representa una ciudad donde existen puntos de inventario.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | `int` [Key] | Clave primaria |
| `Name` | `string` [Required, StringLength(100)] | Nombre de la ciudad. Ej: "San Pedro Sula" |
| `Department` | `string?` [StringLength(50)] | Departamento (opcional). Ej: "Cortés" |
| `IsActive` | `bool` | Si la ciudad está activa. Default: `true` |
| `CreatedDate` | `DateTime` | Fecha de creación. Default: `DateTime.Now` |
| `InventoryPoints` | `ICollection<InventoryPoint>` | Puntos de inventario en esta ciudad |

---

### InventoryPoint (`Models/InventoryPoint.cs`)

Representa un punto/bodega/sucursal dentro de una ciudad.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | `int` [Key] | Clave primaria |
| `Name` | `string` [Required, StringLength(150)] | Nombre del punto. Ej: "Colonia Moderna" |
| `Address` | `string?` [StringLength(255)] | Dirección física (opcional) |
| `Phone` | `string?` [StringLength(50)] | Teléfono de contacto (opcional) |
| `CityId` | `int` [Required] | FK → `City.Id` |
| `IsActive` | `bool` | Si el punto está activo. Default: `true` |
| `CreatedDate` | `DateTime` | Fecha de creación. Default: `DateTime.Now` |
| `City` | `City` | Navegación al padre |
| `Items` | `ICollection<InventoryItem>` | Productos en este punto |
| `TotalProducts` | `int` [NotMapped] | Computed: `Items.Count` |
| `LowStockCount` | `int` [NotMapped] | Computed: items donde `Quantity <= MinStock` |

---

### InventoryItem (`Models/InventoryItem.cs`)

Representa un producto específico en un punto de inventario con su stock.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | `int` [Key] | Clave primaria |
| `InventoryPointId` | `int` [Required] | FK → `InventoryPoint.Id` |
| `ProductId` | `int` [Required] | FK → `Products.IdProducto` |
| `Quantity` | `int` | Stock actual. Default: `0` |
| `MinStock` | `int` | Umbral de alerta bajo stock. Default: `5` |
| `MaxStock` | `int?` | Límite máximo (opcional) |
| `LastUpdated` | `DateTime` | Última modificación. Default: `DateTime.Now` |
| `Notes` | `string?` [StringLength(255)] | Notas opcionales |
| `InventoryPoint` | `InventoryPoint` | Navegación al punto |
| `Product` | `Products` | Navegación al producto existente |
| `Movements` | `ICollection<InventoryMovement>` | Historial de movimientos |
| `IsLowStock` | `bool` [NotMapped] | `Quantity <= MinStock` |
| `StockStatus` | `string` [NotMapped] | `"Sin Stock"` / `"Bajo Stock"` / `"Normal"` |

**Índice único:** `(InventoryPointId, ProductId)` — un producto solo puede existir una vez por punto.

---

### InventoryMovement (`Models/InventoryMovement.cs`)

Registro de auditoría de cada cambio de stock.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | `int` [Key] | Clave primaria |
| `InventoryItemId` | `int` [Required] | FK → `InventoryItem.Id` |
| `MovementType` | `int` | Tipo de movimiento (ver enum abajo) |
| `Quantity` | `int` | Cantidad del movimiento |
| `PreviousQuantity` | `int` | Stock antes del movimiento |
| `NewQuantity` | `int` | Stock después del movimiento |
| `MovementDate` | `DateTime` | Fecha/hora del movimiento |
| `Description` | `string?` [StringLength(255)] | Razón o descripción |
| `UserId` | `int?` | FK → `User.Id` (quién realizó el cambio) |
| `TransferFromPointId` | `int?` | Punto origen (solo en transferencias) |
| `TransferToPointId` | `int?` | Punto destino (solo en transferencias) |
| `InventoryItem` | `InventoryItem` | Navegación al item |
| `User` | `User?` | Navegación al usuario |

#### Enum MovementType

```csharp
public enum MovementType
{
    Entry = 0,        // Entrada de stock
    Exit = 1,         // Salida de stock
    Adjustment = 2,   // Ajuste directo (establece cantidad)
    TransferIn = 3,   // Recepción por transferencia
    TransferOut = 4   // Envío por transferencia
}
```

---

## Base de Datos

### DbContext (`Services/ApplicationDbContext.cs`)

Se agregaron 4 `DbSet`:

```csharp
public DbSet<City> Cities { get; set; }
public DbSet<InventoryPoint> InventoryPoints { get; set; }
public DbSet<InventoryItem> InventoryItems { get; set; }
public DbSet<InventoryMovement> InventoryMovements { get; set; }
```

### Relaciones configuradas en `OnModelCreating`

| Relación | Tipo | Configuración |
|----------|------|---------------|
| `City` → `InventoryPoint` | 1:N | `City.InventoryPoints` ↔ `InventoryPoint.CityId` |
| `InventoryPoint` → `InventoryItem` | 1:N | `InventoryPoint.Items` ↔ `InventoryItem.InventoryPointId` |
| `InventoryItem` → `Products` | N:1 | `InventoryItem.ProductId` → `Products` |
| `InventoryItem` → `InventoryMovement` | 1:N | `InventoryItem.Movements` ↔ `InventoryMovement.InventoryItemId` |
| `InventoryMovement` → `User` | N:1 | `InventoryMovement.UserId` → `User` (opcional) |

### Índices

| Tabla | Columna(s) | Tipo |
|-------|-----------|------|
| `InventoryItems` | `(InventoryPointId, ProductId)` | Único |
| `InventoryItems` | `InventoryPointId` | Performance |
| `InventoryItems` | `ProductId` | Performance |
| `InventoryMovements` | `InventoryItemId` | Performance |
| `InventoryMovements` | `MovementDate` | Performance |
| `InventoryPoints` | `CityId` | Performance |

### Migración

- **Nombre:** `AddInventoryModule`
- **Estado:** Creada y aplicada a la base de datos Azure (`FerrexpressHN`)

---

## Servicio (`Services/InventoryService.cs`)

Registrado como **Scoped** en `Program.cs`:

```csharp
builder.Services.AddScoped<InventoryService>();
```

### Métodos disponibles

#### Ciudades

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetAllCitiesAsync()` | `List<City>` | Todas las ciudades con sus puntos, ordenadas por nombre |
| `GetCityByIdAsync(int id)` | `City?` | Ciudad por ID con puntos incluidos |
| `AddCityAsync(City city)` | `void` | Crear nueva ciudad |
| `UpdateCityAsync(City city)` | `void` | Actualizar nombre, departamento, estado |
| `DeleteCityAsync(int id)` | `bool` | Eliminar ciudad. Retorna `false` si tiene puntos asociados |

#### Puntos de Inventario

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetAllPointsAsync()` | `List<InventoryPoint>` | Todos los puntos con ciudad e items |
| `GetPointsByCityIdAsync(int cityId)` | `List<InventoryPoint>` | Puntos filtrados por ciudad |
| `GetPointByIdAsync(int id)` | `InventoryPoint?` | Punto con ciudad, items y productos |
| `AddPointAsync(InventoryPoint point)` | `void` | Crear nuevo punto |
| `UpdatePointAsync(InventoryPoint point)` | `void` | Actualizar punto |
| `DeletePointAsync(int id)` | `bool` | Eliminar punto. Retorna `false` si tiene items |

#### Items de Inventario

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetItemsByPointIdAsync(int pointId)` | `List<InventoryItem>` | Items de un punto con producto y ciudad |
| `GetItemByIdAsync(int id)` | `InventoryItem?` | Item con producto, punto, ciudad y movimientos |
| `ProductExistsInPointAsync(int pointId, int productId)` | `bool` | Validar duplicados |
| `AddItemAsync(InventoryItem item, int? userId)` | `void` | Agregar producto al punto. Si `Quantity > 0`, registra movimiento de entrada inicial |
| `UpdateStockAsync(int itemId, int movementType, int quantity, string? description, int? userId)` | `void` | Actualizar stock con registro de movimiento |
| `DeleteItemAsync(int id)` | `bool` | Eliminar item y todos sus movimientos |

#### Movimientos

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetMovementsByItemIdAsync(int itemId)` | `List<InventoryMovement>` | Movimientos de un item, ordenados por fecha desc |

#### Transferencias

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `TransferStockAsync(int sourceItemId, int destinationPointId, int quantity, string? description, int? userId)` | `void` | Transferir stock entre puntos. Usa **transacción DB**. Crea item en destino si no existe |

**Lógica de transferencia:**
1. Abre transacción
2. Resta cantidad del item origen → registra movimiento `TransferOut`
3. Busca item en destino (mismo producto):
   - Si existe: suma cantidad → registra movimiento `TransferIn`
   - Si no existe: crea nuevo item → registra movimiento `TransferIn`
4. Commit de transacción
5. Si falla: rollback automático

#### Dashboard

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetLowStockItemsAsync()` | `List<InventoryItem>` | Items donde `Quantity <= MinStock`, con producto y punto |
| `GetTotalInventoryQuantityAsync()` | `int` | Suma total de stock en todos los puntos |
| `GetActivePointsCountAsync()` | `int` | Cantidad de puntos activos |
| `GetActiveCitiesCountAsync()` | `int` | Cantidad de ciudades activas |
| `GetLowStockAlertCountAsync()` | `int` | Cantidad de items con bajo stock |

#### Helpers

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `SearchProductsAsync(string search)` | `List<Products>` | Buscar productos por nombre o código (máx 20 resultados) |

---

## Páginas Razor

### 1. AdminInventoryDashboard (`/admin/inventory`)

**Archivo:** `Pages/AdminInventoryDashboard.razor`

Vista general del inventario con:
- **4 cards resumen:** Ciudades Activas, Puntos de Inventario, Stock Total, Alertas Bajo Stock
- **Tabla de alertas:** Lista de productos con bajo stock mostrando Ciudad, Punto, Código, Producto, Cantidad, Mínimo, Estado
- **Botones de navegación:** "Gestionar Ciudades" y "Gestionar Puntos"

**Autorización:** `SuperAdmin`

---

### 2. AdminCities (`/admin/cities`)

**Archivo:** `Pages/AdminCities.razor`

CRUD completo de ciudades:
- **Tabla:** Nombre, Departamento, Puntos (badge), Estado, Acciones
- **Búsqueda:** Por nombre o departamento en tiempo real
- **Ordenamiento:** Por Nombre o Departamento (asc/desc)
- **Paginación:** 10 items por página
- **Modales:**
  - Agregar ciudad (nombre, departamento, activa)
  - Editar ciudad
  - Eliminar ciudad (validación: no permite si tiene puntos asociados)
- **Navegación:** Botón "Ver Puntos" navega a `/admin/inventorypoints/{CityId}`

**Autorización:** `SuperAdmin`

---

### 3. AdminInventoryPoints (`/admin/inventorypoints` y `/admin/inventorypoints/{CityId}`)

**Archivo:** `Pages/AdminInventoryPoints.razor`

CRUD completo de puntos de inventario:
- **Tabla:** Nombre, Ciudad, Dirección, Teléfono, Productos (badge), Bajo Stock (badge rojo), Estado, Acciones
- **Filtro por ciudad:** Dropdown que filtra por ciudad. Si se navega con `{CityId}`, se preselecciona
- **Búsqueda:** Por nombre, dirección o ciudad
- **Ordenamiento:** Por Nombre o Ciudad
- **Paginación:** 10 items por página
- **Modales:**
  - Agregar punto (nombre, ciudad, dirección, teléfono, activo)
  - Editar punto
  - Eliminar punto (validación: no permite si tiene productos)
- **Navegación:** Botón "Ver Inventario" navega a `/admin/inventory/{PointId}`

**Autorización:** `SuperAdmin`

---

### 4. AdminInventory (`/admin/inventory/{PointId}`)

**Archivo:** `Pages/AdminInventory.razor`

Gestión completa del inventario de un punto específico:

#### Header
- Breadcrumb: Inventario > Puntos > [Nombre del punto]
- Nombre del punto y ciudad

#### Cards resumen
- Total Productos, Stock Total, Bajo Stock, Sin Stock

#### Tabla de inventario
- **Columnas:** Código, Producto, Cantidad, Stock Mínimo, Estado (badge), Última Actualización, Acciones
- **Filtro por estado:** Todos / Normal / Bajo Stock / Sin Stock
- **Búsqueda:** Por nombre o código de producto
- **Paginación:** 15 items por página

#### Modales

**Agregar Producto:**
- Búsqueda de productos existentes (mínimo 2 caracteres)
- Lista seleccionable de resultados
- Campos: cantidad inicial, stock mínimo
- Validación: no permite duplicados en el mismo punto

**Editar Stock:**
- Muestra stock actual
- Tipo de movimiento: Entrada (+), Salida (-), Ajuste (=)
- Preview del nuevo stock
- Campo de descripción/razón
- Registra movimiento en historial

**Transferir Stock:**
- Dropdown de puntos destino (excluye punto actual, solo activos)
- Muestra: `Ciudad — Nombre del punto`
- Cantidad a transferir (máximo = stock disponible)
- Campo de descripción
- Validación de stock suficiente

**Historial de Movimientos:**
- Modal grande (modal-lg)
- Tabla con scroll: Fecha, Tipo (badge color), Cantidad, Anterior, Nuevo, Descripción, Usuario
- Ordenado por fecha descendente

**Eliminar Producto:**
- Confirmación con advertencia de eliminación de movimientos

**Autorización:** `SuperAdmin`

---

## Navegación

### Desde el Panel de Administración (`/admin`)

Se agregó una card con icono `fa-warehouse`:

```
Inventario
Gestiona el inventario por ciudades y puntos
[Ver Inventario] → /admin/inventory
```

### Flujo de navegación

```
/admin
  └── /admin/inventory (Dashboard)
        ├── /admin/cities (Gestión de Ciudades)
        │     └── /admin/inventorypoints/{CityId} (Puntos por ciudad)
        ├── /admin/inventorypoints (Todos los Puntos)
        │     └── /admin/inventory/{PointId} (Inventario del punto)
        └── Alertas → /admin/inventory/{PointId} (click en alerta)
```

---

## Archivos del Módulo

### Archivos nuevos (8)

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `Models/City.cs` | Modelo | Entidad Ciudad |
| `Models/InventoryPoint.cs` | Modelo | Entidad Punto de Inventario |
| `Models/InventoryItem.cs` | Modelo | Entidad Item de Inventario |
| `Models/InventoryMovement.cs` | Modelo | Entidad Movimiento + Enum MovementType |
| `Services/InventoryService.cs` | Servicio | Lógica de negocio completa |
| `Pages/AdminInventoryDashboard.razor` | Página | Dashboard de inventario |
| `Pages/AdminCities.razor` | Página | CRUD de ciudades |
| `Pages/AdminInventoryPoints.razor` | Página | CRUD de puntos |
| `Pages/AdminInventory.razor` | Página | Gestión de stock por punto |

### Archivos modificados (3)

| Archivo | Cambio |
|---------|--------|
| `Services/ApplicationDbContext.cs` | 4 DbSets + relaciones + índices en `OnModelCreating` |
| `Program.cs` | Registro de `InventoryService` como Scoped |
| `Pages/Admin.razor` | Card de navegación + método `IrAInventario()` |

### Migración

| Archivo | Descripción |
|---------|-------------|
| `Migrations/*_AddInventoryModule.cs` | Migración EF Core aplicada |

---

## Reglas de Negocio

1. **No se puede eliminar una ciudad** que tenga puntos de inventario asociados
2. **No se puede eliminar un punto** que tenga productos en inventario
3. **Un producto solo puede existir una vez** por punto (índice único)
4. **Las transferencias son atómicas** (transacción DB): si falla alguna parte, no se modifica nada
5. **Cada cambio de stock** genera un registro de movimiento con auditoría (quién, cuándo, stock anterior/nuevo)
6. **Stock mínimo por defecto:** 5 unidades
7. **Alertas de bajo stock:** cuando `Quantity <= MinStock`
8. **Al agregar un producto** con cantidad > 0, se registra automáticamente un movimiento de tipo "Entrada" con descripción "Stock inicial"
9. **Al eliminar un item**, se eliminan en cascada todos sus movimientos
10. **Salidas de stock** no pueden dejar el stock por debajo de 0 (`Math.Max(0, ...)`)

---

## Patrones de UI

Todas las páginas siguen los patrones establecidos en el proyecto:

- **Autorización:** `@attribute [Authorize(Roles = "SuperAdmin")]`
- **Modales Bootstrap:** `modal-backdrop show` + `modal fade show` con `display: block`
- **Formularios:** `EditForm` con `DataAnnotationsValidator` y `ValidationSummary`
- **Búsqueda:** Input con icono de lupa y botón de limpiar
- **Paginación:** Skip/Take con botones de página
- **Ordenamiento:** Click en headers con iconos `fa-sort-up`/`fa-sort-down`
- **Loading:** Animación de 3 círculos
- **Badges de estado:** `bg-success` (Normal/Activo), `bg-warning` (Bajo Stock), `bg-danger` (Sin Stock/Inactivo)
- **Responsive:** Media queries para móviles con layout vertical
- **Estilos:** CSS scoped dentro de cada componente con `<style>`
