namespace AutoRenta.Web.Aplicacion;

// Objetos que viajan por la API REST (JSON) entre la aplicación web y el backend.
public record FiltrosVehiculo(string? Categoria, string? Transmision, string? Marca);

public record VehiculoDto(int Id, string Placa, string Marca, string Modelo, string Categoria, string Transmision, int Puestos, decimal TarifaDiaria, string Estado);

public record TarifaDto(string Clave, string Nombre, string Descripcion);

public record CotizacionRequest(int VehiculoId, DateTime FechaInicio, DateTime FechaFin, string TipoTarifa);
public record CotizacionDto(int Dias, decimal TarifaDiaria, decimal Subtotal, decimal Seguro, decimal Total, string Tarifa);

public record ReservaRequest(int VehiculoId, DateTime FechaInicio, DateTime FechaFin, string TipoTarifa, string NombreCliente, string CorreoCliente);

public record PagoRequest(string Titular, string NumeroTarjeta, string Vencimiento, string Cvv);

public record ReservaDto(int Id, string? Codigo, string Estado, string EstadoVehiculo, VehiculoDto Vehiculo, string Cliente, string Correo,
    DateTime FechaInicio, DateTime FechaFin, int Dias, string TipoTarifa, decimal Total, DateTime VenceBloqueo, string? ReferenciaPago, string? Observacion);

public record ErrorDto(string Mensaje);
