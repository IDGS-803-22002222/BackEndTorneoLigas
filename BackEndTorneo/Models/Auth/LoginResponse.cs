using System.Text.Json.Serialization;

namespace BackEndTorneo.Models.Auth
{
    public class LoginResponse
    {
        [JsonPropertyName("usua_Id")]
        public int Usua_Id { get; set; }

        [JsonPropertyName("usua_NombreCompleto")]
        public string? Usua_NombreCompleto { get; set; }

        [JsonPropertyName("usua_Email")]
        public string? Usua_Email { get; set; }

        [JsonPropertyName("rol_Id")]
        public int Rol_Id { get; set; }

        [JsonPropertyName("rol_Nombre")]
        public string? Rol_Nombre { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}
