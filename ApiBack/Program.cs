
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "tablasprohibidas.json",
    optional: true,
    reloadOnChange: true
);
builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("PermitirTodo", politica => politica
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{

    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
<<<<<<< HEAD
builder.Services.AddScoped<ApiBack.Servicios.Abstracciones.IServicioCrud,
                           ApiBack.Servicios.ServicioCrud>();
builder.Services.AddSingleton<ApiBack.Servicios.Abstracciones.IProveedorConexion,
                              ApiBack.Servicios.Conexion.ProveedorConexion>();
var proveedorBD = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
=======

// -----------------------------------------------------------------
// NOTA DIP: el registro de interfaces → implementaciones irá aquí.
// Ejemplos (se dejan comentados hasta el siguiente paso):
//
// builder.Services.AddScoped<IServicioCrud, ServicioCrud>();
// builder.Services.AddScoped<IRepositorioLecturaTabla, RepositorioLecturaSql>();
// builder.Services.AddSingleton<IValidadorIdentificadorSql, ValidadorIdentificadorSql>();
// builder.Services.AddSingleton<IPoliticaTablasProhibidas, PoliticaTablasProhibidas>();
// -----------------------------------------------------------------

// REGISTRO DE SERVICIO CRUD (DIP): interfaz → implementación (una instancia por request)
builder.Services.AddScoped<ApiBack.Servicios.Abstracciones.IServicioCrud,
                           ApiBack.Servicios.ServicioCrud>();

// REGISTRO DEL PROVEEDOR DE CONEXIÓN (DIP):
// Cuando se solicite IProveedorConexion, el contenedor entregará ProveedorConexion.
// NOTA: IProveedorConexion ahora está en Servicios.Abstracciones
builder.Services.AddSingleton<ApiBack.Servicios.Abstracciones.IProveedorConexion,
                              ApiBack.Servicios.Conexion.ProveedorConexion>();

// REGISTRO AUTOMÁTICO DEL REPOSITORIO SEGÚN DatabaseProvider (DIP + OCP)
// La API genérica lee la configuración y usa el proveedor correcto automáticamente
var proveedorBD = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";

// -----------------------------------------------------------------------------
// REGISTRO DE SERVICIO CONSULTAS (DIP)
// -----------------------------------------------------------------------------
// Este registro enlaza IServicioConsultas con la clase ServicioConsultas.
// Para que funcione correctamente, siempre debe estar registrado también
// un IRepositorioConsultas que cubra al motor de base de datos en uso.
//
// Si no existe un repositorio de consultas para el motor activo, conviene
// mover esta línea dentro del switch de Program.cs y dejarla solamente
// en los casos donde sí esté implementado el repositorio correspondiente.
// -----------------------------------------------------------------------------
>>>>>>> d6bc79f102c9e378d54a2da008111658f0d2b68e
builder.Services.AddScoped<ApiBack.Servicios.Abstracciones.IServicioConsultas,
    ApiBack.Servicios.ServicioConsultas>();


switch (proveedorBD.ToLower())
{
    case "postgres":
<<<<<<< HEAD
                builder.Services.AddScoped<ApiBack.Repositorios.Abstracciones.IRepositorioLecturaTabla,
                                           ApiBack.Repositorios.RepositorioLecturaPostgreSQL>();
=======
        // Usar PostgreSQL cuando DatabaseProvider = "Postgres"
                builder.Services.AddScoped<ApiBack.Repositorios.Abstracciones.IRepositorioLecturaTabla,
                                           ApiBack.Repositorios.RepositorioLecturaPostgreSQL>();
        // Repositorio de consultas para PostgreSQL (necesario porque IServicioConsultas se registra global)
>>>>>>> d6bc79f102c9e378d54a2da008111658f0d2b68e
        builder.Services.AddScoped<
            ApiBack.Repositorios.Abstracciones.IRepositorioConsultas,
            ApiBack.Repositorios.RepositorioConsultasPostgreSQL
        >();                                           
        break;
    case "mariadb":
    case "mysql":
        builder.Services.AddScoped<
            ApiBack.Repositorios.Abstracciones.IRepositorioLecturaTabla,
            ApiBack.Repositorios.RepositorioLecturaMysqlMariaDB>();
<<<<<<< HEAD
        builder.Services.AddScoped<
            ApiBack.Repositorios.Abstracciones.IRepositorioConsultas,
            ApiBack.Repositorios.RepositorioConsultasMysqlMariaDB>();
=======

        // Repositorio de consultas para MySQL/MariaDB
        builder.Services.AddScoped<
            ApiBack.Repositorios.Abstracciones.IRepositorioConsultas,
            ApiBack.Repositorios.RepositorioConsultasMysqlMariaDB>();

        // Nota: si IServicioConsultas está registrado de forma global (como en tu patrón),
        // aquí no se agrega nada más; el contenedor ya podrá construirlo porque existe
        // IRepositorioConsultas para este motor.
>>>>>>> d6bc79f102c9e378d54a2da008111658f0d2b68e
        break;

    case "sqlserver":
    case "sqlserverexpress":
    case "localdb":
    default:
<<<<<<< HEAD
=======
        // Usar SQL Server para todos los demás casos (incluyendo el valor por defecto)
>>>>>>> d6bc79f102c9e378d54a2da008111658f0d2b68e
        builder.Services.AddScoped<ApiBack.Repositorios.Abstracciones.IRepositorioLecturaTabla,
                                   ApiBack.Repositorios.RepositorioLecturaSqlServer>();

        builder.Services.AddScoped<ApiBack.Repositorios.Abstracciones.IRepositorioConsultas,
                               ApiBack.Repositorios.RepositorioConsultasSqlServer>();


        break;
}
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
<<<<<<< HEAD
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "webapicsharp v1");
=======
    // Indica dónde vive el documento OpenAPI.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiBack v1");

    // Define el prefijo de ruta. Con esto, la UI queda en /swagger.
>>>>>>> d6bc79f102c9e378d54a2da008111658f0d2b68e
    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.UseCors("PermitirTodo");
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();
