namespace Inmobiliaria.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ContratoId { get; set; }
        public int NumeroPago { get; set; }
        public Contrato? Contrato { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }

        // Auditoría
        public int UsuarioCreadorId { get; set; }
        public int? UsuarioAnuladorId { get; set; }

        // Nombres de usuario (para mostrar en la vista)
        public string? UsuarioCreador { get; set; }
        public string? UsuarioAnulador { get; set; }
    }
}
