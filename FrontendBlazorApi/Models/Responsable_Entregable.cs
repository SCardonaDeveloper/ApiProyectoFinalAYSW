using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Responsable_Entregable
    {
        [JsonPropertyName("IdResponsable")]
        public int IdResponsable;
        [JsonPropertyName("IdEntregable")]
        public int IdEntregable;
        public DateTime FechaAsociacion;
        
        public Responsable_Entregable()
        {
            FechaAsociacion = DateTime.Today;
        }
    }
    
    public class Responsable_EntregableTratar
    {
        [JsonIgnore]
        [JsonPropertyName("IdResponsable")]
        public int IdResponsable;
        [JsonIgnore]
        [JsonPropertyName("IdEntregable")]
        public int IdEntregable;
        public DateOnly FechaAsociacion;
    }
}