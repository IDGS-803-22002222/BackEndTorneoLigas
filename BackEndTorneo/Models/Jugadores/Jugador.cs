namespace BackEndTorneo.Models.Jugadores
{
    public class Jugador
    {
        public int Juga_Id { get; set; }
        public int Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public int Equi_Id { get; set; }
        public string? Equi_Nombre { get; set; }
        public int? Juga_Numero { get; set; }
        public string? Juga_Posicion { get; set; }
        public DateTime? Juga_FechaInscripcion { get; set; }
        public bool? Juga_Activo { get; set; }
    }
}