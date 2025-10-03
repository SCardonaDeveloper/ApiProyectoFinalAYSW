
using System;                                             // Para DateTime y excepciones básicas
using Microsoft.AspNetCore.Authorization;                // Para [AllowAnonymous] y políticas de autorización
using Microsoft.AspNetCore.Mvc;                          // Para ControllerBase, IActionResult, atributos HTTP
using System.Threading.Tasks;                            // Para async/await
using System.Collections.Generic;                        // Para Dictionary y List
using System.Data;                                       // Para DataRow y DataColumn
using System.Linq;                                       // Para Cast y ToDictionary
using System.Text.Json;                                 // Para JsonElement
using Microsoft.Extensions.Logging;                      // Para ILogger y logging estructurado
using ApiBack.Servicios.Abstracciones;              // Para IServicioConsultas

namespace ApiBack.Controllers
{
    [Route("api/consultas")]                              // Ruta específica: /api/consultas
    [ApiController]                                       // Activa validación automática, binding, y comportamientos REST
    public class ConsultasController : ControllerBase
    {
        // Dependencias inyectadas - Aplicando DIP (Dependency Inversion Principle)
        private readonly IServicioConsultas _servicioConsultas;    // Para lógica de negocio y validaciones de consultas
        private readonly ILogger<ConsultasController> _logger;     // Para logging estructurado y auditoría
        public ConsultasController(
            IServicioConsultas servicioConsultas,        // Lógica de negocio para consultas SQL especializadas
            ILogger<ConsultasController> logger          // Logging estructurado, auditoría y monitoreo
        )
        {
            // Validaciones defensivas - Guard clauses para prevenir errores de configuración DI
            _servicioConsultas = servicioConsultas ?? throw new ArgumentNullException(
                nameof(servicioConsultas), 
                "IServicioConsultas no fue inyectado correctamente. Verificar registro de servicios en Program.cs"
            );
            
            _logger = logger ?? throw new ArgumentNullException(
                nameof(logger), 
                "ILogger no fue inyectado correctamente. Problema en configuración de logging de ASP.NET Core"
            );
        }
        [AllowAnonymous]                                  // Permite acceso sin autenticación (apropiado para desarrollo)
        [HttpPost("ejecutarconsultaparametrizada")]      // Responde a POST /api/consultas/ejecutarconsultaparametrizada
        public async Task<IActionResult> EjecutarConsultaParametrizadaAsync([FromBody] Dictionary<string, object?> cuerpoSolicitud)
        {
            const int maximoRegistros = 10000;            // Límite para generar advertencias en respuesta

            try
            {
                if (!cuerpoSolicitud.TryGetValue("consulta", out var consultaObj) || consultaObj is null)
                    return BadRequest("La consulta no puede estar vacía.");
                string consulta = consultaObj switch
                {
                    string texto => texto,                                                    // Cadena directa
                    JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString() ?? string.Empty, // JSON string
                    _ => string.Empty                                             // Cualquier otro tipo se considera vacío
                };

                if (string.IsNullOrWhiteSpace(consulta))
                    return BadRequest("La consulta no puede estar vacía.");
                Dictionary<string, object?>? parametros = null;
                
                if (cuerpoSolicitud.TryGetValue("parametros", out var parametrosObj) &&
                    parametrosObj is JsonElement jsonParametros &&
                    jsonParametros.ValueKind == JsonValueKind.Object)
                {

                    parametros = new Dictionary<string, object?>();
                    foreach (var p in jsonParametros.EnumerateObject())
                    {

                        parametros[p.Name] = p.Value;
                    }
                }
                _logger.LogInformation(
                    "INICIO ejecución consulta SQL - Consulta: {Consulta}, Parámetros: {CantidadParametros}",
                    consulta.Length > 100 ? consulta.Substring(0, 100) + "..." : consulta,  // Truncar consultas muy largas en logs
                    parametros?.Count ?? 0                                        // Cantidad de parámetros recibidos
                );
                var resultado = await _servicioConsultas.EjecutarConsultaParametrizadaDesdeJsonAsync(consulta, parametros);
                var lista = new List<Dictionary<string, object?>>();
                foreach (DataRow fila in resultado.Rows)
                {
                    // Crear diccionario para cada fila con conversión de tipos apropiada
                    var filaDiccionario = resultado.Columns.Cast<DataColumn>()
                        .ToDictionary(
                            col => col.ColumnName,                               // Clave: nombre de columna
                            col => fila[col] == DBNull.Value ? null : fila[col]  // Valor: convertir DBNull a null C#
                        );
                    lista.Add(filaDiccionario);
                }
                _logger.LogInformation(
                    "ÉXITO ejecución consulta SQL - Registros obtenidos: {Cantidad}", 
                    lista.Count     // Cantidad exacta de registros devueltos
                );
                if (lista.Count == 0)
                {
                    _logger.LogInformation("SIN DATOS - Consulta ejecutada correctamente pero no devolvió registros");
                    return NotFound("La consulta se ejecutó correctamente pero no devolvió resultados.");
                }
                return Ok(new
                {
                    Resultados = lista,                                          // Datos reales de la consulta
                    Total = lista.Count,                                         // Cantidad exacta de registros
                    Advertencia = lista.Count == maximoRegistros ? 
                        $"Se alcanzó el límite de {maximoRegistros} registros." : null  // Advertencia si se alcanzó límite
                });
            }
            
            catch (UnauthorizedAccessException excepcionAcceso)
            {
                
                _logger.LogWarning(
                    "ACCESO DENEGADO - Consulta rechazada por políticas de seguridad: {Mensaje}",
                    excepcionAcceso.Message    // Mensaje específico de la política violada
                );

                // Respuesta 403 Forbidden con información específica sobre la violación de seguridad
                return StatusCode(403, new
                {
                    estado = 403,                                      // Código HTTP explícito
                    mensaje = "Acceso denegado por políticas de seguridad.", // Mensaje general
                    detalle = excepcionAcceso.Message,                // Detalle específico de la violación
                    sugerencia = "Verifique que la consulta cumple con las políticas de seguridad configuradas"
                });
            }
            catch (ArgumentException excepcionArgumento)
            {
                _logger.LogWarning(
                    "PARÁMETROS INVÁLIDOS - Formato de entrada incorrecto: {Mensaje}",
                    excepcionArgumento.Message     // Detalle específico del problema de formato
                );

                // Respuesta 400 Bad Request con información para corregir la entrada
                return BadRequest(new
                {
                    estado = 400,                                      // Código HTTP explícito
                    mensaje = "Parámetros de entrada inválidos.",     // Mensaje general para el usuario
                    detalle = excepcionArgumento.Message,             // Detalle específico del problema
                    sugerencia = "Verifique el formato de la consulta y los nombres de parámetros"
                });
            }
            catch (Exception excepcionGeneral)
            {
                _logger.LogError(excepcionGeneral,
                    "ERROR CRÍTICO - Falla inesperada ejecutando consulta SQL"
                );
                return StatusCode(500, new
                {
                    estado = 500,                                        // Código HTTP explícito
                    mensaje = "Error interno del servidor.",             // Mensaje genérico y seguro
                    detalle = "Contacte al administrador del sistema.", // Instrucción para el usuario
                    timestamp = DateTime.UtcNow                          // Timestamp para correlación con logs
                });
            }
        }
    }
}
