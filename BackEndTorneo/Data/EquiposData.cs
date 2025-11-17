using BackEndTorneo.Models.Equipos;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class EquiposData
    {
        private readonly string conexion;

        public EquiposData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Equipo>> ListarEquipos()
        {
            List<Equipo> lista = new List<Equipo>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        e.Equi_Id,
                        e.Equi_Nombre,
                        e.Equi_Logo,
                        e.Equi_ColorUniforme,
                        e.Usua_Id,
                        u.Usua_NombreCompleto,
                        e.Equi_CodigoQR,
                        e.Equi_FechaCreacion,
                        e.Equi_Activo
                    FROM Equipos e
                    LEFT JOIN Usuarios u ON e.Usua_Id = u.Usua_Id
                    WHERE e.Equi_Activo = 1
                    ORDER BY e.Equi_Nombre", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Equipo
                        {
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Equi_Logo = reader["Equi_Logo"].ToString(),
                            Equi_ColorUniforme = reader["Equi_ColorUniforme"].ToString(),
                            Usua_Id = reader["Usua_Id"] != DBNull.Value ? Convert.ToInt32(reader["Usua_Id"]) : null,
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_CodigoQR = reader["Equi_CodigoQR"].ToString(),
                            Equi_FechaCreacion = reader["Equi_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Equi_FechaCreacion"]) : null,
                            Equi_Activo = reader["Equi_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Equi_Activo"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Equipo?> ObtenerEquipo(int equipoId)
        {
            Equipo? equipo = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        e.Equi_Id,
                        e.Equi_Nombre,
                        e.Equi_Logo,
                        e.Equi_ColorUniforme,
                        e.Usua_Id,
                        u.Usua_NombreCompleto,
                        e.Equi_CodigoQR,
                        e.Equi_FechaCreacion,
                        e.Equi_Activo
                    FROM Equipos e
                    LEFT JOIN Usuarios u ON e.Usua_Id = u.Usua_Id
                    WHERE e.Equi_Id = @EquipoId", con);

                cmd.Parameters.AddWithValue("@EquipoId", equipoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        equipo = new Equipo
                        {
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Equi_Logo = reader["Equi_Logo"].ToString(),
                            Equi_ColorUniforme = reader["Equi_ColorUniforme"].ToString(),
                            Usua_Id = reader["Usua_Id"] != DBNull.Value ? Convert.ToInt32(reader["Usua_Id"]) : null,
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_CodigoQR = reader["Equi_CodigoQR"].ToString(),
                            Equi_FechaCreacion = reader["Equi_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Equi_FechaCreacion"]) : null,
                            Equi_Activo = reader["Equi_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Equi_Activo"]) : null
                        };
                    }
                }
            }
            return equipo;
        }

        public async Task<int> CrearEquipo(CrearEquipo equipo)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Generar código QR único
                string codigoQR = "QR-EQUIPO-" + Guid.NewGuid().ToString();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Equipos (
                        Equi_Nombre, 
                        Equi_Logo, 
                        Equi_ColorUniforme, 
                        Usua_Id, 
                        Equi_CodigoQR,
                        Equi_FechaCreacion,
                        Equi_Activo
                    )
                    VALUES (
                        @Nombre, 
                        @Logo, 
                        @ColorUniforme, 
                        @UsuaId, 
                        @CodigoQR,
                        GETDATE(),
                        1
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@Nombre", equipo.Equi_Nombre);
                cmd.Parameters.AddWithValue("@Logo", equipo.Equi_Logo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ColorUniforme", equipo.Equi_ColorUniforme ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuaId", equipo.Usua_Id);
                cmd.Parameters.AddWithValue("@CodigoQR", codigoQR);

                var equipoId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(equipoId);
            }
        }

        public async Task<bool> EditarEquipo(Equipo equipo)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Equipos SET
                        Equi_Nombre = @Nombre,
                        Equi_Logo = @Logo,
                        Equi_ColorUniforme = @ColorUniforme,
                        Usua_Id = @UsuaId
                    WHERE Equi_Id = @EquipoId", con);

                cmd.Parameters.AddWithValue("@EquipoId", equipo.Equi_Id);
                cmd.Parameters.AddWithValue("@Nombre", equipo.Equi_Nombre);
                cmd.Parameters.AddWithValue("@Logo", equipo.Equi_Logo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ColorUniforme", equipo.Equi_ColorUniforme ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuaId", equipo.Usua_Id ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarEquipo(int equipoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Equipos SET Equi_Activo = 0 WHERE Equi_Id = @EquipoId", con);
                cmd.Parameters.AddWithValue("@EquipoId", equipoId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<string?> ObtenerCodigoQR(int equipoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("SELECT Equi_CodigoQR FROM Equipos WHERE Equi_Id = @EquipoId", con);
                cmd.Parameters.AddWithValue("@EquipoId", equipoId);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString();
            }
        }
    }
}