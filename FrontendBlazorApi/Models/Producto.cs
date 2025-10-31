using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public int? IdTipoProducto { get; set; }
        public string? Codigo { get; set; } = string.Empty;
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinPrevista { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string? RutaLogo { get; set; } = string.Empty;
    }
    public class ProductoTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int? IdTipoProducto { get; set; }
        public string? Codigo { get; set; } = string.Empty;
        public string? Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinPrevista { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string? RutaLogo { get; set; } = string.Empty;
    }

    public class RespuestaApiProducto<T>
    {
        public T? Datos { get; set; }

    }
}