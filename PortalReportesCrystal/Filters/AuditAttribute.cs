// ============================================================================
// AuditAttribute.cs - FILTRO GLOBAL DE AUDITORIA
// ============================================================================
// Se ejecuta automaticamente en cada request MVC autenticado. Su unica
// responsabilidad es asegurar que exista una SesionId asignada al HttpContext
// y actualizar la ultima actividad. Los eventos especificos (VER_REPORTE,
// EXPORTAR_*, etc.) se registran desde los controladores, NO aqui.
//
// Por que se hace asi:
//   - Un filtro global que registra un evento por request llenaria la BD de
//     ruido (cada JS/CSS/imagen contaria como acceso).
//   - Aqui solo garantizamos la correlacion (SesionId) y actualizacion de
//     UltimaActividad. Los eventos de negocio se emiten explicitamente.
//
// Registro:
//   En Global.asax.cs Application_Start:
//     GlobalFilters.Filters.Add(new AuditAttribute());
//
// Nota: hereda de ActionFilterAttribute (no de IActionFilter directo) para
//       permitir tambien uso como atributo sobre acciones especificas.
// ============================================================================

using System;
using System.Web.Mvc;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal.Filters
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (!AuditoriaService.Habilitado) return;
                if (filterContext == null || filterContext.HttpContext == null) return;

                // Solo si el usuario esta autenticado
                var user = filterContext.HttpContext.User;
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated) return;

                var ctx = filterContext.HttpContext.ApplicationInstance != null
                    ? filterContext.HttpContext.ApplicationInstance.Context
                    : System.Web.HttpContext.Current;

                // Asegurar SesionId (registra LOGIN si es la primera vez)
                Guid sesion = AuditoriaService.ObtenerSesionActual(ctx);

                // Guardar sesion en el HttpContext.Items para que los controladores
                // la reutilicen sin volver a consultarla
                filterContext.HttpContext.Items["AuditSesionId"] = sesion;
            }
            catch
            {
                // Nunca reventar la peticion por auditoria
            }
        }
    }

    // Helper para consumir la sesion en los controladores
    public static class AuditContext
    {
        public static Guid? SesionActual(System.Web.HttpContextBase ctx)
        {
            if (ctx == null) return null;
            object val = ctx.Items != null ? ctx.Items["AuditSesionId"] : null;
            if (val is Guid) return (Guid)val;
            return null;
        }

        public static Guid? SesionActual(System.Web.HttpContext ctx)
        {
            if (ctx == null) return null;
            object val = ctx.Items != null ? ctx.Items["AuditSesionId"] : null;
            if (val is Guid) return (Guid)val;
            return null;
        }
    }
}
