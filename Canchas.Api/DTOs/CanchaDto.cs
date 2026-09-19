using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class CanchaDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Range(0.01, 999999)]
        public decimal PrecioHora { get; set; }
    }
}