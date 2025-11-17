using BackEndTorneo.Models.Auth;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class AuthData
    {
        private readonly string conexion;

        public AuthData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<LoginResponse?> Login(string email)
        {
            LoginResponse? usuario = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Usua_Id, 
                        u.Usua_NombreCompleto, 
                        u.Usua_Email, 
                        u.Rol_Id,
                        r.Rol_Nombre,
                        u.Usua_Activo
                    FROM Usuarios u
                    INNER JOIN Roles r ON u.Rol_Id = r.Rol_Id
                    WHERE u.Usua_Email = @Email", con);

                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        usuario = new LoginResponse
                        {
                            Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Usua_Email = reader["Usua_Email"].ToString(),
                            Rol_Id = Convert.ToInt32(reader["Rol_Id"]),
                            Rol_Nombre = reader["Rol_Nombre"].ToString()
                        };
                    }
                }
            }
            return usuario;
        }

        public async Task<string?> ObtenerPasswordHash(string email)
        {
            string? passwordHash = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("SELECT Usua_Password FROM Usuarios WHERE Usua_Email = @Email", con);
                cmd.Parameters.AddWithValue("@Email", email);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    passwordHash = result.ToString();
                }
            }
            return passwordHash;
        }

        public async Task<bool> ActualizarUltimoAcceso(int usuaId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Usuarios SET Usua_UltimoAcceso = GETDATE() WHERE Usua_Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", usuaId);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> VerificarEmailExiste(string email)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Usuarios WHERE Usua_Email = @Email", con);
                cmd.Parameters.AddWithValue("@Email", email);

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<int> Registrar(Register registro, string passwordHash)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Usuarios (
                        Usua_NombreCompleto, 
                        Usua_Email, 
                        Usua_Telefono, 
                        Usua_Password, 
                        Rol_Id, 
                        Usua_FechaNacimiento,
                        Usua_FechaRegistro,
                        Usua_Activo
                    )
                    VALUES (
                        @NombreCompleto, 
                        @Email, 
                        @Telefono, 
                        @Password, 
                        2, 
                        @FechaNacimiento,
                        GETDATE(),
                        1
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@NombreCompleto", registro.Usua_NombreCompleto);
                cmd.Parameters.AddWithValue("@Email", registro.Usua_Email);
                cmd.Parameters.AddWithValue("@Telefono", registro.Usua_Telefono);
                cmd.Parameters.AddWithValue("@Password", passwordHash);
                cmd.Parameters.AddWithValue("@FechaNacimiento", registro.Usua_FechaNacimiento ?? (object)DBNull.Value);

                var usuaId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(usuaId);
            }
        }
    }
}