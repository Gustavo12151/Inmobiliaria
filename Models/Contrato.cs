namespace Inmobiliaria.Models
{
    public class Contrato
    {
        public int Id { get; set; }
        public int InquilinoId { get; set; }
        public Inquilino? Inquilino { get; set; }
        public int InmuebleId { get; set; }
        public Inmueble? Inmueble { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoMensual { get; set; }
        public string? UsuarioCreador { get; set; }
        public string? UsuarioTerminador { get; set; }
    }
}
