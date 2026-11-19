namespace AutoRenta.Web.Aplicacion.Pagos;

/// <summary>Patrón Adapter – TARGET: contrato de pagos propio de AutoRenta.</summary>
public interface IPasarelaPago
{
    ResultadoPago Cobrar(decimal monto, DatosPago datos);
}

public record DatosPago(string Titular, string NumeroTarjeta, string Vencimiento, string Cvv, string Correo, string Referencia);

public record ResultadoPago(bool Aprobado, string Referencia, string Mensaje);
