using AutoRenta.Web.Dominio.Estados;

namespace AutoRenta.Web.Dominio;

/// <summary>
/// Vehículo de la flota. Es el CONTEXTO del patrón State:
/// guarda su estado actual (IVehiculoState) y le delega las acciones.
/// </summary>
public class Vehiculo
{
    private IVehiculoState? estado;

    public int Id { get; set; }
    public string Placa { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Transmision { get; set; } = "";
    public int Puestos { get; set; }
    public decimal TarifaDiaria { get; set; }

    /// <summary>Nombre del estado actual; es lo que se guarda en la base de datos.</summary>
    public string EstadoActual { get; private set; } = EstadoDisponible.NombreEstado;

    /// <summary>Objeto de estado (State) reconstruido a partir del nombre guardado.</summary>
    private IVehiculoState Estado => estado ??= EstadosVehiculo.Desde(EstadoActual);

    public bool EstaDisponible => EstadoActual == EstadoDisponible.NombreEstado;

    // Cada acción se delega al estado actual, que decide si la transición es válida.
    public void Bloquear() => Estado.Bloquear(this);
    public void LiberarBloqueo() => Estado.LiberarBloqueo(this);
    public void Confirmar() => Estado.Confirmar(this);
    public void Entregar() => Estado.Entregar(this);
    public void RegistrarDevolucion(bool conDanos) => Estado.RegistrarDevolucion(this, conDanos);

    /// <summary>Solo lo usan las clases de estado para pasar al siguiente estado.</summary>
    public void CambiarEstado(IVehiculoState nuevoEstado)
    {
        estado = nuevoEstado;
        EstadoActual = nuevoEstado.Nombre;
    }
}
