# Uso del API REST de SAP BusinessObjects y Arquitectura de Auditoria

**Autor:** Jonathan Flores — BI/DWH, Super Repuestos El Salvador  
**Fecha:** 26 de agosto de 2026  
**Proposito:** documentar la implementacion del SDK/API REST de SAP BusinessObjects
en el Portal de Reportes Crystal, y la arquitectura de auditoria construida sobre el.
Este documento responde al compromiso de la minuta del Portal SIG del 20 de agosto
de 2026 respecto a la documentacion del uso del SDK de SAP BO, los alcances de la
API y la cobertura de auditoria de accesos, filtros y permisos.

---

## 1. Uso del SDK / API REST de SAP BusinessObjects

### 1.1 Conexion y autenticacion

El portal se conecta al servidor SAP BusinessObjects 4.x mediante dos interfaces:

| Interface | Endpoint | Puerto | Uso |
|---|---|---|---|
| REST API (biprws) | `http://<servidor>:6405/biprws` | 6405 | Descubrimiento de reportes, metadatos, estadisticas |
| OpenDocument (CMC) | `http://<servidor>:8080/BOE/OpenDocument/opendoc/custom.jsp` | 8080 | Visualizacion de reportes en el visor nativo de SAP BO |

**Flujo de autenticacion REST API:**

1. POST a `/biprws/logon/long` con cuerpo XML:
   ```xml
   <attrs xmlns="http://www.sap.com/rws/bip">
     <attr name="userName" type="string">...</attr>
     <attr name="password" type="string">...</attr>
     <attr name="auth" type="string">secEnterprise</attr>
   </attrs>
   ```
2. La respuesta contiene un `logonToken` (bttoken) con validez de ~30 minutos.
3. El token se envia en todas las peticiones subsecuentes via header
   `X-SAP-LogonToken` (URL-encoded, entre comillas).
4. El portal cachea el token en memoria por 25 minutos y lo renueva
   transparentemente al expirar.

**Flujo de autenticacion OpenDocument (visor nativo):**

1. POST a `/BOE/BI/logon/start.do` con credenciales Enterprise
   (`cms`, `username`, `password`, `auth_type=secEnterprise`).
2. El servidor responde con cookie `JSESSIONID` que autoriza la sesion del visor.
3. El visor SAP BO recibe un token por URL (`&token=...`) que vincula la sesion
   autenticada con el documento solicitado.

**Archivos involucrados:**

- `Services/SapBoClient.cs` lineas 176-245: metodo `ObtenerToken()`.
- `Web.config`: claves `SapBo:ApiUrl`, `SapBo:Usuario`, `SapBo:Password`,
  `SapBo:TipoAuth`, `SapBo:CmsName`, `SapBo:LogonPath`.

### 1.2 Limitacion confirmada de la API REST

La API REST `/biprws/` del servidor SAP BO 4.x expone un unico EntitySet:

```json
{"EntitySets": ["infostore"]}
```

Esto significa que la API **solo permite consultar metadatos** (nombre, CUID,
tipo, carpeta, fecha de modificacion) de los objetos publicados en el CMS.
**No existe un endpoint para exportar o descargar el contenido** (PDF, Excel,
datos) de un reporte Crystal o WebI.

**Verificaciones realizadas:**

| Endpoint probado | Resultado |
|---|---|
| `GET /biprws/infostore/{id}` | 200 — devuelve metadatos JSON/XML, no contenido |
| `GET /biprws/infostore/{id}` con `Accept: application/pdf` | 406 Not Acceptable |
| `GET /biprws/v1/documents/{id}` | 200 — metadatos; 406 para Accept: application/pdf |
| `GET /biprws/v1/documents/{id}/content` | 404 Not Found |
| `GET /biprws/v1/crystalreports/{id}` | 404 Not Found |
| `GET /biprws/v1/reports/{id}` | 404 Not Found |
| `GET /biprws/raylight/v1/documents/{id}` | 404 (raylight es exclusivo para WebI) |
| `POST /biprws/infostore/{id}/scheduleForms/now` | 201 — crea instancia, pero la instancia no ofrece endpoint de descarga (`/content` retorna 404) |
| `DELETE /biprws/infostore/{instancia_id}` | 405 Method Not Allowed |

**Conclusion:** la API REST de SAP BO 4.x en esta version no soporta la
exportacion programatica de Crystal Reports. La unica forma de visualizar un
reporte es a traves del visor nativo OpenDocument (ver seccion 1.4).

### 1.3 Implementacion en SapBoClient.cs

El cliente REST API esta implementado como clase estatica en
`Services/SapBoClient.cs` (1,087 lineas). Funcionalidad:

**Descubrimiento de reportes:**

- `ObtenerReportes()`: punto de entrada principal. Retorna una lista unificada
  de Crystal Reports y WebI publicados en el CMS.
- `ConsultarWebI(token)`: consulta reportes WebI via
  `/biprws/infostore?query=SELECT ... FROM CI_INFOOBJECTS WHERE SI_KIND='Webi'`.
- `ConsultarCrystalReports(token)`: descubre Crystal Reports via
  `/biprws/infostore/cuid_<CUID>` recursivo desde la raiz de carpetas.
- `BuscarCrystalEnCarpeta(token, folderId, ...)`: recursion en carpetas
  incluyendo tipos `Folder`, `User` y `PersonalCategory` (correccion aplicada
  para no omitir carpetas de usuario).
- Cache en memoria con TTL configurable (default 15 minutos). Si la API falla,
  retorna la ultima cache valida en lugar de error.

**Estadisticas del CMS:**

- `ObtenerEstadisticasConexiones(token)`: sesiones activas, usuarios conectados,
  licencias en uso (Current Access Level, Named Users, Concurrent).
- `ObtenerLicencias(token)`: detalle de licencias por tipo.
- `ObtenerServidores(token)`: listado de servidores del CMS con estado
  (Running/Stopped/Failed).

**Resultados en produccion:** 95-96 Crystal Reports descubiertos + WebI
listados en el portal, presentados en un listado unificado con busqueda y
filtros.

### 1.4 Enfoques evaluados para visualizacion embebida

El requerimiento original era visualizar los reportes SAP BO **dentro del
portal** (iframe) en lugar de abrir una pestana externa, para capturar eventos
de uso con mayor granularidad. Se evaluaron exhaustivamente los siguientes
enfoques:

| # | Enfoque | Resultado | Motivo de descarte |
|---|---|---|---|
| 1 | **iframe directo** al OpenDocument URL | Pantalla en blanco | Cookies third-party bloqueadas por el navegador; `Set-Cookie: JSESSIONID` con `Path=/BOE; HttpOnly` no se envia en contexto cross-origin |
| 2 | **iframe sin sandbox** | Error "Error al intentar ver el documento" | El visor SAP UI5 requiere cookies de sesion que el iframe cross-origin no puede mantener |
| 3 | **Proxy interno con URL rewrite** (SapboController.Contenido) | Login y auto-submit exitosos; visor final en blanco | El visor OpenDocument es una SPA SAP UI5/YUI que carga decenas de sub-recursos JS/CSS con URLs relativas internas; reescribir todas las referencias es inviable y fragil |
| 4 | **Proxy con auto-submit recursivo** (EjecutarAutoSubmit) | Cadena de 3 POSTs exitosos server-side | El paso final sigue devolviendo HTML del visor SPA, no el contenido del reporte |
| 5 | **Header `Accept: application/pdf`** en proxy | CMC ignora el header | Siempre retorna `Content-Type: text/html` con el visor JavaScript, independientemente del Accept solicitado |
| 6 | **REST API para exportar contenido** (seccion 1.2) | 404 / 406 en todos los endpoints de contenido | La API no expone exportacion de Crystal Reports |
| 7 | **`scheduleForms/now`** para generar instancia y descargarla | 201 (instancia creada), pero `/content` del resultado retorna 404 | Las instancias no ofrecen endpoint de descarga via REST |

**Solucion adoptada:**

Pagina intermedia (`Views/Sapbo/Ver.cshtml`) que presenta una card informativa
con el nombre, categoria y tipo del reporte, y un boton
**"Abrir reporte en SAP BO"** que:

1. Registra el evento de auditoria `VER_REPORTE` con todos los parametros
   (`ls*` del OpenDocument URL: almacen, pais, fechas, etc.).
2. Registra un segundo evento `DESCARGA_IFRAME` al hacer click en el boton.
3. Redirige al usuario al visor nativo de SAP BO en una nueva pestana
   (`target="_blank"` via `Sapbo/AbrirExterno`).

Esta solucion captura la trazabilidad completa del acceso **antes** de que el
usuario salga del portal, sin depender de que el visor SAP BO sea embebible.

**Archivos involucrados:**

- `Controllers/SapboController.cs`: metodos `Ver()` (linea ~270) y
  `AbrirExterno()` (linea ~180).
- `Views/Sapbo/Ver.cshtml`: pagina intermedia con card y boton.
- `Views/Home/Index.cshtml`: los 106 enlaces de reportes SAP BO redirigidos
  a traves de `/Sapbo/Ver` en lugar de `target="_blank"` directo.

---

## 2. Arquitectura de auditoria construida

### 2.1 AuditoriaService.cs — servicio central

Clase estatica thread-safe en `Services/AuditoriaService.cs` (525 lineas).

**Arquitectura:**

```
Request HTTP
    |
    v
AuditAttribute (filtro global)          <-- asigna SesionId
    |
    v
Controller (Home/Reportes/Sapbo)        <-- emite EventoAuditoria
    |
    v
AuditoriaService.RegistrarEvento()      <-- enqueue no-bloqueante
    |
    v
ConcurrentQueue<EventoAuditoria>        <-- cola en memoria
    |
    v  (cada 5 segundos, via System.Threading.Timer)
InsertarLote()                          <-- INSERT en audit.Evento + audit.EventoParametro
    |                                       por lotes de hasta 500 eventos
    v (si BD no disponible)
audit_pending.jsonl                     <-- fallback JSONL en App_Data
    |
    v  (en el siguiente ciclo de flush)
ReintentarPendientes()                  <-- reintenta INSERT desde JSONL
```

**Caracteristicas clave:**

- **No bloqueante**: `RegistrarEvento()` solo hace `_cola.Enqueue()` y retorna
  inmediatamente. El request HTTP no espera a la BD.
- **Por lotes**: un `Timer` ejecuta `InsertarLote()` cada N segundos (default 5).
  Agrupa hasta 500 eventos por ciclo.
- **Resiliente**: si la conexion a SQL Server falla, el evento se serializa a
  `App_Data/audit_pending.jsonl` en formato JSON Lines. El siguiente ciclo de
  flush intenta reinsertar los pendientes antes de procesar la cola nueva.
- **El portal nunca falla por auditoria**: todos los bloques de auditoria estan
  envueltos en `try/catch` silencioso. Una BD caida no afecta la operacion del
  portal.
- **Cache de tipos**: `ObtenerTiposCache()` carga la tabla `audit.EventoTipo`
  en memoria una vez y resuelve codigos de evento (`VER_REPORTE`, `EXPORTAR_PDF`)
  a IDs numericos sin consulta por cada INSERT.

**Conexion a SQL Server**: via Windows Authentication (`Integrated Security=SSPI`).
La cuenta del Application Pool de IIS se autentica directamente, sin credenciales
en el connection string.

### 2.2 AuditAttribute.cs — filtro global

Clase `AuditAttribute` en `Filters/AuditAttribute.cs` (79 lineas). Hereda de
`ActionFilterAttribute` y esta registrado globalmente en `Global.asax.cs`:

```csharp
GlobalFilters.Filters.Add(new AuditAttribute());
```

**Responsabilidad unica:** en `OnActionExecuting`, verifica si el usuario esta
autenticado y llama a `AuditoriaService.ObtenerSesionActual(ctx)`, que:

- Si es la primera peticion del usuario en esta sesion HTTP, crea un nuevo
  `SesionId` (GUID), lo almacena en `Session` y registra un evento `LOGIN`.
- Si ya existe, retorna el `SesionId` existente y actualiza
  `UltimaActividadUtc` en la sesion.

El `SesionId` se guarda en `HttpContext.Items["AuditSesionId"]` para que los
controladores lo lean via `AuditContext.SesionActual(HttpContext)` sin repetir
la consulta.

**Diseno intencional:** el filtro NO registra un evento por cada request (eso
llenaría la BD de ruido por cada CSS/JS/imagen). Solo garantiza la correlacion
de sesion. Los eventos de negocio se emiten explicitamente desde los
controladores.

### 2.3 Hooks activos por controlador

| Controlador | Metodo | Tipo de evento | Datos capturados |
|---|---|---|---|
| `HomeController` | `Index()` | `VER_LISTADO` | Total de reportes en el listado, usuario, IP |
| `ReportesController` | `Ver()` | `VER_REPORTE` | raizId, path del .rpt, nombre, tipo=Local, parametros `p_*` del querystring |
| `ReportesController` | `Preview()` | `PREVIEW` | raizId, path, duracion (ms), tamano (bytes) |
| `ReportesController` | `Exportar()` | `EXPORTAR_PDF` / `EXPORTAR_EXCEL` / `EXPORTAR_EXCELDATA` / `EXPORTAR_WORD` | raizId, path, formato, duracion (ms), tamano (bytes) |
| `ReportesController` | `Ver()`, `Preview()`, `Exportar()` (catch) | `ERROR_GENERACION` | raizId, path, mensaje de error |
| `SapboController` | `Ver()` | `VER_REPORTE` | CUID, nombre, categoria, tipo (Sapbo/WebI), servidor, URL de origen, parametros `ls*` del OpenDocument URL |
| `SapboController` | `AbrirExterno()` | `DESCARGA_IFRAME` | CUID, nombre, categoria, tipo, URL destino |
| `AuditoriaController` | `RegistrarInteraccion()` | (dinamico, via POST JSON) | tipo, CUID, formato — endpoint para JS del cliente |
| `AuditoriaController` | `Dashboard()` | `ACCESO_DASHBOARD` | usuario que accede al dashboard de auditoria |

**Captura de parametros/filtros:**

- **Reportes Crystal locales** (`ReportesController`): el helper `AuditarReporte()`
  itera el querystring buscando claves con prefijo `p_` (convencion del portal
  para parametros Crystal). Cada `p_ALMACEN=06` se registra en
  `audit.EventoParametro` como `NombreParametro='ALMACEN'`,
  `ValorParametro='06'`.

- **Reportes SAP BO** (`SapboController.Ver()`): itera el querystring buscando
  claves con prefijo `ls` (convencion OpenDocument). Cada `lsSALMACEN=06` se
  parsea como tipo `S` (String) + nombre `ALMACEN` y se registra en
  `audit.EventoParametro`. Soporta prefijos `lsS` (String), `lsN` (Number),
  `lsD` (Date).

### 2.4 Comportamiento de resiliencia

El diseno garantiza que **la auditoria nunca interrumpe la operacion del portal**:

1. **Todos los bloques de auditoria en controladores** estan envueltos en
   `try { ... } catch { }`. Una excepcion en auditoria se traga silenciosamente.

2. **El filtro global `AuditAttribute`** tiene su propio `try/catch`. Si
   `ObtenerSesionActual` falla, el request continua sin `SesionId`.

3. **Si SQL Server no esta disponible** al momento del flush:
   - `InsertarLote()` captura la excepcion.
   - Serializa los eventos pendientes a `App_Data/audit_pending.jsonl` en
     formato JSON Lines (un JSON por linea, append-only).
   - En el siguiente ciclo de flush (5 segundos despues), `ReintentarPendientes()`
     intenta leer el archivo JSONL y reinsertar los eventos en la BD.
   - Si el reintento tiene exito, el archivo JSONL se limpia.
   - Si falla de nuevo, los eventos permanecen en el archivo hasta el proximo
     ciclo.

4. **El servicio se inicializa gracefully**: si la conexion a la BD no esta
   disponible al arrancar (`Application_Start`), el servicio se marca como
   inicializado pero opera en modo fallback desde el inicio.

---

## 3. Cobertura frente a los tres niveles solicitados por Portal SIG

La minuta del Portal SIG del 20 de agosto de 2026 solicita cobertura en tres
niveles: accesos, filtros y permisos.

| Nivel | Estado | Detalle de implementacion |
|---|---|---|
| **Accesos** | **Cubierto** | Cada apertura de reporte (Crystal local o SAP BO) queda registrada en `audit.Evento` con usuario, IP, timestamp UTC, nombre del reporte, categoria, tipo de reporte y servidor de origen. El evento `VER_LISTADO` registra tambien cuando el usuario accede al listado principal. Las exportaciones se registran con formato, duracion y tamano del archivo generado. |
| **Filtros** | **Parcialmente cubierto** | Los parametros/filtros aplicados se capturan en `audit.EventoParametro`: para Crystal locales via prefijo `p_*` del querystring, para SAP BO via prefijo `ls*` del OpenDocument URL (almacen, pais, fechas, etc.). La cobertura depende de que los parametros se pasen por URL; filtros aplicados *dentro* del visor nativo de SAP BO (despues de la redireccion) no son capturables por el portal. |
| **Permisos** | **No cubierto** | La auditoria registra *quien accedio* a *que reporte*, pero no gestiona ni audita *quien tiene permiso* de acceder a que. El portal usa Windows Authentication sin restricciones por reporte: cualquier usuario autenticado del dominio puede ver cualquier reporte del listado. Una capa de permisos granulares (por reporte, por grupo AD, por pais) requiere definicion funcional (pendiente de Ivan) antes de poder implementarse. |

**Nota sobre filtros SAP BO:** cuando el usuario hace click en "Abrir reporte
en SAP BO", los filtros que vienen en la URL del OpenDocument (`lsSALMACEN`,
`lsSPAIS`, `lsSDESDE`, etc.) se capturan en el evento `VER_REPORTE` **antes**
de la redireccion. Si el usuario modifica filtros dentro del visor nativo de
SAP BO (despues de abrir la pestana externa), esos cambios no son visibles
para el portal. Para capturar esos cambios se requeriria activar la auditoria
nativa de SAP BO desde CMC > Auditoria, lo cual es una configuracion del
servidor SAP BO independiente del portal.

---

## 4. Mapeo de reportes y carpetas via API REST de SAP BO

### 4.1 Estructura jerarquica del CMS

El CMS (Central Management Server) de SAP BO organiza los objetos publicados
en una jerarquia de carpetas similar a un sistema de archivos. La estructura
tipica en produccion es:

```
Root Folder
├── Public Folders                       (carpeta raiz permitida)
│   ├── Finanzas                         (nivel 1 — categoria principal)
│   │   ├── CxC                          (nivel 2 — subcategoria)
│   │   │   ├── Mensuales                (nivel 3 — sub-subcategoria)
│   │   │   │   └── Reporte_CxC_Mensual.rpt
│   │   │   └── Reporte_CxC_Diario.rpt
│   │   └── Reporte_Balance.rpt
│   ├── Inventarios                      (nivel 1)
│   │   ├── Kardex                       (nivel 2)
│   │   │   └── Kardex_Detalle.rpt
│   │   └── Existencias.rpt
│   ├── Ventas                           (nivel 1)
│   │   ├── Por Pais                     (nivel 2)
│   │   │   ├── SV                       (nivel 3)
│   │   │   │   └── Ventas_SV.wid
│   │   │   └── HN                      (nivel 3)
│   │   │       └── Ventas_HN.wid
│   │   └── Consolidado_Ventas.wid
│   └── RRHH                             (nivel 1)
│       └── Planilla.rpt
├── User Folders                         (carpeta raiz permitida)
│   └── <usuario>/
│       └── Reportes personales
└── Carpetas del sistema (excluidas)
    ├── Temporary Storage
    ├── Instances
    └── ~WebIntelligence
```

**Profundidad maxima configurada:** 10 niveles (parametro `depth > 10` en
`BuscarCrystalEnCarpeta`). En la practica, la mayoria de reportes estan entre
los niveles 1 y 4.

### 4.2 Descubrimiento de carpetas raiz

**Endpoint:** `GET /biprws/infostore`

Retorna los EntitySets disponibles. El portal consulta el nodo raiz del
infostore para obtener las carpetas de primer nivel:

**Endpoint:** `GET /biprws/infostore/Root%20Folder/children?pageSize=200`

**Respuesta (simplificada):**
```json
{
  "entries": [
    {
      "id": "42",
      "name": "Public Folders",
      "type": "Folder",
      "cuid": "AcXyz..."
    },
    {
      "id": "58",
      "name": "User Folders",
      "type": "Folder",
      "cuid": "BdWyz..."
    },
    {
      "id": "99",
      "name": "Temporary Storage",
      "type": "Folder"
    }
  ]
}
```

**Filtro aplicado en el portal:** solo se recorren las carpetas cuyo `name`
esta en la lista `carpetasRaizPermitidas`:

- `"Public Folders"` / `"Carpetas publicas"` / `"Carpetas públicas"`
- `"User Folders"` / `"Carpetas de usuario"`

Las carpetas del sistema (`Temporary Storage`, `Instances`, `Desktop Add-ons`,
`~WebIntelligence`, etc.) se excluyen via el conjunto `_carpetasExcluidas`.

### 4.3 Recorrido recursivo de subcarpetas (Crystal Reports)

**Endpoint:** `GET /biprws/infostore/{folderId}/children?pageSize=200`

Desde cada carpeta raiz permitida, el metodo `BuscarCrystalEnCarpeta` recorre
recursivamente todos los hijos. Para cada entrada evalua el campo `type`:

| Valor de `type` | Accion |
|---|---|
| `Folder`, `User`, `PersonalCategory`, `FavoritesFolder` | Si no esta en `_carpetasExcluidas`, entra recursivamente y acumula la ruta |
| Contiene `CrystalReport`, `Crystal Report` o `CR4E` | Lo agrega como reporte Crystal con la ruta acumulada como carpeta |
| Cualquier otro | Lo ignora (no es Crystal ni carpeta navegable) |

**Respuesta por entrada (simplificada):**
```json
{
  "id": "12345",
  "name": "Kardex_Detalle",
  "type": "CrystalReport",
  "cuid": "FnKv3...",
  "description": ""
}
```

**Acumulacion de la ruta completa:**

La ruta se construye concatenando el nombre de cada carpeta padre con el
separador ` / `. Ejemplo de acumulacion durante la recursion:

| Profundidad | `pathPadre` al entrar | Nombre de la carpeta | `subPath` resultante |
|---|---|---|---|
| 0 | `""` | `"Finanzas"` | `"Finanzas"` |
| 1 | `"Finanzas"` | `"CxC"` | `"Finanzas / CxC"` |
| 2 | `"Finanzas / CxC"` | `"Mensuales"` | `"Finanzas / CxC / Mensuales"` |

Cuando se encuentra un Crystal Report en profundidad 2, su campo `Carpeta`
sera `"Finanzas / CxC / Mensuales"` — la ruta completa, no solo el nombre
de la carpeta inmediata.

**Cache compartido:** cada carpeta visitada se registra en
`_cacheRutas[folderId] = subPath`. Este cache es un `ConcurrentDictionary`
estatico compartido entre las consultas de Crystal y WebI, evitando llamadas
duplicadas a la API.

### 4.4 Resolucion de carpetas para WebI (cadena de padres)

**Endpoint para listar WebI:** `GET /biprws/raylight/v1/documents`

La API Raylight retorna documentos WebI con su `folderId` pero **sin la ruta
completa** de la carpeta. Para reconstruir la jerarquia, el portal usa dos
estrategias complementarias:

**Estrategia 1 — Cache compartido (rapida, sin llamada API adicional):**

Si el `folderId` del WebI ya fue visitado durante el recorrido de Crystal
Reports (`_cacheRutas` ya tiene la ruta), se reutiliza directamente:

```
_cacheRutas.GetOrAdd(folderId, id => ResolverRutaCarpeta(id, token))
```

**Estrategia 2 — Resolucion ascendente via `parentId` (fallback):**

Si el `folderId` no esta en cache (el WebI esta en una carpeta que Crystal
no visito), el metodo `ResolverRutaCarpeta` camina hacia arriba por la
jerarquia usando el campo `parentId` de cada carpeta:

**Endpoint:** `GET /biprws/infostore/{folderId}`

**Respuesta:**
```json
{
  "id": "789",
  "name": "Por Pais",
  "type": "Folder",
  "parentId": "456"
}
```

**Algoritmo de resolucion ascendente:**

1. Consultar `GET /biprws/infostore/{currentId}`.
2. Extraer `name` y `parentId`.
3. Si `name` es una carpeta raiz (`Root Folder`, `Public Folders`, etc.)
   o excluida, detenerse.
4. Si `currentId` ya esta en `_cacheRutas`, usar la ruta cacheada como
   prefijo y detenerse.
5. Agregar `name` al inicio de la lista de partes.
6. Avanzar a `currentId = parentId`.
7. Repetir hasta un maximo de 12 niveles.
8. Unir las partes con ` / ` para formar la ruta completa.

**Ejemplo de resolucion para un WebI en `Ventas / Por Pais / SV`:**

| Iteracion | `currentId` | `name` obtenido | `parentId` | Accion |
|---|---|---|---|---|
| 1 | `"789"` (SV) | `"SV"` | `"456"` | partes = [`"SV"`] |
| 2 | `"456"` (Por Pais) | `"Por Pais"` | `"123"` | partes = [`"Por Pais"`, `"SV"`] |
| 3 | `"123"` (Ventas) | `"Ventas"` | `"42"` | partes = [`"Ventas"`, `"Por Pais"`, `"SV"`] |
| 4 | `"42"` (Public Folders) | `"Public Folders"` | — | Es carpeta raiz → detenerse |

**Resultado:** `"Ventas / Por Pais / SV"`.

La ruta resuelta se almacena en `_cacheRutas[folderId]` para evitar repetir
la cadena de consultas si otro WebI esta en la misma carpeta.

### 4.5 Como se muestra en el portal

La ruta completa de la carpeta se asigna al campo `Carpeta` del modelo
`ReporteWebI`. En `HomeController.CargarReportesWebI()`, este campo se mapea
a `Categoria` del `ReporteInfo`:

```csharp
Categoria = w.Carpeta   // "Finanzas / CxC / Mensuales"
```

La vista `Index.cshtml` agrupa los reportes SAP BO por `Categoria` en
secciones colapsables (`<details>`). Esto significa que los reportes se
presentan agrupados por su ruta completa en el CMS:

```
▸ Finanzas / CxC / Mensuales          (2 reportes)
▸ Finanzas / CxC                       (1 reporte)
▸ Inventarios / Kardex                 (1 reporte)
▸ Ventas / Por Pais / SV              (1 reporte)
▸ Ventas / Por Pais / HN              (1 reporte)
▸ Ventas                               (1 reporte)
```

### 4.6 Rendimiento y optimizaciones

| Aspecto | Detalle |
|---|---|
| **Cache de rutas** | `ConcurrentDictionary<string, string>` estatico compartido entre Crystal y WebI. Se llena durante el recorrido de Crystal y se reutiliza para WebI. |
| **Cache de resultados** | Todos los reportes (Crystal + WebI) se cachean por 15 minutos (configurable). Las llamadas a la API solo ocurren al expirar el cache. |
| **Profundidad maxima Crystal** | 10 niveles de recursion. Carpetas mas profundas se ignoran silenciosamente. |
| **Profundidad maxima WebI** | 12 niveles de resolucion ascendente via `parentId`. |
| **Paginacion** | `pageSize=200` en cada consulta de hijos. Si una carpeta tiene mas de 200 objetos, solo se ven los primeros 200 (en la practica ninguna carpeta del CMC tiene tantos hijos directos). |
| **Thread-safety** | `ConcurrentDictionary` y `TryAdd` garantizan que el cache sea seguro en escenarios multi-hilo (IIS con multiples requests simultaneos). |

### 4.7 Archivos involucrados

| Archivo | Lineas | Rol |
|---|---|---|
| `Services/SapBoClient.cs` | 36-44 | Definicion de `_cacheRutas` y `_carpetasRaiz` |
| `Services/SapBoClient.cs` | 46-53 | Definicion de `_carpetasExcluidas` |
| `Services/SapBoClient.cs` | 315-355 | `ConsultarCrystalReports`: recorre carpetas raiz |
| `Services/SapBoClient.cs` | 357-420 | `BuscarCrystalEnCarpeta`: recursion con acumulacion de ruta |
| `Services/SapBoClient.cs` | 260-310 | `ConsultarWebI`: resolucion de carpeta con cache compartido |
| `Services/SapBoClient.cs` | 955-997 | `ResolverRutaCarpeta`: cadena ascendente via `parentId` |
| `Controllers/HomeController.cs` | 317-342 | `CargarReportesWebI`: mapeo `Carpeta` → `Categoria` |
| `Views/Home/Index.cshtml` | — | Agrupacion visual por `Categoria` en `<details>` |

### 4.8 Referencia rapida de endpoints utilizados

| Endpoint | Metodo | Uso en el portal |
|---|---|---|
| `GET /biprws/infostore` | GET | Verificar EntitySets disponibles |
| `GET /biprws/infostore/{id}/children?pageSize=200` | GET | Listar hijos de una carpeta (recursion) |
| `GET /biprws/infostore/{id}` | GET | Obtener metadatos de una carpeta (`name`, `parentId`) para resolucion ascendente |
| `GET /biprws/raylight/v1/documents` | GET | Listar documentos WebI con `folderId` |
| Header `X-SAP-LogonToken` | — | Autenticacion en todas las peticiones |

