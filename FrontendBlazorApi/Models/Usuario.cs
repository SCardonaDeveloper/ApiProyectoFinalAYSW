using System.Text.Json.Serialization;
namespace FrontendBlazorApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string RutaAvatar { get; set; } = string.Empty;
        public bool Activo { get; set; }

    }

    public class UsuarioTratar
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string RutaAvatar { get; set; } = string.Empty;
        public bool Activo { get; set; }

    }

    // Clase genérica para mapear la respuesta de la API
    public class RespuestaApiUsuario<T>
    {
        public T? Datos { get; set; }

    }

    // prueba 
 
}
