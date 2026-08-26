// ============================================================================
// ReportesController.cs - CONTROLADOR DE CRYSTAL REPORTS
// ============================================================================
// Este controlador visualiza y exporta reportes .rpt usando el SDK de Crystal.
//
// RESOLUCION DE RUTAS (importante para seguridad):
// Un reporte se identifica por dos parametros en la URL:
//   raizId  -> ID de la raiz configurada (ej. "crystalxi") o "proyecto"
//   path    -> ruta relativa DENTRO de esa raiz (ej. "CREDITOS/rep.rpt")
//
// El metodo ResolverRuta() combina raiz + path, normaliza y VALIDA que el
// resultado siga dentro de la carpeta base configurada. Esto impide ataques
// de directory traversal (path=..\..\Windows\system.ini).
//
// Ciclo de vida de un ReportDocument:
//   1. new ReportDocument()
//   2. Load(rutaFisica)
//   3. (opcional) aplicar credenciales de BD
//   4. ExportToStream(formato)
//   5. Close() + Dispose()   (SIEMPRE, en bloque finally)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using PortalReportesCrystal.Filters;
using PortalReportesCrystal.Services;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using PortalReportesCrystal.Models;

namespace PortalReportesCrystal.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private const string RAIZ_LEGACY_ID = "proyecto";

        // --------------------------------------------------------------
        // Helper de auditoria: registra un evento con los datos del reporte
        // actual sin duplicar codigo entre Ver/Preview/PreviewDatos/Exportar.
        // No lanza si el servicio esta deshabilitado o falla.
        // --------------------------------------------------------------
        private void AuditarReporte(
            string tipoEvento,
            string raizId,
            string path,
            string formato = null,
            long? tamanioBytes = null,
            int? duracionMs = null,
            string mensajeError = null)
        {
            try
            {
                if (!AuditoriaService.Habilitado) return;
                var evento = new EventoAuditoria
                {
                    SesionId = AuditContext.SesionActual(HttpContext),
                    TipoEvento = tipoEvento,
                    Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                    IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                    RaizId = raizId,
                    PathReporte = path,
                    NombreReporte = string.IsNullOrEmpty(path) ? null : Path.GetFileNameWithoutExtension(path),
                    TipoReporte = "Local",
                    Servidor = "Local",
                    Formato = formato,
                    TamanioBytes = tamanioBytes,
                    DuracionMs = duracionMs,
                    MensajeError = mensajeError
                };

                // Adjuntar los p_* parametros del querystring como EventoParametro
                var qs = Request != null ? Request.QueryString : null;
                if (qs != null && qs.Count > 0)
                {
                    var dict = new Dictionary<string, string>();
                    foreach (string key in qs.AllKeys)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!key.StartsWith("p_", StringComparison.OrdinalIgnoreCase)) continue;
                        string nombre = key.Substring(2);
                        if (string.IsNullOrWhiteSpace(nombre)) continue;
                        dict[nombre.ToUpperInvariant()] = qs[key] ?? "";
                    }
                    if (dict.Count > 0)
                        evento.Parametros = dict;
                }

                AuditoriaService.RegistrarEvento(evento);
            }
            catch { /* no reventar el request por auditoria */ }
        }

        // ==================================================================
        // GET: /Reportes/Ver?raizId=...&path=...
        //   Si el reporte tiene parametros y NO se recibieron via querystring
        //   (con prefijo "p_"), la vista muestra un formulario para llenarlos.
        //   Cuando estan completos, se activa el visor embebido.
        // ==================================================================
        public ActionResult Ver(string raizId, string path, string archivo)
        {
            NormalizarParametros(ref raizId, ref path, archivo);
            string ruta = ResolverRuta(raizId, path);
            if (ruta == null || !System.IO.File.Exists(ruta))
                return HttpNotFound("Reporte no encontrado");

            var model = new ReporteViewModel
            {
                NombreReporte = Path.GetFileNameWithoutExtension(path),
                ArchivoRpt = path,
                UsuarioActual = User.Identity.Name,
                Parametros = LeerParametros(ruta)
            };

            // Rellenar con los valores del querystring (formulario ya enviado)
            var valores = ExtraerValoresDelQueryString();
            foreach (var pr in model.Parametros)
            {
                if (valores.ContainsKey(pr.Nombre))
                    pr.ValorActual = valores[pr.Nombre];
            }

            // Se considera "completo" si TODO parametro no opcional tiene valor
            model.ParametrosCompletos = model.Parametros.All(pr =>
                pr.Opcional || !string.IsNullOrEmpty(pr.ValorActual));

            // Si no hay parametros declarados, es automaticamente completo
            if (model.Parametros.Count == 0) model.ParametrosCompletos = true;

            ViewBag.RaizId = raizId;
            ViewBag.Path = path;

            AuditarReporte("VER_REPORTE", raizId, path);
            return View(model);
        }

        // Cantidad de paginas que se conservan al principio y al final
        // cuando el PDF completo es "grande" (mas de 2*PAGINAS_MUESTRA).
        private const int PAGINAS_MUESTRA = 3;

        // ==================================================================
        // GET: /Reportes/Preview?raizId=...&path=...
        //
        // Genera un PDF de vista previa "condensado":
        //   - Si el reporte tiene <= 6 paginas -> se devuelve completo.
        //   - Si tiene mas de 6 -> primeras 3 + separador + ultimas 3.
        //
        // Se usa PDFsharp para recortar el PDF completo generado por Crystal.
        // Esto respeta el ancho original del reporte (Crystal lo define) y
        // permite que el visor del navegador lo muestre sin cortar columnas.
        // ==================================================================
        public ActionResult Preview(string raizId, string path, string archivo)
        {
            NormalizarParametros(ref raizId, ref path, archivo);
            string ruta = ResolverRuta(raizId, path);
            if (ruta == null || !System.IO.File.Exists(ruta))
                return HttpNotFound();

            var reportDocument = new ReportDocument();
            var sw = Stopwatch.StartNew();
            try
            {
                reportDocument.Load(ruta);
                AplicarValoresParametros(reportDocument, ExtraerValoresDelQueryString());

                byte[] pdfCompleto;
                using (var stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    pdfCompleto = ms.ToArray();
                }

                EstadoReportes.RegistrarExito(EstadoReportes.ClaveDeLocal(raizId, path));

                byte[] pdfCondensado = CondensarPdf(pdfCompleto, PAGINAS_MUESTRA);

                sw.Stop();
                AuditarReporte("PREVIEW", raizId, path,
                    tamanioBytes: pdfCondensado.LongLength,
                    duracionMs: (int)sw.ElapsedMilliseconds);

                Response.AddHeader("Content-Disposition",
                    "inline; filename=\"" + Path.GetFileNameWithoutExtension(path) + "_preview.pdf\"");
                return File(pdfCondensado, "application/pdf");
            }
            catch (Exception ex)
            {
                sw.Stop();
                EstadoReportes.RegistrarError(
                    EstadoReportes.ClaveDeLocal(raizId, path),
                    MensajeCorto(ex),
                    User.Identity.Name);
                AuditarReporte("ERROR_GENERACION", raizId, path,
                    duracionMs: (int)sw.ElapsedMilliseconds,
                    mensajeError: MensajeCorto(ex));
                System.Diagnostics.Trace.TraceError(
                    "Error al previsualizar '{0}/{1}': {2}", raizId, path, ex);
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                ViewBag.NombreReporte = Path.GetFileNameWithoutExtension(path);
                return View("PreviewError", (object)MensajeAmigable(ex));
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }

        // Recorta un PDF conservando las primeras N + ultimas N paginas.
        // Inserta una hoja de separacion entre ambos bloques indicando
        // cuantas paginas intermedias se omitieron.
        private static byte[] CondensarPdf(byte[] pdfCompleto, int muestraCadaLado)
        {
            using (var msIn = new MemoryStream(pdfCompleto))
            {
                var origen = PdfSharp.Pdf.IO.PdfReader.Open(msIn, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                int total = origen.PageCount;

                // Si cabe entero, devolvemos el PDF sin modificar
                if (total <= muestraCadaLado * 2)
                    return pdfCompleto;

                var destino = new PdfSharp.Pdf.PdfDocument();
                destino.Info.Title = origen.Info.Title;
                destino.Info.Creator = "Portal Reportes Crystal (vista previa)";

                // Primeras N paginas
                for (int i = 0; i < muestraCadaLado; i++)
                    destino.AddPage(origen.Pages[i]);

                // Hoja separadora dimensionada al ancho del reporte
                var refPage = origen.Pages[0];
                DibujarSeparador(destino, refPage.Width, refPage.Height,
                                 total, muestraCadaLado);

                // Ultimas N paginas
                for (int i = total - muestraCadaLado; i < total; i++)
                    destino.AddPage(origen.Pages[i]);

                using (var msOut = new MemoryStream())
                {
                    destino.Save(msOut, closeStream: false);
                    return msOut.ToArray();
                }
            }
        }

        // Renderiza la hoja intermedia con el conteo de paginas omitidas
        private static void DibujarSeparador(PdfSharp.Pdf.PdfDocument doc,
                                             PdfSharp.Drawing.XUnit ancho,
                                             PdfSharp.Drawing.XUnit alto,
                                             int totalPaginas,
                                             int muestraCadaLado)
        {
            var pagina = doc.AddPage();
            pagina.Width = ancho;
            pagina.Height = alto;

            using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(pagina))
            {
                // Fondo neutro
                gfx.DrawRectangle(PdfSharp.Drawing.XBrushes.WhiteSmoke,
                    new PdfSharp.Drawing.XRect(0, 0, pagina.Width, pagina.Height));

                // Banda superior con color de marca
                var brushBanda = new PdfSharp.Drawing.XSolidBrush(
                    PdfSharp.Drawing.XColor.FromArgb(0x00, 0x15, 0x22));
                gfx.DrawRectangle(brushBanda,
                    new PdfSharp.Drawing.XRect(0, 0, pagina.Width, 40));

                var brushAcento = new PdfSharp.Drawing.XSolidBrush(
                    PdfSharp.Drawing.XColor.FromArgb(0xFF, 0x00, 0x00));
                gfx.DrawRectangle(brushAcento,
                    new PdfSharp.Drawing.XRect(0, 40, pagina.Width, 3));

                var fuenteTitulo = new PdfSharp.Drawing.XFont("Calibri", 22,
                    PdfSharp.Drawing.XFontStyle.Bold);
                var fuenteTexto = new PdfSharp.Drawing.XFont("Calibri", 12);
                var fuenteNota = new PdfSharp.Drawing.XFont("Calibri", 10,
                    PdfSharp.Drawing.XFontStyle.Italic);

                int intermedias = totalPaginas - muestraCadaLado * 2;
                double y = pagina.Height / 2 - 60;

                gfx.DrawString("Paginas intermedias omitidas",
                    fuenteTitulo,
                    new PdfSharp.Drawing.XSolidBrush(
                        PdfSharp.Drawing.XColor.FromArgb(0x00, 0x15, 0x22)),
                    new PdfSharp.Drawing.XRect(0, y, pagina.Width, 40),
                    PdfSharp.Drawing.XStringFormats.TopCenter);

                gfx.DrawString(
                    string.Format(
                        "Se muestran las primeras {0} y las ultimas {0} paginas.",
                        muestraCadaLado),
                    fuenteTexto, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XRect(0, y + 50, pagina.Width, 20),
                    PdfSharp.Drawing.XStringFormats.TopCenter);

                gfx.DrawString(
                    string.Format("Se omitieron {0:N0} paginas intermedias de un total de {1:N0}.",
                        intermedias, totalPaginas),
                    fuenteTexto, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XRect(0, y + 75, pagina.Width, 20),
                    PdfSharp.Drawing.XStringFormats.TopCenter);

                gfx.DrawString(
                    "Para examinar todos los datos, descargue el reporte en Excel (solo datos).",
                    fuenteNota,
                    new PdfSharp.Drawing.XSolidBrush(
                        PdfSharp.Drawing.XColor.FromArgb(0x66, 0x66, 0x66)),
                    new PdfSharp.Drawing.XRect(0, y + 120, pagina.Width, 20),
                    PdfSharp.Drawing.XStringFormats.TopCenter);
            }
        }

        // ==================================================================
        // GET: /Reportes/PreviewDatos?raizId=...&path=...&nInicio=N&nFin=M
        //
        // Vista previa RAPIDA orientada a datos:
        //   - Exporta el reporte como CSV (Character Separated Values)
        //   - Toma la fila de cabeceras + las primeras N filas + las ultimas M
        //   - Renderiza como tabla HTML con scroll horizontal para no cortar
        //     reportes de gran ancho.
        //   - Incluye un aviso claro guiando a Excel solo datos para analisis.
        //
        // Ventajas sobre el PDF inline:
        //   - Carga rapida (no procesa layout ni paginacion)
        //   - No corta columnas: scroll horizontal automatico
        //   - Ancho fluido segun tamano de ventana
        // Limitaciones aceptadas:
        //   - Se pierde el diseno grafico del .rpt (colores, agrupamiento)
        //   - Los subtotales del reporte no se calculan (son datos crudos)
        //   Estas limitaciones son deliberadas: el proposito es previsualizar,
        //   no reemplazar la exportacion formal.
        // ==================================================================
        public ActionResult PreviewDatos(string raizId, string path, string archivo,
                                         int nInicio = 20, int nFin = 10)
        {
            NormalizarParametros(ref raizId, ref path, archivo);
            string ruta = ResolverRuta(raizId, path);
            if (ruta == null || !System.IO.File.Exists(ruta))
                return HttpNotFound();

            // Limitar los valores para evitar peticiones abusivas
            if (nInicio < 1) nInicio = 20;
            if (nInicio > 200) nInicio = 200;
            if (nFin < 0) nFin = 10;
            if (nFin > 200) nFin = 200;

            var reportDocument = new ReportDocument();
            try
            {
                reportDocument.Load(ruta);
                AplicarValoresParametros(reportDocument, ExtraerValoresDelQueryString());

                string csv;
                using (var stream = reportDocument.ExportToStream(ExportFormatType.CharacterSeparatedValues))
                using (var reader = new StreamReader(stream, Encoding.Default))
                {
                    csv = reader.ReadToEnd();
                }

                EstadoReportes.RegistrarExito(EstadoReportes.ClaveDeLocal(raizId, path));

                var previa = ConstruirTablaHtml(csv, nInicio, nFin);
                ViewBag.NombreReporte = Path.GetFileNameWithoutExtension(path);
                ViewBag.RaizId = raizId;
                ViewBag.Path = path;
                ViewBag.QueryString = Request.QueryString.ToString();
                return View("PreviewDatos", previa);
            }
            catch (Exception ex)
            {
                EstadoReportes.RegistrarError(
                    EstadoReportes.ClaveDeLocal(raizId, path),
                    MensajeCorto(ex),
                    User.Identity.Name);
                System.Diagnostics.Trace.TraceError(
                    "Error en PreviewDatos '{0}/{1}': {2}", raizId, path, ex);
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                ViewBag.NombreReporte = Path.GetFileNameWithoutExtension(path);
                // Cast a object para evitar la sobrecarga View(viewName, masterName)
                // que MVC elige cuando el modelo es string.
                return View("PreviewError", (object)MensajeAmigable(ex));
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }

        // Construye el modelo de vista previa: cabeceras + primeras + ultimas filas
        private static PreviewDatosModel ConstruirTablaHtml(string csv, int nInicio, int nFin)
        {
            var modelo = new PreviewDatosModel();
            if (string.IsNullOrWhiteSpace(csv))
            {
                modelo.SinDatos = true;
                return modelo;
            }

            // Parseo CSV respetando campos entre comillas
            var todasLasFilas = ParsearCsv(csv);
            if (todasLasFilas.Count == 0)
            {
                modelo.SinDatos = true;
                return modelo;
            }

            // Primera fila = cabeceras
            modelo.Cabeceras = todasLasFilas[0];
            var datos = todasLasFilas.Skip(1).ToList();
            modelo.TotalFilas = datos.Count;

            if (datos.Count <= nInicio + nFin)
            {
                // Caben todas sin necesidad de truncar
                modelo.PrimerasFilas = datos;
                modelo.UltimasFilas = new List<List<string>>();
                modelo.Truncado = false;
            }
            else
            {
                modelo.PrimerasFilas = datos.Take(nInicio).ToList();
                modelo.UltimasFilas = datos.Skip(datos.Count - nFin).ToList();
                modelo.Truncado = true;
                modelo.NInicio = nInicio;
                modelo.NFin = nFin;
            }
            return modelo;
        }

        // Parser CSV minimo: soporta campos entre comillas dobles con escape ""
        private static List<List<string>> ParsearCsv(string csv)
        {
            var filas = new List<List<string>>();
            var actual = new List<string>();
            var campo = new StringBuilder();
            bool enComillas = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (enComillas)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            campo.Append('"'); i++;
                        }
                        else enComillas = false;
                    }
                    else campo.Append(c);
                }
                else
                {
                    if (c == '"') enComillas = true;
                    else if (c == ',')
                    {
                        actual.Add(campo.ToString());
                        campo.Clear();
                    }
                    else if (c == '\r') { /* ignorar */ }
                    else if (c == '\n')
                    {
                        actual.Add(campo.ToString());
                        campo.Clear();
                        if (actual.Any(x => !string.IsNullOrEmpty(x)))
                            filas.Add(actual);
                        actual = new List<string>();
                    }
                    else campo.Append(c);
                }
            }
            if (campo.Length > 0 || actual.Count > 0)
            {
                actual.Add(campo.ToString());
                if (actual.Any(x => !string.IsNullOrEmpty(x)))
                    filas.Add(actual);
            }
            return filas;
        }

        private static string MensajeCorto(Exception ex)
        {
            string msg = ex.Message ?? ex.GetType().Name;
            if (msg.Length > 120) msg = msg.Substring(0, 120) + "...";
            return msg;
        }

        // ==================================================================
        // GET: /Reportes/Exportar?raizId=...&path=...&formato=pdf|excel|exceldata|word
        // ==================================================================
        public ActionResult Exportar(string raizId, string path, string archivo, string formato)
        {
            NormalizarParametros(ref raizId, ref path, archivo);
            string ruta = ResolverRuta(raizId, path);
            if (ruta == null || !System.IO.File.Exists(ruta))
                return HttpNotFound();

            // Si el reporte tiene parametros no completados, redirigir al formulario
            var faltantes = ParametrosFaltantes(ruta);
            if (faltantes.Count > 0)
            {
                var rvd = new RouteValueDictionary();
                rvd["raizId"] = raizId;
                rvd["path"] = path;
                if (!string.IsNullOrEmpty(formato)) rvd["formato"] = formato;
                foreach (var k in ExtraerValoresDelQueryString())
                    rvd["p_" + k.Key] = k.Value;
                return RedirectToAction("Ver", rvd);
            }

            var reportDocument = new ReportDocument();
            var sw = Stopwatch.StartNew();
            string formatoNormalizado = (formato ?? "pdf").ToLower();
            try
            {
                reportDocument.Load(ruta);
                AplicarValoresParametros(reportDocument, ExtraerValoresDelQueryString());

                ExportFormatType exportFormat;
                string contentType;
                string extension;
                switch (formatoNormalizado)
                {
                    case "excel":
                        exportFormat = ExportFormatType.Excel;
                        contentType = "application/vnd.ms-excel";
                        extension = ".xls";
                        break;
                    case "exceldata":
                        exportFormat = ExportFormatType.ExcelRecord;
                        contentType = "application/vnd.ms-excel";
                        extension = "_datos.xls";
                        break;
                    case "word":
                        exportFormat = ExportFormatType.WordForWindows;
                        contentType = "application/msword";
                        extension = ".doc";
                        break;
                    default:
                        exportFormat = ExportFormatType.PortableDocFormat;
                        contentType = "application/pdf";
                        extension = ".pdf";
                        break;
                }

                var stream = reportDocument.ExportToStream(exportFormat);
                string fileName = Path.GetFileNameWithoutExtension(path) + extension;
                EstadoReportes.RegistrarExito(EstadoReportes.ClaveDeLocal(raizId, path));

                sw.Stop();
                string codigoEvento;
                switch (formatoNormalizado)
                {
                    case "excel":     codigoEvento = "EXPORTAR_EXCEL"; break;
                    case "exceldata": codigoEvento = "EXPORTAR_EXCELDATA"; break;
                    case "word":      codigoEvento = "EXPORTAR_WORD"; break;
                    default:          codigoEvento = "EXPORTAR_PDF"; break;
                }
                long tamanio = 0;
                try { if (stream != null && stream.CanSeek) tamanio = stream.Length; } catch { }
                AuditarReporte(codigoEvento, raizId, path,
                    formato: formatoNormalizado,
                    tamanioBytes: tamanio,
                    duracionMs: (int)sw.ElapsedMilliseconds);

                return File(stream, contentType, fileName);
            }
            catch (CrystalReportsException ex)
            {
                sw.Stop();
                EstadoReportes.RegistrarError(
                    EstadoReportes.ClaveDeLocal(raizId, path), MensajeCorto(ex), User.Identity.Name);
                AuditarReporte("ERROR_GENERACION", raizId, path,
                    formato: formatoNormalizado,
                    duracionMs: (int)sw.ElapsedMilliseconds,
                    mensajeError: MensajeCorto(ex));
                return VistaDeError(path, ex);
            }
            catch (Exception ex)
            {
                sw.Stop();
                EstadoReportes.RegistrarError(
                    EstadoReportes.ClaveDeLocal(raizId, path), MensajeCorto(ex), User.Identity.Name);
                AuditarReporte("ERROR_GENERACION", raizId, path,
                    formato: formatoNormalizado,
                    duracionMs: (int)sw.ElapsedMilliseconds,
                    mensajeError: MensajeCorto(ex));
                return VistaDeError(path, ex);
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }

        // ==================================================================
        // Resolucion segura de rutas.
        //
        // Retorna:
        //   - Ruta fisica absoluta al .rpt si es valida
        //   - null si la raiz no existe, el path esta vacio, o el path
        //     resuelto queda fuera de la raiz (intento de traversal).
        //
        // Traversal: incluso si el path viene como "..\..\Windows\...", tras
        // Path.GetFullPath se obtiene la ruta canonica y se compara contra
        // la ruta canonica de la raiz. Si no empieza con ella, rechazamos.
        // ==================================================================
        private string ResolverRuta(string raizId, string pathRelativo)
        {
            if (string.IsNullOrWhiteSpace(raizId) || string.IsNullOrWhiteSpace(pathRelativo))
                return null;

            string rutaBase;
            if (raizId == RAIZ_LEGACY_ID)
            {
                rutaBase = Server.MapPath("~/Reportes/");
            }
            else
            {
                var raiz = ObtenerRaiz(raizId);
                if (raiz == null) return null;
                rutaBase = raiz.Ruta;
            }
            if (string.IsNullOrWhiteSpace(rutaBase) || !Directory.Exists(rutaBase))
                return null;

            string baseCanon = Path.GetFullPath(rutaBase).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            string combinado;
            try
            {
                combinado = Path.GetFullPath(Path.Combine(rutaBase, pathRelativo.Replace('/', Path.DirectorySeparatorChar)));
            }
            catch
            {
                return null;
            }

            if (!combinado.StartsWith(baseCanon, StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Trace.TraceWarning(
                    "Intento de traversal bloqueado: raiz='{0}' path='{1}'", raizId, pathRelativo);
                return null;
            }

            return combinado;
        }

        // Cache muy simple de la configuracion de raices (se recarga si cambia el archivo).
        private static readonly object _cfgLock = new object();
        private static ConfiguracionRaices _cfgCache;
        private static DateTime _cfgLastWrite;

        private RaizLocal ObtenerRaiz(string raizId)
        {
            string cfgPath = Server.MapPath("~/ReportesLocales/configuracion.json");
            if (!System.IO.File.Exists(cfgPath)) return null;

            DateTime lw = System.IO.File.GetLastWriteTimeUtc(cfgPath);
            lock (_cfgLock)
            {
                if (_cfgCache == null || lw != _cfgLastWrite)
                {
                    try
                    {
                        string json = System.IO.File.ReadAllText(cfgPath);
                        _cfgCache = new JavaScriptSerializer().Deserialize<ConfiguracionRaices>(json);
                        _cfgLastWrite = lw;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(
                            "Error al leer configuracion.json: {0}", ex.Message);
                        return null;
                    }
                }
            }
            if (_cfgCache == null || _cfgCache.Raices == null) return null;
            foreach (var r in _cfgCache.Raices)
                if (r != null && string.Equals(r.Id, raizId, StringComparison.OrdinalIgnoreCase))
                    return r;
            return null;
        }

        // Compatibilidad con URLs viejas: si vino solo 'archivo', tratarlo
        // como raiz "proyecto" (carpeta ~/Reportes/).
        private static void NormalizarParametros(ref string raizId, ref string path, string archivo)
        {
            if (string.IsNullOrEmpty(raizId) && string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(archivo))
            {
                raizId = RAIZ_LEGACY_ID;
                path = archivo;
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

        // Escribe el detalle tecnico del error a App_Data\errores.log.
        // Este log NO se sirve por HTTP (App_Data esta protegido por ASP.NET),
        // pero es accesible al equipo de tecnologia via acceso al filesystem.
        // Se rota manualmente si crece demasiado.
        private void RegistrarError(string archivo, Exception ex)
        {
            System.Diagnostics.Trace.TraceError(
                "Error en reporte '{0}': {1}", archivo, ex);
            try
            {
                string dir = Server.MapPath("~/App_Data/");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string ruta = Path.Combine(dir, "errores.log");
                string linea = string.Format(
                    "[{0:yyyy-MM-dd HH:mm:ss}] user={1} archivo=\"{2}\" excepcion={3} mensaje=\"{4}\"{5}",
                    DateTime.Now,
                    User.Identity.Name,
                    archivo,
                    ex.GetType().FullName,
                    ex.Message,
                    Environment.NewLine);
                System.IO.File.AppendAllText(ruta, linea);
            }
            catch
            {
                // No romper la respuesta si falla el log
            }
        }

        // ==================================================================
        // Helpers de parametros (prompts) del reporte
        // ==================================================================

        // Extrae del querystring todos los pares con prefijo "p_"
        // y los devuelve como {nombreParametro -> valor}
        private Dictionary<string, string> ExtraerValoresDelQueryString()
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var qs = Request.QueryString;
            foreach (string clave in qs.AllKeys)
            {
                if (string.IsNullOrEmpty(clave)) continue;
                if (clave.StartsWith("p_", StringComparison.OrdinalIgnoreCase))
                {
                    string nombre = clave.Substring(2);
                    string valor = qs[clave];
                    if (!string.IsNullOrEmpty(valor)) dic[nombre] = valor;
                }
            }
            return dic;
        }

        // Abre el .rpt SOLO para inspeccionar parametros (no ejecuta consulta).
        // Filtra los parametros vinculados a subreportes (no los pide el usuario final).
        private List<ParametroReporte> LeerParametros(string rutaFisica)
        {
            var lista = new List<ParametroReporte>();
            var rd = new ReportDocument();
            try
            {
                rd.Load(rutaFisica);
                foreach (ParameterField pf in rd.ParameterFields)
                {
                    // Ignorar parametros de subreportes: no los llena el usuario final
                    if (pf.ReportName != null && pf.ReportName.Length > 0) continue;

                    lista.Add(new ParametroReporte
                    {
                        Nombre = pf.Name,
                        Etiqueta = string.IsNullOrEmpty(pf.PromptText) ? pf.Name : pf.PromptText,
                        Tipo = MapearTipo(pf.ParameterValueType),
                        Opcional = pf.EnableNullValue,
                        MultiValor = pf.EnableAllowMultipleValue,
                        ValorDefecto = null
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Error al leer parametros de '{0}': {1}", rutaFisica, ex.Message);
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return lista;
        }

        // Devuelve los parametros que aun no tienen valor (para decidir si redirigir al formulario)
        private List<ParametroReporte> ParametrosFaltantes(string rutaFisica)
        {
            var parametros = LeerParametros(rutaFisica);
            var valores = ExtraerValoresDelQueryString();
            return parametros
                .Where(p => !p.Opcional
                            && !valores.ContainsKey(p.Nombre))
                .ToList();
        }

        // Aplica los valores del formulario al reporte antes de ejecutar
        private void AplicarValoresParametros(ReportDocument rd, Dictionary<string, string> valores)
        {
            if (valores == null || valores.Count == 0) return;

            foreach (ParameterField pf in rd.ParameterFields)
            {
                if (pf.ReportName != null && pf.ReportName.Length > 0) continue;
                if (!valores.ContainsKey(pf.Name)) continue;

                string bruto = valores[pf.Name];
                if (string.IsNullOrEmpty(bruto)) continue;

                try
                {
                    if (pf.EnableAllowMultipleValue && bruto.Contains(","))
                    {
                        // Multi-valor: separar por coma y setear cada uno
                        foreach (var v in bruto.Split(','))
                        {
                            object typed = ConvertirValor(v.Trim(), pf.ParameterValueType);
                            if (typed != null) rd.SetParameterValue(pf.Name, typed);
                        }
                    }
                    else
                    {
                        object typed = ConvertirValor(bruto, pf.ParameterValueType);
                        if (typed != null) rd.SetParameterValue(pf.Name, typed);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Valor invalido para parametro '{0}': '{1}' ({2})",
                        pf.Name, bruto, ex.Message);
                }
            }
        }

        private static object ConvertirValor(string bruto, ParameterValueKind tipo)
        {
            switch (tipo)
            {
                case ParameterValueKind.NumberParameter:
                case ParameterValueKind.CurrencyParameter:
                    return decimal.Parse(bruto, System.Globalization.CultureInfo.InvariantCulture);
                case ParameterValueKind.DateParameter:
                case ParameterValueKind.DateTimeParameter:
                    return DateTime.Parse(bruto, System.Globalization.CultureInfo.InvariantCulture);
                case ParameterValueKind.BooleanParameter:
                    return string.Equals(bruto, "true", StringComparison.OrdinalIgnoreCase)
                        || bruto == "1";
                default:
                    return bruto;
            }
        }

        private static string MapearTipo(ParameterValueKind k)
        {
            switch (k)
            {
                case ParameterValueKind.NumberParameter: return "Number";
                case ParameterValueKind.CurrencyParameter: return "Currency";
                case ParameterValueKind.DateParameter: return "Date";
                case ParameterValueKind.DateTimeParameter: return "DateTime";
                case ParameterValueKind.TimeParameter: return "Time";
                case ParameterValueKind.BooleanParameter: return "Boolean";
                default: return "String";
            }
        }

        private static string MensajeAmigable(Exception ex)
        {
            // Deteccion por TIPO de excepcion (mas robusto que por texto)
            string tipoExc = ex.GetType().FullName ?? "";
            if (tipoExc.Contains("LogOnException") ||
                tipoExc.Contains("DBException") ||
                tipoExc.Contains("SqlException"))
            {
                return "No fue posible conectar con la base de datos del reporte. " +
                       "Verifique que el servidor de datos este disponible y que el " +
                       "reporte tenga configuradas sus credenciales de acceso.";
            }

            string texto = (ex.Message ?? string.Empty).ToLowerInvariant();

            // Conexion a base de datos (por texto del mensaje)
            if (Contiene(texto, "conexi", "conect", "logon", "log on", "login",
                         "connection", "connect",
                         "no se pudo abrir", "no se pudo conect",
                         "cannot open", "failed to open", "failed to connect",
                         "database", "odbc", "ole db", "server does not exist"))
            {
                return "No fue posible conectar con la base de datos del reporte. " +
                       "Verifique que el servidor de datos este disponible y que el " +
                       "reporte tenga configuradas sus credenciales de acceso.";
            }

            // Parametros faltantes o invalidos
            if (Contiene(texto, "parameter", "parametro", "prompt",
                         "missing parameter", "current value",
                         "invalid value", "valor no valido"))
            {
                return "El reporte requiere parametros que no fueron proporcionados " +
                       "o cuyos valores no son validos. Verifique los datos ingresados " +
                       "y vuelva a intentar.";
            }

            // Archivo corrupto o incompatible
            if (Contiene(texto, "load report failed", "invalid report",
                         "not a valid crystal", "load failed"))
            {
                return "El archivo del reporte no pudo ser cargado. Puede estar " +
                       "corrupto o ser de una version incompatible.";
            }

            // Memoria o recursos
            if (Contiene(texto, "out of memory", "insufficient memory", "resources"))
            {
                return "El servidor no tiene recursos suficientes para generar este " +
                       "reporte en este momento. Intente nuevamente en unos minutos.";
            }

            // Fallback: mensaje generico + tipo de excepcion en corto
            // (el detalle completo queda solo en el log de servidor)
            return "Ocurrio un error al generar el reporte. Detalle tecnico registrado " +
                   "en el servidor (referencia: " + ex.GetType().Name + "). " +
                   "Contacte al area de Tecnologia si el problema persiste.";
        }

        private static bool Contiene(string texto, params string[] fragmentos)
        {
            foreach (var f in fragmentos)
                if (texto.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }
    }
}
