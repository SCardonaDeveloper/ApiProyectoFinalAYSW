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
    }
    
    public class Proyecto_ProductoTratar
    {
        [JsonPropertyName("IdProyecto")]
        public int IdProyecto;
        [JsonPropertyName("IdProducto")]
        public int IdProducto;
        public DateTime FechaAsociacion;
    }
}