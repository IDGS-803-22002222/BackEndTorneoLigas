namespace BackEndTorneo.Models.Partidos
{
    public class CrearPartido
    {
        public int Torn_Id { get; set; }
        public int Equi_Id_Local { get; set; }
        public int Equi_Id_Visitante { get; set; }
        public DateTime Part_FechaPartido { get; set; }
        public int? Sede_Id { get; set; }
        public int? Usua_Id { get; set; }
        public int? Part_Jornada { get; set; }
    }
}