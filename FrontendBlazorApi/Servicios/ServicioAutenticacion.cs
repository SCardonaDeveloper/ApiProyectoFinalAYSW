using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace FrontendBlazorApi.Servicios;

public class ServicioAutenticacion
{
    private readonly IJSRuntime _js;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _tokenEnMemoria;

    public ServicioAutenticacion(IJSRuntime js, IHttpClientFactory httpClientFactory)
    {
        _js = js;
        _httpClientFactory = httpClientFactory;
    }
    public async Task IniciarSesionAsync(string token, string email)
    {
        await _js.InvokeVoidAsync("sessionStorage.setItem", "token", token);
        await _js.InvokeVoidAsync("sessionStorage.setItem", "email", email);
        _tokenEnMemoria = token;
    }
    public async Task CerrarSesionAsync()
    {
        await _js.InvokeVoidAsync("sessionStorage.clear");
        _tokenEnMemoria = null;
    }
    public async Task<string?> ObtenerTokenValidoAsync()
    {
        if (!string.IsNullOrWhiteSpace(_tokenEnMemoria))
            return _tokenEnMemoria;

        var token = await _js.InvokeAsync<string>("sessionStorage.getItem", "token");

        if (!string.IsNullOrWhiteSpace(token))
            _tokenEnMemoria = token;

        return token;
    }
    public async Task<bool> EstaAutenticadoAsync()
    {
        var token = await ObtenerTokenValidoAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
    public async Task<HttpClient> ObtenerClienteAutenticadoAsync()
    {
        var cliente = _httpClientFactory.CreateClient("ApiGenerica");
        var token = await ObtenerTokenValidoAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            cliente.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return cliente;
    }
    public async Task<string?> ObtenerEmailUsuarioAsync()
    {
        try
        {
            return await _js.InvokeAsync<string>("sessionStorage.getItem", "email");
        }
        catch
        {
            return null;
        }
    }
}
