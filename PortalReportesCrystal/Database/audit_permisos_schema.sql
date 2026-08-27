/*
================================================================================
audit_permisos_schema.sql
================================================================================
Tablas de permisos del Portal de Reportes Crystal (Nivel 1 de Auditoria).
Servidor destino: Perseo
Base de datos:    DWH_FRAMEWORK
Esquema:          audit  (ya debe existir — ejecutar audit_schema.sql primero)

Prerequisito:
    El esquema audit y la tabla audit.EventoTipo deben existir.
    Ejecutar Database\audit_schema.sql antes de este script.

Idempotencia:
    Usa IF NOT EXISTS / IF OBJECT_ID. Seguro re-ejecutar.

Reversa:
    DROP TABLE audit.PermisoLog;
    DROP TABLE audit.UsuarioRol;
    DROP TABLE audit.RolReporte;
    DROP TABLE audit.Rol;

Autoria:
    BI/DWH - Portal de Reportes Crystal
    Fecha:  2026-08-26
================================================================================
*/

USE DWH_FRAMEWORK;
GO

---------------------------------------------------------------------------
-- 1. Roles logicos del portal
---------------------------------------------------------------------------

IF OBJECT_ID('audit.Rol', 'U') IS NULL
BEGIN
    CREATE TABLE audit.Rol (
        RolId           INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Nombre          NVARCHAR(100)  NOT NULL,
        Descripcion     NVARCHAR(400)  NULL,
        GrupoAD         NVARCHAR(200)  NULL,
        Activo          BIT            NOT NULL DEFAULT 1,
        CreadoPor       NVARCHAR(200)  NOT NULL,
        CreadoUtc       DATETIME2(3)   NOT NULL DEFAULT SYSUTCDATETIME(),
        ModificadoPor   NVARCHAR(200)  NULL,
        ModificadoUtc   DATETIME2(3)   NULL,

        CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre)
    );

    CREATE INDEX IX_Rol_GrupoAD ON audit.Rol (GrupoAD) WHERE GrupoAD IS NOT NULL;
END
GO

---------------------------------------------------------------------------
-- 2. Asignacion rol -> reporte/categoria/raiz
---------------------------------------------------------------------------

IF OBJECT_ID('audit.RolReporte', 'U') IS NULL
BEGIN
    CREATE TABLE audit.RolReporte (
        RolReporteId    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        RolId           INT            NOT NULL,
        TipoAcceso      VARCHAR(20)    NOT NULL,
        ValorAcceso     NVARCHAR(400)  NOT NULL,
        PuedeVer        BIT            NOT NULL DEFAULT 1,
        PuedeExportar   BIT            NOT NULL DEFAULT 0,
        AsignadoPor     NVARCHAR(200)  NOT NULL,
        AsignadoUtc     DATETIME2(3)   NOT NULL DEFAULT SYSUTCDATETIME(),
        Activo          BIT            NOT NULL DEFAULT 1,

        CONSTRAINT FK_RolReporte_Rol
            FOREIGN KEY (RolId) REFERENCES audit.Rol (RolId),
        CONSTRAINT CK_RolReporte_TipoAcceso
            CHECK (TipoAcceso IN ('RAIZ', 'CATEGORIA', 'REPORTE'))
    );

    CREATE INDEX IX_RolReporte_RolId ON audit.RolReporte (RolId);
    CREATE INDEX IX_RolReporte_TipoAcceso_Valor ON audit.RolReporte (TipoAcceso, ValorAcceso);
END
GO

---------------------------------------------------------------------------
-- 3. Asignacion usuario -> rol (con soft-delete)
---------------------------------------------------------------------------

IF OBJECT_ID('audit.UsuarioRol', 'U') IS NULL
BEGIN
    CREATE TABLE audit.UsuarioRol (
        UsuarioRolId    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Usuario         NVARCHAR(200)  NOT NULL,
        RolId           INT            NOT NULL,
        AsignadoPor     NVARCHAR(200)  NOT NULL,
        AsignadoUtc     DATETIME2(3)   NOT NULL DEFAULT SYSUTCDATETIME(),
        RemovidoPor     NVARCHAR(200)  NULL,
        RemovidoUtc     DATETIME2(3)   NULL,
        Activo          BIT            NOT NULL DEFAULT 1,

        CONSTRAINT FK_UsuarioRol_Rol
            FOREIGN KEY (RolId) REFERENCES audit.Rol (RolId)
    );

    CREATE UNIQUE INDEX UX_UsuarioRol_Activo
        ON audit.UsuarioRol (Usuario, RolId)
        WHERE Activo = 1;

    CREATE INDEX IX_UsuarioRol_Usuario ON audit.UsuarioRol (Usuario) WHERE Activo = 1;
    CREATE INDEX IX_UsuarioRol_RolId   ON audit.UsuarioRol (RolId)   WHERE Activo = 1;
END
GO

---------------------------------------------------------------------------
-- 4. Historial de cambios de permisos
---------------------------------------------------------------------------

IF OBJECT_ID('audit.PermisoLog', 'U') IS NULL
BEGIN
    CREATE TABLE audit.PermisoLog (
        PermisoLogId    BIGINT         NOT NULL IDENTITY(1,1) PRIMARY KEY,
        FechaUtc        DATETIME2(3)   NOT NULL DEFAULT SYSUTCDATETIME(),
        Accion          VARCHAR(40)    NOT NULL,
        Usuario         NVARCHAR(200)  NOT NULL,
        Detalle         NVARCHAR(1000) NOT NULL,
        RolId           INT            NULL,
        UsuarioAfectado NVARCHAR(200)  NULL,
        FechaLocal      AS (DATEADD(HOUR, -6, FechaUtc)),

        CONSTRAINT FK_PermisoLog_Rol
            FOREIGN KEY (RolId) REFERENCES audit.Rol (RolId),
        CONSTRAINT CK_PermisoLog_Accion
            CHECK (Accion IN (
                'ROL_CREADO', 'ROL_MODIFICADO', 'ROL_DESACTIVADO',
                'REPORTE_ASIGNADO', 'REPORTE_REVOCADO',
                'USUARIO_ASIGNADO', 'USUARIO_REMOVIDO'
            ))
    );

    CREATE INDEX IX_PermisoLog_Fecha    ON audit.PermisoLog (FechaUtc DESC);
    CREATE INDEX IX_PermisoLog_RolId    ON audit.PermisoLog (RolId);
    CREATE INDEX IX_PermisoLog_Usuario  ON audit.PermisoLog (Usuario);
END
GO

---------------------------------------------------------------------------
-- 5. Nuevos tipos de evento para auditoria de permisos
---------------------------------------------------------------------------

MERGE audit.EventoTipo AS t
USING (VALUES
    (70, 'PERMISO_ASIGNADO',  'Asignacion de permiso a rol o usuario',  'Seguridad'),
    (71, 'PERMISO_REVOCADO',  'Revocacion de permiso a rol o usuario',  'Seguridad'),
    (72, 'ROL_CREADO',        'Creacion de un rol en el portal',        'Seguridad'),
    (73, 'ROL_MODIFICADO',    'Modificacion de un rol en el portal',    'Seguridad')
) AS s (TipoEventoId, Codigo, Descripcion, Categoria)
    ON t.TipoEventoId = s.TipoEventoId
WHEN NOT MATCHED THEN
    INSERT (TipoEventoId, Codigo, Descripcion, Categoria)
    VALUES (s.TipoEventoId, s.Codigo, s.Descripcion, s.Categoria)
WHEN MATCHED AND (t.Codigo <> s.Codigo OR t.Descripcion <> s.Descripcion OR t.Categoria <> s.Categoria) THEN
    UPDATE SET Codigo = s.Codigo, Descripcion = s.Descripcion, Categoria = s.Categoria;
GO

---------------------------------------------------------------------------
-- 6. Grants para desarrollo
---------------------------------------------------------------------------

-- Grants para la cuenta del usuario actual (desarrollo)
-- En produccion, reemplazar por la cuenta del AppPool o grupo AD
DECLARE @sql NVARCHAR(MAX);

IF OBJECT_ID('audit.Rol', 'U') IS NOT NULL
BEGIN
    SET @sql = '
        GRANT SELECT, INSERT, UPDATE ON audit.Rol         TO [' + SUSER_SNAME() + '];
        GRANT SELECT, INSERT, UPDATE ON audit.RolReporte   TO [' + SUSER_SNAME() + '];
        GRANT SELECT, INSERT, UPDATE ON audit.UsuarioRol   TO [' + SUSER_SNAME() + '];
        GRANT SELECT, INSERT         ON audit.PermisoLog   TO [' + SUSER_SNAME() + '];
    ';
    EXEC sp_executesql @sql;
    PRINT 'GRANTs de permisos aplicados a ' + SUSER_SNAME();
END
GO

PRINT 'audit permisos schema listo.';
GO
