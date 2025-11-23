namespace BackEndTorneo.Models.CalendarioIA
{
    public class CalendarioGeneradoIA
    {
        public List<PartidoGeneradoIA> Partidos { get; set; } = new();
        public string Explicacion { get; set; } = null!;
    }

    public class PartidoGeneradoIA
    {
        public int Jornada { get; set; }
        public int EquipoLocalId { get; set; }
        public int EquipoVisitanteId { get; set; }
        public DateTime FechaHora { get; set; }
        public int SedeId { get; set; }
    }
}