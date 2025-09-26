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
                        c.FechaInicio,
                        c.FechaFin,
                        c.MontoMensual,
                        c.UsuarioCreadorId,
                        uc.NombreUsuario AS CreadorUsuario,
                        c.UsuarioFinalizadorId,
                        uf.NombreUsuario AS FinalizadorUsuario
                    FROM Contratos c
                    INNER JOIN Usuarios uc ON c.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios uf ON c.UsuarioFinalizadorId = uf.Id
                ";

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
                                    : null
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
                        uf.NombreUsuario AS FinalizadorUsuario
                    FROM Contratos c
                    INNER JOIN Usuarios uc ON c.UsuarioCreadorId = uc.Id
                    LEFT JOIN Usuarios uf ON c.UsuarioFinalizadorId = uf.Id
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
                                    : null
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
                    command.Parameters.AddWithValue("@usuarioFinalizadorId", c.UsuarioFinalizadorId?? (object)DBNull.Value);
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

        // =========================
        // LÓGICA ADICIONAL
        // =========================
        private bool HaySuperposicion(int inmuebleId, DateTime inicio, DateTime fin, int idExcluido = 0)
        {
            using var connection = GetConnection();
            string sql = @"
                SELECT COUNT(*) FROM Contratos
                WHERE InmuebleId=@inmuebleId
                  AND NOT (@fin < FechaInicio OR @inicio > FechaFin)
            ";

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
