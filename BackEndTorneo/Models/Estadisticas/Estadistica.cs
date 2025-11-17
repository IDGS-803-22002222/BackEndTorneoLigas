namespace BackEndTorneo.Models.Estadisticas
{
    public class Estadistica
    {
        public int EsPa_Id { get; set; }
        public int Part_Id { get; set; }
        public int Juga_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Equi_Nombre { get; set; }
        public int? EsPa_Goles { get; set; }
        public int? EsPa_Asistencias { get; set; }
        public int? EsPa_TarjetasAmarillas { get; set; }
        public int? EsPa_TarjetasRojas { get; set; }
        public int? EsPa_MinutosJugados { get; set; }
    }
}