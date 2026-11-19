using System.Globalization;
using System.Net;
using AutoRenta.Web.Aplicacion.Eventos;
using AutoRenta.Web.Dominio;

namespace AutoRenta.Web.Aplicacion;

/// <summary>
/// Observer concreto: al confirmarse la reserva envía el correo de confirmación (RF-02).
/// En el prototipo el envío es SIMULADO: el correo se guarda como archivo HTML
/// en la carpeta Salidas/Correos para poder mostrarlo.
/// </summary>
public class NotificacionService : IReservaObserver
{
    private readonly FacturacionService facturacion;
    private readonly string carpeta;
    private readonly ILogger<NotificacionService> log;

    public NotificacionService(FacturacionService facturacion, IWebHostEnvironment env, ILogger<NotificacionService> log)
    {
        this.facturacion = facturacion;
        this.log = log;
        carpeta = Path.Combine(env.ContentRootPath, "Salidas", "Correos");
        Directory.CreateDirectory(carpeta);
    }

    public void AlConfirmarReserva(Reserva reserva)
    {
        var adjunto = facturacion.ObtenerComprobante(reserva.Codigo!);
        var co = CultureInfo.GetCultureInfo("es-CO");
        string E(string s) => WebUtility.HtmlEncode(s);
        var html = $"""
            <!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Confirmación {E(reserva.Codigo!)}</title></head>
            <body style="font-family:Segoe UI,Arial,sans-serif;background:#f3f5f9;padding:24px">
            <div style="max-width:560px;margin:auto;background:#fff;border-radius:10px;overflow:hidden;border:1px solid #dde3ec">
              <div style="background:#0c0c0e;color:#fff;padding:16px 20px;font-size:20px;font-weight:800">Auto<span style="color:#f6c300">Renta</span></div>
              <div style="padding:20px">
                <p style="color:#666;font-size:13px">Para: {E(reserva.Cliente.Correo)}<br>Asunto: Confirmación de tu reserva {E(reserva.Codigo!)}</p>
                <h2 style="color:#0c0c0e">¡Tu reserva está confirmada!</h2>
                <p>Hola {E(reserva.Cliente.Nombre)}, recibimos tu pago. Estos son los datos de tu reserva:</p>
                <p style="font-size:22px;font-weight:700;color:#b38f00">{E(reserva.Codigo!)}</p>
                <p><b>Vehículo:</b> {E(reserva.Vehiculo.Marca)} {E(reserva.Vehiculo.Modelo)} ({E(reserva.Vehiculo.Placa)})<br>
                   <b>Fechas:</b> {reserva.FechaInicio:dd/MM/yyyy} al {reserva.FechaFin:dd/MM/yyyy} ({reserva.Dias} días)<br>
                   <b>Total pagado:</b> {reserva.CostoEstimado.ToString("C0", co)} COP</p>
                <p><b>Adjunto:</b> {(adjunto is null ? "(sin comprobante)" : E(Path.GetFileName(adjunto)))}</p>
                <p style="color:#666;font-size:12px">Correo simulado por el prototipo AutoRenta (no se envió a un servidor SMTP real).</p>
              </div>
            </div></body></html>
            """;
        EnviarCorreo(reserva.Codigo!, html);
    }

    private void EnviarCorreo(string codigo, string html)
    {
        File.WriteAllText(Path.Combine(carpeta, $"Correo_{codigo}.html"), html);
        log.LogInformation("Correo de confirmación {Codigo} enviado (simulado).", codigo);
    }

    public string? ObtenerCorreo(string codigo)
    {
        var ruta = Path.Combine(carpeta, $"Correo_{codigo}.html");
        return File.Exists(ruta) ? ruta : null;
    }
}
