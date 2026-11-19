using AutoRenta.Web.Aplicacion;
using AutoRenta.Web.Dominio;
using AutoRenta.Web.Dominio.Estados;
using Microsoft.EntityFrameworkCore;

namespace AutoRenta.Web.Infraestructura;

public interface IVehiculoRepository
{
    List<Vehiculo> ObtenerDisponibles(FiltrosVehiculo filtros);
    Vehiculo? ObtenerPorId(int id);
    List<Vehiculo> ObtenerTodos();
}

public interface IReservaRepository
{
    void Guardar(Reserva reserva);
    void GuardarCambios();
    Reserva? ObtenerPorId(int id);
    List<Reserva> ObtenerPendientesVencidas(DateTime ahora);
    Cliente ObtenerOCrearCliente(string nombre, string correo);
}

public class VehiculoRepository : IVehiculoRepository
{
    private readonly AutoRentaDbContext db;
    public VehiculoRepository(AutoRentaDbContext db) => this.db = db;

    public List<Vehiculo> ObtenerDisponibles(FiltrosVehiculo f)
    {
        var q = db.Vehiculos.Where(v => v.EstadoActual == EstadoDisponible.NombreEstado);
        if (!string.IsNullOrWhiteSpace(f.Categoria)) q = q.Where(v => v.Categoria == f.Categoria);
        if (!string.IsNullOrWhiteSpace(f.Transmision)) q = q.Where(v => v.Transmision == f.Transmision);
        if (!string.IsNullOrWhiteSpace(f.Marca)) q = q.Where(v => v.Marca == f.Marca);
        return q.ToList().OrderBy(v => v.TarifaDiaria).ToList(); // se ordena en memoria: SQLite no ordena columnas decimal
    }

    public Vehiculo? ObtenerPorId(int id) => db.Vehiculos.Find(id);

    public List<Vehiculo> ObtenerTodos() => db.Vehiculos.OrderBy(v => v.Marca).ToList();
}

public class ReservaRepository : IReservaRepository
{
    private readonly AutoRentaDbContext db;
    public ReservaRepository(AutoRentaDbContext db) => this.db = db;

    public void Guardar(Reserva reserva)
    {
        if (reserva.Id == 0) db.Reservas.Add(reserva);
        db.SaveChanges();
    }

    public void GuardarCambios() => db.SaveChanges();

    public Reserva? ObtenerPorId(int id) =>
        db.Reservas.Include(r => r.Vehiculo).Include(r => r.Cliente).FirstOrDefault(r => r.Id == id);

    public List<Reserva> ObtenerPendientesVencidas(DateTime ahora) =>
        db.Reservas.Include(r => r.Vehiculo)
          .Where(r => r.Estado == EstadoReserva.Pendiente && r.VenceBloqueo < ahora).ToList();

    public Cliente ObtenerOCrearCliente(string nombre, string correo)
    {
        var cliente = db.Clientes.FirstOrDefault(c => c.Correo == correo);
        if (cliente is null)
        {
            cliente = new Cliente { Nombre = nombre, Correo = correo };
            db.Clientes.Add(cliente);
        }
        else cliente.Nombre = nombre;
        return cliente;
    }
}
