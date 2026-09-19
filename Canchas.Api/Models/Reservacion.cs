namespace Canchas.Api.Models;

public class Reservacion
{
    public int IdReservacion { get; set; }
    public int IdCliente { get; set; }
    public int IdCancha { get; set; }
    public int IdEstadoReservacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal DuracionHoras { get; set; }
    public decimal PrecioHora { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; } = null!;
    public Cancha Cancha { get; set; } = null!;
    public EstadoReservacion EstadoReservacion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
