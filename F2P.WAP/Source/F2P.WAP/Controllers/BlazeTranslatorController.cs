using F2P.Utilitarios.Handler;
using F2P.WAP.Models;
using F2P.WAP.Models.DTO;
using F2P.WAP.Models.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;


namespace F2P.WAP.Controllers
{
    public class BlazeTranslatorController : ApiController
    {
        /// <summary>
        /// llamado al flujo de blaze 
        /// </summary>
        /// <param name="Invoke">DTO DE LLAMADO, CON DATOS NECESARIOS PARA LA INVOCACION DE BLAZE</param>
        /// <returns>Json CON IHttpActionResult</returns>
        [HttpPost]
        [Route("api/BlazeTranslator/Invoke")]
        public IHttpActionResult Invoke(DTO_BLAZE Invoke)
        {

            ////crea objeto en runtime
            //DynamicClass MCB = new DynamicClass("DTO_BLAZE");
            //var PropiertyName = Invoke.GetType().GetProperties().Select(x => x.Name).ToArray();
            //var types = Invoke.GetType().GetProperties().Select(x => x.PropertyType).ToArray();
            //var vInvoke = MCB.CreateObject(PropiertyName, types);
            ////copia las propiedades de un objeto a otro
            //ClassSetProperties.CopyProperties(Invoke, vInvoke);
            var idPais = string.Empty;
            var abreviacion = string.Empty;
            switch (Invoke.STR_COD_PAIS)
            {
                case Pais.COSTA_RICA:
                    idPais = "1";
                    abreviacion = "CR";
                    break;
                case Pais.GUATEMALA:
                    idPais = "4";
                    abreviacion = "GT";
                    break;
                case Pais.NICARAGUA:
                    idPais = "2";
                    abreviacion = "NI";
                    break;
                case Pais.SALVADOR:
                    idPais = "3";
                    abreviacion = "SV";
                    break;
                default:
                    idPais = "1";
                    abreviacion = "CR";
                    break;
            }
            GlobalClass.STR_PAIS = abreviacion;
            #region Validacion de datos
            var validaData = false;
            bool.TryParse(ConfigurationManager.AppSettings["ValidaDatosGenerales"], out validaData);

            if (validaData && Invoke.INT_SUB_ORIGEN > 1)
            {

                #region validacion de Datos de entrada por pais

                var resConfig = ProcessOneToOne.GetConfig(idPais, string.Concat(ConfigurationManager.AppSettings["LLAVE_BUSQUEDA"], abreviacion), string.Concat(ConfigurationManager.AppSettings["LLAVE02"], abreviacion));


                if (string.IsNullOrEmpty(resConfig))
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("ERROR DE CONFIGURACIÓN", System.Text.Encoding.UTF8, "application/json"),
                        ReasonPhrase = "FALTA CONFIGURACIÓN DE PARAMETROS"
                    };
                    throw new HttpResponseException(resp);
                }

                var ArrConfig = resConfig.Split('|');

                for (int i = 0; i < ArrConfig.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ArrConfig[i]))
                    {
                        var value = Invoke.GetType().GetProperty(ArrConfig[i]).GetValue(Invoke);

                        if (value != null)
                        {
                            if (string.IsNullOrEmpty(value.ToString()) || value.ToString() == "0")
                            {
                                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                                {
                                    Content = new StringContent(string.Format("FALTA PARAMETRO: {0}", ArrConfig[i], System.Text.Encoding.UTF8, "application/json")),
                                    ReasonPhrase = string.Format("FALTA PARAMETRO: {0}", ArrConfig[i])
                                };
                                throw new HttpResponseException(resp);
                            }
                        }
                        else
                        {
                            var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                            {
                                Content = new StringContent(string.Format("FALTA PARAMETRO: {0}", ArrConfig[i]), System.Text.Encoding.UTF8, "application/json"),
                                ReasonPhrase = string.Format("FALTA PARAMETRO: {0}", ArrConfig[i])
                            };
                            throw new HttpResponseException(resp);
                        }
                    }
                }

                #endregion

                var regexFlag = false;
                var expresion = string.Empty;
                switch (Invoke.STR_COD_PAIS)
                {
                    case Pais.COSTA_RICA:
                        regexFlag = bool.Parse(ConfigurationManager.AppSettings["Flag_Identificacion_CR"]);
                        expresion = ConfigurationManager.AppSettings["Identificacion_CR"];
                        break;
                    case Pais.GUATEMALA:
                        regexFlag = bool.Parse(ConfigurationManager.AppSettings["Flag_Identificacion_GT"]);
                        expresion = ConfigurationManager.AppSettings["Identificacion_GT"];
                        break;
                    case Pais.NICARAGUA:
                        regexFlag = bool.Parse(ConfigurationManager.AppSettings["Flag_Identificacion_NI"]);
                        expresion = ConfigurationManager.AppSettings["Identificacion_NI"];
                        break;
                    case Pais.SALVADOR:
                        regexFlag = bool.Parse(ConfigurationManager.AppSettings["Flag_Identificacion_SV"]);
                        expresion = ConfigurationManager.AppSettings["Identificacion_SV"];
                        break;
                }

                //valida si usa Expresiones regulares
                if (regexFlag && !string.IsNullOrEmpty(expresion))
                {
                    Regex regex = new Regex(expresion);
                    Match match = regex.Match(Invoke.STR_CEDULA);

                    if (!match.Success)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent(string.Format("IDENTIFICACIÓN INVALIDA: {0}", Invoke.STR_CEDULA, System.Text.Encoding.UTF8, "application/json")),
                            ReasonPhrase = string.Format("IDENTIFICACIÓN INVALIDA: {0}", Invoke.STR_CEDULA)
                        };
                        throw new HttpResponseException(resp);
                    }
                }
            }
            #endregion

            #region Encabezado de Objetos Enviado

            var infDto = new InfoClass
            {
                ApplicationCode = "1",
                Consecutivo = default(short),
                PaisCode = idPais,
                IdentificadorUnico = Guid.NewGuid().ToString(),
                BIN_FK_BOM_LOG_OPERACIONES = string.Empty
            };

            int pais;
            int.TryParse(infDto.PaisCode, out pais);

            #endregion

            #region [Data Trace]

            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                                          MethodBase.GetCurrentMethod().Module.Name,
                                          MethodBase.GetCurrentMethod().DeclaringType.Name);
            string xProceso = MethodBase.GetCurrentMethod().Name;


            var valuess = (from property in Invoke.GetType().GetProperties()
                           select string.Concat(property.Name, "=", property.GetValue(Invoke)));

            string xParametros = string.Empty;

            // var methodParams = MethodBase.GetCurrentMethod().GetParameters();

            xParametros = string.Join(",", valuess);

            xParametros = string.Format("({0})", xParametros);



            var xTraceDto = new TraceDto
            {
                LOG_TYPE = LogType.Trace,
                FEC_SISTEMA = DateTime.Now,
                INT_CONSECUTIVO = infDto.Consecutivo,
                STR_IDENTIFICADOR_EXTERNO = infDto.IdentificadorUnico,
                STR_APLICATIVO = xAplicativo,
                STR_CLASE = xClase,
                STR_PROCESO = xProceso,
                INT_ID_ORIGEN = Convert.ToInt32(ID_ORIGEN.WEB_SERVICE),
                STR_PARAMETROS = xParametros,
                SIN_FK_UTL_PAR_PAIS = pais,
            };


            var xStringBuilder = new StringBuilder();

            DateTime oldDate = DateTime.Now;
            xStringBuilder.AppendLine(new string('-', 40) + string.Format("[INICIO-EJECUCION PAIS [ " + GlobalClass.STR_PAIS + "]]***************[{0}]", oldDate) + new string('-', 40));

            xStringBuilder.AppendLine(new string('-', 40) + "[INICIO]" + new string('-', 40));
            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [Inicio | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7}]",
                    DateTime.Now,
                    infDto.IdentificadorUnico,
                    infDto.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    xParametros,
                    String.Empty));
            xTraceDto.STR_MENSAJE = xStringBuilder.ToString();

            #endregion

            var data = new JObject();

            try
            {
                Invoke.BIN_ID_SOLICITUD_REF = Invoke.BIN_ID_SOLICITUD_REF == null ? 0 : Invoke.BIN_ID_SOLICITUD_REF;
                data = ProcessOneToOne.ProcessRequest(infDto, Invoke, ref xStringBuilder);
            }
            catch (BuroExceptions xException)
            {
                #region [Manejo de Errores]

                xProceso = xException.Message.Split('|')[2];
                string bancarioIngresado = xException.Message.Split('|').Length > 4 ? xException.Message.Split('|')[4] : "0";

                var res = ProcessOneToOne.GetCodigosRetorno(xProceso, Invoke.INT_SUB_ORIGEN.ToString(), Invoke.SIN_FACE.ToString(), xException.Message.Split('|')[0], infDto.STR_COD_PAIS);

                string xMensajeError = string.Format(
                       "<F2P_FILE><TRANSACTION_RESPONSE><RESPONSE_CODE>{0}</RESPONSE_CODE><RESPONSE_DESC>{1}</RESPONSE_DESC><DATETIME_TRANSACTION>{2}</DATETIME_TRANSACTION><USUARIO_CONSULTA>{3}</USUARIO_CONSULTA></TRANSACTION_RESPONSE><MENSAJES_BLAZE><MENSAJE><CODIGO /><FEC_CREACION /><STR_IDENTIFICACION /><TIPO /><MENSAJE_CREDITO>{4}</MENSAJE_CREDITO><MENSAJE_VENTAS>{4}</MENSAJE_VENTAS></MENSAJE></MENSAJES_BLAZE><OTROS_DATOS_FLAG><CAMBIO_PRODUCTO>0</CAMBIO_PRODUCTO><PANTALLA_OFER_CREDIT>0</PANTALLA_OFER_CREDIT><BIT_BANCARIO_INGRESADO>{5}</BIT_BANCARIO_INGRESADO></OTROS_DATOS_FLAG></F2P_FILE>"
                       , res.FirstOrDefault().CODIGO, res.FirstOrDefault().MENSAJE
                       , DateTime.Now, Invoke.STR_USUARIO_CREACION, res.FirstOrDefault().MENSAJE
                       ,bancarioIngresado
                       );

                //CONVERSION DEL XML EN UN OBJETO
                XmlDocument dat = new XmlDocument();
                dat.LoadXml(xMensajeError);

                data = JObject .Parse(JsonConvert.SerializeObject(dat));

                //data = xMensajeError;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error  | Proyecto: {2} | Clase: {3} | Metodo: {4} | Parametros: {5} | Detalle: {6}",
                        DateTime.Now,
                        infDto.IdentificadorUnico,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xMensajeError));

                xTraceDto.LOG_TYPE = LogType.Error;
                xTraceDto.FEC_SISTEMA = DateTime.Now;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.STR_OBSERVACION = xMensajeError;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.SIN_FK_UTL_PAR_PAIS = pais;
                xTraceDto.STR_IDENTIFICADOR_UNICO = infDto.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = infDto.IdentificadorUnico;
                xTraceDto.STR_APLICATIVO = xAplicativo;
                xTraceDto.STR_ESTACION = Environment.MachineName;
                xTraceDto.STR_USUARIO_CREACION = infDto.STR_USUARIO_CREACION;
                xTraceDto.STR_MENSAJE_SISTEMA = xException.Message.Split('|')[1];
                xTraceDto.STR_DETALLE = xException.Message.Split('|')[0];
                xTraceDto.STR_PROCESO = xProceso;
                ExceptionsClass.WriteLog(xTraceDto);

                #endregion
            }
            catch (Exception xException)
            {
                #region [Manejo de Errores]
                infDto.Consecutivo = 999;
                string xMensajeError = string.Format("( ErrorMessage:{0} | StackTrace:{1} )", xException.Message,
                                                     xException.StackTrace);

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error  | Proyecto: {2} | Clase: {3} | Metodo: {4} | Parametros: {5} | Detalle: {6}",
                        DateTime.Now,
                        infDto.IdentificadorUnico,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xMensajeError));

                xTraceDto.LOG_TYPE = LogType.Error;
                xTraceDto.FEC_SISTEMA = DateTime.Now;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.STR_OBSERVACION = xMensajeError;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.SIN_FK_UTL_PAR_PAIS = pais;
                xTraceDto.STR_IDENTIFICADOR_UNICO = infDto.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = infDto.IdentificadorUnico;
                xTraceDto.STR_APLICATIVO = xAplicativo;
                xTraceDto.STR_ESTACION = Environment.MachineName;
                xTraceDto.STR_USUARIO_CREACION = infDto.STR_USUARIO_CREACION;
                xTraceDto.STR_MENSAJE_SISTEMA = xException.Message;
                ExceptionsClass.WriteLog(xTraceDto);

                #endregion
            }


            finally
            {
                #region [Data Trace]

                xTraceDto.LOG_TYPE = LogType.Trace;
                xTraceDto.FEC_SISTEMA = DateTime.Now;
                xTraceDto.INT_CONSECUTIVO = infDto.Consecutivo;
                xTraceDto.SIN_FK_UTL_PAR_PAIS = pais;
                xTraceDto.STR_IDENTIFICADOR_UNICO = infDto.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = infDto.IdentificadorUnico;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Fin PAIS [ " + GlobalClass.STR_PAIS + " ] | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7}]",
                        DateTime.Now,
                        infDto.IdentificadorUnico,
                        infDto.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        String.Empty));

                xStringBuilder.AppendLine(new string('-', 40) + "[FIN]" + new string('-', 40));
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.STR_IDENTIFICADOR_UNICO = infDto.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = infDto.IdentificadorUnico;
                xTraceDto.STR_APLICATIVO = xAplicativo;
                xTraceDto.STR_ESTACION = Environment.MachineName;
                xTraceDto.STR_USUARIO_CREACION = infDto.STR_USUARIO_CREACION;

                xStringBuilder.AppendLine(new string('-', 40) + "[FIN]" + new string('-', 40));

                DateTime newDate = DateTime.Now;
                // Difference in days, hours, and minutes.
                TimeSpan ts = newDate - oldDate;

                xStringBuilder.AppendLine(new string('-', 40) + string.Format("[FIN-EJECUCION PAIS [ " + GlobalClass.STR_PAIS + " ]]***************[FINALIZA: {0},TOTAL SEGUNDOS: {1}]", newDate, ts.TotalSeconds) + new string('-', 40));

                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();

                ExceptionsClass.WriteLog(xTraceDto);

                //GlobalClass.connectionString = string.Empty;
                //GlobalClass.blazeUri = string.Empty;

                #endregion
            }
            if (data.Count==0)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("LA IDENTIFICACIÓN: {0}", Invoke.STR_CEDULA), System.Text.Encoding.UTF8, "application/json"),
                    ReasonPhrase = string.Format("IDENTIFICACIÓN: {0} EXTRAVIADA CONTACTE AL ADMINISTRADOR", Invoke.STR_CEDULA)
                };
                throw new HttpResponseException(resp);
            }

            return Json(data);
        }

        /// <summary>
        /// llama a configuracion de datos por pais
        /// </summary>
        /// <param name="CONF"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/BlazeTranslator/InvokeConfig")]
        public IHttpActionResult InvokeConfig(DTO_CONFIG CONF)
        {
            var res = ProcessOneToOne.GetConfig(CONF.P_STR_LLAVE_01, CONF.P_STR_LLAVE_02, CONF.P_STR_LLAVE_03, CONF.P_STR_LLAVE_04, CONF.P_STR_LLAVE_05);

            return Ok(res);
        }
        /// <summary>
        /// valida a donde esta el servidor
        /// </summary>
        /// <param name="InvokeMember"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BlazeTranslator/ValidateService")]
        public IHttpActionResult ValidateService([CallerMemberName]string InvokeMember = "")
        {
            return Ok(ContextUtility.ValidateService(InvokeMember));
        }

    }

}
