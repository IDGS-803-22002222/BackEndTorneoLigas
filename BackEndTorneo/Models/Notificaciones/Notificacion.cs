namespace BackEndTorneo.Models.Notificaciones
{
    public class Notificacion
    {
        public int Noti_Id { get; set; }
        public int Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Noti_Titulo { get; set; }
        public string? Noti_Mensaje { get; set; }
        public string? Noti_Tipo { get; set; }
        public bool? Noti_Leida { get; set; }
        public DateTime? Noti_FechaCreacion { get; set; }
    }
}