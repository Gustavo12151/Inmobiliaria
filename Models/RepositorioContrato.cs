using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration; // Se asume que IConfiguration requiere este using

namespace Inmobiliaria.Models
{
    public class RepositorioContrato : RepositorioBase
    {
        public RepositorioContrato(IConfiguration configuration) : base(configuration) { }

        // ======================================
        // OPERACIONES CRUD
        // ======================================

        public List<Contrato> ObtenerTodos()
        {
            var lista = new List<Contrato>();
            using (var connection = GetConnection())
            {
                string sql = @"SELECT c.Id, c.InquilinoId, c.InmuebleId, c.FechaInicio, c.FechaFin, c.MontoMensual,
                                       c.UsuarioCreador, c.UsuarioTerminador,
                                       i.Nombre, i.Apellido, inm.Direccion
                                FROM Contratos c
                                INNER JOIN Inquilinos i ON c.InquilinoId = i.Id
                                INNER JOIN Inmuebles inm ON c.InmuebleId = inm.Id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Contrato
                        {
                            Id = reader.GetInt32("Id"),
                            InquilinoId = reader.GetInt32("InquilinoId"),
                            InmuebleId = reader.GetInt32("InmuebleId"),
                            FechaInicio = reader.GetDateTime("FechaInicio"),
                            FechaFin = reader.GetDateTime("FechaFin"),
                            MontoMensual = reader.GetDecimal("MontoMensual"),
                            UsuarioCreador = reader.IsDBNull(reader.GetOrdinal("UsuarioCreador")) ? null : reader.GetString("UsuarioCreador"),
                            UsuarioTerminador = reader.IsDBNull(reader.GetOrdinal("UsuarioTerminador")) ? null : reader.GetString("UsuarioTerminador"),
                            Inquilino = new Inquilino
                            {
                                Id = reader.GetInt32("InquilinoId"),
                                Nombre = reader.GetString("Nombre"),
                                Apellido = reader.GetString("Apellido")
                            },
                            Inmueble = new Inmueble
                            {
                                Id = reader.GetInt32("InmuebleId"),
                                Direccion = reader.GetString("Direccion")
                            }
                        });
                    }
                }
            }
            return lista;
        }

        public Contrato? ObtenerPorId(int id)
        {
            Contrato? contrato = null;
            using (var connection = GetConnection())
            {
                string sql = @"SELECT c.Id, c.InquilinoId, c.InmuebleId, c.FechaInicio, c.FechaFin, c.MontoMensual,
                                       c.UsuarioCreador, c.UsuarioTerminador
                                FROM Contratos c
                                WHERE c.Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        contrato = new Contrato
                        {
                            Id = reader.GetInt32("Id"),
                            InquilinoId = reader.GetInt32("InquilinoId"),
                            InmuebleId = reader.GetInt32("InmuebleId"),
                            FechaInicio = reader.GetDateTime("FechaInicio"),
                            FechaFin = reader.GetDateTime("FechaFin"),
                            MontoMensual = reader.GetDecimal("MontoMensual"),
                            UsuarioCreador = reader.IsDBNull(reader.GetOrdinal("UsuarioCreador")) ? null : reader.GetString("UsuarioCreador"),
                            UsuarioTerminador = reader.IsDBNull(reader.GetOrdinal("UsuarioTerminador")) ? null : reader.GetString("UsuarioTerminador"),
                            // Nota: Para obtener Inquilino y Inmueble completos se necesitaría una subconsulta o JOINs adicionales
                        };
                    }
                }
            }
            return contrato;
        }

        public int Alta(Contrato c)
        {
            // 1. Validación de superposición
            if (HaySuperposicion(c.InmuebleId, c.FechaInicio, c.FechaFin))
                throw new Exception("El inmueble ya tiene un contrato activo en ese rango de fechas.");

            // 2. Ejecución del alta
            return EjecutarAlta(c);
        }

        public int Modificacion(Contrato c)
        {
            // 1. Validación de superposición (excluyendo el contrato actual)
            if (HaySuperposicion(c.InmuebleId, c.FechaInicio, c.FechaFin, c.Id))
                throw new Exception("La modificación causa una superposición con otro contrato existente para el mismo inmueble.");

            // 2. Ejecución de la modificación
            return EjecutarModificacion(c);
        }

        public int Baja(int id)
        {
            int res = -1;
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
            return res;
        }

        // ======================================
        // MÉTODOS PRIVADOS DE PERSISTENCIA
        // ======================================

        private int EjecutarAlta(Contrato c)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO Contratos (InquilinoId, InmuebleId, FechaInicio, FechaFin, MontoMensual, UsuarioCreador)
                                 VALUES (@inquilinoId, @inmuebleId, @fechaInicio, @fechaFin, @monto, @usuarioCreador)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilinoId", c.InquilinoId);
                    command.Parameters.AddWithValue("@inmuebleId", c.InmuebleId);
                    command.Parameters.AddWithValue("@fechaInicio", c.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", c.FechaFin);
                    command.Parameters.AddWithValue("@monto", c.MontoMensual);
                    command.Parameters.AddWithValue("@usuarioCreador", c.UsuarioCreador);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    c.Id = (int)command.LastInsertedId;
                }
            }
            return res;
        }

        private int EjecutarModificacion(Contrato c)
        {
            int res = -1;
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE Contratos
                                SET InquilinoId=@inquilinoId, InmuebleId=@inmuebleId, FechaInicio=@fechaInicio, FechaFin=@fechaFin,
                                    MontoMensual=@monto, UsuarioTerminador=@usuarioTerminador
                                WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilinoId", c.InquilinoId);
                    command.Parameters.AddWithValue("@inmuebleId", c.InmuebleId);
                    command.Parameters.AddWithValue("@fechaInicio", c.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", c.FechaFin);
                    command.Parameters.AddWithValue("@monto", c.MontoMensual);
                    command.Parameters.AddWithValue("@usuarioTerminador", c.UsuarioTerminador);
                    command.Parameters.AddWithValue("@id", c.Id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        // ======================================
        // LÓGICA DE NEGOCIO ADICIONAL
        // ======================================

        private bool HaySuperposicion(int inmuebleId, DateTime inicio, DateTime fin, int idExcluido = 0)
        {
            using var connection = GetConnection();
            string sql = @"SELECT COUNT(*) FROM Contratos
                           WHERE InmuebleId=@inmuebleId
                           AND NOT (@fin < FechaInicio OR @inicio > FechaFin)";

            if (idExcluido > 0)
                sql += " AND Id <> @idExcluido";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@inmuebleId", inmuebleId);
            command.Parameters.AddWithValue("@inicio", inicio);
            command.Parameters.AddWithValue("@fin", fin);

            if (idExcluido > 0)
                command.Parameters.AddWithValue("@idExcluido", idExcluido);

            connection.Open();
          

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}