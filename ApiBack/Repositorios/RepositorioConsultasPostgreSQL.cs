using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ApiBack.Repositorios.Abstracciones;
using ApiBack.Servicios.Abstracciones;

namespace ApiBack.Repositorios
{
    /// <summary>
    /// Implementación de repositorio para ejecutar consultas y procedimientos almacenados en PostgreSQL.
    /// Encapsula la lógica de conexión y mapeo de parámetros.
    /// </summary>
    public sealed class RepositorioConsultasPostgreSQL : IRepositorioConsultas
    {
        private readonly IProveedorConexion _proveedorConexion;

        public RepositorioConsultasPostgreSQL(IProveedorConexion proveedorConexion)
        {
            _proveedorConexion = proveedorConexion ?? throw new ArgumentNullException(nameof(proveedorConexion));
        }

        // ================================================================
        // MÉTODO AUXILIAR: Mapea tipos de datos de PostgreSQL a NpgsqlDbType
        // ================================================================
        private NpgsqlDbType MapearTipo(string tipo)
        {
            // Traduce tipos de la base de datos a tipos que Npgsql entiende
            return tipo.ToLower() switch
            {
                "text"        => NpgsqlDbType.Text,
                "varchar"     => NpgsqlDbType.Varchar,
                "character varying" => NpgsqlDbType.Varchar,
                "integer"     => NpgsqlDbType.Integer,
                "int"         => NpgsqlDbType.Integer,
                "int4"        => NpgsqlDbType.Integer,
                "bigint"      => NpgsqlDbType.Bigint,
                "int8"        => NpgsqlDbType.Bigint,
                "smallint"    => NpgsqlDbType.Smallint,
                "int2"        => NpgsqlDbType.Smallint,
                "boolean"     => NpgsqlDbType.Boolean,
                "bool"        => NpgsqlDbType.Boolean,
                "json"        => NpgsqlDbType.Json,
                "jsonb"       => NpgsqlDbType.Jsonb,
                "timestamp"   => NpgsqlDbType.Timestamp,
                "timestamp without time zone" => NpgsqlDbType.Timestamp,
                "timestamptz" => NpgsqlDbType.TimestampTz,
                "date"        => NpgsqlDbType.Date,
                "numeric"     => NpgsqlDbType.Numeric,
                "decimal"     => NpgsqlDbType.Numeric,
                _             => NpgsqlDbType.Text // valor por defecto
            };
        }

        // ================================================================
        // MÉTODO AUXILIAR: Obtiene metadatos de parámetros de un SP en PostgreSQL
        // ================================================================
        private async Task<List<(string Nombre, string Modo, string Tipo)>> ObtenerMetadatosParametrosAsync(
            NpgsqlConnection conexion,
            string nombreSP)
        {
            var lista = new List<(string, string, string)>();

            string sql = @"
                SELECT parameter_name, parameter_mode, data_type
                FROM information_schema.parameters
                WHERE specific_name = (
                    SELECT specific_name
                    FROM information_schema.routines
                    WHERE routine_schema = 'public'
                      AND routine_name = @spName
                    LIMIT 1
                );";

            await using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@spName", nombreSP);

            await using var reader = await comando.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string nombre = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                string modo = reader.IsDBNull(1) ? "IN" : reader.GetString(1);   // valor por defecto: IN
                string tipo = reader.IsDBNull(2) ? "text" : reader.GetString(2);

                lista.Add((nombre, modo, tipo));
            }

            return lista;
        }

        // ================================================================
        // MÉTODO PRINCIPAL: Ejecuta un procedimiento almacenado genérico con parámetros dinámicos
        // ================================================================
        public async Task<DataTable> EjecutarProcedimientoAlmacenadoConDictionaryAsync(
            string nombreSP,
            Dictionary<string, object?> parametros)
        {
            if (string.IsNullOrWhiteSpace(nombreSP))
                throw new ArgumentException("El nombre del procedimiento no puede estar vacío.");

            string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
            await using var conexion = new NpgsqlConnection(cadenaConexion);
            await conexion.OpenAsync();

            // 1. Consulta los metadatos de los parámetros del SP
            var metadatos = await ObtenerMetadatosParametrosAsync(conexion, nombreSP);

            // 2. Normaliza las claves de parámetros (@ opcional, case-insensitive)
            var parametrosNormalizados = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in parametros ?? new Dictionary<string, object?>())
            {
                var clave = kv.Key.StartsWith("@") ? kv.Key.Substring(1) : kv.Key;
                parametrosNormalizados[clave] = kv.Value;
            }

            // 3. Construye el comando para ejecutar el SP
            await using var comando = new NpgsqlCommand(nombreSP, conexion);
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 300;

            // Agrega cada parámetro según IN, OUT o INOUT
            foreach (var meta in metadatos)
            {
                string clave = meta.Nombre;
                var npgsqlTipo = MapearTipo(meta.Tipo);

                if (meta.Modo == "IN")
                {
                    object valor = parametrosNormalizados.TryGetValue(clave, out var v) && v != null
                        ? v
                        : DBNull.Value;

                    comando.Parameters.Add(new NpgsqlParameter(clave, npgsqlTipo)
                    {
                        Direction = ParameterDirection.Input,
                        Value = valor
                    });
                }
                else if (meta.Modo == "OUT")
                {
                    comando.Parameters.Add(new NpgsqlParameter(clave, npgsqlTipo)
                    {
                        Direction = ParameterDirection.Output
                    });
                }
                else if (meta.Modo == "INOUT")
                {
                    object valor = parametrosNormalizados.TryGetValue(clave, out var v) && v != null
                        ? v
                        : DBNull.Value;

                    comando.Parameters.Add(new NpgsqlParameter(clave, npgsqlTipo)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = valor
                    });
                }
            }

            // 4. Ejecuta el SP y captura resultados
            var tabla = new DataTable();
            try
            {
                await using var reader = await comando.ExecuteReaderAsync();
                tabla.Load(reader);
            }
            catch
            {
                // Si no devuelve resultados, al menos se ejecuta
                await comando.ExecuteNonQueryAsync();
            }

            // 5. Agrega los valores de parámetros OUT/INOUT como filas del DataTable
            foreach (NpgsqlParameter param in comando.Parameters)
            {
                if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                {
                    if (!tabla.Columns.Contains(param.ParameterName))
                        tabla.Columns.Add(param.ParameterName);

                    var fila = tabla.NewRow();
                    fila[param.ParameterName] = param.Value == null ? DBNull.Value : param.Value;
                    tabla.Rows.Add(fila);
                }
            }

            return tabla;
        }

        // ================================================================
        // MÉTODO: Ejecuta una consulta SQL parametrizada
        // ================================================================
        public async Task<DataTable> EjecutarConsultaParametrizadaConDictionaryAsync(
            string consultaSQL, Dictionary<string, object?> parametros,
            int maximoRegistros = 10000, string? esquema = null)
        {
            var tabla = new DataTable();
            string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
            await using var conexion = new NpgsqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            await using var comando = new NpgsqlCommand(consultaSQL, conexion);

            foreach (var p in parametros ?? new Dictionary<string, object?>())
                comando.Parameters.AddWithValue(p.Key.StartsWith("@") ? p.Key : $"@{p.Key}", p.Value ?? DBNull.Value);

            await using var reader = await comando.ExecuteReaderAsync();
            tabla.Load(reader);
            return tabla;
        }

        // ================================================================
        // MÉTODO: Valida si una consulta SQL con parámetros es sintácticamente correcta
        // ================================================================
        public async Task<(bool esValida, string? mensajeError)> ValidarConsultaConDictionaryAsync(
            string consultaSQL, Dictionary<string, object?> parametros)
        {
            try
            {
                string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
                await using var conexion = new NpgsqlConnection(cadenaConexion);
                await conexion.OpenAsync();
                await using var comando = new NpgsqlCommand(consultaSQL, conexion);

                foreach (var p in parametros ?? new Dictionary<string, object?>())
                    comando.Parameters.AddWithValue(p.Key.StartsWith("@") ? p.Key : $"@{p.Key}", p.Value ?? DBNull.Value);

                await comando.PrepareAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ================================================================
        // MÉTODOS: Consultas de metadatos de base de datos/tablas
        // ================================================================
        public async Task<string?> ObtenerEsquemaTablaAsync(string nombreTabla, string esquemaPredeterminado)
        {
            string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
            await using var conexion = new NpgsqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            await using var comando = new NpgsqlCommand(
                "SELECT table_schema FROM information_schema.tables WHERE table_name = @tabla LIMIT 1", conexion);
            comando.Parameters.AddWithValue("@tabla", nombreTabla);
            var resultado = await comando.ExecuteScalarAsync();
            return resultado?.ToString();
        }

        public async Task<DataTable> ObtenerEstructuraTablaAsync(string nombreTabla, string esquema)
        {
            var tabla = new DataTable();
            string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
            await using var conexion = new NpgsqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            await using var comando = new NpgsqlCommand(
                "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = @tabla", conexion);
            comando.Parameters.AddWithValue("@tabla", nombreTabla);
            await using var reader = await comando.ExecuteReaderAsync();
            tabla.Load(reader);
            return tabla;
        }

        public async Task<DataTable> ObtenerEstructuraBaseDatosAsync(string? nombreBD)
        {
            var tabla = new DataTable();
            string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();
            await using var conexion = new NpgsqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            await using var comando = new NpgsqlCommand(
                "SELECT table_name, column_name FROM information_schema.columns WHERE table_schema = 'public'", conexion);
            await using var reader = await comando.ExecuteReaderAsync();
            tabla.Load(reader);
            return tabla;
        }
    }
}
