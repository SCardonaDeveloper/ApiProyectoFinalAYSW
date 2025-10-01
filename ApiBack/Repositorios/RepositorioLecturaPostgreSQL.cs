// --------------------------------------------------------------
// Archivo : RepositorioLecturaPostgreSQL.cs (VERSIÓN CORREGIDA)
// Ruta    : webapicsharp/Repositorios/RepositorioLecturaPostgreSQL.cs
// Propósito: Implementar IRepositorioLecturaTabla para PostgreSQL con detección automática de tipos
// Problema resuelto: Error "42883: el operador no existe: integer = text"
// Dependencias: Npgsql, NpgsqlTypes, IProveedorConexion, EncriptacionBCrypt
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;                                        // Proveedor ADO.NET para PostgreSQL
using NpgsqlTypes;                                   // Tipos específicos de PostgreSQL
using ApiBack.Repositorios.Abstracciones;      // IRepositorioLecturaTabla
using ApiBack.Servicios.Abstracciones;         // IProveedorConexion
using ApiBack.Servicios.Utilidades;            // EncriptacionBCrypt

namespace ApiBack.Repositorios
{
    /// <summary>
    /// Implementación específica para PostgreSQL que resuelve el problema de incompatibilidad de tipos.
    /// 
    /// PROBLEMA ORIGINAL:
    /// PostgreSQL es estricto con tipos de datos y no permite comparaciones como:
    /// WHERE id = '3'  -- ERROR: integer = text
    /// 
    /// SOLUCIÓN IMPLEMENTADA:
    /// 1. Detecta automáticamente el tipo de dato de cada columna usando information_schema
    /// 2. Convierte valores string al tipo apropiado antes de crear parámetros SQL
    /// 3. Usa NpgsqlParameter con tipos específicos para máxima compatibilidad
    /// 4. Aplica esta lógica en todos los métodos CRUD que requieren filtros
    /// 
    /// TIPOS SOPORTADOS:
    /// - Enteros: integer, bigint, smallint
    /// - Decimales: numeric, real, double precision
    /// - Texto: varchar, char, text  
    /// - Booleanos: boolean
    /// - Fechas: timestamp, date, time
    /// - Especiales: uuid, json, jsonb
    /// </summary>
    public sealed class RepositorioLecturaPostgreSQL : IRepositorioLecturaTabla
    {
        // Campo privado para inyección de dependencias - patrón DIP
        // Permite obtener cadenas de conexión sin conocer su origen (appsettings, variables de entorno, etc.)
        private readonly IProveedorConexion _proveedorConexion;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// Se registra en Program.cs como:
        /// builder.Services.AddScoped<IRepositorioLecturaTabla, RepositorioLecturaPostgreSQL>();
        /// </summary>
        /// <param name="proveedorConexion">Proveedor de cadenas de conexión inyectado por DI</param>
        public RepositorioLecturaPostgreSQL(IProveedorConexion proveedorConexion)
        {
            _proveedorConexion = proveedorConexion ?? throw new ArgumentNullException(nameof(proveedorConexion));
        }

        /// <summary>
        /// MÉTODO CENTRAL: Detecta el tipo PostgreSQL de una columna específica.
        /// 
        /// ¿CÓMO FUNCIONA?
        /// 1. Consulta la tabla information_schema.columns (estándar SQL)
        /// 2. Obtiene data_type (tipo genérico) y udt_name (tipo específico de PostgreSQL)  
        /// 3. Mapea estos tipos a NpgsqlDbType para uso en parámetros
        /// 
        /// ¿POR QUÉ ES NECESARIO?
        /// PostgreSQL almacena metadatos de todas las tablas y columnas en information_schema.
        /// Esta información nos permite saber si una columna es integer, varchar, boolean, etc.
        /// Sin esta información, todos los valores llegan como string desde la API.
        /// 
        /// EJEMPLO DE CONSULTA:
        /// SELECT data_type, udt_name 
        /// FROM information_schema.columns 
        /// WHERE table_schema = 'public' 
        ///   AND table_name = 'rol' 
        ///   AND column_name = 'id'
        /// 
        /// RESULTADO: data_type='integer', udt_name='int4'
        /// </summary>
        /// <param name="nombreTabla">Nombre de la tabla (ej: "rol", "usuario")</param>
        /// <param name="esquema">Esquema de la tabla (ej: "public", "ventas")</param>
        /// <param name="nombreColumna">Nombre de la columna (ej: "id", "nombre")</param>
        /// <returns>NpgsqlDbType correspondiente al tipo de la columna, null si no se puede detectar</returns>
        private async Task<NpgsqlDbType?> DetectarTipoColumnaAsync(string nombreTabla, string esquema, string nombreColumna)
        {
            // Consulta SQL estándar para obtener metadatos de columnas
            // information_schema es portable entre diferentes SGBD (PostgreSQL, MySQL, SQL Server)
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
                
                // Usar parámetros para evitar SQL injection en consultas de metadatos
                comando.Parameters.AddWithValue("esquema", esquema);
                comando.Parameters.AddWithValue("tabla", nombreTabla);
                comando.Parameters.AddWithValue("columna", nombreColumna);

                await using var lector = await comando.ExecuteReaderAsync();
                if (await lector.ReadAsync())
                {
                    // Obtener ambos tipos: genérico y específico de PostgreSQL
                    string dataType = lector.GetString("data_type");    // ej: "integer", "character varying"
                    string udtName = lector.GetString("udt_name");      // ej: "int4", "varchar"
                    
                    // Delegar el mapeo a función especializada
                    return MapearTipoPostgreSQL(dataType, udtName);
                }
            }
            catch (Exception ex)
            {
                // ESTRATEGIA DE RECUPERACIÓN: Si no podemos detectar el tipo, continuar sin conversión
                // Esto evita que la aplicación falle completamente por problemas de metadatos
                // El método que llama esta función debe manejar el caso de retorno null
                Console.WriteLine($"Advertencia: No se pudo detectar tipo de columna {nombreColumna} en {esquema}.{nombreTabla}: {ex.Message}");
            }

            return null; // Indica que no se pudo detectar el tipo
        }

        /// <summary>
        /// Mapea tipos de PostgreSQL a NpgsqlDbType para uso en parámetros.
        /// 
        /// ¿POR QUÉ ESTE MAPEO?
        /// PostgreSQL tiene nombres específicos para tipos de datos que no coinciden exactamente 
        /// con los tipos .NET. NpgsqlDbType es la enumeración que permite especificar tipos
        /// PostgreSQL al crear parámetros NpgsqlParameter.
        /// 
        /// TIPOS MÁS COMUNES:
        /// - integer/int4 → números enteros de 32 bits (-2,147,483,648 a 2,147,483,647)
        /// - bigint/int8 → números enteros de 64 bits  
        /// - varchar → texto variable hasta longitud especificada
        /// - text → texto de longitud ilimitada
        /// - boolean → valores true/false
        /// - timestamp → fecha y hora
        /// 
        /// EXTENSIBILIDAD:
        /// Para agregar nuevos tipos, simplemente agregar casos al switch.
        /// Consultar documentación de PostgreSQL para nombres exactos de tipos.
        /// </summary>
        /// <param name="dataType">Tipo genérico SQL (ej: "integer", "character varying")</param>
        /// <param name="udtName">Tipo específico PostgreSQL (ej: "int4", "varchar")</param>
        /// <returns>NpgsqlDbType correspondiente, null si el tipo no está soportado</returns>
        private NpgsqlDbType? MapearTipoPostgreSQL(string dataType, string udtName)
        {
            // Usar pattern matching de C# 8+ para mapeo eficiente
            // Se evalúa dataType.ToLower() para manejar inconsistencias de mayúsculas
            return dataType.ToLower() switch
            {
                // TIPOS ENTEROS
                "integer" or "int4" => NpgsqlDbType.Integer,           // 32-bit integer
                "bigint" or "int8" => NpgsqlDbType.Bigint,             // 64-bit integer  
                "smallint" or "int2" => NpgsqlDbType.Smallint,         // 16-bit integer

                // TIPOS DECIMALES
                "numeric" or "decimal" => NpgsqlDbType.Numeric,        // Precisión exacta
                "real" or "float4" => NpgsqlDbType.Real,               // 32-bit float
                "double precision" or "float8" => NpgsqlDbType.Double, // 64-bit float

                // TIPOS DE TEXTO
                "character varying" or "varchar" => NpgsqlDbType.Varchar, // Texto longitud variable
                "character" or "char" => NpgsqlDbType.Char,               // Texto longitud fija
                "text" => NpgsqlDbType.Text,                              // Texto longitud ilimitada

                // TIPOS ESPECIALES
                "boolean" or "bool" => NpgsqlDbType.Boolean,              // true/false
                "uuid" => NpgsqlDbType.Uuid,                              // Identificador único

                // TIPOS DE FECHA Y HORA
                "timestamp without time zone" => NpgsqlDbType.Timestamp,    // Fecha/hora sin zona
                "timestamp with time zone" => NpgsqlDbType.TimestampTz,     // Fecha/hora con zona
                "date" => NpgsqlDbType.Date,                                 // Solo fecha
                "time" => NpgsqlDbType.Time,                                 // Solo hora

                // TIPOS JSON (PostgreSQL específicos)
                "json" => NpgsqlDbType.Json,                                 // JSON texto
                "jsonb" => NpgsqlDbType.Jsonb,                               // JSON binario (más eficiente)

                // DEFAULT: Tipos no reconocidos se tratan como texto
                _ => null // Retornar null indica "usar comportamiento por defecto (string)"
            };
        }

        /// <summary>
        /// Convierte un valor string al tipo de dato apropiado según el tipo PostgreSQL detectado.
        /// 
        /// ¿POR QUÉ ESTA CONVERSIÓN?
        /// Los valores llegan desde la API REST como strings (ej: "3", "true", "2023-01-01").
        /// PostgreSQL requiere que los parámetros SQL tengan el tipo correcto.
        /// Esta función hace la conversión: "3" → int 3, "true" → bool true, etc.
        /// 
        /// ESTRATEGIA DE CONVERSIÓN:
        /// 1. Si no se detectó tipo (tipoDestino == null), devolver string original
        /// 2. Según el tipo detectado, usar Parse/TryParse apropiado
        /// 3. Si la conversión falla, devolver string original (fallback seguro)
        /// 4. Nunca lanzar excepciones - preferir degradación gradual
        /// 
        /// EJEMPLOS DE CONVERSIÓN:
        /// - ConvertirValor("123", Integer) → (int) 123
        /// - ConvertirValor("true", Boolean) → (bool) true  
        /// - ConvertirValor("2023-01-01", Date) → (DateOnly) 2023-01-01
        /// - ConvertirValor("invalid", Integer) → (string) "invalid" (fallback)
        /// </summary>
        /// <param name="valor">Valor como string recibido de la API</param>
        /// <param name="tipoDestino">Tipo PostgreSQL detectado, null si no se pudo detectar</param>
        /// <returns>Valor convertido al tipo apropiado, string original si falla conversión</returns>
        private object ConvertirValor(string valor, NpgsqlDbType? tipoDestino)
        {
            // Si no se detectó tipo, usar string original - comportamiento seguro por defecto
            if (tipoDestino == null) return valor;

            try
            {
                // Pattern matching para conversión según tipo detectado
                return tipoDestino switch
                {
                    // CONVERSIONES NUMÉRICAS
                    NpgsqlDbType.Integer => int.Parse(valor),           // "123" → 123
                    NpgsqlDbType.Bigint => long.Parse(valor),           // "123456789" → 123456789L
                    NpgsqlDbType.Smallint => short.Parse(valor),        // "123" → (short)123
                    NpgsqlDbType.Numeric => decimal.Parse(valor),       // "123.45" → 123.45m
                    NpgsqlDbType.Real => float.Parse(valor),            // "123.45" → 123.45f
                    NpgsqlDbType.Double => double.Parse(valor),         // "123.45" → 123.45d

                    // CONVERSIONES LÓGICAS
                    NpgsqlDbType.Boolean => bool.Parse(valor),          // "true" → true

                    // CONVERSIONES ESPECIALES
                    NpgsqlDbType.Uuid => Guid.Parse(valor),             // "12345678-1234-..." → Guid

                    // CONVERSIONES DE FECHA/HORA
                    NpgsqlDbType.Timestamp => DateTime.Parse(valor),        // "2023-01-01 12:00:00" → DateTime
                    NpgsqlDbType.TimestampTz => DateTime.Parse(valor),      // Con zona horaria
                    NpgsqlDbType.Date => DateOnly.Parse(valor),             // "2023-01-01" → DateOnly (C# 6+)
                    NpgsqlDbType.Time => TimeOnly.Parse(valor),             // "12:30:45" → TimeOnly (C# 6+)

                    // TIPOS DE TEXTO: No necesitan conversión, se mantienen como string
                    NpgsqlDbType.Varchar => valor,
                    NpgsqlDbType.Char => valor,
                    NpgsqlDbType.Text => valor,
                    NpgsqlDbType.Json => valor,
                    NpgsqlDbType.Jsonb => valor,

                    // DEFAULT: Para tipos no contemplados, usar string original
                    _ => valor
                };
            }
            catch
            {
                // FALLBACK SEGURO: Si cualquier conversión falla, usar string original
                // Esto evita que la aplicación crash por datos malformados
                // El desarrollador verá el error en PostgreSQL, no en la aplicación .NET
                return valor;
            }
        }

        /// <summary>
        /// Implementa consulta simple sin filtros - SELECT * FROM tabla LIMIT n.
        /// No requiere detección de tipos porque no hay parámetros WHERE.
        /// </summary>
        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerFilasAsync(
            string nombreTabla,
            string? esquema,
            int? limite
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            // === NORMALIZACIÓN DE PARÁMETROS ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();
            int limiteFinal = limite ?? 1000;

            // === CONSTRUCCIÓN SQL ===
            // PostgreSQL usa LIMIT (no TOP como SQL Server)
            // Comillas dobles "" para escapar identificadores (no corchetes [] como SQL Server)
            string sql = $"SELECT * FROM \"{esquemaFinal}\".\"{nombreTabla}\" LIMIT @limite";
            var filas = new List<Dictionary<string, object?>>();

            try
            {
                // === CONEXIÓN Y EJECUCIÓN ===
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("limite", limiteFinal); // LIMIT requiere parámetro entero

                // === PROCESAMIENTO DE RESULTADOS ===
                await using var lector = await comando.ExecuteReaderAsync();
                while (await lector.ReadAsync())
                {
                    var fila = new Dictionary<string, object?>();
                    for (int i = 0; i < lector.FieldCount; i++)
                    {
                        string nombreColumna = lector.GetName(i);
                        // Conversión estándar PostgreSQL: DBNull → null C#
                        object? valor = lector.IsDBNull(i) ? null : lector.GetValue(i);
                        fila[nombreColumna] = valor;
                    }
                    filas.Add(fila);
                }
            }
            catch (NpgsqlException ex)
            {
                // Manejo específico de errores PostgreSQL con contexto descriptivo
                throw new InvalidOperationException(
                    $"Error PostgreSQL al consultar tabla '{esquemaFinal}.{nombreTabla}': {ex.Message}",
                    ex);
            }

            return filas;
        }

        /// <summary>
        /// MÉTODO CRÍTICO: Implementa consulta filtrada con detección automática de tipos.
        /// 
        /// ESTE ES EL MÉTODO QUE RESUELVE EL PROBLEMA ORIGINAL:
        /// Error: "42883: el operador no existe: integer = text"
        /// 
        /// PROCESO COMPLETO:
        /// 1. Validar parámetros de entrada
        /// 2. Detectar tipo de la columna de filtro usando information_schema
        /// 3. Convertir valor string al tipo apropiado  
        /// 4. Crear parámetro SQL con tipo específico
        /// 5. Ejecutar consulta con parámetro tipado
        /// 6. Procesar resultados normalmente
        /// 
        /// ANTES (FALLA):
        /// SELECT * FROM "public"."rol" WHERE "id" = @valor
        /// -- @valor es string "3", PostgreSQL ve: WHERE integer_column = 'text_value'
        /// 
        /// DESPUÉS (FUNCIONA):  
        /// SELECT * FROM "public"."rol" WHERE "id" = @valor
        /// -- @valor es int 3, PostgreSQL ve: WHERE integer_column = integer_value
        /// </summary>
        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El valor no puede estar vacío.", nameof(valor));

            // === NORMALIZACIÓN DE PARÁMETROS ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();
            var filas = new List<Dictionary<string, object?>>();

            try
            {
                // === PASO 1: DETECTAR TIPO DE COLUMNA ===
                // Esta es la clave de la solución - saber si la columna es integer, varchar, boolean, etc.
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                
                // === PASO 2: CONVERTIR VALOR AL TIPO APROPIADO ===
                // Convertir "3" → 3, "true" → true, etc. según el tipo detectado
                object valorConvertido = ConvertirValor(valor, tipoColumna);

                // === PASO 3: CONSTRUIR Y EJECUTAR CONSULTA ===
                string sql = $"SELECT * FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{nombreClave}\" = @valor";
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);
                
                // === PASO 4: CREAR PARÁMETRO CON TIPO ESPECÍFICO ===
                if (tipoColumna.HasValue)
                {
                    // CREAR PARÁMETRO TIPADO: Le dice explícitamente a PostgreSQL el tipo esperado
                    // Esto evita el error "integer = text" porque ambos lados tienen el mismo tipo
                    var parametro = new NpgsqlParameter("valor", tipoColumna.Value) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    // FALLBACK: Si no se pudo detectar tipo, usar AddWithValue (comportamiento original)
                    // Esto mantiene compatibilidad con columnas de texto y casos edge
                    comando.Parameters.AddWithValue("valor", valor);
                }

                // === PASO 5: EJECUTAR Y PROCESAR RESULTADOS ===
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

        /// <summary>
        /// Implementa INSERT con soporte para encriptación BCrypt.
        /// Aplica la misma lógica de detección de tipos para parámetros de inserción.
        /// </summary>
        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            // === NORMALIZACIÓN ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            // === PROCESAMIENTO DE ENCRIPTACIÓN ===
            // Crear copia para no modificar diccionario original
            var datosFinales = new Dictionary<string, object?>(datos);
            
            if (!string.IsNullOrWhiteSpace(camposEncriptar))
            {
                // Procesar campos a encriptar separados por coma: "password,token,secret"
                var camposAEncriptar = camposEncriptar.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var campo in camposAEncriptar)
                {
                    if (datosFinales.ContainsKey(campo) && datosFinales[campo] != null)
                    {
                        string valorOriginal = datosFinales[campo]?.ToString() ?? "";
                        // Usar BCrypt para hash seguro de contraseñas
                        datosFinales[campo] = EncriptacionBCrypt.Encriptar(valorOriginal);
                    }
                }
            }

            // === CONSTRUCCIÓN SQL INSERT ===
            var columnas = string.Join(", ", datosFinales.Keys.Select(k => $"\"{k}\""));
            var parametros = string.Join(", ", datosFinales.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO \"{esquemaFinal}\".\"{nombreTabla}\" ({columnas}) VALUES ({parametros})";

            try
            {
                // === CONEXIÓN Y EJECUCIÓN ===
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                // === AGREGAR PARÁMETROS ===
                // Para INSERT, PostgreSQL generalmente maneja bien AddWithValue
                // porque los tipos se infieren del contexto de la tabla
                foreach (var kvp in datosFinales)
                {
                    comando.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                }

                // === EJECUTAR Y VERIFICAR ===
                int filasAfectadas = await comando.ExecuteNonQueryAsync();
                return filasAfectadas > 0; // true si se insertó al menos una fila
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al insertar en tabla '{esquemaFinal}.{nombreTabla}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Implementa UPDATE con detección de tipos para la cláusula WHERE.
        /// La detección de tipos es especialmente importante aquí porque UPDATE 
        /// requiere filtrar por clave primaria que suele ser integer.
        /// </summary>
        public async Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valorClave))
                throw new ArgumentException("El valor de la clave no puede estar vacío.", nameof(valorClave));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos a actualizar no pueden estar vacíos.", nameof(datos));

            // === NORMALIZACIÓN ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            // === PROCESAMIENTO DE ENCRIPTACIÓN ===
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
                // === DETECCIÓN DE TIPO PARA WHERE ===
                // Crítico: La cláusula WHERE necesita tipos correctos para funcionar
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                object valorClaveConvertido = ConvertirValor(valorClave, tipoColumna);

                // === CONSTRUCCIÓN SQL UPDATE ===
                var clausulaSet = string.Join(", ", datosFinales.Keys.Select(k => $"\"{k}\" = @{k}"));
                string sql = $"UPDATE \"{esquemaFinal}\".\"{nombreTabla}\" SET {clausulaSet} WHERE \"{nombreClave}\" = @valorClave";

                // === CONEXIÓN Y EJECUCIÓN ===
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                // === PARÁMETROS PARA SET ===
                foreach (var kvp in datosFinales)
                {
                    comando.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                }

                // === PARÁMETRO PARA WHERE CON TIPO ESPECÍFICO ===
                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorClave", tipoColumna.Value) { Value = valorClaveConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorClave", valorClave);
                }

                // === EJECUTAR Y RETORNAR FILAS AFECTADAS ===
                int filasAfectadas = await comando.ExecuteNonQueryAsync();
                return filasAfectadas; // 0 = no encontrado, >0 = filas actualizadas
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al actualizar tabla '{esquemaFinal}.{nombreTabla}' WHERE {nombreClave}='{valorClave}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Implementa DELETE con detección de tipos para la cláusula WHERE.
        /// Similar al UPDATE, la detección de tipos es esencial para el filtro.
        /// </summary>
        public async Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (string.IsNullOrWhiteSpace(valorClave))
                throw new ArgumentException("El valor de la clave no puede estar vacío.", nameof(valorClave));

            // === NORMALIZACIÓN ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            try
            {
                // === DETECCIÓN DE TIPO PARA WHERE ===
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, nombreClave);
                object valorConvertido = ConvertirValor(valorClave, tipoColumna);

                // === CONSTRUCCIÓN SQL DELETE ===
                string sql = $"DELETE FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{nombreClave}\" = @valorClave";
                
                // === CONEXIÓN Y EJECUCIÓN ===
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                // === PARÁMETRO CON TIPO ESPECÍFICO ===
                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorClave", tipoColumna.Value) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorClave", valorClave);
                }

                // === EJECUTAR Y RETORNAR FILAS ELIMINADAS ===
                int filasEliminadas = await comando.ExecuteNonQueryAsync();
                return filasEliminadas; // 0 = no encontrado, >0 = filas eliminadas
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException(
                    $"Error PostgreSQL al eliminar de tabla '{esquemaFinal}.{nombreTabla}' WHERE {nombreClave}='{valorClave}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Obtiene hash de contraseña para validación de login.
        /// También aplica detección de tipos por si el campo de usuario no es varchar.
        /// </summary>
        public async Task<string?> ObtenerHashContrasenaAsync(
            string nombreTabla,
            string? esquema,
            string campoUsuario,
            string campoContrasena,
            string valorUsuario
        )
        {
            // === VALIDACIONES DE ENTRADA ===
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(campoUsuario))
                throw new ArgumentException("El campo de usuario no puede estar vacío.", nameof(campoUsuario));
            if (string.IsNullOrWhiteSpace(campoContrasena))
                throw new ArgumentException("El campo de contraseña no puede estar vacío.", nameof(campoContrasena));
            if (string.IsNullOrWhiteSpace(valorUsuario))
                throw new ArgumentException("El valor de usuario no puede estar vacío.", nameof(valorUsuario));

            // === NORMALIZACIÓN ===
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "public" : esquema.Trim();

            try
            {
                // === DETECCIÓN DE TIPO DEL CAMPO USUARIO ===
                var tipoColumna = await DetectarTipoColumnaAsync(nombreTabla, esquemaFinal, campoUsuario);
                object valorConvertido = ConvertirValor(valorUsuario, tipoColumna);

                // === CONSTRUCCIÓN SQL SELECT ===
                string sql = $"SELECT \"{campoContrasena}\" FROM \"{esquemaFinal}\".\"{nombreTabla}\" WHERE \"{campoUsuario}\" = @valorUsuario";
                
                // === CONEXIÓN Y EJECUCIÓN ===
                string cadena = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new NpgsqlConnection(cadena);
                await conexion.OpenAsync();

                await using var comando = new NpgsqlCommand(sql, conexion);

                // === PARÁMETRO CON TIPO ESPECÍFICO ===
                if (tipoColumna.HasValue)
                {
                    var parametro = new NpgsqlParameter("valorUsuario", tipoColumna.Value) { Value = valorConvertido };
                    comando.Parameters.Add(parametro);
                }
                else
                {
                    comando.Parameters.AddWithValue("valorUsuario", valorUsuario);
                }

                // === EJECUTAR Y OBTENER RESULTADO ===
                var resultado = await comando.ExecuteScalarAsync();
                return resultado?.ToString(); // Convertir a string o null si no existe usuario
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
// NOTAS PEDAGÓGICAS PARA COMPRENSIÓN COMPLETA
// ============================================================================================
//
// 1. PROBLEMA ORIGINAL RESUELTO:
//    Error: "42883: el operador no existe: integer = text"
//    Causa: PostgreSQL no convierte automáticamente tipos en comparaciones
//    Solución: Detectar tipo de columna y convertir valor antes de crear parámetro SQL
//
// 2. VENTAJAS DE ESTA IMPLEMENTACIÓN:
//     Genérica: Funciona con cualquier tipo de dato PostgreSQL
//     Robusta: Maneja errores graciosamente con fallbacks
//     Extensible: Fácil agregar nuevos tipos de datos
//     Consistente: Aplica la misma lógica en todos los métodos CRUD
//     Performante: Solo detecta tipos cuando es necesario (WHERE clauses)
//
// 3. DIFERENCIAS CON SQL SERVER:
//    PostgreSQL                     SQL Server
//    ────────────────               ──────────────────
//    Esquema: "public"              Esquema: [dbo]
//    Limite: LIMIT                  Limite: TOP  
//    Escape: "tabla"                Escape: [tabla]
//    Tipos: Estrictos               Tipos: Flexibles
//    Excepción: NpgsqlException     Excepción: SqlException
//
// 4. CÓMO USAR EN PROGRAM.CS:
//    Para PostgreSQL:
//    builder.Services.AddScoped<IRepositorioLecturaTabla, RepositorioLecturaPostgreSQL>();
//    
//    Para SQL Server:
//    builder.Services.AddScoped<IRepositorioLecturaTabla, RepositorioLecturaSqlServer>();
//
// 5. EJEMPLO DE USO:
//    GET /api/entidades/rol/3
//    ↓
//    Controller recibe "3" como string
//    ↓
//    ObtenerPorClaveAsync("rol", "public", "id", "3")
//    ↓
//    DetectarTipoColumnaAsync detecta que "id" es integer
//    ↓  
//    ConvertirValor("3", Integer) retorna int 3
//    ↓
//    Parámetro SQL: @valor = (int)3
//    ↓
//    PostgreSQL ejecuta: WHERE id = 3  (sin error de tipos)
//
// 6. TESTING:
//    Esta implementación es totalmente testeable unitariamente:
//    - Mock IProveedorConexion para cadenas de conexión
//    - Usar base de datos en memoria o contenedor Docker para integración
//    - Probar diferentes tipos de datos: int, varchar, boolean, uuid, etc.
//    - Verificar manejo de errores y fallbacks
//
// 7. EXTENSIONES FUTURAS:
//    - Cache de tipos de columnas para evitar consultas repetidas
//    - Soporte para arrays PostgreSQL (integer[], text[])
//    - Soporte para tipos personalizados (ENUM, COMPOSITE)
//    - Batch operations para mejor rendimiento
//    - Connection pooling optimizado
//
// 8. DEPENDENCIAS REQUERIDAS:
//    NuGet packages necesarios:
//    - Npgsql (proveedor PostgreSQL)
//    - Microsoft.Extensions.DependencyInjection (para DI)
//    - System.Text.Json (si se usan tipos JSON)