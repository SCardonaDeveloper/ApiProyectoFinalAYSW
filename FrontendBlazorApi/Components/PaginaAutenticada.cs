using Microsoft.AspNetCore.Components;
using FrontendBlazorApi.Servicios;

namespace FrontendBlazorApi.Components;

public abstract class PaginaAutenticada : ComponentBase
{
    [Inject] protected ServicioAutenticacion ServicioAuth { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected bool AutenticacionVerificada { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await VerificarAutenticacion();
            StateHasChanged();
        }
    }

    private async Task VerificarAutenticacion()
    {
        var autenticado = await ServicioAuth.EstaAutenticadoAsync();

        if (!autenticado)
        {
            Navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        AutenticacionVerificada = true;
    }
     protected virtual Task OnAutenticacionVerificada()
    {
        return Task.CompletedTask;
    }
}
