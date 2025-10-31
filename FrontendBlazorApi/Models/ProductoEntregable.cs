using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class ProductoEntregable
    {
        public int IdProducto { get; set; }
        public int IdEntregable { get; set; }

        public DateTime? FechaAsociacion { get; set; }
    }
    
    public class RespuestaApiProductoEntregable<T>
    {
        public T? Datos { get; set; }

    }
}