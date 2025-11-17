using BackEndTorneo.Models.Sedes;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class SedesData
    {
        private readonly string conexion;

        public SedesData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Sede>> ListarSedes()
        {
            List<Sede> lista = new List<Sede>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        Sede_Id,
                        Sede_Nombre,
                        Sede_Direccion,
                        Sede_Capacidad,
                        Sede_TipoCampo,
                        Sede_Activo
                    FROM Sedes
                    WHERE Sede_Activo = 1
                    ORDER BY Sede_Nombre", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Sede
                        {
                            Sede_Id = Convert.ToInt32(reader["Sede_Id"]),
                            Sede_Nombre = reader["Sede_Nombre"].ToString(),
                            Sede_Direccion = reader["Sede_Direccion"].ToString(),
                            Sede_Capacidad = reader["Sede_Capacidad"] != DBNull.Value ? Convert.ToInt32(reader["Sede_Capacidad"]) : null,
                            Sede_TipoCampo = reader["Sede_TipoCampo"].ToString(),
                            Sede_Activo = reader["Sede_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Sede_Activo"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Sede?> ObtenerSede(int sedeId)
        {
            Sede? sede = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        Sede_Id,
                        Sede_Nombre,
                        Sede_Direccion,
                        Sede_Capacidad,
                        Sede_TipoCampo,
                        Sede_Activo
                    FROM Sedes
                    WHERE Sede_Id = @SedeId", con);

                cmd.Parameters.AddWithValue("@SedeId", sedeId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        sede = new Sede
                        {
                            Sede_Id = Convert.ToInt32(reader["Sede_Id"]),
                            Sede_Nombre = reader["Sede_Nombre"].ToString(),
                            Sede_Direccion = reader["Sede_Direccion"].ToString(),
                            Sede_Capacidad = reader["Sede_Capacidad"] != DBNull.Value ? Convert.ToInt32(reader["Sede_Capacidad"]) : null,
                            Sede_TipoCampo = reader["Sede_TipoCampo"].ToString(),
                            Sede_Activo = reader["Sede_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Sede_Activo"]) : null
                        };
                    }
                }
            }
            return sede;
        }

        public async Task<int> CrearSede(Sede sede)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Sedes (
                        Sede_Nombre, 
                        Sede_Direccion, 
                        Sede_Capacidad, 
                        Sede_TipoCampo,
                        Sede_Activo
                    )
                    VALUES (
                        @Nombre, 
                        @Direccion, 
                        @Capacidad, 
                        @TipoCampo,
                        1
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@Nombre", sede.Sede_Nombre);
                cmd.Parameters.AddWithValue("@Direccion", sede.Sede_Direccion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Capacidad", sede.Sede_Capacidad ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoCampo", sede.Sede_TipoCampo ?? (object)DBNull.Value);

                var sedeId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(sedeId);
            }
        }

        public async Task<bool> EditarSede(Sede sede)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Sedes SET
                        Sede_Nombre = @Nombre,
                        Sede_Direccion = @Direccion,
                        Sede_Capacidad = @Capacidad,
                        Sede_TipoCampo = @TipoCampo
                    WHERE Sede_Id = @SedeId", con);

                cmd.Parameters.AddWithValue("@SedeId", sede.Sede_Id);
                cmd.Parameters.AddWithValue("@Nombre", sede.Sede_Nombre);
                cmd.Parameters.AddWithValue("@Direccion", sede.Sede_Direccion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Capacidad", sede.Sede_Capacidad ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoCampo", sede.Sede_TipoCampo ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarSede(int sedeId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Sedes SET Sede_Activo = 0 WHERE Sede_Id = @SedeId", con);
                cmd.Parameters.AddWithValue("@SedeId", sedeId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}