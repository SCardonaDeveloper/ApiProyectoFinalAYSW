using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    
    public class Proyecto
    {
        [JsonPropertyName("id")]
        public int Id;
        [JsonPropertyName("IdProyectoPadre")]
        public int IdProyectoPadre;
        [JsonPropertyName("IdResponsable")]
        public int IdResponsable;
        [JsonPropertyName("IdTipoProyecto")]
        public int IdTipoProyecto;
        public string Codigo;
        public string Titulo;
        public string Descripcion;
        public DateTime FechaInicio;
        public DateTime FechaFinPrevista;
        public DateTime FechaModificacion;
        public DateTime FechaFinalizacion;
        public string RutaLogo;

        public Proyecto()
        {
            FechaInicio = DateTime.Today;
            FechaFinPrevista = DateTime.Today;
            FechaFinalizacion = DateTime.Today;
        }

    }
    public class ProyectoTratar
    {
        [JsonIgnore]
        public int Id;
        [JsonPropertyName("IdProyectoPadre")]
        [JsonIgnore]
        public int IdProyectoPadre;
        [JsonPropertyName("IdResponsable")]
        [JsonIgnore]
        public int IdResponsable;
        [JsonPropertyName("IdTipoProyecto")]
        [JsonIgnore]
        public int IdTipoProyecto;
        public string Codigo;
        public string Titulo;
        public string Descripcion;
        public DateTime FechaInicio;
        public DateTime FechaFinPrevista;
        public DateTime FechaModificacion;
        public DateTime FechaFinalizacion;
        public string RutaLogo;
    }
}