namespace BackEndTorneo.Models.Partidos
{
    public class Partido
    {
        public int Part_Id { get; set; }
        public int Torn_Id { get; set; }
        public string? Torn_Nombre { get; set; }
        public int Equi_Id_Local { get; set; }
        public string? Equi_Nombre_Local { get; set; }
        public int Equi_Id_Visitante { get; set; }
        public string? Equi_Nombre_Visitante { get; set; }
        public DateTime Part_FechaPartido { get; set; }
        public int? Sede_Id { get; set; }
        public string? Sede_Nombre { get; set; }
        public int? Usua_Id { get; set; }
        public string? Arbitro_Nombre { get; set; }
        public int? Part_GolesLocal { get; set; }
        public int? Part_GolesVisitante { get; set; }
        public string? Part_Estado { get; set; }
        public int? Part_Jornada { get; set; }
        public DateTime? Part_FechaCreacion { get; set; }

        public string? Goleadores_Local { get; set; }
        public string? Tarjetas_Amarillas_Local { get; set; }
        public string? Tarjetas_Rojas_Local { get; set; }

        public string? Goleadores_Visitante { get; set; }
        public string? Tarjetas_Amarillas_Visitante { get; set; }
        public string? Tarjetas_Rojas_Visitante { get; set; }
    }
}
