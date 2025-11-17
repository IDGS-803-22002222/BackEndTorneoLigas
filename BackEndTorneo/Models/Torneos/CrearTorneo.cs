namespace BackEndTorneo.Models.Torneos
{
    public class CrearTorneo
    {
        public string? Torn_Nombre { get; set; }
        public string? Torn_Descripcion { get; set; }
        public DateTime Torn_FechaInicio { get; set; }
        public DateTime? Torn_FechaFin { get; set; }
        public string? Torn_Tipo { get; set; }
        public int? Torn_NumeroEquipos { get; set; }
    }
}