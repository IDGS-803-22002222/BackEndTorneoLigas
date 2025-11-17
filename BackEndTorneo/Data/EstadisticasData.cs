using BackEndTorneo.Models.Estadisticas;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class EstadisticasData
    {
        private readonly string conexion;

        public EstadisticasData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Estadistica>> ListarEstadisticasPorPartido(int partidoId)
        {
            List<Estadistica> lista = new List<Estadistica>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        ep.EsPa_Id,
                        ep.Part_Id,
                        ep.Juga_Id,
                        u.Usua_NombreCompleto,
                        e.Equi_Nombre,
                        ep.EsPa_Goles,
                        ep.EsPa_Asistencias,
                        ep.EsPa_TarjetasAmarillas,
                        ep.EsPa_TarjetasRojas,
                        ep.EsPa_MinutosJugados
                    FROM EstadisticasPartido ep
                    INNER JOIN Jugadores j ON ep.Juga_Id = j.Juga_Id
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id
                    WHERE ep.Part_Id = @PartidoId
                    ORDER BY ep.EsPa_Goles DESC", con);

                cmd.Parameters.AddWithValue("@PartidoId", partidoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearEstadistica(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<List<Estadistica>> ListarEstadisticasPorJugador(int jugadorId)
        {
            List<Estadistica> lista = new List<Estadistica>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        ep.EsPa_Id,
                        ep.Part_Id,
                        ep.Juga_Id,
                        u.Usua_NombreCompleto,
                        e.Equi_Nombre,
                        ep.EsPa_Goles,
                        ep.EsPa_Asistencias,
                        ep.EsPa_TarjetasAmarillas,
                        ep.EsPa_TarjetasRojas,
                        ep.EsPa_MinutosJugados
                    FROM EstadisticasPartido ep
                    INNER JOIN Jugadores j ON ep.Juga_Id = j.Juga_Id
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id
                    WHERE ep.Juga_Id = @JugadorId
                    ORDER BY ep.Part_Id DESC", con);

                cmd.Parameters.AddWithValue("@JugadorId", jugadorId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearEstadistica(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<bool> RegistrarEstadistica(RegistrarEstadistica estadistica)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO EstadisticasPartido (
                        Part_Id,
                        Juga_Id,
                        EsPa_Goles,
                        EsPa_Asistencias,
                        EsPa_TarjetasAmarillas,
                        EsPa_TarjetasRojas,
                        EsPa_MinutosJugados
                    )
                    VALUES (
                        @PartidoId,
                        @JugadorId,
                        @Goles,
                        @Asistencias,
                        @TarjetasAmarillas,
                        @TarjetasRojas,
                        @MinutosJugados
                    )", con);

                cmd.Parameters.AddWithValue("@PartidoId", estadistica.Part_Id);
                cmd.Parameters.AddWithValue("@JugadorId", estadistica.Juga_Id);
                cmd.Parameters.AddWithValue("@Goles", estadistica.EsPa_Goles);
                cmd.Parameters.AddWithValue("@Asistencias", estadistica.EsPa_Asistencias);
                cmd.Parameters.AddWithValue("@TarjetasAmarillas", estadistica.EsPa_TarjetasAmarillas);
                cmd.Parameters.AddWithValue("@TarjetasRojas", estadistica.EsPa_TarjetasRojas);
                cmd.Parameters.AddWithValue("@MinutosJugados", estadistica.EsPa_MinutosJugados);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<List<RankingGoleador>> ObtenerRankingGoleadores(int? torneoId = null)
        {
            List<RankingGoleador> lista = new List<RankingGoleador>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                string query = @"
                    SELECT TOP 20
                        u.Usua_NombreCompleto,
                        e.Equi_Nombre,
                        SUM(ep.EsPa_Goles) AS TotalGoles,
                        COUNT(DISTINCT ep.Part_Id) AS PartidosJugados
                    FROM EstadisticasPartido ep
                    INNER JOIN Jugadores j ON ep.Juga_Id = j.Juga_Id
                    INNER JOIN Usuarios u ON j.Usua_Id = u.Usua_Id
                    INNER JOIN Equipos e ON j.Equi_Id = e.Equi_Id";

                if (torneoId.HasValue)
                {
                    query += @"
                    INNER JOIN Partidos p ON ep.Part_Id = p.Part_Id
                    WHERE p.Torn_Id = @TorneoId";
                }

                query += @"
                    GROUP BY u.Usua_NombreCompleto, e.Equi_Nombre
                    ORDER BY TotalGoles DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (torneoId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@TorneoId", torneoId.Value);
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new RankingGoleador
                        {
                            Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            TotalGoles = Convert.ToInt32(reader["TotalGoles"]),
                            PartidosJugados = Convert.ToInt32(reader["PartidosJugados"])
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<List<TablaPosicion>> ObtenerTablaPosiciones(int torneoId)
        {
            List<TablaPosicion> lista = new List<TablaPosicion>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        tp.TaPo_Id,
                        tp.Torn_Id,
                        tp.Equi_Id,
                        e.Equi_Nombre,
                        e.Equi_Logo,
                        tp.TaPo_PartidosJugados,
                        tp.TaPo_PartidosGanados,
                        tp.TaPo_PartidosEmpatados,
                        tp.TaPo_PartidosPerdidos,
                        tp.TaPo_GolesFavor,
                        tp.TaPo_GolesContra,
                        tp.TaPo_DiferenciaGoles,
                        tp.TaPo_Puntos
                    FROM TablaPosiciones tp
                    INNER JOIN Equipos e ON tp.Equi_Id = e.Equi_Id
                    WHERE tp.Torn_Id = @TorneoId
                    ORDER BY tp.TaPo_Puntos DESC, tp.TaPo_DiferenciaGoles DESC, tp.TaPo_GolesFavor DESC", con);

                cmd.Parameters.AddWithValue("@TorneoId", torneoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new TablaPosicion
                        {
                            TaPo_Id = Convert.ToInt32(reader["TaPo_Id"]),
                            Torn_Id = Convert.ToInt32(reader["Torn_Id"]),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            Equi_Logo = reader["Equi_Logo"].ToString(),
                            TaPo_PartidosJugados = reader["TaPo_PartidosJugados"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_PartidosJugados"]) : null,
                            TaPo_PartidosGanados = reader["TaPo_PartidosGanados"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_PartidosGanados"]) : null,
                            TaPo_PartidosEmpatados = reader["TaPo_PartidosEmpatados"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_PartidosEmpatados"]) : null,
                            TaPo_PartidosPerdidos = reader["TaPo_PartidosPerdidos"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_PartidosPerdidos"]) : null,
                            TaPo_GolesFavor = reader["TaPo_GolesFavor"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_GolesFavor"]) : null,
                            TaPo_GolesContra = reader["TaPo_GolesContra"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_GolesContra"]) : null,
                            TaPo_DiferenciaGoles = reader["TaPo_DiferenciaGoles"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_DiferenciaGoles"]) : null,
                            TaPo_Puntos = reader["TaPo_Puntos"] != DBNull.Value ? Convert.ToInt32(reader["TaPo_Puntos"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        private Estadistica MapearEstadistica(SqlDataReader reader)
        {
            return new Estadistica
            {
                EsPa_Id = Convert.ToInt32(reader["EsPa_Id"]),
                Part_Id = Convert.ToInt32(reader["Part_Id"]),
                Juga_Id = Convert.ToInt32(reader["Juga_Id"]),
                Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                Equi_Nombre = reader["Equi_Nombre"].ToString(),
                EsPa_Goles = reader["EsPa_Goles"] != DBNull.Value ? Convert.ToInt32(reader["EsPa_Goles"]) : null,
                EsPa_Asistencias = reader["EsPa_Asistencias"] != DBNull.Value ? Convert.ToInt32(reader["EsPa_Asistencias"]) : null,
                EsPa_TarjetasAmarillas = reader["EsPa_TarjetasAmarillas"] != DBNull.Value ? Convert.ToInt32(reader["EsPa_TarjetasAmarillas"]) : null,
                EsPa_TarjetasRojas = reader["EsPa_TarjetasRojas"] != DBNull.Value ? Convert.ToInt32(reader["EsPa_TarjetasRojas"]) : null,
                EsPa_MinutosJugados = reader["EsPa_MinutosJugados"] != DBNull.Value ? Convert.ToInt32(reader["EsPa_MinutosJugados"]) : null
            };
        }
    }
}