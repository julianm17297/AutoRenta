namespace AutoRenta.Web.Dominio;

/// <summary>Se lanza cuando una operación viola una regla de negocio (por ejemplo, reservar un vehículo bloqueado).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
