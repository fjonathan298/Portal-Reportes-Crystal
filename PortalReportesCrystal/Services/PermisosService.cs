using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using PortalReportesCrystal.Models;

namespace PortalReportesCrystal.Services
{
    public static class PermisosService
    {
        private static bool _habilitado;
        private static string _connectionString;
        private static bool _inicializado;

        private static Timer _timerRecarga;
        private static readonly object _cargaLock = new object();

        private static List<RolInfo> _roles = new List<RolInfo>();
        private static List<RolReporteInfo> _asignaciones = new List<RolReporteInfo>();
        private static List<UsuarioRolInfo> _usuarioRoles = new List<UsuarioRolInfo>();

        public static bool Habilitado { get { return _habilitado; } }

        public static void Inicializar()
        {
            if (_inicializado) return;
            _inicializado = true;

            _habilitado = string.Equals(
                ConfigurationManager.AppSettings["Permisos:Habilitado"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (!_habilitado) return;

            _connectionString = ConfigurationManager.AppSettings["Audit:ConnectionString"] ?? "";
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _habilitado = false;
                return;
            }

            RecargarCache();

            _timerRecarga = new Timer(_ => RecargarCacheSeguro(),
                null,
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(60));
        }

        public static bool TieneAcceso(HttpContextBase httpContext, ReporteInfo reporte)
        {
            if (!_habilitado) return true;
            if (_roles.Count == 0) return true;

            string usuario = httpContext.User != null && httpContext.User.Identity != null
                ? httpContext.User.Identity.Name
                : null;
            if (string.IsNullOrEmpty(usuario)) return false;

            var rolesUsuario = ObtenerRolesUsuario(httpContext, usuario);
            if (rolesUsuario.Count == 0) return false;

            foreach (var rolId in rolesUsuario)
            {
                var asigs = _asignaciones.Where(a => a.RolId == rolId && a.Activo && a.PuedeVer).ToList();
                foreach (var asig in asigs)
                {
                    if (CoincideAcceso(asig, reporte))
                        return true;
                }
            }
            return false;
        }

        public static bool TienePermisoExportar(HttpContextBase httpContext, ReporteInfo reporte)
        {
            if (!_habilitado) return true;
            if (_roles.Count == 0) return true;

            string usuario = httpContext.User != null && httpContext.User.Identity != null
                ? httpContext.User.Identity.Name
                : null;
            if (string.IsNullOrEmpty(usuario)) return false;

            var rolesUsuario = ObtenerRolesUsuario(httpContext, usuario);
            foreach (var rolId in rolesUsuario)
            {
                var asigs = _asignaciones.Where(a => a.RolId == rolId && a.Activo && a.PuedeExportar).ToList();
                foreach (var asig in asigs)
                {
                    if (CoincideAcceso(asig, reporte))
                        return true;
                }
            }
            return false;
        }

        private static List<int> ObtenerRolesUsuario(HttpContextBase httpContext, string usuario)
        {
            var result = new List<int>();

            foreach (var ur in _usuarioRoles)
            {
                if (string.Equals(ur.Usuario, usuario, StringComparison.OrdinalIgnoreCase))
                    result.Add(ur.RolId);
            }

            foreach (var rol in _roles)
            {
                if (!rol.Activo || string.IsNullOrEmpty(rol.GrupoAD)) continue;
                if (result.Contains(rol.RolId)) continue;

                try
                {
                    if (httpContext.User.IsInRole(rol.GrupoAD))
                        result.Add(rol.RolId);
                }
                catch { }
            }

            return result;
        }

        private static bool CoincideAcceso(RolReporteInfo asig, ReporteInfo reporte)
        {
            switch (asig.TipoAcceso)
            {
                case "RAIZ":
                    return string.Equals(asig.ValorAcceso, reporte.RaizId, StringComparison.OrdinalIgnoreCase);

                case "CATEGORIA":
                    return string.Equals(asig.ValorAcceso, reporte.Categoria, StringComparison.OrdinalIgnoreCase);

                case "REPORTE":
                    if (!string.IsNullOrEmpty(reporte.PathRelativo))
                        return string.Equals(asig.ValorAcceso, reporte.PathRelativo, StringComparison.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(reporte.Nombre))
                        return string.Equals(asig.ValorAcceso, reporte.Nombre, StringComparison.OrdinalIgnoreCase);
                    return false;

                default:
                    return false;
            }
        }

        private static void RecargarCacheSeguro()
        {
            try { RecargarCache(); }
            catch { }
        }

        private static void RecargarCache()
        {
            lock (_cargaLock)
            {
                try
                {
                    var roles = new List<RolInfo>();
                    var asignaciones = new List<RolReporteInfo>();
                    var usuarioRoles = new List<UsuarioRolInfo>();

                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();

                        using (var cmd = new SqlCommand("SELECT RolId, Nombre, GrupoAD, Activo FROM audit.Rol WHERE Activo = 1", conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                roles.Add(new RolInfo
                                {
                                    RolId = r.GetInt32(0),
                                    Nombre = r.GetString(1),
                                    GrupoAD = r.IsDBNull(2) ? null : r.GetString(2),
                                    Activo = r.GetBoolean(3)
                                });
                            }
                        }

                        using (var cmd = new SqlCommand("SELECT RolReporteId, RolId, TipoAcceso, ValorAcceso, PuedeVer, PuedeExportar, Activo FROM audit.RolReporte WHERE Activo = 1", conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                asignaciones.Add(new RolReporteInfo
                                {
                                    RolReporteId = r.GetInt32(0),
                                    RolId = r.GetInt32(1),
                                    TipoAcceso = r.GetString(2),
                                    ValorAcceso = r.GetString(3),
                                    PuedeVer = r.GetBoolean(4),
                                    PuedeExportar = r.GetBoolean(5),
                                    Activo = r.GetBoolean(6)
                                });
                            }
                        }

                        using (var cmd = new SqlCommand("SELECT UsuarioRolId, Usuario, RolId FROM audit.UsuarioRol WHERE Activo = 1", conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                usuarioRoles.Add(new UsuarioRolInfo
                                {
                                    UsuarioRolId = r.GetInt32(0),
                                    Usuario = r.GetString(1),
                                    RolId = r.GetInt32(2)
                                });
                            }
                        }
                    }

                    _roles = roles;
                    _asignaciones = asignaciones;
                    _usuarioRoles = usuarioRoles;
                }
                catch
                {
                    // Si falla la carga, el portal sigue con el cache anterior
                }
            }
        }

        // --- CRUD (para PermisosController) ---

        public static int CrearRol(string nombre, string descripcion, string grupoAD, string creadoPor)
        {
            int rolId;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO audit.Rol (Nombre, Descripcion, GrupoAD, CreadoPor)
                    OUTPUT INSERTED.RolId
                    VALUES (@Nombre, @Descripcion, @GrupoAD, @CreadoPor)", conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrupoAD", (object)grupoAD ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreadoPor", creadoPor);
                    rolId = (int)cmd.ExecuteScalar();
                }

                RegistrarLog(conn, "ROL_CREADO", creadoPor,
                    "Rol creado: " + nombre + (string.IsNullOrEmpty(grupoAD) ? "" : " (AD: " + grupoAD + ")"),
                    rolId, null);
            }
            RecargarCache();
            return rolId;
        }

        public static void AsignarReporte(int rolId, string tipoAcceso, string valorAcceso,
            bool puedeVer, bool puedeExportar, string asignadoPor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO audit.RolReporte (RolId, TipoAcceso, ValorAcceso, PuedeVer, PuedeExportar, AsignadoPor)
                    VALUES (@RolId, @TipoAcceso, @ValorAcceso, @PuedeVer, @PuedeExportar, @AsignadoPor)", conn))
                {
                    cmd.Parameters.AddWithValue("@RolId", rolId);
                    cmd.Parameters.AddWithValue("@TipoAcceso", tipoAcceso);
                    cmd.Parameters.AddWithValue("@ValorAcceso", valorAcceso);
                    cmd.Parameters.AddWithValue("@PuedeVer", puedeVer);
                    cmd.Parameters.AddWithValue("@PuedeExportar", puedeExportar);
                    cmd.Parameters.AddWithValue("@AsignadoPor", asignadoPor);
                    cmd.ExecuteNonQuery();
                }

                RegistrarLog(conn, "REPORTE_ASIGNADO", asignadoPor,
                    tipoAcceso + " '" + valorAcceso + "' asignado al rol " + rolId, rolId, null);
            }
            RecargarCache();
        }

        public static void RevocarReporte(int rolReporteId, string revocadoPor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                int rolId = 0;
                string detalle = "";
                using (var cmd = new SqlCommand("SELECT RolId, TipoAcceso, ValorAcceso FROM audit.RolReporte WHERE RolReporteId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", rolReporteId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            rolId = r.GetInt32(0);
                            detalle = r.GetString(1) + " '" + r.GetString(2) + "' revocado del rol " + rolId;
                        }
                    }
                }

                using (var cmd = new SqlCommand("UPDATE audit.RolReporte SET Activo = 0 WHERE RolReporteId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", rolReporteId);
                    cmd.ExecuteNonQuery();
                }

                if (rolId > 0)
                    RegistrarLog(conn, "REPORTE_REVOCADO", revocadoPor, detalle, rolId, null);
            }
            RecargarCache();
        }

        public static void AsignarUsuario(string usuario, int rolId, string asignadoPor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM audit.UsuarioRol WHERE Usuario = @Usuario AND RolId = @RolId AND Activo = 1)
                        INSERT INTO audit.UsuarioRol (Usuario, RolId, AsignadoPor)
                        VALUES (@Usuario, @RolId, @AsignadoPor)", conn))
                {
                    cmd.Parameters.AddWithValue("@Usuario", usuario);
                    cmd.Parameters.AddWithValue("@RolId", rolId);
                    cmd.Parameters.AddWithValue("@AsignadoPor", asignadoPor);
                    cmd.ExecuteNonQuery();
                }

                RegistrarLog(conn, "USUARIO_ASIGNADO", asignadoPor,
                    "Usuario " + usuario + " asignado al rol " + rolId, rolId, usuario);
            }
            RecargarCache();
        }

        public static void RemoverUsuario(int usuarioRolId, string removidoPor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string usuario = null;
                int rolId = 0;
                using (var cmd = new SqlCommand("SELECT Usuario, RolId FROM audit.UsuarioRol WHERE UsuarioRolId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", usuarioRolId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) { usuario = r.GetString(0); rolId = r.GetInt32(1); }
                    }
                }

                using (var cmd = new SqlCommand(@"
                    UPDATE audit.UsuarioRol SET Activo = 0, RemovidoPor = @Por, RemovidoUtc = SYSUTCDATETIME()
                    WHERE UsuarioRolId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", usuarioRolId);
                    cmd.Parameters.AddWithValue("@Por", removidoPor);
                    cmd.ExecuteNonQuery();
                }

                if (usuario != null)
                    RegistrarLog(conn, "USUARIO_REMOVIDO", removidoPor,
                        "Usuario " + usuario + " removido del rol " + rolId, rolId, usuario);
            }
            RecargarCache();
        }

        // --- Consultas para la UI de administracion ---

        public static List<RolInfo> ObtenerRoles()
        {
            var result = new List<RolInfo>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT RolId, Nombre, Descripcion, GrupoAD, Activo, CreadoPor, CreadoUtc FROM audit.Rol ORDER BY Nombre", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        result.Add(new RolInfo
                        {
                            RolId = r.GetInt32(0),
                            Nombre = r.GetString(1),
                            Descripcion = r.IsDBNull(2) ? null : r.GetString(2),
                            GrupoAD = r.IsDBNull(3) ? null : r.GetString(3),
                            Activo = r.GetBoolean(4),
                            CreadoPor = r.GetString(5),
                            CreadoUtc = r.GetDateTime(6)
                        });
                    }
                }
            }
            return result;
        }

        public static List<RolReporteInfo> ObtenerAsignaciones(int rolId)
        {
            var result = new List<RolReporteInfo>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT RolReporteId, RolId, TipoAcceso, ValorAcceso, PuedeVer, PuedeExportar, Activo FROM audit.RolReporte WHERE RolId = @RolId ORDER BY TipoAcceso, ValorAcceso", conn))
                {
                    cmd.Parameters.AddWithValue("@RolId", rolId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new RolReporteInfo
                            {
                                RolReporteId = r.GetInt32(0),
                                RolId = r.GetInt32(1),
                                TipoAcceso = r.GetString(2),
                                ValorAcceso = r.GetString(3),
                                PuedeVer = r.GetBoolean(4),
                                PuedeExportar = r.GetBoolean(5),
                                Activo = r.GetBoolean(6)
                            });
                        }
                    }
                }
            }
            return result;
        }

        public static List<UsuarioRolInfo> ObtenerUsuariosRol(int rolId)
        {
            var result = new List<UsuarioRolInfo>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT UsuarioRolId, Usuario, RolId FROM audit.UsuarioRol WHERE RolId = @RolId AND Activo = 1 ORDER BY Usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@RolId", rolId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new UsuarioRolInfo
                            {
                                UsuarioRolId = r.GetInt32(0),
                                Usuario = r.GetString(1),
                                RolId = r.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return result;
        }

        public static List<PermisoLogInfo> ObtenerLogReciente(int top = 50)
        {
            var result = new List<PermisoLogInfo>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TOP (@Top) PermisoLogId, FechaUtc, Accion, Usuario, Detalle, RolId, UsuarioAfectado FROM audit.PermisoLog ORDER BY FechaUtc DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@Top", top);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new PermisoLogInfo
                            {
                                PermisoLogId = r.GetInt64(0),
                                FechaUtc = r.GetDateTime(1),
                                Accion = r.GetString(2),
                                Usuario = r.GetString(3),
                                Detalle = r.GetString(4),
                                RolId = r.IsDBNull(5) ? (int?)null : r.GetInt32(5),
                                UsuarioAfectado = r.IsDBNull(6) ? null : r.GetString(6)
                            });
                        }
                    }
                }
            }
            return result;
        }

        private static void RegistrarLog(SqlConnection conn, string accion, string usuario,
            string detalle, int? rolId, string usuarioAfectado)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO audit.PermisoLog (Accion, Usuario, Detalle, RolId, UsuarioAfectado)
                VALUES (@Accion, @Usuario, @Detalle, @RolId, @UsuarioAfectado)", conn))
            {
                cmd.Parameters.AddWithValue("@Accion", accion);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                cmd.Parameters.AddWithValue("@Detalle", detalle);
                cmd.Parameters.AddWithValue("@RolId", (object)rolId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioAfectado", (object)usuarioAfectado ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // DTOs para el cache y la UI
    public class RolInfo
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string GrupoAD { get; set; }
        public bool Activo { get; set; }
        public string CreadoPor { get; set; }
        public DateTime CreadoUtc { get; set; }
    }

    public class RolReporteInfo
    {
        public int RolReporteId { get; set; }
        public int RolId { get; set; }
        public string TipoAcceso { get; set; }
        public string ValorAcceso { get; set; }
        public bool PuedeVer { get; set; }
        public bool PuedeExportar { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioRolInfo
    {
        public int UsuarioRolId { get; set; }
        public string Usuario { get; set; }
        public int RolId { get; set; }
    }

    public class PermisoLogInfo
    {
        public long PermisoLogId { get; set; }
        public DateTime FechaUtc { get; set; }
        public string Accion { get; set; }
        public string Usuario { get; set; }
        public string Detalle { get; set; }
        public int? RolId { get; set; }
        public string UsuarioAfectado { get; set; }
    }
}
