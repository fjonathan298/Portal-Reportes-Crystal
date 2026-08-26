-- ==========================================================================
-- LOV_Centro_Costo.sql
-- Proposito:     Lista de centros de costo con opcion TODOS
-- Parametro:     {?Centro_Costo} (String, multi-seleccion)
-- Dependencia:   Ninguna
-- Retorna:       CODIGO (VARCHAR) = codigo del centro de costo o 'TODOS'
--                DESCRIPCION (VARCHAR) = nombre descriptivo
-- Ejemplo:       TODOS | Todos los Centros de Costo
--                CC001 | Administracion Central
--                CC002 | Ventas Region Metropolitana
-- ==========================================================================

SELECT 'TODOS'                                  AS CODIGO,
       'Todos los Centros de Costo'             AS DESCRIPCION
UNION ALL
SELECT CAST(CC.CODIGO_CENTRO AS VARCHAR(50))    AS CODIGO,
       CC.NOMBRE_CENTRO                         AS DESCRIPCION
FROM   DIM_CENTRO_COSTO CC
WHERE  CC.ACTIVO = 1
ORDER  BY 1;
