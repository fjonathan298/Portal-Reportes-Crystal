/*
================================================================================
audit_schema.sql
================================================================================
Esquema de auditoria del Portal de Reportes Crystal.
Servidor destino: Perseo
Base de datos:    DWH_FRAMEWORK
Esquema:          audit

Objetivo:
    Registrar TODO evento de usuario en el portal (login, apertura de listado,
    apertura de reporte, exportacion, filtros aplicados, descargas y errores)
    con trazabilidad completa por sesion, usuario, IP y timestamp.

Alcance:
    Solo metadatos de eventos. NUNCA se almacena contenido de los reportes,
    credenciales, tokens ni parametros sensibles.

Idempotencia:
    Este script es idempotente. Se puede ejecutar N veces sin duplicar objetos
    ni perder datos existentes (usa IF NOT EXISTS / IF OBJECT_ID checks).

Reversa:
    Ver audit_purge_job.sql para retencion, y para reversa completa:
        DROP SCHEMA audit;  -- solo si audit.* esta vacio; ver DROP script aparte

Autoria:
    BI/DWH - Portal de Reportes Crystal
    Fecha:  2026-08-24
================================================================================
*/

USE DWH_FRAMEWORK;
GO

---------------------------------------------------------------------------
-- 1. Esquema
---------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'audit')
    EXEC('CREATE SCHEMA audit AUTHORIZATION dbo');
GO

---------------------------------------------------------------------------
-- 2. Catalogo de tipos de evento
---------------------------------------------------------------------------

IF OBJECT_ID('audit.EventoTipo', 'U') IS NULL
BEGIN
    CREATE TABLE audit.EventoTipo (
        TipoEventoId    TINYINT       NOT NULL PRIMARY KEY,
        Codigo          VARCHAR(40)   NOT NULL UNIQUE,
        Descripcion     VARCHAR(200)  NOT NULL,
        Categoria       VARCHAR(40)   NOT NULL,   -- Navegacion / Reporte / Exportacion / Seguridad / Sistema
        Activo          BIT           NOT NULL DEFAULT 1
    );
END
GO

MERGE audit.EventoTipo AS t
USING (VALUES
    ( 1, 'LOGIN',              'Inicio de sesion Windows autenticada',        'Seguridad'),
    ( 2, 'LOGOUT',             'Fin de sesion (timeout o expreso)',           'Seguridad'),
    (10, 'VER_LISTADO',        'Apertura del listado principal de reportes',  'Navegacion'),
    (11, 'BUSQUEDA',           'Uso del cuadro de busqueda / filtros UI',     'Navegacion'),
    (20, 'VER_REPORTE',        'Apertura del visor de un reporte',            'Reporte'),
    (21, 'PREVIEW',            'Vista previa condensada (PDF corto)',         'Reporte'),
    (22, 'PREVIEW_DATOS',      'Vista previa solo-datos',                     'Reporte'),
    (23, 'FILTRO_APLICADO',    'Parametros/filtros aplicados a un reporte',   'Reporte'),
    (30, 'EXPORTAR_PDF',       'Exportacion a PDF',                           'Exportacion'),
    (31, 'EXPORTAR_EXCEL',     'Exportacion a Excel',                         'Exportacion'),
    (32, 'EXPORTAR_EXCELDATA', 'Exportacion Excel solo-datos',                'Exportacion'),
    (33, 'EXPORTAR_WORD',      'Exportacion a Word',                          'Exportacion'),
    (40, 'DESCARGA_IFRAME',    'Descarga detectada dentro del iframe SAP BO', 'Exportacion'),
    (50, 'ERROR_GENERACION',   'Error al generar un reporte',                 'Reporte'),
    (51, 'ERROR_ACCESO',       'Intento de acceso denegado',                  'Seguridad'),
    (60, 'HEARTBEAT',           'Pulso periodico de sesion activa',           'Sistema'),
    (61, 'ACCESO_DASHBOARD',   'Ingreso al panel de auditoria',               'Seguridad')
) AS s (TipoEventoId, Codigo, Descripcion, Categoria)
    ON t.TipoEventoId = s.TipoEventoId
WHEN NOT MATCHED THEN
    INSERT (TipoEventoId, Codigo, Descripcion, Categoria)
    VALUES (s.TipoEventoId, s.Codigo, s.Descripcion, s.Categoria)
WHEN MATCHED AND (t.Codigo <> s.Codigo OR t.Descripcion <> s.Descripcion OR t.Categoria <> s.Categoria) THEN
    UPDATE SET Codigo = s.Codigo, Descripcion = s.Descripcion, Categoria = s.Categoria;
GO

---------------------------------------------------------------------------
-- 3. Sesiones de usuario
---------------------------------------------------------------------------

IF OBJECT_ID('audit.Sesion', 'U') IS NULL
BEGIN
    CREATE TABLE audit.Sesion (
        SesionId              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        Usuario               NVARCHAR(200)    NOT NULL,
        IpCliente             VARCHAR(45)      NULL,       -- cabe IPv6
        UserAgent             NVARCHAR(500)    NULL,
        InicioUtc             DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME(),
        UltimaActividadUtc    DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME(),
        FinUtc                DATETIME2(3)     NULL,
        DuracionSegundos      AS (CASE WHEN FinUtc IS NULL
                                       THEN DATEDIFF(SECOND, InicioUtc, UltimaActividadUtc)
                                       ELSE DATEDIFF(SECOND, InicioUtc, FinUtc)
                                  END),
        InicioLocal           AS (DATEADD(HOUR, -6, InicioUtc)),  -- GMT-6 El Salvador
        UltimaActividadLocal  AS (DATEADD(HOUR, -6, UltimaActividadUtc))
    );

    CREATE INDEX IX_Sesion_Usuario           ON audit.Sesion (Usuario);
    CREATE INDEX IX_Sesion_InicioUtc         ON audit.Sesion (InicioUtc DESC);
    CREATE INDEX IX_Sesion_UltimaActividadUtc ON audit.Sesion (UltimaActividadUtc DESC);
END
GO

---------------------------------------------------------------------------
-- 4. Eventos
---------------------------------------------------------------------------

IF OBJECT_ID('audit.Evento', 'U') IS NULL
BEGIN
    CREATE TABLE audit.Evento (
        EventoId          BIGINT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        SesionId          UNIQUEIDENTIFIER NULL,       -- FK debil: puede ser null si aun no se ha creado la sesion
        TipoEventoId      TINYINT          NOT NULL,
        FechaUtc          DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME(),
        Usuario           NVARCHAR(200)    NOT NULL,   -- desnormalizado a proposito (consultas rapidas)
        IpCliente         VARCHAR(45)      NULL,
        RaizId            VARCHAR(60)      NULL,       -- ej. crystalxi / sapbo / proyecto / cmc
        PathReporte       NVARCHAR(600)    NULL,       -- ruta relativa cuando aplica
        NombreReporte     NVARCHAR(400)    NULL,
        Categoria         NVARCHAR(200)    NULL,       -- carpeta o grupo del reporte
        TipoReporte       VARCHAR(20)      NULL,       -- Local / WebI / Sapbo / Externo
        Servidor          NVARCHAR(100)    NULL,       -- Local / SAP BO WebI / SAP BO .rpt / SAP BO / ...
        Formato           VARCHAR(20)      NULL,       -- pdf / excel / exceldata / word / ...
        DuracionMs        INT              NULL,
        TamanioBytes      BIGINT           NULL,
        HttpStatus        SMALLINT         NULL,
        UrlOrigen         NVARCHAR(600)    NULL,
        MensajeError      NVARCHAR(1000)   NULL,
        FechaLocal        AS (DATEADD(HOUR, -6, FechaUtc)),

        CONSTRAINT FK_Evento_TipoEvento
            FOREIGN KEY (TipoEventoId) REFERENCES audit.EventoTipo (TipoEventoId),
        CONSTRAINT FK_Evento_Sesion
            FOREIGN KEY (SesionId)     REFERENCES audit.Sesion     (SesionId)
    );

    CREATE INDEX IX_Evento_Fecha_Tipo     ON audit.Evento (FechaUtc DESC, TipoEventoId);
    CREATE INDEX IX_Evento_SesionId       ON audit.Evento (SesionId);
    CREATE INDEX IX_Evento_Usuario_Fecha  ON audit.Evento (Usuario, FechaUtc DESC);
    CREATE INDEX IX_Evento_NombreReporte  ON audit.Evento (NombreReporte, FechaUtc DESC);
    CREATE INDEX IX_Evento_TipoReporte    ON audit.Evento (TipoReporte, FechaUtc DESC);
END
GO

---------------------------------------------------------------------------
-- 5. Parametros por evento (Almacen, Pais, Fechas, etc.)
---------------------------------------------------------------------------

IF OBJECT_ID('audit.EventoParametro', 'U') IS NULL
BEGIN
    CREATE TABLE audit.EventoParametro (
        EventoParametroId  BIGINT        NOT NULL IDENTITY(1,1) PRIMARY KEY,
        EventoId           BIGINT        NOT NULL,
        NombreParametro    VARCHAR(60)   NOT NULL,   -- ej. ALMACEN / PAIS / FECHA_DESDE / FECHA_HASTA
        ValorParametro     NVARCHAR(400) NOT NULL,

        CONSTRAINT FK_EventoParametro_Evento
            FOREIGN KEY (EventoId) REFERENCES audit.Evento (EventoId)
            ON DELETE CASCADE
    );

    CREATE INDEX IX_EventoParametro_Evento           ON audit.EventoParametro (EventoId);
    CREATE INDEX IX_EventoParametro_Nombre_Valor     ON audit.EventoParametro (NombreParametro, ValorParametro);
END
GO

---------------------------------------------------------------------------
-- 6. Agregados diarios (materializadas por SP)
---------------------------------------------------------------------------

IF OBJECT_ID('audit.ReporteAgregado', 'U') IS NULL
BEGIN
    CREATE TABLE audit.ReporteAgregado (
        ReporteAgregadoId  BIGINT       NOT NULL IDENTITY(1,1) PRIMARY KEY,
        FechaCorte         DATE         NOT NULL,      -- dia calculado (00:00 local)
        NombreReporte      NVARCHAR(400) NOT NULL,
        TipoReporte        VARCHAR(20)  NULL,
        Categoria          NVARCHAR(200) NULL,
        TotalAperturas     INT          NOT NULL DEFAULT 0,
        TotalDescargas     INT          NOT NULL DEFAULT 0,
        UsuariosUnicos     INT          NOT NULL DEFAULT 0,
        UltimoAcceso       DATETIME2(3) NULL
    );

    CREATE UNIQUE INDEX UX_ReporteAgregado_Fecha_Reporte
        ON audit.ReporteAgregado (FechaCorte, NombreReporte);
    CREATE INDEX IX_ReporteAgregado_Fecha
        ON audit.ReporteAgregado (FechaCorte DESC);
END
GO

IF OBJECT_ID('audit.UsuarioAgregado', 'U') IS NULL
BEGIN
    CREATE TABLE audit.UsuarioAgregado (
        UsuarioAgregadoId  BIGINT         NOT NULL IDENTITY(1,1) PRIMARY KEY,
        FechaCorte         DATE           NOT NULL,
        Usuario            NVARCHAR(200)  NOT NULL,
        TotalAperturas     INT            NOT NULL DEFAULT 0,
        ReportesUnicos     INT            NOT NULL DEFAULT 0,
        SegundosUso        INT            NOT NULL DEFAULT 0,
        UltimoAcceso       DATETIME2(3)   NULL
    );

    CREATE UNIQUE INDEX UX_UsuarioAgregado_Fecha_Usuario
        ON audit.UsuarioAgregado (FechaCorte, Usuario);
    CREATE INDEX IX_UsuarioAgregado_Fecha
        ON audit.UsuarioAgregado (FechaCorte DESC);
END
GO

---------------------------------------------------------------------------
-- 7. Grants (Windows Authentication + minimo privilegio)
---------------------------------------------------------------------------
-- El portal se conecta a SQL Server con Windows Auth (Integrated Security).
-- La cuenta que corre el Application Pool de IIS (o IIS Express en dev) es
-- la que se autentica contra Perseo. En produccion se recomienda un
-- Application Pool con Identity = cuenta de dominio dedicada, idealmente
-- gMSA (Group Managed Service Account).
--
-- Convencion sugerida:
--   Grupo AD  SUPERREPUESTOS\Portal_Audit_Writers  -> escribe eventos
--   Grupo AD  SUPERREPUESTOS\Portal_Audit_Readers  -> lee dashboard
--
-- Cuentas individuales (desarrollo o transicion): agregarlas al grupo AD;
-- si no se pueden crear grupos AD, ajustar los nombres abajo.
---------------------------------------------------------------------------

-- Login del grupo AD escritor (debe existir en la instancia)
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'SUPERREPUESTOS\Portal_Audit_Writers')
BEGIN
    -- Crear el user en DWH_FRAMEWORK si no existe
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SUPERREPUESTOS\Portal_Audit_Writers')
        CREATE USER [SUPERREPUESTOS\Portal_Audit_Writers]
            FOR LOGIN [SUPERREPUESTOS\Portal_Audit_Writers];

    GRANT SELECT ON audit.EventoTipo       TO [SUPERREPUESTOS\Portal_Audit_Writers];
    GRANT INSERT, SELECT, UPDATE ON audit.Sesion  TO [SUPERREPUESTOS\Portal_Audit_Writers];
    GRANT INSERT, SELECT ON audit.Evento          TO [SUPERREPUESTOS\Portal_Audit_Writers];
    GRANT INSERT, SELECT ON audit.EventoParametro TO [SUPERREPUESTOS\Portal_Audit_Writers];
    PRINT 'GRANTs aplicados a SUPERREPUESTOS\Portal_Audit_Writers';
END
ELSE
BEGIN
    PRINT 'AVISO: no existe el login SUPERREPUESTOS\Portal_Audit_Writers.';
    PRINT '       Crearlo con: CREATE LOGIN [SUPERREPUESTOS\Portal_Audit_Writers] FROM WINDOWS;';
END
GO

-- Login del grupo AD lector
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'SUPERREPUESTOS\Portal_Audit_Readers')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SUPERREPUESTOS\Portal_Audit_Readers')
        CREATE USER [SUPERREPUESTOS\Portal_Audit_Readers]
            FOR LOGIN [SUPERREPUESTOS\Portal_Audit_Readers];

    GRANT SELECT ON SCHEMA::audit TO [SUPERREPUESTOS\Portal_Audit_Readers];
    PRINT 'GRANTs aplicados a SUPERREPUESTOS\Portal_Audit_Readers';
END
ELSE
BEGIN
    PRINT 'AVISO: no existe el login SUPERREPUESTOS\Portal_Audit_Readers.';
    PRINT '       Crearlo con: CREATE LOGIN [SUPERREPUESTOS\Portal_Audit_Readers] FROM WINDOWS;';
END
GO

PRINT 'audit schema listo.';
GO
