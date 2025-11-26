namespace BackEndTorneo.Models.Auth
{
    public class RegistroArbitroRequest
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Licencia { get; set; } = null!;
    }
}
