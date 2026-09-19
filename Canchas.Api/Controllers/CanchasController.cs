using Canchas.Api.Data;
using Canchas.Api.DTOs;
using Canchas.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Canchas.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public CanchasController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] bool? activo)
        {
            var consulta =
                _context.Canchas
                    .AsNoTracking()
                    .AsQueryable();

            if (activo.HasValue)
            {
                consulta =
                    consulta.Where(
                        c => c.Activo == activo.Value
                    );
            }

            return Ok(
                await consulta
                    .OrderBy(c => c.Nombre)
                    .ToListAsync()
            );
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            CanchaDto dto)
        {
            var nombre = dto.Nombre.Trim();

            if (
                await _context.Canchas
                    .AnyAsync(c => c.Nombre == nombre)
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ya existe una cancha con ese nombre."
                    }
                );
            }

            var idUsuario = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                )!
            );

            var cancha = new Cancha
            {
                Nombre = nombre,
                Tipo = dto.Tipo.Trim(),
                PrecioHora = dto.PrecioHora,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                IdUsuarioCreacion = idUsuario
            };

            _context.Canchas.Add(cancha);

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Cancha creada correctamente.",

                    cancha.IdCancha
                }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(
            int id,
            CanchaDto dto)
        {
            var cancha =
                await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Cancha no encontrada."
                    }
                );
            }

            var nombre = dto.Nombre.Trim();

            if (
                await _context.Canchas.AnyAsync(c =>
                    c.Nombre == nombre &&
                    c.IdCancha != id)
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ya existe una cancha con ese nombre."
                    }
                );
            }

            cancha.Nombre = nombre;
            cancha.Tipo = dto.Tipo.Trim();
            cancha.PrecioHora = dto.PrecioHora;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Cancha actualizada correctamente."
                }
            );
        }

        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> Estado(
            int id,
            EstadoDto dto)
        {
            var cancha =
                await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Cancha no encontrada."
                    }
                );
            }

            cancha.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Estado de la cancha actualizado."
                }
            );
        }
    }
}