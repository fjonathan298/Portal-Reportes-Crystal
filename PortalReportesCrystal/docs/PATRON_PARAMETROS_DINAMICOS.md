# Patron Estandar de Parametros Dinamicos para Crystal Reports

## 1. Objetivo

Definir un estandar unico de parametrizacion dinamica aplicable a cualquier
reporte Crystal del portal (~316 reportes SV/HN/GT) y del inventario legacy
(1,555 .rpt en produccion). El estandar cubre tres tipos de filtro que se
repiten en la mayoria de reportes:

1. **Seleccion unica obligatoria** (ej. Pais, Compania, Moneda).
2. **Seleccion multiple con opcion "TODOS"** (ej. Almacen, Producto, Centro de Costo).
3. **Filtros en cascada** (Almacen depende de Pais; Producto depende de Almacen).

El reporte final solo recibe parametros ya resueltos. Toda la logica de
listas de valores y la convencion WHERE vive en este framework compartido,
no dentro de cada Command SQL individual.

---

## 2. Convencion de Nomenclatura

| Tipo | Convencion | Ejemplos |
|------|-----------|----------|
| Seleccion unica obligatoria (FK numerica) | `{?ID_<Entidad>}` | `{?ID_PAIS}`, `{?ID_COMPANIA}`, `{?ID_MONEDA}` |
| Seleccion multiple con TODOS | `{?<Entidad>}` PascalCase | `{?Almacen}`, `{?Codigo_Producto}`, `{?Centro_Costo}`, `{?Vendedor}` |
| Rango de fecha (siempre en pares) | `{?Fecha_Desde}` y `{?Fecha_Hasta}` | Siempre estos nombres exactos |
| Booleanos | `{?Incluir_<Cosa>}` | `{?Incluir_Anulados}`, `{?Incluir_Devoluciones}` |

### Prohibido

- Nombres genericos: `{?Param1}`, `{?P}`, `{?a}`.
- Nombres con espacios, acentos o mayusculas irregulares.
- Un revisor debe entender el proposito del parametro leyendo solo su nombre.

---

## 3. Listas de Valores Dinamicas (LOV Commands)

Cada parametro consume una Lista de Valores basada en un Command SQL
estandarizado. Los Commands se guardan en `Database/LOV/` como archivos
`.sql` versionados.

### 3.1 Contrato de Retorno

Todo Command LOV retorna **exactamente 2 columnas**, en este orden:

```
CODIGO       VARCHAR(50)   -- valor que se envia al parametro
DESCRIPCION  VARCHAR(200)  -- texto que ve el usuario en el selector
```

`CODIGO` siempre se emite como `VARCHAR`, aunque el campo real sea numerico
(usar `CAST`). Esto permite que el valor literal `'TODOS'` conviva con
codigos reales sin errores de tipo.

### 3.2 Convencion de Nombres de Commands

| Tipo | Patron | Ejemplo |
|------|--------|---------|
| Lista simple | `LOV_<Entidad>` | `LOV_Paises`, `LOV_Monedas` |
| Lista en cascada | `LOV_<Entidad>_Por_<Padre>` | `LOV_Almacen_Por_Pais`, `LOV_Producto_Por_Almacen` |

### 3.3 Prefijado de "TODOS"

Los Commands de tipo multi-seleccion (tipo 2) emiten el registro
`'TODOS' / 'Todos los <entidad>'` como **primera fila**, para que aparezca
arriba en el selector y sea el default cuando el usuario no seleccione nada.

```sql
SELECT 'TODOS' AS CODIGO, 'Todos los Almacenes' AS DESCRIPCION
UNION ALL
SELECT CAST(A.CODIGO_ALMACEN AS VARCHAR(50)) AS CODIGO,
       A.NOMBRE_ALMACEN AS DESCRIPCION
FROM   DIM_ALMACEN A
WHERE  A.ID_PAIS = {?ID_PAIS}
ORDER  BY 1
```

---

## 4. Cascada entre Parametros

### 4.1 Alternativas Evaluadas

| # | Alternativa | Ventaja | Desventaja |
|---|------------|---------|------------|
| A | Command dependiente puro (LOV con `{?ID_PAIS}` en WHERE) | Simple, comportamiento nativo Crystal | Cada cambio re-ejecuta LOV en BD |
| B | Vista materializada / catalogo intermedio | LOV consulta vista pequena precalculada | Requiere job de refresh |
| C | Clave compuesta (sin cascada visual) | Sin cascada = sin lentitud | UX pobre: mezcla todos los valores |

### 4.2 Decision

**Alternativa A (Command dependiente) + indice de soporte.**

Preserva la UX nativa de Crystal. El requisito de performance se cumple con
un indice cubriente en el catalogo de dimensiones:

```sql
CREATE NONCLUSTERED INDEX IX_DIM_ALMACEN_ID_PAIS
    ON DIM_ALMACEN(ID_PAIS)
    INCLUDE (CODIGO_ALMACEN, NOMBRE_ALMACEN)
    WHERE ACTIVO = 1;
```

### 4.3 Fallback para Tablas Grandes

Cuando la tabla de dimension supere 500K filas o este fuera del DW, crear
vista materializada con `SCHEMABINDING` + indice unico, refrescada por job
diario. Se registra como "opcion de rendimiento", no como default.

---

## 5. Formato "TODOS" y Patron WHERE Unificado

### 5.1 Regla Clave

**Todos los parametros multi-seleccion son de tipo `String`** en la
definicion Crystal, aunque el campo real sea numerico. Esto permite que
`'TODOS'` conviva con codigos reales.

### 5.2 Patron WHERE Estandarizado

```sql
WHERE
    -- Tipo 1: filtro obligatorio, seleccion unica
    T.ID_PAIS = {?ID_PAIS}

    -- Tipo 2: filtro multi-seleccion con TODOS
    AND ('TODOS' IN ({?Almacen})
         OR CAST(T.CODIGO_ALMACEN AS VARCHAR(50)) IN ({?Almacen}))

    AND ('TODOS' IN ({?Codigo_Producto})
         OR CAST(T.CODIGO_PRODUCTO AS VARCHAR(50)) IN ({?Codigo_Producto}))

    -- Tipo 3: rango de fecha
    AND T.FECHA BETWEEN {?Fecha_Desde} AND {?Fecha_Hasta}
```

### 5.3 Por que Funciona

1. `IN ({?Param})` funciona porque Crystal expande el multi-valor automaticamente.
2. `CAST(<campo> AS VARCHAR(50))` unifica la comparacion cuando el campo es numerico.
3. `'TODOS' IN (...)` actua como short-circuit: si el usuario dejo "TODOS", no se
   evalua la segunda parte contra el campo real.
4. SQL Server optimiza `IN (constante, constante, ...)` como serie de OR.
5. Para listas > 1000 valores, considerar TVP o tabla temporal (excepcion documentada).

### 5.4 Anti-Patrones Prohibidos

| Anti-patron | Por que no funciona |
|-------------|-------------------|
| `IIF({?Param} = "TODOS", ...)` | No funciona con multi-valor |
| Reemplazar parametro por concatenacion en formula Crystal | Impide push-down al servidor |
| Hardcodear la lista de valores dentro del Command del reporte | Imposible de mantener |
| Usar `LIKE '%'` en vez de `'TODOS' IN (...)` | Falsos positivos, no funciona con codigos numericos |

---

## 6. Principio: Reportes que Solo Reciben Parametros Resueltos

El Command SQL del **reporte** no resuelve la logica de parametrizacion;
solo la consume. Toda la logica vive en:

- Los LOV Commands (listas de valores).
- El patron WHERE unificado.
- Las vistas de soporte (opcional, para cascada rapida).

**Beneficio**: agregar un pais, almacen o producto nuevo es un cambio de
datos (`INSERT INTO DIM_*`), no requiere tocar ningun `.rpt`.

---

## 7. Seguridad

- Los LOV **nunca** reciben input directo del usuario sin pasar por
  parametro Crystal -- evita inyeccion SQL.
- `{?ID_PAIS}` como `Number` en Crystal rechaza strings; el motor bloquea
  intentos de inyeccion.
- Los LOV se conectan al DW con usuario de servicio con `SELECT` unicamente
  en las dimensiones (minimo privilegio).
- Cambios al catalogo de LOV pasan por revision y control de cambios (mismo
  flujo que un reporte productivo).

---

## 8. Caso Demostrativo: Kardex de Inventario

Parametros del Kardex aplicando el estandar:

| Parametro | Tipo | LOV Command |
|-----------|------|-------------|
| `{?ID_PAIS}` | Seleccion unica obligatoria | `LOV_Paises` |
| `{?Almacen}` | Multi-seleccion con TODOS | `LOV_Almacen_Por_Pais` (depende de `{?ID_PAIS}`) |
| `{?Codigo_Producto}` | Multi-seleccion con TODOS | `LOV_Producto_Por_Almacen` (depende de `{?Almacen}`) |
| `{?Fecha_Desde}` | Fecha inicio | N/A (calendario nativo) |
| `{?Fecha_Hasta}` | Fecha fin | N/A (calendario nativo) |

**WHERE del Command del Kardex**:
```sql
WHERE
    K.ID_PAIS = {?ID_PAIS}
    AND ('TODOS' IN ({?Almacen})
         OR CAST(K.CODIGO_ALMACEN AS VARCHAR(50)) IN ({?Almacen}))
    AND ('TODOS' IN ({?Codigo_Producto})
         OR CAST(K.CODIGO_PRODUCTO AS VARCHAR(50)) IN ({?Codigo_Producto}))
    AND K.FECHA BETWEEN {?Fecha_Desde} AND {?Fecha_Hasta}
```

**Nota sobre cascada Producto -> Almacen**: si el usuario elige `TODOS` en
Almacen, el LOV `LOV_Producto_Por_Almacen` cae automaticamente a mostrar
todos los productos del pais (por el `'TODOS' IN` en el LOV padre). Si se
necesita una lista independiente, usar `LOV_Producto_Por_Pais` como
fallback configurado en Crystal.

---

## 9. Checklist de Revision para .rpt Nuevos o Migrados

Antes de publicar un reporte que adopte el estandar, verificar:

- [ ] Los nombres de parametros siguen la convencion de la seccion 2.
- [ ] Los LOV son Commands referenciados por nombre, no formulas embebidas.
- [ ] El contrato de retorno del LOV es exactamente 2 columnas: `CODIGO`, `DESCRIPCION`.
- [ ] Los LOV multi-seleccion incluyen fila `'TODOS'` como primera opcion.
- [ ] El WHERE aplica el patron unificado de la seccion 5.2.
- [ ] No hay `IIF`/formulas Crystal que rompan push-down al servidor.
- [ ] Se probo con `TODOS` seleccionado (devuelve datos sin filtro de ese campo).
- [ ] Se probo con seleccion parcial (devuelve solo lo seleccionado).
- [ ] Al cambiar un filtro padre, el LOV hijo se actualiza correctamente.
- [ ] Los indices de soporte existen en las tablas de dimension consultadas.
- [ ] El parametro no almacena input libre del usuario (solo valores del LOV).

---

## 10. Biblioteca de LOV Disponibles

Los archivos SQL estan en `Database/LOV/`:

| Archivo | Proposito | Dependencia |
|---------|----------|-------------|
| `LOV_Paises.sql` | Lista de paises activos | Ninguna |
| `LOV_Monedas.sql` | Lista de monedas activas | Ninguna |
| `LOV_Centro_Costo.sql` | Centros de costo con TODOS | Ninguna |
| `LOV_Vendedor.sql` | Vendedores activos con TODOS | Ninguna |
| `LOV_Almacen_Por_Pais.sql` | Almacenes filtrados por pais, con TODOS | `{?ID_PAIS}` |
| `LOV_Producto_Por_Almacen.sql` | Productos filtrados por almacen, con TODOS | `{?Almacen}` |

Para contribuir un LOV nuevo, seguir el contrato de la seccion 3.1 y
agregar el archivo a `Database/LOV/` con el header estandar.
