namespace AutoRenta.Web.Aplicacion.Pagos;

/// <summary>
/// Patrón Adapter – ADAPTER: implementa el contrato propio (IPasarelaPago)
/// y traduce cada llamada al formato de la API de la pasarela (WompiClient).
/// Cambiar de proveedor solo exige escribir otro adaptador.
/// </summary>
public class WompiPagoAdapter : IPasarelaPago
{
    private readonly WompiClient cliente;

    public WompiPagoAdapter(WompiClient cliente) => this.cliente = cliente;

    public ResultadoPago Cobrar(decimal monto, DatosPago datos)
    {
        // 1. Traducir de "idioma AutoRenta" a "idioma de la pasarela"
        var solicitud = new TransactionRequest(
            AmountInCents: (long)(monto * 100),
            Currency: "COP",
            CustomerEmail: datos.Correo,
            Reference: datos.Referencia,
            PaymentMethod: new PaymentMethod("CARD", datos.NumeroTarjeta.Replace(" ", ""), Installments: 1));

        var respuesta = cliente.CreateTransaction(solicitud);

        // 2. Traducir la respuesta de vuelta al contrato propio
        var aprobado = respuesta.Status == "APPROVED";
        return new ResultadoPago(aprobado, respuesta.Id,
            aprobado ? "Pago aprobado" : "La entidad financiera rechazó el pago");
    }
}
