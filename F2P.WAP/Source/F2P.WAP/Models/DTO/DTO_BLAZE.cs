using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F2P.WAP.Models
{
    //[JsonObject(MemberSerialization.OptIn)]
    public class DTO_BLAZE
    {
        #region BLAZE
        public int? INT_ID_MARCA { get; set; }
        [Required]
        public string STR_COD_PAIS { get; set; }
        [Required]
        public string STR_USUARIO_CREACION { get; set; }
        [Required]
        public Int16? SIN_FACE { get; set; }//F1
        [Required]
        public string STR_CEDULA { get; set; }//F1
        public string STR_ORIGEN { get; set; }//F1=inbound
        [Required]
        public Int64 INT_SUB_ORIGEN { get; set; }//F1=inbound
        public string STR_CANAL_VENTAS { get; set; }
        public string STR_PROMOCION { get; set; }
        public string STR_PRODUCTO { get; set; }
        public string STR_CODIGO_FORZAMIENTO { get; set; }
        public string STR_TIPO_PRESTAMO { get; set; }
        public string STR_GENERO { get; set; }
        public string FEC_NACIMIENTO { get; set; }
        public string STR_ESTADO_CIVIL { get; set; }
        public string INT_CANTIDAD_DEPENDIENTES { get; set; }
        public string INT_CANTIDAD_VEHICULOS { get; set; }
        public string STR_PROFESION { get; set; }
        public string STR_PAIS { get; set; }
        public string STR_DIRPROVINCIA { get; set; }
        public string STR_CANTON { get; set; }
        public string STR_DISTRITO { get; set; }
        public string STR_NACIONALIDAD { get; set; }
        public string FEC_INGRESO_LABORAL { get; set; }
        public string STR_INGRESO_BRUTO { get; set; }
        public string STR_OTROS_INGRESO { get; set; }
        public string STR_ACTIVIDAD { get; set; }
        public string STR_OCUPACION { get; set; }
        public string STR_TRAB_DIR_PAIS { get; set; }
        public string STR_TRAB_DIR_PROVINCIA { get; set; }
        public string STR_TRAB_DIR_CANTON { get; set; }
        public string STR_TRAB_DIR_DISTRITO { get; set; }
        public string STR_TEL_CELULAR { get; set; }
        public string STR_TEL_CASA { get; set; }
        public string STR_TEL_TRABAJO { get; set; }
        public string STR_EMAIL { get; set; }
        public bool BIT_BURO_BANCARIO { get; set; }
        public bool BIT_ULTIMO_LLAMADO_TIMER { get; set; }
        public int INT_TIMER_ACTUAL { get; set; }
        public int INT_TIMER_CANTIDAD { get; set; }
        #endregion

        //[JsonProperty(PropertyName = "STR_VARIABLE")]
        public List<VARIABLE> STR_VARIABLE { get; set; }
        public Boolean BIT_GENTE_NOMINA { get; set; }
        public int INT_ID_EMPRESA { get; set; }

        /// <summary>
        /// almacena el numero de referencia de las solicitud del cliente
        /// </summary>
        public Int64? BIN_ID_SOLICITUD_REF { get; set; }

    }

    public class VARIABLE
    {
        public string STR_TIPO { set; get; }
        public string STR_VALOR { set; get; }
        public string STR_DESCRIPCION { set; get; }

    }

    public class RootObject
    {
        public DTO_BLAZE DTO_BLAZE { get; set; }
    }

    public class DTO_BURO_BANCARIO_TOKEN
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class DTO_BURO_BANCARIO_ANALISIS
    {
        public string responseCode { get; set; }
        public int token { get; set; }
        public string mensaje { get; set; }
        public List<BURO_BANCARIO_ANALISIS> data { get; set; }
        public string errorMessage { get; set; }
    }

    public class BURO_BANCARIO_ANALISIS
    {
        public string type { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public bool required { get; set; }
        public bool visible { get; set; }
        public string observation { get; set; }
        public string other_data { get; set; }
    }
}