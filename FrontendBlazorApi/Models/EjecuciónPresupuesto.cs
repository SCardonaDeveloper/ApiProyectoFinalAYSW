using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendBlazorApi.Models
{
    public class EjecucionPresupuesto
    {
        public int Id { get; set; }

        [Required]
        public int IdPresupuesto { get; set; }

        [Required]
        public int Anio { get; set; }

        [DataType(DataType.Currency)]
        public decimal? MontoPlaneado { get; set; }

        [DataType(DataType.Currency)]
        public decimal? MontoEjecutado { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }
    }
}
