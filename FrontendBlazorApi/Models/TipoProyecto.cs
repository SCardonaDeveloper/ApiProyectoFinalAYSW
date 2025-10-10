using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class TipoProyecto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

    }
    public class TipoProyectoTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
    
}