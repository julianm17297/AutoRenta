using AutoRenta.Web.Dominio;

namespace AutoRenta.Web.Aplicacion.Eventos;

/// <summary>Patrón Observer – OBSERVER: quien quiera enterarse de una reserva confirmada.</summary>
public interface IReservaObserver
{
    void AlConfirmarReserva(Reserva reserva);
}

/// <summary>
/// Patrón Observer – SUBJECT: mantiene la lista de observadores y les avisa
/// cuando se confirma una reserva. ReservaService solo llama a Publicar().
/// </summary>
public class EventosReserva
{
    private readonly List<IReservaObserver> observadores = new();
    private readonly ILogger<EventosReserva> log;

    public EventosReserva(IEnumerable<IReservaObserver> suscritos, ILogger<EventosReserva> log)
    {
        this.log = log;
        foreach (var o in suscritos) Suscribir(o);
    }

    public void Suscribir(IReservaObserver observador) => observadores.Add(observador);

    public void Publicar(Reserva reserva)
    {
        foreach (var o in observadores)
        {
            try { o.AlConfirmarReserva(reserva); }
            catch (Exception ex)
            {
                // Un observador que falla no debe deshacer una reserva ya pagada.
                log.LogError(ex, "El observador {Observador} falló para la reserva {Codigo}", o.GetType().Name, reserva.Codigo);
            }
        }
    }
}
