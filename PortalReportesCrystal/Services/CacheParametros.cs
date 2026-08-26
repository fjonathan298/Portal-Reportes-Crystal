// ============================================================================
// CacheParametros.cs - CACHE DE ANALISIS DE PARAMETROS POR REPORTE
// ============================================================================
// Detectar si un .rpt tiene parametros requiere cargar el archivo con el SDK
// (unos 100-300 ms cada uno). Con cientos de reportes es inviable hacerlo en
// cada peticion del listado.
//
// Este cache resuelve el problema:
//   - Guarda por cada ruta absoluta: {tieneParametros, cantidad, ultimaMod}
//   - Se persiste en App_Data/parametros_cache.json entre reinicios
//   - Se invalida por entrada cuando el archivo .rpt cambia (LastWriteTime)
//   - El escaneo inicial se dispara en background desde Application_Start
//     para no bloquear el arranque del portal
//
// Uso:
//   CacheParametros.Analizar(rutaAbsoluta)  -> bool? (null si aun no analizado)
//   CacheParametros.IniciarEscaneoBackground(raices)
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace PortalReportesCrystal.Services
{
    public static class CacheParametros
    {
        private class Entrada
        {
            public bool TieneParametros { get; set; }
            public int Cantidad { get; set; }
            public long UltimaModTicks { get; set; }
        }

        // Diccionario thread-safe para escritura desde el background scanner
        private static readonly ConcurrentDictionary<string, Entrada> _mapa
            = new ConcurrentDictionary<string, Entrada>(StringComparer.OrdinalIgnoreCase);

        private static string _rutaCache;
        private static readonly object _guardarLock = new object();
        private static bool _cargado;
        private static volatile bool _escaneando;

        public static bool EscaneoEnProgreso { get { return _escaneando; } }
        public static int TotalCacheados { get { return _mapa.Count; } }

        // Devuelve:
        //   true  -> el reporte tiene parametros
        //   false -> no tiene
        //   null  -> aun no analizado (cache aun no lo tiene)
        //
        // NUNCA carga el .rpt sincronicamente para no bloquear el listado.
        public static bool? Analizar(string rutaAbsoluta)
        {
            if (string.IsNullOrEmpty(rutaAbsoluta)) return null;

            Entrada entrada;
            if (!_mapa.TryGetValue(rutaAbsoluta, out entrada))
                return null;

            // Validar que el archivo no se haya modificado desde el cache
            try
            {
                long actual = File.GetLastWriteTimeUtc(rutaAbsoluta).Ticks;
                if (actual != entrada.UltimaModTicks)
                {
                    Entrada _tmp;
                    _mapa.TryRemove(rutaAbsoluta, out _tmp);
                    return null;
                }
            }
            catch
            {
                return null;
            }

            return entrada.TieneParametros;
        }

        // Carga el cache desde disco (llamar una vez al iniciar)
        public static void Inicializar(string rutaAppData)
        {
            if (_cargado) return;
            _cargado = true;

            try
            {
                if (!Directory.Exists(rutaAppData))
                    Directory.CreateDirectory(rutaAppData);
                _rutaCache = Path.Combine(rutaAppData, "parametros_cache.json");

                if (File.Exists(_rutaCache))
                {
                    string json = File.ReadAllText(_rutaCache);
                    var dic = new JavaScriptSerializer().Deserialize<System.Collections.Generic.Dictionary<string, Entrada>>(json);
                    if (dic != null)
                    {
                        foreach (var kv in dic)
                            _mapa[kv.Key] = kv.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "CacheParametros.Inicializar: {0}", ex.Message);
            }
        }

        // Escanea en BACKGROUND todos los .rpt bajo las rutas dadas.
        // No bloquea el hilo llamante. Cuando termina, persiste el cache a disco.
        public static void IniciarEscaneoBackground(System.Collections.Generic.IEnumerable<string> rutasRaices)
        {
            if (_escaneando) return;
            _escaneando = true;

            Task.Run(() =>
            {
                try
                {
                    foreach (var raiz in rutasRaices)
                    {
                        if (string.IsNullOrEmpty(raiz) || !Directory.Exists(raiz))
                            continue;

                        foreach (var archivo in Directory.EnumerateFiles(raiz, "*.rpt", SearchOption.AllDirectories))
                        {
                            AnalizarArchivo(archivo);
                        }
                    }

                    Guardar();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "Error en escaneo de cache: {0}", ex.Message);
                }
                finally
                {
                    _escaneando = false;
                }
            });
        }

        private static void AnalizarArchivo(string archivo)
        {
            try
            {
                long modActual = File.GetLastWriteTimeUtc(archivo).Ticks;

                Entrada existente;
                if (_mapa.TryGetValue(archivo, out existente) && existente.UltimaModTicks == modActual)
                    return; // ya cacheado y sin cambios

                var rd = new ReportDocument();
                try
                {
                    rd.Load(archivo);
                    int cuenta = 0;
                    foreach (ParameterField pf in rd.ParameterFields)
                    {
                        // Ignorar parametros de subreportes
                        if (pf.ReportName != null && pf.ReportName.Length > 0) continue;
                        cuenta++;
                    }

                    _mapa[archivo] = new Entrada
                    {
                        TieneParametros = cuenta > 0,
                        Cantidad = cuenta,
                        UltimaModTicks = modActual
                    };
                }
                finally
                {
                    rd.Close();
                    rd.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "No se pudo analizar '{0}': {1}", archivo, ex.Message);
            }
        }

        private static void Guardar()
        {
            if (string.IsNullOrEmpty(_rutaCache)) return;
            try
            {
                lock (_guardarLock)
                {
                    var dic = new System.Collections.Generic.Dictionary<string, Entrada>(_mapa);
                    string json = new JavaScriptSerializer().Serialize(dic);
                    File.WriteAllText(_rutaCache, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "CacheParametros.Guardar: {0}", ex.Message);
            }
        }
    }
}
