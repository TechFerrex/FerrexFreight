# Carga Inicial de Inventario - Colonia Moderna

## Fecha: 2026-03-14

## Resumen

Se cargo el inventario inicial del punto **Colonia Moderna** (San Pedro Sula) a partir del archivo Excel `inventario ferrexpress 26.xlsx`. Los productos del inventario son accesorios **PVC y CPVC**.

---

## Datos de Entrada

| Archivo | Descripcion |
|---------|-------------|
| `FERR26_normalizado_v3.xlsx` | Productos normalizados ya en BD (513 productos) |
| `inventario ferrexpress 26.xlsx` | Inventario sin normalizar (87 lineas, accesorios PVC/CPVC) |

---

## Proceso de Carga

### Paso 1: Matching (cruce de datos)

Se cruzo cada item del inventario con los productos existentes en la BD usando matching manual por nombre, tipo y tamano.

**Resultado del cruce:**

| Categoria | Items | Unidades |
|-----------|-------|----------|
| Matched (existian en BD) | 56 | 725 |
| Unmatched (productos nuevos) | 29 | 174 |
| Excluido (ambiguo) | 1 | 5 |
| **Total cargado** | **85** | **899** |

**Item excluido:** "adaptador hembra sin campana pvc" - no especificaba tamano.

### Paso 2: Creacion de 29 productos nuevos

Productos que no existian en la BD y fueron creados (Cat:4, Sub:33, Precio: L.0.00):

| Id | Producto | Tipo |
|----|----------|------|
| 514 | YEE DWV INYECTADO DRENAJE 6" | PVC Drenaje |
| 515 | TEE 6 LISA PVC POTABLE | PVC Potable |
| 516 | ADAPT MACHO 6 PVC | PVC |
| 517 | CODO POTABLE 6X90 PVC | PVC Potable |
| 518 | TAPON LISO 6 PVC POTABLE | PVC Potable |
| 519 | UNION LISA 6 PVC POTABLE | PVC Potable |
| 520 | YEE DWV INYECTADO DRENAJE 4" | PVC Drenaje |
| 521 | YEE DWV INYECTADO DRENAJE 3" | PVC Drenaje |
| 522 | YEE DWV INYECTADO DRENAJE 2" | PVC Drenaje |
| 523 | YEE 2 LISA PVC POTABLE | PVC Potable |
| 524 | ABRAZADERA PVC 4" A 3/4" | PVC |
| 525 | SIFON 2" PVC DRENAJE INY | PVC Drenaje |
| 526 | TAPON CON ROSCA HEMBRA 2" PVC | PVC |
| 527 | BUJE REDUCTOR PVC 1-1/4"X3/4" | PVC |
| 528 | TAPON 2-1/2" PVC | PVC |
| 529 | REDUCTOR DRENAJE TUBO DWV INYECTADO 6X4 | PVC Drenaje |
| 530 | BUJE REDUCTOR PVC 6"X2" | PVC |
| 531 | TAPON CON ROSCA HEMBRA 1/2" PVC | PVC |
| 532 | CODO CPVC 3/4"X45 | CPVC |
| 533 | UNION CPVC 3/4" | CPVC |
| 534 | TAPON CPVC 3/4" | CPVC |
| 535 | CODO CPVC 1/2"X90 | CPVC |
| 536 | CODO CPVC 1/2"X45 | CPVC |
| 537 | TAPON CPVC 1/2" | CPVC |
| 538 | TEE CPVC 1/2" | CPVC |
| 539 | ADAPTADOR CPVC 1/2" | CPVC |
| 540 | UNION CPVC 1/2" | CPVC |
| 541 | ADAPTADOR HEMBRA CPVC 3/4" | CPVC |
| 542 | REDUCTOR CPVC 3/4"X1/2" | CPVC |

> **Nota:** Estos 29 productos quedaron con precio L.0.00. Deben actualizarse.

### Paso 3: Insercion de inventario

Se insertaron 85 registros en `InventoryItems` para el InventoryPoint Id=1 (Colonia Moderna).

### Paso 4: Movimientos iniciales

Se creo un `InventoryMovement` tipo "Entrada" (MovementType=0) por cada item, con descripcion "Carga inicial de inventario".

---

## Glosario de Matching PVC

Terminologia del inventario informal vs BD normalizada:

| Termino Inventario | Significado en BD |
|-------------------|-------------------|
| inyectado / inyectada / iny | DRENAJE INY / DWV INYECTADO |
| potable / potble | POTABLE o LISA (tamanos chicos) |
| campana | REDUCTOR DRENAJE TUBO DWV (bell-end) |
| reductor X a Y | BUJE REDUCTOR PVC Y"XX" |
| yee | YEE (Y-fitting) - no existia en BD |
| tapon macho | TAPON LISO DRENAJE INY |
| tapon de rosca | TAPON CON ROSCA |
| sifon | SIFON (P-trap) |
| abrazadera | ABRAZADERA (saddle clamp) |

---

## Agregaciones

Dos items del Excel mapeaban al mismo producto:

| Items originales | Producto BD | Cantidad total |
|-----------------|-------------|----------------|
| "tapon macho 4 pvc" (23) + "tapon de 4 inyectado pvc" (20) | [251] TAPON LISO 4 PVC DRENAJE INY | 43 |

---

## Scripts Utilizados

Los scripts de Python usados para el proceso estan en `C:\Users\ThinkPad\Desktop\ExcelParaBD\`:

| Script | Funcion |
|--------|---------|
| `match_inventory.py` | Matching: cruza items del Excel con productos de BD |
| `insert_inventory.py` | Ejecucion: crea productos nuevos, inserta inventario y movimientos |

---

## Estado Final de la BD

| Metrica | Valor |
|---------|-------|
| Productos totales en BD | 542 |
| Items en inventario Colonia Moderna | 85 |
| Unidades totales en stock | 899 |
| Punto Plantel (Choloma) | Sin inventario cargado |
