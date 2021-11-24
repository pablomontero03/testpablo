namespace F2P.WAP.Models
{

    public class DTO_Condicion
    {
        public int? SolicitudID;
        public string Llave;
        public string Valor;
        public int ProductoVendido;
    }

    public class DTO_CondicionCredito
    {
        public int? SolicitudID;
        public string IdOperacion;
        public string IdCondicionCredito;
        public string SaldoCredito;
        public string LimiteCredito;
        public int ProductoVendido;
    }
}