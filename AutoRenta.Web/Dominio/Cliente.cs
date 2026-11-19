namespace AutoRenta.Web.Dominio;

/// <summary>Cliente que realiza la reserva.</summary>
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
}
