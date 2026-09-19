namespace Canchas.Api.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int IdUsuarioCreacion { get; set; }

    public Usuario UsuarioCreacion { get; set; } = null!;
    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
}
