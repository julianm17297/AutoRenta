using System.Net.Http.Json;
using AutoRenta.Web.Aplicacion;
using Microsoft.AspNetCore.Components;

namespace AutoRenta.Web.Frontend;

/// <summary>
/// Cliente HTTP que usan las pantallas Blazor para llamar a la API REST (IReservaAPI),
/// tal como lo muestran los diagramas: Aplicación Web -> HTTPS/JSON -> ReservaController.
/// </summary>
public class ReservaApiClient
{
    private readonly HttpClient http;

    public ReservaApiClient(IHttpClientFactory fabrica, NavigationManager nav)
    {
        http = fabrica.CreateClient();
        http.BaseAddress = new Uri(nav.BaseUri);
    }

    public async Task<List<VehiculoDto>> ObtenerVehiculos(string? categoria, string? transmision, string? marca)
    {
        var url = $"api/vehiculos?categoria={Uri.EscapeDataString(categoria ?? "")}&transmision={Uri.EscapeDataString(transmision ?? "")}&marca={Uri.EscapeDataString(marca ?? "")}";
        return await http.GetFromJsonAsync<List<VehiculoDto>>(url) ?? new();
    }

    public Task<Resultado<VehiculoDto>> ObtenerVehiculo(int id) => Get<VehiculoDto>($"api/vehiculos/{id}");
    public async Task<List<TarifaDto>> ObtenerTarifas() => await http.GetFromJsonAsync<List<TarifaDto>>("api/tarifas") ?? new();
    public Task<Resultado<CotizacionDto>> Cotizar(CotizacionRequest req) => Post<CotizacionDto>("api/reservas/cotizacion", req);
    public Task<Resultado<ReservaDto>> CrearReserva(ReservaRequest req) => Post<ReservaDto>("api/reservas", req);
    public Task<Resultado<ReservaDto>> Pagar(int id, PagoRequest req) => Post<ReservaDto>($"api/reservas/{id}/pago", req);
    public Task<Resultado<ReservaDto>> ObtenerReserva(int id) => Get<ReservaDto>($"api/reservas/{id}");

    private async Task<Resultado<T>> Get<T>(string url) => await Leer<T>(await http.GetAsync(url));
    private async Task<Resultado<T>> Post<T>(string url, object cuerpo) => await Leer<T>(await http.PostAsJsonAsync(url, cuerpo));

    private static async Task<Resultado<T>> Leer<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return new(await resp.Content.ReadFromJsonAsync<T>(), null);
        var error = await resp.Content.ReadFromJsonAsync<ErrorDto>();
        return new(default, error?.Mensaje ?? "Ocurrió un error inesperado.");
    }
}

public record Resultado<T>(T? Datos, string? Error)
{
    public bool Ok => Error is null;
}
