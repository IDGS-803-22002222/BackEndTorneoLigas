namespace BackEndTorneo.Models.QR
{
    public class QREquipo
    {
        public int InQR_Id { get; set; }
        public string? InQR_CodigoQR { get; set; }
        public int Equi_Id { get; set; }
        public string? Equi_Nombre { get; set; }
        public DateTime? InQR_FechaGeneracion { get; set; }
        public DateTime? InQR_FechaExpiracion { get; set; }
        public bool? InQR_Usado { get; set; }
    }
}