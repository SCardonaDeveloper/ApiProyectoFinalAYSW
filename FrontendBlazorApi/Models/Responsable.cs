using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Responsables
    {
        [JsonPropertyName("Id)")]
        public int Id;
        [JsonPropertyName("IdTipoResponsable")]
        public int IdTipoResponsable;
        [JsonPropertyName("IdUsuario")]
        public int IdUsuario;
        public string Nombre;
    }
    
    public class ResponsablesTratar
    {
        [JsonIgnore]
        [JsonPropertyName("Id")]
        public int Id;
        [JsonIgnore]
        [JsonPropertyName("IdTipoResponsable")]
        public int IdTipoResponsable;
        [JsonIgnore]
        [JsonPropertyName("IdUsuario")]
        public int IdUsuario;
        public string Nombre;
    }
}