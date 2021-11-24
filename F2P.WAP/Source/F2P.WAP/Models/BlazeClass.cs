using F2P.Utilitarios.Common.DataBase;
using F2P.Utilitarios.DataAccess;
using F2P.Utilitarios.Handler;
using F2P.WAP.Models.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace F2P.WAP.Models
{
    internal class BlazeClass
    {
        private static ConcurrentDictionary<XmlDocument, XmlDocument> doc = new ConcurrentDictionary<XmlDocument, XmlDocument>();
        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de Blaze
        /// </summary>
        /// <param name="xInfoClass"></param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="xDatos"></param>
        /// <param name="xStringBuilder"></param>
        internal static void BlazeInvoke(InfoClass xInfoClass, Solicitud xSolicitud, ref string xDatos,
            ref StringBuilder xStringBuilder)
        {

            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xProceso = MethodBase.GetCurrentMethod().Name;

                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));


                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO DE BlazeInvoke | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion

                bool xLogEnable;
                bool bit_gestion = true;
                //Se realiza cambio para crear nuevo ID UNICO
                xInfoClass.IdentificadorUnico = Guid.NewGuid().ToString();


                Boolean.TryParse(ConfigurationManager.AppSettings["logEnabledInBlaze"], out xLogEnable);

                /* INICIALIZO EL DATO UNICO */
                var blazeInpute = new input { solicitud = xSolicitud, logEnabled = xLogEnable };
                BlazeOutput blazeOutput;

                blazeInpute.solicitud.CodigoRespuesta = EnumCodigoRespuesta.Aprovada;
                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA A BLAZE [{0}] -------------------------------------------------------***", oldDate));

                //xStringBuilder.AppendLine(string.Format("*** {0} ***", DateTime.Now));

                #region Web Services new

                #region Serializacion de Datos

                var doc = new XmlDocument();

                doc.LoadXml(ConvertClass.SerializeObject(blazeInpute));

                #endregion

                #region [Data Trace]

                bool flag;

                if (Boolean.TryParse(ConfigurationManager.AppSettings["EnableDataBase_Solicitud_Antes"], out flag))
                {
                    #region Informacion para el SP

                    var xDynamicDtoTemp = new DynamicDto
                    {
                        ParameterList = new List<SpParameter>
                        {
                            new SpParameter
                            {
                                Name = "INT_PK_BOM_PAR_APLICATIVO",
                                Value = xInfoClass.ApplicationCode
                            },
                            new SpParameter
                            {
                                Name = "BIN_ID_PERSONA",
                                Value = xSolicitud.Id
                            },
                            new SpParameter
                            {
                                Name = "SIN_FK_UTL_PAR_PAIS",
                                Value = "1"
                            },
                            new SpParameter
                            {
                                Name = "STR_IDENTIFICADOR_EXTERNO",
                                Value = xInfoClass.IdentificadorUnico
                            },
                            new SpParameter
                            {
                                Name = "STR_IDENTIFICADOR_UNICO",
                                Value = xInfoClass.IdentificadorUnico
                            },
                            new SpParameter
                            {
                                Name = "XML_DATA_ENVIADA",
                                Value = ConvertClass.SerializeObject(blazeInpute)
                            },
                            new SpParameter
                            {
                                Name = "INT_FK_BOM_PAR_APLICATIVO",
                                Value = xInfoClass.ApplicationCode
                            },
                            new SpParameter
                            {
                                Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                                Value = xSolicitud.IdentificadorUnicoGestion
                            },
                            new SpParameter
                            {
                                Name = "INT_FLUJO",
                                Value = xSolicitud.Flujo.ToString(CultureInfo.InvariantCulture)
                            },
                            new SpParameter
                            {
                                Name = "TIN_TIPO_DATA",
                                Value = "5"
                            },
                             new SpParameter
                            {
                                Name = "BIN_ID_BASE",
                                Value = xSolicitud.Id_Base
                            },
                            new SpParameter
                            {
                                Name = "TIN_ORIGEN",
                                Value = xInfoClass.TIN_ORIGEN
                            },
                            new SpParameter
                                {
                                    Name = "BIN_ID_SOLICITUD",
                                    Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                                },
                            new SpParameter
                            {
                                Name = "BIN_FK_BOM_LOG_OPERACIONES",
                                Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                            },
                            new SpParameter
                            {
                                Name = "BIN_ID_SOLICITUD_REF",
                                Value = xInfoClass.BIN_ID_SOLICITUD_REF.ToString()
                            },
                            new SpParameter
                            {
                                Name = "INT_SUB_ORIGEN",
                                Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                            }
                              ,
                            new SpParameter
                            {
                                Name = "STR_USUARIO_CREACION",
                                Value = xInfoClass.STR_USUARIO_CREACION
                            }
                            },
                        Result = null,
                        SPName = "PA_PRO_BOM_LOG_OPERACIONES"
                    };

                    #endregion

                    DynamicSqlDAO.ExecuterSp(xDynamicDtoTemp, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                }

                if (Boolean.TryParse(ConfigurationManager.AppSettings["EnableXMLFile_Blaze_Antes"], out flag))
                {

                    var path = Path.Combine(ConfigurationManager.AppSettings["EnableXMLFilePath_Blaze_Antes"], string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));

                    FileClass.CreateLogFile(
                        xContent: ConvertClass.SerializeObject(blazeInpute),
                        xFileName: string.Format("Antes_{0}_{1}_.xml", blazeInpute.solicitud.Id, DateTime.Now.ToFileTimeUtc()),
                        xPath: path);
                }

                #endregion

                XmlElement objXml;


                #region Invocacion al Blaze
                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA LLAMADA WSBLAZE [{0}] -------------------------------------------------------***", oldDate));

                using (var blaze = new BlazeService())
                {
                    blaze.Url = GlobalClass.blazeUri.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value;

                    var jsonXml = JsonConvert.SerializeXmlNode(doc);
                    var blazeOutputXml = JsonConvert.DeserializeXmlNode(blaze.BlazeInvoke(jsonXml));
                    XDocument xdoc = XDocument.Parse(blazeOutputXml.OuterXml);
                    xdoc.Descendants().Where(node => (string)node.Attribute("nil") == "true").Remove();
                    xStringBuilder.AppendLine(string.Format("***RETORNO BLAZE{0}***", xdoc.ToString()));
                    doc.LoadXml(xdoc.ToString());
                    blazeOutput = ConvertClass.DeserializeFromXmlElement<BlazeOutput>(doc.DocumentElement); //JsonConvert.DeserializeObject<BlazeOutput>(blazeOutputXml.InnerXml);//JsonConvert.DeserializeObject<BlazeOutput>(JsonConvert.SerializeXmlNode(blazeOutputXml));;
                    objXml = (XmlElement)blazeOutputXml.ChildNodes[0];
                    xStringBuilder.AppendLine(string.Format("***URL llamado Blaze: {0} Pais:{1} Identificacion: {2}***", blaze.Url, xInfoClass.STR_COD_PAIS, xSolicitud.Persona.ListadoIdentificaciones.FirstOrDefault().Valor));
                }
                DateTime newDate2 = DateTime.Now;
                TimeSpan ts2 = newDate2 - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA WSBLAZE [{0}] -------------------------------------------------------***", newDate2));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts2.TotalSeconds));

                #endregion

                // Fecha: 13/02/2016, Mdailey 
                blazeOutput.xmlResponse = objXml != null ?
                    string.Format("<BlazeOutput>{0}</BlazeOutput>",
                        objXml.InnerXml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"",
                            String.Empty).Replace("xsi:nil=\"true\"", String.Empty))
            : ConvertClass.SerializeObject(blazeOutput).Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"",
                            String.Empty)
                        .Replace("xsi:nil=\"true\"", String.Empty);
                // Fecha: 13/02/2016, Mdailey 



                switch (xInfoClass.STR_COD_PAIS)
                {
                    case Pais.GUATEMALA:
                        var resLista = new List<VARIABLE>();
                        if (!string.IsNullOrEmpty(xInfoClass.xmlObjetSentSOC))
                        {
                            //CONVERSION DEL XML EN UN OBJETO
                            doc = new XmlDocument();
                            doc.LoadXml(xInfoClass.xmlObjetSentSOC);

                            var objBlaze = JsonConvert.DeserializeObject<RootObject>(JsonConvert.SerializeObject(doc)); //ConvertClass.StringToObject<DTO_BLAZE>(xInfoClass.xmlObjetSentSOC);

                            VARIABLE objVar = new VARIABLE();

                            if (objBlaze.DTO_BLAZE.STR_VARIABLE != null)
                            {
                                resLista = (from VARIABLE list in objBlaze.DTO_BLAZE.STR_VARIABLE
                                            where list.STR_TIPO.Equals("ANTIGUEDADDOMICILIO")
                                            select list).ToList();
                                if (resLista.Any())

                                {
                                    objBlaze.DTO_BLAZE.STR_VARIABLE.Remove(resLista.FirstOrDefault());
                                }

                                var info = (from data in blazeOutput.ListadoVariables select data);
                                try
                                {
                                    if (info.Any())
                                    {
                                        objVar.STR_TIPO = "ANTIGUEDADDOMICILIO";
                                        var marca = info.Where(x => x.Llave.Equals("TipoFlujo")).FirstOrDefault().Valor;
                                        int? valNull = null;
                                        objVar.STR_VALOR = info.Where(x => x.Llave.Equals("Antiguedade Residencia en meses")).FirstOrDefault().Valor;
                                        objVar.STR_DESCRIPCION = info.Where(x => x.Llave.Equals("Antiguedade Residencia")).FirstOrDefault().Valor;
                                        xInfoClass.INT_ID_MARCA = !marca.Any() ? valNull : int.Parse(marca.ToString());
                                    }
                                    else
                                    {
                                        objVar.STR_VALOR = resLista.FirstOrDefault().STR_VALOR;
                                        objVar.STR_DESCRIPCION = resLista.FirstOrDefault().STR_DESCRIPCION;
                                    }
                                }
                                catch
                                {
                                    //objVar.STR_VALOR = resLista.FirstOrDefault().STR_VALOR;
                                    //objVar.STR_DESCRIPCION = resLista.FirstOrDefault().STR_DESCRIPCION;
                                }
                                objBlaze.DTO_BLAZE.STR_VARIABLE.Add(objVar);
                                //xInfoClass.xmlObjetSentSOC = ConvertClass.SerializeObject(objBlaze);
                            }
                        }
                        break;
                    case Pais.NICARAGUA:
                        xInfoClass.xmlObjetSentSOC = blazeOutput.xmlResponse;
                        break;
                }



                /* BITACORA DE SEGIMINETO PARA LA GENRACION DE REPORTE (TIPO_DATO = 6 ES OBLIGATORIO) */

                #region Informacion para el SP

                var xDynamicDtoTemp2 = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_PK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_PERSONA",
                        Value = xSolicitud.Id
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = /*xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrBlaze
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrCore
                                     ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrRenap
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrEquifax
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrTuca
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta
                                 ||
                        xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Segunda_Consulta
                                ? Guid.NewGuid().ToString()
                                :*/
                                xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrBlaze
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrCore
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrRenap
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrEquifax
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrTuca
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta
                                ||
                                xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Segunda_Consulta
                                ? Guid.NewGuid().ToString()
                                : xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA_ENVIADA",
                        Value = blazeOutput.xmlResponse
                    },
                    new SpParameter
                    {
                        Name = "INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    }
                    ,
                    new SpParameter
                    {
                        Name = "INT_FLUJO",
                        Value = xSolicitud.Flujo.ToString(CultureInfo.InvariantCulture)
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                        Value = xSolicitud.IdentificadorUnicoGestion
                    },
                    new SpParameter
                    {
                        Name = "TIN_TIPO_DATA",
                        Value = "6"
                    },
                    new SpParameter
                    {
                        Name = "TIN_ORIGEN",
                        Value = xInfoClass.TIN_ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_BASE",
                        Value = xSolicitud.Id_Base
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                    },
                            new SpParameter
                            {
                                Name = "BIT_GESTION",
                                Value = bit_gestion.ToString()
                            }
                            ,
                            new SpParameter
                            {
                                Name = "INT_SUB_ORIGEN",
                                Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                            },
                            new SpParameter
                            {
                                Name = "BIN_ID_SOLICITUD_REF",
                                Value = xInfoClass.BIN_ID_SOLICITUD_REF.ToString()
                            },
                            new SpParameter
                            {
                                Name = "STR_USUARIO_CREACION",
                                Value = xInfoClass.STR_USUARIO_CREACION
                            },
                            new SpParameter
                            {
                                Name = "STR_TEL_CELULAR",
                                Value = xInfoClass.STR_TEL_CELULAR
                            },
                            new SpParameter
                            {
                                Name = "STR_EMAIL",
                                Value = xInfoClass.STR_EMAIL
                            }
                },
                    Result = null,
                    SPName = "PA_PRO_BOM_LOG_OPERACIONES"
                };


                if (xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrBlaze
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrCore
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrEquifax
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrTuca
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Segunda_Consulta
                    ||
                    xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.IrRenap)
                {
                    // No Hace Nada 
                }
                else
                {
                    if (Boolean.TryParse(ConfigurationManager.AppSettings["EnableXMLFile_Blaze_Despues"], out flag))
                    {
                        string path = Path.Combine(ConfigurationManager.AppSettings["EnableXMLFilePath_Blaze_Despues"],
                            string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));

                        FileClass.CreateLogFile(ConvertClass.SerializeObject(blazeOutput),
                            string.Format("Despues_{0}_{1}_.xml", blazeOutput.solicitud.Id, DateTime.Now.ToFileTimeUtc()),
                            path);
                    }
                    DateTime oldDate1 = DateTime.Now;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", xDynamicDtoTemp2.SPName));

                    //Guarda la respuesta 
                    DynamicSqlDAO.ExecuterSp(xDynamicDtoTemp2, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                    DateTime newDate1 = DateTime.Now;
                    TimeSpan ts1 = newDate1 - oldDate1;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", xDynamicDtoTemp2.SPName));
                    xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts1.TotalSeconds));

                }

                #endregion

                #endregion
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA A BLAZE [{0}] -------------------------------------------------------***", newDate));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

                //xStringBuilder.AppendLine(string.Format("*** SALIDA A BLAZE ***"));
                //xStringBuilder.AppendLine(string.Format("*** {0} ***", DateTime.Now));
                if (xInfoClass.BIT_BURO_BANCARIO && xInfoClass.BIT_LLAMADO_BANCARIO && blazeOutput.solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta)
                {
                    blazeOutput.solicitud.CodigoRespuesta = EnumCodigoRespuesta.Aprovada;
                    blazeOutput.xmlResponse = blazeOutput.xmlResponse.Replace(EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta.ToString(), EnumCodigoRespuesta.Aprovada.ToString());
                }
                    

                #region Decisiones del Codigo de Respuesta
                switch (blazeOutput.solicitud.CodigoRespuesta)
                {
                    case EnumCodigoRespuesta.IrCore:
                        /*Invoca al Corre y la clase vuelve a llamar a BlazeInvoke*/
                        CoreClass.CoreInvoke(xInfoClass, blazeOutput.solicitud, ref xDatos, ref xStringBuilder);
                        break;

                    case EnumCodigoRespuesta.IrTuca:
                    case EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta:
                    case EnumCodigoRespuesta.Ir_Tuca_Segunda_Consulta:
                        /*Invoca al Tuca (Buro) y la clase vuelve a llamar a BlazeInvoke*/
                        BuroClass.TucaInvoke(xInfoClass, blazeOutput.solicitud, ref xDatos, ref xStringBuilder);
                        break;
                    case EnumCodigoRespuesta.IrEquifax:
                        /*Invoca al Equifax (Buro) y la clase vuelve a llamar a BlazeInvoke*/
                        BuroClass.EquifaxInvoke(xInfoClass, blazeOutput.solicitud, ref xDatos, ref xStringBuilder);
                        break;
                    case EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta:
                    case EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta:
                        BuroClass.BancarioInvoke(xInfoClass, blazeOutput, ref xDatos, ref xStringBuilder);
                        break;
                    case EnumCodigoRespuesta.IrRenap:
                        /*Invoca al Equifax (Buro) y la clase vuelve a llamar a BlazeInvoke*/
                        BuroClass.RenapInvoke(xInfoClass, blazeOutput.solicitud, ref xDatos, ref xStringBuilder);
                        break;
                    case EnumCodigoRespuesta.IrBlaze:
                        /*Invoca al Blaze y la clase vuelve a llamar a BlazeInvoke*/
                        BlazeInvoke(xInfoClass, blazeOutput.solicitud, ref xDatos, ref xStringBuilder);
                        break;

                    default:
                        /*Metodo de saliida del ciclo ya sea por Rechazo o Aprobacion*/
                        EndInvoke(xInfoClass, blazeOutput, ref xDatos, ref xStringBuilder);
                        break;
                }

                #endregion
            }
            catch (InvalidOperationException xException)
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xProceso = MethodBase.GetCurrentMethod().Name;

                string xParametros = String.Empty;


                var xTraceDto = new TraceDto
                {
                    LOG_TYPE = LogType.Trace,
                    FEC_SISTEMA = DateTime.Now,
                    INT_CONSECUTIVO = xInfoClass.Consecutivo,
                    STR_IDENTIFICADOR_EXTERNO = xInfoClass.IdentificadorUnico,
                    STR_APLICATIVO = xAplicativo,
                    STR_CLASE = xClase,
                    STR_PROCESO = xProceso,
                    INT_ID_ORIGEN = Convert.ToInt32(ID_ORIGEN.WEB_SERVICE),
                    STR_PARAMETROS = xParametros,
                    SIN_FK_UTL_PAR_PAIS = Convert.ToInt32(xInfoClass.PaisCode)
                };

                xInfoClass.Consecutivo = 9999;

                xStringBuilder.AppendLine(new string('-', 40) + "[INICIO  - CONVERSION A MODELO BLAZE]" +
                                          new string('-', 40));
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [Solicitud: {7} | Error: {8}]",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xSolicitud.Id,
                        xException.InnerException));
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();

                #endregion

                #region [Manejo de Errores]

                string xMensajeError = string.Format("( ErrorMessage:{0} | StackTrace:{1} )", xException.Message,
                    xException.StackTrace);

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error  | Proyecto: {2} | Clase: {3} | Metodo: {4} | Parametros: {5} | Detalle: {6}",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xMensajeError));


                xStringBuilder.AppendLine(new string('-', 40) + "[FINAL - CONVERSION A MODELO BLAZE]" +
                                          new string('-', 40));

                xTraceDto.LOG_TYPE = LogType.Error;
                xTraceDto.FEC_SISTEMA = DateTime.Now;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.STR_OBSERVACION = xMensajeError;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.SIN_FK_UTL_PAR_PAIS = Convert.ToInt32(xInfoClass.PaisCode);
                xTraceDto.STR_IDENTIFICADOR_UNICO = xInfoClass.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = xInfoClass.IdentificadorUnico;
                ExceptionsClass.WriteLog(xTraceDto);

                #endregion
            }
            catch (BuroExceptions)
            {
                throw;
            }
            catch (Exception xException)
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xProceso = MethodBase.GetCurrentMethod().Name;

                string xParametros = String.Empty;


                var xTraceDto = new TraceDto
                {
                    LOG_TYPE = LogType.Trace,
                    FEC_SISTEMA = DateTime.Now,
                    INT_CONSECUTIVO = xInfoClass.Consecutivo,
                    STR_IDENTIFICADOR_EXTERNO = xInfoClass.IdentificadorUnico,
                    STR_APLICATIVO = xAplicativo,
                    STR_CLASE = xClase,
                    STR_PROCESO = xProceso,
                    INT_ID_ORIGEN = Convert.ToInt32(ID_ORIGEN.WEB_SERVICE),
                    STR_PARAMETROS = xParametros,
                    SIN_FK_UTL_PAR_PAIS = Convert.ToInt32(xInfoClass.PaisCode)
                };

                xInfoClass.Consecutivo = 9999;

                xStringBuilder.AppendLine(new string('-', 40) + "[INICIO - ERROR]" + new string('-', 40));
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [Solicitud: {7}]",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xSolicitud.Id));
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();

                #endregion

                #region [Manejo de Errores]

                string xMensajeError = string.Format("( ErrorMessage:{0} | StackTrace:{1} )", xException.Message,
                    xException.StackTrace);

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Error  | Proyecto: {2} | Clase: {3} | Metodo: {4} | Parametros: {5} | Detalle: {6}",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xAplicativo,
                        xClase,
                        xProceso,
                        xParametros,
                        xMensajeError));


                xStringBuilder.AppendLine(new string('-', 40) + "[FINAL - ERROR]" + new string('-', 40));

                xTraceDto.LOG_TYPE = LogType.Error;
                xTraceDto.FEC_SISTEMA = DateTime.Now;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.STR_OBSERVACION = xMensajeError;
                xTraceDto.STR_MENSAJE = xStringBuilder.ToString();
                xTraceDto.SIN_FK_UTL_PAR_PAIS = Convert.ToInt32(xInfoClass.PaisCode);
                xTraceDto.STR_IDENTIFICADOR_UNICO = xInfoClass.IdentificadorUnico;
                xTraceDto.STR_IDENTIFICADOR_EXTERNO = xInfoClass.IdentificadorUnico;
                xTraceDto.STR_USUARIO_CREACION = xInfoClass.STR_USUARIO_CREACION;
                ExceptionsClass.WriteLog(xTraceDto);

                #endregion
            }
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        internal void BlazeInvoke(InfoClass xInfoClass, Solicitud xSolicitud)
        {
            string xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();
            BlazeInvoke(xInfoClass, xSolicitud, ref xDatos, ref xStringBuilder);
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo encargado de la finalizacion de la invokacion de Blaze
        /// </summary>
        /// <param name="xInfoClass">The x information class.</param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="xDatos"></param>
        /// <param name="xStringBuilder"></param>
        internal static void EndInvoke(InfoClass xInfoClass, BlazeOutput xSolicitud, ref string xDatos,
            ref StringBuilder xStringBuilder)
        {
            #region [Data Trace]

            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                MethodBase.GetCurrentMethod().Module.Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);
            string xProceso = MethodBase.GetCurrentMethod().Name;
            string xParametros = string.Empty;

            var methodParams = MethodBase.GetCurrentMethod().GetParameters();
            //for (int i = 0; i < methodParams.Length; i++)
            //{
            //    xParametros = xParametros +
            //                  string.Format("Parametro [{0}] = {1} |", i.ToString(CultureInfo.InvariantCulture),
            //                      methodParams[i]);
            //}
            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE END INVOKE | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    xInfoClass.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion

            #region Obtener Informacion del Aplicativo

            var xDynamicDto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_PK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA",
                        Value = "<message>INICIO DE FINALIZACION - ENVIO DE RESPUESTA</message>"
                    },
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "INT_TIPO",
                        Value = ConfigurationManager.AppSettings["PA_CON_OBTIENE_APLICATIVO_RESULTADO"]
                    },
                    new SpParameter
                    {
                        Name = "SIN_FACE",
                        Value = xSolicitud.solicitud.Flujo.ToString(CultureInfo.InvariantCulture)
                    }
                },
                Result = null,
                SPName = "PA_CON_OBTIENE_APLICATIVO"
            };

            #endregion

            /*Se Valida cual tiene que ejecutar el procedimiento o si este viene por el proceso de batch*/
            if (String.IsNullOrEmpty(xInfoClass.SPName_EndInvoke))
            {
                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [EJECUCION DE PA_CON_OBTIENE_APLICATIVO | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xDynamicDto.ParameterList),
                        "SE BUSCA EL SP SEGUN EL PAIS "));

                #endregion

                DateTime oldDate1 = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", xDynamicDto.SPName));

                xDynamicDto = DynamicSqlDAO.ExecuterSp(xDynamicDto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                DateTime newDate1 = DateTime.Now;
                TimeSpan ts1 = newDate1 - oldDate1;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", xDynamicDto.SPName));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts1.TotalSeconds));

                if (!xDynamicDto.HasResult) return;
            }

            // Fecha: 13/02/2016, Mdailey 
            //string xmlDataEnviada = ConvertClass.SerializeObject(xSolicitud);
            // Fecha: 13/02/2016, Mdailey 

            if (xSolicitud.solicitud.TipoLlamada.Equals(EnumTipoLlamada.Batch))
                xInfoClass.BIN_FK_BOM_LOG_OPERACIONES = xSolicitud.solicitud.Id;

            #region Create DynamicDto

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },//yuyuy
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = xSolicitud.solicitud.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA_ENVIADA",
                        Value = xSolicitud.xmlResponse // Fecha: 13/02/2016, Mdailey 
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_HILO",
                        Value = xInfoClass.IdentificadorUnicoHilo
                    },
                    new SpParameter
                    {
                        Name = "SIN_FACE",
                        Value = xSolicitud.solicitud.Flujo.ToString(CultureInfo.InvariantCulture)
                    },
                    new SpParameter
                           {
                               Name = "STR_USUARIO_CREACION",
                               Value = xInfoClass.STR_USUARIO_CREACION
                           },
                            new SpParameter
                            {
                                Name = "INT_SUB_ORIGEN",
                                Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                            } ,
                                new SpParameter
                            {
                                Name = "STR_TEL_CELULAR",
                                Value = xInfoClass.STR_TEL_CELULAR
                            },
                            new SpParameter
                            {
                                Name = "STR_EMAIL",
                                Value = xInfoClass.STR_EMAIL
                            }
                },
                Result = null,
                SPName = String.IsNullOrEmpty(xInfoClass.SPName_EndInvoke)
                    ? xDynamicDto.Result.Tables[0].Rows[0]["STR_PROCEDIMIENTOS"].ToString()
                    : xInfoClass.SPName_EndInvoke
            };


            #endregion
            DateTime oldDate = DateTime.Now;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));

            xDynamicDto = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
            DateTime newDate = DateTime.Now;
            TimeSpan ts = newDate - oldDate;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

            if (xDynamicDto.HasResult)
            {
                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CONVERSION RESPUESTA EN XML [{0}] -------------------------------------------------------***", oldDate));

                if (xDynamicDto.Result.Tables[0].Columns["XML_DATA"] != null)
                {
                    //CONVERSION DEL XML EN UN OBJETO
                    var doc = new XmlDocument();
                    doc.LoadXml(xDynamicDto.Result.Tables[0].Rows[0]["XML_DATA"].ToString());

                    xDatos = JsonConvert.SerializeObject(doc);
                    // xDatos = string.Format("{0} {1} ", xDatos, xDynamicDto.Result.Tables[0].Rows[0]["XML_DATA"]);
                }
                else
                {
                    var values = (from property in dto.GetType().GetProperties()
                                  where property.Name == "ParameterList"
                                  select new
                                  {
                                      Name = property.Name,
                                      Value = property.GetValue(dto)
                                  }.Value);
                    throw new Exception(string.Format("PROBLEMAS A EJECUTAR EL FLUJO: {0} PARAMETROS: {1} ", dto.SPName, JsonConvert.SerializeObject(values)));
                }
            }

            newDate = DateTime.Now;
            ts = newDate - oldDate;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA CONVERSION RESPUESTA EN XML [{0}] -------------------------------------------------------***", newDate));
            xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

            xStringBuilder.AppendLine(string.Format("*** SALIDA A EndInvoke ***"));
            xStringBuilder.AppendLine(string.Format("*** {0} ***", DateTime.Now));
        }

        internal void EndInvoke(InfoClass infDto, BlazeOutput blazeOutput)
        {
            string xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();

            EndInvoke(infDto, blazeOutput, ref xDatos, ref xStringBuilder);
        }
    }
}