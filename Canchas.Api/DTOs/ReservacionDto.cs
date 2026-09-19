using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.DTOs
{
    public class ReservacionDto
    {
        [Range(1, int.MaxValue)]
        public int IdCliente { get; set; }

        [Range(1, int.MaxValue)]
        public int IdCancha { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}
