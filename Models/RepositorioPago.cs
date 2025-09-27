using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria.Models
{
    public class RepositorioPago : RepositorioBase
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration) { }

        // =========================
        // OBTENER TODOS
        // =========================
        public List<Pago> ObtenerTodos()
        {
            var lista = new List<Pago>();
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe, 
                           p.UsuarioCreadorId, uc.NombreUsuario AS UsuarioCreador,
                           p.UsuarioAnuladorId, ua.NombreUsuario AS UsuarioAnulador,
                           c.FechaInicio, c.FechaFin
                    FROM Pagos p
                    INNER JOIN Contratos c ON p.ContratoId = c.Id
                    INNER JOIN Usuarios uc ON p.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios ua ON p.UsuarioAnuladorId = ua.Id
                    ORDER BY p.FechaPago DESC
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Pago
                            {
                                Id = reader.GetInt32("Id"),
                                ContratoId = reader.GetInt32("ContratoId"),
                                NumeroPago = reader.GetInt32("NumeroPago"),
                                FechaPago = reader.GetDateTime("FechaPago"),
                                Importe = reader.GetDecimal("Importe"),
                                UsuarioCreadorId = reader.GetInt32("UsuarioCreadorId"),
                                UsuarioCreador = reader["UsuarioCreador"] != DBNull.Value ? reader.GetString("UsuarioCreador") : "",
                                UsuarioAnuladorId = reader["UsuarioAnuladorId"] != DBNull.Value ? reader.GetInt32("UsuarioAnuladorId") : 0,
                                UsuarioAnulador = reader["UsuarioAnulador"] != DBNull.Value ? reader.GetString("UsuarioAnulador") : null,
                                Contrato = new Contrato
                                {
                                    Id = reader.GetInt32("ContratoId"),
                                    FechaInicio = reader.GetDateTime("FechaInicio"),
                                    FechaFin = reader.GetDateTime("FechaFin")
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // =========================
        // OBTENER POR ID
        // =========================
        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe, 
                           p.UsuarioCreadorId, uc.NombreUsuario AS UsuarioCreador,
                           p.UsuarioAnuladorId, ua.NombreUsuario AS UsuarioAnulador
                    FROM Pagos p
                    INNER JOIN Usuarios uc ON p.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios ua ON p.UsuarioAnuladorId = ua.Id
                    WHERE p.Id=@id
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pago = new Pago
                            {
                                Id = reader.GetInt32("Id"),
                                ContratoId = reader.GetInt32("ContratoId"),
                                NumeroPago = reader.GetInt32("NumeroPago"),
                                FechaPago = reader.GetDateTime("FechaPago"),
                                Importe = reader.GetDecimal("Importe"),
                                UsuarioCreadorId = reader.GetInt32("UsuarioCreadorId"),
                                UsuarioCreador = reader["UsuarioCreador"] != DBNull.Value ? reader.GetString("UsuarioCreador") : "",
                                UsuarioAnuladorId = reader["UsuarioAnuladorId"] != DBNull.Value ? reader.GetInt32("UsuarioAnuladorId") : 0,
                                UsuarioAnulador = reader["UsuarioAnulador"] != DBNull.Value ? reader.GetString("UsuarioAnulador") : null
                            };
                        }
                    }
                }
            }
            return pago;
        }

        // =========================
        // ALTA
        // =========================
        public int Alta(Pago p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO Pagos (ContratoId, NumeroPago, FechaPago, Importe, UsuarioCreadorId) 
                    VALUES (@contratoId, @numeroPago, @fechaPago, @importe, @usuarioCreadorId)
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    command.Parameters.AddWithValue("@numeroPago", p.NumeroPago);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@usuarioCreadorId", p.UsuarioCreadorId);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    p.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        // =========================
        // MODIFICACION / ANULACION
        // =========================
        public int Modificacion(Pago p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
                    UPDATE Pagos 
                    SET ContratoId=@contratoId, NumeroPago=@numeroPago, FechaPago=@fechaPago, 
                        Importe=@importe, UsuarioCreadorId=@usuarioCreadorId, UsuarioAnuladorId=@usuarioAnuladorId
                    WHERE Id=@id
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    command.Parameters.AddWithValue("@numeroPago", p.NumeroPago);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@usuarioCreadorId", p.UsuarioCreadorId);
                    if (p.UsuarioAnuladorId > 0)
                        command.Parameters.AddWithValue("@usuarioAnuladorId", p.UsuarioAnuladorId);
                    else
                        command.Parameters.AddWithValue("@usuarioAnuladorId", DBNull.Value);
                    command.Parameters.AddWithValue("@id", p.Id);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        // =========================
        // BAJA
        // =========================
        public int Baja(int id)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM Pagos WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
    }
}
