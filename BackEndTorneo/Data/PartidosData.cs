using BackEndTorneo.Models.Partidos;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class PartidosData
    {
        private readonly string conexion;

        public PartidosData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Partido>> ListarPartidos()
        {
            List<Partido> lista = new List<Partido>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        p.Part_Id,
                        p.Torn_Id,
                        t.Torn_Nombre,
                        p.Equi_Id_Local,
                        el.Equi_Nombre AS Equi_Nombre_Local,
                        p.Equi_Id_Visitante,
                        ev.Equi_Nombre AS Equi_Nombre_Visitante,
                        p.Part_FechaPartido,
                        p.Sede_Id,
                        s.Sede_Nombre,
                        p.Usua_Id,
                        u.Usua_NombreCompleto AS Arbitro_Nombre,
                        p.Part_GolesLocal,
                        p.Part_GolesVisitante,
                        p.Part_Estado,
                        p.Part_Jornada,
                        p.Part_FechaCreacion
                    FROM Partidos p
                    INNER JOIN Torneos t ON p.Torn_Id = t.Torn_Id
                    INNER JOIN Equipos el ON p.Equi_Id_Local = el.Equi_Id
                    INNER JOIN Equipos ev ON p.Equi_Id_Visitante = ev.Equi_Id
                    LEFT JOIN Sedes s ON p.Sede_Id = s.Sede_Id
                    LEFT JOIN Usuarios u ON p.Usua_Id = u.Usua_Id
                    ORDER BY p.Part_FechaPartido DESC", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearPartido(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<List<Partido>> ListarPartidosPorTorneo(int torneoId)
        {
            List<Partido> lista = new List<Partido>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        p.Part_Id,
                        p.Torn_Id,
                        t.Torn_Nombre,
                        p.Equi_Id_Local,
                        el.Equi_Nombre AS Equi_Nombre_Local,
                        p.Equi_Id_Visitante,
                        ev.Equi_Nombre AS Equi_Nombre_Visitante,
                        p.Part_FechaPartido,
                        p.Sede_Id,
                        s.Sede_Nombre,
                        p.Usua_Id,
                        u.Usua_NombreCompleto AS Arbitro_Nombre,
                        p.Part_GolesLocal,
                        p.Part_GolesVisitante,
                        p.Part_Estado,
                        p.Part_Jornada,
                        p.Part_FechaCreacion
                    FROM Partidos p
                    INNER JOIN Torneos t ON p.Torn_Id = t.Torn_Id
                    INNER JOIN Equipos el ON p.Equi_Id_Local = el.Equi_Id
                    INNER JOIN Equipos ev ON p.Equi_Id_Visitante = ev.Equi_Id
                    LEFT JOIN Sedes s ON p.Sede_Id = s.Sede_Id
                    LEFT JOIN Usuarios u ON p.Usua_Id = u.Usua_Id
                    WHERE p.Torn_Id = @TorneoId
                    ORDER BY p.Part_Jornada, p.Part_FechaPartido", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearPartido(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<Partido?> ObtenerPartido(int partidoId)
        {
            Partido? partido = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        p.Part_Id,
                        p.Torn_Id,
                        t.Torn_Nombre,
                        p.Equi_Id_Local,
                        el.Equi_Nombre AS Equi_Nombre_Local,
                        p.Equi_Id_Visitante,
                        ev.Equi_Nombre AS Equi_Nombre_Visitante,
                        p.Part_FechaPartido,
                        p.Sede_Id,
                        s.Sede_Nombre,
                        p.Usua_Id,
                        u.Usua_NombreCompleto AS Arbitro_Nombre,
                        p.Part_GolesLocal,
                        p.Part_GolesVisitante,
                        p.Part_Estado,
                        p.Part_Jornada,
                        p.Part_FechaCreacion
                    FROM Partidos p
                    INNER JOIN Torneos t ON p.Torn_Id = t.Torn_Id
                    INNER JOIN Equipos el ON p.Equi_Id_Local = el.Equi_Id
                    INNER JOIN Equipos ev ON p.Equi_Id_Visitante = ev.Equi_Id
                    LEFT JOIN Sedes s ON p.Sede_Id = s.Sede_Id
                    LEFT JOIN Usuarios u ON p.Usua_Id = u.Usua_Id
                    WHERE p.Part_Id = @PartidoId", con);

                cmd.Parameters.AddWithValue("@PartidoId", partidoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        partido = MapearPartido(reader);
                    }
                }
            }
            return partido;
        }

        public async Task<int> CrearPartido(CrearPartido partido)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Partidos (
                        Torn_Id,
                        Equi_Id_Local,
                        Equi_Id_Visitante,
                        Part_FechaPartido,
                        Sede_Id,
                        Usua_Id,
                        Part_GolesLocal,
                        Part_GolesVisitante,
                        Part_Estado,
                        Part_Jornada,
                        Part_FechaCreacion
                    )
                    VALUES (
                        @TorneoId,
                        @EquipoLocal,
                        @EquipoVisitante,
                        @FechaPartido,
                        @SedeId,
                        @ArbitroId,
                        0,
                        0,
                        'Programado',
                        @Jornada,
                        GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@TorneoId", partido.Torn_Id);
                cmd.Parameters.AddWithValue("@EquipoLocal", partido.Equi_Id_Local);
                cmd.Parameters.AddWithValue("@EquipoVisitante", partido.Equi_Id_Visitante);
                cmd.Parameters.AddWithValue("@FechaPartido", partido.Part_FechaPartido);
                cmd.Parameters.AddWithValue("@SedeId", partido.Sede_Id ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ArbitroId", partido.Usua_Id ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Jornada", partido.Part_Jornada ?? (object)DBNull.Value);

                var partidoId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(partidoId);
            }
        }

        public async Task<bool> RegistrarResultado(RegistrarResultado resultado)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Partidos SET
                        Part_GolesLocal = @GolesLocal,
                        Part_GolesVisitante = @GolesVisitante,
                        Part_Estado = @Estado
                    WHERE Part_Id = @PartidoId", con);

                cmd.Parameters.AddWithValue("@PartidoId", resultado.Part_Id);
                cmd.Parameters.AddWithValue("@GolesLocal", resultado.Part_GolesLocal);
                cmd.Parameters.AddWithValue("@GolesVisitante", resultado.Part_GolesVisitante);
                cmd.Parameters.AddWithValue("@Estado", resultado.Part_Estado ?? "Finalizado");

                bool actualizado = await cmd.ExecuteNonQueryAsync() > 0;

                // Si se finalizó el partido, actualizar tabla de posiciones
                if (actualizado && resultado.Part_Estado == "Finalizado")
                {
                    await ActualizarTablaPosiciones(con, resultado.Part_Id);
                }

                return actualizado;
            }
        }

        private async Task ActualizarTablaPosiciones(SqlConnection con, int partidoId)
        {
            SqlCommand cmd = new SqlCommand("sp_ActualizarTablaPosiciones", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Part_Id", partidoId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> EditarPartido(Partido partido)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Partidos SET
                        Part_FechaPartido = @FechaPartido,
                        Sede_Id = @SedeId,
                        Usua_Id = @ArbitroId,
                        Part_Jornada = @Jornada
                    WHERE Part_Id = @PartidoId", con);

                cmd.Parameters.AddWithValue("@PartidoId", partido.Part_Id);
                cmd.Parameters.AddWithValue("@FechaPartido", partido.Part_FechaPartido);
                cmd.Parameters.AddWithValue("@SedeId", partido.Sede_Id ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ArbitroId", partido.Usua_Id ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Jornada", partido.Part_Jornada ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> CambiarEstadoPartido(int partidoId, string estado)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"UPDATE Partidos SET Part_Estado = @Estado WHERE Part_Id = @PartidoId", con);
                cmd.Parameters.AddWithValue("@PartidoId", partidoId);
                cmd.Parameters.AddWithValue("@Estado", estado);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> FinalizarPartidoAutomatico(int partidoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Obtener datos del partido
                SqlCommand cmdPartido = new SqlCommand(@"SELECT Torn_Id, Equi_Id_Local, Equi_Id_Visitante FROM Partidos WHERE Part_Id = @PartidoId", con);
                cmdPartido.Parameters.AddWithValue("@PartidoId", partidoId);

                int? torneoId = null;
                int equiLocal = 0;
                int equiVisitante = 0;

                using (var reader = await cmdPartido.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return false;
                    }

                    torneoId = Convert.ToInt32(reader["Torn_Id"]);
                    equiLocal = Convert.ToInt32(reader["Equi_Id_Local"]);
                    equiVisitante = Convert.ToInt32(reader["Equi_Id_Visitante"]);
                }

                // Obtener goles por equipo a partir de EstadisticasPartido
                SqlCommand cmdGoles = new SqlCommand(@"
                    SELECT j.Equi_Id, SUM(ep.EsPa_Goles) AS Goles
                    FROM EstadisticasPartido ep
                    INNER JOIN Jugadores j ON ep.Juga_Id = j.Juga_Id
                    WHERE ep.Part_Id = @PartidoId
                    GROUP BY j.Equi_Id", con);

                cmdGoles.Parameters.AddWithValue("@PartidoId", partidoId);

                int golesLocal = 0;
                int golesVisitante = 0;

                using (var readerG = await cmdGoles.ExecuteReaderAsync())
                {
                    while (await readerG.ReadAsync())
                    {
                        int equiId = Convert.ToInt32(readerG["Equi_Id"]);
                        int goles = Convert.ToInt32(readerG["Goles"]);

                        if (equiId == equiLocal)
                            golesLocal = goles;
                        else if (equiId == equiVisitante)
                            golesVisitante = goles;
                    }
                }

                // Actualizar resultado y estado
                SqlCommand cmdUpdate = new SqlCommand(@"UPDATE Partidos SET
                        Part_GolesLocal = @GolesLocal,
                        Part_GolesVisitante = @GolesVisitante,
                        Part_Estado = 'Finalizado'
                    WHERE Part_Id = @PartidoId", con);

                cmdUpdate.Parameters.AddWithValue("@PartidoId", partidoId);
                cmdUpdate.Parameters.AddWithValue("@GolesLocal", golesLocal);
                cmdUpdate.Parameters.AddWithValue("@GolesVisitante", golesVisitante);

                bool actualizado = await cmdUpdate.ExecuteNonQueryAsync() > 0;

                if (actualizado)
                {
                    await ActualizarTablaPosiciones(con, partidoId);
                }

                return actualizado;
            }
        }

        public async Task<List<Partido>> ObtenerProximosPartidos()
        {
            List<Partido> lista = new List<Partido>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 10
                        p.Part_Id,
                        p.Torn_Id,
                        t.Torn_Nombre,
                        p.Equi_Id_Local,
                        el.Equi_Nombre AS Equi_Nombre_Local,
                        p.Equi_Id_Visitante,
                        ev.Equi_Nombre AS Equi_Nombre_Visitante,
                        p.Part_FechaPartido,
                        p.Sede_Id,
                        s.Sede_Nombre,
                        p.Usua_Id,
                        u.Usua_NombreCompleto AS Arbitro_Nombre,
                        p.Part_GolesLocal,
                        p.Part_GolesVisitante,
                        p.Part_Estado,
                        p.Part_Jornada,
                        p.Part_FechaCreacion
                    FROM Partidos p
                    INNER JOIN Torneos t ON p.Torn_Id = t.Torn_Id
                    INNER JOIN Equipos el ON p.Equi_Id_Local = el.Equi_Id
                    INNER JOIN Equipos ev ON p.Equi_Id_Visitante = ev.Equi_Id
                    LEFT JOIN Sedes s ON p.Sede_Id = s.Sede_Id
                    LEFT JOIN Usuarios u ON p.Usua_Id = u.Usua_Id
                    WHERE p.Part_Estado = 'Programado' AND p.Part_FechaPartido > GETDATE()
                    ORDER BY p.Part_FechaPartido", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearPartido(reader));
                    }
                }
            }
            return lista;
        }

        private Partido MapearPartido(SqlDataReader reader)
        {
            return new Partido
            {
                Part_Id = Convert.ToInt32(reader["Part_Id"]),
                Torn_Id = Convert.ToInt32(reader["Torn_Id"]),
                Torn_Nombre = reader["Torn_Nombre"].ToString(),
                Equi_Id_Local = Convert.ToInt32(reader["Equi_Id_Local"]),
                Equi_Nombre_Local = reader["Equi_Nombre_Local"].ToString(),
                Equi_Id_Visitante = Convert.ToInt32(reader["Equi_Id_Visitante"]),
                Equi_Nombre_Visitante = reader["Equi_Nombre_Visitante"].ToString(),
                Part_FechaPartido = Convert.ToDateTime(reader["Part_FechaPartido"]),
                Sede_Id = reader["Sede_Id"] != DBNull.Value ? Convert.ToInt32(reader["Sede_Id"]) : null,
                Sede_Nombre = reader["Sede_Nombre"].ToString(),
                Usua_Id = reader["Usua_Id"] != DBNull.Value ? Convert.ToInt32(reader["Usua_Id"]) : null,
                Arbitro_Nombre = reader["Arbitro_Nombre"].ToString(),
                Part_GolesLocal = reader["Part_GolesLocal"] != DBNull.Value ? Convert.ToInt32(reader["Part_GolesLocal"]) : null,
                Part_GolesVisitante = reader["Part_GolesVisitante"] != DBNull.Value ? Convert.ToInt32(reader["Part_GolesVisitante"]) : null,
                Part_Estado = reader["Part_Estado"].ToString(),
                Part_Jornada = reader["Part_Jornada"] != DBNull.Value ? Convert.ToInt32(reader["Part_Jornada"]) : null,
                Part_FechaCreacion = reader["Part_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Part_FechaCreacion"]) : null
            };
        }
    }
}