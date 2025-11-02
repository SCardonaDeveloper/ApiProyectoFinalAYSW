using System.ComponentModel.DataAnnotations;

namespace FrontendBlazorApi.Models
{
    public class MetaEstrategica
    {
        public int Id { get; set; }

        [Required]
        public int IdObjetivo { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }
    }
}
