namespace F2P.WAP.Models
{
    public class DTO_RENAP
    {
        #region Respuesta
        public class Dpi
        {
            public string cui { get; set; }
            public string depNacimiento { get; set; }
            public string registroCedula { get; set; }
            public string genero { get; set; }
            public string estadoCivil { get; set; }
            public string foto { get; set; }
            public string municNacimiento { get; set; }
            public string nacionalidad { get; set; }
            public string paisnacimiento { get; set; }
            public string fechaNacimiento { get; set; }
            public string primerNombre { get; set; }
            public string segundoNombre { get; set; }
            public string primerApellido { get; set; }
            public string segundoApellido { get; set; }
        }

        public class Ezmovil
        {
            public Dpi dpi { get; set; }
        }

        public class RootObject
        {
            public Ezmovil ezmovil { get; set; }
        }

        #endregion

        #region Consulta

        public class CONSULTA_RENAP
        {
            public string USERID { get; set; }
            public string PORTALID { get; set; }
            public string CUI { get; set; }
        }
        #endregion
    }
}