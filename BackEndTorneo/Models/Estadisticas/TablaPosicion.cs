namespace BackEndTorneo.Models.Estadisticas
{
    public class TablaPosicion
    {
        public int TaPo_Id { get; set; }
        public int Torn_Id { get; set; }
        public int Equi_Id { get; set; }
        public string? Equi_Nombre { get; set; }
        public string? Equi_Logo { get; set; }
        public int? TaPo_PartidosJugados { get; set; }
        public int? TaPo_PartidosGanados { get; set; }
        public int? TaPo_PartidosEmpatados { get; set; }
        public int? TaPo_PartidosPerdidos { get; set; }
        public int? TaPo_GolesFavor { get; set; }
        public int? TaPo_GolesContra { get; set; }
        public int? TaPo_DiferenciaGoles { get; set; }
        public int? TaPo_Puntos { get; set; }
    }
}