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
        public bool? Activo { get; set; }


    }

    public class UsuarioAutenticar 
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;
    }

    // DTO para usuario con roles
    public class UsuarioConRoles
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("roles")]
        public List<RolDto> Roles { get; set; } = new();
    }

    public class RolDto
    {
        [JsonPropertyName("idrol")]
        public int IdRol { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }

    // Clase genérica para mapear la respuesta de la API
    public class RespuestaApiUsuario<T>
    {
        public T? Datos { get; set; }

    }

}
