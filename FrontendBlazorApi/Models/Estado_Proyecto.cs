using System.ComponentModel.DataAnnotations;

namespace FrontendBlazorApi.Models
{
    // Representa la tabla Estado_Proyecto (relación Proyecto ↔ Estado)
    public class EstadoProyecto
    {
        [Required]
        public int IdProyecto { get; set; }

        [Required]
        public int IdEstado { get; set; }
    }
}
