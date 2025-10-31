using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Actividad
    {
        public int Id;
        [JsonPropertyName("IdEntregable")]
        public int IdEntregable;
        public string Titulo;
        public string Descripcion;
        public DateTime FechaInicio;
        public DateTime FechaFinPrevista;
        public DateTime FechaModificacion;
        public DateTime FechaFinalizacion;
        public int Prioridad;
        public int PorcentajeAvance;
    }
    public class ActividadTratar
    {
        [JsonIgnore]
        public int Id;
        [JsonPropertyName("IdEntregable")]
        [JsonIgnore]
        public int IdEntregable;
        public string Titulo;
        public string Descripcion;
        public DateOnly FechaInicio;
        public DateOnly FechaFinPrevista;
        public DateOnly FechaModificacion;
        public DateOnly FechaFinalizacion;
        public string Prioridad;
        public double PorcentajeAvance;
    }
}