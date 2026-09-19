using Canchas.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EstadosReservacionController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public EstadosReservacionController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var estados = await _context.EstadosReservacion
                .AsNoTracking()
                .Where(e => e.Activo)
                .OrderBy(e => e.IdEstadoReservacion)
                .ToListAsync();

            return Ok(estados);
        }
    }
}