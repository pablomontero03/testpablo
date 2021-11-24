using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace F2P.WAP.Models.DTO
{
    public class DTO_UTL_CAT_CODIGOS_RETORNO_API_TRANSLATE
    {
        public int PK_ID { get; set; }
        public string ORIGEN { get; set; }
        public Int64? SUB_ORIGEN { get; set; }
        public string MENSAJE { get; set; }
        public Int64? FLUJO { get; set; }
        public bool ACTIVO { get; set; }
        public string USUARIO_INGRESA { get; set; }
        public string USUARIO_MODIFICA { get; set; }
        public DateTime? FECHA_INGRESO { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string CODIGO { get; set; }
    }
}