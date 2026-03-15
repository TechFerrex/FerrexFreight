# Mejoras Responsive del Modulo de Inventario

## Problema

Las tablas del modulo de inventario solo hacian scroll horizontal en movil, dificultando la lectura. Los headers, filtros y modales no se adaptaban bien a pantallas pequenas.

---

## Solucion: Mobile Card View

Se implemento un patron CSS que transforma las filas de tabla en **tarjetas apiladas** en pantallas < 768px.

### Patron Tecnico

1. Agregar `data-label` a cada `<td>`:
```razor
<td data-label="Ciudad">@item.City</td>
<td data-label="Cantidad"><strong>@item.Quantity</strong></td>
<td data-label="">  <!-- vacio para columna de acciones -->
    <div class="action-buttons">...</div>
</td>
```

2. Agregar clase `mobile-cards` a la tabla:
```razor
<table class="table table-hover mobile-cards">
```

3. CSS que transforma filas en tarjetas:
```css
@@media (max-width: 767.98px) {
    .mobile-cards thead { display: none; }
    .mobile-cards tbody tr {
        display: block;
        margin-bottom: 0.75rem;
        border: 1px solid #dee2e6;
        border-radius: 0.5rem;
        padding: 0.75rem;
        background: #fff;
        box-shadow: 0 1px 3px rgba(0,0,0,0.06);
    }
    .mobile-cards tbody td {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.3rem 0;
        border: none;
        font-size: 0.9rem;
    }
    .mobile-cards tbody td::before {
        content: attr(data-label);
        font-weight: 600;
        color: #495057;
        margin-right: 1rem;
        flex-shrink: 0;
    }
    .mobile-cards tbody td:last-child {
        justify-content: flex-end;
        padding-top: 0.5rem;
        border-top: 1px solid #eee;
        margin-top: 0.25rem;
    }
    .mobile-cards tbody td:last-child::before { display: none; }
}
```

---

## Paginas Modificadas

| Pagina | Tabla(s) afectada(s) |
|--------|---------------------|
| `AdminInventoryDashboard.razor` | Alertas de bajo stock |
| `AdminInventory.razor` | Productos en punto + Historial de movimientos (modal) |
| `AdminInventoryPoints.razor` | Lista de puntos de inventario |
| `AdminCities.razor` | Lista de ciudades |

---

## Mejoras por Breakpoint

### Movil (< 768px)
- Tablas → tarjetas apiladas con labels
- Headers: titulo + botones apilados verticalmente, botones 100% ancho
- Search box y filtros (dropdown ciudades): ancho completo
- Summary cards: grid 2x2 (`col-6`)
- Paginacion centrada con wrap
- Modales con margenes reducidos
- Font sizes reducidos

### Tablet (768px - 992px)
- Titulos y botones con tamano intermedio
- Filtros del card-header se apilan para no comprimir la busqueda

---

## Fix del Navbar (MainLayout.razor)

Se cambio `.sticky-nav` de `position: fixed` a `position: sticky`:

```css
/* Antes: requeria margin-top fijo que se desincronizaba */
.sticky-nav { position: fixed; top: 0; }
main { margin-top: 150px; }

/* Despues: el nav ocupa espacio natural, no requiere margin-top */
.sticky-nav { position: sticky; top: 0; background: #fff; }
```

Esto elimina el problema de contenido cortado detras del navbar en cualquier pantalla.
