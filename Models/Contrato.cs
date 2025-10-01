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
    public DateTime FechaFinOriginal { get; set; }
    public DateTime? FechaTerminacionAnticipada { get; set; }
    public decimal MontoMensual { get; set; }
    public decimal? MultaCalculada { get; set; }
    public string EstadoContrato { get; set; } = "Vigente";

    public Inmueble? Inmueble { get; set; }
    public Inquilino? Inquilino { get; set; }
    public Usuario? UsuarioCreador { get; set; }
    public Usuario? UsuarioFinalizador { get; set; }

    // <-- Agregar esta propiedad
    public string DescripcionContrato
    {
        get
        {
            return $"Contrato {Id} - Inmueble: {Inmueble?.Direccion ?? "Sin info"} - " +
                   $"Inquilino: {Inquilino?.Nombre ?? "Sin info"} - " +
                   $"{FechaInicio:dd/MM/yyyy} a {FechaFin:dd/MM/yyyy}";
        }
    }
}
}
