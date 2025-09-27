namespace Inmobiliaria.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ContratoId { get; set; }
        public Contrato? Contrato { get; set; }

        public int NumeroPago { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }

        // Auditoría
        public int UsuarioCreadorId { get; set; }
        public string? UsuarioCreador { get; set; }

        public int? UsuarioAnuladorId { get; set; } // Nullable porque puede no estar anulado
        public string? UsuarioAnulador { get; set; }
    }
}
    