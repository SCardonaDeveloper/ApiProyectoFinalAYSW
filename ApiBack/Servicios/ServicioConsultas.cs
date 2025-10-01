// --------------------------------------------------------------
// Archivo: ServicioConsultas.cs (VERSIÓN COMPLETA)
// Ruta: Servicios/ServicioConsultas.cs
// --------------------------------------------------------------
//
// Implementación GENÉRICA de la lógica de negocio para consultas SQL parametrizadas
// y procedimientos almacenados.
// Usa Dictionary<string, object?> en lugar de parámetros específicos de motor.
//
// Arquitectura:
// JSON → Dictionary<string, object?> → IRepositorioConsultas → Motor específico
//
// Beneficios:
// - Independencia total del motor de BD
// - Reutilizable con PostgreSQL, SQL Server, MariaDB, etc.
// - Testing más simple
// - Mantiene compatibilidad con SqlParameter legacy
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using ApiBack.Servicios.Abstracciones;
using ApiBack.Repositorios.Abstracciones;

namespace ApiBack.Servicios
{
    public sealed class ServicioConsultas : IServicioConsultas
    {
        private readonly IRepositorioConsultas _repositorioConsultas;
        private readonly IConfiguration _configuration;

        public ServicioConsultas(IRepositorioConsultas repositorioConsultas, IConfiguration configuration)
        {
            _repositorioConsultas = repositorioConsultas ?? throw new ArgumentNullException(
                nameof(repositorioConsultas),
                "IRepositorioConsultas no puede ser null. Verificar registro en Program.cs.");

            _configuration = configuration ?? throw new ArgumentNullException(
                nameof(configuration),
                "IConfiguration no puede ser null. Problema en configuración de ASP.NET Core.");
        }

        // ================================================================
        // VALIDACIÓN DE CONSULTAS SQL (GENÉRICA)
        // ================================================================
        public (bool esValida, string? mensajeError) ValidarConsultaSQL(string consulta, string[] tablasProhibidas)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return (false, "La consulta no puede estar vacía.");

            string consultaNormalizada = consulta.Trim().ToUpperInvariant();

            if (!consultaNormalizada.StartsWith("SELECT") && !consultaNormalizada.StartsWith("WITH"))
                return (false, "Solo se permiten consultas SELECT y WITH por motivos de seguridad.");

            foreach (var tabla in tablasProhibidas)
            {
                if (consulta.Contains(tabla, StringComparison.OrdinalIgnoreCase))
                    return (false, $"La consulta intenta acceder a la tabla prohibida '{tabla}'.");
            }

            return (true, null);
        }

        // ================================================================
        // CONVERSIÓN DE PARÁMETROS JSON → Dictionary TIPADO
        // ================================================================
        private Dictionary<string, object?> ConvertirParametrosDesdeJson(Dictionary<string, object?>? parametros)
        {
            var parametrosGenericos = new Dictionary<string, object?>();

            if (parametros == null) return parametrosGenericos;

            foreach (var parametro in parametros)
            {
                string nombre = parametro.Key.StartsWith("@") ? parametro.Key : "@" + parametro.Key;

                if (!Regex.IsMatch(nombre, @"^@\w+$"))
                    throw new ArgumentException($"Nombre de parámetro inválido: {nombre}");

                object? valorTipado;

                if (parametro.Value == null)
                {
                    valorTipado = null;
                }
                else if (parametro.Value is JsonElement json)
                {
                    valorTipado = json.ValueKind switch
                    {
                        JsonValueKind.String => DetectarTipoDesdeString(json.GetString()),
                        JsonValueKind.Number => DetectarTipoNumerico(json),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        JsonValueKind.Array => json.GetRawText(),
                        JsonValueKind.Object => json.GetRawText(),
                        JsonValueKind.Undefined => null,
                        _ => json.GetRawText() ?? ""
                    };
                }
                else
                {
                    valorTipado = parametro.Value;
                }

                parametrosGenericos[nombre] = valorTipado;
            }

            return parametrosGenericos;
        }

        private object DetectarTipoDesdeString(string? valor)
        {
            if (string.IsNullOrEmpty(valor))
                return valor ?? "";

            if (DateTime.TryParse(valor, out DateTime fechaValor))
                return fechaValor;

            if (int.TryParse(valor, out int intValor))
                return intValor;

            if (long.TryParse(valor, out long longValor))
                return longValor;

            if (double.TryParse(valor, out double doubleValor))
                return doubleValor;

            if (bool.TryParse(valor, out bool boolValor))
                return boolValor;

            if (Guid.TryParse(valor, out Guid guidValor))
                return guidValor;

            return valor;
        }

        private object DetectarTipoNumerico(JsonElement element)
        {
            if (element.TryGetInt32(out int intVal))
                return intVal;
            if (element.TryGetInt64(out long longVal))
                return longVal;
            if (element.TryGetDouble(out double doubleVal))
                return doubleVal;

            return element.ToString() ?? "";
        }

        // ================================================================
        // EJECUCIÓN DE CONSULTAS PARAMETRIZADAS
        // ================================================================
        public async Task<DataTable> EjecutarConsultaParametrizadaAsync(
            string consulta,
            Dictionary<string, object?> parametros,
            int maximoRegistros,
            string? esquema)
        {
            var tablasProhibidas = _configuration.GetSection("TablasProhibidas").Get<string[]>() ?? Array.Empty<string>();
            var (esConsultaValida, mensajeError) = ValidarConsultaSQL(consulta, tablasProhibidas);

            if (!esConsultaValida)
                throw new UnauthorizedAccessException(mensajeError ?? "Consulta no autorizada.");

            return await _repositorioConsultas.EjecutarConsultaParametrizadaConDictionaryAsync(
                consulta, parametros, maximoRegistros, esquema);
        }

        public async Task<DataTable> EjecutarConsultaParametrizadaAsync(
            string consulta,
            List<Microsoft.Data.SqlClient.SqlParameter> parametros,
            int maximoRegistros,
            string? esquema)
        {
            var parametrosGenericos = new Dictionary<string, object?>();
            foreach (var param in parametros)
                parametrosGenericos[param.ParameterName] = param.Value;

            return await EjecutarConsultaParametrizadaAsync(consulta, parametrosGenericos, maximoRegistros, esquema);
        }

        public async Task<DataTable> EjecutarConsultaParametrizadaDesdeJsonAsync(
            string consulta,
            Dictionary<string, object?>? parametros)
        {
            var parametrosGenericos = ConvertirParametrosDesdeJson(parametros);
            return await EjecutarConsultaParametrizadaAsync(consulta, parametrosGenericos, 10000, null);
        }

        // ================================================================
        // EJECUCIÓN DE PROCEDIMIENTOS ALMACENADOS
        // ================================================================
        public async Task<DataTable> EjecutarProcedimientoAlmacenadoAsync(
            string nombreSP,
            Dictionary<string, object?>? parametros,
            List<string>? camposAEncriptar)
        {
            if (string.IsNullOrWhiteSpace(nombreSP))
                throw new ArgumentException("El nombre del procedimiento almacenado no puede estar vacío.", nameof(nombreSP));

            var parametrosGenericos = ConvertirParametrosConEncriptacion(parametros, camposAEncriptar);

            return await _repositorioConsultas.EjecutarProcedimientoAlmacenadoConDictionaryAsync(nombreSP, parametrosGenericos);
        }

        private Dictionary<string, object?> ConvertirParametrosConEncriptacion(
            Dictionary<string, object?>? parametros,
            List<string>? camposAEncriptar)
        {
            var parametrosGenericos = ConvertirParametrosDesdeJson(parametros);

            if (camposAEncriptar != null)
            {
                foreach (var campo in camposAEncriptar)
                {
                    var claveParametro = campo.StartsWith("@") ? campo : "@" + campo;

                    if (parametrosGenericos.ContainsKey(claveParametro) &&
                        parametrosGenericos[claveParametro] is string valorTexto &&
                        !string.IsNullOrWhiteSpace(valorTexto) &&
                        !valorTexto.StartsWith("$2"))
                    {
                        parametrosGenericos[claveParametro] = BCrypt.Net.BCrypt.HashPassword(valorTexto);
                    }
                }
            }

            return parametrosGenericos;
        }
    }
}
