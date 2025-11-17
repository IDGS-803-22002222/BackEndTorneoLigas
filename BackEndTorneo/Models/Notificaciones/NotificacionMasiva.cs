namespace BackEndTorneo.Models.Notificaciones
{
    public class NotificacionMasiva
    {
        public List<int>? Usua_Ids { get; set; }
        public string? Noti_Titulo { get; set; }
        public string? Noti_Mensaje { get; set; }
        public string? Noti_Tipo { get; set; }
    }
}