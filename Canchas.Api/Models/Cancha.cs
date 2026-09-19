namespace Canchas.Api.Models;

public class Cancha
{
    public int IdCancha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal PrecioHora { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int IdUsuarioCreacion { get; set; }

    public Usuario UsuarioCreacion { get; set; } = null!;
    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
}