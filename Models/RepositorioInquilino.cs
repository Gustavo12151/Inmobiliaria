using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioInquilino : RepositorioBase
    {
        public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

        public List<Inquilino> ObtenerTodos()
        {
            var lista = new List<Inquilino>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, DNI, Nombre, Apellido, Contacto FROM Inquilinos";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Inquilino
                        {
                            Id = reader.GetInt32("Id"),
                            DNI = reader.GetString("DNI"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Contacto = reader.GetString("Contacto")
                        });
                    }
                }
            }
            return lista;
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, DNI, Nombre, Apellido, Contacto FROM Inquilinos WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            Id = reader.GetInt32("Id"),
                            DNI = reader.GetString("DNI"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Contacto = reader.GetString("Contacto")
                        };
                    }
                }
            }
            return i;
        }

        public int Alta(Inquilino i)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO Inquilinos (DNI, Nombre, Apellido, Contacto) 
                               VALUES (@dni, @nombre, @apellido, @contacto)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", i.DNI);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@contacto", i.Contacto);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    i.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        public int Modificacion(Inquilino i)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Inquilinos 
                               SET DNI=@dni, Nombre=@nombre, Apellido=@apellido, Contacto=@contacto 
                               WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", i.DNI);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@contacto", i.Contacto);
                    command.Parameters.AddWithValue("@id", i.Id);
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
                string sql = "DELETE FROM Inquilinos WHERE Id=@id";
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
