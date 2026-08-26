// ============================================================================
// ReporteViewModel.cs - MODELOS DE DATOS (la "M" de MVC)
// ============================================================================
// Los ViewModels son clases simples que transportan datos desde el Controller
// hacia la Vista (.cshtml). No tienen logica, solo propiedades.
//
// Por que no pasar los datos directamente?
// Porque el ViewModel te permite elegir EXACTAMENTE que datos necesita la vista,
// sin exponer mas informacion de la necesaria.
//
// Flujo:  Controller crea el ViewModel -> lo llena con datos -> lo pasa a la Vista
//         La Vista lee las propiedades del ViewModel para mostrar HTML
// ============================================================================

using System;
using System.Collections.Generic;

namespace PortalReportesCrystal.Models
{
    // Tipos de origen de un reporte
    public enum TipoReporte
    {
        // Archivo .rpt local que el portal ejecuta con el SDK de Crystal
        Local,
        // Reporte publicado en un servidor externo (SAP BO CMC, ServerReports, etc.)
        // El portal solo redirige al usuario a la URL del servidor
        Externo,
        // Documento Web Intelligence descubierto via API REST de SAP BO
        WebI
    }

    // Informacion de un reporte disponible en el portal (local o externo)
    public class ReporteInfo
    {
        public string Nombre { get; set; }

        // Solo aplica cuando Tipo == Local. Identifica la raiz configurada
        // (ver ReportesLocales/configuracion.json). Ejemplo: "crystalxi".
        public string RaizId { get; set; }

        // Solo aplica cuando Tipo == Local. Ruta relativa del .rpt dentro
        // de la raiz. Ejemplo: "CREDITOS/DocumentoRemitido.rpt".
        // Se envia URL-encoded en los enlaces del portal.
        public string PathRelativo { get; set; }

        // Nombre del archivo .rpt (mostrado en detalle, no usado como path).
        public string Archivo { get; set; }

        // Agrupacion visual: el listado se ordena por categoria
        public string Categoria { get; set; }

        public TipoReporte Tipo { get; set; }

        // Solo aplica cuando Tipo == Externo: URL directa al reporte publicado
        public string UrlExterna { get; set; }

        // Solo aplica cuando Tipo == Externo o Local: origen para mostrar en
        // la UI (ej: "SAP BO", "Crystal XI"). Ayuda a que el usuario sepa
        // desde donde se abrira o carga el reporte.
        public string Servidor { get; set; }

        // Descripcion breve opcional para ambos tipos
        public string Descripcion { get; set; }

        // Info del cache de analisis:
        //   null = aun no analizado (ej. cache en construccion)
        //   true = el reporte requiere parametros del usuario
        //   false = se puede ejecutar directamente sin prompts
        // Se completa via CacheParametros (analisis en background al arrancar).
        public bool? TieneParametros { get; set; }

        // Estado del ultimo intento de ejecucion (null si nunca fallo o se
        // ejecuto con exito despues). Sirve para mostrar una senal en el
        // listado cuando el reporte tiene problemas.
        public string UltimoError { get; set; }
        public string FechaUltimoError { get; set; }
    }

    // Clases usadas para deserializar el JSON de raices locales.
    // Estructura debe coincidir con ReportesLocales/configuracion.json
    public class ConfiguracionRaices
    {
        public List<RaizLocal> Raices { get; set; }
    }

    public class RaizLocal
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public string PrefijoGrupoRaiz { get; set; }
    }

    // Datos que necesita la vista Ver.cshtml para mostrar un reporte individual
    public class ReporteViewModel
    {
        public string NombreReporte { get; set; }
        public string ArchivoRpt { get; set; }
        public string UsuarioActual { get; set; }

        // Parametros que el reporte declara. Vacio si el reporte no tiene prompts.
        public List<ParametroReporte> Parametros { get; set; } = new List<ParametroReporte>();

        // true = todos los parametros obligatorios ya tienen valor -> se puede renderizar
        // false = falta al menos uno -> mostrar formulario, no cargar el visor
        public bool ParametrosCompletos { get; set; }
    }

    // Descripcion de un parametro (prompt) declarado en el .rpt
    public class ParametroReporte
    {
        public string Nombre { get; set; }         // Nombre tecnico interno
        public string Etiqueta { get; set; }       // PromptText mostrado al usuario
        public string Tipo { get; set; }           // "String" | "Number" | "Date" | "DateTime" | "Boolean" | "Currency"
        public bool Opcional { get; set; }         // Puede aceptar valor nulo
        public bool MultiValor { get; set; }       // Acepta varios valores separados por coma
        public string ValorActual { get; set; }    // Valor recibido del formulario (si lo hay)
        public string ValorDefecto { get; set; }   // Sugerencia de valor por defecto
    }

    // Datos que necesita la vista ErrorReporte.cshtml cuando falla la generacion
    // de un reporte. Contiene solo lo que el usuario debe ver: nunca la traza
    // tecnica de la excepcion (esa se registra en el log del servidor).
    public class ErrorReporteViewModel
    {
        public string NombreReporte { get; set; }
        public string Mensaje { get; set; }
    }

    // Datos que necesita la vista Index.cshtml (pagina principal)
    public class HomeViewModel
    {
        public List<ReporteInfo> Reportes { get; set; }
        public string UsuarioActual { get; set; }
        public bool WebIDesdeCache { get; set; }
        public DateTime? WebIUltimaActualizacion { get; set; }
        public bool WebIHabilitado { get; set; }
    }

    // Clases usadas para deserializar el JSON de reportes CMC.
    // La estructura debe coincidir con ReportesCMC\catalogo.json
    public class CatalogoCMC
    {
        public List<GrupoCMC> Grupos { get; set; }
    }

    public class GrupoCMC
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<ReporteCMC> Reportes { get; set; }
    }

    public class ReporteCMC
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Servidor { get; set; }
        public string Url { get; set; }
    }

    // Datos que necesita la vista Estadisticas.cshtml
    public class EstadisticasViewModel
    {
        public string UsuarioActual { get; set; }

        // Resumen de reportes (del cache)
        public int TotalCrystalReports { get; set; }
        public int TotalWebI { get; set; }
        public int TotalReportes { get; set; }
        public DateTime? UltimoEscaneo { get; set; }
        public bool DatosDesdeCache { get; set; }

        // Sesiones
        public List<SesionSapBo> Sesiones { get; set; } = new List<SesionSapBo>();
        public string SesionesError { get; set; }

        // Licencias
        public List<LicenciaSapBo> Licencias { get; set; } = new List<LicenciaSapBo>();
        public string LicenciasError { get; set; }

        // Servidores
        public List<ServidorSapBo> Servidores { get; set; } = new List<ServidorSapBo>();
        public string ServidoresError { get; set; }
    }

    public class SesionSapBo
    {
        public string Usuario { get; set; }
        public string TipoSesion { get; set; }
        public string HoraInicio { get; set; }
        public string Id { get; set; }
    }

    public class LicenciaSapBo
    {
        public string Tipo { get; set; }
        public string Producto { get; set; }
        public int Total { get; set; }
        public int EnUso { get; set; }
        public int Pico { get; set; }
        public string Expiracion { get; set; }
        public string Clave { get; set; }
    }

    public class ServidorSapBo
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public string Id { get; set; }
    }

    // Datos de la vista previa "solo datos": primeras + ultimas filas
    public class PreviewDatosModel
    {
        public bool SinDatos { get; set; }
        public List<string> Cabeceras { get; set; }
        public List<List<string>> PrimerasFilas { get; set; }
        public List<List<string>> UltimasFilas { get; set; }
        public int TotalFilas { get; set; }
        public bool Truncado { get; set; }
        public int NInicio { get; set; }
        public int NFin { get; set; }
    }
}
