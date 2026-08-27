using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal.Controllers
{
    [Authorize]
    public class PermisosController : Controller
    {
        // GET: /Permisos
        public ActionResult Index()
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            if (!PermisosService.Habilitado)
            {
                ViewBag.Mensaje = "El sistema de permisos no esta habilitado. Configure Permisos:Habilitado=true en Web.config.";
                return View();
            }

            ViewBag.Roles = PermisosService.ObtenerRoles();
            ViewBag.LogReciente = PermisosService.ObtenerLogReciente(20);
            return View();
        }

        // GET: /Permisos/Rol/5
        public ActionResult Rol(int id)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            if (!PermisosService.Habilitado) return RedirectToAction("Index");

            var roles = PermisosService.ObtenerRoles();
            var rol = roles.FirstOrDefault(r => r.RolId == id);
            if (rol == null) return HttpNotFound();

            ViewBag.Rol = rol;
            ViewBag.Asignaciones = PermisosService.ObtenerAsignaciones(id);
            ViewBag.Usuarios = PermisosService.ObtenerUsuariosRol(id);
            return View();
        }

        // POST: /Permisos/CrearRol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearRol(string nombre, string descripcion, string grupoAD)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            if (string.IsNullOrWhiteSpace(nombre))
                return new HttpStatusCodeResult(400, "El nombre del rol es obligatorio.");

            string usuario = User.Identity.Name;
            PermisosService.CrearRol(nombre.Trim(), descripcion?.Trim(), grupoAD?.Trim(), usuario);
            return RedirectToAction("Index");
        }

        // POST: /Permisos/AsignarReporte
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarReporte(int rolId, string tipoAcceso, string valorAcceso,
            bool puedeVer, bool puedeExportar)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            if (string.IsNullOrWhiteSpace(tipoAcceso) || string.IsNullOrWhiteSpace(valorAcceso))
                return new HttpStatusCodeResult(400, "Tipo y valor de acceso son obligatorios.");

            PermisosService.AsignarReporte(rolId, tipoAcceso.Trim(), valorAcceso.Trim(),
                puedeVer, puedeExportar, User.Identity.Name);
            return RedirectToAction("Rol", new { id = rolId });
        }

        // POST: /Permisos/RevocarReporte
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevocarReporte(int rolReporteId, int rolId)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            PermisosService.RevocarReporte(rolReporteId, User.Identity.Name);
            return RedirectToAction("Rol", new { id = rolId });
        }

        // POST: /Permisos/AsignarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarUsuario(string usuario, int rolId)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            if (string.IsNullOrWhiteSpace(usuario))
                return new HttpStatusCodeResult(400, "El usuario es obligatorio.");

            PermisosService.AsignarUsuario(usuario.Trim(), rolId, User.Identity.Name);
            return RedirectToAction("Rol", new { id = rolId });
        }

        // POST: /Permisos/RemoverUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoverUsuario(int usuarioRolId, int rolId)
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            PermisosService.RemoverUsuario(usuarioRolId, User.Identity.Name);
            return RedirectToAction("Rol", new { id = rolId });
        }

        // GET: /Permisos/Log
        public ActionResult Log()
        {
            if (!EsAdmin()) return new HttpStatusCodeResult(403);
            ViewBag.Log = PermisosService.ObtenerLogReciente(100);
            return View();
        }

        private bool EsAdmin()
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated) return false;

            // Bypass explicito para desarrollo / pruebas (Audit:BypassAdminEnDev=true)
            string bypass = ConfigurationManager.AppSettings["Audit:BypassAdminEnDev"] ?? "false";
            if (string.Equals(bypass, "true", StringComparison.OrdinalIgnoreCase)) return true;

            // Usuarios individuales autorizados (Audit:UsuariosAdmin)
            string usuarios = ConfigurationManager.AppSettings["Audit:UsuariosAdmin"] ?? "";
            if (!string.IsNullOrWhiteSpace(usuarios) && !string.IsNullOrEmpty(User.Identity.Name))
            {
                var listaUsr = usuarios.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(u => u.Trim())
                                       .Where(u => !string.IsNullOrEmpty(u));
                foreach (var u in listaUsr)
                {
                    if (string.Equals(u, User.Identity.Name, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }

            string grupos = ConfigurationManager.AppSettings["Audit:GruposAdmin"] ?? "";
            if (string.IsNullOrWhiteSpace(grupos)) return false;

            var partes = grupos.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(g => g.Trim())
                               .Where(g => !string.IsNullOrEmpty(g));

            foreach (var grupo in partes)
            {
                try { if (User.IsInRole(grupo)) return true; }
                catch { }
            }
            return false;
        }
    }
}
