using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

        // =========================
        // CRUD BÁSICO
        // =========================
        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, NombreUsuario, Clave, Rol, Avatar FROM Usuarios";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id = reader.GetInt32("Id"),
                            NombreUsuario = reader.GetString("NombreUsuario"),
                            Clave = reader.GetString("Clave"),
                            Rol = reader.GetString("Rol"),
                            Avatar = reader["Avatar"]?.ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? usuario = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, NombreUsuario, Clave, Rol, Avatar FROM Usuarios WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            Id = reader.GetInt32("Id"),
                            NombreUsuario = reader.GetString("NombreUsuario"),
                            Clave = reader.GetString("Clave"),
                            Rol = reader.GetString("Rol"),
                            Avatar = reader["Avatar"]?.ToString()
                        };
                    }
                }
            }
            return usuario;
        }

        public int Alta(Usuario u)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO Usuarios (NombreUsuario, Clave, Rol, Avatar) 
                               VALUES (@nombreUsuario, @clave, @rol, @avatar)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", u.NombreUsuario);
                    command.Parameters.AddWithValue("@clave", u.Clave);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    u.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        public int Modificacion(Usuario u)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Usuarios 
                               SET NombreUsuario=@nombreUsuario, Clave=@clave, Rol=@rol, Avatar=@avatar
                               WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", u.NombreUsuario);
                    command.Parameters.AddWithValue("@clave", u.Clave);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@id", u.Id);
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
                string sql = "DELETE FROM Usuarios WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        // =========================
        // LOGIN / PERFIL
        // =========================
        public Usuario? ObtenerPorUsuario(string nombreUsuario)
        {
            Usuario? usuario = null;
            using (var connection = GetConnection())
            {
                var sql = @"SELECT Id, NombreUsuario, Clave, Rol, Avatar 
                            FROM Usuarios 
                            WHERE NombreUsuario = @nombreUsuario";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32("Id"),
                                NombreUsuario = reader.GetString("NombreUsuario"),
                                Clave = reader.GetString("Clave"),
                                Rol = reader["Rol"]?.ToString(),
                                Avatar = reader["Avatar"]?.ToString()
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        public void ActualizarPerfil(Usuario usuario)
        {
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Usuarios 
                               SET NombreUsuario = @nombreUsuario, Avatar = @avatar
                               WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
                    command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@id", usuario.Id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CambiarClave(int id, string nuevaClave)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE Usuarios SET Clave = @clave WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@clave", nuevaClave);
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // =========================
        // EMPLEADOS (solo Admin)
        // =========================
        public List<Usuario> ObtenerEmpleados()
        {
            var lista = new List<Usuario>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, NombreUsuario, Rol, Avatar FROM Usuarios WHERE Rol = 'Empleado'";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id = reader.GetInt32("Id"),
                            NombreUsuario = reader.GetString("NombreUsuario"),
                            Rol = reader.GetString("Rol"),
                            Avatar = reader["Avatar"]?.ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public void CambiarRol(int id, string nuevoRol)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE Usuarios SET Rol = @rol WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@rol", nuevoRol);
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
