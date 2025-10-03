using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Contrasena { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}