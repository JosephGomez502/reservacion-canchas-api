using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class UsuarioActualizarDto
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        public bool Activo { get; set; }

        [Required]
        public string Rol { get; set; } = string.Empty;
    }
}
