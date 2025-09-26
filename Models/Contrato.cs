namespace Inmobiliaria.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        // 🔹 Claves foráneas
        public int InmuebleId { get; set; }
        public int InquilinoId { get; set; }
        public int UsuarioCreadorId { get; set; }
        public int? UsuarioFinalizadorId { get; set; }  // puede ser NULL

        // 🔹 Propiedades propias
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoMensual { get; set; }

        // 🔹 Propiedades de navegación (no mapeadas directamente en la tabla)
        public Inmueble? Inmueble { get; set; }
        public Inquilino? Inquilino { get; set; }
        public Usuario? UsuarioCreador { get; set; }
        public Usuario? UsuarioFinalizador { get; set; }
    }
}

