namespace BackEndTorneo.Models.Auth
{
    public class RegistroCapitanRequest
    {
        public string Token { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Telefono { get; set; }
    }
}
