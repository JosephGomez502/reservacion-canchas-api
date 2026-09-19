namespace Canchas.Api.Models;

public class EstadoReservacion
{
    public int IdEstadoReservacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
}