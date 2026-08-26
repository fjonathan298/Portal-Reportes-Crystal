/*
================================================================================
audit_purge_job.sql
================================================================================
Politica de retencion de auditoria: elimina registros de detalle mas antiguos
que el umbral configurado, preservando los agregados.

Retencion propuesta (validar con negocio antes de activar):
    audit.Evento             -> 24 meses (720 dias)
    audit.EventoParametro    -> se borra en cascada con Evento
    audit.Sesion             -> 24 meses
    audit.ReporteAgregado    -> sin purga (historico consultado)
    audit.UsuarioAgregado    -> sin purga

Ejecucion recomendada:
    SQL Server Agent - Job "PortalReportesCrystal - Retencion Auditoria"
    Frecuencia: mensual (dia 1 a las 02:00 hora local)
    Duracion tipica: < 5 minutos si los indices estan sanos

Seguridad:
    Antes de la primera corrida, hacer respaldo de audit.* y validar el
    parametro @DiasRetencion con el area de cumplimiento.
================================================================================
*/

USE DWH_FRAMEWORK;
GO

IF OBJECT_ID('audit.sp_Retencion', 'P') IS NOT NULL
    DROP PROCEDURE audit.sp_Retencion;
GO

CREATE PROCEDURE audit.sp_Retencion
    @DiasRetencion INT = 720,        -- 24 meses
    @LotesMax      INT = 100,        -- max lotes antes de rendirse
    @TamLote       INT = 5000,       -- filas por lote (evita bloqueo grande)
    @SoloReportar  BIT = 0           -- 1 = calcula lo que borraria pero no borra
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @DiasRetencion < 30
    BEGIN
        RAISERROR ('Retencion menor a 30 dias no permitida por politica. Cancelando.', 16, 1);
        RETURN;
    END

    DECLARE @Umbral DATETIME2(3) = DATEADD(DAY, -@DiasRetencion, SYSUTCDATETIME());

    IF @SoloReportar = 1
    BEGIN
        SELECT
            'audit.Evento (a purgar)' AS Objeto,
            COUNT(*)                  AS Filas
        FROM audit.Evento
        WHERE FechaUtc < @Umbral;

        SELECT
            'audit.Sesion (a purgar)' AS Objeto,
            COUNT(*)                  AS Filas
        FROM audit.Sesion
        WHERE ISNULL(FinUtc, UltimaActividadUtc) < @Umbral;
        RETURN;
    END

    -- Borrar eventos por lotes. EventoParametro cae en cascada por FK.
    DECLARE @Lote INT = 0;
    DECLARE @Borrados INT = 1;

    WHILE @Borrados > 0 AND @Lote < @LotesMax
    BEGIN
        DELETE TOP (@TamLote)
        FROM audit.Evento
        WHERE FechaUtc < @Umbral;

        SET @Borrados = @@ROWCOUNT;
        SET @Lote += 1;
    END

    PRINT 'audit.Evento: lotes procesados = ' + CAST(@Lote AS VARCHAR(10));

    -- Borrar sesiones cerradas (o inactivas) mas antiguas que umbral
    SET @Lote = 0;
    SET @Borrados = 1;
    WHILE @Borrados > 0 AND @Lote < @LotesMax
    BEGIN
        DELETE TOP (@TamLote)
        FROM audit.Sesion
        WHERE ISNULL(FinUtc, UltimaActividadUtc) < @Umbral;

        SET @Borrados = @@ROWCOUNT;
        SET @Lote += 1;
    END

    PRINT 'audit.Sesion: lotes procesados = ' + CAST(@Lote AS VARCHAR(10));
END
GO

PRINT 'audit.sp_Retencion listo.';
GO
