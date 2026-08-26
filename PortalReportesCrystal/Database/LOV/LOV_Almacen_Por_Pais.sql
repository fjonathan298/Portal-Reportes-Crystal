-- ==========================================================================
-- LOV_Almacen_Por_Pais.sql
-- Proposito:     Lista de almacenes filtrados por pais, con opcion TODOS
-- Parametro:     {?Almacen} (String, multi-seleccion)
-- Dependencia:   {?ID_PAIS} (Number) - debe evaluarse antes que este LOV
-- Retorna:       CODIGO (VARCHAR) = codigo del almacen o 'TODOS'
--                DESCRIPCION (VARCHAR) = nombre del almacen
-- Ejemplo:       TODOS | Todos los Almacenes
--                06    | Almacen Central SV
--                07    | Almacen Zona Norte
--
-- Indice requerido para performance:
--   CREATE NONCLUSTERED INDEX IX_DIM_ALMACEN_ID_PAIS
--       ON DIM_ALMACEN(ID_PAIS)
--       INCLUDE (CODIGO_ALMACEN, NOMBRE_ALMACEN)
--       WHERE ACTIVO = 1;
-- ==========================================================================

SELECT 'TODOS'                                  AS CODIGO,
       'Todos los Almacenes'                    AS DESCRIPCION
UNION ALL
SELECT CAST(A.CODIGO_ALMACEN AS VARCHAR(50))    AS CODIGO,
       A.NOMBRE_ALMACEN                         AS DESCRIPCION
FROM   DIM_ALMACEN A
WHERE  A.ID_PAIS = {?ID_PAIS}
  AND  A.ACTIVO = 1
ORDER  BY 1;
