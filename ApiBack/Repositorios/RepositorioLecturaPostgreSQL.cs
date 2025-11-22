// --------------------------------------------------------------
// Archivo : RepositorioLecturaPostgreSQL.cs (VERSIÓN FINAL CON SOPORTE TIMESTAMP)
// Ruta    : webapicsharp/Repositorios/RepositorioLecturaPostgreSQL.cs
// Propósito: Implementar IRepositorioLecturaTabla para PostgreSQL con detección automática de tipos
//                    Búsquedas en TIMESTAMP con solo fecha
// Dependencias: Npgsql, NpgsqlTypes, IProveedorConexion, EncriptacionBCrypt
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using webapicsharp.Repositorios.Abstracciones;
using webapicsharp.Servicios.Abstracciones;
using webapicsharp.Servicios.Utilidades;

namespace webapicsharp.Repositorios
{
    /// <summary>
    /// Implementación específica para PostgreSQL que resuelve problemas de incompatibilidad de tipos.
    /// 
    /// PROBLEMAS RESUELTOS:
    /// 1. Error "42883: el operador no existe: integer = text"
    /// 2. Error "42804: column 'fecha' is of type date but expression is of type text"
    /// 3. Búsquedas en columnas TIMESTAMP usando solo fecha (sin hora)
    /// 
    /// MEJORA PARA API GENÉRICA:
    /// Detecta automáticamente cuando el usuario busca en una columna TIMESTAMP
    /// usando solo fecha (ej: "2025-02-01") y ajusta la consulta SQL para buscar
    /// todos los registros de ese día, ignorando la hora almacenada.
    /// </summary>
    public sealed class RepositorioLecturaPostgreSQL : IRepositorioLecturaTabla
    {
        private readonly IProveedorConexion _proveedorConexion;

        public RepositorioLecturaPostgreSQL(IProveedorConexion proveedorConexion)
        {
            _proveedorConexion = proveedorConexion ?? throw new ArgumentNullException(nameof(proveedorConexion));
        }

        private async Task<NpgsqlDbType?> DetectarTipoColumnaAsync(string nombreTabla, string esquema, string nombreColumna)
        {
            string sql = @"
                SELECT data_type, udt_name 
                FROM information_schema.columns 
                WHERE table_schema = @esquema 
                AND table_name = @tabla 
                AND column_name = @columna";

            try
            {
                string cadena = _proveedorConexion.ObtenerCadenaConexion();
                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("esquema", esquema);
                comando.Parameters.AddWithValue("tabla", nombreTabla);
                comando.Parameters.AddWithValue("columna", nombreColumna);

                await using var lector = await comando.ExecuteReaderAsync();
                if (await lector.ReadAsync())
                {
                    string dataType = lector.GetString(0);
                    string udtName = lector.GetString(1);
                    return MapearTipoPostgreSQL(dataType, udtName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Advertencia: No se pudo detectar tipo de columna {nombreColumna} en {esquema}.{nombreTabla}: {ex.Message}");
            }

            return null;
        }

        private NpgsqlDbType? MapearTipoPostgreSQL(string dataType, string udtName)
        {
            return dataType.ToLower() switch
            {
                "integer" or "int4" => NpgsqlDbType.Integer,
                "bigint" or "int8" => NpgsqlDbType.Bigint,
                "smallint" or "int2" => NpgsqlDbType.Smallint,
                "numeric" or "decimal" => NpgsqlDbType.Numeric,
                "real" or "float4" => NpgsqlDbType.Real,
                "double precision" or "float8" => NpgsqlDbType.Double,
                "character varying" or "varchar" => NpgsqlDbType.Varchar,
                "character" or "char" => NpgsqlDbType.Char,
                "text" => NpgsqlDbType.Text,
                "boolean" or "bool" => NpgsqlDbType.Boolean,
                "uuid" => NpgsqlDbType.Uuid,
                "timestamp without time zone" => NpgsqlDbType.Timestamp,
                "timestamp with time zone" => NpgsqlDbType.TimestampTz,
                "date" => NpgsqlDbType.Date,
                "time" => NpgsqlDbType.Time,
                "json" => NpgsqlDbType.Json,
                "jsonb" => NpgsqlDbType.Jsonb,
                _ => null
            };
        }

        private object ConvertirValor(string valor, NpgsqlDbType? tipoDestino)
        {
            if (tipoDestino == null) return valor;

            try
            {
                return tipoDestino switch
                {
                    NpgsqlDbType.Integer => int.Parse(valor),
                    NpgsqlDbType.Bigint => long.Parse(valor),
                    NpgsqlDbType.Smallint => short.Parse(valor),
                    NpgsqlDbType.Numeric => decimal.Parse(valor),
                    NpgsqlDbType.Real => float.Parse(valor),
                    NpgsqlDbType.Double => double.Parse(valor),
                    NpgsqlDbType.Boolean => bool.Parse(valor),
                    NpgsqlDbType.Uuid => Guid.Parse(valor),
                    NpgsqlDbType.Timestamp => DateTime.Parse(valor),
                    NpgsqlDbType.TimestampTz => DateTime.Parse(valor),
                    NpgsqlDbType.Date => ExtraerSoloFecha(valor),
                    NpgsqlDbType.Time => TimeOnly.Parse(valor),
                    NpgsqlDbType.Varchar => valor,
                    NpgsqlDbType.Char => valor,
                    NpgsqlDbType.Text => valor,
                    NpgsqlDbType.Json => valor,
                    NpgsqlDbType.Jsonb => valor,
                    _ => valor
                };
            }
            catch
            {
                return valor;
            }
        }

        private DateOnly ExtraerSoloFecha(string valor)
        {
            if (DateTime.TryParse(valor, out DateTime fechaCompleta))
                return DateOnly.FromDateTime(fechaCompleta);
            
            if (DateOnly.TryParse(valor, out DateOnly soloFecha))
                return soloFecha;
            
            throw new FormatException(
                $"No se pudo convertir '{valor}' a fecha. " +
                $"Formatos válidos: '2025-09-25', '2025-09-25T00:00:00'");
        }

        /// <summary>
        /// Detecta si un valor parece ser una fecha sin hora (formato YYYY-MM-DD).
        /// Usado para decidir si buscar en TIMESTAMP ignorando hora.
        /// </summary>
        private bool EsFechaSinHora(string valor)
        {
            return valor.Length == 10 && 
                   valor.Count(c => c == '-') == 2 &&
                   !valor.Contains("T") && 
                   !valor.Contains(":");
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerFilasAsync(
            string nombreTabla,
            string? esquema,
            int? limite
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();
            int limiteFinal = limite ?? 1000;

            string sql = $"SELECT * FROM \"{esquemaFinal}\".\"{nombreTabla}\" LIMIT @limite";
            var filas = new List<Dictionary<string, object?>>();

            try
            {
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("limite", limiteFinal);

                await using var lector = await comando.ExecuteReaderAsync();
                while (await lector.ReadAsync())
                {
                    var fila = new Dictionary<string, object?>();
                    for (int i = 0; i < lector.FieldCount; i++)
                    {
                        string nombreColumna = lector.GetName(i);
                        object? valor = lector.IsDBNull(i) ? null : lector.GetValue(i);
                        fila[nombreColumna] = valor;
                    }
                    filas.Add(fila);
                }
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al consultar tabla '{esquemaFinal}.{nombreTabla}': {ex.Message}",
                    ex);
            }

            return filas;
        }

        /// <summary>
        /// MÉTODO MEJORADO: Consulta filtrada con soporte inteligente para TIMESTAMP.
        /// 
        /// NUEVAS CAPACIDADES:
        /// - Detecta cuando se busca en columna TIMESTAMP con valor que parece solo fecha
        /// - Ajusta automáticamente la consulta SQL para buscar por fecha ignorando hora
        /// - Mantiene compatibilidad total con comportamiento anterior
        /// 
        /// EJEMPLOS DE USO:
        /// 
        /// Columna DATE:
        /// - Búsqueda: "2025-02-01" → WHERE fecha = '2025-02-01' 
        /// - Búsqueda: "2025-02-01T14:30:00" → WHERE fecha = '2025-02-01' 
        /// 
        /// Columna TIMESTAMP:
        /// - Búsqueda: "2025-02-01" → WHERE CAST(fecha AS DATE) = '2025-02-01'  (NUEVO)
        /// - Búsqueda: "2025-02-01T14:30:00" → WHERE fecha = '2025-02-01 14:30:00' 
        /// </summary>
        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El valor no puede estar vacío.", nameof(valor));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();
            var filas = new List<Dictionary<string, object?>>();

            try
            {
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                
                // DETECCIÓN INTELIGENTE: ¿Es búsqueda de fecha en columna TIMESTAMP?
                bool esBusquedaFechaSoloEnTimestamp = 
                    tipoColumna == NpgsqlDbType.Timestamp && 
                    EsFechaSinHora(valor);
                
                string sql;
                object valorConvertido;
                NpgsqlDbType tipoParametro;
                
                if (esBusquedaFechaSoloEnTimestamp)
                {
                    // CASO ESPECIAL: Buscar por fecha en TIMESTAMP ignorando hora
                    // SQL: WHERE CAST(columna_timestamp AS DATE) = '2025-02-01'
                    // Encuentra todos los registros del día sin importar la hora
                    sql = $"SELECT * FROM \"{esquemaFinal}\".\"{nombreTabla}\" " +
                          $"WHERE CAST(\"{nombreClave}\" AS DATE) = @valor";
                    
                    valorConvertido = ExtraerSoloFecha(valor);
                    tipoParametro = NpgsqlDbType.Date;
                }
                else
                {
                    // CASO NORMAL: Comparación exacta
                    sql = $"SELECT * FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{nombreClave}\" = @valor";
                    valorConvertido = ConvertirValor(valor, tipoColumna);
                    tipoParametro = tipoColumna ?? NpgsqlDbType.Text;
                }

                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);
                
                if (tipoColumna.HasValue || esBusquedaFechaSoloEnTimestamp)
                {
                    var parametro = new NpgsqlParameter("valor", tipoParametro) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valor", valor);
                }

                await using var lector = await comando.ExecuteReaderAsync();
                while (await lector.ReadAsync())
                {
                    var fila = new Dictionary<string, object?>();
                    for (int i = 0; i < lector.FieldCount; i++)
                    {
                        string nombreColumna = lector.GetName(i);
                        object? valorColumna = lector.IsDBNull(i) ? null : lector.GetValue(i);
                        fila[nombreColumna] = valorColumna;
                    }
                    filas.Add(fila);
                }
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al filtrar tabla '{esquemaFinal}.{nombreTabla}' por {nombreClave}='{valor}': {ex.Message}",
                    ex);
            }

            return filas;
        }

        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            
            if (!string.IsNullOrWhiteSpace(camposEncriptar))
            {
                var camposAEncriptar = camposEncriptar.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var campo in camposAEncriptar)
                {
                    if (datosFinales.ContainsKey(campo) && datosFinales[campo] != null)
                    {
                        string valorOriginal = datosFinales[campo]?.ToString() ?? "";
                        datosFinales[campo] = EncriptacionBCrypt.Encriptar(valorOriginal);
                    }
                }
            }

            var columnas = string.Join(", ", datosFinales.Keys.Select(k => $"\"{k}\""));
            var parametros = string.Join(", ", datosFinales.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO \"{esquemaFinal}\".\"{nombreTabla}\" ({columnas}) VALUES ({parametros})";

            try
            {
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                {
                    var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, kvp.Key);
                    
                    if (kvp.Value == null)
                    {
                        comando.Parameters.AddWithValue(kvp.Key, DBNull.Value);
                    }
                    else if (tipoColumna.HasValue && kvp.Value is string valorString)
                    {
                        object valorConvertido = ConvertirValor(valorString, tipoColumna);
                        var parametro = new NpgsqlParameter(kvp.Key, tipoColumna.Value) { Value = valorConvertido };
                        comando.Parameters.Add(parametro);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                int filasAfectadas = await comando.ExecuteNonQueryAsync();
                return filasAfectadas > 0;
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al insertar en tabla '{esquemaFinal}.{nombreTabla}': {ex.Message}",
                    ex);
            }
        }

        public async Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valorClave))
                throw new ArgumentException("El valor de la clave no puede estar vacío.", nameof(valorClave));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos a actualizar no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            if (!string.IsNullOrWhiteSpace(camposEncriptar))
            {
                var camposAEncriptar = camposEncriptar.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var campo in camposAEncriptar)
                {
                    if (datosFinales.ContainsKey(campo) && datosFinales[campo] != null)
                    {
                        string valorOriginal = datosFinales[campo]?.ToString() ?? "";
                        datosFinales[campo] = EncriptacionBCrypt.Encriptar(valorOriginal);
                    }
                }
            }

            try
            {
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                object valorClaveConvertido = ConvertirValor(valorClave, tipoColumna);

                var clausulaSet = string.Join(", ", datosFinales.Keys.Select(k => $"\"{k}\" = @{k}"));
                string sql = $"UPDATE \"{esquemaFinal}\".\"{nombreTabla}\" SET {clausulaSet} WHERE \"{nombreClave}\" = @valorClave";

                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                {
                    var tipoColumnaSet = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, kvp.Key);
                    
                    if (kvp.Value == null)
                    {
                        comando.Parameters.AddWithValue(kvp.Key, DBNull.Value);
                    }
                    else if (tipoColumnaSet.HasValue && kvp.Value is string valorString)
                    {
                        object valorConvertido = ConvertirValor(valorString, tipoColumnaSet);
                        var parametro = new NpgsqlParameter(kvp.Key, tipoColumnaSet.Value) { Value = valorConvertido };
                        comando.Parameters.Add(parametro);
                    }
                    else
                    {
                        comando.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorClave", tipoColumna.Value) { Value = valorClaveConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorClave", valorClave);
                }

                int filasAfectadas = await comando.ExecuteNonQueryAsync();
                return filasAfectadas;
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al actualizar tabla '{esquemaFinal}.{nombreTabla}' WHERE {nombreClave}='{valorClave}': {ex.Message}",
                    ex);
            }
        }

        public async Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valorClave))
                throw new ArgumentException("El valor de la clave no puede estar vacío.", nameof(valorClave));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            try
            {
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                object valorConvertido = ConvertirValor(valorClave, tipoColumna);

                string sql = $"DELETE FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{nombreClave}\" = @valorClave";
                
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorClave", tipoColumna.Value) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorClave", valorClave);
                }

                int filasEliminadas = await comando.ExecuteNonQueryAsync();
                return filasEliminadas;
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al eliminar de tabla '{esquemaFinal}.{nombreTabla}' WHERE {nombreClave}='{valorClave}': {ex.Message}",
                    ex);
            }
        }

        public async Task<string?> ObtenerHashContrasenaAsync(
            string nombreTabla,
            string? esquema,
            string campoUsuario,
            string campoContrasena,
            string valorUsuario
        )
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(campoUsuario))
                throw new ArgumentException("El campo de usuario no puede estar vacío.", nameof(campoUsuario));
            if (string.IsNullOrWhiteSpace(campoContrasena))
                throw new ArgumentException("El campo de contraseña no puede estar vacío.", nameof(campoContrasena));
            if (string.IsNullOrWhiteSpace(valorUsuario))
                throw new ArgumentException("El valor de usuario no puede estar vacío.", nameof(valorUsuario));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            try
            {
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, campoUsuario);
                object valorConvertido = ConvertirValor(valorUsuario, tipoColumna);

                string sql = $"SELECT \"{campoContrasena}\" FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{campoUsuario}\" = @valorUsuario";
                
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorUsuario", tipoColumna.Value) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorUsuario", valorUsuario);
                }

                var resultado = await comando.ExecuteScalarAsync();
                return resultado?.ToString();
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al obtener hash de contraseña de tabla '{esquemaFinal}.{nombreTabla}': {ex.Message}",
                    ex);
            }
        }
    }
}

// ============================================================================================
// RESUMEN DE MEJORAS EN ESTA VERSIÓN
// ============================================================================================
//

//

// En una API genérica, los usuarios esperan buscar por fecha en cualquier columna
// sin necesidad de conocer si es DATE o TIMESTAMP. Esto es especialmente útil en logs
//
// 1. El método EsFechaSinHora() que detecta valores formato YYYY-MM-DD
// 2. Modificación en ObtenerPorClaveAsync() que detecta este caso especial
// 3. Genera SQL con CAST para buscar por fecha ignorando hora:
//    WHERE CAST(columna_timestamp AS DATE) = '2025-02-01'
// 4. Encuentra todos los registros del día sin importar la hora
//
// COMPORTAMIENTO RESULTANTE:
//
// Columna DATE:
// - GET /api/proyecto/fecha_inicio/2025-02-01 →  Funciona (como antes)
// - GET /api/proyecto/fecha_inicio/2025-02-01T14:30:00 →  Funciona (extrae fecha)
//
// Columna TIMESTAMP :
// - GET /api/logs/timestamp/2025-02-01 →  Encuentra todos del día 
// - GET /api/logs/timestamp/2025-02-01T14:30:00 →  Búsqueda exacta (como antes)
//
// COMPATIBILIDAD:
// - No afecta ninguna funcionalidad existente
// - Todos los métodos CRUD mantienen su comportamiento actual
// - Solo agrega capacidad adicional para búsquedas en TIMESTAMP
// - API sigue siendo 100% genérica y funcional con cualquier tabla
//
// TESTING RECOMENDADO:
// 1. Crear tabla con columna TIMESTAMP
// 2. Insertar registros con diferentes horas
// 3. Buscar solo por fecha y verificar que encuentra todos
// 4. Buscar con hora específica y verificar que encuentra solo ese registro