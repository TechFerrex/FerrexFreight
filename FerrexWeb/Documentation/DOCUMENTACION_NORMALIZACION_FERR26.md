# Documentacion - Normalizacion FERR26 e Insercion en BD FerrexpressHN

**Fecha:** 1 de marzo de 2026
**Autor:** Generado con Claude Code
**Proyecto:** Ferrexpress - Carga inicial de productos

---

## 1. Objetivo

Normalizar los datos del archivo **FERR26.xlsx** (lista de precios de proveedores) al formato estructurado de **revision materiales v1.xlsx** (formato de base de datos), para luego insertar todos los registros en la base de datos Azure SQL **FerrexpressHN**.

---

## 2. Archivos de Entrada

### 2.1 revision materiales v1.xlsx
- **Ubicacion:** `C:\Users\ThinkPad\desktop\excel\revision materiales v1.xlsx`
- **Funcion:** Archivo de referencia con la estructura normalizada destino.
- **Hojas:** Hoja2 (catalogo de subcategorias), Estructura Metalica, Cementos agregados y pegamento, Techos, PVC Potable y Drenaje, Electricidad, Tabla Yeso y Perfileria, Pintura, Consumible y Complementario, Equipos Proteccion, Tornilleria Especializada, Productos En General.
- **Columnas normalizadas:**

| Columna | Descripcion |
|---------|-------------|
| CODIGO PRODUCTO | Identificador unico del producto |
| PRODUCTO | Nombre completo del producto |
| Categoria | ID numerico de la categoria (1-10) |
| SubCategoria | ID numerico de la subcategoria |
| SubCategoria2 | Nombre de la sub-subcategoria (texto) |
| Tipo | Tipo/variante del producto |
| Tamano | Dimensiones o medidas |
| UNIDAD | Unidad de venta (Unidad, Saco, Bolsa, Rollo, etc.) |
| VALOR MARG/MONTO | Precio de venta |
| image | URL de la imagen del producto |

### 2.2 FERR26.xlsx
- **Ubicacion:** `C:\Users\ThinkPad\desktop\TESTFERREX\TestFreight\FERR26.xlsx`
- **Funcion:** Lista de precios actualizada de proveedores 2026, sin normalizar.
- **Hojas y formato:**

| Hoja | Filas | Formato |
|------|-------|---------|
| cementos,hierro,tub.pvc | 676 | DESCRIPCION, PRECIO COSTO, PRECIO VENTA. Datos agrupados por secciones con encabezados intercalados (CEMENTOS, MORTEROS TORO, WCS, TBA, CANALETA, ALUZINC, VARILLAS, TUBERIA GALVANIZADA, TUBERIA PVC, ACCESORIOS PVC, PEGAMENTOS). Algunos productos con codigo, otros sin codigo. |
| linea ferretera,elec | 69 | #, Codigo proveedor, Descripcion, Cantidad, Cod.UM, Precio Unitario, Total, Precio Venta. Celdas combinadas (merged). |
| electrico y herramientas | 81 | Formato similar: #, Codigo, Descripcion, Cantidad, UM, Costo, Total, Venta. |
| elec y pvc | 42 | Codigo, Descripcion, Cantidad, UM, Costo, Total formateado, Venta. |
| pvc segundo precio y spray | 42 | Codigo, Descripcion, Cantidad, UM, Costo, Total, Venta. |
| tornilleria | 21 | Codigo, Descripcion, Cantidad, UM, Costo, Total, Precio venta/unidad. |
| mat.pend cotizar | 260 | Codigo (C-XXXXXX), Descripcion. Sin precios (pendientes de cotizar). |

**Problemas identificados en FERR26:**
- No tiene categorias ni subcategorias asignadas
- Secciones mezcladas con encabezados como filas de datos
- Celdas combinadas (merged cells) en algunas hojas
- Saltos de linea dentro de celdas de descripcion
- Formatos de columna inconsistentes entre hojas
- Productos sin codigo
- Productos con precio 0 o sin precio

---

## 3. Proceso de Normalizacion

### 3.1 Script de normalizacion
- **Archivo:** `C:\Users\ThinkPad\desktop\normalizar_ferr26.py`
- **Lenguaje:** Python 3.13 con openpyxl

### 3.2 Logica de clasificacion

El script clasifica cada producto asignando Categoria, SubCategoria, SubCategoria2, Tipo, Tamano y Unidad mediante analisis de:

1. **Hoja de origen** - determina la categoria general
2. **Seccion actual** - para la hoja 1, detecta encabezados de seccion (CEMENTOS, VARILLAS, etc.)
3. **Prefijo del codigo del proveedor** - para hojas con codigo:
   - `ACE` = Accesorios Electricos
   - `CCB` = Centros de Carga / Breakers
   - `CTR` = Cercas / Mallas
   - `HER` = Herramientas
   - `HOG` = Hogar
   - `ILU` = Iluminacion
   - `PVC` = PVC
   - `TOR` = Tornilleria
   - `SPR` = Spray
   - `BSG` = Bisagras
   - `ELE` = Electrico
   - `C-` = Productos pendientes de cotizar
4. **Palabras clave en la descripcion** - clasificacion por contenido (CEMENTO, VARILLA, TUBO, CODO, TEE, BREAKER, LAMPARA, etc.)
5. **Extraccion de tamano** - patrones regex para dimensiones (ej: `2X4`, `1/2"`, `0.40MM`, `42.kg`, `100W`, etc.)

### 3.3 Asignacion de imagenes

Se reutilizan las URLs de imagen existentes en revision materiales v1, con el patron:
```
/images/product/{CategoriaID}/{nombre}.webp
```

Productos del mismo tipo comparten la misma imagen. Productos sin imagen equivalente quedan con el campo vacio (NULL).

### 3.4 Filtrado

Se eliminaron todos los productos con precio de venta = 0 o NULL (202 productos eliminados, incluyendo todos los de la hoja "mat.pend cotizar").

---

## 4. Archivo de Salida

- **Archivo:** `C:\Users\ThinkPad\desktop\excel\FERR26_normalizado_v3.xlsx`
- **Hojas:**
  - **Productos Normalizados** - 513 productos con las 10 columnas normalizadas
  - **Categorias** - Catalogo de referencia (ID, Nombre)
  - **SubCategorias** - Catalogo de referencia (ID, Nombre)

### 4.1 Resumen de productos por categoria

| Cat ID | Categoria | Productos |
|--------|-----------|-----------|
| 1 | Estructura Metalica | 65 |
| 2 | Cementos, agregados y pegamento | 44 |
| 3 | Techos | 77 |
| 4 | PVC, Potable y Drenaje | 188 |
| 5 | Electricidad | 74 |
| 7 | Pintura | 6 |
| 8 | Consumible y Complementario | 242 |
| 10 | Tornilleria Especializada | 19 |
| **Total** | | **513** |

### 4.2 Resumen de imagenes

- 362 productos con imagen asignada
- 151 productos sin imagen (categorias sin imagenes definidas: Electricidad, Pintura, Equipos Proteccion, Tornilleria, y algunos de Consumible)
- 28 imagenes unicas reutilizadas

---

## 5. Insercion en Base de Datos

### 5.1 Conexion

| Parametro | Valor |
|-----------|-------|
| Servidor | ferrexpresshn.database.windows.net |
| Base de datos | FerrexpressHN |
| Autenticacion | SQL Server (usuario/contrasena) |
| Herramienta | sqlcmd (linea de comandos) |

### 5.2 Estructura de tablas

```
Categories (ID identity, Nombre, Description, ImageUrl, IsActive)
    |
    +-- Subcategory (id_subcategory identity, id_categories FK, name, imageCat)
            |
            +-- Subcategory2 (id_subcategory2 identity, id_subcategory FK, name, ImageCat)

Image (id_image identity, url, title)

Products (ID identity, Codigo, Product, Types, Size, Precio, CategoriaID, Unit,
          id_subcategory FK, id_subcategory2 FK, id_image FK->Image)
```

### 5.3 Orden de insercion (por dependencias FK)

1. **Categories** (10 registros) - `SET IDENTITY_INSERT ON` para controlar IDs exactos (1-10)
2. **Subcategory** (95 registros) - IDs controlados, FK a Categories.ID
3. **Subcategory2** (48 registros) - IDs controlados, FK a Subcategory.id_subcategory
4. **Image** (28 registros) - IDs controlados
5. **Products** (513 registros) - IDs auto-generados, FKs mapeados por script Python

### 5.4 Script de insercion de productos
- **Archivo:** `C:\Users\ThinkPad\desktop\insert_products.sql`
- Generado automaticamente por Python
- Inserta en lotes de 50 registros
- Mapea `id_subcategory2` y `id_image` usando diccionarios de lookup

### 5.5 Resultado final en BD

| Tabla | Registros insertados |
|-------|---------------------|
| Categories | 10 |
| Subcategory | 95 |
| Subcategory2 | 48 |
| Image | 28 |
| Products | 513 |

---

## 6. Catalogo de Categorias

| ID | Nombre |
|----|--------|
| 1 | Estructura Metalica |
| 2 | Cementos, agregados y pegamento |
| 3 | Techos |
| 4 | PVC, Potable y Drenaje |
| 5 | Electricidad |
| 6 | Tabla Yeso y Perfileria |
| 7 | Pintura |
| 8 | Consumible y Complementario |
| 9 | Equipos Proteccion |
| 10 | Tornilleria Especializada |

---

## 7. Catalogo de SubCategorias

| ID | Cat | Nombre |
|----|-----|--------|
| 1 | 1 | Accesorios HG |
| 2 | 1 | Alambre de amarre |
| 3 | 1 | Angulo |
| 4 | 1 | Lamina de Hierro |
| 5 | 1 | Malla |
| 6 | 1 | Platina |
| 7 | 1 | Rollo Serpentina |
| 8 | 1 | Tela Mosquitera |
| 9 | 1 | Tuberia HG |
| 10 | 1 | Tuberia HN |
| 11 | 1 | Tubo Industrial |
| 12 | 1 | Varilla |
| 13 | 1 | Viga |
| 15 | 2 | Adhesivo |
| 16 | 2 | Arenas |
| 17 | 2 | BaseCoat |
| 18 | 2 | Bloque |
| 19 | 2 | Cemento |
| 20 | 2 | Fraguado |
| 21 | 2 | Hydroflex |
| 22 | 2 | Monocapa |
| 23 | 2 | Pulido |
| 24 | 2 | Revestimiento Hidrofugo |
| 25 | 2 | Texturizado |
| 26 | 2 | Plywoof Fenolico |
| 27 | 3 | Accesorios Techos |
| 28 | 3 | Aislantes Termicos |
| 29 | 3 | Canaleta HG |
| 30 | 3 | Laminas Techo |
| 31 | 4 | Accesorios Complementarios |
| 32 | 4 | CPVC |
| 33 | 4 | Potable/Drenaje |
| 34 | 6 | Accesorios Complementarios Tabla Yeso |
| 35 | 6 | Angulos y Refuerzos |
| 36 | 6 | Laminas |
| 37 | 7 | Aislador |
| 38 | 7 | Anticorrosivo |
| 39 | 7 | Brocha |
| 40 | 7 | Cromatic Latex |
| 41 | 7 | Desoxidante/Desegrasante Nit |
| 42 | 7 | Diluyente |
| 43 | 7 | Fastyl |
| 44 | 7 | Masking tape |
| 45 | 7 | Rodillo |
| 46 | 7 | Spray |
| 47 | 7 | Thiner |
| 48 | 8 | Acido Muriatico |
| 49 | 8 | Aldaba |
| 50 | 8 | Almadana |
| 51 | 8 | Barra de Acero |
| 52 | 8 | Bisagra |
| 53 | 8 | Cabo |
| 54 | 8 | Caja de Clavos |
| 55 | 8 | Candado |
| 56 | 8 | Carreta |
| 57 | 8 | Cepillo |
| 58 | 8 | Ceramica |
| 59 | 8 | Cerradura |
| 60 | 8 | Cinta |
| 61 | 8 | CLAVO |
| 62 | 8 | Disco Conico Alambre |
| 63 | 8 | Disco Corte |
| 64 | 8 | Disco Desbaste |
| 65 | 8 | Disco Diamantado |
| 66 | 8 | Disco Estacional/Madera |
| 67 | 8 | Electrodo |
| 68 | 8 | Espansor |
| 69 | 8 | Esponja |
| 70 | 8 | Guante |
| 71 | 8 | Lampara |
| 72 | 8 | Lija |
| 73 | 8 | Llamador |
| 74 | 8 | Llavin |
| 75 | 8 | Pala |
| 76 | 8 | Pasador |
| 77 | 8 | Pegamento |
| 78 | 8 | Pistola |
| 79 | 8 | Pulidora |
| 80 | 8 | Remache |
| 81 | 8 | Rollo |
| 82 | 8 | Saco |
| 83 | 8 | Segueta |
| 84 | 8 | Separador |
| 85 | 8 | Silicon |
| 86 | 8 | Taladro |
| 87 | 8 | Sikaflex |
| 88 | 8 | Tape |
| 89 | 9 | Casco |
| 90 | 9 | Chaleco |
| 91 | 9 | Lentes |
| 92 | 10 | Arandela |
| 93 | 10 | Perno |
| 94 | 10 | Perno con Tuerca y Arandela |
| 95 | 10 | Tornillo R/G G5 |
| 96 | 10 | Tuerca |

---

## 8. Catalogo de Imagenes

| ID | URL | Usada por |
|----|-----|-----------|
| 1 | /images/product/1/tubocuadradohg.webp | Tubos cuadrados HG |
| 2 | /images/product/1/tuborectangularhg.webp | Tubos rectangulares HG |
| 3 | /images/product/1/varillacorrugada.webp | Varillas corrugadas |
| 4 | /images/product/1/varillalisa.webp | Varillas lisas |
| 5 | /images/product/2/argos.webp | Cemento Argos |
| 6 | /images/product/2/bijao.webp | Cementos (Bijao, generico) |
| 7 | /images/product/2/fraguadobase.webp | Fraguados |
| 8 | /images/product/2/sacoadhesivotoro.webp | Adhesivos y morteros |
| 9 | /images/product/2/sacobasecoat.webp | BaseCoat |
| 10 | /images/product/2/sacohidrofugo.webp | Revestimiento hidrofugo |
| 11 | /images/product/2/sacomonocapa.webp | Monocapa |
| 12 | /images/product/2/sacopulelisto.webp | Pulido y masillas |
| 13 | /images/product/3/laminaalunatural.webp | Laminas aluzinc natural |
| 14 | /images/product/3/laminaaluroja.webp | Laminas aluzinc roja |
| 15 | /images/product/4/adaptadorlisoH.webp | Adaptadores macho PVC |
| 16 | /images/product/4/adaptadorpvch.webp | Adaptadores hembra PVC |
| 17 | /images/product/4/codopvc45.webp | Codos PVC 45 grados |
| 18 | /images/product/4/codopvc90.webp | Codos PVC 90 grados |
| 19 | /images/product/4/pegamentotangit.webp | Pegamentos PVC |
| 20 | /images/product/4/reductorpvc.webp | Bujes reductores PVC |
| 21 | /images/product/4/taponpvcliso.webp | Tapones PVC |
| 22 | /images/product/4/teepvc.webp | Tees PVC |
| 23 | /images/product/4/tubopvc.webp | Tubos PVC |
| 24 | /images/product/4/unionpvclisa.webp | Uniones lisas PVC |
| 25 | /images/product/4/unionpvcrosca.webp | Uniones reparacion PVC |
| 26 | /images/product/4/unionuniversalpvc.webp | Uniones universales PVC |
| 27 | /images/product/8/bisagrasoldar.webp | Bisagras |
| 28 | /images/product/8/electrodo6011.webp | Electrodos |

---

## 9. Archivos Generados

| Archivo | Descripcion |
|---------|-------------|
| `excel/FERR26_normalizado_v3.xlsx` | Excel final normalizado con 513 productos |
| `normalizar_ferr26.py` | Script Python de normalizacion |
| `insert_products.sql` | Script SQL de insercion de productos |
| `excel/DOCUMENTACION_NORMALIZACION_FERR26.md` | Este documento |

---

## 10. Notas y Consideraciones

1. **Productos sin codigo:** Los productos de la hoja "cementos,hierro,tub.pvc" que no tenian codigo del proveedor quedaron con el campo Codigo vacio en la BD.
2. **Productos excluidos:** Se excluyeron 202 productos con precio de venta = 0 (incluyendo todos los de "mat.pend cotizar").
3. **Categorias 6 y 9:** No se insertaron productos en estas categorias porque FERR26 no contenia productos de Tabla Yeso ni Equipos de Proteccion con precio.
4. **Imagenes compartidas:** Multiples productos del mismo tipo comparten la misma imagen (ej: 73 laminas aluzinc natural usan la misma imagen).
5. **Subcategorias sin uso:** Se insertaron las 95 subcategorias del catalogo completo (de revision materiales v1), aunque no todas tienen productos de FERR26 asociados. Esto permite agregar productos futuros sin modificar el catalogo.
6. **Identity IDs:** Se uso `SET IDENTITY_INSERT ON` para Categories, Subcategory, Subcategory2 e Image, asegurando que los IDs coincidan con los del archivo de referencia. Para Products se dejo el identity automatico.
7. **Alambre de puas:** Clasificado bajo Cat 1 > SubCat 2 (Alambre de amarre) con tipo "Puas".
8. **Lamparas:** Clasificadas bajo Cat 5 (Electricidad) pero referenciando SubCat 71 (Lampara) que esta catalogada bajo Cat 8. La FK de Products apunta directamente a id_subcategory sin pasar por la relacion Categories-Subcategory.
