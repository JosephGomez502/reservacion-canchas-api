using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class UsuarioPasswordDto
    {
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
