using BackEndTorneo.Models.QR;
using System.Data;
using System.Data.SqlClient;

namespace BackEndTorneo.Data
{
    public class QRData
    {
        private readonly string conexion;

        public QRData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL")!;
        }

        public async Task<string> GenerarQRCapitan()
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                string codigoQR = "QRCAP-" + Guid.NewGuid().ToString();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO QR_Capitanes (
                        QRCa_Codigo,
                        QRCa_FechaGeneracion,
                        QRCa_FechaExpiracion,
                        QRCa_Usado
                    )
                    VALUES (
                        @Codigo,
                        GETDATE(),
                        DATEADD(MONTH, 3, GETDATE()),
                        0
                    )", con);

                cmd.Parameters.AddWithValue("@Codigo", codigoQR);
                await cmd.ExecuteNonQueryAsync();

                return codigoQR;
            }
        }

        public async Task<bool> ValidarYUsarQRCapitan(ValidarQRCapitan validacion)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Verificar si el QR es válido
                SqlCommand cmdVerificar = new SqlCommand(@"
                    SELECT COUNT(1) 
                    FROM QR_Capitanes 
                    WHERE QRCa_Codigo = @Codigo 
                      AND QRCa_Usado = 0 
                      AND QRCa_FechaExpiracion > GETDATE()", con);

                cmdVerificar.Parameters.AddWithValue("@Codigo", validacion.QRCa_Codigo);

                int valido = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());

                if (valido == 0)
                {
                    return false;
                }

                // Marcar QR como usado
                SqlCommand cmdActualizar = new SqlCommand(@"
                    UPDATE QR_Capitanes 
                    SET QRCa_Usado = 1, Usua_Id = @UsuaId 
                    WHERE QRCa_Codigo = @Codigo", con);

                cmdActualizar.Parameters.AddWithValue("@Codigo", validacion.QRCa_Codigo);
                cmdActualizar.Parameters.AddWithValue("@UsuaId", validacion.Usua_Id);

                await cmdActualizar.ExecuteNonQueryAsync();

                // Actualizar rol del usuario a Capitán (Rol_Id = 3)
                SqlCommand cmdRol = new SqlCommand(@"
                    UPDATE Usuarios 
                    SET Rol_Id = 3 
                    WHERE Usua_Id = @UsuaId", con);

                cmdRol.Parameters.AddWithValue("@UsuaId", validacion.Usua_Id);
                await cmdRol.ExecuteNonQueryAsync();

                return true;
            }
        }

        public async Task<List<QRCapitan>> ListarQRCapitanes()
        {
            List<QRCapitan> lista = new List<QRCapitan>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        QRCa_Id,
                        QRCa_Codigo,
                        QRCa_FechaGeneracion,
                        QRCa_FechaExpiracion,
                        QRCa_Usado,
                        Usua_Id
                    FROM QR_Capitanes
                    ORDER BY QRCa_FechaGeneracion DESC", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new QRCapitan
                        {
                            QRCa_Id = Convert.ToInt32(reader["QRCa_Id"]),
                            QRCa_Codigo = reader["QRCa_Codigo"].ToString(),
                            QRCa_FechaGeneracion = reader["QRCa_FechaGeneracion"] != DBNull.Value ? Convert.ToDateTime(reader["QRCa_FechaGeneracion"]) : null,
                            QRCa_FechaExpiracion = reader["QRCa_FechaExpiracion"] != DBNull.Value ? Convert.ToDateTime(reader["QRCa_FechaExpiracion"]) : null,
                            QRCa_Usado = reader["QRCa_Usado"] != DBNull.Value ? Convert.ToBoolean(reader["QRCa_Usado"]) : null,
                            Usua_Id = reader["Usua_Id"] != DBNull.Value ? Convert.ToInt32(reader["Usua_Id"]) : null
                        });
                    }
                }
            }
            return lista;
        }


        public async Task<string> GenerarQREquipo(int equipoId)
        {
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                string codigoQR = "QR-EQUIPO-" + equipoId + "-" + Guid.NewGuid().ToString();

                // Actualizar el QR del equipo
                SqlCommand cmdEquipo = new SqlCommand(@"
            UPDATE Equipos 
            SET Equi_CodigoQR = @CodigoQR 
            WHERE Equi_Id = @EquipoId", con);

                cmdEquipo.Parameters.AddWithValue("@CodigoQR", codigoQR);
                cmdEquipo.Parameters.AddWithValue("@EquipoId", equipoId);
                await cmdEquipo.ExecuteNonQueryAsync();

                // Insertar en InscripcionesQR
                SqlCommand cmd = new SqlCommand(@"
            INSERT INTO InscripcionesQR (
                InQR_CodigoQR,
                Equi_Id,
                InQR_FechaGeneracion,
                InQR_FechaExpiracion,
                InQR_Usado
            )
            VALUES (
                @Codigo,
                @EquipoId,
                GETDATE(),
                DATEADD(MONTH, 1, GETDATE()),
                0
            )", con);

                cmd.Parameters.AddWithValue("@Codigo", codigoQR);
                cmd.Parameters.AddWithValue("@EquipoId", equipoId);
                await cmd.ExecuteNonQueryAsync();

                return codigoQR;
            }
        }

        public async Task<List<QREquipo>> ListarQREquipos()
        {
            List<QREquipo> lista = new List<QREquipo>();
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
            SELECT 
                iq.InQR_Id,
                iq.InQR_CodigoQR,
                iq.Equi_Id,
                e.Equi_Nombre,
                iq.InQR_FechaGeneracion,
                iq.InQR_FechaExpiracion,
                iq.InQR_Usado
            FROM InscripcionesQR iq
            INNER JOIN Equipos e ON iq.Equi_Id = e.Equi_Id
            ORDER BY iq.InQR_FechaGeneracion DESC", con);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new QREquipo
                        {
                            InQR_Id = Convert.ToInt32(reader["InQR_Id"]),
                            InQR_CodigoQR = reader["InQR_CodigoQR"].ToString(),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            InQR_FechaGeneracion = reader["InQR_FechaGeneracion"] != DBNull.Value ? Convert.ToDateTime(reader["InQR_FechaGeneracion"]) : null,
                            InQR_FechaExpiracion = reader["InQR_FechaExpiracion"] != DBNull.Value ? Convert.ToDateTime(reader["InQR_FechaExpiracion"]) : null,
                            InQR_Usado = reader["InQR_Usado"] != DBNull.Value ? Convert.ToBoolean(reader["InQR_Usado"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<QREquipo?> ObtenerQREquipo(int equipoId)
        {
            QREquipo? qr = null;
            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(@"
            SELECT 
                iq.InQR_Id,
                iq.InQR_CodigoQR,
                iq.Equi_Id,
                e.Equi_Nombre,
                iq.InQR_FechaGeneracion,
                iq.InQR_FechaExpiracion,
                iq.InQR_Usado
            FROM InscripcionesQR iq
            INNER JOIN Equipos e ON iq.Equi_Id = e.Equi_Id
            WHERE iq.Equi_Id = @EquipoId
            ORDER BY iq.InQR_FechaGeneracion DESC", con);

                cmd.Parameters.AddWithValue("@EquipoId", equipoId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        qr = new QREquipo
                        {
                            InQR_Id = Convert.ToInt32(reader["InQR_Id"]),
                            InQR_CodigoQR = reader["InQR_CodigoQR"].ToString(),
                            Equi_Id = Convert.ToInt32(reader["Equi_Id"]),
                            Equi_Nombre = reader["Equi_Nombre"].ToString(),
                            InQR_FechaGeneracion = reader["InQR_FechaGeneracion"] != DBNull.Value ? Convert.ToDateTime(reader["InQR_FechaGeneracion"]) : null,
                            InQR_FechaExpiracion = reader["InQR_FechaExpiracion"] != DBNull.Value ? Convert.ToDateTime(reader["InQR_FechaExpiracion"]) : null,
                            InQR_Usado = reader["InQR_Usado"] != DBNull.Value ? Convert.ToBoolean(reader["InQR_Usado"]) : null
                        };
                    }
                }
            }
            return qr;
        }
    }
}