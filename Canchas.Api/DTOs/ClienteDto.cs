using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class ClienteDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Documento { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }
    }
}
