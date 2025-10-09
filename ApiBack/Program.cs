
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
builder.Services.AddScoped<ApiBack.Servicios.Abstracciones.IServicioCrud,
                           ApiBack.Servicios.ServicioCrud>();
builder.Services.AddSingleton<ApiBack.Servicios.Abstracciones.IProveedorConexion,
                              ApiBack.Servicios.Conexion.ProveedorConexion>();
var proveedorBD = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServerEXPRESS";

builder.Services.AddScoped<ApiBack.Servicios.Abstracciones.IServicioConsultas,
    ApiBack.Servicios.ServicioConsultas>();


switch (proveedorBD.ToLower())
{
    case "postgres":
                builder.Services.AddScoped<ApiBack.Repositorios.Abstracciones.IRepositorioLecturaTabla,
                                           ApiBack.Repositorios.RepositorioLecturaPostgreSQL>();

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
        builder.Services.AddScoped<
            ApiBack.Repositorios.Abstracciones.IRepositorioConsultas,
            ApiBack.Repositorios.RepositorioConsultasMysqlMariaDB>();
        break;

    case "sqlserver":
    case "sqlserverEXPRESS":
    case "localdb":
    default:
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "webapicsharp v1");
    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.UseCors("PermitirTodo");
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();
