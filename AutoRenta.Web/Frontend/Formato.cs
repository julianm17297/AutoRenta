using System.Globalization;

namespace AutoRenta.Web.Frontend;

public static class Formato
{
    private static readonly CultureInfo Co = CultureInfo.GetCultureInfo("es-CO");
    public static string Pesos(decimal valor) => valor.ToString("C0", Co);
    public static string Fecha(DateTime f) => f.ToString("dd MMM yyyy", Co);
}
