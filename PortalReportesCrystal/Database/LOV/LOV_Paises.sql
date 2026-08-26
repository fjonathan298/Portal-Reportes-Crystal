-- ==========================================================================
-- LOV_Paises.sql
-- Proposito:     Lista de paises activos para seleccion unica obligatoria
-- Parametro:     {?ID_PAIS} (Number)
-- Dependencia:   Ninguna
-- Retorna:       CODIGO (VARCHAR) = ID numerico del pais
--                DESCRIPCION (VARCHAR) = nombre del pais
-- Ejemplo:       1 | El Salvador
--                2 | Honduras
--                3 | Guatemala
-- ==========================================================================

SELECT CAST(P.ID_PAIS AS VARCHAR(50))   AS CODIGO,
       P.NOMBRE_PAIS                    AS DESCRIPCION
FROM   DIM_PAIS P
WHERE  P.ACTIVO = 1
ORDER  BY P.NOMBRE_PAIS;
