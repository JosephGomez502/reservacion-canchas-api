using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class UsuarioCrearDto
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "EMPLEADO";
    }
}
