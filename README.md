# Portal de Reportes Crystal

Portal web interno para visualizar, exportar y gestionar reportes Crystal Reports y SAP BusinessObjects de Super Repuestos (El Salvador, Honduras, Guatemala).

## Que hace este portal

- **Reportes Crystal locales**: abre archivos `.rpt` directamente desde el servidor, con vista previa, exportacion a PDF/Excel y formulario dinamico de parametros.
- **Reportes SAP BO**: descubre automaticamente reportes Crystal y WebI publicados en el servidor SAP BusinessObjects via API REST, y los presenta en un listado unificado.
- **Auditoria**: registra quien abre que reporte, cuando, con que filtros y en que formato lo exporta. Dashboard de metricas para administradores.
- **Busqueda y filtros**: busqueda por nombre, filtros por servidor/tipo/estado, agrupacion por carpeta.

## Tecnologias

| Componente | Tecnologia |
|---|---|
| Backend | ASP.NET MVC 5, .NET Framework 4.8 |
| Reportes | Crystal Reports SDK v13.0.4000.0 |
| Integracion SAP BO | REST API `/biprws/` + OpenDocument |
| Base de datos | SQL Server (Windows Authentication) |
| Autenticacion | Windows Authentication (Active Directory) |
| Frontend | HTML/CSS puro (sin frameworks JS) |

## Estructura del repositorio

```
Portal-Reportes-Crystal/
|-- PortalReportesCrystal/          <-- Proyecto principal ASP.NET MVC
|   |-- Controllers/                <-- Controladores MVC
|   |-- Services/                   <-- Logica de negocio (SapBoClient, Auditoria)
|   |-- Views/                      <-- Vistas Razor (.cshtml)
|   |-- Database/                   <-- Scripts SQL
|   |   |-- audit_schema.sql        <-- DDL del esquema de auditoria
|   |   |-- LOV/                    <-- Listas de valores estandarizadas
|   |-- docs/                       <-- Documentacion tecnica
|   |-- Reportes/                   <-- Archivos .rpt locales
|   |-- Web.config.example          <-- Plantilla de configuracion (sin credenciales)
|-- Tutorial_Sample_Code/           <-- Ejemplos oficiales de Crystal Reports SDK
|-- MANUAL_PORTAL_CRYSTAL.md        <-- Manual tecnico completo (15 modulos)
```

## Prerequisitos

1. **Windows Server** con IIS o **Windows 10/11** con IIS Express para desarrollo.
2. **Visual Studio 2022** (Community o superior) con workload "ASP.NET and web development".
3. **Crystal Reports Runtime** v13.0.4000 (SP 36+) instalado en la maquina. Descargar desde [SAP Downloads](https://help.sap.com/docs/SAP_CRYSTAL_REPORTS_FOR_VISUAL_STUDIO).
4. **SQL Server** accesible via Windows Authentication para la base de auditoria.
5. (Opcional) Acceso al servidor **SAP BusinessObjects** si se desea la integracion con reportes del CMC.

## Configuracion inicial

### 1. Clonar el repositorio

```bash
git clone https://github.com/fjonathan298/Portal-Reportes-Crystal.git
cd Portal-Reportes-Crystal
```

### 2. Crear el Web.config

El archivo `Web.config` con credenciales **no se incluye** en el repositorio por seguridad. Crear una copia desde la plantilla:

```bash
cp PortalReportesCrystal/Web.config.example PortalReportesCrystal/Web.config
```

Editar `Web.config` y reemplazar los placeholders:

| Placeholder | Descripcion |
|---|---|
| `__SERVIDOR_SAPBO__` | Hostname del servidor SAP BusinessObjects |
| `__USUARIO_SAPBO__` | Usuario con acceso al CMS de SAP BO |
| `__PASSWORD_SAPBO__` | Password del usuario SAP BO |
| `__SERVIDOR_BD__` | Hostname del servidor SQL Server para auditoria |
| `__BASE_DE_DATOS__` | Nombre de la base de datos de auditoria |
| `__DOMINIO__` | Dominio de Active Directory |
| `__GRUPO_AD_LECTORES__` | Grupo AD con acceso al dashboard de auditoria |

### 3. Restaurar paquetes NuGet

Abrir `PortalReportesCrystal/PortalReportesCrystal.sln` en Visual Studio. Los paquetes NuGet se restauran automaticamente al compilar.

### 4. Compilar y ejecutar

**Desde Visual Studio**: F5 (Debug) o Ctrl+F5 (sin debug).

**Desde linea de comandos**:

```powershell
cd PortalReportesCrystal
.\Ejecutar-Portal.ps1
```

El portal se abre en `http://localhost:58172`.

### 5. (Opcional) Configurar auditoria

La auditoria esta deshabilitada por defecto (`Audit:Habilitado=false`). Para activarla:

1. Ejecutar `Database/audit_schema.sql` en el servidor SQL Server destino.
2. Crear los grupos de Active Directory indicados en el script.
3. Cambiar `Audit:Habilitado` a `true` en `Web.config`.

## Documentacion

| Documento | Descripcion |
|---|---|
| [MANUAL_PORTAL_CRYSTAL.md](MANUAL_PORTAL_CRYSTAL.md) | Manual tecnico completo: 15 modulos desde cero hasta produccion |
| [docs/PATRON_PARAMETROS_DINAMICOS.md](PortalReportesCrystal/docs/PATRON_PARAMETROS_DINAMICOS.md) | Estandar de parametrizacion dinamica para Crystal Reports |
| [Database/LOV/README.md](PortalReportesCrystal/Database/LOV/README.md) | Biblioteca de listas de valores (LOV Commands) |
| [Documentacion_Tecnica_Portal_Reportes_Crystal.docx](Documentacion_Tecnica_Portal_Reportes_Crystal.docx) | Documentacion tecnica general |
| [Nota_Arquitectura_Estrategia_Portal.docx](Nota_Arquitectura_Estrategia_Portal.docx) | Nota de arquitectura y estrategia |

## Modulos del manual

El manual cubre 15 modulos progresivos:

1. Estructura del proyecto MVC
2. Autenticacion Windows
3. Integracion Crystal Reports SDK
4. Manejo de errores y bitacora
5. Multiples raices de reportes
6. Reportes externos (SAP BO CMC)
7. Deteccion y formulario de parametros
8. Cache de parametros y estado
9. Vista previa con PDFsharp
10. Busqueda, filtros y agrupacion
11. Identidad corporativa
12. Descubrimiento automatico via API REST SAP BO
13. Estadisticas SAP BO (sesiones, licencias, servidores)
14. Auditoria integral del portal
15. Parametrizacion dinamica estandar

## Seguridad

- **Credenciales**: nunca se almacenan en el repositorio. Usar `Web.config.example` como plantilla.
- **Autenticacion**: Windows Authentication via Active Directory. No hay login manual.
- **Auditoria BD**: conexion via Integrated Security (SSPI). Sin passwords en connection strings.
- **SAP BO**: credenciales Enterprise en Web.config local (cifrar con `aspnet_regiis -pe` en produccion).
- **Dashboard**: acceso restringido por grupo de Active Directory.

## Contribuir

1. Crear un branch desde `master`: `git checkout -b feature/mi-cambio`
2. Hacer los cambios y probar localmente.
3. **Verificar** que `Web.config` no se incluya en el commit (`git status`).
4. Crear un Pull Request con descripcion de los cambios.

## Contacto

Area de BI/DWH - Super Repuestos El Salvador.
