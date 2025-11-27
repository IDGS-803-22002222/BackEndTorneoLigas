using BackEndTorneo.Models.Jugadores;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class JugadoresData
    {
        private readonly string conexion;

        public JugadoresData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Jugador>> ListarJugadores()
        {
            List<Jugador> lista = new List<Jugador>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        j.Juga_Id,
                        j.Usua_Id,
                        u.Usua_NombreCompleto,
                        j.Equi_Id,
                        e.Equi_Nombre,
                        j.Juga_Numero,
                        j.Juga_Posicion,
                        j.Juga_FechaInscripcion,
                        j.Juga_Activo
                    FROM Jugadores j
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id
                    WHERE j.Juga_Activo = 1
                    ORDER BY e.Equi_Nombre, j.Juga_Numero", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Jugador
                        {
                            Juga_Id = Convert.ToInt32(reader["Juga_Id"]),
                            Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Juga_Numero = reader["Juga_Numero"] != DBNull.Value ? Convert.ToInt32(reader["Juga_Numero"]) : null,
                            Juga_Posicion = reader["Juga_Posicion"].ToString(),
                            Juga_FechaInscripcion = reader["Juga_FechaInscripcion"] != DBNull.Value ? Convert.ToDateTime(reader["Juga_FechaInscripcion"]) : null,
                            Juga_Activo = reader["Juga_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Juga_Activo"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<List<Jugador>> ListarJugadoresPorEquipo(int equipoId)
        {
            List<Jugador> lista = new List<Jugador>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        j.Juga_Id,
                        j.Usua_Id,
                        u.Usua_NombreCompleto,
                        j.Equi_Id,
                        e.Equi_Nombre,
                        j.Juga_Numero,
                        j.Juga_Posicion,
                        j.Juga_FechaInscripcion,
                        j.Juga_Activo
                    FROM Jugadores j
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id
                    WHERE j.Equi_Id = @EquipoId AND j.Juga_Activo = 1
                    ORDER BY j.Juga_Numero", con);

                cmd.Parameters.AddWithValue("@EquipoId", equipoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Jugador
                        {
                            Juga_Id = Convert.ToInt32(reader["Juga_Id"]),
                            Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Juga_Numero = reader["Juga_Numero"] != DBNull.Value ? Convert.ToInt32(reader["Juga_Numero"]) : null,
                            Juga_Posicion = reader["Juga_Posicion"].ToString(),
                            Juga_FechaInscripcion = reader["Juga_FechaInscripcion"] != DBNull.Value ? Convert.ToDateTime(reader["Juga_FechaInscripcion"]) : null,
                            Juga_Activo = reader["Juga_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Juga_Activo"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Jugador?> ObtenerJugadorPorUsuario(int usuaId)
        {
            Jugador? jugador = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        j.Juga_Id,
                        j.Usua_Id,
                        u.Usua_NombreCompleto,
                        j.Equi_Id,
                        e.Equi_Nombre,
                        j.Juga_Numero,
                        j.Juga_Posicion,
                        j.Juga_FechaInscripcion,
                        j.Juga_Activo
                    FROM Jugadores j
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id
                    WHERE j.Usua_Id = @UsuaId AND j.Juga_Activo = 1", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        jugador = new Jugador
                        {
                            Juga_Id = Convert.ToInt32(reader["Juga_Id"]),
                            Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Juga_Numero = reader["Juga_Numero"] != DBNull.Value ? Convert.ToInt32(reader["Juga_Numero"]) : null,
                            Juga_Posicion = reader["Juga_Posicion"].ToString(),
                            Juga_FechaInscripcion = reader["Juga_FechaInscripcion"] != DBNull.Value ? Convert.ToDateTime(reader["Juga_FechaInscripcion"]) : null,
                            Juga_Activo = reader["Juga_Activo"] != DBNull.Value ? Convert.ToBoolean(reader["Juga_Activo"]) : null
                        };
                    }
                }
            }
            return jugador;
        }

        public async Task<bool> ActualizarJugadorPorUsuario(int usuaId, int numero, string posicion)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Jugadores SET
                        Juga_Numero = @Numero,
                        Juga_Posicion = @Posicion
                    WHERE Usua_Id = @UsuaId AND Juga_Activo = 1", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);
                cmd.Parameters.AddWithValue("@Numero", numero);
                cmd.Parameters.AddWithValue("@Posicion", posicion);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<int?> ValidarYObtenerEquipoDeQR(string codigoQR)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT Equi_Id 
                    FROM Equipos 
                    WHERE Equi_CodigoQR = @CodigoQR AND Equi_Activo = 1", con);

                cmd.Parameters.AddWithValue("@CodigoQR", codigoQR);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : null;
            }
        }

        public async Task<bool> InscribirJugador(InscribirJugador inscripcion, int equipoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Jugadores (
                        Usua_Id, 
                        Equi_Id, 
                        Juga_Numero, 
                        Juga_Posicion,
                        Juga_FechaInscripcion,
                        Juga_Activo
                    )
                    VALUES (
                        @UsuaId, 
                        @EquipoId, 
                        @Numero, 
                        @Posicion,
                        GETDATE(),
                        1
                    )", con);

                cmd.Parameters.AddWithValue("@UsuaId", inscripcion.Usua_Id);
                cmd.Parameters.AddWithValue("@EquipoId", equipoId);
                cmd.Parameters.AddWithValue("@Numero", inscripcion.Juga_Numero);
                cmd.Parameters.AddWithValue("@Posicion", inscripcion.Juga_Posicion);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarJugador(int jugadorId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UPDATE Jugadores SET Juga_Activo = 0 WHERE Juga_Id = @JugadorId", con);
                cmd.Parameters.AddWithValue("@JugadorId", jugadorId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}