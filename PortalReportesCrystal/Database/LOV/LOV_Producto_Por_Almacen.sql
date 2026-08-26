-- ==========================================================================
-- LOV_Producto_Por_Almacen.sql
-- Proposito:     Lista de productos filtrados por almacen, con opcion TODOS
-- Parametro:     {?Codigo_Producto} (String, multi-seleccion)
-- Dependencia:   {?Almacen} (String) - debe evaluarse antes que este LOV
--                Cuando {?Almacen} = 'TODOS', este LOV muestra todos los
--                productos del pais (requiere {?ID_PAIS} como filtro
--                adicional o que la vista incluya el pais).
-- Retorna:       CODIGO (VARCHAR) = codigo del producto o 'TODOS'
--                DESCRIPCION (VARCHAR) = nombre/descripcion del producto
-- Ejemplo:       TODOS  | Todos los Productos
--                P00123 | Filtro de aceite 10W-40
--                P00456 | Bujia NGK BKR6E
--
-- Nota: este LOV maneja el caso donde Almacen = 'TODOS' usando la misma
-- logica del patron WHERE unificado dentro del propio LOV.
-- ==========================================================================

SELECT 'TODOS'                                      AS CODIGO,
       'Todos los Productos'                        AS DESCRIPCION
UNION ALL
SELECT DISTINCT
       CAST(P.CODIGO_PRODUCTO AS VARCHAR(50))       AS CODIGO,
       P.DESCRIPCION_PRODUCTO                       AS DESCRIPCION
FROM   DIM_PRODUCTO P
       INNER JOIN FACT_INVENTARIO I
           ON P.ID_PRODUCTO = I.ID_PRODUCTO
       INNER JOIN DIM_ALMACEN A
           ON I.ID_ALMACEN = A.ID_ALMACEN
WHERE  ('TODOS' IN ({?Almacen})
        OR CAST(A.CODIGO_ALMACEN AS VARCHAR(50)) IN ({?Almacen}))
  AND  P.ACTIVO = 1
ORDER  BY 1;
