using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioPago : RepositorioBase
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration) { }

        public List<Pago> ObtenerTodos()
        {
            var lista = new List<Pago>();
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe,
                        p.UsuarioCreadorId, uc.NombreUsuario AS UsuarioCreador,
                        p.UsuarioAnuladorId, ua.NombreUsuario AS UsuarioAnulador,
                        c.FechaInicio, c.FechaFin
                    FROM Pagos p
                    INNER JOIN Contratos c ON p.ContratoId = c.Id
                    INNER JOIN Usuarios uc ON p.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios ua ON p.UsuarioAnuladorId = ua.Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
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
                            UsuarioCreador = reader["UsuarioCreador"]?.ToString(),
                            UsuarioAnuladorId = reader.IsDBNull(reader.GetOrdinal("UsuarioAnuladorId"))
                                ? (int?)null
                                : reader.GetInt32("UsuarioAnuladorId"),
                            UsuarioAnulador = reader["UsuarioAnulador"]?.ToString(),
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
            return lista;
        }
        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe,
                        p.UsuarioCreadorId, uc.NombreUsuario AS UsuarioCreador,
                        p.UsuarioAnuladorId, ua.NombreUsuario AS UsuarioAnulador
                    FROM Pagos p
                    INNER JOIN Usuarios uc ON p.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios ua ON p.UsuarioAnuladorId = ua.Id
                    WHERE p.Id=@id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
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
                            UsuarioCreador = reader["UsuarioCreador"]?.ToString(),
                            UsuarioAnuladorId = reader.IsDBNull(reader.GetOrdinal("UsuarioAnuladorId"))
                                ? (int?)null
                                : reader.GetInt32("UsuarioAnuladorId"),
                            UsuarioAnulador = reader["UsuarioAnulador"]?.ToString()
                        };
                    }
                }
            }
            return pago;
        }
        public int Alta(Pago p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                // Calcular el próximo número correlativo
                string sqlNumero = @"SELECT IFNULL(MAX(NumeroPago), 0) + 1 
                             FROM Pagos WHERE ContratoId = @contratoId;";
                int proximoNumero = 1;
                using (var cmdNum = new MySqlCommand(sqlNumero, connection))
                {
                    cmdNum.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    connection.Open();
                    proximoNumero = Convert.ToInt32(cmdNum.ExecuteScalar());
                    connection.Close();
                }

                string sql = @"
            INSERT INTO Pagos 
            (ContratoId, NumeroPago, FechaPago, Importe, Concepto, UsuarioCreadorId, Estado)
            VALUES (@contratoId, @numeroPago, @fechaPago, @importe, @concepto, @usuarioCreadorId, 'Activo');
        ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    command.Parameters.AddWithValue("@numeroPago", proximoNumero);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@concepto", p.Concepto ?? "");
                    command.Parameters.AddWithValue("@usuarioCreadorId", p.UsuarioCreadorId);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    p.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        public int Modificacion(Pago p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
            UPDATE Pagos
            SET Concepto=@concepto
            WHERE Id=@id AND Estado='Activo';
        ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", p.Concepto ?? "");
                    command.Parameters.AddWithValue("@id", p.Id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }


        public int Baja(int id)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM Pagos WHERE Id=@id;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Anular(int idPago, int usuarioAnuladorId)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Pagos 
                       SET UsuarioAnuladorId = @usuarioAnuladorId,
                           Estado = 'Anulado'
                       WHERE Id = @idPago";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@usuarioAnuladorId", usuarioAnuladorId);
                    command.Parameters.AddWithValue("@idPago", idPago);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
        public List<Pago> ObtenerTodosPorContrato(int contratoId)
        {
            var lista = new List<Pago>();
            using (var connection = GetConnection())
            {
                string sql = @"
            SELECT 
                p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe, p.Concepto, p.Estado,
                p.UsuarioCreadorId, uc.NombreUsuario AS UsuarioCreador,
                p.UsuarioAnuladorId, ua.NombreUsuario AS UsuarioAnulador,
                c.FechaInicio, c.FechaFin
            FROM Pagos p
            INNER JOIN Contratos c ON p.ContratoId = c.Id
            INNER JOIN Usuarios uc ON p.UsuarioCreadorId = uc.Id
            LEFT JOIN Usuarios ua ON p.UsuarioAnuladorId = ua.Id
            WHERE p.ContratoId = @contratoId
            ORDER BY p.NumeroPago;
        ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", contratoId);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Pago
                        {
                            Id = reader.GetInt32("Id"),
                            ContratoId = reader.GetInt32("ContratoId"),
                            NumeroPago = reader.GetInt32("NumeroPago"),
                            FechaPago = reader.GetDateTime("FechaPago"),
                            Importe = reader.GetDecimal("Importe"),
                            Concepto = reader["Concepto"]?.ToString(),
                            Estado = reader["Estado"]?.ToString() ?? "Activo",
                            UsuarioCreadorId = reader.GetInt32("UsuarioCreadorId"),
                            UsuarioCreador = reader["UsuarioCreador"]?.ToString(),
                            UsuarioAnuladorId = reader.IsDBNull(reader.GetOrdinal("UsuarioAnuladorId")) ? (int?)null : reader.GetInt32("UsuarioAnuladorId"),
                            UsuarioAnulador = reader["UsuarioAnulador"]?.ToString(),
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
            return lista;
        }



    }
}
