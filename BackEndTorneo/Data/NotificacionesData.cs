using BackEndTorneo.Models.Notificaciones;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class NotificacionesData
    {
        private readonly string conexion;

        public NotificacionesData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<List<Notificacion>> ListarNotificacionesPorUsuario(int usuaId)
        {
            List<Notificacion> lista = new List<Notificacion>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        n.Noti_Id,
                        n.Usua_Id,
                        u.Usua_NombreCompleto,
                        n.Noti_Titulo,
                        n.Noti_Mensaje,
                        n.Noti_Tipo,
                        n.Noti_Leida,
                        n.Noti_FechaCreacion
                    FROM Notificaciones n
                    INNER JOIN Usuarios u ON n.Usua_Id = u.Usua_Id
                    WHERE n.Usua_Id = @UsuaId
                    ORDER BY n.Noti_FechaCreacion DESC", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearNotificacion(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<List<Notificacion>> ListarNotificacionesNoLeidas(int usuaId)
        {
            List<Notificacion> lista = new List<Notificacion>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        n.Noti_Id,
                        n.Usua_Id,
                        u.Usua_NombreCompleto,
                        n.Noti_Titulo,
                        n.Noti_Mensaje,
                        n.Noti_Tipo,
                        n.Noti_Leida,
                        n.Noti_FechaCreacion
                    FROM Notificaciones n
                    INNER JOIN Usuarios u ON n.Usua_Id = u.Usua_Id
                    WHERE n.Usua_Id = @UsuaId AND n.Noti_Leida = 0
                    ORDER BY n.Noti_FechaCreacion DESC", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearNotificacion(reader));
                    }
                }
            }
            return lista;
        }

        public async Task<int> ContarNotificacionesNoLeidas(int usuaId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(1) 
                    FROM Notificaciones 
                    WHERE Usua_Id = @UsuaId AND Noti_Leida = 0", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task<int> CrearNotificacion(CrearNotificacion notificacion)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Notificaciones (
                        Usua_Id,
                        Noti_Titulo,
                        Noti_Mensaje,
                        Noti_Tipo,
                        Noti_Leida,
                        Noti_FechaCreacion
                    )
                    VALUES (
                        @UsuaId,
                        @Titulo,
                        @Mensaje,
                        @Tipo,
                        0,
                        GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int)", con);

                cmd.Parameters.AddWithValue("@UsuaId", notificacion.Usua_Id);
                cmd.Parameters.AddWithValue("@Titulo", notificacion.Noti_Titulo);
                cmd.Parameters.AddWithValue("@Mensaje", notificacion.Noti_Mensaje);
                cmd.Parameters.AddWithValue("@Tipo", notificacion.Noti_Tipo ?? (object)DBNull.Value);

                var notiId = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(notiId);
            }
        }

        public async Task<bool> EnviarNotificacionMasiva(NotificacionMasiva notificacion)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                foreach (var usuaId in notificacion.Usua_Ids!)
                {
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Notificaciones (
                            Usua_Id,
                            Noti_Titulo,
                            Noti_Mensaje,
                            Noti_Tipo,
                            Noti_Leida,
                            Noti_FechaCreacion
                        )
                        VALUES (
                            @UsuaId,
                            @Titulo,
                            @Mensaje,
                            @Tipo,
                            0,
                            GETDATE()
                        )", con);

                    cmd.Parameters.AddWithValue("@UsuaId", usuaId);
                    cmd.Parameters.AddWithValue("@Titulo", notificacion.Noti_Titulo);
                    cmd.Parameters.AddWithValue("@Mensaje", notificacion.Noti_Mensaje);
                    cmd.Parameters.AddWithValue("@Tipo", notificacion.Noti_Tipo ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
        }

        public async Task<bool> MarcarComoLeida(int notificacionId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Notificaciones 
                    SET Noti_Leida = 1 
                    WHERE Noti_Id = @NotificacionId", con);

                cmd.Parameters.AddWithValue("@NotificacionId", notificacionId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> MarcarTodasComoLeidas(int usuaId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Notificaciones 
                    SET Noti_Leida = 1 
                    WHERE Usua_Id = @UsuaId AND Noti_Leida = 0", con);

                cmd.Parameters.AddWithValue("@UsuaId", usuaId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarNotificacion(int notificacionId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("DELETE FROM Notificaciones WHERE Noti_Id = @NotificacionId", con);
                cmd.Parameters.AddWithValue("@NotificacionId", notificacionId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        // Notificaciones automáticas
        public async Task NotificarProximoPartido(int partidoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Obtener datos del partido
                SqlCommand cmdPartido = new SqlCommand(@"
                    SELECT 
                        p.Equi_Id_Local,
                        p.Equi_Id_Visitante,
                        el.Equi_Nombre AS EquipoLocal,
                        ev.Equi_Nombre AS EquipoVisitante,
                        p.Part_FechaPartido,
                        s.Sede_Nombre
                    FROM Partidos p
                    INNER JOIN Equipos el ON p.Equi_Id_Local = el.Equi_Id
                    INNER JOIN Equipos ev ON p.Equi_Id_Visitante = ev.Equi_Id
                    LEFT JOIN Sedes s ON p.Sede_Id = s.Sede_Id
                    WHERE p.Part_Id = @PartidoId", con);

                cmdPartido.Parameters.AddWithValue("@PartidoId", partidoId);

                using (var reader = await cmdPartido.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string mensaje = $"Próximo partido: {reader["EquipoLocal"]} vs {reader["EquipoVisitante"]} el {Convert.ToDateTime(reader["Part_FechaPartido"]):dd/MM/yyyy HH:mm} en {reader["Sede_Nombre"]}";

                        // Obtener jugadores de ambos equipos
                        var equipoLocal = Convert.ToInt32(reader["Equi_Id_Local"]);
                        var equipoVisitante = Convert.ToInt32(reader["Equi_Id_Visitante"]);

                        reader.Close();

                        // Notificar jugadores del equipo local
                        await NotificarJugadoresEquipo(con, equipoLocal, "Próximo Partido", mensaje, "Recordatorio");

                        // Notificar jugadores del equipo visitante
                        await NotificarJugadoresEquipo(con, equipoVisitante, "Próximo Partido", mensaje, "Recordatorio");
                    }
                }
            }
        }

        private async Task NotificarJugadoresEquipo(SqlConnection con, int equipoId, string titulo, string mensaje, string tipo)
        {
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Notificaciones (Usua_Id, Noti_Titulo, Noti_Mensaje, Noti_Tipo, Noti_Leida, Noti_FechaCreacion)
                SELECT j.Usua_Id, @Titulo, @Mensaje, @Tipo, 0, GETDATE()
                FROM Jugadores j
                WHERE j.Equi_Id = @EquipoId AND j.Juga_Activo = 1", con);

            cmd.Parameters.AddWithValue("@EquipoId", equipoId);
            cmd.Parameters.AddWithValue("@Titulo", titulo);
            cmd.Parameters.AddWithValue("@Mensaje", mensaje);
            cmd.Parameters.AddWithValue("@Tipo", tipo);

            await cmd.ExecuteNonQueryAsync();
        }

        private Notificacion MapearNotificacion(SqlDataReader reader)
        {
            return new Notificacion
            {
                Noti_Id = Convert.ToInt32(reader["Noti_Id"]),
                Usua_Id = Convert.ToInt32(reader["Usua_Id"]),
                Usua_NombreCompleto = reader["Usua_NombreCompleto"].ToString(),
                Noti_Titulo = reader["Noti_Titulo"].ToString(),
                Noti_Mensaje = reader["Noti_Mensaje"].ToString(),
                Noti_Tipo = reader["Noti_Tipo"].ToString(),
                Noti_Leida = reader["Noti_Leida"] != DBNull.Value ? Convert.ToBoolean(reader["Noti_Leida"]) : null,
                Noti_FechaCreacion = reader["Noti_FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(reader["Noti_FechaCreacion"]) : null
            };
        }
    }
}