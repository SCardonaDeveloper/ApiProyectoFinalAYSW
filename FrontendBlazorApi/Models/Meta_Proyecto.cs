using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendBlazorApi.Models
{
    // Representa la tabla Meta_Proyecto (relación N:M entre MetaEstrategica y Proyecto)
    public class MetaProyecto
    {
        [Required]
        public int IdMeta { get; set; }

        [Required]
        public int IdProyecto { get; set; }

        public DateTime? FechaAsociacion { get; set; }
    }
}
