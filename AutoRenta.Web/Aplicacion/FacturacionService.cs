using AutoRenta.Web.Aplicacion.Documentos;
using AutoRenta.Web.Aplicacion.Eventos;
using AutoRenta.Web.Dominio;

namespace AutoRenta.Web.Aplicacion;

/// <summary>
/// Observer concreto: al confirmarse la reserva genera el comprobante PDF y la factura XML.
/// Usa las fábricas del patrón Factory Method para no depender de cómo se crea cada formato.
/// </summary>
public class FacturacionService : IReservaObserver
{
    private readonly IEnumerable<DocumentoFactory> fabricas;
    private readonly string carpeta;

    public FacturacionService(IEnumerable<DocumentoFactory> fabricas, IWebHostEnvironment env)
    {
        this.fabricas = fabricas;
        carpeta = Path.Combine(env.ContentRootPath, "Salidas", "Documentos");
        Directory.CreateDirectory(carpeta);
    }

    public void AlConfirmarReserva(Reserva reserva)
    {
        foreach (var fabrica in fabricas)
        {
            var documento = fabrica.CrearDocumento(reserva);
            File.WriteAllBytes(Path.Combine(carpeta, documento.NombreArchivo), documento.Contenido());
        }
    }

    /// <summary>Devuelve la ruta del comprobante PDF ya generado, o null si no existe.</summary>
    public string? ObtenerComprobante(string codigoReserva)
    {
        var ruta = Path.Combine(carpeta, $"Comprobante_{codigoReserva}.pdf");
        return File.Exists(ruta) ? ruta : null;
    }
}
