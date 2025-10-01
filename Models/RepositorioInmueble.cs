using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioInmueble : RepositorioBase
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration) { }

        public List<Inmueble> ObtenerTodos()
        {
            var lista = new List<Inmueble>();
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        i.Id,
                        i.Direccion,
                        i.Ambientes,
                        i.Superficie,
                        i.Estado AS EstadoBase,
                        i.Precio,
                        i.PropietarioId,
                        p.Nombre AS PropietarioNombre,
                        p.Apellido AS PropietarioApellido,
                        i.TipoInmuebleId,
                        t.Nombre AS TipoNombre,
                        CASE 
                            WHEN EXISTS (
                                SELECT 1 FROM Contratos c
                                WHERE c.InmuebleId = i.Id
                                   AND c.EstadoContrato='Vigente' 
                                  AND @hoy BETWEEN c.FechaInicio AND c.FechaFin
                            ) THEN 'Ocupado'
                            ELSE 'Disponible'
                        END AS EstadoActual
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.PropietarioId = p.Id
                    INNER JOIN TiposInmuebles t ON i.TipoInmuebleId = t.Id
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@hoy", DateTime.Now.Date);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Inmueble
                            {
                                Id = reader.GetInt32("Id"),
                                Direccion = reader.GetString("Direccion"),
                                Ambientes = reader.GetInt32("Ambientes"),
                                Superficie = reader.GetDecimal("Superficie"),
                                Estado = reader.GetString("EstadoBase"),
                                Precio = reader.GetDecimal("Precio"),
                                IdPropietario = reader.GetInt32("PropietarioId"),
                                IdTipo = reader.GetInt32("TipoInmuebleId"),
                                Propietario = new Propietario
                                {
                                    Id = reader.GetInt32("PropietarioId"),
                                    Nombre = reader.GetString("PropietarioNombre"),
                                    Apellido = reader.GetString("PropietarioApellido")
                                },
                                Tipo = new TipoInmueble
                                {
                                    Id = reader.GetInt32("TipoInmuebleId"),
                                    Nombre = reader.GetString("TipoNombre")
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? inmueble = null;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        i.Id,
                        i.Direccion,
                        i.Ambientes,
                        i.Superficie,
                        i.Estado AS EstadoBase,
                        i.Precio,
                        i.PropietarioId,
                        i.TipoInmuebleId,
                        p.Nombre AS PropietarioNombre,
                        p.Apellido AS PropietarioApellido,
                        t.Nombre AS TipoNombre,
                        CASE 
                            WHEN EXISTS (
                                SELECT 1 FROM Contratos c
                                WHERE c.InmuebleId = i.Id
                                  AND c.EstadoContrato='Vigente' 
                                  AND @hoy BETWEEN c.FechaInicio AND c.FechaFin
                            ) THEN 'Ocupado'
                            ELSE 'Disponible'
                        END AS EstadoActual
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.PropietarioId = p.Id
                    INNER JOIN TiposInmuebles t ON i.TipoInmuebleId = t.Id
                    WHERE i.Id=@id
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@hoy", DateTime.Now.Date);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inmueble = new Inmueble
                            {
                                Id = reader.GetInt32("Id"),
                                Direccion = reader.GetString("Direccion"),
                                Ambientes = reader.GetInt32("Ambientes"),
                                Superficie = reader.GetDecimal("Superficie"),
                                Estado = reader.GetString("EstadoBase"),
                                Precio = reader.GetDecimal("Precio"),
                                IdPropietario = reader.GetInt32("PropietarioId"),
                                IdTipo = reader.GetInt32("TipoInmuebleId"),
                                Propietario = new Propietario
                                {
                                    Id = reader.GetInt32("PropietarioId"),
                                    Nombre = reader.GetString("PropietarioNombre"),
                                    Apellido = reader.GetString("PropietarioApellido")
                                },
                                Tipo = new TipoInmueble
                                {
                                    Id = reader.GetInt32("TipoInmuebleId"),
                                    Nombre = reader.GetString("TipoNombre")
                                }
                            };
                        }
                    }
                }
            }
            return inmueble;
        }



        public int Alta(Inmueble i)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
            INSERT INTO Inmuebles 
                (`Direccion`, `Ambientes`, `Superficie`, `Precio`, `Estado`, `PropietarioId`, `TipoInmuebleId`)
            VALUES 
                (@direccion, @ambientes, @superficie, @precio, @estado, @propietarioId, @tipoInmuebleId)";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@direccion", i.Direccion);
                    command.Parameters.AddWithValue("@ambientes", i.Ambientes);
                    command.Parameters.AddWithValue("@superficie", i.Superficie);
                    command.Parameters.AddWithValue("@precio", i.Precio);
                    command.Parameters.AddWithValue("@estado", i.Estado); // Debe ser "Disponible" o "Ocupado"
                    command.Parameters.AddWithValue("@propietarioId", i.IdPropietario);
                    command.Parameters.AddWithValue("@tipoInmuebleId", i.IdTipo);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    i.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }


        public int Modificacion(Inmueble i)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
            UPDATE Inmuebles
            SET Direccion=@direccion, 
                Ambientes=@ambientes, 
                Superficie=@superficie, 
                Precio=@precio, 
                Estado=@estado,
                PropietarioId=@propietarioId, 
                TipoInmuebleId=@tipoInmuebleId
            WHERE Id=@id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@direccion", i.Direccion);
                    command.Parameters.AddWithValue("@ambientes", i.Ambientes);
                    command.Parameters.AddWithValue("@superficie", i.Superficie);
                    command.Parameters.AddWithValue("@precio", i.Precio);
                    command.Parameters.AddWithValue("@estado", i.Estado);
                    command.Parameters.AddWithValue("@propietarioId", i.IdPropietario);
                    command.Parameters.AddWithValue("@tipoInmuebleId", i.IdTipo);
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
                string sql = "DELETE FROM Inmuebles WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public List<Inmueble> ObtenerDisponibles()
        {
            var lista = new List<Inmueble>();
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        i.Id,
                        i.Direccion,
                        i.Ambientes,
                        i.Superficie,
                        i.Estado AS EstadoBase,
                        i.Precio,
                        i.PropietarioId,
                        p.Nombre AS PropietarioNombre,
                        p.Apellido AS PropietarioApellido,
                        i.TipoInmuebleId,
                        t.Nombre AS TipoNombre
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.PropietarioId = p.Id
                    INNER JOIN TiposInmuebles t ON i.TipoInmuebleId = t.Id
                    WHERE i.Id NOT IN (
                        SELECT InmuebleId FROM Contratos
                        WHERE @hoy BETWEEN FechaInicio AND FechaFin
                    )
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@hoy", DateTime.Now.Date);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Inmueble
                            {
                                Id = reader.GetInt32("Id"),
                                Direccion = reader.GetString("Direccion"),
                                Ambientes = reader.GetInt32("Ambientes"),
                                Superficie = reader.GetDecimal("Superficie"),
                                Estado = "Disponible", // ya sabemos que está libre
                                Precio = reader.GetDecimal("Precio"),
                                IdPropietario = reader.GetInt32("PropietarioId"),
                                IdTipo = reader.GetInt32("TipoInmuebleId"),
                                Propietario = new Propietario
                                {
                                    Id = reader.GetInt32("PropietarioId"),
                                    Nombre = reader.GetString("PropietarioNombre"),
                                    Apellido = reader.GetString("PropietarioApellido")
                                },
                                Tipo = new TipoInmueble
                                {
                                    Id = reader.GetInt32("TipoInmuebleId"),
                                    Nombre = reader.GetString("TipoNombre")
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<Inmueble> ObtenerNoOcupadosEntreFechas(DateTime inicio, DateTime fin)
{
    var lista = new List<Inmueble>();
    using (var connection = GetConnection())
    {
        string sql = @"
            SELECT i.Id, i.Direccion, i.Ambientes, i.Superficie, i.Estado, i.Precio,
                   i.PropietarioId, p.Nombre, p.Apellido,
                   i.TipoInmuebleId, t.Nombre AS TipoNombre
            FROM Inmuebles i
            INNER JOIN Propietarios p ON i.PropietarioId = p.Id
            INNER JOIN TiposInmuebles t ON i.TipoInmuebleId = t.Id
            WHERE i.Id NOT IN (
                SELECT InmuebleId 
                FROM Contratos c
                WHERE c.EstadoContrato = 'Vigente'
                AND NOT (@fin < c.FechaInicio OR @inicio > c.FechaFin)
            )";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@inicio", inicio);
            command.Parameters.AddWithValue("@fin", fin);
            connection.Open();

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("Id"),
                    Direccion = reader.GetString("Direccion"),
                    Ambientes = reader.GetInt32("Ambientes"),
                    Superficie = reader.GetDecimal("Superficie"),
                    Estado = reader.GetString("Estado"),
                    Precio = reader.GetDecimal("Precio"),
                    IdPropietario = reader.GetInt32("PropietarioId"),
                    IdTipo = reader.GetInt32("TipoInmuebleId"),
                    Propietario = new Propietario
                    {
                        Id = reader.GetInt32("PropietarioId"),
                        Nombre = reader.GetString("Nombre"),
                        Apellido = reader.GetString("Apellido")
                    },
                    Tipo = new TipoInmueble
                    {
                        Id = reader.GetInt32("TipoInmuebleId"),
                        Nombre = reader.GetString("TipoNombre")
                    }
                });
            }
        }
    }
    return lista;
}

        public List<Propietario> ObtenerPropietarios()
        {
            var lista = new List<Propietario>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, Nombre, Apellido FROM Propietarios";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Propietario
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Apellido = reader.GetString("Apellido")
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<TipoInmueble> ObtenerTipos()
        {
            var lista = new List<TipoInmueble>();
            using (var connection = GetConnection())
            {
                string sql = "SELECT Id, Nombre FROM TiposInmuebles";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
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
            }
            return lista;
        }
        
        public void CambiarEstado(int id, string nuevoEstado)
{
    using (var connection = GetConnection())
    {
        string sql = @"UPDATE Inmuebles 
                       SET Estado = @estado
                       WHERE Id = @id";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@estado", nuevoEstado);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}



    }
}