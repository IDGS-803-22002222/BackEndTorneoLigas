namespace BackEndTorneo.Models.Estadisticas
{
    public class RankingGoleador
    {
        public string? Usua_NombreCompleto { get; set; }
        public string? Equi_Nombre { get; set; }
        public int TotalGoles { get; set; }
        public int PartidosJugados { get; set; }
    }
}