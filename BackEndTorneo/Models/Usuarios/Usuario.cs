namespace BackEndTorneo.Models.Usuarios
{
    public class Usuario
    {
        public int Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Usua_Email { get; set; }
        public string? Usua_Telefono { get; set; }
        public int Rol_Id { get; set; }
        public string? Rol_Nombre { get; set; }
        public DateTime? Usua_FechaNacimiento { get; set; }
        public string? Usua_Foto { get; set; }
        public DateTime? Usua_FechaRegistro { get; set; }
        public DateTime? Usua_UltimoAcceso { get; set; }
        public bool? Usua_Activo { get; set; }
    }
}