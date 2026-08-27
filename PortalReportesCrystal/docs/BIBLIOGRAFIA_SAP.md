# Bibliografía oficial — Tecnologías SAP utilizadas en el Portal de Reportes Crystal

**Propósito**: referencia consolidada de la documentación oficial de SAP para las tecnologías, SDKs y APIs empleadas en el portal. Dirigida al equipo revisor de código y arquitectura para validar buenas prácticas y compatibilidad de versiones.

**Última revisión**: 2026-08-27
**Ambiente objetivo**: SAP BusinessObjects BI Platform 4.x (algunos entornos legacy sobre XI 3.1) + Crystal Reports for Visual Studio 13.0.4000.0

---

## Índice

1. [Crystal Reports for Visual Studio (SDK .NET)](#1-crystal-reports-for-visual-studio-sdk-net)
2. [SAP BusinessObjects RESTful Web Services (biprws)](#2-sap-businessobjects-restful-web-services-biprws)
3. [OpenDocument URL Reporting](#3-opendocument-url-reporting)
4. [Web Intelligence Raylight REST SDK](#4-web-intelligence-raylight-rest-sdk)
5. [Autenticación (secEnterprise, LDAP, Windows AD)](#5-autenticación-secenterprise-ldap-windows-ad)
6. [Ensamblados y versiones utilizadas](#6-ensamblados-y-versiones-utilizadas)
7. [Fuentes de soporte comunitario](#7-fuentes-de-soporte-comunitario)

---

## 1. Crystal Reports for Visual Studio (SDK .NET)

**Uso en el portal**: renderizado de reportes `.rpt` locales, exportación a PDF/Excel/Excel Data-only, gestión de parámetros dinámicos y conexiones DB.

**Versión implementada**: `13.0.4000.0` (compatible con Visual Studio 2019/2022, .NET Framework 4.8).

### Documentación oficial

| Recurso | Ubicación |
|---|---|
| Portal oficial del producto | `https://www.sap.com/products/technology-platform/crystal-reports-visual-studio.html` |
| SAP Help Portal — Crystal Reports for Visual Studio | `https://help.sap.com/docs/SAP_CRYSTAL_REPORTS_DEVELOPER_VS` |
| Developer Guide | Buscar en Help Portal: *"Crystal Reports for Visual Studio Developer Guide"* |
| API Reference (.NET) | Namespace `CrystalDecisions.CrystalReports.Engine`, `CrystalDecisions.Shared`, `CrystalDecisions.Web` |
| SAP Note maestra de compatibilidad | SAP Note 1970519 — *"Crystal Reports, developer version for Microsoft Visual Studio — Downloads"* |
| Página de descargas oficial | `https://origin.softwaredownloads.sap.com` (requiere cuenta S-user) |

### Namespaces principales utilizados

```csharp
using CrystalDecisions.CrystalReports.Engine;   // ReportDocument, ParameterField
using CrystalDecisions.Shared;                  // ExportOptions, ExportFormatType
using CrystalDecisions.Web;                     // CrystalReportViewer (WebForms; no usado en MVC)
using CrystalDecisions.ReportSource;
```

### Buenas prácticas recomendadas por SAP

- **Liberar recursos**: `ReportDocument.Close()` + `Dispose()` en `finally` — evita memory leaks del runtime nativo.
- **Sesión por request**: no reutilizar instancias de `ReportDocument` entre threads (no es thread-safe).
- **Runtime instalado**: verificar que el servidor tiene el redistribuible correspondiente (`CRforVS_redist_install_64bit_13_0_x.zip`).
- **Timeouts de DB**: configurar `ReportDocument.Database.Tables[].LogOnInfo.ConnectionInfo.LogonProperties["Connection Timeout"]`.

### Guías clave a solicitar al revisor

- SAP Note 1970519 (compatibilidad de versiones)
- SAP Note 1218560 (redistribuible)
- KBA 1218562 — *"How to deploy Crystal Reports for Visual Studio (CR4VS) runtime files"*

---

## 2. SAP BusinessObjects RESTful Web Services (biprws)

**Uso en el portal**: consulta del CMS (inventario de reportes, sesiones activas, licencias, servidores), autenticación Enterprise, generación de tokens `X-SAP-LogonToken`.

**Endpoint base**: `http://<servidor>:6405/biprws/`

### Documentación oficial

| Recurso | Ubicación |
|---|---|
| SAP Help Portal — BI Platform RESTful Web Services | `https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM` |
| Developer Guide (RESTful) | Buscar en Help Portal: *"Business Intelligence Platform RESTful Web Service Developer Guide"* — versión específica por SP |
| SAP Business Accelerator Hub | `https://api.sap.com/` (algunos endpoints públicos catalogados) |

### Endpoints usados en el portal

| Endpoint | Método | Uso |
|---|---|---|
| `/biprws/logon/long` | POST | Autenticación Enterprise → devuelve `logonToken` |
| `/biprws/infostore/<id>` | GET | Metadatos de un objeto del CMS |
| `/biprws/infostore/folder` | GET | Navegación de carpetas |
| `/biprws/v1/users` | GET | Listado de usuarios / detalles |
| `/biprws/v1/cmsquery` | POST | Query CMS SQL-like |
| `/biprws/serversmonitoring/servermetrics` | GET | Métricas de servidores |
| `/biprws/session` | GET/DELETE | Estado de sesión / logout |

### Buenas prácticas

- **Header obligatorio**: `X-SAP-LogonToken: "<token>"` en cada request tras autenticación.
- **Content negotiation**: preferir `Accept: application/xml` para respuestas parseables con `XDocument`. JSON está soportado a partir de BI 4.2 SP4.
- **Timeouts razonables**: 15-30 s por request; caché en portal para inventarios que no cambian minuto a minuto.
- **CSRF**: para operaciones de escritura, extraer `X-CSRF-Token` con `X-CSRF-Token: fetch` en un GET previo.

### SAP Notes recomendadas

- SAP Note 2500067 — *"BI Platform RESTful Web Services — troubleshooting overview"*
- KBA 2338440 — *"How to authenticate with the RESTful Web Services"*
- KBA 2178141 — *"Session management in BI RESTful Web Services"*

---

## 3. OpenDocument URL Reporting

**Uso en el portal**: construcción de URLs que abren reportes WebI/Crystal desde el visor de SAP BO (BILaunchPad / InfoView), pasando parámetros (`lsS`, `lsN`, `lsD`) y opciones de formato.

**Endpoint base**: `http://<servidor>:8080/BOE/OpenDocument/opendoc/custom.jsp` (BI 4.x)
**Legacy XI 3.1**: `http://<servidor>:8080/OpenDocument/opendoc/openDocument.jsp`

### Documentación oficial

| Recurso | Ubicación |
|---|---|
| SAP Help Portal — Viewing Reports and Documents Using OpenDocument | `https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM` — buscar *"Viewing Reports Using OpenDocument"* |
| Guía versionada | *"BI 4.x Viewing Documents Using OpenDocument Guide"* (PDF por Service Pack) |

### Parámetros clave utilizados en el portal

| Parámetro | Descripción | Ejemplo |
|---|---|---|
| `iDocID` | Identificador único del documento | `iDocID=AXeQxxx` |
| `sIDType` | Tipo del ID (`CUID`, `InfoObjectID`, `RepositoryPath`) | `sIDType=CUID` |
| `sWindow` | `Same` (embed) / `New` (pestaña nueva) | `sWindow=Same` |
| `sType` | Tipo de documento: `wid`, `rpt`, `pdf` | `sType=wid` |
| `sOutputFormat` | `H` (HTML viewer), `P` (PDF), `E` (Excel), `X` (Excel data-only), `W` (Word) | `sOutputFormat=P` |
| `sRefresh` | `Y` fuerza refresh contra fuente de datos | `sRefresh=Y` |
| `lsS<Param>=<valor>` | Parámetro tipo String | `lsSALMACEN=06` |
| `lsN<Param>=<valor>` | Parámetro tipo Number | `lsNID_PAIS=1` |
| `lsD<Param>=<valor>` | Parámetro tipo Date (formato `AAAAMMDD`) | `lsDFECHA=20260827` |
| `lsM<Param>=<v1>;<v2>` | Parámetro Multi-valor | `lsMPRODUCTOS=A;B;C` |
| `lsR<Param>=<min>..<max>` | Rango | `lsRPRECIO=100..500` |

### Consideraciones de seguridad y embed

- **`X-Frame-Options: DENY`** es la política por defecto de SAP BO 4.x. Embeber el visor en `<iframe>` **requiere excepción explícita del administrador del CMS** — no es un fallo del portal.
- Autenticación transitiva vía cookies `MysapSSO2` o `X-SAP-LogonToken` cuando se usa el mismo dominio.
- Los `ls*` van en la URL — **no incluir información sensible** (contraseñas, PII cruda). Los tokens de autenticación de BO tampoco deben pasar por query string.

---

## 4. Web Intelligence Raylight REST SDK

**Uso en el portal**: descubrimiento de documentos WebI (`.wid`) publicados en el CMS.

**Endpoint base**: `http://<servidor>:6405/biprws/raylight/v1/`

### Documentación oficial

| Recurso | Ubicación |
|---|---|
| SAP Help Portal — Web Intelligence RESTful Web Service | `https://help.sap.com/docs/SAP_BUSINESSOBJECTS_WEB_INTELLIGENCE` |
| Guía específica | *"Web Intelligence RESTful Web Services SDK Developer Guide"* |

### Endpoints usados en el portal

| Endpoint | Uso |
|---|---|
| `/raylight/v1/documents` | Listado de documentos WebI |
| `/raylight/v1/documents/<id>` | Metadatos de un documento |
| `/raylight/v1/documents/<id>/reports` | Reportes/pestañas del documento |
| `/raylight/v1/documents/<id>/parameters` | Prompts del documento |

### Consideraciones

- **Introducido en BI 4.0 SP4**. En XI 3.1 este SDK no existe — usar OpenDocument + inventario CMC HTML.
- Los payloads XML son verbosos: preferir paginación (`?limit=50&offset=0`).

---

## 5. Autenticación (secEnterprise, LDAP, Windows AD)

**Uso en el portal**: autenticación del portal a SAP BO como sistema, y propagación transparente de la identidad del usuario final vía OpenDocument.

### Tipos de autenticación soportados

| Tipo | `authType` | Uso en el portal |
|---|---|---|
| Enterprise | `secEnterprise` | Autenticación del sistema al CMS (cuenta de servicio) |
| Windows AD | `secWinAD` | Propagación SSO del usuario final |
| LDAP | `secLDAP` | Alternativa a AD |
| SAP | `secSAPR3` | No aplica |

### Documentación oficial

| Recurso | Ubicación |
|---|---|
| SAP Help Portal — Business Intelligence Platform Administrator Guide | Buscar *"Managing Authentication"* dentro del Admin Guide de la versión instalada |
| SAP Note 1631734 | *"How to configure Windows AD authentication in BI Platform"* |
| SAP Note 1780469 | *"Configuring Kerberos SSO for BI Platform"* |

### Buenas prácticas de seguridad (aplicables a este portal)

- **Cuenta de servicio dedicada** (`svc_portal_reader`) con permisos mínimos de View sobre las carpetas del CMS. **Nunca usar `Administrator`** en producción.
- **Cifrar el `Web.config`** con `aspnet_regiis -pe "appSettings"` — evita credenciales en texto plano.
- **Rotar contraseñas** de cuentas de servicio con la periodicidad definida por la política corporativa.
- **Auditoría de sesiones**: monitorear `/biprws/session` para detectar sesiones huérfanas.

---

## 6. Ensamblados y versiones utilizadas

| Ensamblado | Versión | Origen |
|---|---|---|
| `CrystalDecisions.CrystalReports.Engine` | 13.0.4000.0 | SAP Crystal Reports runtime for VS |
| `CrystalDecisions.Shared` | 13.0.4000.0 | SAP Crystal Reports runtime for VS |
| `CrystalDecisions.ReportSource` | 13.0.4000.0 | SAP Crystal Reports runtime for VS |
| `CrystalDecisions.Web` | 13.0.4000.0 | SAP Crystal Reports runtime for VS |
| `System.Net.Http` (para REST biprws) | 4.0.0.0 (framework) | Base .NET Framework 4.8 |
| `System.Web.Mvc` | 5.2.9.0 | NuGet — Microsoft.AspNet.Mvc |

### Requisitos de plataforma

- **.NET Framework 4.8** (target definido en `PortalReportesCrystal.csproj`)
- **IIS 10.0 o superior** con Windows Authentication habilitada
- **Crystal Reports Runtime for .NET Framework**: `CRforVS_redist_install_64bit_13_0_35.zip` o superior — descarga desde SAP con S-user
- **Visual Studio 2019/2022** para desarrollo

---

## 7. Fuentes de soporte comunitario

Estas fuentes NO son documentación oficial pero son referenciadas por SAP como recursos legítimos de la comunidad:

| Recurso | Uso |
|---|---|
| SAP Community (`community.sap.com`) | Foros técnicos, blogs de SAP Mentors |
| SAP Business Accelerator Hub (`api.sap.com`) | Catálogo público de APIs SAP |
| SAP Learning Hub | Cursos oficiales certificados (requiere suscripción) |
| SAP User Groups (ASUG, DSAG) | Grupos regionales de usuarios |

### Cómo buscar SAP Notes / KBAs

Las SAP Notes y KBAs (Knowledge Base Articles) son la fuente autoritativa para issues técnicos. Se acceden desde:

- `https://launchpad.support.sap.com/#/notes` (requiere S-user con contrato de soporte activo)
- Cada Note tiene un identificador numérico (ej. *"SAP Note 1970519"*). Búsqueda por palabra clave o ID.

---

## Notas para el revisor de código

1. **Versión de Crystal usada (13.0.4000.0)** es la línea CR4VS. Es SAP Crystal Reports **for Visual Studio**, NO SAP Crystal Reports 2020/2016 (línea Designer standalone). Son productos separados con licenciamiento distinto.
2. **La API REST de SAP BO (`biprws`) requiere que el servicio esté habilitado** en el CMC → **Aplicaciones → RESTful Web Service**. Si no responde en `:6405`, revisar Tomcat y el estado del `WebIntelligenceProcessingServer`.
3. **`X-Frame-Options: DENY`** es política obligatoria de SAP BO 4.x. Cualquier propuesta de "embeber el visor en iframe" debe pasar por evaluación conjunta con el administrador del CMS (implica cambios en Tomcat que afectan a todos los consumidores del servidor).
4. **Cifrado de credenciales**: cualquier despliegue productivo debe partir de un `Web.config` cifrado (DPAPI o RSA). El repositorio nunca contiene el `Web.config` real — solo `Web.config.example` con placeholders.

---

## Historial de este documento

| Fecha | Cambio | Autor |
|---|---|---|
| 2026-08-27 | Versión inicial consolidada | Portal de Reportes Crystal — equipo BI |
