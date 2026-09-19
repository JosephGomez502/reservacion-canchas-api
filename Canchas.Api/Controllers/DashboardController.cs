using Canchas.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public DashboardController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen(
            [FromQuery] DateTime? fecha)
        {
            var fechaConsulta =
                (fecha ?? DateTime.Today).Date;

            var fechaFin =
                fechaConsulta.AddDays(1);

            var reservaciones =
                _context.Reservaciones
                    .AsNoTracking()
                    .Where(r =>
                        r.FechaInicio >= fechaConsulta &&
                        r.FechaInicio < fechaFin);

            var reservadas =
                await reservaciones
                    .CountAsync(r =>
                        r.EstadoReservacion.Nombre ==
                        "RESERVADA");

            var utilizadas =
                await reservaciones
                    .CountAsync(r =>
                        r.EstadoReservacion.Nombre ==
                        "UTILIZADA");

            var canceladas =
                await reservaciones
                    .CountAsync(r =>
                        r.EstadoReservacion.Nombre ==
                        "CANCELADA");

            var ingresos =
                await reservaciones
                    .Where(r =>
                        r.EstadoReservacion.Nombre !=
                        "CANCELADA")
                    .SumAsync(
                        r => (decimal?)r.Total
                    ) ?? 0;

            return Ok(
                new
                {
                    Fecha = fechaConsulta,

                    CanchasActivas =
                        await _context.Canchas
                            .CountAsync(
                                c => c.Activo
                            ),

                    Reservadas = reservadas,
                    Utilizadas = utilizadas,
                    Canceladas = canceladas,

                    TotalReservaciones =
                        reservadas +
                        utilizadas +
                        canceladas,

                    IngresoProgramado =
                        ingresos
                }
            );
        }
    }
}