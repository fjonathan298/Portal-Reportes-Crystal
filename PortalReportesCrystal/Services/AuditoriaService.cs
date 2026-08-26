// ============================================================================
// AuditoriaService.cs - SERVICIO DE AUDITORIA
// ============================================================================
// Registra eventos de usuario (login, ver reporte, exportar, filtros, errores)
// en la base de datos audit del servidor Perseo/DWH_FRAMEWORK.
//
// Caracteristicas clave:
//   - Thread-safe: usa ConcurrentQueue
//   - No bloqueante: RegistrarEvento hace enqueue y regresa inmediatamente
//   - Por lotes: un timer vuelca la cola cada N segundos a la BD
//   - Fallback graceful: si la BD esta caida, persiste en App_Data\audit_pending.jsonl
//     y un ciclo posterior lo reintenta
//   - El portal NUNCA falla por auditoria
//
// Configuracion (Web.config appSettings):
//   Audit:Habilitado          = "true" para activar
//   Audit:ConnectionString    = "Server=Perseo;Database=DWH_FRAMEWORK;
//                                Integrated Security=SSPI;..."
//                                (Windows Authentication - Sin usuario/password)
//   Audit:IntervaloFlushSeg   = "5" (default)
//   Audit:MaxLoteInsert       = "500" (default)
//   Audit:GruposAdmin         = "SUPERREPUESTOS\Portal_Audit_Readers"
//
// Autenticacion a SQL Server:
//   El portal se conecta con Integrated Security (Windows Auth). La cuenta
//   que corre el Application Pool de IIS es la que se autentica contra
//   Perseo. En produccion se recomienda:
//     - Identity del AppPool = cuenta de dominio dedicada (idealmente gMSA)
//     - Esa cuenta miembro del grupo AD  Portal_Audit_Writers
//     - El grupo AD con permisos INSERT/SELECT en audit.* (ver audit_schema.sql)
//
// Uso:
//   AuditoriaService.Inicializar(Server.MapPath("~/App_Data"));   // en Application_Start
//   AuditoriaService.RegistrarEvento(new EventoAuditoria { ... });
//
// Nota de seguridad (org instructions):
//   - Nunca se registra contenido de reportes ni credenciales
//   - Sin passwords en Web.config (Integrated Security elimina ese vector)
//   - Los grupos AD ejercen mínimo privilegio (solo INSERT/SELECT en audit.*)
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

namespace PortalReportesCrystal.Services
{
    // Datos que se registran por evento.
    // Solo Usuario y TipoEvento son obligatorios; el resto es opcional segun el contexto.
    public class EventoAuditoria
    {
        public Guid? SesionId { get; set; }
        public string TipoEvento { get; set; }            // Codigo de audit.EventoTipo (VER_REPORTE, EXPORTAR_PDF, ...)
        public string Usuario { get; set; }
        public string IpCliente { get; set; }
        public string RaizId { get; set; }
        public string PathReporte { get; set; }
        public string NombreReporte { get; set; }
        public string Categoria { get; set; }
        public string TipoReporte { get; set; }           // Local / WebI / Sapbo / Externo
        public string Servidor { get; set; }
        public string Formato { get; set; }               // pdf / excel / exceldata / ...
        public int? DuracionMs { get; set; }
        public long? TamanioBytes { get; set; }
        public int? HttpStatus { get; set; }
        public string UrlOrigen { get; set; }
        public string MensajeError { get; set; }
        public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

        // Parametros aplicados al reporte (ALMACEN=06, PAIS=SV, FECHA_DESDE=2026-01-01)
        public Dictionary<string, string> Parametros { get; set; }
    }

    public static class AuditoriaService
    {
        // -------------------- Configuracion --------------------

        private static bool _habilitado;
        private static string _connectionString;
        private static int _intervaloFlushSeg = 5;
        private static int _maxLoteInsert = 500;
        private static string _appDataPath;
        private static string _pendingJsonlPath;

        // -------------------- Estado runtime --------------------

        private static readonly ConcurrentQueue<EventoAuditoria> _cola = new ConcurrentQueue<EventoAuditoria>();
        private static Timer _timerFlush;
        private static readonly object _flushLock = new object();
        private static bool _inicializado;

        // Codigo tipo -> id resuelto en cada flush (cache en memoria una vez cargado)
        private static Dictionary<string, byte> _tiposCache;
        private static readonly object _tiposCacheLock = new object();

        public static bool Habilitado
        {
            get { return _habilitado; }
        }

        // -------------------- Inicializacion --------------------

        public static void Inicializar(string appDataPath)
        {
            if (_inicializado) return;
            _inicializado = true;

            _appDataPath = appDataPath;
            _pendingJsonlPath = Path.Combine(appDataPath, "audit_pending.jsonl");

            _habilitado = string.Equals(
                ConfigurationManager.AppSettings["Audit:Habilitado"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (!_habilitado)
                return;

            _connectionString = ConfigurationManager.AppSettings["Audit:ConnectionString"] ?? "";
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _habilitado = false;
                RegistrarInternoError("Inicializar",
                    new InvalidOperationException("Audit:ConnectionString vacio o ausente en Web.config."));
                return;
            }

            int intervalo;
            if (int.TryParse(ConfigurationManager.AppSettings["Audit:IntervaloFlushSeg"], out intervalo) && intervalo > 0)
                _intervaloFlushSeg = intervalo;

            int lote;
            if (int.TryParse(ConfigurationManager.AppSettings["Audit:MaxLoteInsert"], out lote) && lote > 0)
                _maxLoteInsert = lote;

            _timerFlush = new Timer(_ => FlushSeguro(),
                null,
                TimeSpan.FromSeconds(_intervaloFlushSeg),
                TimeSpan.FromSeconds(_intervaloFlushSeg));

            RegistrarInternoInfo("AuditoriaService inicializado. Flush cada " + _intervaloFlushSeg + "s.");
        }

        // -------------------- API publica --------------------

        // Enqueue no bloqueante. Se puede llamar desde cualquier hilo.
        public static void RegistrarEvento(EventoAuditoria evento)
        {
            if (!_habilitado || evento == null) return;
            try
            {
                evento.Usuario = string.IsNullOrWhiteSpace(evento.Usuario)
                    ? "(anonimo)"
                    : evento.Usuario;
                evento.FechaUtc = evento.FechaUtc == default(DateTime)
                    ? DateTime.UtcNow
                    : evento.FechaUtc;

                _cola.Enqueue(evento);
            }
            catch (Exception ex)
            {
                // Nunca reventar la peticion HTTP por un fallo de auditoria
                RegistrarInternoError("RegistrarEvento", ex);
            }
        }

        // Obtiene o crea una sesion "logica" del portal para el HttpContext actual.
        // La sesion se guarda en la Session ASP.NET; si no hay session (state=Off) se
        // genera un GUID por request (menos correlacion pero funciona).
        public static Guid ObtenerSesionActual(HttpContext ctx)
        {
            if (ctx == null || ctx.Session == null)
                return Guid.NewGuid();

            const string key = "__portal_audit_sesion_id";
            object val = ctx.Session[key];
            if (val is Guid) return (Guid)val;

            Guid nueva = Guid.NewGuid();
            ctx.Session[key] = nueva;

            // Registrar la sesion (LOGIN) en la BD tambien
            try
            {
                string usuario = ctx.User != null && ctx.User.Identity != null
                    ? ctx.User.Identity.Name
                    : "(anonimo)";
                string ip = ObtenerIpCliente(ctx);
                string ua = ctx.Request != null ? (ctx.Request.UserAgent ?? "") : "";

                InsertarSesion(nueva, usuario, ip, ua);

                RegistrarEvento(new EventoAuditoria
                {
                    SesionId = nueva,
                    TipoEvento = "LOGIN",
                    Usuario = usuario,
                    IpCliente = ip
                });
            }
            catch (Exception ex)
            {
                RegistrarInternoError("ObtenerSesionActual", ex);
            }

            return nueva;
        }

        // Actualiza UltimaActividadUtc de la sesion (barato, corre en el flush si es necesario).
        public static void MarcarActividad(Guid sesionId)
        {
            // Se resuelve durante el flush con un UPDATE agrupado; no bloqueante aqui.
            _cola.Enqueue(new EventoAuditoria
            {
                SesionId = sesionId,
                TipoEvento = "HEARTBEAT",
                Usuario = "(sistema)",
                FechaUtc = DateTime.UtcNow
            });
        }

        public static string ObtenerIpCliente(HttpContext ctx)
        {
            if (ctx == null || ctx.Request == null) return null;
            // Preferir X-Forwarded-For si viene atras de un proxy interno; si no, UserHostAddress.
            string xff = ctx.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(xff))
            {
                int coma = xff.IndexOf(',');
                return coma > 0 ? xff.Substring(0, coma).Trim() : xff.Trim();
            }
            return ctx.Request.UserHostAddress;
        }

        // -------------------- Flush --------------------

        private static void FlushSeguro()
        {
            if (!Monitor.TryEnter(_flushLock)) return;  // ya hay un flush corriendo
            try
            {
                // 1. Vaciar cola en memoria
                var lote = new List<EventoAuditoria>(_maxLoteInsert);
                EventoAuditoria e;
                while (lote.Count < _maxLoteInsert && _cola.TryDequeue(out e))
                    lote.Add(e);

                if (lote.Count == 0)
                {
                    // Aprovechar el flush para reintentar pendientes en disco
                    ReintentarPendientes();
                    return;
                }

                try
                {
                    InsertarLote(lote);
                }
                catch (Exception ex)
                {
                    // Fallback: persistir en JSONL para reintentar mas tarde
                    RegistrarInternoError("Flush.InsertarLote", ex);
                    PersistirPendientes(lote);
                }

                // Con la BD viva, aprovechar el ciclo para vaciar pendientes acumulados
                ReintentarPendientes();
            }
            catch (Exception ex)
            {
                RegistrarInternoError("FlushSeguro", ex);
            }
            finally
            {
                Monitor.Exit(_flushLock);
            }
        }

        // -------------------- Insercion en BD --------------------

        private static void InsertarSesion(Guid sesionId, string usuario, string ip, string userAgent)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM audit.Sesion WHERE SesionId = @SesionId)
BEGIN
    INSERT INTO audit.Sesion (SesionId, Usuario, IpCliente, UserAgent, InicioUtc, UltimaActividadUtc)
    VALUES (@SesionId, @Usuario, @Ip, @UserAgent, SYSUTCDATETIME(), SYSUTCDATETIME());
END";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@SesionId", SqlDbType.UniqueIdentifier).Value = sesionId;
                cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 200).Value = (object)usuario ?? DBNull.Value;
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 45).Value = (object)(ip ?? "") ?? DBNull.Value;
                cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = (object)(userAgent ?? "") ?? DBNull.Value;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarLote(List<EventoAuditoria> lote)
        {
            var tipos = ObtenerTiposCache();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                foreach (var e in lote)
                {
                    byte tipoId;
                    if (!tipos.TryGetValue(e.TipoEvento ?? "", out tipoId))
                    {
                        RegistrarInternoInfo("Tipo evento desconocido, se omite: " + e.TipoEvento);
                        continue;
                    }

                    using (var tx = cn.BeginTransaction())
                    {
                        try
                        {
                            long eventoId;

                            using (var cmd = new SqlCommand(@"
INSERT INTO audit.Evento
    (SesionId, TipoEventoId, FechaUtc, Usuario, IpCliente,
     RaizId, PathReporte, NombreReporte, Categoria, TipoReporte,
     Servidor, Formato, DuracionMs, TamanioBytes, HttpStatus, UrlOrigen, MensajeError)
OUTPUT INSERTED.EventoId
VALUES
    (@SesionId, @TipoEventoId, @FechaUtc, @Usuario, @IpCliente,
     @RaizId, @PathReporte, @NombreReporte, @Categoria, @TipoReporte,
     @Servidor, @Formato, @DuracionMs, @TamanioBytes, @HttpStatus, @UrlOrigen, @MensajeError);", cn, tx))
                            {
                                cmd.Parameters.Add("@SesionId", SqlDbType.UniqueIdentifier).Value =
                                    e.SesionId.HasValue ? (object)e.SesionId.Value : DBNull.Value;
                                cmd.Parameters.Add("@TipoEventoId", SqlDbType.TinyInt).Value = tipoId;
                                cmd.Parameters.Add("@FechaUtc", SqlDbType.DateTime2).Value = e.FechaUtc;
                                cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 200).Value = (object)e.Usuario ?? DBNull.Value;
                                cmd.Parameters.Add("@IpCliente", SqlDbType.VarChar, 45).Value = (object)e.IpCliente ?? DBNull.Value;
                                cmd.Parameters.Add("@RaizId", SqlDbType.VarChar, 60).Value = (object)e.RaizId ?? DBNull.Value;
                                cmd.Parameters.Add("@PathReporte", SqlDbType.NVarChar, 600).Value = (object)e.PathReporte ?? DBNull.Value;
                                cmd.Parameters.Add("@NombreReporte", SqlDbType.NVarChar, 400).Value = (object)e.NombreReporte ?? DBNull.Value;
                                cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 200).Value = (object)e.Categoria ?? DBNull.Value;
                                cmd.Parameters.Add("@TipoReporte", SqlDbType.VarChar, 20).Value = (object)e.TipoReporte ?? DBNull.Value;
                                cmd.Parameters.Add("@Servidor", SqlDbType.NVarChar, 100).Value = (object)e.Servidor ?? DBNull.Value;
                                cmd.Parameters.Add("@Formato", SqlDbType.VarChar, 20).Value = (object)e.Formato ?? DBNull.Value;
                                cmd.Parameters.Add("@DuracionMs", SqlDbType.Int).Value = e.DuracionMs.HasValue ? (object)e.DuracionMs.Value : DBNull.Value;
                                cmd.Parameters.Add("@TamanioBytes", SqlDbType.BigInt).Value = e.TamanioBytes.HasValue ? (object)e.TamanioBytes.Value : DBNull.Value;
                                cmd.Parameters.Add("@HttpStatus", SqlDbType.SmallInt).Value = e.HttpStatus.HasValue ? (object)e.HttpStatus.Value : DBNull.Value;
                                cmd.Parameters.Add("@UrlOrigen", SqlDbType.NVarChar, 600).Value = (object)e.UrlOrigen ?? DBNull.Value;
                                cmd.Parameters.Add("@MensajeError", SqlDbType.NVarChar, 1000).Value = (object)e.MensajeError ?? DBNull.Value;

                                eventoId = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            if (e.Parametros != null && e.Parametros.Count > 0)
                            {
                                foreach (var kvp in e.Parametros)
                                {
                                    if (string.IsNullOrWhiteSpace(kvp.Key)) continue;
                                    using (var cmdP = new SqlCommand(@"
INSERT INTO audit.EventoParametro (EventoId, NombreParametro, ValorParametro)
VALUES (@EventoId, @Nombre, @Valor);", cn, tx))
                                    {
                                        cmdP.Parameters.Add("@EventoId", SqlDbType.BigInt).Value = eventoId;
                                        cmdP.Parameters.Add("@Nombre", SqlDbType.VarChar, 60).Value = kvp.Key;
                                        cmdP.Parameters.Add("@Valor", SqlDbType.NVarChar, 400).Value = kvp.Value ?? "";
                                        cmdP.ExecuteNonQuery();
                                    }
                                }
                            }

                            // Actualizar UltimaActividadUtc de la sesion si es un evento con sesion
                            if (e.SesionId.HasValue)
                            {
                                using (var cmdS = new SqlCommand(@"
UPDATE audit.Sesion SET UltimaActividadUtc = SYSUTCDATETIME() WHERE SesionId = @SesionId;", cn, tx))
                                {
                                    cmdS.Parameters.Add("@SesionId", SqlDbType.UniqueIdentifier).Value = e.SesionId.Value;
                                    cmdS.ExecuteNonQuery();
                                }
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }
            }
        }

        private static Dictionary<string, byte> ObtenerTiposCache()
        {
            lock (_tiposCacheLock)
            {
                if (_tiposCache != null) return _tiposCache;

                var dict = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                using (var cn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT TipoEventoId, Codigo FROM audit.EventoTipo WHERE Activo = 1", cn))
                {
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                            dict[rd.GetString(1)] = (byte)rd.GetByte(0);
                    }
                }
                _tiposCache = dict;
                return dict;
            }
        }

        // -------------------- Fallback JSONL --------------------

        private static void PersistirPendientes(List<EventoAuditoria> lote)
        {
            if (string.IsNullOrEmpty(_pendingJsonlPath)) return;
            try
            {
                var js = new JavaScriptSerializer();
                var sb = new StringBuilder(lote.Count * 200);
                foreach (var e in lote)
                    sb.AppendLine(js.Serialize(e));
                File.AppendAllText(_pendingJsonlPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                RegistrarInternoError("PersistirPendientes", ex);
            }
        }

        private static void ReintentarPendientes()
        {
            if (string.IsNullOrEmpty(_pendingJsonlPath) || !File.Exists(_pendingJsonlPath))
                return;

            string tmp = _pendingJsonlPath + ".processing";
            try
            {
                // Renombrar para trabajar sobre snapshot atomico
                File.Move(_pendingJsonlPath, tmp);
            }
            catch
            {
                return;
            }

            try
            {
                var js = new JavaScriptSerializer();
                var lineas = File.ReadAllLines(tmp, Encoding.UTF8);
                var lote = new List<EventoAuditoria>();
                foreach (var linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    try
                    {
                        var e = js.Deserialize<EventoAuditoria>(linea);
                        if (e != null) lote.Add(e);
                    }
                    catch (Exception ex)
                    {
                        RegistrarInternoError("ReintentarPendientes.Parse", ex);
                    }
                }

                if (lote.Count > 0)
                    InsertarLote(lote);

                File.Delete(tmp);
            }
            catch (Exception ex)
            {
                // No pudimos procesar; devolver el archivo a su ubicacion original
                RegistrarInternoError("ReintentarPendientes", ex);
                try
                {
                    if (File.Exists(tmp) && !File.Exists(_pendingJsonlPath))
                        File.Move(tmp, _pendingJsonlPath);
                }
                catch { }
            }
        }

        // -------------------- Logging interno --------------------

        private static void RegistrarInternoInfo(string mensaje)
        {
            EscribirLogInterno("[INFO] " + mensaje);
        }

        private static void RegistrarInternoError(string contexto, Exception ex)
        {
            EscribirLogInterno("[ERROR] " + contexto + ": " + ex.GetType().Name + " " + ex.Message);
        }

        private static void EscribirLogInterno(string mensaje)
        {
            if (string.IsNullOrEmpty(_appDataPath)) return;
            try
            {
                string ruta = Path.Combine(_appDataPath, "auditoria.log");
                string linea = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}",
                    DateTime.Now, mensaje, Environment.NewLine);
                File.AppendAllText(ruta, linea, Encoding.UTF8);
            }
            catch { }
        }
    }
}
