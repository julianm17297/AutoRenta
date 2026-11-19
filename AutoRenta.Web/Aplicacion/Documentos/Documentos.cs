using System.Globalization;
using System.Text;
using System.Xml.Linq;
using AutoRenta.Web.Dominio;

namespace AutoRenta.Web.Aplicacion.Documentos;

/// <summary>Patrón Factory Method – PRODUCT: documento que se genera al confirmar.</summary>
public interface IDocumento
{
    string NombreArchivo { get; }
    string Extension();
    byte[] Contenido();
}

/// <summary>
/// Patrón Factory Method – CREATOR: define el método fábrica CrearDocumento().
/// Las subclases deciden qué documento concreto crear.
/// </summary>
public abstract class DocumentoFactory
{
    public abstract IDocumento CrearDocumento(Reserva reserva);

    public byte[] Generar(Reserva reserva) => CrearDocumento(reserva).Contenido();
}

public class ComprobantePdfFactory : DocumentoFactory
{
    public override IDocumento CrearDocumento(Reserva reserva) => new ComprobantePdf(reserva);
}

public class FacturaXmlFactory : DocumentoFactory
{
    public override IDocumento CrearDocumento(Reserva reserva) => new FacturaXml(reserva);
}

/// <summary>Comprobante de reserva en PDF para el cliente.</summary>
public class ComprobantePdf : IDocumento
{
    private readonly Reserva r;
    public ComprobantePdf(Reserva reserva) => r = reserva;
    public string NombreArchivo => $"Comprobante_{r.Codigo}.pdf";
    public string Extension() => "pdf";

    public byte[] Contenido()
    {
        var co = CultureInfo.GetCultureInfo("es-CO");
        var lineas = new List<(string, int, bool)>
        {
            ("AutoRenta - Sistema de Alquiler de Vehículos", 18, true),
            ("Comprobante de reserva", 14, true),
            ("", 10, false),
            ($"Código de reserva: {r.Codigo}", 13, true),
            ($"Fecha de emisión: {DateTime.Now.ToString("dd/MM/yyyy HH:mm", co)}", 11, false),
            ("", 10, false),
            ($"Cliente: {r.Cliente.Nombre}", 11, false),
            ($"Correo: {r.Cliente.Correo}", 11, false),
            ("", 10, false),
            ($"Vehículo: {r.Vehiculo.Marca} {r.Vehiculo.Modelo} ({r.Vehiculo.Categoria})", 11, false),
            ($"Placa: {r.Vehiculo.Placa}   Transmisión: {r.Vehiculo.Transmision}", 11, false),
            ($"Desde: {r.FechaInicio:dd/MM/yyyy}   Hasta: {r.FechaFin:dd/MM/yyyy}   ({r.Dias} días)", 11, false),
            ($"Tarifa aplicada: {r.TipoTarifa}", 11, false),
            ("", 10, false),
            ($"Total pagado: {r.CostoEstimado.ToString("C0", co)} COP", 13, true),
            ($"Referencia de pago: {r.ReferenciaPago}", 11, false),
            ("", 10, false),
            ("Presente este código en la agencia al recoger el vehículo.", 10, false),
            ("Documento generado automáticamente por el prototipo AutoRenta.", 9, false),
        };
        return MiniPdf.Crear(lineas);
    }
}

/// <summary>Factura electrónica en XML (estructura simplificada).</summary>
public class FacturaXml : IDocumento
{
    private readonly Reserva r;
    public FacturaXml(Reserva reserva) => r = reserva;
    public string NombreArchivo => $"Factura_{r.Codigo}.xml";
    public string Extension() => "xml";

    public byte[] Contenido()
    {
        var inv = CultureInfo.InvariantCulture;
        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("FacturaElectronica",
                new XElement("Numero", r.Codigo),
                new XElement("FechaEmision", DateTime.Now.ToString("s", inv)),
                new XElement("Emisor", new XElement("Nombre", "AutoRenta S.A.S."), new XElement("NIT", "900000000-0")),
                new XElement("Adquiriente", new XElement("Nombre", r.Cliente.Nombre), new XElement("Correo", r.Cliente.Correo)),
                new XElement("Detalle",
                    new XElement("Descripcion", $"Alquiler {r.Vehiculo.Marca} {r.Vehiculo.Modelo} - placa {r.Vehiculo.Placa}"),
                    new XElement("Dias", r.Dias),
                    new XElement("Tarifa", r.TipoTarifa)),
                new XElement("Total", new XAttribute("moneda", "COP"), r.CostoEstimado.ToString("0.00", inv)),
                new XElement("ReferenciaPago", r.ReferenciaPago)));
        return Encoding.UTF8.GetBytes(xml.Declaration + Environment.NewLine + xml.ToString());
    }
}

/// <summary>Generador mínimo de PDF de una página (sin librerías externas).</summary>
public static class MiniPdf
{
    public static byte[] Crear(IEnumerable<(string Texto, int Tamano, bool Negrita)> lineas)
    {
        var contenido = new StringBuilder();
        var y = 760;
        contenido.Append("0.96 0.76 0 rg 0 792 612 -10 re f 0 0 0 rg\n");
        foreach (var (texto, tamano, negrita) in lineas)
        {
            contenido.Append($"BT /{(negrita ? "F2" : "F1")} {tamano} Tf 60 {y} Td ({Escapar(texto)}) Tj ET\n");
            y -= tamano + 10;
        }
        var stream = Encoding.Latin1.GetBytes(contenido.ToString());

        var objetos = new List<byte[]>
        {
            Encoding.Latin1.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"),
            Encoding.Latin1.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            Encoding.Latin1.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>"),
            Encoding.Latin1.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            Encoding.Latin1.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"),
            Encoding.Latin1.GetBytes($"<< /Length {stream.Length} >>\nstream\n").Concat(stream).Concat(Encoding.Latin1.GetBytes("\nendstream")).ToArray()
        };

        using var ms = new MemoryStream();
        void W(string s) { var b = Encoding.Latin1.GetBytes(s); ms.Write(b, 0, b.Length); }
        W("%PDF-1.4\n");
        var offsets = new List<long>();
        for (var i = 0; i < objetos.Count; i++)
        {
            offsets.Add(ms.Position);
            W($"{i + 1} 0 obj\n"); ms.Write(objetos[i]); W("\nendobj\n");
        }
        var xref = ms.Position;
        W($"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offsets) W($"{o:D10} 00000 n \n");
        W($"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return ms.ToArray();
    }

    private static string Escapar(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
