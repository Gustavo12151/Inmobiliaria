using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioPropietario : RepositorioBase
    {
        public RepositorioPropietario(IConfiguration configuration) : base(configuration) { }
        /*////////////////////////////////////////////// METODOS //////////////////////////////////////////////////////*/
        public List<Propietario> ObtenerTodos()
        {
            var lista = new List<Propietario>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, DNI, Nombre, Apellido, Contacto FROM Propietarios";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Propietario
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
        /*///////////////////////////////////////////////////////////////////////////////////////////////////////*/
        public Propietario? ObtenerPorId(int id)
        {
            Propietario? p = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, DNI, Nombre, Apellido, Contacto FROM Propietarios WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        p = new Propietario
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
            return p;
        }
        /*////////////////////////////////////////////////////////////////////////////////////////////////////////*/
        public int Alta(Propietario p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO Propietarios (DNI, Nombre, Apellido, Contacto) 
                               VALUES (@dni, @nombre, @apellido, @contacto)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.DNI);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@contacto", p.Contacto);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    p.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }
        /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
        public int Modificacion(Propietario p)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Propietarios SET DNI=@dni, Nombre=@nombre, Apellido=@apellido, Contacto=@contacto 
                               WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.DNI);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@contacto", p.Contacto);
                    command.Parameters.AddWithValue("@id", p.Id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
        /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
        public int Baja(int id)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM Propietarios WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
        /*/////////////////////////////////////////////////////////////////////////////////////////////////*/
    }
}
