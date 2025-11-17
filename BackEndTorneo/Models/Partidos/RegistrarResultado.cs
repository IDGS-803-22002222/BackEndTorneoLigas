namespace BackEndTorneo.Models.Partidos
{
    public class RegistrarResultado
    {
        public int Part_Id { get; set; }
        public int Part_GolesLocal { get; set; }
        public int Part_GolesVisitante { get; set; }
        public string? Part_Estado { get; set; }
    }
}