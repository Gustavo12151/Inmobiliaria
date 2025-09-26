using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioTipoInmueble : RepositorioBase
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration) { }

        public List<TipoInmueble> ObtenerTodos()
        {
            var lista = new List<TipoInmueble>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, Nombre FROM TiposInmuebles";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new TipoInmueble
                        {
                            Id = reader.GetInt32("Id"),
                            Nombre = reader.GetString("Nombre")
                        });
                    }
                }
            }
            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? tipo = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, Nombre FROM TiposInmuebles WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        tipo = new TipoInmueble
                        {
                            Id = reader.GetInt32("Id"),
                            Nombre = reader.GetString("Nombre")
                        };
                    }
                }
            }
            return tipo;
        }

        public int Alta(TipoInmueble t)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = "INSERT INTO TiposInmuebles (Nombre) VALUES (@nombre)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    t.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        public int Modificacion(TipoInmueble t)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = "UPDATE TiposInmuebles SET Nombre=@nombre WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    command.Parameters.AddWithValue("@id", t.Id);
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
                string sql = "DELETE FROM TiposInmuebles WHERE Id=@id";
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
