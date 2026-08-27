# Manual de construcción — Portal de Reportes Crystal

**Objetivo:** guía paso a paso para construir desde cero un portal web que expone reportes de SAP Crystal Reports (archivos `.rpt`) y enlaces a SAP BO CMC, con autenticación integrada de Windows, organizado por carpetas y con vista previa optimizada.

**Audiencia:** desarrolladores con conocimientos básicos de C# y HTML que quieran entender el porqué de cada decisión, no solo copiar código.

**Tiempo estimado:** 12–16 horas repartidas en 14 módulos secuenciales.

---

## Tabla de contenidos

- [Módulo 0. Prerrequisitos y arquitectura](#módulo-0-prerrequisitos-y-arquitectura)
- [Módulo 1. Estructura del proyecto MVC](#módulo-1-estructura-del-proyecto-mvc)
- [Módulo 2. Autenticación integrada de Windows](#módulo-2-autenticación-integrada-de-windows)
- [Módulo 3. Integración con Crystal Reports SDK](#módulo-3-integración-con-crystal-reports-sdk)
- [Módulo 4. Manejo de errores y bitácora](#módulo-4-manejo-de-errores-y-bitácora)
- [Módulo 5. Múltiples raíces de reportes](#módulo-5-múltiples-raíces-de-reportes)
- [Módulo 6. Reportes externos (SAP BO CMC)](#módulo-6-reportes-externos-sap-bo-cmc)
- [Módulo 7. Detección y formulario de parámetros](#módulo-7-detección-y-formulario-de-parámetros)
- [Módulo 8. Cache de parámetros y estado por reporte](#módulo-8-cache-de-parámetros-y-estado-por-reporte)
- [Módulo 9. Vista previa condensada con PDFsharp](#módulo-9-vista-previa-condensada-con-pdfsharp)
- [Módulo 10. Búsqueda, filtros y agrupación](#módulo-10-búsqueda-filtros-y-agrupación)
- [Módulo 11. Identidad corporativa](#módulo-11-identidad-corporativa)
- [Módulo 12. Descubrimiento automático de reportes vía API REST SAP BO](#módulo-12-descubrimiento-automático-de-reportes-vía-api-rest-sap-bo)
- [Módulo 13. Página de estadísticas SAP BO (sesiones, licencias, servidores)](#módulo-13-página-de-estadísticas-sap-bo-sesiones-licencias-servidores)
- [Módulo 14. Auditoría integral del portal](#módulo-14-auditoría-integral-del-portal)
- [Anexo A. Ejecución sin depender de F5](#anexo-a-ejecución-sin-depender-de-f5)
- [Anexo B. Guía de despliegue a producción](#anexo-b-guía-de-despliegue-a-producción)
- [Anexo C. Troubleshooting común](#anexo-c-troubleshooting-común)

---

## Módulo 0. Prerrequisitos y arquitectura

### Objetivo

Comprender por qué la stack elegida es esta y verificar que todo el software base esté instalado.

### Teoría: por qué .NET Framework 4.8 y no .NET Core

SAP Crystal Reports para .NET tiene un SDK oficial que **solo funciona con .NET Framework** (versiones 4.x). No existe una versión oficial para .NET Core, .NET 5, 6, 7 u 8. Cualquier proyecto que consuma el SDK debe compilarse contra .NET Framework.

.NET Framework 4.8 es la última versión de la rama clásica de .NET. Microsoft mantiene el runtime como parte del sistema operativo Windows, pero ya no evoluciona la plataforma. Es adecuada para proyectos internos con horizonte de 5–10 años.

### Teoría: por qué ASP.NET MVC y no WebForms

- **WebForms** es el patrón original de ASP.NET, orientado a eventos y control-tree. El visor oficial de Crystal Reports (`CrystalReportViewer`) es un control de WebForms.
- **MVC** es un patrón moderno (modelo-vista-controlador) con URLs limpias, testable y con separación clara de responsabilidades.

Se elige MVC porque el visor de PDF nativo del navegador reemplaza al `CrystalReportViewer`, y MVC tiene mejor mantenibilidad. WebForms queda descartado.

### Software requerido

| Componente | Versión | Uso |
|---|---|---|
| Windows 10/11 o Windows Server | 2016 o superior | Sistema operativo |
| Visual Studio Community | 2022, 17.x | IDE (opcional: MSBuild + editor de texto) |
| Carga de trabajo "ASP.NET y desarrollo web" | — | Instala plantillas y IIS Express |
| .NET Framework 4.8 | — | Target del proyecto |
| SAP Crystal Reports 2020 SP4+ | 14.3.x | Diseñador para crear/editar los `.rpt` |
| SAP Crystal Reports Runtime for .NET | 13.0.35+ | Motor de ejecución que consume el portal |
| IIS Express | 10 | Servidor de desarrollo local |
| Un editor de texto | — | Para editar CSS/JSON |

### Verificaciones antes de empezar

```powershell
# .NET Framework 4.8
(Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release
# Debe devolver 528040 o superior

# Ensamblados Crystal en GAC
Get-ChildItem "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\CrystalDecisions.*" -Directory

# IIS Express
Test-Path "C:\Program Files\IIS Express\iisexpress.exe"
```

### Arquitectura del portal (visión general)

```
Navegador
    │
    │ HTTP (Windows Auth)
    ▼
IIS Express  ─────►  ASP.NET MVC 5
                          │
                          ├─► HomeController      → listado de reportes
                          ├─► ReportesController  → visor, exportación, parámetros
                          │       │
                          │       ▼
                          │   Crystal Reports SDK
                          │       │
                          │       ▼
                          │   Motor de ejecución
                          │       │
                          │       ▼
                          │   BD del reporte (opcional)
                          │
                          ├─► Services/CacheParametros    → detecta prompts
                          └─► Services/EstadoReportes     → marca errores
```

---

## Módulo 1. Estructura del proyecto MVC

### Objetivo

Crear el esqueleto de un proyecto ASP.NET MVC 5 sobre .NET Framework 4.8.

### Teoría: qué es MVC

**Model-View-Controller** separa la aplicación en tres responsabilidades:

- **Model:** clases con datos (POCOs). Sin lógica de UI ni de acceso a datos.
- **View:** archivos `.cshtml` (Razor) que renderizan HTML.
- **Controller:** clases que responden a las URLs, procesan lógica y devuelven vistas o archivos.

Una petición HTTP fluye así:
```
GET /Home/Index
     ↓
RouteConfig traduce URL → controlador + acción
     ↓
HomeController.Index() se ejecuta
     ↓
Devuelve View(model)
     ↓
MVC busca Views/Home/Index.cshtml
     ↓
Renderiza HTML y responde al navegador
```

### Estructura de carpetas objetivo

```
PortalReportesCrystal/
├── App_Data/                 (datos de runtime: logs, caches)
├── App_Start/
│   └── RouteConfig.cs        (mapeo URL → controlador)
├── Content/
│   ├── Site.css              (estilos)
│   └── logo-superrepuestos.svg
├── Controllers/
│   ├── HomeController.cs
│   └── ReportesController.cs
├── Models/
│   └── ReporteViewModel.cs
├── Reportes/                 (carpeta para .rpt de prueba)
├── ReportesCMC/
│   └── catalogo.json         (reportes externos)
├── ReportesLocales/
│   └── configuracion.json    (raíces adicionales de .rpt)
├── Services/
│   ├── CacheParametros.cs
│   └── EstadoReportes.cs
├── Views/
│   ├── Home/
│   ├── Reportes/
│   ├── Shared/_Layout.cshtml
│   ├── _ViewStart.cshtml
│   └── Web.config
├── Global.asax
├── Global.asax.cs
├── Web.config
├── packages.config
└── PortalReportesCrystal.csproj
```

### Paso 1.1 — Crear el proyecto

**Opción A (desde Visual Studio):**
1. Archivo → Nuevo → Proyecto
2. Buscar "ASP.NET Web Application (.NET Framework)"
3. Nombre: `PortalReportesCrystal`
4. Framework: `.NET Framework 4.8`
5. En el diálogo de plantilla, elegir "MVC"

**Opción B (a mano):** crear el `.csproj` con los `ProjectTypeGuids` de proyecto web C#:

```xml
<ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
<UseIISExpress>true</UseIISExpress>
```

### Paso 1.2 — Instalar los paquetes NuGet mínimos

Contenido de `packages.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Microsoft.AspNet.Mvc" version="5.2.9" targetFramework="net48" />
  <package id="Microsoft.AspNet.Razor" version="3.2.9" targetFramework="net48" />
  <package id="Microsoft.AspNet.WebPages" version="3.2.9" targetFramework="net48" />
  <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" targetFramework="net48" />
</packages>
```

Ejecutar desde consola:

```bash
nuget restore
```

### Paso 1.3 — `Global.asax.cs` (punto de entrada)

```csharp
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PortalReportesCrystal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
```

### Paso 1.4 — `App_Start/RouteConfig.cs`

```csharp
using System.Web.Mvc;
using System.Web.Routing;

namespace PortalReportesCrystal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });
        }
    }
}
```

### Paso 1.5 — Vista mínima para verificar

`Views/Home/Index.cshtml`:
```html
<h2>Portal de Reportes Crystal</h2>
<p>El proyecto arrancó correctamente.</p>
```

`Controllers/HomeController.cs`:
```csharp
using System.Web.Mvc;

namespace PortalReportesCrystal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View();
    }
}
```

### Verificación del módulo

- El proyecto compila con MSBuild sin errores
- Al ejecutar con IIS Express se ve el mensaje "El proyecto arrancó correctamente"

---

## Módulo 2. Autenticación integrada de Windows

### Objetivo

Que el portal identifique al usuario del dominio Windows sin pantalla de login.

### Teoría: cómo funciona Windows Auth

1. El navegador envía la petición sin credenciales
2. IIS responde `401 Unauthorized` con el encabezado `WWW-Authenticate: Negotiate`
3. El navegador reenvía la petición con un token Kerberos/NTLM del usuario logueado en Windows
4. IIS valida el token contra el dominio y expone `User.Identity.Name` a la aplicación

Solo funciona si:
- El servidor y el cliente están en la misma red (o alcanzables por el mismo Active Directory)
- El sitio está agregado a "Sitios de intranet local" del navegador (o el navegador confía en él por otra vía)

### Paso 2.1 — `Web.config`

```xml
<configuration>
  <system.web>
    <authentication mode="Windows" />
    <authorization>
      <deny users="?" />
    </authorization>
    <compilation debug="true" targetFramework="4.8" />
    <httpRuntime targetFramework="4.8" maxRequestLength="51200" />
  </system.web>

  <system.webServer>
    <validation validateIntegratedModeConfiguration="false" />
    <handlers>
      <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
      <add name="ExtensionlessUrlHandler-Integrated-4.0"
           path="*." verb="*"
           type="System.Web.Handlers.TransferRequestHandler"
           preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
    <!--
      NOTA: la sección <security><authentication> está bloqueada por defecto
      en IIS Express. Se configura vía applicationhost.config del sitio.
      En IIS de producción sí se descomenta aquí:
      <security>
        <authentication>
          <windowsAuthentication enabled="true" />
          <anonymousAuthentication enabled="false" />
        </authentication>
      </security>
    -->
  </system.webServer>
</configuration>
```

### Paso 2.2 — Proteger controladores

Cada controlador se anota con `[Authorize]`:

```csharp
[Authorize]
public class HomeController : Controller
{
    public ActionResult Index()
    {
        ViewBag.Usuario = User.Identity.Name;
        return View();
    }
}
```

### Paso 2.3 — Habilitar en IIS Express

En el `.csproj.user`:

```xml
<IISExpressAnonymousAuthentication>disabled</IISExpressAnonymousAuthentication>
<IISExpressWindowsAuthentication>enabled</IISExpressWindowsAuthentication>
```

Al arrancar con F5, Visual Studio genera el `applicationhost.config` del sitio con las secciones desbloqueadas.

### Verificación del módulo

Al abrir el portal se ve el usuario del dominio en la esquina superior derecha (`DOMINIO\usuario`). No hay pantalla de login.

---

## Módulo 3. Integración con Crystal Reports SDK

### Objetivo

Cargar un archivo `.rpt` con el SDK y exportarlo en varios formatos (PDF, Excel, Word) desde el portal.

### Teoría: ciclo de vida de un ReportDocument

`ReportDocument` es la clase principal del SDK. Un ciclo completo es:

```csharp
var report = new ReportDocument();          // 1. instanciar
try
{
    report.Load("ruta/al/archivo.rpt");     // 2. abrir el .rpt
    // (opcional) aplicar credenciales, parámetros
    var stream = report.ExportToStream(...); // 3. ejecutar y exportar
    return stream;
}
finally
{
    report.Close();                          // 4. cerrar recursos nativos
    report.Dispose();                        // 5. liberar memoria
}
```

**Punto crítico:** Crystal usa memoria nativa fuera del recolector de basura de .NET. Si no se llama a `Close()` y `Dispose()`, el proceso de IIS acumula memoria hasta caer.

### Paso 3.1 — Referenciar los ensamblados

En el `.csproj`:

```xml
<Reference Include="CrystalDecisions.CrystalReports.Engine, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
<Reference Include="CrystalDecisions.ReportSource, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
<Reference Include="CrystalDecisions.Shared, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
<Reference Include="CrystalDecisions.Web, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
```

Y en `Web.config`:

```xml
<compilation debug="true" targetFramework="4.8">
  <assemblies>
    <add assembly="CrystalDecisions.CrystalReports.Engine, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
    <add assembly="CrystalDecisions.Shared, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
    <add assembly="CrystalDecisions.Web, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
    <add assembly="CrystalDecisions.ReportSource, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" />
  </assemblies>
</compilation>
```

### Paso 3.2 — Acción de exportación

```csharp
using System.IO;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public ActionResult Exportar(string archivo, string formato)
{
    string reportPath = Server.MapPath("~/Reportes/" + archivo);
    if (!System.IO.File.Exists(reportPath))
        return HttpNotFound();

    var report = new ReportDocument();
    try
    {
        report.Load(reportPath);

        ExportFormatType tipo;
        string mime;
        string ext;
        switch ((formato ?? "pdf").ToLower())
        {
            case "excel":
                tipo = ExportFormatType.Excel;
                mime = "application/vnd.ms-excel";
                ext = ".xls";
                break;
            case "exceldata":                       // "Excel — Data Only"
                tipo = ExportFormatType.ExcelRecord;
                mime = "application/vnd.ms-excel";
                ext = "_datos.xls";
                break;
            default:
                tipo = ExportFormatType.PortableDocFormat;
                mime = "application/pdf";
                ext = ".pdf";
                break;
        }

        var stream = report.ExportToStream(tipo);
        string fileName = Path.GetFileNameWithoutExtension(archivo) + ext;
        return File(stream, mime, fileName);
    }
    finally
    {
        report.Close();
        report.Dispose();
    }
}
```

### Paso 3.3 — Aplicar credenciales de base de datos (opcional)

Solo si el `.rpt` no trae credenciales guardadas y necesita conectarse a la BD:

```csharp
var connInfo = new ConnectionInfo
{
    ServerName = "servidor",
    DatabaseName = "basededatos",
    UserID = "usuario",
    Password = "contraseña"
};

foreach (Table table in report.Database.Tables)
{
    var logon = table.LogOnInfo;
    logon.ConnectionInfo = connInfo;
    table.ApplyLogOnInfo(logon);
}
```

**Recomendación de seguridad:** nunca incrustar credenciales en el código. Almacenarlas en la sección `connectionStrings` del `Web.config` cifrada con `aspnet_regiis -pe`, o usar autenticación integrada de Windows contra el servidor de datos.

### Verificación del módulo

Colocar un `.rpt` con datos guardados en `~/Reportes/`, navegar a `/Reportes/Exportar?archivo=X.rpt&formato=pdf` y confirmar que el navegador descarga un PDF válido.

---

## Módulo 4. Manejo de errores y bitácora

### Objetivo

Que los errores de Crystal se muestren al usuario como mensajes comprensibles, no como trazas técnicas, y que el detalle quede registrado para diagnóstico.

### Teoría: por qué no exponer trazas de excepción

Una traza de excepción revela:
- Rutas internas del servidor
- Nombres de bases de datos y servidores
- Fragmentos de código
- A veces credenciales

Esto es una **vulnerabilidad de divulgación de información** (OWASP A05). En producción, `<compilation debug="false">` oculta las trazas automáticamente, pero es mejor manejarlo explícitamente en cada controlador.

### Paso 4.1 — Catch estructurado en el controlador

```csharp
public ActionResult Exportar(string archivo, string formato)
{
    // ... resolver ruta ...
    var report = new ReportDocument();
    try
    {
        report.Load(reportPath);
        // ... exportar ...
        return File(stream, mime, fileName);
    }
    catch (CrystalReportsException ex)
    {
        return VistaDeError(archivo, ex);
    }
    catch (Exception ex)
    {
        return VistaDeError(archivo, ex);
    }
    finally
    {
        report.Close();
        report.Dispose();
    }
}

private ActionResult VistaDeError(string archivo, Exception ex)
{
    RegistrarError(archivo, ex);
    Response.StatusCode = 500;
    Response.TrySkipIisCustomErrors = true;
    var model = new ErrorReporteViewModel
    {
        NombreReporte = Path.GetFileNameWithoutExtension(archivo ?? ""),
        Mensaje = MensajeAmigable(ex)
    };
    return View("ErrorReporte", model);
}
```

### Paso 4.2 — Clasificar el mensaje según el tipo de fallo

```csharp
private static string MensajeAmigable(Exception ex)
{
    string tipoExc = ex.GetType().FullName ?? "";
    if (tipoExc.Contains("LogOnException") ||
        tipoExc.Contains("DBException") ||
        tipoExc.Contains("SqlException"))
    {
        return "No fue posible conectar con la base de datos del reporte. " +
               "Verifique que el servidor de datos esté disponible.";
    }

    string texto = (ex.Message ?? "").ToLowerInvariant();
    if (Contiene(texto, "conexi", "conect", "logon", "connection", "database", "odbc"))
        return "No fue posible conectar con la base de datos del reporte.";
    if (Contiene(texto, "parameter", "prompt", "invalid value"))
        return "El reporte requiere parámetros que no fueron proporcionados o son inválidos.";
    if (Contiene(texto, "load report failed", "invalid report"))
        return "El archivo del reporte no pudo ser cargado. Puede estar corrupto.";

    return "Ocurrió un error al generar el reporte. Detalle registrado en el servidor " +
           "(referencia: " + ex.GetType().Name + ").";
}

private static bool Contiene(string texto, params string[] fragmentos)
{
    foreach (var f in fragmentos)
        if (texto.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
    return false;
}
```

### Paso 4.3 — Log a archivo en `App_Data`

`App_Data` es una carpeta especial de ASP.NET: **está bloqueada para acceso HTTP**, pero es escribible desde código. Ideal para logs internos.

```csharp
private void RegistrarError(string archivo, Exception ex)
{
    System.Diagnostics.Trace.TraceError("Error en reporte '{0}': {1}", archivo, ex);
    try
    {
        string dir = Server.MapPath("~/App_Data/");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string ruta = Path.Combine(dir, "errores.log");
        string linea = string.Format(
            "[{0:yyyy-MM-dd HH:mm:ss}] user={1} archivo=\"{2}\" excepcion={3} mensaje=\"{4}\"{5}",
            DateTime.Now, User.Identity.Name, archivo,
            ex.GetType().FullName, ex.Message, Environment.NewLine);
        System.IO.File.AppendAllText(ruta, linea);
    }
    catch { /* no romper la respuesta si falla el log */ }
}
```

### Paso 4.4 — Vista `ErrorReporte.cshtml`

```html
@model PortalReportesCrystal.Models.ErrorReporteViewModel
<div class="aviso aviso-error">
    <h2>No se pudo generar el reporte</h2>
    <p><strong>Reporte:</strong> @Model.NombreReporte</p>
    <p>@Model.Mensaje</p>
    <a href="@Url.Action("Index", "Home")" class="btn">Volver al listado</a>
</div>
```

### Verificación del módulo

Cargar un `.rpt` que requiera BD inaccesible → el navegador muestra la caja amigable con el mensaje de conexión. En `App_Data/errores.log` queda el detalle técnico con timestamp y usuario.

---

## Módulo 5. Múltiples raíces de reportes

### Objetivo

Que el portal lea `.rpt` desde varias carpetas configurables externamente, no solo de `~/Reportes/`.

### Teoría: por qué múltiples raíces

En un caso real, los `.rpt` viven en un servidor de archivos con estructura de carpetas por área (`\\servidor\Reportes\Bodega\`, `\\servidor\Reportes\Créditos\`, etc.). Copiarlos al proyecto duplicaría información. Es mejor apuntar el portal a esas carpetas y respetar su organización.

### Paso 5.1 — Archivo de configuración

`ReportesLocales/configuracion.json`:

```json
{
  "raices": [
    {
      "id": "crystalxi",
      "nombre": "Crystal XI",
      "ruta": "C:\\CRYSTAL\\06_output\\Input_Procesado",
      "prefijoGrupoRaiz": "Sin carpeta"
    },
    {
      "id": "sapbo",
      "nombre": "SAP BO 4.x",
      "ruta": "C:\\CRYSTAL\\06_output_sapbo\\Input_Procesado",
      "prefijoGrupoRaiz": "Sin carpeta"
    }
  ]
}
```

**Regla:** los `.rpt` sueltos en la raíz caen en el grupo `prefijoGrupoRaiz`. Las subcarpetas de primer nivel se convierten en grupos/categorías (Créditos, Bodega, Ventas, etc.).

### Paso 5.2 — Deserializar sin agregar librerías

.NET Framework ya incluye `JavaScriptSerializer` en `System.Web.Extensions`:

```csharp
using System.Web.Script.Serialization;

string json = System.IO.File.ReadAllText(cfgPath);
var conf = new JavaScriptSerializer().Deserialize<ConfiguracionRaices>(json);
```

### Paso 5.3 — Escanear las raíces desde el controlador

```csharp
private List<ReporteInfo> CargarReportesRaices()
{
    string cfgPath = Server.MapPath("~/ReportesLocales/configuracion.json");
    if (!System.IO.File.Exists(cfgPath)) return new List<ReporteInfo>();

    var conf = new JavaScriptSerializer()
        .Deserialize<ConfiguracionRaices>(System.IO.File.ReadAllText(cfgPath));

    var lista = new List<ReporteInfo>();
    foreach (var raiz in conf.Raices)
    {
        if (!Directory.Exists(raiz.Ruta)) continue;

        // Archivos sueltos en la raíz
        foreach (var f in Directory.GetFiles(raiz.Ruta, "*.rpt"))
            lista.Add(BuildReporte(raiz, raiz.PrefijoGrupoRaiz, Path.GetFileName(f)));

        // Subcarpetas → grupos
        foreach (var dir in Directory.GetDirectories(raiz.Ruta))
        {
            string grupo = Path.GetFileName(dir);
            foreach (var f in Directory.GetFiles(dir, "*.rpt", SearchOption.AllDirectories))
            {
                string relDesdeRaiz = f.Substring(raiz.Ruta.Length).TrimStart('\\', '/');
                lista.Add(BuildReporte(raiz, grupo, relDesdeRaiz));
            }
        }
    }
    return lista;
}
```

### Paso 5.4 — Resolución segura de rutas (anti-traversal)

Un atacante podría pedir `path=..\..\..\Windows\win.ini`. Hay que rechazar cualquier ruta que salga de la raíz declarada.

```csharp
private string ResolverRuta(string raizId, string pathRelativo)
{
    if (string.IsNullOrWhiteSpace(raizId) || string.IsNullOrWhiteSpace(pathRelativo))
        return null;

    string rutaBase = ObtenerRaiz(raizId)?.Ruta;
    if (string.IsNullOrWhiteSpace(rutaBase) || !Directory.Exists(rutaBase)) return null;

    string baseCanon = Path.GetFullPath(rutaBase).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
    string combinado = Path.GetFullPath(Path.Combine(rutaBase,
        pathRelativo.Replace('/', Path.DirectorySeparatorChar)));

    // Validación crítica: el path final DEBE empezar con la raíz canónica
    if (!combinado.StartsWith(baseCanon, StringComparison.OrdinalIgnoreCase))
        return null;

    return combinado;
}
```

### Verificación del módulo

Editar el JSON para apuntar a una carpeta real, refrescar la página → aparecen los reportes agrupados por subcarpeta. Intentar `path=..\..\Windows\...` en la URL → responde 404.

---

## Módulo 6. Reportes externos (SAP BO CMC)

### Objetivo

Agregar al listado enlaces directos a reportes publicados en SAP BusinessObjects Central Management Console.

### Teoría: OpenDocument URLs

SAP BO expone reportes vía URLs con formato estándar (`OpenDocument`):

```
http://SAPBO:8080/BOE/OpenDocument/opendoc/custom.jsp
    ?sIDType=CUID
    &iDocID=AZOBysqEYlpEnRqGRTJKNp0
    &sType=rpt
    &sOutputFormat=H
    &lsSALMACEN=06
    &lsSFECHA=20260510
    &sWindow=New
    &sRefresh=Y
```

Cada reporte tiene un **CUID** (identificador único). Los prompts se prellenan con `ls[S|N|D]NombrePrompt=valor` (`S`=string, `N`=number, `D`=date).

El portal solo genera el enlace y lo abre en pestaña nueva. La autenticación y el renderizado los maneja SAP BO.

### Paso 6.1 — Catálogo en JSON

`ReportesCMC/catalogo.json`:

```json
{
  "grupos": [
    {
      "nombre": "CREDITOS",
      "descripcion": "Reportes del área de Créditos",
      "reportes": [
        {
          "nombre": "DOCUMENTACION REMITIDA A CREDITOS",
          "descripcion": "Documentación remitida por almacén y fecha",
          "servidor": "SAP BO",
          "url": "http://SAPBO:8080/BOE/OpenDocument/opendoc/custom.jsp?sIDType=CUID&iDocID=AZOBysqEYlpEnRqGRTJKNp0&sType=rpt&sOutputFormat=H&lsSALMACEN=06&sWindow=New&sRefresh=Y"
        }
      ]
    }
  ]
}
```

### Paso 6.2 — Leerlo en el HomeController

```csharp
private List<ReporteInfo> CargarReportesCMC()
{
    string ruta = Server.MapPath("~/ReportesCMC/catalogo.json");
    if (!System.IO.File.Exists(ruta)) return new List<ReporteInfo>();

    var catalogo = new JavaScriptSerializer()
        .Deserialize<CatalogoCMC>(System.IO.File.ReadAllText(ruta));

    var lista = new List<ReporteInfo>();
    foreach (var grupo in catalogo.Grupos)
        foreach (var r in grupo.Reportes)
            lista.Add(new ReporteInfo
            {
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Categoria = grupo.Nombre,
                Tipo = TipoReporte.Externo,
                Servidor = r.Servidor,
                UrlExterna = r.Url
            });
    return lista;
}
```

### Paso 6.3 — Renderizado con `target="_blank"` y `rel="noopener"`

```html
@if (reporte.Tipo == TipoReporte.Externo)
{
    <a href="@reporte.UrlExterna" target="_blank" rel="noopener" class="btn">
        Abrir en @reporte.Servidor →
    </a>
}
```

**`rel="noopener"`** evita que la pestaña abierta pueda manipular `window.opener`. Es una buena práctica siempre que uses `target="_blank"`.

### Verificación del módulo

Editar el JSON, refrescar → el reporte aparece en el listado. Al hacer clic se abre en pestaña nueva y termina en SAP BO.

---

## Módulo 7. Detección y formulario de parámetros

### Objetivo

Que los reportes con prompts muestren un formulario para llenarlos antes de ejecutar.

### Teoría: `ParameterFields` del SDK

Un `.rpt` puede declarar parámetros que Crystal solicita al ejecutar. El SDK los expone así:

```csharp
foreach (ParameterField pf in report.ParameterFields)
{
    pf.Name;                          // nombre interno
    pf.PromptText;                    // texto mostrado al usuario
    pf.ParameterValueType;            // StringParameter, NumberParameter, DateParameter...
    pf.EnableNullValue;               // ¿puede quedar vacío?
    pf.EnableAllowMultipleValue;      // ¿acepta lista de valores?
    pf.ReportName;                    // vacío = del reporte principal; no vacío = subreporte
}
```

Los parámetros de subreportes no los llena el usuario final: Crystal los pasa desde el reporte principal.

### Paso 7.1 — Leer los parámetros

```csharp
private List<ParametroReporte> LeerParametros(string rutaFisica)
{
    var lista = new List<ParametroReporte>();
    var rd = new ReportDocument();
    try
    {
        rd.Load(rutaFisica);
        foreach (ParameterField pf in rd.ParameterFields)
        {
            if (pf.ReportName != null && pf.ReportName.Length > 0) continue;  // subreporte

            lista.Add(new ParametroReporte
            {
                Nombre = pf.Name,
                Etiqueta = string.IsNullOrEmpty(pf.PromptText) ? pf.Name : pf.PromptText,
                Tipo = MapearTipo(pf.ParameterValueType),
                Opcional = pf.EnableNullValue,
                MultiValor = pf.EnableAllowMultipleValue
            });
        }
    }
    finally
    {
        rd.Close();
        rd.Dispose();
    }
    return lista;
}
```

### Paso 7.2 — Aplicar los valores

Los valores llegan por querystring con prefijo `p_`: `?p_ALMACEN=06&p_FECHA=20260817`.

```csharp
private void AplicarValoresParametros(ReportDocument rd, Dictionary<string, string> valores)
{
    foreach (ParameterField pf in rd.ParameterFields)
    {
        if (pf.ReportName != null && pf.ReportName.Length > 0) continue;
        if (!valores.ContainsKey(pf.Name)) continue;

        object typed = ConvertirValor(valores[pf.Name], pf.ParameterValueType);
        rd.SetParameterValue(pf.Name, typed);
    }
}

private static object ConvertirValor(string bruto, ParameterValueKind tipo)
{
    switch (tipo)
    {
        case ParameterValueKind.NumberParameter:
        case ParameterValueKind.CurrencyParameter:
            return decimal.Parse(bruto, CultureInfo.InvariantCulture);
        case ParameterValueKind.DateParameter:
        case ParameterValueKind.DateTimeParameter:
            return DateTime.Parse(bruto, CultureInfo.InvariantCulture);
        case ParameterValueKind.BooleanParameter:
            return bruto == "true" || bruto == "1";
        default:
            return bruto;
    }
}
```

### Paso 7.3 — Redireccionar Exportar → Ver si faltan parámetros

Cuando el usuario hace clic directamente en "PDF" desde el listado y el reporte necesita parámetros, se redirige al formulario. Se preserva el formato deseado.

```csharp
var faltantes = ParametrosFaltantes(ruta);
if (faltantes.Count > 0)
{
    var rvd = new RouteValueDictionary { ["raizId"] = raizId, ["path"] = path };
    if (!string.IsNullOrEmpty(formato)) rvd["formato"] = formato;
    foreach (var k in ExtraerValoresDelQueryString())
        rvd["p_" + k.Key] = k.Value;
    return RedirectToAction("Ver", rvd);
}
```

### Paso 7.4 — Formulario dinámico según tipo

```html
@switch (p.Tipo)
{
    case "Number":
    case "Currency":
        <input type="number" step="any" name="p_@p.Nombre" @(p.Opcional ? "" : "required") />
        break;
    case "Date":
        <input type="date" name="p_@p.Nombre" @(p.Opcional ? "" : "required") />
        break;
    case "DateTime":
        <input type="datetime-local" name="p_@p.Nombre" @(p.Opcional ? "" : "required") />
        break;
    case "Boolean":
        <select name="p_@p.Nombre" @(p.Opcional ? "" : "required")>
            <option value="true">Sí</option>
            <option value="false">No</option>
        </select>
        break;
    default:
        <input type="text" name="p_@p.Nombre" @(p.Opcional ? "" : "required") />
        break;
}
```

### Verificación del módulo

Cargar un reporte con 3 parámetros → aparece el formulario. Llenarlos y enviar → se muestra el visor con los valores aplicados.

---

## Módulo 8. Cache de parámetros y estado por reporte

### Objetivo

- **Cache de parámetros:** saber por cada `.rpt` si tiene o no prompts, sin cargar el SDK en cada refresh (200+ archivos = 40+ segundos por página).
- **Estado por reporte:** marcar los reportes que fallaron para que se vean señalizados en el listado.

### Teoría: escaneo en background

Al iniciar la aplicación (`Application_Start`), se dispara un `Task.Run` que escanea todos los `.rpt` de todas las raíces y guarda `{ruta → tieneParametros}` en memoria. El listado consulta esa memoria, no el SDK. La primera petición devuelve el HTML instantáneamente aunque el cache aún no esté lleno; los reportes se marcan como "Analizando" hasta que se procesen.

### Paso 8.1 — Cache thread-safe

```csharp
public static class CacheParametros
{
    private class Entrada
    {
        public bool TieneParametros;
        public int Cantidad;
        public long UltimaModTicks;
    }
    private static readonly ConcurrentDictionary<string, Entrada> _mapa
        = new ConcurrentDictionary<string, Entrada>(StringComparer.OrdinalIgnoreCase);

    public static bool? Analizar(string rutaAbsoluta)
    {
        if (!_mapa.TryGetValue(rutaAbsoluta, out var e)) return null;
        // Invalidar si el archivo cambió desde el cache
        long actual = File.GetLastWriteTimeUtc(rutaAbsoluta).Ticks;
        if (actual != e.UltimaModTicks) { _mapa.TryRemove(rutaAbsoluta, out _); return null; }
        return e.TieneParametros;
    }

    public static void IniciarEscaneoBackground(IEnumerable<string> raices)
    {
        Task.Run(() =>
        {
            foreach (var raiz in raices)
                foreach (var archivo in Directory.EnumerateFiles(raiz, "*.rpt", SearchOption.AllDirectories))
                    AnalizarArchivo(archivo);
            Guardar();
        });
    }
    // ... AnalizarArchivo, Inicializar/Guardar a JSON en App_Data ...
}
```

### Paso 8.2 — Estado por reporte (marca de error)

```csharp
public static class EstadoReportes
{
    public class Estado
    {
        public bool ConError { get; set; }
        public string Mensaje { get; set; }
        public string FechaIso { get; set; }
        public string Usuario { get; set; }
        public int Repeticiones { get; set; }
    }

    public static void RegistrarError(string clave, string mensaje, string usuario) { ... }
    public static void RegistrarExito(string clave) { ... }
    public static Estado Obtener(string clave) { ... }

    // La clave estable identifica al reporte independientemente de la sesión
    public static string ClaveDeLocal(string raizId, string pathRel)
        => "local:" + raizId.ToLowerInvariant() + "/" + pathRel.Replace('\\', '/');
}
```

### Paso 8.3 — Registrar éxito o error en cada intento

```csharp
try
{
    // ... exportar ...
    EstadoReportes.RegistrarExito(EstadoReportes.ClaveDeLocal(raizId, path));  // ← limpia marca
    return File(...);
}
catch (Exception ex)
{
    EstadoReportes.RegistrarError(
        EstadoReportes.ClaveDeLocal(raizId, path),
        MensajeCorto(ex),
        User.Identity.Name);
    return VistaDeError(path, ex);
}
```

### Paso 8.4 — Arrancar todo desde `Global.asax`

```csharp
protected void Application_Start()
{
    // ... rutas ...
    string appData = Server.MapPath("~/App_Data");
    CacheParametros.Inicializar(appData);
    EstadoReportes.Inicializar(appData);
    CacheParametros.IniciarEscaneoBackground(ObtenerRaicesDeReportes());
}
```

### Verificación del módulo

- Al arrancar, todos los reportes empiezan como "Analizando"
- En 1–3 minutos aparecen clasificados como "Sí"/"No" tienen parámetros
- Al reiniciar, la clasificación queda persistida en `App_Data/parametros_cache.json`
- Un reporte que falla queda marcado con badge rojo "⚠ Con problemas"
- Al ejecutarse con éxito, la marca desaparece automáticamente

---

## Módulo 9. Vista previa condensada con PDFsharp

### Objetivo

Que el visor embebido cargue rápido incluso para reportes grandes: solo las 3 primeras páginas + una hoja separadora + las 3 últimas.

### Teoría: por qué recortar el PDF

Un reporte de 66 páginas tarda ~4 segundos en renderizar completo en el visor del navegador. Recortado a 7 páginas tarda ~1 segundo. La reducción beneficia sobre todo la percepción de velocidad: para verificar que el reporte "corrió bien", no hace falta ver todas las páginas intermedias.

### Paso 9.1 — Instalar PDFsharp

Vía NuGet:

```bash
nuget install PDFsharp -Version 1.50.5147 -OutputDirectory packages
```

En el `.csproj`:

```xml
<Reference Include="PdfSharp">
  <HintPath>packages\PDFsharp.1.50.5147\lib\net20\PdfSharp.dll</HintPath>
</Reference>
```

### Paso 9.2 — Función para condensar

```csharp
private const int PAGINAS_MUESTRA = 3;

private static byte[] CondensarPdf(byte[] pdfCompleto, int muestraCadaLado)
{
    using (var msIn = new MemoryStream(pdfCompleto))
    {
        var origen = PdfSharp.Pdf.IO.PdfReader.Open(msIn,
            PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

        int total = origen.PageCount;
        if (total <= muestraCadaLado * 2) return pdfCompleto;   // cabe entero

        var destino = new PdfSharp.Pdf.PdfDocument();
        destino.Info.Title = origen.Info.Title;

        for (int i = 0; i < muestraCadaLado; i++)
            destino.AddPage(origen.Pages[i]);

        DibujarSeparador(destino, origen.Pages[0].Width, origen.Pages[0].Height,
                         total, muestraCadaLado);

        for (int i = total - muestraCadaLado; i < total; i++)
            destino.AddPage(origen.Pages[i]);

        using (var msOut = new MemoryStream())
        {
            destino.Save(msOut, closeStream: false);
            return msOut.ToArray();
        }
    }
}
```

### Paso 9.3 — Página separadora institucional

Se dibuja al mismo tamaño que las páginas del reporte para no romper el flujo visual:

```csharp
private static void DibujarSeparador(PdfDocument doc, XUnit ancho, XUnit alto,
                                     int totalPaginas, int muestraCadaLado)
{
    var pagina = doc.AddPage();
    pagina.Width = ancho; pagina.Height = alto;
    using (var gfx = XGraphics.FromPdfPage(pagina))
    {
        var azul = XColor.FromArgb(0x00, 0x15, 0x22);       // color primario
        var rojo = XColor.FromArgb(0xFF, 0x00, 0x00);       // color acento

        gfx.DrawRectangle(new XSolidBrush(azul), new XRect(0, 0, pagina.Width, 40));
        gfx.DrawRectangle(new XSolidBrush(rojo), new XRect(0, 40, pagina.Width, 3));

        gfx.DrawString("Páginas intermedias omitidas",
            new XFont("Calibri", 22, XFontStyle.Bold),
            new XSolidBrush(azul),
            new XRect(0, alto/2 - 60, ancho, 40),
            XStringFormats.TopCenter);

        int intermedias = totalPaginas - muestraCadaLado * 2;
        gfx.DrawString(
            $"Se omitieron {intermedias:N0} páginas intermedias de un total de {totalPaginas:N0}.",
            new XFont("Calibri", 12), XBrushes.Black,
            new XRect(0, alto/2 + 15, ancho, 20),
            XStringFormats.TopCenter);
    }
}
```

### Paso 9.4 — Endpoint que devuelve el PDF condensado

```csharp
public ActionResult Preview(string raizId, string path)
{
    string ruta = ResolverRuta(raizId, path);
    if (ruta == null) return HttpNotFound();

    var rd = new ReportDocument();
    try
    {
        rd.Load(ruta);
        AplicarValoresParametros(rd, ExtraerValoresDelQueryString());

        byte[] pdfCompleto;
        using (var s = rd.ExportToStream(ExportFormatType.PortableDocFormat))
        using (var ms = new MemoryStream())
        {
            s.CopyTo(ms);
            pdfCompleto = ms.ToArray();
        }

        byte[] condensado = CondensarPdf(pdfCompleto, PAGINAS_MUESTRA);
        Response.AddHeader("Content-Disposition",
            "inline; filename=\"" + Path.GetFileNameWithoutExtension(path) + "_preview.pdf\"");
        return File(condensado, "application/pdf");
    }
    finally { rd.Close(); rd.Dispose(); }
}
```

### Paso 9.5 — Visor en la vista

```html
<iframe src="@Url.Action("Preview", "Reportes", new { raizId = raizId, path = path })"
        width="100%" height="900px" frameborder="0"
        onload="document.getElementById('cargando-visor').classList.add('oculto')">
</iframe>
```

Se acompaña de un overlay con spinner que se oculta al terminar de cargar:

```html
<div class="cargando-overlay" id="cargando-visor">
    <div class="spinner"></div>
    <p>Generando vista previa...</p>
</div>
```

### Verificación del módulo

Cargar un reporte de más de 6 páginas → el PDF embebido tiene 7 páginas (3 + separadora + 3) y peso reducido a la mitad.

---

## Módulo 10. Búsqueda, filtros y agrupación

### Objetivo

Que el listado sea usable con 200+ reportes: carpetas plegables, búsqueda por nombre y filtros por parámetros / origen / estado.

### Teoría: filtrado del lado del cliente

Con menos de ~2000 filas, filtrar en JavaScript sobre el DOM es más rápido y responsivo que ir al servidor con AJAX. La página se renderiza una vez con todos los reportes y sus `data-attributes`, y JS los oculta/muestra según los filtros.

### Paso 10.1 — Agrupar con `<details>/<summary>` nativos

```html
<details class="grupo-reportes" data-grupo="@grupo.Key">
    <summary class="grupo-titulo">
        <span>📁</span> @grupo.Key
        <span class="grupo-conteo">@grupo.Count() reportes</span>
    </summary>
    <table>
        <!-- filas -->
    </table>
</details>
```

**Ventaja:** cero JavaScript necesario para plegar/desplegar. La flecha ▾ se rota con CSS `[open]`.

### Paso 10.2 — Data-attributes para filtros

```html
<tr data-tipo="@dataTipo"
    data-param="@dataParam"
    data-estado="@dataEstado"
    data-busq="@textoConcatenadoMinusculas">
    ...
</tr>
```

### Paso 10.3 — JavaScript de filtrado

```javascript
function aplicarFiltros() {
    var busqLower = textoBusqueda.toLowerCase().trim();

    document.querySelectorAll('.lista-carpetas details').forEach(grupo => {
        var visibles = 0;
        grupo.querySelectorAll('tbody tr').forEach(fila => {
            var pasa =
                (filtroTipo === 'todos'   || fila.dataset.tipo   === filtroTipo)   &&
                (filtroParam === 'todos'  || fila.dataset.param  === filtroParam)  &&
                (filtroEstado === 'todos' || fila.dataset.estado === filtroEstado) &&
                (busqLower === ''         || fila.dataset.busq.indexOf(busqLower) >= 0);
            fila.style.display = pasa ? '' : 'none';
            if (pasa) visibles++;
        });
        grupo.style.display = visibles ? '' : 'none';
        if (busqLower && visibles) grupo.setAttribute('open', '');
    });
}

document.getElementById('busqueda').addEventListener('input', e => {
    textoBusqueda = e.target.value;
    clearTimeout(busqTimeout);
    busqTimeout = setTimeout(aplicarFiltros, 150);   // debounce
});
```

**Debounce de 150 ms** evita filtrar en cada tecla: se espera a que el usuario pare de escribir por 150 ms antes de actualizar el DOM.

### Verificación del módulo

- Con 210 reportes, filtrar por "cred" muestra solo los que contienen esa palabra
- Al filtrar, las carpetas vacías se ocultan automáticamente
- Los contadores por grupo se actualizan
- Al hacer clic en el chip "Sin parámetros", solo quedan los que se pueden ejecutar sin prompts

---

## Módulo 11. Identidad corporativa

### Objetivo

Aplicar la paleta de colores y el logo de la organización sin complicar el mantenimiento.

### Teoría: variables CSS

Definir los colores como custom properties permite cambiarlos en un solo lugar (`:root`) y que se propaguen a todo el sitio.

### Paso 11.1 — Variables en `:root`

```css
:root {
    --sr-primario:     #001522;
    --sr-primario-alt: #002236;
    --sr-acento:       #FF0000;
    --sr-acento-alt:   #CC0000;
    --sr-fondo:        #F5F5F5;
    --sr-blanco:       #FFFFFF;
    --sr-borde:        #E5E5E5;
    --sr-texto:        #333333;
    --sr-texto-sec:    #666666;
}
```

Uso posterior:
```css
header nav { background: var(--sr-primario); border-bottom: 3px solid var(--sr-acento); }
.btn { background: var(--sr-acento); color: var(--sr-blanco); }
```

Para reemplazar la paleta cuando cambie el manual de marca, se editan estas 9 líneas y punto.

### Paso 11.2 — Logo en SVG

Un SVG:
- Escala a cualquier resolución sin perder nitidez
- Pesa 5–50 KB
- Puede usarse como favicon con `<link rel="icon" type="image/svg+xml" href="...">`
- Se puede recolorear via CSS `filter` o editando el XML

### Paso 11.3 — Extraer colores oficiales del logo

Los HEX del manual de marca pueden diferir sutilmente del logo real. Extraerlos del SVG garantiza consistencia perfecta:

```powershell
$logo = Get-Content "logo.svg" -Raw
[regex]::Matches($logo, 'fill:\s*#([0-9A-Fa-f]{6})') |
  ForEach-Object { "#$($_.Groups[1].Value)".ToUpper() } |
  Group-Object | Sort-Object Count -Descending
```

### Verificación del módulo

- El encabezado del portal muestra el logo corporativo
- La paleta se refleja en botones, títulos y footer
- El favicon del navegador es el mismo logo

---

## Módulo 12. Descubrimiento automático de reportes vía API REST SAP BO

### Objetivo

Que el portal consulte la API REST de SAP BusinessObjects para descubrir automáticamente todos los reportes WebI y Crystal Reports (.rpt / CR4E) publicados en el servidor, organizándolos por la estructura de carpetas del CMS.

### Teoría: API REST de SAP BO (`/biprws/`)

SAP BO expone un servicio RESTful en el puerto 6405 bajo la ruta `/biprws/`. Los endpoints relevantes son:

| Endpoint | Método | Propósito |
|---|---|---|
| `/biprws/logon/long` | POST | Autenticación (XML body, devuelve token) |
| `/biprws/raylight/v1/documents` | GET | Listado de documentos WebI |
| `/biprws/infostore` | GET | Raíz del CMS (carpetas de nivel superior) |
| `/biprws/infostore/<id>/children` | GET | Hijos de una carpeta |
| `/biprws/infostore/<id>` | GET | Detalle de un objeto (nombre, tipo) |

**Autenticación:** cada petición lleva el encabezado `X-SAP-LogonToken` con el token obtenido del logon. El token tiene caducidad configurable en el servidor.

**Tipos de objeto en el CMS:** `Folder`, `User`, `PersonalCategory`, `CrystalReport`, `CR4E`, `Webi`, `CCIS.DataConnection`, `DSL.Universe`, `Calendar`, `RecycleBin`, entre otros.

### Arquitectura del descubrimiento

```
Application_Start
    │
    ▼
SapBoClient.ObtenerReportes()         ←  cache en memoria (15 min TTL)
    │
    ├─► ConsultarWebI(token)          →  GET /raylight/v1/documents
    │                                     Devuelve documentos WebI con carpeta y CUID
    │
    └─► ConsultarCrystalReports(token)
            │
            ├─► GET /infostore        →  Obtener carpetas raíz del CMS
            │       Filtrar por whitelist: "Carpeta raíz", "Carpetas de usuario"
            │
            └─► BuscarCrystalEnCarpeta(id, depth=0)    ←  recursivo, profundidad máx. 7
                    │
                    ├─► GET /infostore/<id>/children?pageSize=200
                    │
                    ├── Si tipo = Folder/User/PersonalCategory → recursar
                    │       (excluir carpetas de sistema)
                    │
                    └── Si tipo contiene CrystalReport/CR4E → agregar a resultados
                            con URL OpenDocument construida desde el CUID
```

### Paso 12.1 — Configuración en `Web.config`

La conexión al servidor SAP BO se configura en `appSettings`. Las credenciales deben protegerse con `aspnet_regiis -pe` en producción.

```xml
<appSettings>
  <add key="SapBoUrl" value="http://SAPBO:6405/biprws" />
  <add key="SapBoOpenDocUrl"
       value="http://SAPBO:8080/BOE/OpenDocument/opendoc/custom.jsp" />
  <add key="SapBoUsuario" value="(usuario)" />
  <add key="SapBoClave" value="(clave)" />
  <add key="SapBoTipoAuth" value="secEnterprise" />
</appSettings>
```

### Paso 12.2 — Clase `SapBoClient`: autenticación

```csharp
public static class SapBoClient
{
    private static readonly string ApiUrl =
        ConfigurationManager.AppSettings["SapBoUrl"] ?? "";
    private static readonly string OpenDocUrl =
        ConfigurationManager.AppSettings["SapBoOpenDocUrl"] ?? "";
    public static bool Habilitado => !string.IsNullOrWhiteSpace(ApiUrl);

    private static string IniciarSesion()
    {
        string xml = string.Format(
            "<attrs><attr name=\"userName\" type=\"string\">{0}</attr>" +
            "<attr name=\"password\" type=\"string\">{1}</attr>" +
            "<attr name=\"auth\" type=\"string\">{2}</attr></attrs>",
            SecurityElement.Escape(usuario),
            SecurityElement.Escape(clave),
            SecurityElement.Escape(tipoAuth));

        var req = (HttpWebRequest)WebRequest.Create(ApiUrl + "/logon/long");
        req.Method = "POST";
        req.ContentType = "application/xml";
        req.Accept = "application/json";

        using (var sw = new StreamWriter(req.GetRequestStream()))
            sw.Write(xml);

        using (var resp = (HttpWebResponse)req.GetResponse())
        using (var sr = new StreamReader(resp.GetResponseStream()))
        {
            string body = sr.ReadToEnd();
            var js = new JavaScriptSerializer();
            var obj = js.Deserialize<Dictionary<string, object>>(body);
            return obj.ContainsKey("logonToken") ? obj["logonToken"].ToString() : "";
        }
    }
}
```

**Punto de seguridad:** se usa `SecurityElement.Escape()` para evitar inyección XML en el body de autenticación.

### Paso 12.3 — Descubrimiento de documentos WebI

```csharp
private static List<ReporteWebI> ConsultarWebI(string token)
{
    string url = ApiUrl + "/raylight/v1/documents";
    string body = HacerGetAutenticado(url, token);
    var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
    var root = js.Deserialize<Dictionary<string, object>>(body);

    var lista = new List<ReporteWebI>();
    // Navegar: root → "documents" → "document" (array)
    if (root.ContainsKey("documents"))
    {
        var docs = root["documents"] as Dictionary<string, object>;
        if (docs != null && docs.ContainsKey("document"))
        {
            var arr = docs["document"] as System.Collections.ArrayList;
            foreach (Dictionary<string, object> d in arr)
            {
                lista.Add(new ReporteWebI
                {
                    Nombre = ObtenerValorStr(d, "name"),
                    CUID = ObtenerValorStr(d, "cuid") ?? "",
                    Carpeta = ObtenerValorStr(d, "foldername") ?? "Sin carpeta",
                    Descripcion = ObtenerValorStr(d, "description") ?? "",
                    TipoDocumento = "WebI"
                });
            }
        }
    }
    return lista;
}
```

### Paso 12.4 — Descubrimiento recursivo de Crystal Reports en el CMS

Este es el núcleo de la integración. Recorre el árbol del CMS buscando objetos de tipo `CrystalReport` y `CR4E`.

```csharp
private static readonly HashSet<string> _carpetasExcluidas =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Almacenamiento temporal", "Complementos de escritorio",
        "Demonstration", "Feature Samples", "Report Samples",
        "Alert Notifications", "Personal Folders", "Categories",
        "Instances", "Temporary Storage", "Desktop Add-ons",
        "~WebIntelligence"
    };

private static List<ReporteWebI> ConsultarCrystalReports(string token)
{
    var resultados = new List<ReporteWebI>();
    string respBody = HacerGetAutenticado(ApiUrl + "/infostore", token);
    var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
    var root = js.Deserialize<Dictionary<string, object>>(respBody);
    var folders = ExtraerEntries(root);

    // Solo explorar las carpetas públicas y de usuario
    var carpetasRaizPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Carpeta raíz", "Carpeta raiz", "Root Folder",
        "Carpetas de usuario", "User Folders"
    };

    foreach (Dictionary<string, object> folder in folders)
    {
        string folderId = ObtenerValorStr(folder, "id");
        string folderName = ObtenerValorStr(folder, "name");
        if (string.IsNullOrEmpty(folderId)) continue;
        if (!carpetasRaizPermitidas.Contains(folderName ?? "")) continue;

        BuscarCrystalEnCarpeta(folderId, folderName ?? "Raiz",
                               token, resultados, js, 0);
    }
    return resultados;
}
```

**Decisión de diseño:** se usa un whitelist de carpetas raíz en lugar de escanear las ~40 carpetas de sistema del CMS. Esto reduce drásticamente el tiempo de escaneo y evita consultas inútiles a carpetas como Bandejas de entrada, Calendarios, Acciones de cliente, etc.

### Paso 12.5 — Función recursiva `BuscarCrystalEnCarpeta`

```csharp
private static void BuscarCrystalEnCarpeta(
    string folderId, string folderName, string token,
    List<ReporteWebI> resultados, JavaScriptSerializer js, int depth)
{
    if (depth > 7) return;  // profundidad máxima de seguridad

    string url = ApiUrl + "/infostore/" + Uri.EscapeDataString(folderId)
                 + "/children?pageSize=200";
    string respBody = HacerGetAutenticado(url, token);
    var root = js.Deserialize<Dictionary<string, object>>(respBody);
    var entries = ExtraerEntries(root);
    if (entries == null || entries.Count == 0) return;

    foreach (Dictionary<string, object> entry in entries)
    {
        string tipo = ObtenerValorStr(entry, "type");
        string nombre = ObtenerValorStr(entry, "name");
        string cuid = ObtenerValorStr(entry, "cuid");
        string entryId = ObtenerValorStr(entry, "id");

        // Recursar en carpetas, usuarios y categorías personales
        if (tipo == "Folder" || tipo == "User" || tipo == "PersonalCategory")
        {
            if (!_carpetasExcluidas.Contains(nombre ?? ""))
                BuscarCrystalEnCarpeta(entryId, nombre ?? folderName,
                                       token, resultados, js, depth + 1);
            continue;
        }

        // Verificar si es un Crystal Report o CR4E
        bool esCrystal = tipo != null &&
            (tipo.IndexOf("CrystalReport", StringComparison.OrdinalIgnoreCase) >= 0
             || tipo.IndexOf("Crystal Report", StringComparison.OrdinalIgnoreCase) >= 0
             || tipo.IndexOf("CR4E", StringComparison.OrdinalIgnoreCase) >= 0);
        if (!esCrystal) continue;

        // Construir URL de OpenDocument
        string urlDoc = "";
        if (!string.IsNullOrEmpty(OpenDocUrl) && !string.IsNullOrEmpty(cuid))
        {
            urlDoc = OpenDocUrl + "?iDocID=" + Uri.EscapeDataString(cuid)
                + "&sIDType=CUID&sType=rpt&sOutputFormat=H"
                + "&sWindow=New&sRefresh=Y";
        }

        resultados.Add(new ReporteWebI
        {
            Nombre = nombre ?? "(sin nombre)",
            CUID = cuid ?? "",
            Carpeta = folderName,
            TipoDocumento = "CrystalReport"
        });
    }
}
```

**Puntos clave de la recursión:**

1. **Profundidad máxima = 7:** las estructuras reales del CMS (como NIIF > COBROS > subreportes) llegan hasta 5–6 niveles. 7 es un margen de seguridad.

2. **Tipos recursivos:** no solo `Folder`, también `User` y `PersonalCategory`. Las "Carpetas de usuario" del CMS contienen entradas de tipo `User` (Administrator, aojst, etc.), no `Folder`. Sin incluir `User` en la condición, las carpetas personales de usuario nunca se exploran.

3. **Exclusión de carpetas de sistema:** se filtran tanto en la raíz como en niveles recursivos para evitar entrar en Demonstration, Feature Samples, etc.

4. **`pageSize=200`:** por defecto la API devuelve solo 10 hijos. Subir a 200 reduce el número de peticiones paginadas.

### Paso 12.6 — Cache con TTL para reducir carga al servidor

```csharp
private static List<ReporteWebI> _cacheReportes;
private static DateTime _cacheExpira = DateTime.MinValue;
private static readonly object _lock = new object();
private static readonly int CacheMinutos = 15;

public static List<ReporteWebI> ObtenerReportes()
{
    lock (_lock)
    {
        if (_cacheReportes != null && DateTime.UtcNow < _cacheExpira)
        {
            DatosDesdeCache = true;
            return _cacheReportes;
        }
    }

    var token = IniciarSesion();
    var webi = ConsultarWebI(token);
    var crystal = ConsultarCrystalReports(token);
    var todos = new List<ReporteWebI>();
    todos.AddRange(webi);
    todos.AddRange(crystal);

    lock (_lock)
    {
        _cacheReportes = todos;
        _cacheExpira = DateTime.UtcNow.AddMinutes(CacheMinutos);
        UltimaActualizacion = DateTime.Now;
        DatosDesdeCache = false;
    }
    return todos;
}
```

El primer request de cada ciclo de 15 minutos tarda 5–15 segundos (según cantidad de carpetas). Los subsiguientes responden instantáneamente desde cache.

### Paso 12.7 — Integración con el `HomeController`

Los reportes descubiertos se agregan como `TipoReporte.WebI` con un `Servidor` que distingue su origen:

```csharp
private List<ReporteInfo> CargarReportesWebI()
{
    if (!SapBoClient.Habilitado) return new List<ReporteInfo>();

    return SapBoClient.ObtenerReportes()
        .Select(w => new ReporteInfo
        {
            Nombre = w.Nombre,
            Categoria = w.Carpeta,
            Tipo = TipoReporte.WebI,
            Servidor = w.TipoDocumento == "CrystalReport"
                ? "SAP BO .rpt" : "SAP BO WebI",
            UrlExterna = w.UrlOpenDocument
        })
        .ToList();
}
```

### Paso 12.8 — Secciones colapsables por origen en la vista

El listado se divide en dos secciones usando `<details>/<summary>` HTML nativo:

- **Server Report:** reportes locales del disco (`.rpt` de las raíces configuradas)
- **SAP BO:** solo reportes descubiertos vía API (WebI y Crystal Reports del servidor)

```html
<details class="seccion-origen" open>
    <summary class="seccion-origen-titulo">
        <span class="seccion-origen-flecha">&#9660;</span>
        Server Report
        <span class="seccion-origen-conteo">@serverReportCount</span>
    </summary>
    <!-- Grupos de reportes locales -->
</details>

<details class="seccion-origen" open>
    <summary class="seccion-origen-titulo">
        <span class="seccion-origen-flecha">&#9660;</span>
        SAP BO
        <span class="seccion-origen-conteo">@sapBoCount</span>
    </summary>
    <!-- Grupos de reportes WebI y Crystal del servidor -->
</details>
```

Cada sección se puede colapsar independientemente. La función `aplicarFiltros()` de JavaScript oculta secciones completas cuando ninguno de sus reportes pasa los filtros activos.

```css
.seccion-origen > summary { list-style: none; cursor: pointer; user-select: none; }
.seccion-origen > summary::-webkit-details-marker { display: none; }
.seccion-origen-flecha { transition: transform 0.2s; display: inline-block; }
.seccion-origen:not([open]) .seccion-origen-flecha { transform: rotate(-90deg); }
```

### Paso 12.9 — Badges diferenciados

Los reportes del servidor SAP BO se distinguen visualmente con badges de color:

| Badge | Condición | Significado |
|---|---|---|
| `badge-sapbo` | TipoDocumento = CrystalReport | `.rpt` publicado en SAP BO |
| `badge-webi` | TipoDocumento = WebI | Documento Web Intelligence |
| `badge-local` | Tipo = Local | `.rpt` local del disco |

### Paso 12.10 — Log y diagnóstico

El `SapBoClient` registra en `App_Data/errores_sapbo.log`:

- Token obtenido (sin exponer el valor)
- Cantidad de WebI encontrados
- Cantidad de Crystal Reports encontrados
- Errores de conexión con stack trace

```csharp
private static void RegistrarInfo(string mensaje)
{
    string dir = ObtenerRutaAppData();
    string ruta = Path.Combine(dir, "errores_sapbo.log");
    string linea = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [INFO] {1}{2}",
        DateTime.Now, mensaje, Environment.NewLine);
    File.AppendAllText(ruta, linea);
}
```

### Lecciones aprendidas y errores comunes

1. **`/infostore/search` y `/cmsquery` no existen** en todas las versiones de SAP BO. Intentar usarlos devuelve 404. La única forma confiable es la recursión por `/infostore/<id>/children`.

2. **El parámetro `type=CrystalReport` no filtra en la API.** La API lo ignora y devuelve todo. El filtro debe hacerse en código.

3. **Las carpetas de usuario tienen tipo `User`, no `Folder`.** Este es el error más sutil: si la recursión solo entra en tipo `Folder`, nunca explorará las carpetas personales de los usuarios del CMS, donde muchos reportes están publicados.

4. **La estructura del JSON varía entre endpoints.** `/raylight/v1/documents` usa `documents.document[]`, mientras que `/infostore` usa `entries[]`. Cada uno necesita su propio parser.

5. **La profundidad necesaria depende de la organización.** La estructura `Administrator > FINANZAS DEV > NIIF > COBROS > reporte.rpt` necesita al menos 5 niveles. Se recomienda configurar 7 como margen.

### Verificación del módulo

- Al cargar el portal, la sección SAP BO muestra los reportes descubiertos del servidor
- Los reportes aparecen organizados por la carpeta del CMS donde están publicados
- Se distinguen visualmente los WebI de los Crystal Reports (.rpt y CR4E)
- El log `errores_sapbo.log` confirma la cantidad de documentos encontrados
- Al refrescar dentro de los 15 minutos, los datos vienen de cache (instantáneo)
- Los reportes de carpetas de sistema (Demonstration, Feature Samples) no aparecen
- Los reportes dentro de carpetas de usuario (Administrator, etc.) sí aparecen

---

## Módulo 13. Página de estadísticas SAP BO (sesiones, licencias, servidores)

### Objetivo

Agregar al portal una página administrativa que consulte en tiempo real la API REST del CMS de SAP BusinessObjects y muestre: sesiones activas, licencias instaladas, estado de los servidores y un resumen de reportes descubiertos. La página es accesible desde un enlace en la barra de navegación y se refresca bajo demanda.

### Teoría: por qué esta página

El administrador del portal necesita visibilidad rápida sobre:

- **¿Cuánta capacidad de licencia queda?** — para dimensionar el crecimiento
- **¿Quién está conectado ahora mismo?** — para diagnosticar rendimiento
- **¿Todos los servidores están vivos?** — para detectar caídas antes del usuario final
- **¿Cuántos reportes descubre la integración?** — para validar el módulo 12

Todo esto ya existe en la Consola CMC de SAP BO, pero requiere abrir otra herramienta con permisos administrativos. Exponerlo en el propio portal lo hace parte del monitoreo cotidiano del área de BI.

### Teoría: `cmsquery`, el único punto de entrada real

Después de explorar los endpoints REST documentados por SAP (`/sessions`, `/servers`, `/logon/sessioninfo`, `/infostore/licenses`, etc.), se descubre que **en la mayoría de las versiones 4.x ninguno de ellos existe**. El servidor responde 404 con `{"error_code":"RWS 00005"}`.

El único endpoint universalmente disponible que puede leer objetos del CMS es:

```
POST /biprws/v1/cmsquery
```

Acepta una sentencia SQL contra `CI_SYSTEMOBJECTS` (o `CI_INFOOBJECTS`, `CI_APPOBJECTS`) y devuelve un JSON con la propiedad `entries[]`. Toda la página de estadísticas se apoya en él.

**Cuerpo del request:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<attrs xmlns="http://www.sap.com/rws/bip">
  <attr name="query" type="string">
    SELECT SI_ID,SI_NAME,SI_KIND FROM CI_SYSTEMOBJECTS WHERE SI_KIND='Server'
  </attr>
</attrs>
```

**Headers:**

- `Content-Type: application/xml`
- `Accept: application/json`
- `X-SAP-LogonToken: "<token>"` (con comillas literales)

**Errores comunes:**

| HTTP | Significado |
|---|---|
| 400 RWS 000026 | Body con formato incorrecto (falta atributo `name`) |
| 401 RWS 00203 | Token expirado o inválido |
| 403 | Usuario sin permisos administrativos |
| 404 RWS 00005 | El servidor no expone `/v1/cmsquery` en esta versión |

### Tipos de objetos consultables (`SI_KIND`)

| SI_KIND | Contenido | Campos útiles |
|---|---|---|
| `Server` | Servidores registrados en el CMS | SI_NAME, SI_DISABLED, SI_SERVER_IS_ALIVE, SI_DESCRIPTION |
| `LicenseKey` | Claves de licencia instaladas | SI_KEYCODE, SI_EXPIRY_DATE, SI_USER_COUNT, SI_CONCURRENT_USER_COUNT |
| `Connection` | Sesiones activas | SI_USERFULLNAME, SI_AUTHEN_METHOD, SI_LOGON_TIME |
| `User` | Usuarios definidos | SI_NAME, SI_EMAIL |
| `UserGroup` | Grupos de usuarios | SI_NAME, SI_DESCRIPTION |
| `FRSInputTotalSize` | Tamaño del FRS de entrada | valor único |

### Arquitectura

```
Navegador
    │
    │ GET /Home/Estadisticas
    ▼
HomeController.Estadisticas()
    │
    ├─► SapBoClient.ObtenerReportes()      ← cache (15 min)
    │
    ├─► SapBoClient.ConsultarSesiones()    ← en vivo
    │       │
    │       ▼
    │   POST /biprws/v1/cmsquery
    │   { query: "SELECT ... WHERE SI_KIND='Connection'" }
    │
    ├─► SapBoClient.ConsultarLicencias()   ← en vivo
    │       │
    │       ▼
    │   POST /biprws/v1/cmsquery
    │   { query: "SELECT ... WHERE SI_KIND='LicenseKey'" }
    │
    └─► SapBoClient.ConsultarServidores()  ← en vivo
            │
            ▼
        POST /biprws/v1/cmsquery
        { query: "SELECT ... WHERE SI_KIND='Server'" }
```

### Paso 13.1 — Modelo `EstadisticasViewModel`

```csharp
public class EstadisticasViewModel
{
    public string UsuarioActual { get; set; }
    public int TotalCrystalReports { get; set; }
    public int TotalWebI { get; set; }
    public int TotalReportes { get; set; }
    public DateTime? UltimoEscaneo { get; set; }
    public bool DatosDesdeCache { get; set; }

    public List<SesionSapBo> Sesiones { get; set; } = new List<SesionSapBo>();
    public string SesionesError { get; set; }

    public List<LicenciaSapBo> Licencias { get; set; } = new List<LicenciaSapBo>();
    public string LicenciasError { get; set; }

    public List<ServidorSapBo> Servidores { get; set; } = new List<ServidorSapBo>();
    public string ServidoresError { get; set; }
}
```

Cada colección lleva su propio campo `Error` para poder mostrar por separado qué consulta falló, sin que un fallo aislado tumbe toda la página.

### Paso 13.2 — Helper `HacerPostSeguro` y `ConsultarViaCmsQuery`

`HacerGetSeguro`/`HacerPostSeguro` devuelven `null` cuando el servidor responde 400/403/404/405/501 (endpoint no disponible o sin permiso), permitiendo probar múltiples URLs candidatas sin excepciones ruidosas:

```csharp
private static string HacerPostSeguro(string url, string token, string body, string contentType)
{
    try { return HacerPostAutenticado(url, token, body, contentType); }
    catch (WebException wex)
    {
        int code = 0;
        if (wex.Data.Contains("HttpStatusCode"))
            code = (int)wex.Data["HttpStatusCode"];
        if (code == 404 || code == 400 || code == 403 || code == 405 || code == 501)
        {
            RegistrarInfo("Endpoint no disponible (HTTP " + code + "): " + url);
            return null;
        }
        throw;
    }
}

private static string ConsultarViaCmsQuery(string queryTexto, string token)
{
    string url = ApiUrl + "/v1/cmsquery";
    string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
        + "<attrs xmlns=\"http://www.sap.com/rws/bip\">"
        + "<attr name=\"query\" type=\"string\">" + EscapeXml(queryTexto) + "</attr>"
        + "</attrs>";
    return HacerPostSeguro(url, token, xml, "application/xml");
}
```

**Bug crítico durante la implementación:** en el `catch` de `HacerGetAutenticado`, el bloque `using` disponía de `wex.Response` antes de re-lanzar la excepción. El código que atrapaba luego intentaba leer `wex.Response.StatusCode` sobre un objeto ya disposed y lanzaba `ObjectDisposedException`. Solución: capturar el status code ANTES del `using` y guardarlo en `wex.Data["HttpStatusCode"]` para que el consumidor lo lea sin tocar la respuesta.

### Paso 13.3 — Métodos de consulta

```csharp
public static ResultadoConsulta<SesionSapBo> ConsultarSesiones()
{
    var res = new ResultadoConsulta<SesionSapBo>();
    if (!Habilitado) { res.Error = "Cliente SAP BO no habilitado."; return res; }

    try
    {
        string token = ObtenerToken();
        string body = ConsultarViaCmsQuery(
            "SELECT SI_ID,SI_NAME,SI_USERFULLNAME,SI_AUTHEN_METHOD," +
            "SI_STARTTIME,SI_LOGON_TIME FROM CI_SYSTEMOBJECTS " +
            "WHERE SI_KIND='Connection'",
            token);

        if (body == null)
        {
            res.Error = "El servidor no expone endpoints para listar sesiones.";
            return res;
        }

        var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var root = js.Deserialize<Dictionary<string, object>>(body);
        var entries = ExtraerLista(root, new[] { "entries", "sessions" });
        if (entries == null)
        {
            res.Error = "La respuesta no tiene el formato esperado.";
            return res;
        }

        foreach (Dictionary<string, object> item in entries)
        {
            res.Items.Add(new SesionSapBo
            {
                Id = ObtenerValorStr(item, "SI_ID"),
                Usuario = ObtenerValorStr(item, "SI_USERFULLNAME")
                       ?? ObtenerValorStr(item, "SI_NAME"),
                TipoSesion = ObtenerValorStr(item, "SI_AUTHEN_METHOD"),
                HoraInicio = ObtenerValorStr(item, "SI_LOGON_TIME")
                          ?? ObtenerValorStr(item, "SI_STARTTIME")
            });
        }
    }
    catch (Exception ex)
    {
        res.Error = "No se pudo consultar sesiones: " + ex.Message;
        RegistrarError("ConsultarSesiones", ex);
    }
    return res;
}
```

`ConsultarLicencias` y `ConsultarServidores` siguen el mismo patrón cambiando la sentencia SQL. Cada uno intenta el `cmsquery` y, si falla, cae en fallbacks GET a `/servers`, `/license`, etc. (aunque en 4.x rara vez existen).

### Paso 13.4 — Enlace en el `_Layout.cshtml`

```html
<div class="nav-actions">
    @if (PortalReportesCrystal.Services.SapBoClient.Habilitado)
    {
        <a href="@Url.Action("Estadisticas", "Home")" class="nav-link">Estadísticas SAP BO</a>
    }
    <span class="nav-user">@User.Identity.Name</span>
</div>
```

El enlace aparece solo si el cliente SAP BO está habilitado en `Web.config`. Sin credenciales configuradas, la página no tendría datos para mostrar.

### Paso 13.5 — CSS: tarjetas responsivas con paleta corporativa

```css
.stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 18px; }
.stats-card { background: var(--sr-blanco); border: 1px solid var(--sr-borde); border-radius: 6px; padding: 18px; }
.stats-card-titulo { display: flex; justify-content: space-between; align-items: center;
                     border-bottom: 2px solid var(--sr-primario); color: var(--sr-primario); }
.stats-metrica-grande { font-size: 32px; font-weight: 700; color: var(--sr-primario); text-align: center; }
.stats-progreso-barra { height: 100%; transition: width 0.4s; }
.stats-progreso-verde   { background: #2E7D32; }
.stats-progreso-amarillo { background: #F9A825; }
.stats-progreso-rojo    { background: var(--sr-acento); }
.stats-badge-ok    { background: #E8F5E9; color: #2E7D32; border: 1px solid #A5D6A7; }
.stats-badge-error { background: #FFEBEE; color: #B71C1C; border: 1px solid #EF9A9A; }
.stats-card-wide { grid-column: 1 / -1; }
```

**Barras de progreso semánticas:** el color de la barra se decide en la vista según el porcentaje de licencia usada:

```csharp
string claseBarra = porcentaje >= 90 ? "stats-progreso-rojo"
                    : porcentaje >= 70 ? "stats-progreso-amarillo"
                                       : "stats-progreso-verde";
```

### Paso 13.6 — Manejo graceful de errores en la vista

Cada tarjeta consulta `Model.XError` antes de renderizar la lista:

```html
@if (!string.IsNullOrEmpty(Model.SesionesError))
{
    <div class="stats-mensaje-error">@Model.SesionesError</div>
    <p>El endpoint puede no estar habilitado o el usuario no tiene permisos.</p>
}
else if (!Model.Sesiones.Any())
{
    <div class="stats-tabla-vacia">No hay sesiones activas registradas.</div>
}
else
{
    <table class="stats-tabla">...</table>
}
```

### Paso 13.7 — Botón de refresco

En vez de agregar JavaScript, el botón "Actualizar" es un simple `<a>` que apunta a la misma acción, forzando una nueva consulta al servidor:

```html
<a href="@Url.Action("Estadisticas", "Home")" class="stats-boton-refresh">
    &#10227; Actualizar
</a>
```

Las estadísticas no se cachean; el resumen de reportes sí (usa el cache de 15 min del módulo 12).

### Consideraciones de seguridad

- **`[Authorize]`** heredado del controlador: solo usuarios autenticados por Windows pueden ver la página.
- **Enmascaramiento de claves de licencia**: `SI_KEYCODE` se muestra como `DT40W-****-1C` (primeros 4 + últimos 4 caracteres). Aún así, en producción esta vista debería restringirse a un rol de administración específico.
- **Credenciales del cliente SAP BO** se leen de `Web.config` cifrado con `aspnet_regiis`.
- **Sin cache de estadísticas**: los datos son sensibles al momento; cachearlos daría lecturas incorrectas para el operador.

### Verificación del módulo

- Enlace "Estadísticas SAP BO" aparece en la barra de navegación
- La página muestra:
  - Total de reportes (Crystal + WebI)
  - Sesiones activas con usuario y método de autenticación
  - Licencias con clave enmascarada y fecha de expiración
  - Servidores con estado (Running/Stopped/Disabled) en badges de color
- Los tres endpoints se consultan en <10 segundos con datos frescos
- El botón "Actualizar" recarga los datos sin usar cache
- Si un endpoint falla (403/404), solo esa tarjeta muestra el mensaje y el resto sigue funcionando

### Troubleshooting

**"El servidor no expone endpoints para X"**
Significa que ni el `cmsquery` ni los endpoints tradicionales respondieron. Revisar `App_Data/errores_sapbo.log` para el detalle. Causas comunes:
- El servicio Web Application Container Server no está corriendo
- El puerto 6405 está bloqueado
- La versión del servidor no incluye `/v1/cmsquery` (raro en 4.x SP4+)

**HTTP 400 `RWS 000026`**
El body XML no tiene el atributo `name`. Usar exactamente:
```xml
<attrs xmlns="http://www.sap.com/rws/bip">
  <attr name="query" type="string">SELECT ...</attr>
</attrs>
```

**HTTP 403 en todas las consultas**
El usuario configurado no tiene permisos administrativos. Se necesita un usuario con derechos "View Objects" sobre CMC y sobre los objetos de sistema (Servers, LicenseKey, Connection).

**Licencias devuelve 0 resultados**
La sentencia usa `SI_KIND='License'`. Cambiar a `SI_KIND='LicenseKey'` — el kind correcto en SAP BO 4.x.

**Sesiones devuelve resultados vacíos o SERVER-TOKEN repetido**
Es correcto: las sesiones "SERVER-TOKEN" son sesiones internas de servicios de BO. Filtrarlas en la vista si se desea ocultar el ruido:
```csharp
Model.Sesiones.Where(s => !string.Equals(s.TipoSesion, "SERVER-TOKEN", ...))
```

**ObjectDisposedException al leer `wex.Response`**
Bug clásico: la respuesta se dispuso dentro de un `using` antes de re-lanzar la excepción. Guardar el status code en `wex.Data["HttpStatusCode"]` antes de disponer el response.

**Paginación: sólo veo 50 sesiones**
`cmsquery` pagina por defecto a 50. Para más, agregar `?page=2&pagesize=200` al endpoint, o iterar `next.__deferred.uri` hasta que no esté presente.

---

## Módulo 14. Auditoría integral del portal

### Objetivo

Registrar en base de datos todo evento de usuario (login, apertura del listado, apertura de reporte, exportación, filtros aplicados como Almacén/País/Fechas, descargas dentro del iframe de SAP BO y errores) para poder responder preguntas como:

- ¿Qué reportes se están usando más?
- ¿Quién descargó el reporte X ayer y con qué filtros?
- ¿Cuántas sesiones simultáneas soporta el portal en horas pico?
- ¿Existen accesos denegados o intentos fallidos?

Sin este módulo, el portal solo registra fallos técnicos en `errores.log` — no queda rastro de uso normal ni de comportamiento del negocio.

### Arquitectura

```
Navegador (Windows Auth)
    │
    ▼
IIS + Application Pool (Identity: cuenta de dominio o gMSA)
    │
    ├─► AuditAttribute (filtro global)
    │       → asegura SesionId en HttpContext (registra LOGIN una vez)
    │
    ├─► Controllers hooks explícitos
    │       Home.Index()           → VER_LISTADO
    │       Reportes.Ver()         → VER_REPORTE
    │       Reportes.Preview()     → PREVIEW
    │       Reportes.Exportar()    → EXPORTAR_PDF/EXCEL/EXCELDATA/WORD
    │       Sapbo.Ver()            → VER_REPORTE + parámetros ls*
    │       Auditoria.RegistrarInteraccion() → DESCARGA_IFRAME (POST desde JS)
    │
    ▼
AuditoriaService (cola en memoria)
    │
    ▼
Timer flush cada 5 s
    │
    ├──[BD viva]──► DWH_FRAMEWORK.audit.Evento + EventoParametro
    │                                     + Sesion.UltimaActividadUtc
    │
    └──[BD caída]─► App_Data\audit_pending.jsonl  (reintento próximo ciclo)
```

**Punto crítico:** el portal **nunca** falla por auditoría. Si la BD está caída, los eventos se persisten localmente y se reenvían cuando vuelve.

### Base de datos: esquema `audit`

Todo vive en el servidor **Perseo**, BD **DWH_FRAMEWORK**, esquema **`audit`** (aislado del DWH productivo). Tablas:

| Tabla | Rol |
|---|---|
| `audit.EventoTipo` | Catálogo estable de tipos (LOGIN, VER_REPORTE, EXPORTAR_PDF, etc.) |
| `audit.Sesion` | Una fila por sesión ASP.NET del portal (Usuario, IP, UserAgent, InicioUtc, UltimaActividadUtc, FinUtc) |
| `audit.Evento` | Cada evento con FK a Sesion + TipoEvento + campos desnormalizados para consultas rápidas |
| `audit.EventoParametro` | Parámetros del evento (ALMACEN=06, PAIS=SV, FECHA_DESDE=2026-01-01) |
| `audit.ReporteAgregado` | Materializada por SP diario: aperturas/descargas por reporte y día |
| `audit.UsuarioAgregado` | Materializada por SP diario: uso por usuario y día |

Índices críticos: `IX_Evento_Fecha_Tipo`, `IX_Evento_SesionId`, `IX_Evento_NombreReporte`, `IX_Sesion_Usuario`.

### Instalación (una sola vez)

**Prerequisitos**: respaldo previo de `DWH_FRAMEWORK` y ventana autorizada.

1. Ejecutar en Perseo, contra `DWH_FRAMEWORK`:
   ```
   Database\audit_schema.sql          -- crea esquema, tablas, índices y grants
   Database\audit_agregacion_diaria.sql  -- crea SP de agregación
   Database\audit_purge_job.sql          -- crea SP de retención
   ```
2. Crear los grupos AD (o pedir a IT):
   - `SUPERREPUESTOS\Portal_Audit_Writers` — miembros que escriben (mínimo: la identidad del AppPool).
   - `SUPERREPUESTOS\Portal_Audit_Readers` — miembros que ven el dashboard.
3. Crear los logins Windows en la instancia y ejecutar de nuevo `audit_schema.sql` — el script detecta los logins existentes y aplica GRANTs automáticamente.
4. Programar 2 jobs en SQL Server Agent:
   - **Agregación**: `EXEC audit.sp_AgregarDiario` — diario a las 01:00.
   - **Retención**: `EXEC audit.sp_Retencion @DiasRetencion = 720` — mensual día 1 a las 02:00.

### Autenticación: por qué Windows Auth

Se usa `Integrated Security=SSPI` en el connection string, **sin usuario ni contraseña**. Ventajas:

- Cero secretos en `Web.config` — se elimina una vía de fuga de credenciales.
- Trazabilidad: en Perseo se ve qué cuenta AD realizó la escritura, no una cuenta genérica.
- Rotación automática: si es gMSA, Windows rota la contraseña sin intervención humana.
- Cumple política corporativa de segregación de funciones.

Requisito: el AppPool de IIS debe correr con una cuenta de dominio (idealmente gMSA) que sea miembro de `SUPERREPUESTOS\Portal_Audit_Writers`. En IIS Express de desarrollo, se hereda la cuenta del usuario que arranca el proceso.

### Configuración en `Web.config`

```xml
<add key="Audit:Habilitado" value="true" />
<add key="Audit:ConnectionString"
     value="Server=Perseo;Database=DWH_FRAMEWORK;Integrated Security=SSPI;Application Name=PortalReportesCrystal;Connection Timeout=15;" />
<add key="Audit:IntervaloFlushSeg" value="5" />
<add key="Audit:MaxLoteInsert" value="500" />
<add key="Audit:GruposAdmin" value="SUPERREPUESTOS\Portal_Audit_Readers" />
```

Mientras `Audit:Habilitado="false"`, el servicio no toca la BD ni escribe en `audit_pending.jsonl`. Ideal para desarrollo local o mientras el DDL aún no se ha ejecutado en Perseo.

### Reportes SAP BO ahora corren dentro del portal (iframe)

Antes: los reportes WebI del listado abrían el CMC en pestaña nueva → cero trazabilidad de qué hacía el usuario.

Ahora: al hacer clic en "Abrir en portal →" navegación va a `/Sapbo/Ver?nombre=...&url=...`, que:

1. Registra un `VER_REPORTE` con `CUID`, categoría, servidor de origen (`SAP BO WebI` o `SAP BO .rpt`) y todos los parámetros `lsSALMACEN`, `lsSPAIS`, `lsSFECHA...` normalizados en `audit.EventoParametro`.
2. Renderiza `Views/Sapbo/Ver.cshtml` con un `<iframe>` embebiendo la URL de OpenDocument.
3. Un script del cliente detecta si el iframe cargó o quedó en blanco (X-Frame-Options). Si quedó en blanco, muestra un mensaje amigable con enlace a "abrir externo" y registra un evento de diagnóstico.
4. Antes de que el navegador siga a una URL de descarga interna del visor (patrón `sOutputFormat=P`/`E`/`X`), un `beforeunload` emite POST a `/Auditoria/RegistrarInteraccion` con tipo `DESCARGA_IFRAME`.

Si el servidor SAP BO XI 3.1 rechaza el embed en producción, el `SapboController.Contenido` se puede evolucionar a modo **proxy** (el portal descarga el contenido usando el token REST y lo re-sirve desde su propio origen). El endpoint ya está preparado para ese caso; hoy funciona en modo A (redirect).

### Página de diagnóstico

`Auditoria/Dashboard` — solo visible en el header para usuarios en `Audit:GruposAdmin`. Muestra:

- Últimas 24 h: sesiones únicas, eventos totales, exportaciones, errores.
- Últimos 30 días: top 10 reportes, top 10 usuarios.
- Distribución por hora.
- Acceso al dashboard también queda auditado (`ACCESO_DASHBOARD`).

`Sapbo/TestIframe` — herramienta admin. Carga una URL de prueba en iframe y reporta si el servidor SAP BO permite embed directo o requiere proxy.

### Tres niveles de auditoría (Portal SIG, 20-ago-2026)

La reunión del Portal SIG definió tres niveles de auditoría requeridos:

| Nivel | Descripción | Estado |
|-------|-------------|--------|
| **1. Auditoría de Permisos** | Registro de usuarios autorizados, asignaciones de reportes y roles | Nuevo desarrollo (ver abajo) |
| **2. Auditoría de Accesos** | Usuario que accedió, fecha/hora, reporte consultado | Implementado (tablas `audit.Sesion` + `audit.Evento`) |
| **3. Auditoría de Filtros** | Parámetros utilizados y filtros aplicados por el usuario | Implementado (tabla `audit.EventoParametro`) |

**Nivel 1 — Auditoría de Permisos** requiere 4 tablas adicionales en el esquema `audit`:

- `audit.Rol` — roles lógicos del portal, opcionalmente vinculados a grupos AD.
- `audit.RolReporte` — asignación rol→reporte por raíz, categoría o reporte individual.
- `audit.UsuarioRol` — asignación usuario→rol (con soft-delete para trazabilidad).
- `audit.PermisoLog` — historial de todos los cambios de permisos (quién, cuándo, qué).

DDL: `Database/audit_permisos_schema.sql`. Se activa con `Permisos:Habilitado=true` en `Web.config`. Mientras está en `false`, el portal funciona en modo abierto (cualquier usuario autenticado ve todos los reportes).

**Niveles 2 y 3** se activan ejecutando `Database/audit_schema.sql` en Perseo y cambiando `Audit:Habilitado=true` en `Web.config`. No requieren código adicional.

### Control de acceso a los módulos administrativos

Los módulos administrativos del portal (`/Auditoria/Dashboard`, `/Auditoria/Interacciones`, `/Permisos`) están protegidos por una lista de administradores configurable en tres capas evaluadas en orden:

| Clave `Web.config` | Propósito | Valor recomendado |
|---|---|---|
| `Audit:BypassAdminEnDev` | Solo desarrollo. Si es `true` **cualquier** usuario autenticado ve todos los módulos administrativos. Debe volverse a `false` antes de producción. | `false` |
| `Audit:UsuariosAdmin` | Lista de usuarios AD individuales (separados por `;`). Complementa a `Audit:GruposAdmin` para casos donde aún no exista el grupo formal. | `SUPERREPUESTOS\usuario1;SUPERREPUESTOS\usuario2` |
| `Audit:GruposAdmin` | Lista de grupos AD (separados por `;`). Preferido en producción para no depender de cuentas individuales. | `SUPERREPUESTOS\Portal_Audit_Readers` |

Un usuario tiene acceso si cumple **cualquiera** de las tres condiciones. Los helpers `EsAdminAuditoria()` (en `AuditoriaController`) y `EsAdmin()` (en `PermisosController`) implementan la evaluación en cascada, y `Views/Shared/_Layout.cshtml` usa la misma lógica para mostrar u ocultar los enlaces del menú superior.

### Interacciones en vivo (timeline)

Página `/Auditoria/Interacciones` — enlace **"En vivo"** en la barra superior del portal. Muestra en tiempo real (auto-refresh cada 3 segundos) la lista de últimos eventos con:

- **Hora local**, tipo de evento (color por categoría), usuario e IP, nombre del reporte, filtros aplicados como chips (`ALMACEN=06`, `PAIS=SV`, `FECHA=20260827`), duración, HTTP status y errores.
- **Filtros**: origen (Todos / SAP BO / Local), CUID de reporte específico, botón para pausar el auto-refresh.
- **Nuevos eventos** se destacan con fondo amarillo por 2.5 s.

Consume el endpoint `GET /Auditoria/InteraccionesSapbo?desdeEventoId=N&raiz=...&cuid=...&limite=...` que devuelve JSON con los eventos y sus parámetros asociados. Protegido con el mismo helper que el dashboard.

### Auditoría de reportes SAP BO — modelo actual

El servidor SAP BO 4.x devuelve la cabecera `X-Frame-Options: DENY`, por lo que **el visor no puede embeberse en `<iframe>` desde el portal**. Se optó por no forzar la restricción (mantiene el aislamiento de sesiones del CMC intacto). La vista `Sapbo/Ver` implementa el patrón "**abrir en pestaña nueva + timeline de auditoría embebida en el portal**":

- **Columna izquierda**: botón "Abrir en SAP BO" (dispara `Sapbo/AbrirExterno` que registra `DESCARGA_IFRAME` con `APERTURA_EXTERNA` en `MensajeError` y redirige al CMC). Chip con los filtros que se están enviando al reporte (parseados desde la URL `ls*`).
- **Columna derecha**: timeline mini con las últimas 20 interacciones del reporte actual (filtrada por CUID). Se refresca sola cada 3 segundos.

**Lo que sí se captura** (a nivel del portal):
- ✅ Apertura del reporte y timestamp
- ✅ Todos los parámetros `ls*` de la URL OpenDocument (Almacén, País, Fechas, Producto, etc.)
- ✅ Clic para abrir en pestaña nueva
- ✅ Usuario, IP, servidor SAP BO destino

**Lo que no se captura** (por diseño — sucede dentro del visor CMC, cross-origin):
- ❌ Cambios de filtros que el usuario haga **dentro** del visor SAP BO
- ❌ Descargas de PDF/Excel disparadas desde el visor SAP BO

Capturar los eventos internos del visor requeriría intervenir el CMC (agregar hooks JavaScript en InfoView/BILaunchPad) — desarrollo separado, invasivo, con impacto en soporte del vendor. No está en el alcance actual.

### Endpoints del módulo de auditoría

| Ruta | Método | Protección | Descripción |
|---|---|---|---|
| `/Auditoria/Dashboard` | GET | Admin | Vista con 6 tarjetas de métricas |
| `/Auditoria/DashboardData` | GET | Admin | JSON con métricas del dashboard |
| `/Auditoria/Interacciones` | GET | Admin | Vista timeline en vivo |
| `/Auditoria/InteraccionesSapbo` | GET | Admin | JSON con últimos eventos filtrables |
| `/Auditoria/RegistrarInteraccion` | POST | Authenticated | AJAX del cliente (interacciones desde iframe) |
| `/Auditoria/RegistrarDiagnosticoIframe` | POST | Authenticated | AJAX del cliente (test de embed) |
| `/Sapbo/Ver` | GET | Authenticated | Wrapper con timeline embebida + botón externo |
| `/Sapbo/AbrirExterno` | GET | Authenticated | Registra `APERTURA_EXTERNA` y redirige a CMC |
| `/Sapbo/TestIframe` | GET | Authenticated | Diagnóstico admin de X-Frame-Options |

### Configuración local de credenciales

Las credenciales de SAP BusinessObjects (`SapBo:Usuario`, `SapBo:Password`) **NO se committean al repositorio**. En `Web.config` aparecen como placeholders `***CONFIGURAR_LOCAL***`. Cada desarrollador o servidor debe reemplazarlos localmente antes de compilar.

**Procedimiento**:

1. Solicitar las credenciales al administrador de SAP BO (usuario dedicado con permisos mínimos de lectura sobre el CMC).
2. Editar el `Web.config` local reemplazando los placeholders:
   ```xml
   <add key="SapBo:Usuario" value="<usuario_real>" />
   <add key="SapBo:Password" value="<password_real>" />
   ```
3. **No guardar** ese `Web.config` con las credenciales reales en Git. Antes de un commit, revertir a placeholders o usar `git update-index --skip-worktree Web.config` en la copia local (que ignora sus cambios sin afectar el archivo del repo).

**Para producción**: cifrar la sección `appSettings` con DPAPI del propio servidor:
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe" -pe "appSettings" -app "/PortalReportesCrystal"
```
La descifración es transparente para ASP.NET cuando corre bajo la misma identidad del AppPool que se usó para cifrar. Documentar en el runbook de despliegue qué cuenta cifró el archivo.

### Codificación de caracteres

`Web.config` incluye `<globalization requestEncoding="utf-8" responseEncoding="utf-8" fileEncoding="utf-8" culture="es-SV" uiCulture="es-SV" />` en `system.web` para evitar el mojibake tipo "AuditorÃa". Las vistas críticas (`Dashboard.cshtml`, `Interacciones.cshtml`, `Sapbo/Ver.cshtml`, `Permisos/*.cshtml`) se guardan con BOM UTF-8.

### Seguridad y cumplimiento

- Solo metadatos: nunca se almacena contenido del reporte ni credenciales de usuario.
- Timestamps en UTC + columnas calculadas a hora local (GMT-6).
- Retención por política: 720 días (24 meses) para `audit.Evento` y `audit.Sesion`. Agregados quedan indefinidamente.
- Fallback local si BD cae: `App_Data\audit_pending.jsonl` (git-ignorado, restringido por NTFS a la identidad del AppPool).
- Errores del propio servicio: `App_Data\auditoria.log`.
- Antes de habilitar en producción: respaldo previo, plan de reversa (`DROP SCHEMA audit`), monitoreo del impacto en la latencia del portal (esperado: < 5 ms adicionales por request, medido en flush asíncrono).

### Verificación end-to-end

1. `SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID('audit')` en `DWH_FRAMEWORK` — deben existir 6 tablas.
2. Compilar portal, habilitar `Audit:Habilitado=true`, refrescar el listado. Ejecutar:
   ```sql
   SELECT TOP 5 * FROM audit.Sesion ORDER BY InicioUtc DESC;
   SELECT TOP 5 e.*, t.Codigo
   FROM audit.Evento e JOIN audit.EventoTipo t ON t.TipoEventoId = e.TipoEventoId
   ORDER BY EventoId DESC;
   ```
3. Abrir un reporte SAP BO con filtros: verificar filas en `audit.EventoParametro`.
4. Como usuario **no** admin: `/Auditoria/Dashboard` debe devolver 403. Como admin: debe cargar la vista.
5. Detener el servicio SQL Server (o desconectar): el portal debe seguir funcionando; los eventos se acumulan en `App_Data\audit_pending.jsonl`. Al restaurar la BD, el próximo flush debe vaciar el archivo y no dejar `.processing`.

---

## Anexo A. Ejecución sin depender de F5

Si Visual Studio no puede lanzar el proyecto con F5, se puede correr IIS Express desde PowerShell. Esto requiere un `applicationhost.config` propio con las secciones de autenticación Windows desbloqueadas.

Script `Ejecutar-Portal.ps1`:

```powershell
$proyecto = $PSScriptRoot
$config   = Join-Path $proyecto 'applicationhost.iisexpress.config'
$csproj   = Join-Path $proyecto 'PortalReportesCrystal.csproj'
$url      = 'http://localhost:58172/'

$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$iis     = "C:\Program Files\IIS Express\iisexpress.exe"

& $msbuild $csproj /t:Build /p:Configuration=Debug /v:minimal /nologo
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Job -ScriptBlock { Start-Sleep 3; Start-Process $using:url } | Out-Null
& $iis "/config:$config" '/site:PortalReportesCrystal'
```

Para debuggear con puntos de interrupción sin F5: en Visual Studio → **Depurar → Adjuntar al proceso → iisexpress.exe**.

---

## Anexo B. Guía de despliegue a producción

### Lista de verificación previa

- [ ] `<compilation debug="false">` en `Web.config`
- [ ] Sección `system.webServer/security/authentication` **descomentada** en `Web.config`
- [ ] Credenciales de BD cifradas: `aspnet_regiis -pe "connectionStrings" -app "/PortalReportes"`
- [ ] Runtime de Crystal Reports 13.0.x instalado en el servidor (x64)
- [ ] Cuenta de aplicación con permisos NTFS de lectura en las raíces de `.rpt`
- [ ] Reciclado del pool configurado (por memoria o por tiempo)
- [ ] Log de errores rotable (por fecha o tamaño)

### Configuración de IIS

- **Application Pool** en `.NET CLR v4.0.30319`, modo integrado, 64-bit
- **Autenticación:** Windows habilitada, anónima deshabilitada
- **HTTPS obligatorio** si se accede fuera de la LAN

### Consideraciones operativas

- El proceso `w3wp.exe` acumula memoria por la naturaleza nativa del SDK. Programar reciclado cada 8–12 horas fuera de horario pico.
- Los `.rpt` en las raíces configurables no deben modificarse mientras el portal está corriendo (Crystal puede tener handles abiertos).
- Al desplegar una nueva versión, hacer un reciclado explícito para invalidar el `Application_Start` y recargar el cache de parámetros.

---

## Anexo C. Troubleshooting común

### El portal no arranca con F5 en Visual Studio

**Síntoma:** VS muestra "No se puede iniciar la depuración. El proyecto de inicio no se puede iniciar" y no hace nada.

**Diagnóstico:** revisar el ActivityLog de Visual Studio (`%APPDATA%\Microsoft\VisualStudio\17.0_XXX\ActivityLog.xml`). Un error `Unable to load DLL 'PkgDefMgmt.dll'` indica que los registros de paquetes de VS están dañados.

**Solución:** ejecutar `devenv /updateconfiguration` como administrador. Si persiste, "Reparar" desde el Visual Studio Installer.

**Bypass:** ver Anexo A.

### Error 500.19 al arrancar

**Síntoma:** "Cannot read configuration file due to insufficient permissions" o "0x80070021".

**Causa:** la sección `system.webServer/security/authentication` está bloqueada en IIS Express desde línea de comandos.

**Solución:** comentar esa sección en `Web.config` para desarrollo local. Solo descomentarla al desplegar en IIS completo.

### "No se pudo conectar con la base de datos"

**Causa habitual:** el `.rpt` no trae datos guardados y necesita conectarse a una BD que no es alcanzable desde la estación de desarrollo (o cuyas credenciales no están configuradas).

**Solución para desarrollo:** abrir el `.rpt` en Crystal Reports Designer, verificar `File → Options → Reporting → Save Data with Report`, refrescar los datos, guardar. Al mover ese archivo al portal, se ejecutará con los datos incrustados.

**Solución para producción:** aplicar `ConnectionInfo` con las credenciales antes de `ExportToStream()`.

### Directory traversal

**Síntoma:** un intento de acceder a `?path=..\..\Windows\win.ini` devuelve algo distinto de 404.

**Causa:** la validación `combinado.StartsWith(baseCanon)` no se aplicó o se comparó con separadores incompatibles.

**Solución:** asegurar que `baseCanon` termina con `Path.DirectorySeparatorChar` y que la comparación es `OrdinalIgnoreCase`. Ver Módulo 5, paso 5.4.

### El PDF sale cortado horizontalmente

**Causa:** el `.rpt` está diseñado con un ancho mayor a la página estándar y el visor lo escala. El portal genera el PDF con el tamaño original del reporte.

**Solución:** en `Ver.cshtml`, agregar `body.pagina-ver main.container { max-width: 100%; }` para que el `iframe` use todo el ancho del navegador. Si aún se corta, el usuario debe hacer zoom-out en el visor de PDF o descargar el archivo.

### El escaneo de parámetros nunca termina

**Causa:** un `.rpt` está corrupto o requiere credenciales para simplemente cargar sus definiciones.

**Diagnóstico:** revisar `App_Data/errores.log` y buscar entradas con `No se pudo analizar`.

**Solución:** identificar el archivo problemático y removerlo temporalmente del escaneo, o excluirlo agregándolo a una lista de skip en `CacheParametros.IniciarEscaneoBackground()`.

---

## Módulo 15. Parametrización dinámica estándar para Crystal Reports

### Problema

El portal maneja ~316 reportes (SV/HN/GT) y el inventario legacy contiene 1,555 `.rpt`. Sin un estándar, cada desarrollador inventa su propia convención de parámetros, listas de valores y lógica WHERE. Esto genera:
- Nombres inconsistentes (`{?Param1}`, `{?P}`, `{?a}`).
- LOV duplicadas o embebidas en fórmulas Crystal (no reutilizables).
- Anti-patrones como `IIF({?Param} = "TODOS", ...)` que no funcionan con multi-valor.
- Imposibilidad de agregar un país o almacén nuevo sin tocar múltiples `.rpt`.

### Solución: framework de parametrización compartido

Se definió un estándar documentado en `docs/PATRON_PARAMETROS_DINAMICOS.md` que cubre:

1. **Convención de nomenclatura** — `{?ID_<Entidad>}` para selección única, `{?<Entidad>}` para multi-selección, `{?Fecha_Desde}`/`{?Fecha_Hasta}` para rangos.
2. **Contrato de LOV** — todo Command retorna exactamente `CODIGO VARCHAR(50)` + `DESCRIPCION VARCHAR(200)`.
3. **Patrón "TODOS"** — los parámetros multi-selección son tipo String; la fila `'TODOS'` va primera en el LOV.
4. **Patrón WHERE unificado** — `('TODOS' IN ({?Param}) OR CAST(campo AS VARCHAR(50)) IN ({?Param}))`.
5. **Cascada** — Command dependiente con `{?ID_PAIS}` en el WHERE del LOV hijo + índice cubriente.

### Archivos del estándar

| Archivo | Propósito |
|---------|----------|
| `docs/PATRON_PARAMETROS_DINAMICOS.md` | Guía maestra con las 10 secciones del estándar |
| `Database/LOV/README.md` | Contrato de retorno + guía de contribución |
| `Database/LOV/LOV_Paises.sql` | Lista de países (selección única) |
| `Database/LOV/LOV_Monedas.sql` | Lista de monedas (selección única) |
| `Database/LOV/LOV_Centro_Costo.sql` | Centros de costo con TODOS |
| `Database/LOV/LOV_Vendedor.sql` | Vendedores con TODOS |
| `Database/LOV/LOV_Almacen_Por_Pais.sql` | Almacenes en cascada por país, con TODOS |
| `Database/LOV/LOV_Producto_Por_Almacen.sql` | Productos en cascada por almacén, con TODOS |

### Cómo aplicar el estándar a un reporte nuevo

1. Identificar los filtros del reporte y clasificarlos (tipo 1, 2 o 3).
2. Nombrar cada parámetro Crystal según la convención de la sección 2 del estándar.
3. Asignar un LOV Command existente o crear uno nuevo siguiendo el contrato.
4. Aplicar el patrón WHERE unificado en el Command del reporte.
5. Crear índices cubrientes en las tablas de dimensión si hay cascada.
6. Verificar con la checklist de la sección 9 del estándar.

### Caso demostrativo: Kardex de inventario

| Parámetro | Tipo | LOV |
|-----------|------|-----|
| `{?ID_PAIS}` | Única obligatoria | `LOV_Paises` |
| `{?Almacen}` | Multi con TODOS | `LOV_Almacen_Por_Pais` |
| `{?Codigo_Producto}` | Multi con TODOS | `LOV_Producto_Por_Almacen` |
| `{?Fecha_Desde}` / `{?Fecha_Hasta}` | Rango | Calendario nativo |

### Seguridad

- Los LOV nunca reciben input directo — solo valores del selector Crystal.
- `{?ID_PAIS}` como Number rechaza strings (inyección bloqueada por tipo).
- Conexión al DW con usuario de servicio con `SELECT` solo en dimensiones.
- Cambios al catálogo de LOV siguen el mismo flujo de control de cambios que un reporte productivo.

---

## Cierre

Este manual cubre la construcción del portal desde el proyecto vacío hasta el despliegue en producción, incluyendo la integración con la API REST de SAP BO para descubrimiento automático de reportes. Cada módulo es independiente y se puede repetir sobre otro proyecto que necesite integrar Crystal Reports en ASP.NET MVC.

**Recomendación de aprendizaje:** implementar los módulos 1 a 5 primero (portal mínimo funcional), tenerlo corriendo, y solo entonces avanzar con los módulos 6+ conforme el proyecto lo requiera. Los módulos 12 (descubrimiento de reportes) y 13 (estadísticas del CMS) son los más avanzados y requieren acceso a un servidor SAP BO funcional con permisos administrativos. Sobreingeniería temprana es el error más común en proyectos de este tipo.

**Referencias oficiales:**
- [SAP Crystal Reports for Visual Studio](https://help.sap.com/docs/SAP_CRYSTAL_REPORTS_FOR_VISUAL_STUDIO)
- [ASP.NET MVC 5 documentation](https://learn.microsoft.com/aspnet/mvc/mvc5)
- [PDFsharp](http://www.pdfsharp.net/)

**Contacto:** para dudas sobre esta implementación específica, revisar el repositorio del proyecto o contactar al área de BI/DWH.
