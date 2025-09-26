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
        public string? UsuarioCreador { get; set; }
        public string? UsuarioAnulador { get; set; }
    }
}
