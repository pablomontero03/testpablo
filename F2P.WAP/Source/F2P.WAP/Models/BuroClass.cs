using F2P.Utilitarios.Common.DataBase;
using F2P.Utilitarios.DataAccess;
using F2P.Utilitarios.Handler;
using F2P.WAP.Models.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;

namespace F2P.WAP.Models
{
    internal class BuroClass
    {
        #region TUCA Metodos

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de TUCA
        /// </summary>
        internal static void TucaInvoke(InfoClass xInfoClass, Solicitud xSolicitud, ref string xDatos,
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

            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE ProcessRequest | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    xSolicitud.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion

            string xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud);

            #region Create DynamicDto

            //icortes para envierle a tuca operaciones el tipo de consulta del xml buro 
            switch (xInfoClass.STR_COD_PAIS)
            {
                case Pais.NICARAGUA:
                    xInfoClass.INT_ID_MARCA = xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta ? 1 : 2;
                    xml_Data_enviada = xInfoClass.xmlObjetSentSOC;
                    break;
                case Pais.GUATEMALA:
                case Pais.SALVADOR:
                    xml_Data_enviada = xInfoClass.xmlObjetSentSOC;
                    break;
            }


            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_PERSONA",
                        Value = xSolicitud.Id
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = xSolicitud.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                        Value = xSolicitud.IdentificadorUnicoGestion
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "TIN_ORIGEN",
                        Value = xInfoClass.TIN_ORIGEN
                    },
                    //new SpParameter
                    //{
                    //    Name = "XML_DATA_ENVIADA",
                    //    Value = xml_Data_enviada
                    //}  new SpParameter
                    /*ICORTES 24-08-2016 INICIO MODIFICACION XML ENVIADO DE SOC*/
                    new SpParameter
                    {
                        Name = "XML_DATA_ENVIADA",
                        Value = xml_Data_enviada
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                    },
                        new SpParameter
                    {
                        Name = "INT_ID_MARCA",
                        Value = xInfoClass.INT_ID_MARCA.GetValueOrDefault().ToString()
                    },
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }
                    /*FIN ICORTES 24-08-2016 INICIO MODIFICACION XML ENVIADO DE SOC*/
                },
                Result = null,
                SPName = "PA_PRO_BOM_TUCA_OPERACION"
            };

            #endregion
            DateTime oldDate = DateTime.Now;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
            DateTime newDate = DateTime.Now;
            TimeSpan ts = newDate - oldDate;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


            bool flag = false;
            if (Boolean.TryParse(ConfigurationManager.AppSettings["bitImprimeConexion"], out flag))
            {
                xStringBuilder.AppendLine(new string('-', 40) + "[INICIO VALIDACION CONEXION BUROS]" + new string('-', 40));
                xStringBuilder.AppendLine(string.Format("PAIS: {0} CONEXION BLAZE: {1}", GlobalClass.STR_PAIS, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value));
                xStringBuilder.AppendLine(new string('-', 40) + "[FIN VALIDACION CONEXION BUROS]" + new string('-', 40));
            }

            var obj = JsonConvert.SerializeObject(ds.Result.Tables[0]);

            List<DTO_Buro> listadoBuro = JsonConvert.DeserializeObject<List<DTO_Buro>>(obj);

            string xmlResponse = string.Empty;

            /*1- Llama a TUCA*/
            TucaStartConsult(listadoBuro.FirstOrDefault(), ref xmlResponse, xInfoClass, xSolicitud, ref xStringBuilder);

            /*2- Guarda la informacion de Tuca y Carga nuevamente el Modelo Solicitud*/
            TucaEndConsult(xInfoClass, ref xSolicitud, xmlResponse, ref xStringBuilder);

            /*3- Se vuelve a llamar al motor de reglas*/
            BlazeClass.BlazeInvoke(xInfoClass, xSolicitud, ref xDatos, ref xStringBuilder);
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo de inicializacion de Metodo contral el Buro de TUCA
        /// </summary>
        private static void TucaStartConsult(DTO_Buro tucaBuro, ref string xmlResponse, InfoClass xInfoClass,
            Solicitud xSolicitud,
            ref StringBuilder xStringBuilder)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO INVOCACION DE TUCA PAIS " + xInfoClass.STR_COD_PAIS + " | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        string.Join(",", (from property in tucaBuro.GetType().GetProperties()
                                          select string.Concat(property.Name, "=", property.GetValue(tucaBuro))))));

                #endregion

                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA LLAMADO TUCA [{0}] -------------------------------------------------------***", oldDate));

                var url = string.Empty;


                using (var tuca = new TU_Consulta.TU_Consulta())
                {
                    tuca.Url = tucaBuro.TUCA_URL;
                    xmlResponse = tuca.Reporte(tucaBuro.XML_Request_Tuca);
                    url = tuca.Url;
                }

                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA INVOCACION TUCA [{0}] -------------------------------------------------------***", newDate));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [FIN INVOCACION DE TUCA | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7}  | URL: {8} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        xmlResponse, url));

                #endregion
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Problemas de Conexion con el Buro TUCA", "|", ex.Message, "|", xProceso, "|",xSolicitud.Persona.BancarioIngresado));
            }
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo de Finalizacion del proceso de TUCA
        /// </summary>
        /// <param name="xInfoClass">The x information class.</param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="xmlResponse">The XML response.</param>
        /// <param name="xStringBuilder"></param>
        private static void TucaEndConsult(InfoClass xInfoClass, ref Solicitud xSolicitud, string xmlResponse,
            ref StringBuilder xStringBuilder)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO DE TUCA END CONSULT | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion

                string xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud);

                #region Parametros

                var dto = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                    {
                        new SpParameter
                        {
                            Name = "INT_FK_BOM_PAR_APLICATIVO",
                            Value = xInfoClass.ApplicationCode
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_UNICO",
                            Value = xSolicitud.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_EXTERNO",
                            Value = xInfoClass.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "BIN_FK_BOM_LOG_OPERACIONES",
                            Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                        },
                        new SpParameter
                        {
                            Name = "SIN_FK_UTL_PAR_PAIS",
                            Value = "1"
                        },
                        new SpParameter
                        {
                            Name = "XML_DATA_ENVIADA",
                            Value = xml_Data_enviada
                        },
                        new SpParameter
                        {
                            Name = "SIN_FACE",
                            Value = xSolicitud.Flujo.ToString()
                        }
                        ,
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                            Value = xSolicitud.IdentificadorUnicoGestion
                        },
                        new SpParameter
                        {
                            Name = "BIN_ID_PERSONA",
                            Value = xSolicitud.Id
                        },
                        new SpParameter
                        {
                            Name = "TIN_ORIGEN",
                            Value = xInfoClass.TIN_ORIGEN
                        },
                        new SpParameter
                        {
                            Name = "INT_ID_MARCA",
                            Value = xInfoClass.INT_ID_MARCA.GetValueOrDefault().ToString()
                        },
                        new SpParameter
                        {
                            Name = "XML_TUCA_RESPUESTA",
                            Value = xmlResponse
                        },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                    }
                        ,
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }
                         ,
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    }
                    },
                    Result = null,
                    SPName = "PA_PRO_BOM_TUCA_RESULTADO"
                };

                dto.Result = null;

                #endregion

                #region [Data Trace]

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [EJECUCION DE {8} | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(dto.ParameterList),
                        "EJEUCION DE SP (XML_TUCA_RESPUESTA)",
                        dto.SPName));

                #endregion
                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));

                DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                var errorList = new List<string>();

                /**************************************************************************************************************/
                /*****************************************    1- CARGA DE BURO   ********************************************/
                /**************************************************************************************************************/
                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CARGA DE BURO [{0}] -------------------------------------------------------***", oldDate));

                IEnumerable<DataRow> rows = ds.Result.Tables[0].Select().Where(x => true);
                List<Buro> buros = new List<Buro>();
                if (rows.Any())
                {
                    buros = JsonConvert.DeserializeObject<List<Buro>>(JsonConvert.SerializeObject(rows.CopyToDataTable())); //ConvertClass.ObjectToList<Buro>(dt, xInfoClass.IdentificadorUnico, ref errorList);

                    //xSolicitud.Persona.ListadoBuros.RemoveAll(x => x.TipoBuro.Equals(buros.FirstOrDefault().TipoBuro));
                    switch (xInfoClass.STR_COD_PAIS)
                    {
                        case Pais.COSTA_RICA:
                        case Pais.SALVADOR:
                            xSolicitud.Persona.ListadoBuros = buros ?? new List<Buro>();
                            break;
                        case Pais.NICARAGUA:
                        case Pais.GUATEMALA:
                            xSolicitud.Persona.ListadoBuros.RemoveAll(x => x.TipoBuro.Equals(buros.FirstOrDefault().TipoBuro));
                            xSolicitud.Persona.ListadoBuros.Add(buros.FirstOrDefault());
                            break;
                    }

                    List<Precalificados> precalificados = new List<Precalificados>();

                    rows = dto.Result.Tables[1].Select();
                    precalificados = JsonConvert.DeserializeObject<List<Precalificados>>(JsonConvert.SerializeObject(rows.CopyToDataTable()));

                    precalificados = (from resPreca in precalificados
                                      join resburo in buros on resPreca.BuroID equals resburo.Id
                                      where resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Segunda_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Segunda_Consulta")
                                      select new Precalificados
                                      {
                                          BuroID = resPreca.BuroID,
                                          Llave = resPreca.Llave,
                                          Valor = resPreca.Valor,
                                          CodigoPrecalificado = resPreca.CodigoPrecalificado
                                      }).ToList();

                    //buros.FirstOrDefault().ListadoPrecalificados = precalificados ?? new List<Precalificados>();
                    buros.ForEach(x => { x.ListadoPrecalificados = (from res in precalificados where res.BuroID == x.Id select res).ToList(); });
                    //foreach (Buro buro in buros)
                    //{
                    //    /**************************************************************************************************************/
                    //    /******************************    1.1 - CARGA DE PRECALIFICADOS DEL BURO   **********************************/
                    //    /**************************************************************************************************************/
                    //    rows = dto.Result.Tables[1].Select().Where(x => x["BuroId"].Equals(buros.FirstOrDefault().Id));

                    //    List<Precalificados> precalificados = new List<Precalificados>();

                    //    if (rows.Any())
                    //        precalificados = JsonConvert.DeserializeObject<List<Precalificados>>(JsonConvert.SerializeObject(rows.CopyToDataTable()));

                    //    buros.FirstOrDefault().ListadoPrecalificados = precalificados ?? new List<Precalificados>();
                    //}

                    rows = dto.Result.Tables[2].Select();
                    List<Contacto> contactos = new List<Contacto>();
                    if (rows.Any())
                        contactos = JsonConvert.DeserializeObject<List<Contacto>>(JsonConvert.SerializeObject(rows.CopyToDataTable()));


                    xSolicitud.Persona.ListadoContactos = contactos ?? new List<Contacto>();

                    newDate = DateTime.Now;
                    ts = newDate - oldDate;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA CARGA DE BURO [{0}] -------------------------------------------------------***", newDate));
                    xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

                }
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Problemas al guardar Precalificados con el Buro TUCA", "|", ex.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
            }
        }


        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de TUCA
        /// </summary>
        /// <param name="infDto"></param>
        /// <param name="solicitud"></param>
        internal void TucaInvoke(InfoClass infDto, Solicitud solicitud)
        {
            string xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();

            TucaInvoke(infDto, solicitud, ref xDatos, ref xStringBuilder);
        }

        #endregion

        #region EQUIFAX Metodos

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 19/09/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de TUCA
        /// </summary>
        /// <param name="infDto"></param>
        /// <param name="solicitud"></param>
        internal void EquifaxInvoke(InfoClass infDto, Solicitud solicitud)
        {
            string xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();

            EquifaxInvoke(infDto, solicitud, ref xDatos, ref xStringBuilder);
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 19/09/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de EQUIFAX
        /// </summary>
        internal static void EquifaxInvoke(InfoClass xInfoClass, Solicitud xSolicitud, ref string xDatos,
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
                    "[{0} | {1}] - [INICIO DE ProcessRequest | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    xSolicitud.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    string.Empty));

            #endregion
            string xml_Data_enviada = string.Empty;
            switch (xInfoClass.STR_COD_PAIS)
            {
                case Pais.SALVADOR:
                    xml_Data_enviada = xInfoClass.xmlObjetSentSOC;
                    break;
                default:
                    xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud);
                    break;
            }

            #region Create DynamicDto

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = xSolicitud.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_PERSONA",
                        Value = xSolicitud.Id
                    },
                    new SpParameter
                    {
                        Name = "TIN_ORIGEN",
                        Value = xInfoClass.TIN_ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                        Value = xSolicitud.IdentificadorUnicoGestion
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value ="1"
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA_ENVIADA",
                        Value = xml_Data_enviada
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                    },
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }
                },
                Result = null,
                SPName = "PA_PRO_BOM_EQUIFAX_OPERACION"
            };

            #endregion

            DateTime oldDate = DateTime.Now;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));


            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);

            DateTime newDate = DateTime.Now;
            TimeSpan ts = newDate - oldDate;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

            var obj = JsonConvert.SerializeObject(ds.Result.Tables[0]);

            List<DTO_Buro> listadoBuro = JsonConvert.DeserializeObject<List<DTO_Buro>>(obj);

            //string xReferencia = ds.Result.Tables[0].Rows[0]["Referencia"].ToString();
            //string xCedula = ds.Result.Tables[0].Rows[0]["Cedula"].ToString();
            //string xUsuario = ds.Result.Tables[0].Rows[0]["Usuario"].ToString();
            //string xClave = ds.Result.Tables[0].Rows[0]["Clave"].ToString();
            //string xUsuario_datum = ds.Result.Tables[0].Rows[0]["Usuario_datum"].ToString();
            //string xURL = ds.Result.Tables[0].Rows[0]["URL"].ToString();

            string xmlResponse = string.Empty;

            /*1- Llama a EQUIFAX*/
            EquifaxStartConsult(listadoBuro.FirstOrDefault(), ref xmlResponse,
                xInfoClass,
                xSolicitud, ref xStringBuilder);

            /*2- Guarda la informacion de Equifax y Carga nuevamente el Modelo Solicitud*/
            EquifaxEndConsult(xInfoClass, ref xSolicitud, xmlResponse, ref xStringBuilder);

            /*3- Se vuelve a llamar al motor de reglas*/
            BlazeClass.BlazeInvoke(xInfoClass, xSolicitud, ref xDatos, ref xStringBuilder);
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 19/09/2015
        ///     Descripcion: Metodo de inicializacion de Metodo contral el Buro de EQUIFAX
        /// </summary>
        /// <param name="xReferencia"></param>
        /// <param name="xCedula"></param>
        /// <param name="xUsuario"></param>
        /// <param name="xClave"></param>
        /// <param name="xUsuario_datum"></param>
        /// <param name="xURL"></param>
        /// <param name="xmlResponse"></param>
        /// <param name="xInfoClass"></param>
        /// <param name="xSolicitud"></param>
        /// <param name="xStringBuilder"></param>
        private static void EquifaxStartConsult(DTO_Buro listadoBuro, ref string xmlResponse,
            InfoClass xInfoClass, Solicitud xSolicitud, ref StringBuilder xStringBuilder)
        {

            #region [Data Trace]
            var url = string.Empty;
            string xProceso = MethodBase.GetCurrentMethod().Name;
            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                MethodBase.GetCurrentMethod().Module.Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);

            string xParametros = string.Empty;

            var methodParams = MethodBase.GetCurrentMethod().GetParameters();
            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            var valuess = (from property in listadoBuro.GetType().GetProperties()
                           select string.Concat(property.Name, "=", property.GetValue(listadoBuro)));

            #endregion
            try
            {
                //if (ValidaEstadoWebService(xInfoClass, listadoBuro.URL, ref xStringBuilder))
                //{
                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CONSULTA EQUIFAX [{0}] -------------------------------------------------------***", oldDate));
                //aplica seguridad a wsTuca crea SSL/TLS secure channel.
              
                switch (xInfoClass.STR_COD_PAIS)
                {
                    case Pais.SALVADOR:

                        #region Data Trace
                        xStringBuilder.AppendLine(
                   string.Format(
                       "[{0} | {1}] - [INICIO INVOCACION DE EQUIFAX PAIS " + xInfoClass.STR_COD_PAIS + " | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [{7}] ",
                       DateTime.Now,
                       xSolicitud.IdentificadorUnico,
                       xInfoClass.Consecutivo,
                       xAplicativo,
                       xClase,
                       xProceso,
                       ConvertClass.SerializeObject(xParametros),
                       string.Join(",", valuess)));
                        #endregion



                        using (var equifax = new MultivaloresServiceService())
                        {
                            equifax.Url = listadoBuro.URL;
                            url = equifax.Url;
                            xmlResponse = equifax.getCandidateInfo(listadoBuro.Usuario, listadoBuro.Clave, listadoBuro.Dui, listadoBuro.Nit,
                                listadoBuro.FechaNacimiento, listadoBuro.PrimerNombre, listadoBuro.SegundoNombre, listadoBuro.PrimerApellido,
                                listadoBuro.SegundoApellido, listadoBuro.ApellidoCasada);

                            #region Data Trace
                            xStringBuilder.AppendLine(
                                string.Format(
                                "[{0} | {1}] - [FIN INVOCACION DE EQUIFAX | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [{7}] | URL: [{8}] ",
                                DateTime.Now,
                                xSolicitud.IdentificadorUnico,
                                xInfoClass.Consecutivo,
                                xAplicativo,
                                xClase,
                                xProceso,
                                ConvertClass.SerializeObject(xParametros),
                                string.Join(",", valuess), equifax.Url));
                            #endregion

                        }

                        break;
                    default:
                        #region Data Trace
                        xStringBuilder.AppendLine(
                                                string.Format(
                                                "[{0} | {1}] - [INICIO INVOCACION DE EQUIFAX PAIS " + xInfoClass.STR_COD_PAIS + " | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [{7}] ",
                                                DateTime.Now,
                                                xSolicitud.IdentificadorUnico,
                                                xInfoClass.Consecutivo,
                                                xAplicativo,
                                                xClase,
                                                xProceso,
                                                ConvertClass.SerializeObject(xParametros),
                                                string.Join(",", valuess)));
                        #endregion
                        using (var equifax = new EquifaxCr_WebServices_ReportesINFORMENRICHEMPOWER())
                        {
                            equifax.Url = listadoBuro.URL; /*CAMBIO DE LA URL POR SI FUERA QUE CADA PAIS OCUPARA DISTINTOS SITIOS*/

                            xmlResponse = equifax.estudio_personas_fisicas(listadoBuro.Referencia, listadoBuro.Cedula, listadoBuro.Usuario, listadoBuro.Clave,
                                listadoBuro.Usuario_datum);

                            #region Data Trace
                            xStringBuilder.AppendLine(
                                            string.Format(
                                            "[{0} | {1}] - [FIN INVOCACION DE EQUIFAX | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [{7}] | URL: [{8}] ",
                                            DateTime.Now,
                                            xSolicitud.IdentificadorUnico,
                                            xInfoClass.Consecutivo,
                                            xAplicativo,
                                            xClase,
                                            xProceso,
                                            ConvertClass.SerializeObject(xParametros),
                                            string.Join(",", valuess), equifax.Url));
                            #endregion
                        }
                        break;
                }
                //}

                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA CONSULTA EQUIFAX [{0}] -------------------------------------------------------***", newDate));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [FIN INVOCACION DE EQUIFAX | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        xmlResponse));

                #endregion
            }
            catch (Exception ex)
            {
                #region Data Trace
                xStringBuilder.AppendLine(
                                        string.Format(
                                        "[{0} | {1}] - [INICIO INVOCACION DE EQUIFAX PAIS " + xInfoClass.STR_COD_PAIS + " ERROR | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [{7}]  | URL: [{8}]   | Detalle Error: [{9}] ",
                                        DateTime.Now,
                                        xSolicitud.IdentificadorUnico,
                                        xInfoClass.Consecutivo,
                                        xAplicativo,
                                        xClase,
                                        xProceso,
                                        ConvertClass.SerializeObject(xParametros),
                                        string.Join(",", valuess), url, ex.Message));
                #endregion

                throw new BuroExceptions(string.Concat("Problemas de Conexion con el Buro EQUIFAX", "|", ex.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
            }
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 19/09/2015
        ///     Descripcion: Metodo de Finalizacion del proceso de EQUIFAX
        /// </summary>
        /// <param name="xInfoClass">The x information class.</param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="xmlResponse">The XML response.</param>
        /// <param name="xStringBuilder"></param>
        private static void EquifaxEndConsult(InfoClass xInfoClass, ref Solicitud xSolicitud, string xmlResponse,
            ref StringBuilder xStringBuilder)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);

                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();
                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO DE EQUIFAX END CONSULT | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion


                string xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud).Replace(
                            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"",
                            String.Empty)
                        .Replace("xsi:nil=\"true\"", String.Empty).Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", String.Empty);

                switch (xInfoClass.STR_COD_PAIS)
                {
                    case Pais.SALVADOR:
                        xml_Data_enviada = xInfoClass.xmlObjetSentSOC;
                        break;
                }

                #region Parametros

                var dto = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                    {
                        new SpParameter
                        {
                            Name = "INT_FK_BOM_PAR_APLICATIVO",
                            Value = xInfoClass.ApplicationCode
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_UNICO",
                            Value = xSolicitud.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_EXTERNO",
                            Value = xInfoClass.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "BIN_FK_BOM_LOG_OPERACIONES",
                            Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                        },
                        new SpParameter
                        {
                            Name = "SIN_FK_UTL_PAR_PAIS",
                            Value = "1"
                        },
                        new SpParameter
                        {
                            Name = "XML_DATA_ENVIADA",
                            Value = xml_Data_enviada
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                            Value = xSolicitud.IdentificadorUnicoGestion
                        },
                        new SpParameter
                        {
                            Name = "SIN_FACE",
                            Value = xSolicitud.Flujo.ToString()
                        },
                        new SpParameter
                        {
                            Name = "TIN_ORIGEN",
                            Value = xInfoClass.TIN_ORIGEN
                        },
                        new SpParameter
                        {
                            Name = "BIN_ID_PERSONA",
                            Value = xSolicitud.Id
                        },
                        new SpParameter
                        {
                            Name = "BIN_ID_SOLICITUD",
                            Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                        },
                        new SpParameter
                        {
                            Name = "XML_EQUIFAX_RESPUESTA",
                            Value = xmlResponse.Replace(
                            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"",
                            String.Empty)
                        .Replace("xsi:nil=\"true\"", String.Empty).Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>",String.Empty)
                        },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }  ,
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN ",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    }
                    },
                    Result = null,
                    SPName = "PA_PRO_BOM_EQUIFAX_RESULTADO"
                    //dto.Result.Tables[0].Rows[0]["STR_PROCEDIMIENTOS"].ToString()
                };

                dto.Result = null;

                #endregion

                #region [Data Trace]

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [EJECUCION DE {8} | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(dto.ParameterList),
                        "EJEUCION DE SP (XML_EQUIFAX_RESPUESTA)",
                        dto.SPName));

                #endregion

                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));

                DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

                var errorList = new List<string>();

                /**************************************************************************************************************/
                /*****************************************    1- CARGA DE BURO   ********************************************/
                /**************************************************************************************************************/
                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CARGA DE BURO [{0}] -------------------------------------------------------***", oldDate));

                IEnumerable<DataRow> rows = ds.Result.Tables[0].Select().Where(x => true);
                List<Buro> buros = new List<Buro>();
                if (rows.Any())
                {
                    buros = JsonConvert.DeserializeObject<List<Buro>>(JsonConvert.SerializeObject(rows.CopyToDataTable())); //ConvertClass.ObjectToList<Buro>(dt, xInfoClass.IdentificadorUnico, ref errorList);

                    //xSolicitud.Persona.ListadoBuros.RemoveAll(x => x.TipoBuro.Equals(buros.FirstOrDefault().TipoBuro));

                    xSolicitud.Persona.ListadoBuros = buros ?? new List<Buro>();

                    List<Precalificados> precalificados = new List<Precalificados>();

                    rows = dto.Result.Tables[1].Select();
                    precalificados = JsonConvert.DeserializeObject<List<Precalificados>>(JsonConvert.SerializeObject(rows.CopyToDataTable()));

                    precalificados = (from resPreca in precalificados.AsEnumerable()
                                      join resburo in buros.AsEnumerable() on resPreca.BuroID equals resburo.Id
                                      where resburo.TipoBuro.ToString().Equals("Equifax_Precalificado") ||
                                      resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Segunda_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Segunda_Consulta")
                                      select new Precalificados
                                      {
                                          BuroID = resPreca.BuroID,
                                          Llave = resPreca.Llave,
                                          Valor = resPreca.Valor,
                                          CodigoPrecalificado = resPreca.CodigoPrecalificado
                                      }).ToList();

                    buros.ForEach(x => { x.ListadoPrecalificados = (from res in precalificados where res.BuroID == x.Id select res).ToList(); });

                    newDate = DateTime.Now;
                    ts = newDate - oldDate;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA CARGA DE BURO [{0}] -------------------------------------------------------***", newDate));
                    xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                    // buros.Where(x => x.Id == precalificados.FirstOrDefault().BuroID).FirstOrDefault().ListadoPrecalificados = precalificados ?? new List<Precalificados>();
                }
            }
            catch (Exception ex)
            {

                throw new BuroExceptions(string.Concat(string.Format("Problemas al guardar Precalificados con el Buro EQUIFAX: {0}", ex.Message), "|", ex.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
            }
        }

        #endregion

        #region RENAP Metodos


        internal static void RenapInvoke(InfoClass xInfoClass, Solicitud xSolicitud, ref string xDatos,
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

            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE ProcessRequest | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    xSolicitud.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    string.Empty));

            #endregion

            string xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud);
            #region Create DynamicDto

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "P_INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "P_BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "P_STR_IDENTIFICADOR_UNICO",
                        Value = xSolicitud.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "P_STR_RENAP_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "P_XML_DATA_ENVIADA",
                        Value = xml_Data_enviada
                    }
                },
                Result = null,
                SPName = "PA_PRO_BOM_RENAP_OPERACION"
            };

            #endregion

            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
            DTO_RENAP.RootObject objRenap = new DTO_RENAP.RootObject();

            string p_portalid = ds.Result.Tables[0].Rows[0]["Portalid"].ToString();
            string p_userid = ds.Result.Tables[0].Rows[0]["Userid"].ToString();
            string p_cuid = xInfoClass.IdentificadorCliente;
            //string p_cuid = ds.Result.Tables[0].Rows[0]["Cui"].ToString();
            string xURL = ds.Result.Tables[0].Rows[0]["URL"].ToString();

            string xmlResponse = string.Empty;


            //if (ValidaEstadoWebService(string.Concat(xURL, "?WSDL")))
            //{
            /*1- Llama a Renap*/
            RenapStartConsult(p_portalid, p_userid, p_cuid, xURL, ref xmlResponse, xInfoClass, xSolicitud, ref xStringBuilder, ref objRenap);

            /*2- Guarda la informacion de Renap y Carga nuevamente el Modelo Solicitud*/
            RenapEndConsult(xInfoClass, ref xSolicitud, xmlResponse, ref xStringBuilder, ref objRenap);

            /*3- Se vuelve a llamar al motor de reglas*/
            //ProcessOneToOne.ProcessRequest(xInfoClass, xInfoClass.xmlObjetSentSOC, ref xStringBuilder);

            //BlazeClass.BlazeInvoke(xInfoClass, xSolicitud, ref xDatos, ref xStringBuilder);
            //}
            //else throw new BuroExceptions("Problemas de Conexion con RENAP");
        }


        private static void RenapStartConsult(string p_portalid, string p_userid, string p_cuid, string xURL, ref string xmlResponse,
        InfoClass xInfoClass, Solicitud xSolicitud, ref StringBuilder xStringBuilder, ref DTO_RENAP.RootObject xObjRENAP)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO INVOCACION DE RENAP | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: [ p_portalid: {7}, p_userid: {8},p_cuid: {9}] ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        p_portalid, p_userid, p_cuid));

                #endregion

                using (var renap = new Trx())
                {
                    renap.Url = xURL; /*CAMBIO DE LA URL POR SI FUERA QUE CADA PAIS OCUPARA DISTINTOS SITIOS*/
                    xmlResponse = renap.chepeTePresta(p_portalid, p_userid, p_cuid);
                    xObjRENAP = JsonConvert.DeserializeObject<DTO_RENAP.RootObject>(xmlResponse);
                }

                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [FIN INVOCACION DE RENAP | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        xmlResponse));

                #endregion
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Problemas de Conexion con RENAP", "|", ex.Message, "|", xProceso));
            }
        }

        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 19/09/2015
        ///     Descripcion: Metodo de Finalizacion del proceso de EQUIFAX
        /// </summary>
        /// <param name="xInfoClass">The x information class.</param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="xmlResponse">The XML response.</param>
        /// <param name="xStringBuilder"></param>
        private static void RenapEndConsult(InfoClass xInfoClass, ref Solicitud xSolicitud, string xmlResponse,
        ref StringBuilder xStringBuilder, ref DTO_RENAP.RootObject xObjRENAP)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO DE EQUIFAX END CONSULT | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion

                string xml_Data_enviada = ConvertClass.SerializeObject(xObjRENAP);

                #region Parametros

                var dto = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                    {                        new SpParameter
                        {
                            Name = "SIN_FK_UTL_PAR_PAIS",
                            Value = "1"
                        },
                        new SpParameter
                        {
                            Name = "P_INT_FK_BOM_PAR_APLICATIVO",//@P_INT_FK_BOM_PAR_APLICATIVO
                            Value = xInfoClass.ApplicationCode
                        },
                        new SpParameter
                        {
                            Name = "P_BIN_FK_BOM_LOG_OPERACIONES",//@P_BIN_FK_BOM_LOG_OPERACIONES
                            Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                        },
                        new SpParameter
                        {
                            Name = "P_XML_DATA_ENVIADA",//@P_XML_DATA_ENVIADA
                            Value = xml_Data_enviada
                        },
                        new SpParameter
                        {
                            Name = "P_BIN_ID_PERSONA",//@P_BIN_ID_PERSONA
                            Value = xSolicitud.Id
                        },
                        new SpParameter
                        {
                            Name = "P_STR_RENAP_RESPUESTA",//@P_STR_RENAP_RESPUESTA
                            Value = xmlResponse
                        },
                        new SpParameter
                        {
                            Name = "P_STR_IDENTIFICACION",//@P_STR_IDENTIFICACION
                            Value = xObjRENAP.ezmovil.dpi.cui
                        },
                        new SpParameter
                        {
                            Name = "P_STR_GENERO",//@P_STR_GENERO
                            Value = xObjRENAP.ezmovil.dpi.genero.ToUpper().Equals("MASCULINO")?"M":"F"
                        },
                        new SpParameter
                        {
                            Name = "P_STR_ESTADO_CIVIL",//@P_STR_ESTADO_CIVIL
                            Value = xObjRENAP.ezmovil.dpi.estadoCivil
                        },
                        new SpParameter
                        {
                            Name = "P_STR_FOTO",//@P_STR_FOTO
                            Value = xObjRENAP.ezmovil.dpi.foto
                        },
                        new SpParameter
                        {
                            Name = "P_STR_CANTON",//@P_INT_CANTON
                            Value = xObjRENAP.ezmovil.dpi.municNacimiento
                        },
                        new SpParameter
                        {
                            Name = "P_STR_NACIONALIDAD",//@P_STR_NACIONALIDAD
                            Value = xObjRENAP.ezmovil.dpi.nacionalidad
                        },
                        new SpParameter
                        {
                            Name = "P_STR_PAIS_NACIMIENTO",//@P_STR_PAIS_NACIMIENTO
                            Value = xObjRENAP.ezmovil.dpi.paisnacimiento
                        },
                        new SpParameter
                        {
                            Name = "P_STR_APELLIDO_1",//@P_STR_APELLIDO_1
                            Value = xObjRENAP.ezmovil.dpi.primerNombre
                        },
                        new SpParameter
                        {
                            Name = "P_STR_APELLIDO_2",//@P_STR_APELLIDO_2
                            Value = xObjRENAP.ezmovil.dpi.segundoApellido
                        },
                        new SpParameter
                        {
                            Name = "P_STR_NOMBRE_1",//@P_STR_NOMBRE_1
                            Value = xObjRENAP.ezmovil.dpi.primerNombre
                        },
                        new SpParameter
                        {
                            Name = "P_STR_NOMBRE_2",//@P_STR_NOMBRE_2
                            Value = xObjRENAP.ezmovil.dpi.segundoNombre
                        },
                        new SpParameter
                        {
                            Name = "P_FECHA_NACIMIENTO",//@P_FECHA_NACIMIENTO
                            Value = xObjRENAP.ezmovil.dpi.fechaNacimiento
                        },
                        new SpParameter
                        {
                            Name = "P_STR_DEP_NACIMIENTO",//@P_STR_DEP_NACIMIENTO
                            Value = xObjRENAP.ezmovil.dpi.depNacimiento
                        },
                        new SpParameter
                        {
                            Name = "P_STR_REGISTRO_CEDULA",//@P_STR_REGISTRO_CEDULA
                            Value = xObjRENAP.ezmovil.dpi.registroCedula
                        },
                        new SpParameter
                        {
                            Name = "P_STR_IDENTIFICACION_UNICA_GESTION",//@P_SOURCE_CODE_GENERADO
                            Value = xInfoClass.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "P_SIN_FACE",//@P_SIN_FACE
                            Value = xSolicitud.Flujo.ToString()
                        }
                    },
                    Result = null,
                    SPName = "PA_PRO_BOM_RENAP_RESULTADO"
                    //dto.Result.Tables[0].Rows[0]["STR_PROCEDIMIENTOS"].ToString()
                };

                dto.Result = null;

                #endregion

                #region [Data Trace]

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [EJECUCION DE {8} | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(dto.ParameterList),
                        "EJEUCION DE SP (XML_RENAP_RESPUESTA)",
                        dto.SPName));

                #endregion

                DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);

                //var errorList = new List<string>();

                ///**************************************************************************************************************/
                ///*****************************************    1- CARGA DE BURO   ********************************************/
                ///**************************************************************************************************************/
                //IEnumerable<DataRow> rows = ds.Result.Tables[0].Select().Where(x => true);
                //var dt = new DataTable();
                //if (rows.Any())
                //    dt = rows.CopyToDataTable();

                //List<Buro> buros = ConvertClass.ObjectToList<Buro>(dt, xInfoClass.IdentificadorUnico, ref errorList);

                //xSolicitud.Persona.ListadoBuros = buros ?? new List<Buro>();

                //foreach (Buro buro in xSolicitud.Persona.ListadoBuros)
                //{
                //    /**************************************************************************************************************/
                //    /******************************    1.1 - CARGA DE PRECALIFICADOS DEL BURO   **********************************/
                //    /**************************************************************************************************************/
                //    rows = dto.Result.Tables[1].Select().Where(x => x["BuroId"].Equals(buro.Id));

                //    dt = new DataTable();
                //    if (rows.Any())
                //        dt = rows.CopyToDataTable();

                //    List<Precalificados> precalificados = ConvertClass.ObjectToList<Precalificados>(dt, xInfoClass.IdentificadorUnico, ref errorList);

                //    buro.ListadoPrecalificados = precalificados ?? new List<Precalificados>();
                //}
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Problemas al guardar Precalificados con el Buro RENAP", "|", ex.Message, "|", xProceso));
            }
        }


        private static bool ValidaEstadoWebService(InfoClass xInfoClass, string p_dir, ref StringBuilder xStringBuilder)
        {
            bool isServiceUp = true;
            string xProceso = MethodBase.GetCurrentMethod().Name;
            #region [Data Trace]

            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                MethodBase.GetCurrentMethod().Module.Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);
            string xParametros = string.Empty;

            var methodParams = MethodBase.GetCurrentMethod().GetParameters();

            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

            xParametros = string.Format("({0})", xParametros);

            xInfoClass.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE VALIDADOR DE SERVICIOS | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    9999,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion
            try
            {
                MetadataExchangeClient Client = new MetadataExchangeClient(new Uri(p_dir + "?WSDL"),
                                                                            MetadataExchangeClientMode.HttpGet);
                MetadataSet meta = Client.GetMetadata();

                #region [Data Trace]

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [FIN DE VALIDADOR DE SERVICIOS | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        9999,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion
            }

            catch (Exception ex)
            {

                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(string.Concat(new string('-', 40), "********INICIO ERROR VALIDADOR DE SERVICIOS********", new string('-', 40)));
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [ERROR DE VALIDADOR DE SERVICIOS | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        9999,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        string.Format("MENSAJE:[ {0} ], DETALLE: [ {1} ] ", ex.InnerException.Message, ex.InnerException)));
                xStringBuilder.AppendLine(string.Concat(new string('-', 40), "********FIN ERROR VALIDADOR DE SERVICIOS********", new string('-', 40)));

                #endregion
                isServiceUp = false;
                throw ex;
            }
            return isServiceUp;
        }
        #endregion

        #region BANCARIO Metodos
        /// <summary>
        ///     Autor: Alkinson Gonzalez
        ///     Fecha: 24/12/2018
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos del Buro Bancario
        /// </summary>
        internal static void BancarioInvoke(InfoClass xInfoClass, BlazeOutput blazeOutput, ref string xDatos,
            ref StringBuilder xStringBuilder)
        {
            #region [Data Trace]

            Solicitud solicitud = blazeOutput.solicitud;
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
                    "[{0} | {1}] - [INICIO DE ProcessRequest | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    solicitud.IdentificadorUnico,
                    xInfoClass.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion

            xInfoClass.INT_ID_MARCA = solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta ? 1 : 2;

            string xml_Data_enviada = ConvertClass.SerializeObject(solicitud);

            #region Create DynamicDto
            
            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_FK_BOM_PAR_APLICATIVO",
                        Value = xInfoClass.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "BIN_FK_BOM_LOG_OPERACIONES",
                        Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_PERSONA",
                        Value = solicitud.Id
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = solicitud.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = xInfoClass.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                        Value = solicitud.IdentificadorUnicoGestion
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "TIN_ORIGEN",
                        Value = xInfoClass.TIN_ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA_ENVIADA",
                        Value = xml_Data_enviada
                    },
                    new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = solicitud.IdAnalisis.GetValueOrDefault().ToString()
                    },
                        new SpParameter
                    {
                        Name = "INT_ID_MARCA",
                        Value = xInfoClass.INT_ID_MARCA.GetValueOrDefault().ToString()
                    },
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }
                    /*FIN ICORTES 24-08-2016 INICIO MODIFICACION XML ENVIADO DE SOC*/
                },
                Result = null,
                SPName = "PA_PRO_BOM_BANCARIO_OPERACION"
            };

            #endregion
            DateTime oldDate = DateTime.Now;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
            DateTime newDate = DateTime.Now;
            TimeSpan ts = newDate - oldDate;
            xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
            xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


            bool flag = false;
            if (Boolean.TryParse(ConfigurationManager.AppSettings["bitImprimeConexion"], out flag))
            {
                xStringBuilder.AppendLine(new string('-', 40) + "[INICIO VALIDACION CONEXION BUROS]" + new string('-', 40));
                xStringBuilder.AppendLine(string.Format("PAIS: {0} CONEXION BLAZE: {1}", GlobalClass.STR_PAIS, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value));
                xStringBuilder.AppendLine(new string('-', 40) + "[FIN VALIDACION CONEXION BUROS]" + new string('-', 40));
            }

            var obj = JsonConvert.SerializeObject(ds.Result.Tables[0]);

            List<DTO_Buro> listadoBuro = JsonConvert.DeserializeObject<List<DTO_Buro>>(obj);

            DTO_BURO_BANCARIO_ANALISIS BuroResponse = new DTO_BURO_BANCARIO_ANALISIS();

            /*1- Llama a BANCARIO*/
            BancarioStartConsult(listadoBuro.FirstOrDefault(), ref BuroResponse, xInfoClass, blazeOutput.solicitud, ref xStringBuilder);

            
            /*2- Guarda la informacion de Banco y Carga nuevamente el Modelo Solicitud*/
            /*
             Si es primera consulta o segunda consulta exitosa se guarda el precalificado normal
             */
            if(solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta || (BuroResponse.responseCode == "00" && solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta))
            {
                BancarioEndConsult(xInfoClass, ref solicitud, BuroResponse, ref xStringBuilder);
                if (BuroResponse.responseCode == "00" && solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta)
                    blazeOutput.solicitud.Persona.BancarioIngresado = 1;
            }

            /*
             Si la primera consulta no retorna precalificados, retornamos el TOKEN para la segunda consulta
             */
            if (BuroResponse.token != 0)
                blazeOutput.solicitud.Persona.TokenBancario = BuroResponse.token;

            /*
             Si es un llamado desde el TIMER seteamos en true para que no se encicle en consultas Bancario
             */
            if (solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta && xInfoClass.BIT_BURO_BANCARIO)
                xInfoClass.BIT_LLAMADO_BANCARIO = true;
            

            /*
             Si en la primera consulta se retorna el precalificado completo, se ingresa el precalificado completo
             */
            if (solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta && BuroResponse.token == 0)
            {
                xInfoClass.INT_ID_MARCA = 2;
                BancarioEndConsult(xInfoClass, ref solicitud, BuroResponse, ref xStringBuilder);
            }
            
            /*
             Si el timer expiro, o es flujo 2 ejecutado desde SOC o SUV, y no obtuvimos respuesta aun, se ingresa precalificado por defecto
             */
            if ((solicitud.Flujo > 1 && solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta && BuroResponse.responseCode != "00") || 
                (solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta && xInfoClass.BIT_ULTIMO_LLAMADO_TIMER && BuroResponse.responseCode != "00"))
            {
                //Se ingresan los precalificados por defecto
                BuroResponse.responseCode = "77";
                BancarioEndConsult(xInfoClass, ref solicitud, BuroResponse, ref xStringBuilder);
            
            }

            //xStringBuilder.AppendLine(string.Format("*** SALIDA A BLAZE ***"));
            //xStringBuilder.AppendLine(string.Format("*** {0} ***", DateTime.Now));
            if (xInfoClass.BIT_BURO_BANCARIO && xInfoClass.BIT_LLAMADO_BANCARIO && blazeOutput.solicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta && BuroResponse.responseCode != "00")
            {
                blazeOutput.xmlResponse = blazeOutput.xmlResponse.Replace(EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta.ToString(), EnumCodigoRespuesta.Aprovada.ToString());
                blazeOutput.solicitud = solicitud;
                BlazeClass.EndInvoke(xInfoClass, blazeOutput, ref xDatos, ref xStringBuilder);
            }
            else {
                /*3- Se vuelve a llamar al motor de reglas*/
                BlazeClass.BlazeInvoke(xInfoClass, solicitud, ref xDatos, ref xStringBuilder);
            }
        }

        /// <summary>
        ///     Autor: Alkinson Gonzalez
        ///     Fecha: 24/12/2018
        ///     Descripcion: Metodo de inicializacion de Metodo contral el Buro Bancario
        /// </summary>
        private static void BancarioStartConsult(DTO_Buro BancoBuro, ref DTO_BURO_BANCARIO_ANALISIS BuroResponse, InfoClass xInfoClass,
            Solicitud xSolicitud,
            ref StringBuilder xStringBuilder)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO INVOCACION DE BURO BANCARIO PAIS " + xInfoClass.STR_COD_PAIS + " | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        string.Join(",", (from property in BancoBuro.GetType().GetProperties()
                                          select string.Concat(property.Name, "=", property.GetValue(BancoBuro))))));

                #endregion

                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA LLAMADO BANCARIO [{0}] -------------------------------------------------------***", oldDate));

                var url = string.Empty;


                try
                {

                    using (HttpClient client = new HttpClient())
                    {
                        var parameters = new Dictionary<string, string> { { "username", BancoBuro.Usuario }, { "password", BancoBuro.Clave } };
                        var encodedContent = new FormUrlEncodedContent(parameters);
                        client.DefaultRequestHeaders.Accept.Clear();
                        CacheControlHeaderValue cacheControl = new CacheControlHeaderValue();
                        cacheControl.NoCache = true;
                        client.DefaultRequestHeaders.CacheControl = cacheControl;
                        var response = client.PostAsync(BancoBuro.URL_TOKEN, encodedContent).GetAwaiter().GetResult();
                        if(!response.IsSuccessStatusCode)
                            throw new BuroExceptions(string.Concat("Problemas de Conexion con el Buro Bancario obteniendo Token", response.StatusCode));
                        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var objBuro = JsonConvert.DeserializeObject<DTO_BURO_BANCARIO_TOKEN>(content);
                        

                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(objBuro.token_type, objBuro.access_token);
                        dynamic buroAnalisis = new ExpandoObject();
                        buroAnalisis.Identificacion = BancoBuro.Cedula;
                        buroAnalisis.TipoId = BancoBuro.TipoIdentificacion;
                        buroAnalisis.idOrigen = xInfoClass.ApplicationCode;
                        buroAnalisis.usuario = xInfoClass.STR_USUARIO_CREACION;
                        buroAnalisis.pais = BancoBuro.Pais;
                        buroAnalisis.Interface = BancoBuro.Interface;
                        if (xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta)
                            buroAnalisis.IdOperacion = BancoBuro.Token;
                        var contentJson = new StringContent(JsonConvert.SerializeObject(buroAnalisis).ToString(), Encoding.UTF8, "application/json");
                        HttpResponseMessage responseAnalisis = client.PostAsync(BancoBuro.URL, contentJson).GetAwaiter().GetResult();
                        if (!responseAnalisis.IsSuccessStatusCode) {
                            if (xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta) {
                                BuroResponse.responseCode = "77";
                                BancarioEndConsult(xInfoClass, ref xSolicitud, BuroResponse, ref xStringBuilder);
                            }
                            throw new BuroExceptions(string.Concat("Problemas de Conexión Servicio obteniendo precalificado ", responseAnalisis.StatusCode));
                        }
                        content = responseAnalisis.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        BuroResponse = JsonConvert.DeserializeObject<DTO_BURO_BANCARIO_ANALISIS>(content);


                        if (xSolicitud.CodigoRespuesta == EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta && BuroResponse.responseCode != "00") {
                            throw new BuroExceptions(string.Concat("Problemas de Conexión Servicio Primera Consulta", responseAnalisis.StatusCode));
                        }
                        
                    }
                }
                catch (Exception e)
                {
                    throw new BuroExceptions(string.Concat("Servicio sin respuesta, favor continuar.", "|", e.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
                }
                

                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA INVOCACION BANCARIO [{0}] -------------------------------------------------------***", newDate));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                #region [Data Trace]

                xInfoClass.Consecutivo++;
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [FIN INVOCACION DE BURO BANCARIO | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7}  | URL: {8} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        BuroResponse, url));

                #endregion
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Servicio sin respuesta, favor continuar.", "|", ex.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
            }
        }

        
        /// <summary>
        ///     Autor: Alkinson Gonzalez
        ///     Fecha: 24/12/2018
        ///     Descripcion: Metodo de Finalizacion del proceso de BANCARIO
        /// </summary>
        /// <param name="xInfoClass">The x information class.</param>
        /// <param name="xSolicitud">The x solicitud.</param>
        /// <param name="BuroAnalisis">The XML response.</param>
        /// <param name="xStringBuilder"></param>
        private static void BancarioEndConsult(InfoClass xInfoClass, ref Solicitud xSolicitud, DTO_BURO_BANCARIO_ANALISIS BuroAnalisis,
            ref StringBuilder xStringBuilder)
        {
            string xProceso = MethodBase.GetCurrentMethod().Name;
            try
            {
                #region [Data Trace]

                string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
                string xClase = string.Format("{0}|{1}",
                    MethodBase.GetCurrentMethod().Module.Name,
                    MethodBase.GetCurrentMethod().DeclaringType.Name);
                string xParametros = string.Empty;

                var methodParams = MethodBase.GetCurrentMethod().GetParameters();

                xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));

                xParametros = string.Format("({0})", xParametros);

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [INICIO DE BURO BANCARIO END CONSULT | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xSolicitud.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(xParametros),
                        ""));

                #endregion

                string xml_Data_enviada = ConvertClass.SerializeObject(xSolicitud);
                string BuroResponse = ConvertClass.SerializeObject(BuroAnalisis).Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", string.Empty);
                #region Parametros

                var dto = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                    {
                        new SpParameter
                        {
                            Name = "INT_FK_BOM_PAR_APLICATIVO",
                            Value = xInfoClass.ApplicationCode
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_UNICO",
                            Value = xSolicitud.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_EXTERNO",
                            Value = xInfoClass.IdentificadorUnico
                        },
                        new SpParameter
                        {
                            Name = "BIN_FK_BOM_LOG_OPERACIONES",
                            Value = xInfoClass.BIN_FK_BOM_LOG_OPERACIONES
                        },
                        new SpParameter
                        {
                            Name = "SIN_FK_UTL_PAR_PAIS",
                            Value = "1"
                        },
                        new SpParameter
                        {
                            Name = "XML_DATA_ENVIADA",
                            Value = xml_Data_enviada
                        },
                        new SpParameter
                        {
                            Name = "SIN_FACE",
                            Value = xSolicitud.Flujo.ToString()
                        }
                        ,
                        new SpParameter
                        {
                            Name = "STR_IDENTIFICADOR_PERSONA_BASE",
                            Value = xSolicitud.IdentificadorUnicoGestion
                        },
                        new SpParameter
                        {
                            Name = "BIN_ID_PERSONA",
                            Value = xSolicitud.Id
                        },
                        new SpParameter
                        {
                            Name = "TIN_ORIGEN",
                            Value = xInfoClass.TIN_ORIGEN
                        },
                        new SpParameter
                        {
                            Name = "INT_ID_MARCA",
                            Value = xInfoClass.INT_ID_MARCA.GetValueOrDefault().ToString()
                        },
                        new SpParameter
                        {
                            Name = "XML_BANCARIO_RESPUESTA",
                            Value = BuroResponse.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", string.Empty)
                        },
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD",
                        Value = xSolicitud.IdAnalisis.GetValueOrDefault().ToString()
                    }
                        ,
                        new SpParameter
                    {
                        Name = "BIN_ID_SOLICITUD_REF",
                        Value = xInfoClass.BIN_ID_SOLICITUD_REF.GetValueOrDefault().ToString()
                    }
                         ,
                        new SpParameter
                    {
                        Name = "INT_SUB_ORIGEN",
                        Value = xInfoClass.INT_SUB_ORIGEN.ToString()
                    }
                    },
                    Result = null,
                    SPName = "PA_PRO_BOM_BANCARIO_RESULTADO"
                };

                dto.Result = null;

                #endregion

                #region [Data Trace]

                xInfoClass.Consecutivo++;

                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [EJECUCION DE {8} | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        xInfoClass.IdentificadorUnico,
                        xInfoClass.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(dto.ParameterList),
                        "EJEUCION DE SP (XML_TUCA_RESPUESTA)",
                        dto.SPName));

                #endregion
                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA AL SP [{0}] -------------------------------------------------------***", dto.SPName));

                DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == xInfoClass.STR_COD_PAIS).FirstOrDefault().Value);
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA AL SP [{0}] -------------------------------------------------------***", dto.SPName));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                var errorList = new List<string>();

                /**************************************************************************************************************/
                /*****************************************    1- CARGA DE BURO   ********************************************/
                /**************************************************************************************************************/
                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CARGA DE BURO [{0}] -------------------------------------------------------***", oldDate));

                IEnumerable<DataRow> rows = ds.Result.Tables[0].Select().Where(x => true);
                List<Buro> buros = new List<Buro>();
                if (rows.Any())
                {
                    buros = JsonConvert.DeserializeObject<List<Buro>>(JsonConvert.SerializeObject(rows.CopyToDataTable())); //ConvertClass.ObjectToList<Buro>(dt, xInfoClass.IdentificadorUnico, ref errorList);

                    //xSolicitud.Persona.ListadoBuros.RemoveAll(x => x.TipoBuro.Equals(buros.FirstOrDefault().TipoBuro));
                    switch (xInfoClass.STR_COD_PAIS)
                    {
                        case Pais.COSTA_RICA:
                        case Pais.SALVADOR:
                            xSolicitud.Persona.ListadoBuros = buros ?? new List<Buro>();
                            break;
                        case Pais.NICARAGUA:
                        case Pais.GUATEMALA:
                            xSolicitud.Persona.ListadoBuros.RemoveAll(x => x.TipoBuro.Equals(buros.FirstOrDefault().TipoBuro));
                            xSolicitud.Persona.ListadoBuros.Add(buros.FirstOrDefault());
                            break;
                    }

                    
                    List<Precalificados> precalificados = new List<Precalificados>();

                    rows = dto.Result.Tables[1].Select();
                    precalificados = JsonConvert.DeserializeObject<List<Precalificados>>(JsonConvert.SerializeObject(rows.CopyToDataTable()));

                    precalificados = (from resPreca in precalificados.AsEnumerable()
                                      join resburo in buros.AsEnumerable() on resPreca.BuroID equals resburo.Id
                                      where resburo.TipoBuro.ToString().Equals("Equifax_Precalificado") ||
                                      resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("Bancario_Precalificado_Segunda_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Primera_Consulta")
                                      || resburo.TipoBuro.ToString().Equals("TransUnion_Precalificado_Segunda_Consulta")
                                      select new Precalificados
                                      {
                                          BuroID = resPreca.BuroID,
                                          Llave = resPreca.Llave,
                                          Valor = resPreca.Valor,
                                          CodigoPrecalificado = resPreca.CodigoPrecalificado
                                      }).ToList();

                    buros.ForEach(x => { x.ListadoPrecalificados = (from res in precalificados where res.BuroID == x.Id select res).ToList(); });

                    newDate = DateTime.Now;
                    ts = newDate - oldDate;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** SALIDA CARGA DE BURO [{0}] -------------------------------------------------------***", newDate));
                    xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

                }
            }
            catch (Exception ex)
            {
                throw new BuroExceptions(string.Concat("Problemas de Conexión Servicio", "|", ex.Message, "|", xProceso, "|", xSolicitud.Persona.BancarioIngresado));
            }
        }


        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: 27/04/2015
        ///     Descripcion: Metodo encargado de la Invocacion para los metodos de TUCA
        /// </summary>
        /// <param name="infDto"></param>
        /// <param name="solicitud"></param>
        internal void BancarioInvoke(InfoClass infDto, BlazeOutput solicitud)
        {
            string xDatos = string.Empty;
            var xStringBuilder = new StringBuilder();

            BancarioInvoke(infDto, solicitud, ref xDatos, ref xStringBuilder);
        }
        #endregion
    }
}