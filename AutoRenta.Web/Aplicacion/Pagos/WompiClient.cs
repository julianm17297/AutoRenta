namespace AutoRenta.Web.Aplicacion.Pagos;

/// <summary>
/// Patrón Adapter – ADAPTEE: simula el cliente de la API de la pasarela (modo sandbox).
/// Tiene su propio "idioma": montos en centavos, estados en inglés, etc.
/// IMPORTANTE: es una simulación para el prototipo; no realiza cobros reales.
/// </summary>
public class WompiClient
{
    public TransactionResponse CreateTransaction(TransactionRequest request)
    {
        // Regla del sandbox: la tarjeta de prueba 4000 0000 0000 0002 siempre es rechazada.
        var declined = request.PaymentMethod.Token.EndsWith("0002");
        return new TransactionResponse(
            Id: $"TRX-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            Status: declined ? "DECLINED" : "APPROVED",
            StatusMessage: declined ? "Card declined by issuer" : "Approved");
    }
}

public record TransactionRequest(long AmountInCents, string Currency, string CustomerEmail, string Reference, PaymentMethod PaymentMethod);
public record PaymentMethod(string Type, string Token, int Installments);
public record TransactionResponse(string Id, string Status, string StatusMessage);
