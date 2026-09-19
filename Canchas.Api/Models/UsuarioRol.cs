namespace Canchas.Api.Models;

public class UsuarioRol
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}