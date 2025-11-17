namespace BackEndTorneo.Models.Torneos
{
    public class Torneo
    {
        public int Torn_Id { get; set; }
        public string? Torn_Nombre { get; set; }
        public string? Torn_Descripcion { get; set; }
        public DateTime Torn_FechaInicio { get; set; }
        public DateTime? Torn_FechaFin { get; set; }
        public string? Torn_Tipo { get; set; }
        public int? Torn_NumeroEquipos { get; set; }
        public string? Torn_Estado { get; set; }
        public DateTime? Torn_FechaCreacion { get; set; }
        public bool? Torn_Activo { get; set; }
    }
}