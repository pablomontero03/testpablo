using F2P.Utilitarios.Handler;
using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace F2P.WAP.Models.Utility
{
    internal class CoreClass
    {
        internal static void CoreInvoke(InfoClass xInfoClass, Solicitud xSolicitud, ref string xDatos, ref StringBuilder xStringBuilder)
        {
            #region [Data Trace]

            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                                          MethodBase.GetCurrentMethod().Module.Name,
                                          MethodBase.GetCurrentMethod().DeclaringType.Name);
            string xProceso = MethodBase.GetCurrentMethod().Name;
            string xParametros = string.Empty;

            ParameterInfo[] methodParams = MethodBase.GetCurrentMethod().GetParameters();
            for (int i = 0; i < methodParams.Length; i++)
            {
                xParametros = xParametros +
                              string.Format("Parametro [{0}] = {1} |", i.ToString(CultureInfo.InvariantCulture),
                                            methodParams[i]);
            }

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE CoreInvoke | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    xSolicitud.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion

        }

        internal void CoreInvoke(InfoClass infDto, Solicitud solicitud)
        {
            var xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();

            CoreInvoke(infDto, solicitud, ref xDatos, ref xStringBuilder);

        }
    }
}