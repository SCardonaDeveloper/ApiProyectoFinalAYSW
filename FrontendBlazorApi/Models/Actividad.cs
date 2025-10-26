using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Actividad
    {
        [JsonPropertyName("id")]
        private int Id;
        [JsonPropertyName("IdEntregable")]
        private int IdEntregable;
        private string Titulo;
        private string Descripcion;
        private DateOnly FechaInicio;
        private DateOnly FechaFinPrevista;
        private DateOnly FechaModificacion;
        private string Prioridad;
        private double PorcentajeAvance;
    }
    public class ActividadTratar
    {
        [JsonIgnore]
        private int Id;
        [JsonPropertyName("IdEntregable")]
        private int IdEntregable;
        private string Titulo;
        private string Descripcion;
        private DateOnly FechaInicio;
        private DateOnly FechaFinPrevista;
        private DateOnly FechaModificacion;
        private string Prioridad;
        private double PorcentajeAvance;
    }
}