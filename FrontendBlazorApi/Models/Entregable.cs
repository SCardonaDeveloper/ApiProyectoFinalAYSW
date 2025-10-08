using System.Text.Json.Serialization;

namespace FrontendBlazorApi.Models
{
    public class Entregable
    {
        public int Id { get; set; }
        public string? Codigo { get; set; } = string.Empty;
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; } 
        public DateTime? FechaFinPrevista { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

    }

    public class entregableTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Codigo { get; set; } = string.Empty;
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; } 
        public DateTime? FechaFinPrevista { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

    }

    // Clase genérica para mapear la respuesta de la API
    public class RespuestaApiEntregable<T>
    {
        public T? Datos { get; set; }

    }

    
}
