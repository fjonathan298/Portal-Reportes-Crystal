# Bibliografía oficial — Tecnologías SAP utilizadas en el Portal de Reportes Crystal

**Propósito**: lista específica de documentos oficiales SAP (PDFs y guías HTML) para cada tecnología, SDK y API utilizados en el portal. Dirigida al equipo revisor de código.

**Ambiente objetivo**: SAP BusinessObjects BI Platform 4.x + Crystal Reports for Visual Studio 13.0.4000.0 + .NET Framework 4.8

---

## Tecnologías utilizadas — resumen

| # | Tecnología | Versión | Ubicación en el código |
|---|---|---|---|
| 1 | **Crystal Reports for Visual Studio (CR4VS) SDK .NET** | 13.0.4000.0 | `Services/CacheParametros.cs`, `Controllers/ReportesController.cs` |
| 2 | **SAP BO RESTful Web Services** (`biprws`) | BI 4.2/4.3 | `Services/SapBoClient.cs` |
| 3 | **OpenDocument URL Reporting** | BI 4.x | `Controllers/SapboController.cs`, `ReportesCMC/catalogo.json` |
| 4 | **Web Intelligence Raylight REST SDK** | BI 4.2+ | `Services/SapBoClient.cs` |
| 5 | **Autenticación Enterprise / AD** | BI 4.x | `Web.config` (`SapBo:TipoAuth`) |

---

## 1. Crystal Reports for Visual Studio (CR4VS) SDK

### Documentos oficiales

| Documento | Formato | Enlace directo |
|---|---|---|
| **SAP Crystal Reports, developer version for Microsoft Visual Studio — Developer Guide** | PDF | https://help.sap.com/docs/SAP_CRYSTAL_REPORTS_DEVELOPER_VS/e21c39c2f18b4cbca4dcb2a4cd8ffc71/59c98e88f43b47b19dfd7c02be3ce2fe.html |
| **Portal de documentación** (índice completo) | HTML | https://help.sap.com/docs/SAP_CRYSTAL_REPORTS_DEVELOPER_VS |
| **API Reference — .NET Namespaces** | HTML | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_xi4sp7_rpt_dg_dotnet_en_pdf/4.2.7/en-US/xi4sp7_rpt_dg_dotnet_en.pdf |
| **Página oficial del producto** | HTML | https://www.crystalreports.com/crystal-reports-visual-studio/ |
| **Descargas del runtime** (S-user) | ZIP | https://origin.softwaredownloads.sap.com/public/site/index.html |

### SAP Notes clave (requieren S-user)

- **SAP Note 1970519** — *Crystal Reports, developer version for Microsoft Visual Studio — Downloads*
- **SAP Note 1218560** — *Deploying Crystal Reports Runtime*
- **SAP Note 1218562** — *How to deploy Crystal Reports for Visual Studio runtime files*

Portal para consultar Notes: https://launchpad.support.sap.com/#/notes

---

## 2. SAP BusinessObjects RESTful Web Services (`biprws`)

### Documentos oficiales

| Documento | Formato | Enlace directo |
|---|---|---|
| **Business Intelligence Platform RESTful Web Service Developer Guide** (BI 4.3) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_rest_ws_en_pdf/4.3/en-US/sbo43_bip_rest_ws_en.pdf |
| **Business Intelligence Platform RESTful Web Service Developer Guide** (BI 4.2) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_xi4sp7_bip_rest_ws_en_pdf/4.2.7/en-US/xi4sp7_bip_rest_ws_en.pdf |
| **Portal de documentación BI Platform** | HTML | https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM |
| **Business Intelligence Platform Administrator Guide** (incluye configuración RESTful) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_admin_en_pdf/4.3/en-US/sbo43_bip_admin_en.pdf |
| **API Catalog** (endpoints públicos) | HTML | https://api.sap.com/package/SAPBusinessObjectsBI/rest |

### SAP Notes / KBAs relevantes

- **KBA 2500067** — *BI Platform RESTful Web Services — troubleshooting overview*
- **KBA 2338440** — *How to authenticate with the RESTful Web Services*
- **KBA 2178141** — *Session management in BI RESTful Web Services*
- **KBA 2626051** — *Configuring the RESTful Web Service in CMC*

---

## 3. OpenDocument URL Reporting

### Documentos oficiales

| Documento | Formato | Enlace directo |
|---|---|---|
| **Viewing Documents Using OpenDocument** (BI 4.3) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_opendoc_en_pdf/4.3/en-US/sbo43_opendoc_en.pdf |
| **Viewing Documents Using OpenDocument** (BI 4.2 SP7) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_xi4sp7_opendoc_en_pdf/4.2.7/en-US/xi4sp7_opendoc_en.pdf |
| **Página HTML equivalente** | HTML | https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM/59f6b2ea491c458c8a3f2a63f60f65e0/58f9139d6fdb101497906a7cb0e91070.html |

### Parámetros implementados en el portal (cita textual de la guía)

Sección relevante del PDF: **"OpenDocument parameter overview"** — describe `iDocID`, `sIDType`, `sType`, `sOutputFormat`, `sRefresh`, `sWindow`, `lsS[Name]`, `lsN[Name]`, `lsD[Name]`, `lsM[Name]`, `lsR[Name]`.

---

## 4. Web Intelligence Raylight REST SDK

### Documentos oficiales

| Documento | Formato | Enlace directo |
|---|---|---|
| **Web Intelligence RESTful Web Service SDK — Developer Guide** (BI 4.3) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_webi_restful_ws_en_pdf/4.3/en-US/sbo43_webi_restful_ws_en.pdf |
| **Web Intelligence RESTful Web Service SDK — Developer Guide** (BI 4.2 SP7) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_xi4sp7_webi_restful_ws_en_pdf/4.2.7/en-US/xi4sp7_webi_restful_ws_en.pdf |
| **Portal de documentación de WebI** | HTML | https://help.sap.com/docs/SAP_BUSINESSOBJECTS_WEB_INTELLIGENCE |
| **Web Intelligence User Guide** (para entender el modelo de documento antes del API) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_webi_user_en_pdf/4.3/en-US/sbo43_webi_user_en.pdf |

### Endpoints implementados en el portal

- `GET /biprws/raylight/v1/documents` — sección **"Listing all documents"** de la guía
- `GET /biprws/raylight/v1/documents/<id>` — sección **"Getting document metadata"**
- `GET /biprws/raylight/v1/documents/<id>/parameters` — sección **"Document parameters (prompts)"**

---

## 5. Autenticación (Enterprise / AD / LDAP / Kerberos)

### Documentos oficiales

| Documento | Formato | Enlace directo |
|---|---|---|
| **Business Intelligence Platform Administrator Guide — Managing Authentication** | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_admin_en_pdf/4.3/en-US/sbo43_bip_admin_en.pdf |
| **Business Intelligence Platform User's Guide** | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_user_en_pdf/4.3/en-US/sbo43_bip_user_en.pdf |

Buscar en el Administrator Guide las secciones:
- **"Managing Enterprise authentication"**
- **"Managing Windows AD authentication"** (`secWinAD`)
- **"Configuring Kerberos SSO"**
- **"Configuring LDAP authentication"** (`secLDAP`)

### SAP Notes clave

- **SAP Note 1631734** — *How to configure Windows AD authentication in BI Platform*
- **SAP Note 1780469** — *Configuring Kerberos SSO for BI Platform*
- **SAP Note 1245178** — *Enterprise authentication — passwords and lockouts*
- **SAP Note 2323830** — *BI 4.x AD authentication troubleshooting*

---

## 6. Documentación complementaria

### Instalación / actualización de BI Platform

| Documento | Formato | Enlace directo |
|---|---|---|
| **Business Intelligence Platform Installation Guide for Windows** (BI 4.3) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_inst_win_en_pdf/4.3/en-US/sbo43_bip_inst_win_en.pdf |
| **Business Intelligence Platform Upgrade Guide** (BI 4.3) | PDF | https://help.sap.com/doc/businessobject_product_guides_boexir4_en_sbo43_bip_upgrade_en_pdf/4.3/en-US/sbo43_bip_upgrade_en.pdf |
| **Product Availability Matrix (PAM)** — compatibilidad de plataformas | HTML | https://support.sap.com/pam |

### Central Management Console (CMC)

| Documento | Formato | Enlace directo |
|---|---|---|
| **Business Intelligence Platform CMC Help** (referencia completa de administración) | HTML | https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM |

---

## 7. Ensamblados y dependencias .NET utilizadas

| Ensamblado | Versión | Fuente oficial |
|---|---|---|
| `CrystalDecisions.CrystalReports.Engine` | 13.0.4000.0 | SAP Crystal Reports Runtime for .NET Framework (redistribuible SAP) |
| `CrystalDecisions.Shared` | 13.0.4000.0 | Idem |
| `CrystalDecisions.ReportSource` | 13.0.4000.0 | Idem |
| `CrystalDecisions.Web` | 13.0.4000.0 | Idem |
| `System.Web.Mvc` | 5.2.9.0 | NuGet: `Microsoft.AspNet.Mvc` |
| `System.Web.Razor` | 3.2.9 | NuGet: `Microsoft.AspNet.Razor` |
| `PdfSharp` | 1.50.5147 | NuGet: `PDFsharp` (para post-proceso de PDFs) |

### Descarga de runtime Crystal Reports

- **Portal SAP Software Downloads** (S-user requerido): https://origin.softwaredownloads.sap.com/public/site/index.html
- Ruta: **SUPPORT PACKAGES AND PATCHES → By Category → SAP DEVELOPMENT TOOLS → SAP CRYSTAL REPORTS, DEVELOPER FOR VISUAL STUDIO**
- Archivo actual (13.0 SP35): `CRforVS_redist_install_64bit_13_0_35.zip`

---

## 8. Cómo acceder a los PDFs

### Sin cuenta SAP

Todos los enlaces marcados como *"help.sap.com/doc/..."* son **públicos** — no requieren S-user. Se pueden descargar directamente.

### Con cuenta SAP (S-user)

Los enlaces a *"launchpad.support.sap.com"* y *"origin.softwaredownloads.sap.com"* **requieren S-user activo con contrato de soporte**. Contacto interno para tramitarlo: el administrador de la cuenta de licencias SAP de la empresa.

### Si un enlace directo falla

SAP reorganiza sus portales periódicamente. Si un PDF no responde:

1. Ir a https://help.sap.com/docs/SAP_BUSINESSOBJECTS_BUSINESS_INTELLIGENCE_PLATFORM
2. Seleccionar la **versión exacta** del servidor SAP BO en producción (dropdown superior)
3. En el panel izquierdo, expandir **"Documentation"**
4. Buscar por el **título exacto** del documento (los títulos son estables aunque las URLs cambien)
5. Cada documento tiene un botón **"Download PDF"** en la esquina superior derecha

---

## 9. Notas para el revisor

1. **CR4VS ≠ Crystal Reports 2020/Designer.** El SDK utilizado (`13.0.4000.0`) es **Crystal Reports developer for Visual Studio**, no la versión standalone. Son productos con licenciamiento distinto y ciclo de vida separado.
2. **La versión exacta del servidor SAP BO** debe validarse en producción antes de tomar la documentación de BI 4.2 o BI 4.3 — las diferencias entre SPs afectan endpoints REST específicos.
3. **`X-Frame-Options: DENY`** es política obligatoria de SAP BO 4.x. Documentado en el **Administrator Guide**, sección *"Security"*. Cualquier propuesta de embed requiere excepción formal a nivel de Tomcat del CMS.
4. **API pública catalogada por SAP**: https://api.sap.com/package/SAPBusinessObjectsBI — el revisor puede consultar aquí un catálogo curado con specs OpenAPI de los endpoints públicos.

---

## Historial

| Fecha | Cambio |
|---|---|
| 2026-08-27 | Versión inicial con enlaces directos a PDFs oficiales |
