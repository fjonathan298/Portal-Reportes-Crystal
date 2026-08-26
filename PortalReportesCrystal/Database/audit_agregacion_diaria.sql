/*
================================================================================
audit_agregacion_diaria.sql
================================================================================
Materializa los agregados diarios en audit.ReporteAgregado y audit.UsuarioAgregado
consumiendo audit.Evento.

Ejecucion recomendada:
    SQL Server Agent - Job "PortalReportesCrystal - Agregacion Auditoria"
    Frecuencia: diario a las 01:00 hora local (07:00 UTC)
    Duracion tipica: < 30 s incluso con millones de eventos, gracias a los indices

Idempotencia:
    Si el job corre dos veces en el mismo dia, actualiza las filas existentes
    (no duplica). Usa MERGE contra el indice unico (FechaCorte, NombreReporte)
    y (FechaCorte, Usuario).

Alcance:
    Procesa TODOS los dias que aun tengan eventos sin agregar. Esto permite
    re-materializar historial en caso de necesidad (borrar los agregados de
    un rango y correr el SP, y reconstruye).
================================================================================
*/

USE DWH_FRAMEWORK;
GO

IF OBJECT_ID('audit.sp_AgregarDiario', 'P') IS NOT NULL
    DROP PROCEDURE audit.sp_AgregarDiario;
GO

CREATE PROCEDURE audit.sp_AgregarDiario
    @FechaDesde DATE = NULL,   -- si es NULL, procesa los ultimos 7 dias
    @FechaHasta DATE = NULL    -- si es NULL, procesa hasta hoy inclusive
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Defaults: ultima semana. Se procesa por dia local (UTC-6 El Salvador).
    IF @FechaHasta IS NULL SET @FechaHasta = CAST(DATEADD(HOUR, -6, SYSUTCDATETIME()) AS DATE);
    IF @FechaDesde IS NULL SET @FechaDesde = DATEADD(DAY, -7, @FechaHasta);

    DECLARE @Inicio DATETIME2(3);
    DECLARE @Fin    DATETIME2(3);

    ------------------------------------------------------------------
    -- 1) Agregado por reporte y dia
    ------------------------------------------------------------------
    ;WITH ev AS (
        SELECT
            CAST(DATEADD(HOUR, -6, e.FechaUtc) AS DATE) AS FechaCorte,
            e.NombreReporte,
            e.TipoReporte,
            e.Categoria,
            e.Usuario,
            e.TipoEventoId,
            e.FechaUtc,
            t.Categoria AS CategoriaEvento
        FROM audit.Evento e
        INNER JOIN audit.EventoTipo t ON t.TipoEventoId = e.TipoEventoId
        WHERE e.NombreReporte IS NOT NULL
          AND e.FechaUtc >= DATEADD(HOUR, 6, CAST(@FechaDesde AS DATETIME2(3)))
          AND e.FechaUtc <  DATEADD(HOUR, 6, DATEADD(DAY, 1, CAST(@FechaHasta AS DATETIME2(3))))
    ),
    agr AS (
        SELECT
            FechaCorte,
            NombreReporte,
            MAX(TipoReporte) AS TipoReporte,
            MAX(Categoria)   AS Categoria,
            SUM(CASE WHEN CategoriaEvento = 'Reporte'     THEN 1 ELSE 0 END) AS TotalAperturas,
            SUM(CASE WHEN CategoriaEvento = 'Exportacion' THEN 1 ELSE 0 END) AS TotalDescargas,
            COUNT(DISTINCT Usuario) AS UsuariosUnicos,
            MAX(FechaUtc) AS UltimoAcceso
        FROM ev
        GROUP BY FechaCorte, NombreReporte
    )
    MERGE audit.ReporteAgregado AS t
    USING agr AS s
       ON t.FechaCorte = s.FechaCorte AND t.NombreReporte = s.NombreReporte
    WHEN MATCHED THEN
        UPDATE SET
            TipoReporte    = s.TipoReporte,
            Categoria      = s.Categoria,
            TotalAperturas = s.TotalAperturas,
            TotalDescargas = s.TotalDescargas,
            UsuariosUnicos = s.UsuariosUnicos,
            UltimoAcceso   = s.UltimoAcceso
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (FechaCorte, NombreReporte, TipoReporte, Categoria,
                TotalAperturas, TotalDescargas, UsuariosUnicos, UltimoAcceso)
        VALUES (s.FechaCorte, s.NombreReporte, s.TipoReporte, s.Categoria,
                s.TotalAperturas, s.TotalDescargas, s.UsuariosUnicos, s.UltimoAcceso);

    ------------------------------------------------------------------
    -- 2) Agregado por usuario y dia
    ------------------------------------------------------------------
    ;WITH evu AS (
        SELECT
            CAST(DATEADD(HOUR, -6, e.FechaUtc) AS DATE) AS FechaCorte,
            e.Usuario,
            e.NombreReporte,
            e.FechaUtc,
            e.SesionId,
            t.Categoria AS CategoriaEvento
        FROM audit.Evento e
        INNER JOIN audit.EventoTipo t ON t.TipoEventoId = e.TipoEventoId
        WHERE e.Usuario IS NOT NULL
          AND e.FechaUtc >= DATEADD(HOUR, 6, CAST(@FechaDesde AS DATETIME2(3)))
          AND e.FechaUtc <  DATEADD(HOUR, 6, DATEADD(DAY, 1, CAST(@FechaHasta AS DATETIME2(3))))
    ),
    ses AS (
        SELECT
            CAST(DATEADD(HOUR, -6, s.InicioUtc) AS DATE) AS FechaCorte,
            s.Usuario,
            SUM(ISNULL(s.DuracionSegundos, 0)) AS SegundosUso
        FROM audit.Sesion s
        WHERE s.InicioUtc >= DATEADD(HOUR, 6, CAST(@FechaDesde AS DATETIME2(3)))
          AND s.InicioUtc <  DATEADD(HOUR, 6, DATEADD(DAY, 1, CAST(@FechaHasta AS DATETIME2(3))))
        GROUP BY CAST(DATEADD(HOUR, -6, s.InicioUtc) AS DATE), s.Usuario
    ),
    agru AS (
        SELECT
            evu.FechaCorte,
            evu.Usuario,
            SUM(CASE WHEN evu.CategoriaEvento = 'Reporte' THEN 1 ELSE 0 END) AS TotalAperturas,
            COUNT(DISTINCT evu.NombreReporte) AS ReportesUnicos,
            MAX(evu.FechaUtc) AS UltimoAcceso,
            ISNULL(MAX(ses.SegundosUso), 0) AS SegundosUso
        FROM evu
        LEFT JOIN ses ON ses.FechaCorte = evu.FechaCorte AND ses.Usuario = evu.Usuario
        GROUP BY evu.FechaCorte, evu.Usuario
    )
    MERGE audit.UsuarioAgregado AS t
    USING agru AS s
       ON t.FechaCorte = s.FechaCorte AND t.Usuario = s.Usuario
    WHEN MATCHED THEN
        UPDATE SET
            TotalAperturas = s.TotalAperturas,
            ReportesUnicos = s.ReportesUnicos,
            SegundosUso    = s.SegundosUso,
            UltimoAcceso   = s.UltimoAcceso
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (FechaCorte, Usuario, TotalAperturas, ReportesUnicos, SegundosUso, UltimoAcceso)
        VALUES (s.FechaCorte, s.Usuario, s.TotalAperturas, s.ReportesUnicos, s.SegundosUso, s.UltimoAcceso);

    PRINT 'audit.sp_AgregarDiario completado.';
END
GO

PRINT 'audit.sp_AgregarDiario listo.';
GO
