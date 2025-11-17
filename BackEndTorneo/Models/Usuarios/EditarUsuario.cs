namespace BackEndTorneo.Models.Usuarios
{
    public class EditarUsuario
    {
        public int Usua_Id { get; set; }
        public string? Usua_NombreCompleto { get; set; }
        public string? Usua_Telefono { get; set; }
        public DateTime? Usua_FechaNacimiento { get; set; }
        public string? Usua_Foto { get; set; }
    }
}