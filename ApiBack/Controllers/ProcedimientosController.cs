// ProcedimientosController.cs — Controlador específico para ejecutar procedimientos almacenados
// Ubicación: Controllers/ProcedimientosController.cs
//
// Principios SOLID aplicados:
// - SRP: El controlador solo coordina peticiones HTTP para procedimientos almacenados
// - DIP: Depende de IServicioConsultas (abstracción), no de ServicioConsultas (implementación concreta)
// - ISP: Consume solo los métodos necesarios de IServicioConsultas
// - OCP: Preparado para agregar más endpoints sin modificar código existente

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ApiBack.Servicios.Abstracciones;

namespace ApiBack.Controllers
{
    [Route("api/procedimientos")]
    [ApiController]
    public class ProcedimientosController : ControllerBase
    {
        private readonly IServicioConsultas _servicioConsultas;
        private readonly ILogger<ProcedimientosController> _logger;        public ProcedimientosController(
            IServicioConsultas servicioConsultas,
            ILogger<ProcedimientosController> logger)
        {
            _servicioConsultas = servicioConsultas ?? throw new ArgumentNullException(nameof(servicioConsultas));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpPost("ejecutarsp")]
        public async Task<IActionResult> EjecutarProcedimientoAlmacenadoAsync(
            [FromBody] Dictionary<string, object?> parametrosSP,
            [FromQuery] string? camposEncriptar = null)
        {
            try
            {
                // FASE 1: VALIDACIÓN DE ENTRADA
                if (parametrosSP == null || !parametrosSP.TryGetValue("nombreSP", out var nombreSPObj) || nombreSPObj == null)
                {
                    return BadRequest("El parámetro 'nombreSP' es requerido.");
                }

                string nombreSP = nombreSPObj.ToString()!;
                
                // FASE 2: PROCESAMIENTO DE CAMPOS A ENCRIPTAR
                var camposAEncriptar = string.IsNullOrWhiteSpace(camposEncriptar)
                    ? new List<string>()
                    : camposEncriptar.Split(',').Select(c => c.Trim()).ToList();

                // FASE 3: PREPARAR PARÁMETROS (EXCLUIR nombreSP)
                var parametrosLimpios = new Dictionary<string, object?>();
                foreach (var kvp in parametrosSP)
                {
                    if (!kvp.Key.Equals("nombreSP", StringComparison.OrdinalIgnoreCase))
                    {
                        parametrosLimpios[kvp.Key] = kvp.Value;
                    }
                }

                // FASE 4: LOGGING DE AUDITORÍA
                _logger.LogInformation(
                    "INICIO ejecución SP - Procedimiento: {NombreSP}, Parámetros: {CantidadParametros}",
                    nombreSP,
                    parametrosLimpios.Count
                );

                // FASE 5: DELEGACIÓN AL SERVICIO
                var resultado = await _servicioConsultas.EjecutarProcedimientoAlmacenadoAsync(
                    nombreSP, 
                    parametrosLimpios, 
                    camposAEncriptar);

                // FASE 6: CONVERSIÓN DE DATATABLE A JSON-FRIENDLY
                var lista = new List<Dictionary<string, object?>>();
                foreach (DataRow fila in resultado.Rows)
                {
                    var filaDiccionario = resultado.Columns.Cast<DataColumn>()
                        .ToDictionary(
                            col => col.ColumnName,
                            col => fila[col] == DBNull.Value ? null : fila[col]
                        );
                    lista.Add(filaDiccionario);
                }

                // FASE 7: LOGGING DE RESULTADO
                _logger.LogInformation(
                    "ÉXITO ejecución SP - Procedimiento: {NombreSP}, Registros: {Cantidad}",
                    nombreSP,
                    lista.Count
                );

                // FASE 8: RESPUESTA EXITOSA
                return Ok(new
                {
                    Procedimiento = nombreSP,
                    Resultados = lista,
                    Total = lista.Count,
                    Mensaje = "Procedimiento ejecutado correctamente"
                });
            }
            catch (ArgumentException excepcionArgumento)
            {
                _logger.LogWarning(
                    "PARÁMETROS INVÁLIDOS - SP: {Mensaje}",
                    excepcionArgumento.Message
                );

                return BadRequest(new
                {
                    estado = 400,
                    mensaje = "Parámetros de entrada inválidos.",
                    detalle = excepcionArgumento.Message
                });
            }
            catch (Exception excepcionGeneral)
            {
                _logger.LogError(excepcionGeneral,
                    "ERROR CRÍTICO - Falla inesperada ejecutando SP"
                );

                return StatusCode(500, new
                {
                    estado = 500,
                    mensaje = "Error interno del servidor.",
                    detalle = "Contacte al administrador del sistema.",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}