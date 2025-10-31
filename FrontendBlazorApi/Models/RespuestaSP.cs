using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class RespuestaSP
    {
        [JsonPropertyName("Procedimiento")]
        public string? Procedimiento { get; set; }

        [JsonPropertyName("Resultados")]
        public List<Dictionary<string, object>>? Resultados { get; set; }

        [JsonPropertyName("Total")]
        public int Total { get; set; }

        [JsonPropertyName("Mensaje")]
        public string? Mensaje { get; set; }
    }
}