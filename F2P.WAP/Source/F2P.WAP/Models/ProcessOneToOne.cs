using F2P.Utilitarios.Common.DataBase;
using F2P.Utilitarios.DataAccess;
using F2P.Utilitarios.Handler;
using F2P.WAP.Models.DTO;
using F2P.WAP.Models.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;


namespace F2P.WAP.Models
{
    public class ProcessOneToOne
    {
        public static JObject ProcessRequest(InfoClass infDto, DTO_BLAZE invoke, ref StringBuilder xStringBuilder)
        {
            string xDatos = string.Empty;



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

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE ProcessRequest | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    infDto.IdentificadorUnico,
                    infDto.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    ""));

            #endregion

            #region Obtener Informacion del Aplicativo

            var xmlObject = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(invoke), "DTO_BLAZE").InnerXml;


            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "INT_PK_BOM_PAR_APLICATIVO",
                        Value = infDto.ApplicationCode
                    },
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = "1"
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = Guid.NewGuid().ToString()//infDto.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = Guid.NewGuid().ToString()//infDto.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "XML_DATA",
                        Value =  xmlObject
                    },
                    new SpParameter
                    {
                        Name = "TIN_ORIGEN",
                        Value = infDto.TIN_ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "INT_TIPO",
                        Value = ConfigurationManager.AppSettings["PA_CON_OBTIENE_APLICATIVO_INVOCACION"]
                    }
                },
                Result = null,
                SPName = "PA_CON_OBTIENE_APLICATIVO"
            };

            #region Conversion de XML en Parametros

            dto.ParameterList.AddRange(from nodo in invoke.GetType().GetProperties()
                                       where nodo.GetValue(invoke) != null
                                       select new SpParameter
                                       {
                                           Name = nodo.Name,
                                           Value = nodo.GetValue(invoke).ToString()
                                       });

            infDto.INT_SUB_ORIGEN = 0;
            infDto.IdentificadorCliente = invoke.STR_CEDULA;
            infDto.STR_COD_PAIS = invoke.STR_COD_PAIS;
            infDto.PaisCode = invoke.STR_COD_PAIS;
            infDto.STR_USUARIO_CREACION = invoke.STR_USUARIO_CREACION;
            infDto.INT_SUB_ORIGEN = invoke.INT_SUB_ORIGEN;
            infDto.BIN_ID_SOLICITUD_REF = invoke.BIN_ID_SOLICITUD_REF.GetValueOrDefault();
            infDto.STR_TEL_CELULAR = invoke.STR_TEL_CELULAR;
            infDto.STR_EMAIL = invoke.STR_EMAIL;
            String connBD = string.Empty;
            String connBlaze = string.Empty;
            switch (invoke.STR_COD_PAIS)
            {
                case Pais.COSTA_RICA:

                    GlobalClass.connectionString.TryGetValue(infDto.STR_COD_PAIS, out connBD);
                    if (connBD == null || string.IsNullOrEmpty(connBD))
                    {
                        GlobalClass.connectionString.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.ConnectionStrings["BlazeConnection_CR"].ConnectionString);
                    }
                    GlobalClass.blazeUri.TryGetValue(infDto.STR_COD_PAIS, out connBlaze);
                    if (connBlaze == null || string.IsNullOrEmpty(connBlaze))
                    {
                        GlobalClass.blazeUri.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.AppSettings["BlazeUri_CR"]);
                    }
                    break;
                case Pais.NICARAGUA:
                    connBD = string.Empty;
                    GlobalClass.connectionString.TryGetValue(infDto.STR_COD_PAIS, out connBD);
                    if (connBD == null)
                    {
                        GlobalClass.connectionString.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.ConnectionStrings["BlazeConnection_NI"].ConnectionString);
                    }
                    GlobalClass.blazeUri.TryGetValue(infDto.STR_COD_PAIS, out connBlaze);
                    if (connBlaze == null || string.IsNullOrEmpty(connBlaze))
                    {
                        GlobalClass.blazeUri.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.AppSettings["BlazeUri_NI"]);
                    }

                    break;
                case Pais.SALVADOR:
                    connBD = string.Empty;
                    GlobalClass.connectionString.TryGetValue(ConfigurationManager.ConnectionStrings["BlazeConnection_SV"].ConnectionString, out connBD);
                    if (connBD == null)
                    {
                        GlobalClass.connectionString.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.ConnectionStrings["BlazeConnection_SV"].ConnectionString);
                    }
                    GlobalClass.blazeUri.TryGetValue(infDto.STR_COD_PAIS, out connBlaze);
                    if (connBlaze == null || string.IsNullOrEmpty(connBlaze))
                    {
                        GlobalClass.blazeUri.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.AppSettings["BlazeUri_SV"]);
                    }

                    break;
                case Pais.GUATEMALA:
                    connBD = string.Empty;
                    GlobalClass.connectionString.TryGetValue(infDto.STR_COD_PAIS, out connBD);
                    if (connBD == null)
                    {
                        GlobalClass.connectionString.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.ConnectionStrings["BlazeConnection_GT"].ConnectionString);
                    }
                    GlobalClass.blazeUri.TryGetValue(infDto.STR_COD_PAIS, out connBlaze);
                    if (connBlaze == null || string.IsNullOrEmpty(connBlaze))
                    {
                        GlobalClass.blazeUri.TryAdd(infDto.STR_COD_PAIS, ConfigurationManager.AppSettings["BlazeUri_GT"]);
                    }

                    break;
            }

            #endregion

            #endregion

            #region [Data Trace]

            infDto.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [Bitacora-Inicio | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    infDto.IdentificadorUnico,
                    infDto.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(dto.ParameterList),
                    "EJECUCION DE SP [PA_CON_OBTIENE_APLICATIVO]"));

            #endregion

            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == infDto.STR_COD_PAIS).FirstOrDefault().Value);

            Solicitud solicitudx = new Solicitud();

            #region Ejecuciones

            long contador = default(Int64);

            if (ds.HasResult)
            {
                var errorList = new List<string>();
                dto.SPName = ds.Result.Tables[0].Rows[0]["STR_PROCEDIMIENTOS"].ToString();

                string BIN_FK_BOM_LOG_OPERACIONES = ds.Result.Tables[0].Rows[0]["BIN_FK_BOM_LOG_OPERACIONES"].ToString();

                infDto.BIN_FK_BOM_LOG_OPERACIONES = BIN_FK_BOM_LOG_OPERACIONES;

                /*Cargo nombre del sp y parametros por defecto*/

                #region Seteo la informacion

                List<SpParameter> parameter = (from DataRow row in ds.Result.Tables[0].Rows
                                               from DataColumn column in ds.Result.Tables[0].Columns
                                               select new SpParameter
                                               {
                                                   Name = column.ColumnName,
                                                   Value = row[column.ColumnName].ToString()
                                               }).ToList();

                #region Conversion de XML en Parametros

                parameter.AddRange(from nodo in invoke.GetType().GetProperties()
                                   where nodo.GetValue(invoke) != null
                                   select new SpParameter
                                   {
                                       Name = nodo.Name,
                                       Value = nodo.GetValue(invoke).ToString()
                                   });

                #endregion

                parameter.Add(
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value = infDto.IdentificadorUnico
                    });

                dto.ParameterList = parameter;
                dto.Result = null;
                #endregion

                #region [Data Trace]
                xStringBuilder.AppendLine(
                    string.Format(
                        "[{0} | {1}] - [Bitacora-Inicio | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                        DateTime.Now,
                        infDto.IdentificadorUnico,
                        infDto.Consecutivo,
                        xAplicativo,
                        xClase,
                        xProceso,
                        ConvertClass.SerializeObject(dto.ParameterList),
                        string.Format("EJECUCION DE SP [{0}]", dto.SPName)));
                #endregion



                xStringBuilder.AppendLine(string.Format("*** ENTRADA AL SP [{0}] ***", dto.SPName));
                DateTime oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("*** {0} ***", DateTime.Now.ToLongTimeString()));
                dto = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == infDto.STR_COD_PAIS).FirstOrDefault().Value);
                xStringBuilder.AppendLine(string.Format("*** SALIDA DEL SP [{0}] ***", dto.SPName));
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;
                xStringBuilder.AppendLine(string.Format("*** {0} ***", newDate));
                xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));

                var obj = JsonConvert.SerializeObject(dto.Result.Tables[0]);


                List<Solicitud> listadoSolicitud = JsonConvert.DeserializeObject<List<Solicitud>>(obj);

                /*ICORTES */
                //RETORNA UN MENSAJE CUANDO EL CLIENTE NO ESTA EN BASES

                if (dto.Result != null &&
                    dto.Result.Tables.Count > 0 & dto.Result.Tables[0].Columns.Contains("RechazoBD"))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(dto.Result.Tables[0].Rows[0]["MensajeRespuesta"].ToString());

                    xDatos = JsonConvert.SerializeObject(doc);
                    return JObject .Parse(xDatos);
                }

                #region Configuracion de Tablas [Listado]

                const string filtroEmpresaID = "EmpresaID";
                const string filtroSolicitudID = "SolicitudID";
                const string filtroBuroID = "BuroID";
                const int tablePersona = 1;
                const int tableIngresos = 2;
                const int tableLineaCredito = 3;
                const int tableMora = 4;
                const int tableProducto = 5;
                const int tablePago = 6;
                const int tableActivos = 7;
                const int tableEmpresa = 8;
                const int tableDireccionEmpresa = 9;
                const int tableContactosEmpresa = 10;
                const int tableIdentificaciones = 11;
                const int tableContactos = 12;
                const int tableDirecciones = 13;
                const int tableBuros = 14;
                const int tablePrecalificados = 15;
                const int tableDecisionesProducto = 16;
                const int tableAlertasFraude = 17;
                const int tableLaboral = 18;
                const int tableDecisionesResultado = 19;
                const int tableEstadosLineaCredito = 20;
                const int tableSegmento = 21;
                const int tableCondicion = 22;
                const int tableListadoVariablesEnviadas = 23;
                //INICIA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA
                const int tableCondicionCredito = 24;
                //FINALIZA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA
                #endregion

                oldDate = DateTime.Now;
                xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** ENTRADA CONVERSION TABLAS MODELO BLAZE [{0}] -------------------------------------------------------***", oldDate));

                //foreach (Solicitud solicitud in listadoSolicitud)
                if (listadoSolicitud.Any())
                {

                    contador++;
                    infDto.TIN_ORIGEN = Convert.ToString((int)listadoSolicitud.FirstOrDefault().TipoLlamada);
                    infDto.xmlObjetSentSOC = xmlObject;
                    infDto.BIT_BURO_BANCARIO = invoke.BIT_BURO_BANCARIO;
                    infDto.BIT_ULTIMO_LLAMADO_TIMER = invoke.BIT_ULTIMO_LLAMADO_TIMER;
                    infDto.INT_TIMER_ACTUAL = invoke.INT_TIMER_ACTUAL;
                    infDto.INT_TIMER_CANTIDAD = invoke.INT_TIMER_CANTIDAD;
                    #region [Data Trace]

                    infDto.Consecutivo++;

                    xStringBuilder.AppendLine(
                        string.Format(
                            "[{0} | {1}] - [Bitacora-Inicio | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                            DateTime.Now,
                            infDto.IdentificadorUnico,
                            infDto.Consecutivo,
                            xAplicativo,
                            xClase,
                            xProceso,
                            ConvertClass.SerializeObject(listadoSolicitud.FirstOrDefault()),
                            string.Format("INICIO DE EJECUCION DE LOGICA [{0}]", listadoSolicitud.FirstOrDefault().CodigoRespuesta)
                            ));

                    #endregion

                    #region Mapeo de Tablas a los Objetos

                    #region CARGA [PERSONA]

                    /**************************************************************************************************************/
                    /****************************************    1- CARGA DE PERSONA    *******************************************/
                    /**************************************************************************************************************/
                    IEnumerable<DataRow> rows = dto.Result.Tables[tablePersona].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Persona> persona = new List<Persona>();

                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable(), Newtonsoft.Json.Formatting.None);

                        persona = JsonConvert.DeserializeObject<List<Persona>>(obj);
                    }

                    /**************************************************************************************************************/
                    /****************************************    1.1- CARGA DE INGRESO    *****************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tableIngresos].Select()
                            .Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Ingresos> ingresos = new List<Ingresos>();

                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        ingresos = JsonConvert.DeserializeObject<List<Ingresos>>(obj);
                    }

                    persona.FirstOrDefault().ListadoIngresos = new List<Ingresos>();
                    persona.FirstOrDefault().ListadoIngresos = ingresos;

                    /**************************************************************************************************************/
                    /*************************************    1.2- CARGA DE LINEA CREDITO    **************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableLineaCredito].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<LineaCredito> lineaCredito = new List<LineaCredito>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        lineaCredito = JsonConvert.DeserializeObject<List<LineaCredito>>(obj);
                    }
                    persona.FirstOrDefault().LineaCredito = lineaCredito.Count > 0 ? lineaCredito[0] : new LineaCredito();

                    /**************************************************************************************************************/
                    /******************************************    1.2.1 - CARGA DE MORA    ***************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableMora].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Mora> mora = new List<Mora>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        mora = JsonConvert.DeserializeObject<List<Mora>>(obj);

                    }
                    persona.FirstOrDefault().LineaCredito.Mora = mora.Count > 0 ? mora[0] : new Mora();

                    /**************************************************************************************************************/
                    /***************************************    1.2.2 - CARGA DE PRODUCTO    **************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tableProducto].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Producto> producto = new List<Producto>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        producto = JsonConvert.DeserializeObject<List<Producto>>(obj);
                    }
                    persona.FirstOrDefault().LineaCredito.Producto = producto.Count > 0 ? producto[0] : new Producto();

                    /**************************************************************************************************************/
                    /*****************************************    1.2.3 - CARGA DE PAGO    ****************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tablePago].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Pago> pago = new List<Pago>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        pago = JsonConvert.DeserializeObject<List<Pago>>(obj);

                    }
                    persona.FirstOrDefault().LineaCredito.Pago = pago.Count > 0 ? pago[0] : new Pago();

                    /**************************************************************************************************************/
                    /*****************************************    1.3- CARGA DE ACTIVOS    ****************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tableActivos].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Activos> activos = new List<Activos>();
                    if (rows.Count() > 0)
                    {

                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        activos = JsonConvert.DeserializeObject<List<Activos>>(obj);
                    }
                    persona.FirstOrDefault().ListadoActivos = activos ?? new List<Activos>();


                    listadoSolicitud.FirstOrDefault().Persona = persona.FirstOrDefault();

                    /**************************************************************************************************************/
                    /*****************************************    1.4- CARGA DE EMPRESA    ****************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tableEmpresa].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));
                    List<Empresa> empresas = new List<Empresa>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        empresas = JsonConvert.DeserializeObject<List<Empresa>>(obj);
                    }
                    listadoSolicitud.FirstOrDefault().Persona.ListadoEmpresas = empresas ?? new List<Empresa>();

                    foreach (Empresa empresa in empresas)
                    {
                        /**************************************************************************************************************/
                        /******************************    1.4.1 - CARGA DE DIRECCIONES EN EMPRESA    *********************************/
                        /**************************************************************************************************************/
                        rows = dto.Result.Tables[tableDireccionEmpresa].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id)
                                         && x[filtroEmpresaID].ToString().Equals(empresa.Id));

                        List<Direccion> direccionEmpresa = new List<Direccion>();
                        if (rows.Count() > 0)
                        {
                            obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                            direccionEmpresa = JsonConvert.DeserializeObject<List<Direccion>>(obj);

                        }
                        empresa.Direcciones = direccionEmpresa.Count > 0 ? direccionEmpresa[0] : new Direccion();
                        /**************************************************************************************************************/
                        /********************************    1.4.2 - CARGA DE CONTACTOS EN EMPRESA   **********************************/
                        /**************************************************************************************************************/

                        rows = dto.Result.Tables[tableContactosEmpresa].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id)
                                         && x[filtroEmpresaID].ToString().Equals(empresa.Id));

                        List<Contacto> contactosEmpresa = new List<Contacto>();

                        if (rows.Count() > 0)
                        {
                            obj = JsonConvert.SerializeObject(rows);

                            contactosEmpresa = JsonConvert.DeserializeObject<List<Contacto>>(obj);

                        }
                        empresa.ListadoContacto = contactosEmpresa ?? new List<Contacto>();
                    }


                    /**************************************************************************************************************/
                    /*************************************    1.5- CARGA DE IDENTIFICACION   **************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableIdentificaciones].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Identificacion> identificaciones = new List<Identificacion>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        identificaciones = JsonConvert.DeserializeObject<List<Identificacion>>(obj);
                    }

                    listadoSolicitud.FirstOrDefault().Persona.ListadoIdentificaciones = identificaciones ?? new List<Identificacion>();

                    /**************************************************************************************************************/
                    /****************************************    1.6- CARGA DE CONTACTO   *****************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableContactos].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Contacto> contactos = new List<Contacto>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        contactos = JsonConvert.DeserializeObject<List<Contacto>>(obj);

                    }
                    listadoSolicitud.FirstOrDefault().Persona.ListadoContactos = contactos ?? new List<Contacto>();

                    /**************************************************************************************************************/
                    /**************************************    1.7- CARGA DE DIRECCION   ******************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableDirecciones].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));


                    List<Direccion> direcciones = new List<Direccion>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        direcciones = JsonConvert.DeserializeObject<List<Direccion>>(obj);
                    }

                    listadoSolicitud.FirstOrDefault().Persona.ListadoDirecciones = direcciones ?? new List<Direccion>();

                    /**************************************************************************************************************/
                    /*****************************************    1.8- CARGA DE BURO   ********************************************/
                    /**************************************************************************************************************/

                    rows = dto.Result.Tables[tableBuros].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Buro> buros = new List<Buro>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        buros = JsonConvert.DeserializeObject<List<Buro>>(obj);

                    }
                    listadoSolicitud.FirstOrDefault().Persona.ListadoBuros = buros ?? new List<Buro>();

                    foreach (Buro buro in buros)
                    {
                        /**************************************************************************************************************/
                        /******************************    1.4.1 - CARGA DE DIRECCIONES EN EMPRESA    *********************************/
                        /**************************************************************************************************************/
                        rows = dto.Result.Tables[tablePrecalificados].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id.ToString())
                                         && x[filtroBuroID].ToString().Equals(buro.Id.ToString()));

                        List<Precalificados> precalificados = new List<Precalificados>();
                        if (rows.Count() > 0)
                        {
                            obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                            precalificados = JsonConvert.DeserializeObject<List<Precalificados>>(obj);
                        }

                        buro.ListadoPrecalificados = precalificados ?? new List<Precalificados>();

                    }


                    /**************************************************************************************************************/
                    /*****************************************    1.8- CARGA DE BURO   ********************************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableLaboral].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));


                    List<Laboral> laboral = new List<Laboral>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        laboral = JsonConvert.DeserializeObject<List<Laboral>>(obj);

                    }
                    listadoSolicitud.FirstOrDefault().Persona.Laboral = laboral.Count > 0 ? laboral[0] : new Laboral();


                    /**************************************************************************************************************/
                    /*1.9- CARGA DE DECISIONES RESULTADO*/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableDecisionesResultado].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<ResultadosDecision> ListadoDecisionesResultado = new List<ResultadosDecision>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        ListadoDecisionesResultado = JsonConvert.DeserializeObject<List<ResultadosDecision>>(obj);

                    }
                    listadoSolicitud.FirstOrDefault().Persona.ListadoDecision = ListadoDecisionesResultado.Count > 0
                        ? ListadoDecisionesResultado
                        : new List<ResultadosDecision>();


                    /**************************************************************************************************************/
                    /******************************  1.4.1 - CARGA LOS DIFERENTES ESTADOS LINEA   *********************************/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableEstadosLineaCredito].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id.ToString(CultureInfo.InvariantCulture)));


                    List<EstadoLineaCredito> ListaEstadoLinea = new List<EstadoLineaCredito>();
                    var tempEstadoLineaCredito = new List<DTO_EstadoLineaCredito>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        ListaEstadoLinea = JsonConvert.DeserializeObject<List<EstadoLineaCredito>>(obj);
                        tempEstadoLineaCredito = JsonConvert.DeserializeObject<List<DTO_EstadoLineaCredito>>(obj);
                    }

                    //Se agrega flujo multipais
                    //Se agrega flujo multipais
                    //switch (infDto.STR_COD_PAIS)
                    //{
                    //    case Pais.GUATEMALA:
                    //    case Pais.SALVADOR:
                    //    case Pais.NICARAGUA:
                    rows = dto.Result.Tables[tableCondicion].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id.ToString(CultureInfo.InvariantCulture)));


                    var tempCondicion = new List<DTO_Condicion>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        tempCondicion = JsonConvert.DeserializeObject<List<DTO_Condicion>>(obj);
                    }

                    if (infDto.STR_COD_PAIS.Equals(Pais.NICARAGUA))
                    {
                        //INICIA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA
                        rows = dto.Result.Tables[tableCondicionCredito].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id.ToString(CultureInfo.InvariantCulture)));

                        var tempCondicionCredito = new List<DTO_CondicionCredito>();
                        if (rows.Count() > 0)
                        {
                            obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                            tempCondicionCredito = JsonConvert.DeserializeObject<List<DTO_CondicionCredito>>(obj);
                        }

                        //FINALIZA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA

                        var res = from credi in tempEstadoLineaCredito
                                  join cond in tempCondicion on credi.ProductoVendido equals cond.ProductoVendido
                                  join condCred in tempCondicionCredito on new { credi.ProductoVendido, credi.IdOperacion } equals new { condCred.ProductoVendido, condCred.IdOperacion }
                                  select new
                                  {
                                      LLave = cond.Llave,
                                      Valor = cond.Valor,
                                      EstadoLinea = credi.EstadoLinea,
                                      SubEstadoLinea = credi.SubEstadoLinea,
                                      ProductoVendido = credi.ProductoVendido,
                                      IdCondicionCredito = condCred.IdCondicionCredito,
                                      SaldoCredito = condCred.SaldoCredito,
                                      LimiteCredito = condCred.LimiteCredito,
                                      IdOperacion = condCred.IdOperacion
                                  };

                        if (res != null && res.Any())
                        {

                            ListaEstadoLinea.ForEach(x =>
                            {
                                x.ListadoCondiciones = new List<Condicion>();
                                Condicion condicion = new Condicion();
                                condicion.Valor = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido).FirstOrDefault().Valor;
                                condicion.Llave = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido).FirstOrDefault().LLave;

                                x.ListadoCondiciones.Add(condicion);
                                Condicion condicionTemp = new Condicion();
                                condicionTemp.Llave = "MarcaAsignada";
                                condicionTemp.Valor = listadoSolicitud.FirstOrDefault().MarcaAsignada;
                                x.ListadoCondiciones.Add(condicionTemp);

                                x.CondicionCredito = new CondicionCredito();
                                x.CondicionCredito.IdCondicionCredito = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido && r.IdOperacion == x.IdOperacion).FirstOrDefault().IdCondicionCredito;
                                x.CondicionCredito.SaldoCredito = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido && r.IdOperacion == x.IdOperacion).FirstOrDefault().SaldoCredito;
                                x.CondicionCredito.LimiteCredito = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido && r.IdOperacion == x.IdOperacion).FirstOrDefault().LimiteCredito;
                            });
                        }
                    }
                    else
                    {
                        var res = from credi in tempEstadoLineaCredito
                                  join cond in tempCondicion on credi.ProductoVendido equals cond.ProductoVendido
                                  select new
                                  {
                                      LLave = cond.Llave,
                                      Valor = cond.Valor,
                                      EstadoLinea = credi.EstadoLinea,
                                      SubEstadoLinea = credi.SubEstadoLinea,
                                      ProductoVendido = credi.ProductoVendido
                                  };

                        if (res != null && res.Any())
                        {

                            ListaEstadoLinea.ForEach(x =>
                            {
                                x.ListadoCondiciones = new List<Condicion>();
                                Condicion condicion = new Condicion();
                                condicion.Valor = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido).FirstOrDefault().Valor;
                                condicion.Llave = res.Where(r => r.EstadoLinea == x.EstadoLinea.ToString() && r.SubEstadoLinea == x.SubEstadoLinea.ToString()
                                && r.ProductoVendido == x.ProductoVendido).FirstOrDefault().LLave;
                                x.ListadoCondiciones.Add(condicion);
                            });
                        }
                    }
                    //break;
                    //}
                    //Se agrega flujo multipais

                    persona.FirstOrDefault().LineaCredito.ListadoEstadoLineaCredito = ListaEstadoLinea.Count > 0
                        ? ListaEstadoLinea
                        : new List<EstadoLineaCredito>();

                    #endregion

                    #region CARGA [DECISIONES PRODUCTO]

                    /**************************************************************************************************************/
                    /*2- CARGA DE DECISIONESPRODUCTO*/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableDecisionesProducto].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<DecisionesProducto> ListadoDecisionesProducto = new List<DecisionesProducto>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());
                        ListadoDecisionesProducto = JsonConvert.DeserializeObject<List<DecisionesProducto>>(obj);
                    }

                    listadoSolicitud.FirstOrDefault().DecisionesProducto = ListadoDecisionesProducto.Count > 0
                        ? ListadoDecisionesProducto[0]
                        : new DecisionesProducto();

                    #endregion

                    #region CARGA [ALERTAS FRAUDE]

                    /**************************************************************************************************************/
                    /*3- CARGA DE ALERTAS FRAUDE*/
                    /**************************************************************************************************************/
                    rows = dto.Result.Tables[tableAlertasFraude].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<AlertasFraude> ListadoAlertaFraude = new List<AlertasFraude>();
                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        ListadoAlertaFraude = JsonConvert.DeserializeObject<List<AlertasFraude>>(obj);

                    }

                    listadoSolicitud.FirstOrDefault().ListadoAlertasFraude = ListadoAlertaFraude;

                    #endregion

                    rows = dto.Result.Tables[tableSegmento].Select();
                    //.Where(
                    //    x =>
                    //        x[filtroSolicitudID].ToString()
                    //            .Equals(listadoSolicitud.FirstOrDefault().Id));

                    List<Segmentos> segmentos = new List<Segmentos>();

                    if (rows.Count() > 0)
                    {
                        obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                        segmentos = JsonConvert.DeserializeObject<List<Segmentos>>(obj);
                    }
                    listadoSolicitud.FirstOrDefault().ListadoSegmentos = segmentos ?? new List<Segmentos>();

                    switch (infDto.STR_COD_PAIS)
                    {
                        case Pais.SALVADOR:

                            /*inicio tableListadoVariablesEnviadas icortes 2017-02-07*/
                            rows = dto.Result.Tables[tableListadoVariablesEnviadas].Select().Where(x => x[filtroSolicitudID].ToString().Equals(listadoSolicitud.FirstOrDefault().Id));


                            List<CategoriaRiesgos> ListVariablesEnviadas = new List<CategoriaRiesgos>();

                            if (rows.Count() > 0)
                            {
                                obj = JsonConvert.SerializeObject(rows.CopyToDataTable());

                                ListVariablesEnviadas = JsonConvert.DeserializeObject<List<CategoriaRiesgos>>(obj);
                            }

                            listadoSolicitud.FirstOrDefault().ListadoCategoriaRiesgos = ListVariablesEnviadas ?? new List<CategoriaRiesgos>();
                            /*fin tableListadoVariablesEnviadas icortes 2017-02-07*/

                            /**************************************************************************************************************/
                            /*7- ASGINACION DE PERSONA A SOLICITUD*/
                            /**************************************************************************************************************/
                            break;
                    }
                    #endregion


                    newDate = DateTime.Now;
                    ts = newDate - oldDate;
                    xStringBuilder.AppendLine(string.Format("-------------------------------------------------------*** FINALIZA CONVERSION TABLAS MODELO BLAZE [{0}] -------------------------------------------------------***", newDate));
                    xStringBuilder.AppendLine(string.Format("********************************************************************** DURACION: {0} **********************************************************************", ts.TotalSeconds));


                    listadoSolicitud.FirstOrDefault().ModoEjecucion = infDto.BIT_BURO_BANCARIO ? 2 : 1;
                    listadoSolicitud.FirstOrDefault().INT_TIMER_ACTUAL = infDto.INT_TIMER_ACTUAL;
                    listadoSolicitud.FirstOrDefault().INT_TIMER_CANTIDAD = infDto.INT_TIMER_CANTIDAD;

                    switch (listadoSolicitud.FirstOrDefault().CodigoRespuesta)
                    {
                        case EnumCodigoRespuesta.IrBlaze:
                            /*EJECUCION DE BLAZE*/
                            BlazeClass.BlazeInvoke(infDto, listadoSolicitud.FirstOrDefault(), ref xDatos, ref xStringBuilder);
                            break;
                        case EnumCodigoRespuesta.IrTuca:
                        case EnumCodigoRespuesta.Ir_Tuca_Primera_Consulta:
                        case EnumCodigoRespuesta.Ir_Tuca_Segunda_Consulta:
                            BuroClass.TucaInvoke(infDto, listadoSolicitud.FirstOrDefault(), ref xDatos, ref xStringBuilder);
                            break;
                        case EnumCodigoRespuesta.IrEquifax:
                            BuroClass.EquifaxInvoke(infDto, listadoSolicitud.FirstOrDefault(), ref xDatos, ref xStringBuilder);
                            break;
                        case EnumCodigoRespuesta.Ir_Bancario_Primera_Consulta:
                        case EnumCodigoRespuesta.Ir_Bancario_Segunda_Consulta:
                            BuroClass.BancarioInvoke(infDto, new BlazeOutput
                            {
                                solicitud = listadoSolicitud.FirstOrDefault()
                            }, ref xDatos, ref xStringBuilder);
                            break;
                        case EnumCodigoRespuesta.IrCore:
                            CoreClass.CoreInvoke(infDto, listadoSolicitud.FirstOrDefault(), ref xDatos, ref xStringBuilder);
                            break;
                        case EnumCodigoRespuesta.IrRenap:
                            BuroClass.RenapInvoke(infDto, listadoSolicitud.FirstOrDefault(), ref xDatos, ref xStringBuilder);
                            break;
                        default:
                            BlazeClass.EndInvoke(infDto,
                                new BlazeOutput
                                {
                                    solicitud = listadoSolicitud.FirstOrDefault()
                                },
                                ref xDatos, ref xStringBuilder);

                            break;
                    }
                }
            }

            #endregion


            return JObject.Parse(xDatos);
        }


        /// <summary>
        ///     Autor: Marlon Dailey Mata
        ///     Fecha: XX/XX/XXXX
        ///     Descripcion:
        /// </summary>
        /// <param name="infDto"></param>
        /// <param name="xStringBuilder">The x string builder.</param>
        /// <param name="PaisCode">The pais code.</param>
        public static void GeneractionClass(InfoClass infDto, ref StringBuilder xStringBuilder, string PaisCode)
        {
            #region [Data Trace]

            string xAplicativo = ConfigurationManager.AppSettings["NombreAplicacion"];
            string xClase = string.Format("{0}|{1}",
                MethodBase.GetCurrentMethod().Module.Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);
            string xProceso = MethodBase.GetCurrentMethod().Name;
            string xParametros = string.Empty;

            ParameterInfo[] methodParams = MethodBase.GetCurrentMethod().GetParameters();
            //for (int i = 0; i < methodParams.Length; i++)
            //{
            //    xParametros = xParametros +
            //                  string.Format("Parametro [{0}] = {1} |", i.ToString(CultureInfo.InvariantCulture),
            //                      methodParams[i]);
            //}
            xParametros = (methodParams.Aggregate("Parametro [{0}]={1}|", (current, pi) => current + String.Format("Parametro [{0}]={1}|", pi.Position, pi.Name).ToString()));
            xParametros = string.Format("({0})", xParametros);

            infDto.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [INICIO DE GeneractionClass | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    infDto.IdentificadorUnico,
                    infDto.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(xParametros),
                    "GENERACION DE DLL"));

            #endregion

            #region Obtener Informacion del Aplicativo

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "SIN_FK_UTL_PAR_PAIS",
                        Value = PaisCode
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_EXTERNO",
                        Value =Guid.NewGuid().ToString() //infDto.IdentificadorUnico
                    },
                    new SpParameter
                    {
                        Name = "STR_IDENTIFICADOR_UNICO",
                        Value = Guid.NewGuid().ToString()//infDto.IdentificadorUnico
                    }
                },
                Result = null,
                SPName = "PA_CON_CREADOR_DLL"
            };

            #endregion

            #region [Data Trace]

            infDto.Consecutivo++;

            xStringBuilder.AppendLine(
                string.Format(
                    "[{0} | {1}] - [Bitacora-Inicio | Consecutivo: {2} | Proyecto: {3} | Clase: {4} | Metodo: {5} | Parametros: {6} | Detalle: {7} ",
                    DateTime.Now,
                    infDto.IdentificadorUnico,
                    infDto.Consecutivo,
                    xAplicativo,
                    xClase,
                    xProceso,
                    ConvertClass.SerializeObject(dto.ParameterList),
                    "EJECUCION DE SP [PA_CON_CREADOR_DLL]"));

            #endregion

            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == infDto.STR_COD_PAIS).FirstOrDefault().Value);

            if (!ds.HasResult) return;

            var str = new StringBuilder();

            var ListPadres = (from data in ds.Result.Tables[0].AsEnumerable()
                              where
                                  data.Field<string>("STR_LLAVE03").Equals(EnumTypeProperty.CLASS.ToString())
                                  ||
                                  data.Field<string>("STR_LLAVE03").Equals(EnumTypeProperty.ENUM.ToString())
                              select new
                              {
                                  INT_PK_BOM_PAR_MODELO = data.Field<Int32>("INT_PK_BOM_PAR_MODELO"),
                                  STR_NOMBRE = data.Field<string>("STR_NOMBRE"),
                                  Description = data.Field<string>("STR_OBSERVACION"),
                                  PropertyType =
                                      (EnumTypeProperty)
                                          Enum.Parse(typeof(EnumTypeProperty), data.Field<string>("STR_LLAVE03")),
                              }).ToList();

            foreach (var obj in ListPadres)
            {
                try
                {
                    List<PropertiesInfo> ListProperties = (from data in ds.Result.Tables[0].AsEnumerable()
                                                           where data.Field<Int32?>("INT_FK_BOM_PAR_MODELO")
                                                               .Equals(obj.INT_PK_BOM_PAR_MODELO)
                                                           select new PropertiesInfo
                                                           {
                                                               PropertyName = data.Field<string>("STR_NOMBRE"),
                                                               PropertyNameType =
                                                                   string.IsNullOrEmpty(
                                                                       data.Field<string>("STR_NOMBRE_CLASS"))
                                                                       ? string.Empty
                                                                       : data.Field<string>("STR_NOMBRE_CLASS"),
                                                               PropertyType = (EnumTypeProperty)
                                                                   Enum.Parse(
                                                                       typeof(EnumTypeProperty),
                                                                       data.Field<string>(
                                                                           "STR_LLAVE03")),
                                                               PropertyValue =
                                                                   data.Field<Int16>("SIN_ORDEN").ToString()
                                                           }).ToList();

                    if (!ListProperties.Any()) continue;

                    if (obj.PropertyType.Equals(EnumTypeProperty.CLASS))
                    {
                        HelperClass.ClassGeneraction(ref str, obj.STR_NOMBRE, obj.Description, ListProperties);
                    }

                    if (obj.PropertyType.Equals(EnumTypeProperty.ENUM))
                    {
                        HelperClass.EnumGeneraction(ref str, obj.STR_NOMBRE, obj.Description, ListProperties);
                    }
                }
                catch (Exception)
                {
                    // CONTROL DE ERROR POR NO TENER NINGUN HIJO LA CLASE O ENUM 
                }
            }
            HelperClass.FileGeneraction(ref str);
        }


        public static string GetConfig(string P_STR_LLAVE_01, string P_STR_LLAVE_02, string P_STR_LLAVE_03, string P_STR_LLAVE_04, string P_STR_LLAVE_05)
        {
            GlobalClass.connectionStringGlobal = ConfigurationManager.ConnectionStrings["BlazeConnection"].ConnectionString;

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "P_STR_LLAVE_01",
                        Value = P_STR_LLAVE_01
                    },
                    new SpParameter
                    {
                        Name = "P_STR_LLAVE_02",
                        Value = P_STR_LLAVE_02
                    },
                    new SpParameter
                    {
                        Name = "P_STR_LLAVE_03",
                        Value = P_STR_LLAVE_03
                    },
                    new SpParameter
                    {
                        Name = "P_STR_LLAVE_04",
                        Value = P_STR_LLAVE_04
                    },
                    new SpParameter
                    {
                        Name = "P_STR_LLAVE_05",
                        Value = P_STR_LLAVE_05
                    }
                },
                Result = null,
                SPName = ConfigurationManager.AppSettings["Sp_Config_Reg"]
            };

            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionStringGlobal);

            if (ds.HasResult && ds.Result.Tables.Count > 0)
            {
                return JsonConvert.SerializeObject(ds.Result.Tables[0]);
            }
            return string.Empty;
        }

        public static string GetConfig(string FK_UTL_PAR_PAIS, string LLAVE_BUSQUEDA, string LLAVE02)
        {
            GlobalClass.connectionStringGlobal = ConfigurationManager.ConnectionStrings["BlazeConnection"].ConnectionString;
            var idApp = 0;
            #region carga identidad

            var dtoApp = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "FK_UTL_PAR_PAIS",
                        Value = FK_UTL_PAR_PAIS
                    },
                    new SpParameter
                    {
                        Name = "LLAVE02",
                        Value = LLAVE02
                    }
                },
                Result = null,
                SPName = "PA_CON_UTL_PAR_PROPIEDAD_APLICATIVO"
            };

            DynamicDto dsApp = DynamicSqlDAO.ExecuterSp(dtoApp, GlobalClass.connectionStringGlobal);

            if (dsApp.HasResult && dsApp.Result.Tables.Count > 0)
            {
                idApp = dsApp.Result.Tables[0].AsEnumerable().Select(x => x.Field<int>("PK_UTL_PAR_APLICATIVO")).FirstOrDefault();
            }




            #endregion
            if (idApp > 0)
            {
                #region carga configuracion aplicativo
                var dto = new DynamicDto
                {
                    ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "FK_UTL_PAR_PAIS",
                        Value = FK_UTL_PAR_PAIS
                    },
                    new SpParameter
                    {
                        Name = "FK_UTL_PAR_APLICATIVO",
                        Value = idApp.ToString()
                    },
                    new SpParameter
                    {
                        Name = "LLAVE_BUSQUEDA",
                        Value = LLAVE_BUSQUEDA
                    }
                },
                    Result = null,
                    SPName = "PA_CON_UTL_PA_PARAMETROS"
                };

                DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionStringGlobal);

                if (ds.HasResult && ds.Result.Tables.Count > 0)
                {
                    return ds.Result.Tables[0].AsEnumerable().Select(x => x.Field<string>("PARAMETROS")).FirstOrDefault();
                }
                #endregion
            }

            return string.Empty;
        }
        public static List<DTO_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE> GetCodigosRetorno(string ORIGEN, string SUB_ORIGEN, string FLUJO, string err,string pais)
        {

            var dto = new DynamicDto
            {
                ParameterList = new List<SpParameter>
                {
                    new SpParameter
                    {
                        Name = "P_ORIGEN",
                        Value = ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "P_SUB_ORIGEN",
                        Value = SUB_ORIGEN
                    },
                    new SpParameter
                    {
                        Name = "P_FLUJO",
                        Value = FLUJO
                    }
                },
                Result = null,
                SPName = "PA_CON_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE"
            };

            DynamicDto ds = DynamicSqlDAO.ExecuterSp(dto, GlobalClass.connectionString.Where(a => a.Key == pais).FirstOrDefault().Value);

            if (ds.HasResult && ds.Result.Tables.Count > 0)
            {
                return JsonConvert.DeserializeObject<List<DTO_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE>>(JsonConvert.SerializeObject(ds.Result.Tables[0])); //ds.Result.Tables[0].AsEnumerable().Select(x => x.Field<string>("PARAMETROS")).FirstOrDefault();
            }
            var obj = new DTO_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE
            {
                CODIGO = "ACK01",
                MENSAJE = err.Replace("<", string.Empty).Replace(">", string.Empty)
            };

            var ret = new List<DTO_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE>();
            ret.Add(obj);

            return ret;
        }
    }
}