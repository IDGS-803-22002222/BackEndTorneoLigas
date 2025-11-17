namespace BackEndTorneo.Models.QR
{
    public class QRCapitan
    {
        public int QRCa_Id { get; set; }
        public string? QRCa_Codigo { get; set; }
        public DateTime? QRCa_FechaGeneracion { get; set; }
        public DateTime? QRCa_FechaExpiracion { get; set; }
        public bool? QRCa_Usado { get; set; }
        public int? Usua_Id { get; set; }
    }
}