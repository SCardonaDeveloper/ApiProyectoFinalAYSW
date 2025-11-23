// Program.cs
// Archivo de arranque principal de la aplicación Blazor Web App (plantilla moderna unificada).
// Aquí se configuran los servicios y se define cómo se ejecuta la aplicación.

using FrontendBlazorApi.Components;          // Importa el espacio de nombres donde está App.razor
using Microsoft.AspNetCore.Components;       // Librerías base de Blazor
using Microsoft.AspNetCore.Components.Web;   // Funcionalidades adicionales de renderizado
using Microsoft.AspNetCore.Authentication.Cookies; // Para autenticación con cookies
using Microsoft.AspNetCore.Components.Authorization; // Para AuthenticationStateProvider


var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Registro de servicios en el contenedor de dependencias
// -------------------------------

// Se registran los servicios de Razor Components.
// "AddInteractiveServerComponents" habilita el modo interactivo tipo Blazor Server (SignalR).
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar opciones del circuito de Blazor Server
builder.Services.AddServerSideBlazor(options =>
{
    // Desconectar el circuito después de 30 segundos de inactividad
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(30);
    // Intentos de reconexión del cliente
    options.DisconnectedCircuitMaxRetained = 100;
    // Tamaño máximo del buffer de JavaScript interop
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
});

// -----------------------------------------------------------------------------
// REGISTRO: Autenticación SIMPLIFICADA
// -----------------------------------------------------------------------------
// SOLO un servicio: ServicioAutenticacion
builder.Services.AddScoped<FrontendBlazorApi.Servicios.ServicioAutenticacion>();


builder.Services.AddHttpClient("ApiBack", cliente =>
 {
     // URL base de la API que expone /api/producto
     cliente.BaseAddress = new Uri("http://localhost:5031/");
     // Aquí se pueden agregar encabezados por defecto si se requiere.
 });

 // Registrar ServicioApiGenerico (CRUD)
builder.Services.AddScoped<FrontendBlazorApi.Servicios.ServicioApiGenerico>();


/*
 // Política CORS opcional. Útil solo si el navegador llamara
 // directamente a la API externa. Para Blazor Server no es necesario
 // si las llamadas se hacen con HttpClient en el servidor.
 const string nombrePoliticaCors = "PermitirTodo";
 builder.Services.AddCors(opciones =>
 {
     opciones.AddPolicy(nombrePoliticaCors, politica =>
         politica
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());
 });
*/

var app = builder.Build();

// -------------------------------
// Configuración del pipeline HTTP
// -------------------------------
if (!app.Environment.IsDevelopment())
{
    // En producción, se activa un manejador de errores genérico.
    // El parámetro "createScopeForErrors" mejora el aislamiento de errores.
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // HSTS: seguridad extra para navegadores (fuerza HTTPS durante 30 días por defecto).
    app.UseHsts();
}

// Redirección automática a HTTPS si el usuario entra por HTTP.
app.UseHttpsRedirection();

// Servir archivos estáticos desde wwwroot (CSS, JS, imágenes, etc.).
app.UseStaticFiles();

// -----------------------------------------------------------------------------
// MIDDLEWARE: Deshabilitar caché del navegador (IMPORTANTE PARA SEGURIDAD)
// -----------------------------------------------------------------------------
app.Use(async (context, next) =>
{
    // Agregar headers para evitar caché en TODAS las respuestas
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

// Middleware antifalsificación (protección contra CSRF).
app.UseAntiforgery();

/*
 // Activar CORS si se definió una política anteriormente.
 app.UseCors(nombrePoliticaCors);
*/

// -------------------------------
// Mapeo de componentes raíz
// -------------------------------
// Se indica que el componente principal de la aplicación es App.razor.
// Aquí arranca todo el enrutamiento y la estructura del sitio.
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// -------------------------------
// Inicio de la aplicación
// -------------------------------
app.Run();
