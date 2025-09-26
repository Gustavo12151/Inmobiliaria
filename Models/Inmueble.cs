namespace Inmobiliaria.Models
{
   public class Inmueble
{
    public int Id { get; set; }
    public string? Direccion { get; set; }
    public int Ambientes { get; set; }
    public decimal Superficie { get; set; }  // ✅ número
    public string? Estado { get; set; }      // "Disponible", "Ocupado"
    public decimal Precio { get; set; }     // si usas precio de alquiler
    public int IdPropietario { get; set; }
    public Propietario? Propietario { get; set; }
    public int IdTipo { get; set; }
    public TipoInmueble? Tipo { get; set; }
}

}
