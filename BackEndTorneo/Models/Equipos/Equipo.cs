namespace BackEndTorneo.Models.Equipos
{
    public class Equipo
    {
        public int Equi_Id { get; set; }
        public string? Equi_Nombre { get; set; }
        public string? Equi_Logo { get; set; }
        public string? Equi_ColorUniforme { get; set; }
        public int? Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Equi_CodigoQR { get; set; }
        public DateTime? Equi_FechaCreacion { get; set; }
        public bool? Equi_Activo { get; set; }
    }
}