namespace BackEndTorneo.Models.Auth
{
    public class LoginResponse
    {
        public int Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Usua_Email { get; set; }
        public int Rol_Id { get; set; }
        public string? Rol_Nombre { get; set; }
        public string? Token { get; set; }
    }
}