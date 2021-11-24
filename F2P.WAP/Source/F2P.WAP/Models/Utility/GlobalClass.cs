using System.Collections.Concurrent;
using System.Configuration;

namespace F2P.WAP.Models.Utility
{

    internal class GlobalClass
    {
        //internal static readonly string connectionString =
        //    ConfigurationManager.ConnectionStrings["BlazeConnection"].ConnectionString;

        //public static string connectionString { set; get; }
        public static ConcurrentDictionary<string, string> connectionString = new ConcurrentDictionary<string, string>();
        public static string connectionStringGlobal { set; get; }
        public static ConcurrentDictionary<string, string> blazeUri = new ConcurrentDictionary<string, string>();
        //public static string blazeUri { set; get; }
        public static string STR_PAIS { get; set; }
    }

    public class InfoClass
    {
        public int? INT_ID_MARCA { get; set; }
        public string STR_USUARIO_CREACION { get; set; }
        public string ApplicationCode { get; set; }

        public int Consecutivo { get; set; }

        public string BIN_FK_BOM_LOG_OPERACIONES { get; set; }

        public string PaisCode { get; set; }

        public string IdentificadorUnico { get; set; }

        public string IdentificadorUnicoHilo { get; set; }

        public string SPName_EndInvoke { get; set; }

        public string TIN_ORIGEN { get; set; } // 1 - STS ~ 2 - BATCH ~ 0 - No identificado

        public string xmlObjetSentSOC { get; set; }
        /// <summary>
        /// Cedula del cliente
        /// </summary>
        public string IdentificadorCliente { get; set; }

        public string STR_COD_PAIS { get; set; }
        public System.Int64 INT_SUB_ORIGEN { get; set; }//F1=inbound


        /// <summary>
        /// almacena el numero de referencia de las solicitud del cliente
        /// </summary>
        public System.Int64? BIN_ID_SOLICITUD_REF { get; set; }
        public string STR_TEL_CELULAR { get; set; }
        public string STR_EMAIL { get; set; }
        public bool BIT_BURO_BANCARIO { get; set; }
        public bool BIT_LLAMADO_BANCARIO { get; set; }
        public bool BIT_ULTIMO_LLAMADO_TIMER { get; set; }
        public int INT_TIMER_ACTUAL { get; set; }
        public int INT_TIMER_CANTIDAD { get; set; }
    }
}