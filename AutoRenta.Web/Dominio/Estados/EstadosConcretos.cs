namespace AutoRenta.Web.Dominio.Estados;

/// <summary>El vehículo puede reservarse.</summary>
public class EstadoDisponible : EstadoVehiculoBase
{
    public const string NombreEstado = "Disponible";
    public override string Nombre => NombreEstado;
    public override void Bloquear(Vehiculo v) => v.CambiarEstado(new EstadoReservado());
}

/// <summary>Bloqueo temporal mientras el cliente paga (RN-01).</summary>
public class EstadoReservado : EstadoVehiculoBase
{
    public const string NombreEstado = "Reservado";
    public override string Nombre => NombreEstado;
    public override void LiberarBloqueo(Vehiculo v) => v.CambiarEstado(new EstadoDisponible());
    public override void Confirmar(Vehiculo v) => v.CambiarEstado(new EstadoConfirmado());
}

/// <summary>Reserva pagada; el vehículo espera ser entregado.</summary>
public class EstadoConfirmado : EstadoVehiculoBase
{
    public const string NombreEstado = "Confirmado";
    public override string Nombre => NombreEstado;
    public override void Entregar(Vehiculo v) => v.CambiarEstado(new EstadoAlquilado());
    public override void LiberarBloqueo(Vehiculo v) => v.CambiarEstado(new EstadoDisponible()); // el cliente cancela
}

/// <summary>El vehículo está en manos del cliente.</summary>
public class EstadoAlquilado : EstadoVehiculoBase
{
    public const string NombreEstado = "Alquilado";
    public override string Nombre => NombreEstado;
    public override void RegistrarDevolucion(Vehiculo v, bool conDanos) =>
        v.CambiarEstado(conDanos ? new EstadoMantenimiento() : new EstadoDisponible());
}

/// <summary>El vehículo tiene daños y está en reparación.</summary>
public class EstadoMantenimiento : EstadoVehiculoBase
{
    public const string NombreEstado = "Mantenimiento";
    public override string Nombre => NombreEstado;
    public override void LiberarBloqueo(Vehiculo v) => v.CambiarEstado(new EstadoDisponible()); // reparación terminada
}
