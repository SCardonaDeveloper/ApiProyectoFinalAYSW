// Program.cs
// Archivo de arranque de la aplicación Blazor Server.
// Se registran servicios y se configura el pipeline HTTP.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var constructor = WebApplication.CreateBuilder(args);

// ----------------------
// Registro de servicios
// ----------------------

// Motor de Razor Pages (requerido por Blazor Server para la página _Host).
constructor.Services.AddRazorPages();

// Núcleo de Blazor Server (circuitos SignalR, prerenderizado, etc.).
constructor.Services.AddServerSideBlazor();

/*
 // Servicio HttpClient para consumir la API externa de productos.
 // Se deja comentado y se activará cuando se implemente el servicio tipado.
 constructor.Services.AddHttpClient("ApiProductos", cliente =>
 {
     // URL base de la API .NET que expone /api/producto
     cliente.BaseAddress = new Uri("http://localhost:5031/");
     // Se pueden agregar encabezados por defecto aquí si se requiere.
 });
*/

/*
 // Política CORS opcional. Útil si en algún momento el navegador
 // debe llamar directamente a la API externa. Para Blazor Server no es
 // necesario si las llamadas se realizan desde el servidor con HttpClient.
 const string nombrePoliticaCors = "PermitirTodo";
 constructor.Services.AddCors(opciones =>
 {
     opciones.AddPolicy(nombrePoliticaCors, politica =>
         politica
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());
 });
*/

var aplicacion = constructor.Build();

// ---------------------------------
// Configuración del pipeline HTTP
// ---------------------------------

if (!aplicacion.Environment.IsDevelopment())
{
    // Manejador de errores genérico en producción.
    aplicacion.UseExceptionHandler("/Error");

    // HSTS para mejorar seguridad en navegadores.
    aplicacion.UseHsts();
}

// Redirección a HTTPS.
aplicacion.UseHttpsRedirection();

// Servir archivos estáticos desde wwwroot.
aplicacion.UseStaticFiles();

// Enrutamiento de la aplicación.
aplicacion.UseRouting();

/*
// Activar CORS si se definió una política anteriormente.
aplicacion.UseCors(nombrePoliticaCors);
*/

// Endpoint del concentrador de Blazor Server (SignalR) y página host.
aplicacion.MapBlazorHub();
aplicacion.MapFallbackToPage("/_Host");

// Iniciar la aplicación.
aplicacion.Run();
