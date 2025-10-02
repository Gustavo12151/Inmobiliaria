using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria.Models
{
    public class RepositorioContrato : RepositorioBase
    {
        public RepositorioContrato(IConfiguration configuration) : base(configuration) { }

        // =========================
        // OBTENER TODOS
        // =========================
        public List<Contrato> ObtenerTodos()
{
    var lista = new List<Contrato>();
    using (var connection = GetConnection())
    {
        string sql = @"
            SELECT 
                c.Id,
                c.InmuebleId,
                c.InquilinoId,
                c.UsuarioCreadorId,
                uc.NombreUsuario AS CreadorUsuario,
                c.UsuarioFinalizadorId,
                uf.NombreUsuario AS FinalizadorUsuario,
                c.FechaInicio,
                c.FechaFin,
                c.MontoMensual,
                c.EstadoContrato,
                c.Eliminado,
                i.Direccion AS InmuebleDireccion,
                inq.Nombre AS InquilinoNombre,
                inq.Apellido AS InquilinoApellido
            FROM Contratos c
            INNER JOIN Inmuebles i ON c.InmuebleId = i.Id
            INNER JOIN Inquilinos inq ON c.InquilinoId = inq.Id
            INNER JOIN Usuarios uc ON c.UsuarioCreadorId = uc.Id
            LEFT JOIN Usuarios uf ON c.UsuarioFinalizadorId = uf.Id
            WHERE c.Eliminado='No'";

        using (var command = new MySqlCommand(sql, connection))
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var contrato = new Contrato
                    {
                        Id = reader.GetInt32("Id"),
                        InmuebleId = reader.GetInt32("InmuebleId"),
                        InquilinoId = reader.GetInt32("InquilinoId"),
                        UsuarioCreadorId = reader.GetInt32("UsuarioCreadorId"),
                        UsuarioFinalizadorId = reader.IsDBNull(reader.GetOrdinal("UsuarioFinalizadorId"))
                            ? (int?)null
                            : reader.GetInt32("UsuarioFinalizadorId"),
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        MontoMensual = reader.GetDecimal("MontoMensual"),
                        EstadoContrato = reader.GetString("EstadoContrato"),
                        Eliminado = reader.GetString("Eliminado"),

                        Inmueble = new Inmueble
                        {
                            Id = reader.GetInt32("InmuebleId"),
                            Direccion = reader.GetString("InmuebleDireccion")
                        },
                        Inquilino = new Inquilino
                        {
                            Id = reader.GetInt32("InquilinoId"),
                            Nombre = reader.GetString("InquilinoNombre"),
                            Apellido = reader.GetString("InquilinoApellido")
                        },
                        UsuarioCreador = new Usuario
                        {
                            Id = reader.GetInt32("UsuarioCreadorId"),
                            NombreUsuario = reader.GetString("CreadorUsuario")
                        },
                        UsuarioFinalizador = reader.IsDBNull(reader.GetOrdinal("UsuarioFinalizadorId"))
                            ? null
                            : new Usuario
                            {
                                Id = reader.GetInt32("UsuarioFinalizadorId"),
                                NombreUsuario = reader.GetString("FinalizadorUsuario")
                            }
                    };
                    lista.Add(contrato);
                }
            }
        }
    }
    return lista;
}


        // =========================
        // OBTENER POR ID
        // =========================
        public Contrato? ObtenerPorId(int id)
        {
            Contrato? contrato = null;

            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        c.Id,
                        c.InmuebleId,
                        c.InquilinoId,
                        c.FechaInicio,
                        c.FechaFin,
                        c.MontoMensual,
                        c.UsuarioCreadorId,
                        uc.NombreUsuario AS CreadorUsuario,
                        c.UsuarioFinalizadorId,
                        uf.NombreUsuario AS FinalizadorUsuario,
                        i.Direccion AS InmuebleDireccion,
                        inq.Nombre AS InquilinoNombre,
                        inq.Apellido AS InquilinoApellido
                    FROM Contratos c
                    INNER JOIN Usuarios uc ON c.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios uf ON c.UsuarioFinalizadorId = uf.Id
                    INNER JOIN Inmuebles i ON c.InmuebleId = i.Id
                    INNER JOIN Inquilinos inq ON c.InquilinoId = inq.Id
                    WHERE c.Id = @id
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contrato = new Contrato
                            {
                                Id = reader.GetInt32("Id"),
                                InmuebleId = reader.GetInt32("InmuebleId"),
                                InquilinoId = reader.GetInt32("InquilinoId"),
                                FechaInicio = reader.GetDateTime("FechaInicio"),
                                FechaFin = reader.GetDateTime("FechaFin"),
                                MontoMensual = reader.GetDecimal("MontoMensual"),
                                UsuarioCreador = new Usuario
                                {
                                    Id = reader.GetInt32("UsuarioCreadorId"),
                                    NombreUsuario = reader["CreadorUsuario"] != DBNull.Value ? reader.GetString("CreadorUsuario") : ""
                                },
                                UsuarioFinalizador = reader["UsuarioFinalizadorId"] != DBNull.Value
                                    ? new Usuario
                                    {
                                        Id = reader.GetInt32("UsuarioFinalizadorId"),
                                        NombreUsuario = reader["FinalizadorUsuario"] != DBNull.Value ? reader.GetString("FinalizadorUsuario") : ""
                                    }
                                    : null,
                                Inmueble = new Inmueble
                                {
                                    Id = reader.GetInt32("InmuebleId"),
                                    Direccion = reader.GetString("InmuebleDireccion")
                                },
                                Inquilino = new Inquilino
                                {
                                    Id = reader.GetInt32("InquilinoId"),
                                    Nombre = reader.GetString("InquilinoNombre"),
                                    Apellido = reader.GetString("InquilinoApellido")
                                }
                            };
                        }
                    }
                }
            }

            return contrato;
        }

        // =========================
        // ALTA
        // =========================
        public int Alta(Contrato c)
        {
            if (HaySuperposicion(c.InmuebleId, c.FechaInicio, c.FechaFin))
                throw new Exception("El inmueble ya tiene un contrato activo en ese rango de fechas.");

            return EjecutarAlta(c);
        }

        private int EjecutarAlta(Contrato c)
        {
            int res = -1;

            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO Contratos 
                        (InquilinoId, InmuebleId, FechaInicio, FechaFin, MontoMensual, UsuarioCreadorId)
                    VALUES 
                        (@inquilinoId, @inmuebleId, @fechaInicio, @fechaFin, @monto, @usuarioCreadorId)
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilinoId", c.InquilinoId);
                    command.Parameters.AddWithValue("@inmuebleId", c.InmuebleId);
                    command.Parameters.AddWithValue("@fechaInicio", c.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", c.FechaFin);
                    command.Parameters.AddWithValue("@monto", c.MontoMensual);
                    command.Parameters.AddWithValue("@usuarioCreadorId", c.UsuarioCreadorId);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    c.Id = (int)command.LastInsertedId;
                }
            }

            return res;
        }

        // =========================
        // MODIFICACION
        // =========================
        public int Modificacion(Contrato c)
        {
            if (HaySuperposicion(c.InmuebleId, c.FechaInicio, c.FechaFin, c.Id))
                throw new Exception("La modificación causa superposición con otro contrato existente.");

            return EjecutarModificacion(c);
        }

        private int EjecutarModificacion(Contrato c)
        {
            int res = -1;

            using (var connection = GetConnection())
            {
                string sql = @"
                    UPDATE Contratos
                    SET 
                        InquilinoId=@inquilinoId,
                        InmuebleId=@inmuebleId,
                        FechaInicio=@fechaInicio,
                        FechaFin=@fechaFin,
                        MontoMensual=@monto,
                        UsuarioFinalizadorId=@usuarioFinalizadorId
                    WHERE Id=@id
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilinoId", c.InquilinoId);
                    command.Parameters.AddWithValue("@inmuebleId", c.InmuebleId);
                    command.Parameters.AddWithValue("@fechaInicio", c.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", c.FechaFin);
                    command.Parameters.AddWithValue("@monto", c.MontoMensual);
                    command.Parameters.AddWithValue("@usuarioFinalizadorId", c.UsuarioFinalizadorId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", c.Id);

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
            try
            {
                using (var connection = GetConnection())
                {
                    string sql = "DELETE FROM Contratos WHERE Id=@id";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        res = command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Guardar ex.Message en logs para depurar
                throw new Exception("Error al eliminar contrato: " + ex.Message);
            }
            return res;
        }
        // =========================
        // LÓGICA ADICIONAL
        // =========================
        private bool HaySuperposicion(int inmuebleId, DateTime inicio, DateTime fin, int idExcluido = 0)
        {
            using var connection = GetConnection();
            string sql = @"
        SELECT COUNT(*) FROM Contratos
        WHERE InmuebleId = @inmuebleId
          AND EstadoContrato = 'Vigente'  -- 🔹 solo contratos vigentes
          AND NOT (@fin < FechaInicio OR @inicio > FechaFin)  -- 🔹 se superponen fechas
    ";

            if (idExcluido > 0)
                sql += " AND Id <> @idExcluido";  // 🔹 excluir contrato actual al modificar

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@inmuebleId", inmuebleId);
            command.Parameters.AddWithValue("@inicio", inicio);
            command.Parameters.AddWithValue("@fin", fin);

            if (idExcluido > 0)
                command.Parameters.AddWithValue("@idExcluido", idExcluido);

            connection.Open();
            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;  // 🔹 true si hay superposición
        }

        /*/////////////////////////////////////////////////////////////////////////////*/

        public List<Contrato> ObtenerVigentes()
        {
            var lista = new List<Contrato>();
            using (var connection = GetConnection())
            {
                string sql = @"
            SELECT c.Id, c.InmuebleId, c.InquilinoId, c.UsuarioCreadorId, 
                   c.UsuarioFinalizadorId, c.FechaInicio, c.FechaFin, 
                   c.FechaTerminacionAnticipada, c.MontoMensual, 
                   c.MultaCalculada, c.EstadoContrato,
                   i.Direccion AS InmuebleDireccion,
                   inq.Nombre AS InquilinoNombre,
                   inq.Apellido AS InquilinoApellido
            FROM Contratos c
            INNER JOIN Inmuebles i ON c.InmuebleId = i.Id
            INNER JOIN Inquilinos inq ON c.InquilinoId = inq.Id
            WHERE c.EstadoContrato = 'Vigente';";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var contrato = new Contrato
                            {
                                Id = reader.GetInt32("Id"),
                                InmuebleId = reader.GetInt32("InmuebleId"),
                                InquilinoId = reader.GetInt32("InquilinoId"),
                                UsuarioCreadorId = reader.GetInt32("UsuarioCreadorId"),
                                UsuarioFinalizadorId = reader.IsDBNull(reader.GetOrdinal("UsuarioFinalizadorId"))
                                    ? (int?)null
                                    : reader.GetInt32("UsuarioFinalizadorId"),
                                FechaInicio = reader.GetDateTime("FechaInicio"),
                                FechaFin = reader.GetDateTime("FechaFin"),
                                FechaTerminacionAnticipada = reader.IsDBNull(reader.GetOrdinal("FechaTerminacionAnticipada"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime("FechaTerminacionAnticipada"),
                                MontoMensual = reader.GetDecimal("MontoMensual"),
                                MultaCalculada = reader.IsDBNull(reader.GetOrdinal("MultaCalculada"))
                                    ? (decimal?)null
                                    : reader.GetDecimal("MultaCalculada"),
                                EstadoContrato = reader.GetString("EstadoContrato"),

                                Inmueble = new Inmueble
                                {
                                    Id = reader.GetInt32("InmuebleId"),
                                    Direccion = reader.GetString("InmuebleDireccion")
                                },
                                Inquilino = new Inquilino
                                {
                                    Id = reader.GetInt32("InquilinoId"),
                                    Nombre = reader.GetString("InquilinoNombre"),
                                    Apellido = reader.GetString("InquilinoApellido")
                                }
                            };
                            lista.Add(contrato);
                        }
                    }
                }
            }
            return lista;
        }


        public int FinalizarAnticipado(int idContrato, DateTime fechaTerminacion, int usuarioId)
        {
            var contrato = ObtenerPorId(idContrato);
            if (contrato == null) throw new Exception("Contrato no encontrado");

            // 🔹 Calcular duración y meses cumplidos
            double mesesTotales = ((contrato.FechaFin - contrato.FechaInicio).Days) / 30.0;
            double mesesCumplidos = ((fechaTerminacion - contrato.FechaInicio).Days) / 30.0;

            // 🔹 Calcular multa
            decimal multa = 0;
            if (mesesCumplidos < mesesTotales / 2)
                multa = contrato.MontoMensual * 2;
            else
                multa = contrato.MontoMensual;

            // 🔹 Calcular deuda de meses restantes
            int mesesRestantes = (int)Math.Ceiling(mesesTotales - mesesCumplidos);
            decimal deuda = mesesRestantes * contrato.MontoMensual;

            // 🔹 Actualizar contrato
            contrato.FechaTerminacionAnticipada = fechaTerminacion;
            contrato.MultaCalculada = multa;
            contrato.EstadoContrato = "Finalizado anticipadamente";

            // Guardamos al usuario que finaliza
            contrato.UsuarioFinalizadorId = usuarioId;

            using (var connection = GetConnection())
            {
                string sql = @"
            UPDATE Contratos
            SET 
                FechaTerminacionAnticipada=@fechaTerm,
                MultaCalculada=@multa,
                EstadoContrato=@estado,
                UsuarioFinalizadorId=@usuarioFinalizador
            WHERE Id=@id
        ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@fechaTerm", contrato.FechaTerminacionAnticipada);
                    command.Parameters.AddWithValue("@multa", contrato.MultaCalculada);
                    command.Parameters.AddWithValue("@estado", contrato.EstadoContrato);
                    command.Parameters.AddWithValue("@usuarioFinalizador", contrato.UsuarioFinalizadorId);
                    command.Parameters.AddWithValue("@id", contrato.Id);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int FinalizarContrato(int idContrato, int usuarioId)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"
            UPDATE Contratos
            SET EstadoContrato = 'Finalizado',
                UsuarioFinalizadorId = @usuarioId
            WHERE Id = @idContrato";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@usuarioId", usuarioId);
                    command.Parameters.AddWithValue("@idContrato", idContrato);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

public int EliminarLogico(int id)
{
    int res = -1;
    using (var connection = GetConnection())
    {
        string sql = @"UPDATE Contratos 
                       SET Eliminado='Si' 
                       WHERE Id=@id";
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
