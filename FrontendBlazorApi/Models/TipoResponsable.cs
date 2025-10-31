using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FrontendBlazorApi.Models
{
    public class TipoResponsable
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        public string? Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(250, ErrorMessage = "La descripción no puede tener más de 250 caracteres")]
        public string? Descripcion { get; set; } = string.Empty;
        
    }
    public class TipoResponsableTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        public string? Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(250, ErrorMessage = "La descripción no puede tener más de 250 caracteres")]
        public string? Descripcion { get; set; } = string.Empty;
    }
}

