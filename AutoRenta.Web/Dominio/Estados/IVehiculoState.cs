namespace AutoRenta.Web.Dominio.Estados;

/// <summary>
/// Patrón State: contrato que implementa cada estado del vehículo.
/// Si una acción no es válida en el estado actual, se lanza TransicionNoPermitidaException
/// (este comportamiento forma parte del contrato).
/// </summary>
public interface IVehiculoState
{
    string Nombre { get; }
    void Bloquear(Vehiculo v);
    /// <summary>Libera la unidad: vence o falla el pago, el cliente cancela o termina una reparación.</summary>
    void LiberarBloqueo(Vehiculo v);
    void Confirmar(Vehiculo v);
    void Entregar(Vehiculo v);
    void RegistrarDevolucion(Vehiculo v, bool conDanos);
}

public class TransicionNoPermitidaException : ReglaNegocioException
{
    public TransicionNoPermitidaException(string estado, string accion)
        : base($"No se puede '{accion}' un vehículo en estado {estado}.") { }
}

/// <summary>Base con todas las acciones prohibidas; cada estado habilita solo las suyas.</summary>
public abstract class EstadoVehiculoBase : IVehiculoState
{
    public abstract string Nombre { get; }
    public virtual void Bloquear(Vehiculo v) => throw new TransicionNoPermitidaException(Nombre, "bloquear");
    public virtual void LiberarBloqueo(Vehiculo v) => throw new TransicionNoPermitidaException(Nombre, "liberar");
    public virtual void Confirmar(Vehiculo v) => throw new TransicionNoPermitidaException(Nombre, "confirmar");
    public virtual void Entregar(Vehiculo v) => throw new TransicionNoPermitidaException(Nombre, "entregar");
    public virtual void RegistrarDevolucion(Vehiculo v, bool conDanos) => throw new TransicionNoPermitidaException(Nombre, "registrar la devolución de");
}

/// <summary>Convierte el nombre guardado en la base de datos en el objeto de estado.</summary>
public static class EstadosVehiculo
{
    public static IVehiculoState Desde(string nombre) => nombre switch
    {
        EstadoDisponible.NombreEstado => new EstadoDisponible(),
        EstadoReservado.NombreEstado => new EstadoReservado(),
        EstadoConfirmado.NombreEstado => new EstadoConfirmado(),
        EstadoAlquilado.NombreEstado => new EstadoAlquilado(),
        EstadoMantenimiento.NombreEstado => new EstadoMantenimiento(),
        _ => throw new ArgumentException($"Estado desconocido: {nombre}")
    };
}
