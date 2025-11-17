namespace BackEndTorneo.Models.Auth
{
    public class Register
    {
        public string? Usua_NombreCompleto { get; set; }
        public string? Usua_Email { get; set; }
        public string? Usua_Telefono { get; set; }
        public string? Usua_Password { get; set; }
        public DateTime? Usua_FechaNacimiento { get; set; }
    }
}