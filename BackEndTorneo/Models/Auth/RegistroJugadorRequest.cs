using BackEndTorneo.Models.Jugadores;

namespace BackEndTorneo.Models.Auth
{
    public class RegistroJugadorRequest
    {
        public string Token { get; set; } = null!;           // Código QR del equipo
        public int EquipoID { get; set; }                    // Id de equipo que envía la app (referencial)
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Telefono { get; set; }
        public int NumeroCamiseta { get; set; }
        public string Posicion { get; set; } = null!;
    }
}
