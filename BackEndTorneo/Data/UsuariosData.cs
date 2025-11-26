using BackEndTorneo.Models.Usuarios;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class UsuariosData
    {
        private readonly string conexion;

        public UsuariosData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Usuario>> ListarUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Usua_Id,
                        u.Usua_NombreCompleto,
                        u.Usua_Email,
                        u.Usua_Telefono,
                        u.Rol_Id,
                        r.Rol_Nombre,
                        u.Usua_FechaNacimiento,
                        u.Usua_Foto,
                        u.Usua_FechaRegistro,
                        u.Usua_UltimoAcceso,
                        u.Usua_Activo
                    FROM Usuarios u
                    INNER JOIN Roles r ON u.Rol_Id = r.Rol_Id
                    WHERE u.Usua_Activo = 1
                    ORDER BY u.Usua_FechaRegistro DESC", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearUsuario(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<Usuario?> ObtenerUsuario(int usuaId)
        {
            Usuario? usuario = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Usua_Id,
                        u.Usua_NombreCompleto,
                        u.Usua_Email,
                        u.Usua_Telefono,
                        u.Rol_Id,
                        r.Rol_Nombre,
                        u.Usua_FechaNacimiento,
                        u.Usua_Foto,
                        u.Usua_FechaRegistro,
                        u.Usua_UltimoAcceso,
                        u.Usua_Activo
                    FROM Usuarios u
                    INNER JOIN Roles r ON u.Rol_Id = r.Rol_Id
                    WHERE u.Usua_Id = @UsuaId", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        usuario = MapearUsuario(reader);
                    }
                }
            }
            return usuario;
        }

        public async Task<bool> EditarUsuario(EditarUsuario usuario)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Usuarios SET
                        Usua_NombreCompleto = @NombreCompleto,
                        Usua_Telefono = @Telefono,
                        Usua_FechaNacimiento = @FechaNacimiento,
                        Usua_Foto = @Foto
                    WHERE Usua_Id = @UsuaId", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuario.Usua_Id);
                cmd.Parameters.AddWithValue("@NombreCompleto", usuario.Usua_NombreCompleto ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", usuario.Usua_Telefono ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaNacimiento", usuario.Usua_FechaNacimiento ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Foto", usuario.Usua_Foto ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarUsuario(int usuaId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Usuarios SET Usua_Activo = 0 WHERE Usua_Id = @UsuaId", con);
                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<List<Usuario>> ObtenerUsuariosPorRol(int rolId)
        {
            List<Usuario> lista = new List<Usuario>();

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Usua_Id,
                        u.Usua_NombreCompleto,
                        u.Usua_Email,
                        u.Usua_Telefono,
                        u.Rol_Id,
                        r.Rol_Nombre,
                        u.Usua_FechaNacimiento,
                        u.Usua_Foto,
                        u.Usua_FechaRegistro,
                        u.Usua_UltimoAcceso,
                        u.Usua_Activo
                    FROM Usuarios u
                    INNER JOIN Roles r ON u.Rol_Id = r.Rol_Id
                    WHERE u.Rol_Id = @RolId AND u.Usua_Activo = 1
                    ORDER BY u.Usua_NombreCompleto", con);

                cmd.Parameters.AddWithValue("@RolId", rolId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearUsuario(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<List<Usuario>> ObtenerUsuariosPorRolTodos(int rolId)
        {
            List<Usuario> lista = new List<Usuario>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Usua_Id,
                        u.Usua_NombreCompleto,
                        u.Usua_Email,
                        u.Usua_Telefono,
                        u.Rol_Id,
                        r.Rol_Nombre,
                        u.Usua_FechaNacimiento,
                        u.Usua_Foto,
                        u.Usua_FechaRegistro,
                        u.Usua_UltimoAcceso,
                        u.Usua_Activo
                    FROM Usuarios u
                    INNER JOIN Roles r ON u.Rol_Id = r.Rol_Id
                    WHERE u.Rol_Id = @RolId 
                    ORDER BY u.Usua_NombreCompleto", con);

                cmd.Parameters.AddWithValue("@RolId", rolId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearUsuario(reader));
                    }
                }
            }
            return lista;
        }

        private Usuario MapearUsuario(SqlDataReader reader)
        {
            return new Usuario
            {
                Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                Usua_Email = reader["Usua_Email"].ToString(),
                Usua_Telefono = reader["Usua_Telefono"].ToString(),
                Rol_Id = Convert.ToInt32(reader["Rol_Id"]),
                Rol_Nombre = reader["Rol_Nombre"].ToString(),
                Usua_FechaNacimiento = reader["Usua_FechaNacimiento"] != DBNull.Value ? Convert.ToDateTime(reader["Usua_FechaNacimiento"]) : null,
                Usua_Foto = reader["Usua_Foto"].ToString(),
                Usua_FechaRegistro = reader["Usua_FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(reader["Usua_FechaRegistro"]) : null,
                Usua_UltimoAcceso = reader["Usua_UltimoAcceso"] != DBNull.Value ? Convert.ToDateTime(reader["Usua_UltimoAcceso"]) : null,
                Usua_Activo = reader["Usua_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Usua_Activo"]) : null
            };
        }
    }
}