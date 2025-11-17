namespace BackEndTorneo.Models.Sedes
{
    public class Sede
    {
        public int Sede_Id { get; set; }
        public string? Sede_Nombre { get; set; }
        public string? Sede_Direccion { get; set; }
        public int? Sede_Capacidad { get; set; }
        public string? Sede_TipoCampo { get; set; }
        public bool? Sede_Activo { get; set; }
    }
}