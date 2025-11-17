using BackEndTorneo.Models.Torneos;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class TorneosData
    {
        private readonly string conexion;

        public TorneosData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Torneo>> ListarTorneos()
        {
            List<Torneo> lista = new List<Torneo>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        Torn_Id,
                        Torn_Nombre,
                        Torn_Descripcion,
                        Torn_FechaInicio,
                        Torn_FechaFin,
                        Torn_Tipo,
                        Torn_NumeroEquipos,
                        Torn_Estado,
                        Torn_FechaCreacion,
                        Torn_Activo
                    FROM Torneos
                    WHERE Torn_Activo = 1
                    ORDER BY Torn_FechaCreacion DESC", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Torneo
                        {
                            Torn_Id = Convert.ToInt32(reader["Torn_Id"]),
                            Torn_Nombre = reader["Torn_Nombre"].ToString(),
                            Torn_Descripcion = reader["Torn_Descripcion"].ToString(),
                            Torn_FechaInicio = Convert.ToDateTime(reader["Torn_FechaInicio"]),
                            Torn_FechaFin = reader["Torn_FechaFin"] != DBNull.Value ? Convert.ToDateTime(reader["Torn_FechaFin"]) : null,
                            Torn_Tipo = reader["Torn_Tipo"].ToString(),
                            Torn_NumeroEquipos = reader["Torn_NumeroEquipos"] != DBNull.Value ? Convert.ToInt32(reader["Torn_NumeroEquipos"]) : null,
                            Torn_Estado = reader["Torn_Estado"].ToString(),
                            Torn_FechaCreacion = reader["Torn_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Torn_FechaCreacion"]) : null,
                            Torn_Activo = reader["Torn_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Torn_Activo"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Torneo?> ObtenerTorneo(int torneoId)
        {
            Torneo? torneo = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        Torn_Id,
                        Torn_Nombre,
                        Torn_Descripcion,
                        Torn_FechaInicio,
                        Torn_FechaFin,
                        Torn_Tipo,
                        Torn_NumeroEquipos,
                        Torn_Estado,
                        Torn_FechaCreacion,
                        Torn_Activo
                    FROM Torneos
                    WHERE Torn_Id = @TorneoId", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        torneo = new Torneo
                        {
                            Torn_Id = Convert.ToInt32(reader["Torn_Id"]),
                            Torn_Nombre = reader["Torn_Nombre"].ToString(),
                            Torn_Descripcion = reader["Torn_Descripcion"].ToString(),
                            Torn_FechaInicio = Convert.ToDateTime(reader["Torn_FechaInicio"]),
                            Torn_FechaFin = reader["Torn_FechaFin"] != DBNull.Value ? Convert.ToDateTime(reader["Torn_FechaFin"]) : null,
                            Torn_Tipo = reader["Torn_Tipo"].ToString(),
                            Torn_NumeroEquipos = reader["Torn_NumeroEquipos"] != DBNull.Value ? Convert.ToInt32(reader["Torn_NumeroEquipos"]) : null,
                            Torn_Estado = reader["Torn_Estado"].ToString(),
                            Torn_FechaCreacion = reader["Torn_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Torn_FechaCreacion"]) : null,
                            Torn_Activo = reader["Torn_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Torn_Activo"]) : null
                        };
                    }
                }
            }
            return torneo;
        }

        public async Task<int> CrearTorneo(CrearTorneo torneo)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Torneos (
                        Torn_Nombre, 
                        Torn_Descripcion, 
                        Torn_FechaInicio, 
                        Torn_FechaFin, 
                        Torn_Tipo,
                        Torn_NumeroEquipos,
                        Torn_Estado,
                        Torn_FechaCreacion,
                        Torn_Activo
                    )
                    VALUES (
                        @Nombre, 
                        @Descripcion, 
                        @FechaInicio, 
                        @FechaFin, 
                        @Tipo,
                        @NumeroEquipos,
                        'Pendiente',
                        GETDATE(),
                        1
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@Nombre", torneo.Torn_Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", torneo.Torn_Descripcion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInicio", torneo.Torn_FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", torneo.Torn_FechaFin ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", torneo.Torn_Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroEquipos", torneo.Torn_NumeroEquipos ?? (object)DBNull.Value);

                var torneoId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(torneoId);
            }
        }

        public async Task<bool> EditarTorneo(Torneo torneo)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Torneos SET
                        Torn_Nombre = @Nombre,
                        Torn_Descripcion = @Descripcion,
                        Torn_FechaInicio = @FechaInicio,
                        Torn_FechaFin = @FechaFin,
                        Torn_Tipo = @Tipo,
                        Torn_NumeroEquipos = @NumeroEquipos,
                        Torn_Estado = @Estado
                    WHERE Torn_Id = @TorneoId", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneo.Torn_Id);
                cmd.Parameters.AddWithValue("@Nombre", torneo.Torn_Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", torneo.Torn_Descripcion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInicio", torneo.Torn_FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", torneo.Torn_FechaFin ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", torneo.Torn_Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroEquipos", torneo.Torn_NumeroEquipos ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", torneo.Torn_Estado ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarTorneo(int torneoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Torneos SET Torn_Activo = 0 WHERE Torn_Id = @TorneoId", con);
                cmd.Parameters.AddWithValue("@TorneoId", torneoId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> InscribirEquipo(InscribirEquipoTorneo inscripcion)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Verificar si ya está inscrito
                SqlCommand cmdVerificar = new SqlCommand(@"
                    SELECT COUNT(1) 
                    FROM Equipos_Torneos 
                    WHERE Equi_Id = @EquipoId AND Torn_Id = @TorneoId AND EqTo_Activo = 1", con);

                cmdVerificar.Parameters.AddWithValue("@EquipoId", inscripcion.Equi_Id);
                cmdVerificar.Parameters.AddWithValue("@TorneoId", inscripcion.Torn_Id);

                int existe = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());

                if (existe > 0)
                {
                    return false; // Ya está inscrito
                }

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Equipos_Torneos (
                        Equi_Id, 
                        Torn_Id, 
                        EqTo_FechaInscripcion,
                        EqTo_Activo
                    )
                    VALUES (
                        @EquipoId, 
                        @TorneoId, 
                        GETDATE(),
                        1
                    )", con);

                cmd.Parameters.AddWithValue("@EquipoId", inscripcion.Equi_Id);
                cmd.Parameters.AddWithValue("@TorneoId", inscripcion.Torn_Id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<List<dynamic>> ObtenerEquiposDelTorneo(int torneoId)
        {
            List<dynamic> lista = new List<dynamic>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        e.Equi_Id,
                        e.Equi_Nombre,
                        e.Equi_Logo,
                        et.EqTo_FechaInscripcion
                    FROM Equipos_Torneos et
                    INNER JOIN Equipos e ON et.Equi_Id = e.Equi_Id
                    WHERE et.Torn_Id = @TorneoId AND et.EqTo_Activo = 1
                    ORDER BY et.EqTo_FechaInscripcion", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Equi_Logo = reader["Equi_Logo"].ToString(),
                            EqTo_FechaInscripcion = Convert.ToDateTime(reader["EqTo_FechaInscripcion"])
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<bool> CambiarEstadoTorneo(int torneoId, string estado)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Torneos 
                    SET Torn_Estado = @Estado 
                    WHERE Torn_Id = @TorneoId", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneoId);
                cmd.Parameters.AddWithValue("@Estado", estado);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}