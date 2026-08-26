-- ==========================================================================
-- LOV_Vendedor.sql
-- Proposito:     Lista de vendedores activos con opcion TODOS
-- Parametro:     {?Vendedor} (String, multi-seleccion)
-- Dependencia:   Ninguna
-- Retorna:       CODIGO (VARCHAR) = codigo del vendedor o 'TODOS'
--                DESCRIPCION (VARCHAR) = nombre del vendedor
-- Ejemplo:       TODOS | Todos los Vendedores
--                V001  | Juan Perez
--                V002  | Maria Lopez
-- ==========================================================================

SELECT 'TODOS'                                  AS CODIGO,
       'Todos los Vendedores'                   AS DESCRIPCION
UNION ALL
SELECT CAST(V.CODIGO_VENDEDOR AS VARCHAR(50))   AS CODIGO,
       V.NOMBRE_VENDEDOR                        AS DESCRIPCION
FROM   DIM_VENDEDOR V
WHERE  V.ACTIVO = 1
ORDER  BY 1;
