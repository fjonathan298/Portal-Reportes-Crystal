using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace PortalReportesCrystal.Services
{
    public class ReporteWebI
    {
        public string Nombre { get; set; }
        public string CUID { get; set; }
        public string Carpeta { get; set; }
        public string Descripcion { get; set; }
        public string UrlOpenDocument { get; set; }
        public string TipoDocumento { get; set; } = "WebI";
    }

    public static class SapBoClient
    {
        private static readonly object _lockToken = new object();
        private static string _token;
        private static DateTime _tokenExpira = DateTime.MinValue;

        private static List<ReporteWebI> _cacheResultados;
        private static DateTime _cacheExpira = DateTime.MinValue;
        private static readonly object _lockCache = new object();

        private static string _logPath;

        private static readonly HashSet<string> _carpetasExcluidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Almacenamiento temporal", "Complementos de escritorio",
            "Demonstration", "Feature Samples", "Report Samples",
            "Alert Notifications", "Personal Folders", "Categories",
            "Instances", "Temporary Storage", "Desktop Add-ons",
            "~WebIntelligence"
        };

        public static bool Habilitado
        {
            get
            {
                string val = ConfigurationManager.AppSettings["SapBo:Habilitado"];
                return string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string ApiUrl
        {
            get { return (ConfigurationManager.AppSettings["SapBo:ApiUrl"] ?? "").TrimEnd('/'); }
        }

        private static string OpenDocUrl
        {
            get { return (ConfigurationManager.AppSettings["SapBo:OpenDocumentUrl"] ?? "").TrimEnd('/'); }
        }

        private static string Usuario
        {
            get { return ConfigurationManager.AppSettings["SapBo:Usuario"] ?? ""; }
        }

        private static string Password
        {
            get { return ConfigurationManager.AppSettings["SapBo:Password"] ?? ""; }
        }

        private static string TipoAuth
        {
            get { return ConfigurationManager.AppSettings["SapBo:TipoAuth"] ?? "secEnterprise"; }
        }

        private static int CacheTTLMinutos
        {
            get
            {
                int val;
                return int.TryParse(ConfigurationManager.AppSettings["SapBo:CacheTTLMinutos"], out val) ? val : 15;
            }
        }

        private static bool _servicePointConfigurado;

        private static void ConfigurarServicePoint()
        {
            if (_servicePointConfigurado) return;
            _servicePointConfigurado = true;
            try
            {
                string apiUrl = ApiUrl;
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    var sp = ServicePointManager.FindServicePoint(new Uri(apiUrl));
                    sp.ConnectionLimit = 10;
                    sp.ConnectionLeaseTimeout = 60000;
                }
            }
            catch { }
        }

        public static void Inicializar(string appDataPath)
        {
            _logPath = Path.Combine(appDataPath, "errores_sapbo.log");
        }

        public static List<ReporteWebI> ObtenerReportes()
        {
            if (!Habilitado)
                return new List<ReporteWebI>();

            lock (_lockCache)
            {
                if (_cacheResultados != null && DateTime.UtcNow < _cacheExpira)
                    return new List<ReporteWebI>(_cacheResultados);
            }

            try
            {
                ConfigurarServicePoint();
                string token = ObtenerToken();
                RegistrarInfo("Token obtenido OK (longitud=" + token.Length + ")");
                var resultados = ConsultarWebI(token);
                var crystalReports = ConsultarCrystalReports(token);
                resultados.AddRange(crystalReports);
                lock (_lockCache)
                {
                    _cacheResultados = resultados;
                    _cacheExpira = DateTime.UtcNow.AddMinutes(CacheTTLMinutos);
                }
                return new List<ReporteWebI>(resultados);
            }
            catch (Exception ex)
            {
                RegistrarError("ObtenerReportes", ex);

                lock (_lockCache)
                {
                    if (_cacheResultados != null)
                        return new List<ReporteWebI>(_cacheResultados);
                }

                return new List<ReporteWebI>();
            }
        }

        public static bool DatosDesdeCache
        {
            get
            {
                lock (_lockCache)
                {
                    return _cacheResultados != null && DateTime.UtcNow < _cacheExpira;
                }
            }
        }

        public static DateTime? UltimaActualizacion
        {
            get
            {
                lock (_lockCache)
                {
                    if (_cacheResultados == null)
                        return null;
                    return _cacheExpira.AddMinutes(-CacheTTLMinutos);
                }
            }
        }

        private static string ObtenerToken()
        {
            lock (_lockToken)
            {
                if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpira)
                    return _token;

                _token = null;

                string url = ApiUrl + "/logon/long";
                string body = "<attrs xmlns=\"http://www.sap.com/rws/bip\">"
                    + "<attr name=\"userName\" type=\"string\">" + EscapeXml(Usuario) + "</attr>"
                    + "<attr name=\"password\" type=\"string\">" + EscapeXml(Password) + "</attr>"
                    + "<attr name=\"auth\" type=\"string\">" + EscapeXml(TipoAuth) + "</attr>"
                    + "</attrs>";

                HttpWebResponse response = null;
                try
                {
                    var request = CrearRequest(url, "POST");
                    request.ContentType = "application/xml";
                    request.Accept = "application/xml";

                    byte[] data = Encoding.UTF8.GetBytes(body);
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    response = (HttpWebResponse)request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string respBody = reader.ReadToEnd();
                        _token = ExtraerValorXml(respBody, "logonToken");

                        if (string.IsNullOrEmpty(_token))
                        {
                            RegistrarError("ObtenerToken", new InvalidOperationException(
                                "Respuesta sin logonToken. Body (primeros 300): " +
                                respBody.Substring(0, Math.Min(300, respBody.Length))));
                            throw new InvalidOperationException("El API de SAP BO no devolvio logonToken.");
                        }

                        _token = _token.Trim().Trim('"');
                        _token = _token
                            .Replace("&amp;", "&")
                            .Replace("&lt;", "<")
                            .Replace("&gt;", ">")
                            .Replace("&quot;", "\"")
                            .Replace("&apos;", "'");

                        _tokenExpira = DateTime.UtcNow.AddMinutes(25);
                        return _token;
                    }
                }
                catch (WebException wex)
                {
                    string detalle = "Login fallido.";
                    if (wex.Response != null)
                    {
                        using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                        {
                            string errBody = sr.ReadToEnd();
                            detalle += " Body: " + errBody.Substring(0, Math.Min(500, errBody.Length));
                        }
                    }
                    RegistrarError("ObtenerToken", new Exception(detalle, wex));
                    throw;
                }
                finally
                {
                    if (response != null) response.Close();
                }
            }
        }

        private static List<ReporteWebI> ConsultarWebI(string token)
        {
            string url = ApiUrl + "/raylight/v1/documents";

            string respBody = HacerGetAutenticado(url, token);

            var carpetas = new ConcurrentDictionary<string, string>();
            var resultados = new List<ReporteWebI>();

            var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var root = js.Deserialize<Dictionary<string, object>>(respBody);

            var documents = ExtraerDocuments(root);
            if (documents == null || documents.Count == 0)
            {
                RegistrarInfo("No se encontraron documentos WebI.");
                return resultados;
            }

            RegistrarInfo("Documentos WebI encontrados: " + documents.Count);

            foreach (Dictionary<string, object> doc in documents)
            {
                string nombre = ObtenerValorStr(doc, "name");
                string cuid = ObtenerValorStr(doc, "cuid");
                string folderId = ObtenerValorStr(doc, "folderId");

                string carpeta = "WebI";
                if (!string.IsNullOrEmpty(folderId))
                {
                    carpeta = carpetas.GetOrAdd(folderId, id => ResolverNombreCarpeta(id, token));
                }

                string urlDoc = "";
                if (!string.IsNullOrEmpty(OpenDocUrl) && !string.IsNullOrEmpty(cuid))
                {
                    urlDoc = OpenDocUrl + "?iDocID=" + Uri.EscapeDataString(cuid)
                        + "&sIDType=CUID&sWindow=New";
                }

                resultados.Add(new ReporteWebI
                {
                    Nombre = nombre ?? "(sin nombre)",
                    CUID = cuid ?? "",
                    Carpeta = carpeta,
                    Descripcion = "",
                    UrlOpenDocument = urlDoc
                });
            }

            return resultados;
        }

        private static List<ReporteWebI> ConsultarCrystalReports(string token)
        {
            var resultados = new List<ReporteWebI>();
            try
            {
                string rootUrl = ApiUrl + "/infostore";
                string respBody = HacerGetAutenticado(rootUrl, token);
                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var root = js.Deserialize<Dictionary<string, object>>(respBody);
                var folders = ExtraerEntries(root);
                if (folders == null || folders.Count == 0)
                {
                    RegistrarInfo("No se encontraron carpetas raiz en infostore.");
                    return resultados;
                }

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

                    BuscarCrystalEnCarpeta(folderId, folderName ?? "Raiz", token, resultados, js, 0);
                }

                RegistrarInfo("Crystal Reports totales encontrados en servidor: " + resultados.Count);
            }
            catch (Exception ex)
            {
                RegistrarError("ConsultarCrystalReports", ex);
            }

            return resultados;
        }

        private static void BuscarCrystalEnCarpeta(string folderId, string folderName, string token,
            List<ReporteWebI> resultados, JavaScriptSerializer js, int depth)
        {
            if (depth > 7) return;

            try
            {
                string url = ApiUrl + "/infostore/" + Uri.EscapeDataString(folderId) + "/children?pageSize=200";
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

                    if (tipo == "Folder" || tipo == "User" || tipo == "PersonalCategory" || tipo == "FavoritesFolder")
                    {
                        if (!_carpetasExcluidas.Contains(nombre ?? ""))
                            BuscarCrystalEnCarpeta(entryId, nombre ?? folderName, token, resultados, js, depth + 1);
                        continue;
                    }

                    bool esCrystal = tipo != null &&
                        (tipo.IndexOf("CrystalReport", StringComparison.OrdinalIgnoreCase) >= 0
                         || tipo.IndexOf("Crystal Report", StringComparison.OrdinalIgnoreCase) >= 0
                         || tipo.IndexOf("CR4E", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!esCrystal) continue;

                    string urlDoc = "";
                    if (!string.IsNullOrEmpty(OpenDocUrl) && !string.IsNullOrEmpty(cuid))
                    {
                        urlDoc = OpenDocUrl + "?iDocID=" + Uri.EscapeDataString(cuid)
                            + "&sIDType=CUID&sType=rpt&sOutputFormat=H&sWindow=New&sRefresh=Y";
                    }

                    resultados.Add(new ReporteWebI
                    {
                        Nombre = nombre ?? "(sin nombre)",
                        CUID = cuid ?? "",
                        Carpeta = folderName,
                        Descripcion = "",
                        UrlOpenDocument = urlDoc,
                        TipoDocumento = "CrystalReport"
                    });
                }
            }
            catch (Exception ex)
            {
                RegistrarError("BuscarCrystalEnCarpeta(" + folderId + ")", ex);
            }
        }

        // ====================================================================
        // ESTADISTICAS: sesiones, licencias, servidores
        // ====================================================================
        // Estas consultas se hacen en tiempo real (sin cache) desde la pagina
        // de Estadisticas. Cada metodo maneja graceful sus errores y devuelve
        // una lista vacia + mensaje de error si el endpoint no esta disponible
        // en esta version del servidor SAP BO.
        // ====================================================================

        public class ResultadoConsulta<T>
        {
            public List<T> Items { get; set; } = new List<T>();
            public string Error { get; set; }
        }

        public static ResultadoConsulta<Models.SesionSapBo> ConsultarSesiones()
        {
            var res = new ResultadoConsulta<Models.SesionSapBo>();
            if (!Habilitado)
            {
                res.Error = "Cliente SAP BO no habilitado.";
                return res;
            }

            try
            {
                ConfigurarServicePoint();
                string token = ObtenerToken();

                // Consulta las sesiones activas via cmsquery (Connection = sesion activa)
                string body = ConsultarViaCmsQuery(
                    "SELECT SI_ID,SI_NAME,SI_USERFULLNAME,SI_AUTHEN_METHOD,SI_STARTTIME,SI_LOGON_TIME " +
                    "FROM CI_SYSTEMOBJECTS WHERE SI_KIND='Connection'",
                    token);

                if (body == null)
                {
                    // Fallback: GET a endpoints tradicionales (probablemente 404 en 4.x)
                    string[] alternos = new[]
                    {
                        ApiUrl + "/logon/sessioninfo",
                        ApiUrl + "/v1/sessions",
                        ApiUrl + "/sessions"
                    };
                    foreach (var u in alternos)
                    {
                        body = HacerGetSeguro(u, token);
                        if (body != null) break;
                    }
                }

                if (body == null)
                {
                    res.Error = "El servidor no expone endpoints para listar sesiones en esta version.";
                    return res;
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var root = js.Deserialize<Dictionary<string, object>>(body);
                var entries = ExtraerLista(root, new[] { "sessions", "session", "entries" });
                if (entries == null)
                {
                    RegistrarInfo("Sesiones: respuesta sin lista reconocible. Primeros 300: "
                        + body.Substring(0, Math.Min(300, body.Length)));
                    res.Error = "La respuesta del servidor no tiene el formato esperado.";
                    return res;
                }

                foreach (Dictionary<string, object> item in entries)
                {
                    res.Items.Add(new Models.SesionSapBo
                    {
                        Id = ObtenerValorStr(item, "SI_ID") ?? ObtenerValorStr(item, "id"),
                        Usuario = ObtenerValorStr(item, "SI_USERFULLNAME")
                                  ?? ObtenerValorStr(item, "SI_NAME")
                                  ?? ObtenerValorStr(item, "userName")
                                  ?? ObtenerValorStr(item, "user")
                                  ?? ObtenerValorStr(item, "name"),
                        TipoSesion = ObtenerValorStr(item, "SI_AUTHEN_METHOD")
                                     ?? ObtenerValorStr(item, "sessionType")
                                     ?? ObtenerValorStr(item, "type")
                                     ?? ObtenerValorStr(item, "authType"),
                        HoraInicio = ObtenerValorStr(item, "SI_LOGON_TIME")
                                     ?? ObtenerValorStr(item, "SI_STARTTIME")
                                     ?? ObtenerValorStr(item, "loginTime")
                                     ?? ObtenerValorStr(item, "creationTime")
                                     ?? ObtenerValorStr(item, "startTime")
                        });
                }

                RegistrarInfo("Sesiones activas encontradas: " + res.Items.Count);
            }
            catch (WebException wex)
            {
                res.Error = MensajeErrorWeb(wex, "sesiones");
                RegistrarError("ConsultarSesiones", wex);
            }
            catch (Exception ex)
            {
                res.Error = "No se pudo consultar sesiones: " + ex.Message;
                RegistrarError("ConsultarSesiones", ex);
            }
            return res;
        }

        public static ResultadoConsulta<Models.LicenciaSapBo> ConsultarLicencias()
        {
            var res = new ResultadoConsulta<Models.LicenciaSapBo>();
            if (!Habilitado)
            {
                res.Error = "Cliente SAP BO no habilitado.";
                return res;
            }

            try
            {
                ConfigurarServicePoint();
                string token = ObtenerToken();

                string body = ConsultarViaCmsQuery(
                    "SELECT SI_ID,SI_NAME,SI_KIND,SI_KEYCODE,SI_LICENSE_KEY," +
                    "SI_EXPIRY_DATE,SI_USER_COUNT,SI_CONCURRENT_USER_COUNT " +
                    "FROM CI_SYSTEMOBJECTS WHERE SI_KIND='LicenseKey'",
                    token);

                if (body == null)
                {
                    // Fallback: intentar endpoints alternativos
                    string[] alternos = new[]
                    {
                        ApiUrl + "/infostore/licenses",
                        ApiUrl + "/license",
                        ApiUrl + "/v1/licenses"
                    };
                    foreach (var u in alternos)
                    {
                        body = HacerGetSeguro(u, token);
                        if (body != null)
                        {
                            RegistrarInfo("Licencias: fallback -> " + u
                                + ". Primeros 400 chars: " + body.Substring(0, Math.Min(400, body.Length)));
                            break;
                        }
                    }
                }

                if (body == null)
                {
                    res.Error = "El servidor no expone endpoints estandar para licencias en esta version.";
                    return res;
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var root = js.Deserialize<Dictionary<string, object>>(body);
                var entries = ExtraerLista(root, new[] { "entries", "licenses", "license" });
                if (entries == null)
                {
                    res.Error = "La respuesta del servidor no tiene el formato esperado.";
                    return res;
                }

                foreach (Dictionary<string, object> item in entries)
                {
                    int total, uso, concurrent;
                    int.TryParse(ObtenerValorStr(item, "SI_USER_COUNT")
                        ?? ObtenerValorStr(item, "total")
                        ?? ObtenerValorStr(item, "count") ?? "0", out total);
                    int.TryParse(ObtenerValorStr(item, "used")
                        ?? ObtenerValorStr(item, "inUse") ?? "0", out uso);
                    int.TryParse(ObtenerValorStr(item, "SI_CONCURRENT_USER_COUNT") ?? "0", out concurrent);

                    string clave = ObtenerValorStr(item, "SI_KEYCODE")
                        ?? ObtenerValorStr(item, "SI_LICENSE_KEY")
                        ?? ObtenerValorStr(item, "licenseKey");
                    if (!string.IsNullOrEmpty(clave) && clave.Length > 12)
                        clave = clave.Substring(0, 4) + "****" + clave.Substring(clave.Length - 4);

                    string tipoLic = concurrent > 0 ? "Concurrent" : "Named";
                    int totalUsuarios = total > 0 ? total : concurrent;

                    res.Items.Add(new Models.LicenciaSapBo
                    {
                        Tipo = tipoLic,
                        Producto = ObtenerValorStr(item, "SI_NAME")
                            ?? ObtenerValorStr(item, "product")
                            ?? ObtenerValorStr(item, "name") ?? "SAP BO",
                        Total = totalUsuarios,
                        EnUso = uso,
                        Pico = 0,
                        Expiracion = ObtenerValorStr(item, "SI_EXPIRY_DATE") ?? ObtenerValorStr(item, "expiration"),
                        Clave = clave
                    });
                }

                RegistrarInfo("Licencias encontradas: " + res.Items.Count);
            }
            catch (WebException wex)
            {
                res.Error = MensajeErrorWeb(wex, "licencias");
                RegistrarError("ConsultarLicencias", wex);
            }
            catch (Exception ex)
            {
                res.Error = "No se pudo consultar licencias: " + ex.Message;
                RegistrarError("ConsultarLicencias", ex);
            }
            return res;
        }

        public static ResultadoConsulta<Models.ServidorSapBo> ConsultarServidores()
        {
            var res = new ResultadoConsulta<Models.ServidorSapBo>();
            if (!Habilitado)
            {
                res.Error = "Cliente SAP BO no habilitado.";
                return res;
            }

            try
            {
                ConfigurarServicePoint();
                string token = ObtenerToken();

                string body = ConsultarViaCmsQuery(
                    "SELECT SI_ID,SI_NAME,SI_KIND,SI_DISABLED,SI_SERVER_IS_ALIVE,SI_DESCRIPTION " +
                    "FROM CI_SYSTEMOBJECTS WHERE SI_KIND='Server'",
                    token);

                if (body == null)
                {
                    string[] alternos = new[]
                    {
                        ApiUrl + "/servers",
                        ApiUrl + "/infostore/servers",
                        ApiUrl + "/v1/servers"
                    };
                    foreach (var u in alternos)
                    {
                        body = HacerGetSeguro(u, token);
                        if (body != null)
                        {
                            RegistrarInfo("Servidores: fallback -> " + u
                                + ". Primeros 400 chars: " + body.Substring(0, Math.Min(400, body.Length)));
                            break;
                        }
                    }
                }

                if (body == null)
                {
                    res.Error = "El servidor no expone endpoints estandar para lista de servidores.";
                    return res;
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var root = js.Deserialize<Dictionary<string, object>>(body);
                var entries = ExtraerLista(root, new[] { "entries", "servers", "server" });
                if (entries == null)
                {
                    res.Error = "La respuesta del servidor no tiene el formato esperado.";
                    return res;
                }

                foreach (Dictionary<string, object> item in entries)
                {
                    string tipoObj = ObtenerValorStr(item, "SI_KIND") ?? ObtenerValorStr(item, "type");

                    string alive = ObtenerValorStr(item, "SI_SERVER_IS_ALIVE")
                                   ?? ObtenerValorStr(item, "state")
                                   ?? ObtenerValorStr(item, "status");
                    string disabled = ObtenerValorStr(item, "SI_DISABLED");

                    string estado;
                    if (string.Equals(disabled, "True", StringComparison.OrdinalIgnoreCase) || disabled == "1")
                        estado = "Disabled";
                    else if (alive == "1" || string.Equals(alive, "true", StringComparison.OrdinalIgnoreCase))
                        estado = "Running";
                    else if (alive == "0" || string.Equals(alive, "false", StringComparison.OrdinalIgnoreCase))
                        estado = "Stopped";
                    else
                        estado = alive ?? "Desconocido";

                    string nombre = ObtenerValorStr(item, "SI_NAME") ?? ObtenerValorStr(item, "name");
                    // Extraer tipo desde el nombre del servidor (SAPBO.EventServer -> EventServer)
                    string tipoServicio = tipoObj;
                    if (!string.IsNullOrEmpty(nombre) && nombre.Contains("."))
                    {
                        var partes = nombre.Split('.');
                        if (partes.Length >= 2)
                            tipoServicio = partes[partes.Length - 1];
                    }

                    res.Items.Add(new Models.ServidorSapBo
                    {
                        Id = ObtenerValorStr(item, "SI_ID") ?? ObtenerValorStr(item, "id"),
                        Nombre = nombre,
                        Tipo = tipoServicio,
                        Estado = estado,
                        Descripcion = ObtenerValorStr(item, "SI_DESCRIPTION") ?? ObtenerValorStr(item, "description")
                    });
                }

                RegistrarInfo("Servidores encontrados: " + res.Items.Count);
            }
            catch (WebException wex)
            {
                res.Error = MensajeErrorWeb(wex, "servidores");
                RegistrarError("ConsultarServidores", wex);
            }
            catch (Exception ex)
            {
                res.Error = "No se pudo consultar servidores: " + ex.Message;
                RegistrarError("ConsultarServidores", ex);
            }
            return res;
        }

        // Wrapper para /v1/cmsquery: ejecuta una sentencia SQL contra el CMS.
        // El endpoint recibe la query en un body con estructura attrs/attr y
        // devuelve JSON con entries. Devuelve null si el endpoint o el permiso
        // no aplican en este servidor.
        private static string ConsultarViaCmsQuery(string queryTexto, string token)
        {
            string url = ApiUrl + "/v1/cmsquery";
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
                + "<attrs xmlns=\"http://www.sap.com/rws/bip\">"
                + "<attr name=\"query\" type=\"string\">" + EscapeXml(queryTexto) + "</attr>"
                + "</attrs>";
            string resp = HacerPostSeguro(url, token, xml, "application/xml");
            if (resp == null) return null;

            RegistrarInfo("cmsquery OK. Primeros 500 chars: "
                + resp.Substring(0, Math.Min(500, resp.Length)));
            return resp;
        }

        // GET que devuelve null si el endpoint no existe (404) o no esta permitido
        // (403). Cualquier otro error se propaga como excepcion normal.
        private static string HacerGetSeguro(string url, string token)
        {
            try
            {
                return HacerGetAutenticado(url, token);
            }
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

        // POST autenticado que devuelve null si el endpoint no existe. Igual que
        // HacerGetSeguro pero para operaciones POST (ej. cmsquery).
        private static string HacerPostSeguro(string url, string token,
            string body, string contentType)
        {
            try
            {
                return HacerPostAutenticado(url, token, body, contentType);
            }
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

        private static string HacerPostAutenticado(string url, string token,
            string body, string contentType)
        {
            var request = CrearRequest(url, "POST");
            request.Accept = "application/json";
            request.ContentType = contentType ?? "application/json";
            request.Headers.Add("X-SAP-LogonToken", "\"" + token + "\"");

            byte[] data = Encoding.UTF8.GetBytes(body ?? "");
            request.ContentLength = data.Length;
            using (var s = request.GetRequestStream())
            {
                s.Write(data, 0, data.Length);
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex) when (wex.Response != null)
            {
                int statusCode;
                using (var errResp = (HttpWebResponse)wex.Response)
                {
                    statusCode = (int)errResp.StatusCode;
                    string errBody = "";
                    using (var sr = new StreamReader(errResp.GetResponseStream(), Encoding.UTF8))
                    {
                        errBody = sr.ReadToEnd();
                    }
                    RegistrarError("POST " + url,
                        new Exception("HTTP " + statusCode + " Body: "
                        + errBody.Substring(0, Math.Min(500, errBody.Length)), wex));
                }
                wex.Data["HttpStatusCode"] = statusCode;
                throw;
            }

            using (response)
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string MensajeErrorWeb(WebException wex, string contexto)
        {
            var resp = wex.Response as HttpWebResponse;
            if (resp != null)
            {
                int code = (int)resp.StatusCode;
                if (code == 403 || code == 401)
                    return "El usuario configurado no tiene permisos para consultar " + contexto + " (HTTP " + code + ").";
                return "Error del servidor al consultar " + contexto + " (HTTP " + code + ").";
            }
            return "No se pudo comunicar con el servidor para consultar " + contexto + ".";
        }

        private static System.Collections.ArrayList ExtraerLista(
            Dictionary<string, object> root, string[] posiblesLlaves)
        {
            if (root == null) return null;

            foreach (var llave in posiblesLlaves)
            {
                object val;
                if (root.TryGetValue(llave, out val))
                {
                    if (val is System.Collections.ArrayList)
                        return (System.Collections.ArrayList)val;

                    var sub = val as Dictionary<string, object>;
                    if (sub != null)
                    {
                        // buscar array dentro
                        foreach (var kvp in sub)
                        {
                            if (kvp.Value is System.Collections.ArrayList)
                                return (System.Collections.ArrayList)kvp.Value;
                        }
                    }
                }
            }

            // fallback: buscar recursivamente cualquier ArrayList
            foreach (var kvp in root)
            {
                if (kvp.Value is System.Collections.ArrayList)
                    return (System.Collections.ArrayList)kvp.Value;
                var sub = kvp.Value as Dictionary<string, object>;
                if (sub != null)
                {
                    var inner = ExtraerLista(sub, posiblesLlaves);
                    if (inner != null) return inner;
                }
            }

            return null;
        }

        private static string HacerGetAutenticado(string url, string token)
        {
            const int maxReintentos = 2;
            for (int intento = 0; ; intento++)
            {
                var request = CrearRequest(url, "GET");
                request.Accept = "application/json";
                request.Headers.Add("X-SAP-LogonToken", "\"" + token + "\"");

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException wex) when (wex.Response != null)
                {
                    int statusCode;
                    using (var errResp = (HttpWebResponse)wex.Response)
                    {
                        statusCode = (int)errResp.StatusCode;
                        string errBody = "";
                        using (var sr = new StreamReader(errResp.GetResponseStream(), Encoding.UTF8))
                        {
                            errBody = sr.ReadToEnd();
                        }
                        RegistrarError("GET " + url,
                            new Exception("HTTP " + statusCode + " Body: "
                            + errBody.Substring(0, Math.Min(500, errBody.Length)), wex));
                    }
                    // Guarda el status en Data para que quien atrape la excepcion
                    // pueda leerlo aunque wex.Response ya este disposed.
                    wex.Data["HttpStatusCode"] = statusCode;
                    throw;
                }
                catch (WebException wex) when (intento < maxReintentos &&
                    (wex.Status == WebExceptionStatus.Timeout
                     || wex.Status == WebExceptionStatus.NameResolutionFailure
                     || wex.Status == WebExceptionStatus.ConnectFailure))
                {
                    Thread.Sleep(1000 * (intento + 1));
                    continue;
                }

                using (response)
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string ResolverNombreCarpeta(string folderId, string token)
        {
            try
            {
                string url = ApiUrl + "/infostore/" + Uri.EscapeDataString(folderId);
                string body = HacerGetAutenticado(url, token);
                var js = new JavaScriptSerializer();
                var data = js.Deserialize<Dictionary<string, object>>(body);
                string name = ObtenerValorStr(data, "name");
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch (Exception ex)
            {
                RegistrarError("ResolverNombreCarpeta(" + folderId + ")", ex);
            }
            return "WebI - Carpeta " + folderId;
        }

        private static System.Collections.ArrayList ExtraerDocuments(Dictionary<string, object> root)
        {
            if (root == null) return null;

            object docsObj;
            if (root.TryGetValue("documents", out docsObj))
            {
                var docsDict = docsObj as Dictionary<string, object>;
                if (docsDict != null)
                {
                    object docArray;
                    if (docsDict.TryGetValue("document", out docArray) && docArray is System.Collections.ArrayList)
                        return (System.Collections.ArrayList)docArray;
                }
            }
            return null;
        }

        private static System.Collections.ArrayList ExtraerEntries(Dictionary<string, object> root)
        {
            if (root == null) return null;

            object entriesObj;
            if (root.TryGetValue("entries", out entriesObj) && entriesObj is System.Collections.ArrayList)
                return (System.Collections.ArrayList)entriesObj;

            foreach (var kvp in root)
            {
                var sub = kvp.Value as Dictionary<string, object>;
                if (sub != null)
                {
                    var inner = ExtraerEntries(sub);
                    if (inner != null) return inner;
                }
            }

            return null;
        }

        private static string ObtenerValorStr(Dictionary<string, object> dict, string key)
        {
            object val;
            if (dict.TryGetValue(key, out val) && val != null)
                return val.ToString();
            return null;
        }

        private static HttpWebRequest CrearRequest(string url, string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = 30000;
            request.ReadWriteTimeout = 30000;
            request.UserAgent = "PortalReportesCrystal/1.0";
            return request;
        }

        private static void InvalidarToken()
        {
            lock (_lockToken)
            {
                _token = null;
                _tokenExpira = DateTime.MinValue;
            }
        }

        private static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string ExtraerValorXml(string xml, string tag)
        {
            string open = "<" + tag + ">";
            string close = "</" + tag + ">";
            int i = xml.IndexOf(open, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
            {
                open = "\"" + tag + "\"";
                i = xml.IndexOf(open, StringComparison.OrdinalIgnoreCase);
                if (i < 0) return null;
                i = xml.IndexOf('>', i);
                if (i < 0) return null;
                i++;
                int j2 = xml.IndexOf('<', i);
                return j2 > i ? xml.Substring(i, j2 - i).Trim() : null;
            }
            i += open.Length;
            int j = xml.IndexOf(close, i, StringComparison.OrdinalIgnoreCase);
            if (j < 0) return null;
            return xml.Substring(i, j - i).Trim();
        }

        private static void RegistrarInfo(string mensaje)
        {
            if (string.IsNullOrEmpty(_logPath)) return;
            try
            {
                string linea = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [INFO] {1}{2}",
                    DateTime.Now, mensaje, Environment.NewLine);
                File.AppendAllText(_logPath, linea, Encoding.UTF8);
            }
            catch { }
        }

        private static void RegistrarError(string contexto, Exception ex)
        {
            if (string.IsNullOrEmpty(_logPath)) return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendFormat("[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}: {3}",
                    DateTime.Now, contexto, ex.GetType().Name, ex.Message);
                sb.AppendLine();
                if (ex.InnerException != null)
                {
                    sb.AppendFormat("  Inner: {0}: {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    sb.AppendLine();
                }
                File.AppendAllText(_logPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }
    }
}
