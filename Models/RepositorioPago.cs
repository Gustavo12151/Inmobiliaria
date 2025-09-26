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
                string sql = @"SELECT p.Id, p.ContratoId, p.NumeroPago, p.FechaPago, p.Importe, 
                                      p.UsuarioCreador, p.UsuarioAnulador,
                                      c.FechaInicio, c.FechaFin
                               FROM Pagos p
                               INNER JOIN Contratos c ON p.ContratoId = c.Id";
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
                            UsuarioCreador = reader["UsuarioCreador"]?.ToString(),
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
                string sql = @"SELECT Id, ContratoId, NumeroPago, FechaPago, Importe, UsuarioCreador, UsuarioAnulador
                               FROM Pagos WHERE Id=@id";
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
                            UsuarioCreador = reader["UsuarioCreador"]?.ToString(),
                            UsuarioAnulador = reader["UsuarioAnulador"]?.ToString(),
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
                string sql = @"INSERT INTO Pagos (ContratoId, NumeroPago, FechaPago, Importe, UsuarioCreador) 
                               VALUES (@contratoId, @numeroPago, @fechaPago, @importe, @usuarioCreador)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    command.Parameters.AddWithValue("@numeroPago", p.NumeroPago);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@usuarioCreador", p.UsuarioCreador);
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
                string sql = @"UPDATE Pagos 
                               SET ContratoId=@contratoId, NumeroPago=@numeroPago, FechaPago=@fechaPago, 
                                   Importe=@importe, UsuarioCreador=@usuarioCreador, UsuarioAnulador=@usuarioAnulador
                               WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", p.ContratoId);
                    command.Parameters.AddWithValue("@numeroPago", p.NumeroPago);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@usuarioCreador", p.UsuarioCreador);
                    command.Parameters.AddWithValue("@usuarioAnulador", p.UsuarioAnulador);
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
