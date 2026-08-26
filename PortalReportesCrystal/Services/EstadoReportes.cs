// ============================================================================
// EstadoReportes.cs - REGISTRO DE ULTIMO ESTADO DE EJECUCION POR REPORTE
// ============================================================================
// Marca los reportes que fallaron al abrirse para que aparezcan senalizados
// en el listado. La marca se limpia automaticamente cuando el reporte se abre
// con exito. Persistencia en App_Data/estado_reportes.json.
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace PortalReportesCrystal.Services
{
    public static class EstadoReportes
    {
        public class Estado
        {
            public bool ConError { get; set; }
            public string Mensaje { get; set; }        // mensaje breve (para tooltip UI)
            public string FechaIso { get; set; }       // ISO 8601 de la ultima falla
            public string Usuario { get; set; }        // quien la observo
            public int Repeticiones { get; set; }      // veces que se repitio sin exito intermedio
        }

        private static readonly ConcurrentDictionary<string, Estado> _mapa
            = new ConcurrentDictionary<string, Estado>(StringComparer.OrdinalIgnoreCase);
        private static string _rutaCache;
        private static readonly object _lockGuardar = new object();
        private static bool _inicializado;

        public static void Inicializar(string rutaAppData)
        {
            if (_inicializado) return;
            _inicializado = true;
            try
            {
                if (!Directory.Exists(rutaAppData))
                    Directory.CreateDirectory(rutaAppData);
                _rutaCache = Path.Combine(rutaAppData, "estado_reportes.json");
                if (File.Exists(_rutaCache))
                {
                    string json = File.ReadAllText(_rutaCache);
                    var dic = new JavaScriptSerializer().Deserialize<Dictionary<string, Estado>>(json);
                    if (dic != null)
                        foreach (var kv in dic)
                            _mapa[kv.Key] = kv.Value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "EstadoReportes.Inicializar: {0}", ex.Message);
            }
        }

        public static Estado Obtener(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return null;
            Estado e;
            return _mapa.TryGetValue(clave, out e) ? e : null;
        }

        // Marca al reporte como fallido. Si ya estaba, incrementa contador.
        public static void RegistrarError(string clave, string mensajeBreve, string usuario)
        {
            if (string.IsNullOrEmpty(clave)) return;
            Estado prev;
            int rep = _mapa.TryGetValue(clave, out prev) && prev.ConError ? prev.Repeticiones + 1 : 1;
            _mapa[clave] = new Estado
            {
                ConError = true,
                Mensaje = Truncar(mensajeBreve, 200),
                FechaIso = DateTime.Now.ToString("s"),
                Usuario = usuario,
                Repeticiones = rep
            };
            GuardarAsync();
        }

        // Limpia el estado de error de un reporte cuando se abre con exito.
        public static void RegistrarExito(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return;
            Estado _tmp;
            if (_mapa.TryRemove(clave, out _tmp))
                GuardarAsync();
        }

        // Construye la clave estable de un reporte a partir de raizId + path relativo.
        // Debe coincidir con lo que usa el listado principal.
        public static string ClaveDeLocal(string raizId, string pathRel)
        {
            return "local:" + (raizId ?? "").ToLowerInvariant() + "/" + (pathRel ?? "").Replace('\\', '/');
        }

        private static string Truncar(string s, int max)
        {
            if (s == null) return null;
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        // Guardar en un thread aparte para no bloquear la peticion HTTP.
        private static void GuardarAsync()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (string.IsNullOrEmpty(_rutaCache)) return;
                    lock (_lockGuardar)
                    {
                        var dic = new Dictionary<string, Estado>(_mapa);
                        var json = new JavaScriptSerializer().Serialize(dic);
                        File.WriteAllText(_rutaCache, json);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "EstadoReportes.Guardar: {0}", ex.Message);
                }
            });
        }
    }
}
