using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Responsables
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        
        [JsonPropertyName("IdTipoResponsable")]
        public int IdTipoResponsable { get; set; }
        
        [JsonPropertyName("IdUsuario")]
        public int IdUsuario { get; set; }
        
        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
    
    public class ResponsablesTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        
        [JsonIgnore]
        public int IdTipoResponsable { get; set; }
        
        [JsonIgnore]
        public int IdUsuario { get; set; }
        
        public string Nombre { get; set; } = string.Empty;
    }
}