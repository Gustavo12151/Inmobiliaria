using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

      
        // CRUD BÁSICO
      
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
                            Clave = reader.GetString("Clave"), // <- se guarda encriptada
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
                            Clave = reader.GetString("Clave"), // <- se guarda encriptada
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
                // 🔐 Cifrar la clave antes de guardarla
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(u.Clave);

                string sql = @"INSERT INTO Usuarios (NombreUsuario, Clave, Rol, Avatar) 
                               VALUES (@nombreUsuario, @clave, @rol, @avatar)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", u.NombreUsuario);
                    command.Parameters.AddWithValue("@clave", hashedPassword);
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
                // ⚠️ IMPORTANTE: no tocar la clave acá
                // la clave se modifica solo desde CambiarClave()
                string sql = @"UPDATE Usuarios 
                               SET NombreUsuario=@nombreUsuario, Rol=@rol, Avatar=@avatar
                               WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreUsuario", u.NombreUsuario);
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

        
        // LOGIN / PERFIL
     
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
                                Clave = reader.GetString("Clave"), // <- encriptada
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
            // Hashear la nueva contraseña aquí
            string hashed = BCrypt.Net.BCrypt.HashPassword(nuevaClave);

            command.Parameters.AddWithValue("@clave", hashed);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}

              // EMPLEADOS (solo Admin)
        
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

        //* VERIFICACIÓN DE LOGIN*/
       
       public bool VerificarLogin(string nombreUsuario, string claveIngresada, out Usuario? usuario)
{
    usuario = ObtenerPorUsuario(nombreUsuario);
    if (usuario == null) return false;

    var stored = usuario.Clave ?? string.Empty;

    try
    {
        // Si parece un hash de BCrypt (ej. "$2a$...","$2b$...","$2y$...")
        if (stored.StartsWith("$2"))
        {
            // Verifica el texto plano ingresado contra el hash guardado
            return BCrypt.Net.BCrypt.Verify(claveIngresada, stored);
        }

        // Si no parece hash: contraseña en texto plano (compatibilidad heredada)
        if (stored == claveIngresada)
        {
            // Migrar: guardar la nueva clave hasheada en la BD
            // NOTA: CambiarClave() en el repo debe hashear la clave antes de guardar.
            CambiarClave(usuario.Id, claveIngresada);

            // refrescar el objeto usuario (opcional pero útil)
            usuario = ObtenerPorId(usuario.Id);
            return true;
        }

        return false;
    }
    catch (Exception)
    {
        // Si BCrypt lanza por algún motivo (salt inválido u otro), intentar fallback seguro:
        // comparar texto plano (solo como último recurso) y migrar si coincide.
        if (stored == claveIngresada)
        {
            CambiarClave(usuario.Id, claveIngresada);
            usuario = ObtenerPorId(usuario.Id);
            return true;
        }
        return false;
    }
}

    }
}
