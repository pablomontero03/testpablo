namespace F2P.WAP.Models
{
    public class DTO_EstadoLineaCredito
    {

        public int? SolicitudID;
        public string EstadoLinea;
        public string Llave;
        public string Valor;
        public string SubEstadoLinea;
        public int ProductoVendido;
        //INICIA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA
        public string IdOperacion;
        public string IdTipoOperacion;
        //FINALIZA, AGONZALEZ, TABLA CONDICION CREDITO FLUJO NIC, GENTE NOMINA
    }
}