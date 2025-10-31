using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Archivo
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
    }
    public class ArchivoTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
    }
}