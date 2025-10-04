using System.Text.Json.Serialization;

namespace FrontendBlazorApi.Models
{
    public class VariableEstrategica
    {
        public int Id { get; set; }
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;

    }

    public class VariableEstrategicaTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;

    }

    // Clase genérica para mapear la respuesta de la API
    public class RespuestaApiVariable<T>
    {
        public T? Datos { get; set; }

    }

    
}
