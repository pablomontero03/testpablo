using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace F2P.WAP.Models.DTO
{
    public class Decision
    {
        public string denegacionAnulable { get; set; }
        public string reasonCode { get; set; }
        public string type { get; set; }
    }

    public class Business
    {
        public List<Decision> decisions { get; set; }
    }

    public class OtrasCuotas
    {
        public string otrasCuotas { get; set; }
}

public class EmbargoJudicial
{
    public string embargoJudicial { get; set; }
}

public class Ingresos
{
    public string Tipo { get; set; }
    public string FechaCarga { get; set; }
    public string IngresosNetos { get; set; }
    public string IngresosBrutos { get; set; }
    public string OtrosIngreso { get; set; }
    public string GastosHipotecariosRenta { get; set; }
    public string GastosCredito { get; set; }
    public string OtrosGastos { get; set; }
}

public class ListadoIngresos
{
    public Ingresos Ingresos { get; set; }
}

public class CuotaCalculada
{
    public string cuotaCalculada { get; set; }
}

public class Mora
{
    public string DiasMora_30 { get; set; }
    public string DiasMora_60 { get; set; }
    public string DiasMora_90 { get; set; }
    public string DiasMora_120 { get; set; }
    public string PoliticaPerdon { get; set; }
    public string TipoMora { get; set; }
}

public class Producto
{
    public string ComisionApertura { get; set; }
    public string LimiteSolicitado { get; set; }
    public string Plazo { get; set; }
}

public class Pago
{
    public string NumeroCuota { get; set; }
    public string MontoCuota { get; set; }
}

public class LineaCredito
{
    public string Id { get; set; }
    public string FechaApertura { get; set; }
    public string FechaCancelacion { get; set; }
    public string Saldo { get; set; }
    public string MontoInicial { get; set; }
    public string MontoCuota { get; set; }
    public string MontoCuotaMensual { get; set; }
    public string PagoVencidos { get; set; }
    public string PagoPendientes { get; set; }
    public string PagoRealizados { get; set; }
    public string RangoPago { get; set; }
    public string PctgAumento { get; set; }
    public string CantidaAumentos { get; set; }
    public string FechaUltimoAumento { get; set; }
    public CuotaCalculada CuotaCalculada { get; set; }
    public object ListadoEstadoLineaCredito { get; set; }
    public Mora Mora { get; set; }
    public Producto Producto { get; set; }
    public Pago Pago { get; set; }
    public string CuotaComercial { get; set; }
    public string CuotaOID { get; set; }
    public string CuotaChamba { get; set; }
}

public class ResultadosDecision
{
    public string CodigoRazon { get; set; }
    public string FechaRazon { get; set; }
}

public class ListadoDecision
{
    public List<ResultadosDecision> ResultadosDecision { get; set; }
}

public class Activos
{
    public string CantidadActivos { get; set; }
    public string ValorActivo { get; set; }
    public string TipoActivo { get; set; }
    public string Hipotecado { get; set; }
}

public class ListadoActivos
{
    public Activos Activos { get; set; }
}

public class Direcciones
{
    public string TipoDireccion { get; set; }
    public string NivelDireccion1 { get; set; }
    public string NivelDireccion2 { get; set; }
    public string NivelDireccion3 { get; set; }
    public string NivelDireccion4 { get; set; }
    public string NivelDireccion5 { get; set; }
    public string NivelDireccion6 { get; set; }
}

public class Empresa
{
    public string Id { get; set; }
    public string FechaConstitucion { get; set; }
    public string CantidadColaboradores { get; set; }
    public string Ingresos { get; set; }
    public string ActividadEmpresa { get; set; }
    public string TipoEmpresa { get; set; }
    public Direcciones Direcciones { get; set; }
    public object ListadoContacto { get; set; }
}

public class ListadoEmpresas
{
    public Empresa Empresa { get; set; }
}

public class Identificacion
{
    public string Valor { get; set; }
    public string TipoIdentificaion { get; set; }
}

public class ListadoIdentificaciones
{
    public Identificacion Identificacion { get; set; }
}

public class Contacto
{
    public string Valor { get; set; }
    public string TipoContacto { get; set; }
    public string MedioContacto { get; set; }
}

public class ListadoContactos
{
    public List<Contacto> Contacto { get; set; }
}

public class Direccion
{
    public object OtrosDatos { get; set; }
    public string NivelRiesgo { get; set; }
    public string TipoDireccion { get; set; }
    public string NivelDireccion1 { get; set; }
    public string NivelDireccion2 { get; set; }
    public string NivelDireccion3 { get; set; }
    public string NivelDireccion4 { get; set; }
    public string NivelDireccion5 { get; set; }
    public string NivelDireccion6 { get; set; }
}

public class ListadoDirecciones
{
    public Direccion Direccion { get; set; }
}

public class Laboral
{
    public string NombreEmpresa { get; set; }
    public string FechaIngreso { get; set; }
    public string Ocupacion { get; set; }
    public string ActividadEmpresa { get; set; }
    public string DepartamentoEmpresa { get; set; }
}

public class Precalificado
{
    public string Llave { get; set; }
    public string Valor { get; set; }
    public string CodigoPrecalificado { get; set; }
}

public class ListadoPrecalificados
{
    public List<Precalificado> Precalificados { get; set; }
}

public class Buro
{
    public string Id { get; set; }
    public string FechaUltimaConsulta { get; set; }
    public string TipoBuro { get; set; }
    public ListadoPrecalificados ListadoPrecalificados { get; set; }
}

public class ListadoBuros
{
    public Buro Buro { get; set; }
}

public class Persona
{
    public string FechaNacimiento { get; set; }
    public string NumeroDependientes { get; set; }
    public string Edad { get; set; }
    public string CatidadSolicitudActivas { get; set; }
    public string CatidadSolicitudAprobadas { get; set; }
    public string FechaUltimaAprobacion { get; set; }
    public string CatidadVehiculesEnbargables { get; set; }
    public string VehiculoNuevoUsado { get; set; }
    public string FechaIngresoTrabajoAnterior { get; set; }
    public string FechaSalidaTrabajoAnterior { get; set; }
    public string FechaResidencia { get; set; }
    public string TipoVivienda { get; set; }
    public string AntiguedadVivienda { get; set; }
    public string Perfil { get; set; }
    public string FechaOfertaCrediticia { get; set; }
    public string Genero { get; set; }
    public string TieneVehiculo { get; set; }
    public string EstadoCivil { get; set; }
    public string Profesion { get; set; }
    public string TipoSolicitante { get; set; }
    public string Nacionalidad { get; set; }
    public ListadoIngresos ListadoIngresos { get; set; }
    public string PEP { get; set; }
    public string NivelEducativo { get; set; }
    public LineaCredito LineaCredito { get; set; }
    public ListadoDecision ListadoDecision { get; set; }
    public ListadoActivos ListadoActivos { get; set; }
    public ListadoEmpresas ListadoEmpresas { get; set; }
    public ListadoIdentificaciones ListadoIdentificaciones { get; set; }
    public ListadoContactos ListadoContactos { get; set; }
    public ListadoDirecciones ListadoDirecciones { get; set; }
    public Laboral Laboral { get; set; }
    public ListadoBuros ListadoBuros { get; set; }
    public string ReferenciaTelfCalculada { get; set; }
}

public class DecisionesProducto
{
    public string LimiteCredito { get; set; }
    public string PropensiveScore { get; set; }
    public string AsignacionPctgLinea { get; set; }
    public string AsignacionLinea { get; set; }
    public string FinalScore { get; set; }
    public string PreScore { get; set; }
    public string FinalScoreTUCA { get; set; }
    public string FinalScoreEquifax { get; set; }
    public string FinalScoreRenap { get; set; }
    public string CutOffPorSegmento { get; set; }
    public string ValorMatriz { get; set; }
    public string ValorDemo { get; set; }
    public string ValorClasificacionTipoReferencia { get; set; }
    public string IdSegmento { get; set; }
    public string AsignacionSegmentoLinea { get; set; }
}

public class AlertasFraude
{
    public string Descripcion { get; set; }
    public string TipoAlertasFraude { get; set; }
}

public class ListadoAlertasFraude
{
    public AlertasFraude AlertasFraude { get; set; }
}

public class Segmento
{
    public string Cantidad { get; set; }
    public string segmento { get; set; }
    public string Descripcion { get; set; }
}

public class ListadoSegmentos
{
    public List<Segmento> Segmentos { get; set; }
}

public class Solicitud
{
    public string IdAnalisis { get; set; }
    public string Id { get; set; }
    public string IdentificadorUnico { get; set; }
    public string IdentificadorUnicoGestion { get; set; }
    public string StrategyCode { get; set; }
    public string DiasDiferencia { get; set; }
    public string SourceCode { get; set; }
    public string FechaSolicitud { get; set; }
    public string FechaSistema { get; set; }
    public object DatosRespuesta { get; set; }
    public string Flujo { get; set; }
    public string UtilizarEquifax { get; set; }
    public string UtilizarTuca { get; set; }
    public string UtilizarRenap { get; set; }
    public object CodigoForzamientoPorExcepcion { get; set; }
    public OtrasCuotas OtrasCuotas { get; set; }
    public EmbargoJudicial EmbargoJudicial { get; set; }
    public string DiasMoraMaxSSF { get; set; }
    public string FlagExcepcionCatE { get; set; }
    public string SaldoCDConsolidacion { get; set; }
    public string SaldoCDConsumo { get; set; }
    public string SaldoHipo { get; set; }
    public string SaldoSO { get; set; }
    public string SaldoTC { get; set; }
    public string ClienteManta { get; set; }
    public string Pais { get; set; }
    public string Origen { get; set; }
    public string TipoLlamada { get; set; }
    public Persona Persona { get; set; }
    public DecisionesProducto DecisionesProducto { get; set; }
    public string CodigoRespuesta { get; set; }
    public ListadoAlertasFraude ListadoAlertasFraude { get; set; }
    public ListadoSegmentos ListadoSegmentos { get; set; }
}

public class CharacteristicInfo
{
    public string PartialScore { get; set; }
    public string Characteristic { get; set; }
    public string Label { get; set; }
}

public class ModelInfo
{
    public string Name { get; set; }
    public string Total { get; set; }
    public List<CharacteristicInfo> CharacteristicInfo { get; set; }
}

public class ListadoScore
{
    public List<ModelInfo> ModelInfo { get; set; }
}

public class FiredRule
{
    public string name { get; set; }
    public string ruleset { get; set; }
}

public class Variable
{
    public string Llave { get; set; }
    public string Valor { get; set; }
    public string OtrosDatos { get; set; }
}

public class ListadoVariables
{
    public List<Variable> Variable { get; set; }
}

public class BlazeOutput
{
    public Business business { get; set; }
    public object ListadoLimites { get; set; }
    public Solicitud solicitud { get; set; }
    public ListadoScore ListadoScore { get; set; }
    public List<FiredRule> firedRules { get; set; }
    public ListadoVariables ListadoVariables { get; set; }
    public string log { get; set; }
}

public class RootObjectBlazeOutput
    {
    public BlazeOutput BlazeOutput { get; set; }
}
}