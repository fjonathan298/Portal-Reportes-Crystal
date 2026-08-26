-- ==========================================================================
-- LOV_Monedas.sql
-- Proposito:     Lista de monedas activas para seleccion unica
-- Parametro:     {?ID_MONEDA} (Number)
-- Dependencia:   Ninguna
-- Retorna:       CODIGO (VARCHAR) = ID de moneda
--                DESCRIPCION (VARCHAR) = codigo ISO + nombre
-- Ejemplo:       1 | USD - Dolar Estadounidense
--                2 | GTQ - Quetzal
-- ==========================================================================

SELECT CAST(M.ID_MONEDA AS VARCHAR(50))                    AS CODIGO,
       M.CODIGO_ISO + ' - ' + M.NOMBRE_MONEDA              AS DESCRIPCION
FROM   DIM_MONEDA M
WHERE  M.ACTIVO = 1
ORDER  BY M.CODIGO_ISO;
