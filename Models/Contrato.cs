namespace Inmobiliaria.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        public int InmuebleId { get; set; }
        public int InquilinoId { get; set; }
        public int UsuarioCreadorId { get; set; }
        public int? UsuarioFinalizadorId { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaFinOriginal { get; set; } // NUEVO: guarda la fecha original
        public DateTime? FechaTerminacionAnticipada { get; set; }
        public decimal MontoMensual { get; set; }
        public decimal? MultaCalculada { get; set; }
        public string EstadoContrato { get; set; } = "Vigente"; // Vigente / Finalizado / Finalizado anticipadamente

        public Inmueble? Inmueble { get; set; }
        public Inquilino? Inquilino { get; set; }
        public Usuario? UsuarioCreador { get; set; }
        public Usuario? UsuarioFinalizador { get; set; }
    }
}
