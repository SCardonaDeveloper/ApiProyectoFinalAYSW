using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Proyecto_Producto
    {
        [JsonPropertyName("IdProyecto")]
        public int IdProyecto;
        [JsonPropertyName("IdProducto")]
        public int IdProducto;
        public DateTime FechaAsociacion;
        public Proyecto_Producto()
        {
            FechaAsociacion = DateTime.Today;
        }
    }
    
    public class Proyecto_ProductoTratar
    {
        [JsonPropertyName("IdProyecto")]
        public int IdProyecto;
        [JsonPropertyName("IdProducto")]
        public int IdProducto;
        public DateOnly FechaAsociacion;
    }
}