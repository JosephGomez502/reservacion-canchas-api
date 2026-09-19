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
    public class ClientesController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public ClientesController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? buscar)
        {
            var consulta =
                _context.Clientes
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(c =>
                    c.Nombre.Contains(buscar) ||
                    (c.Documento != null &&
                     c.Documento.Contains(buscar)) ||
                    (c.Telefono != null &&
                     c.Telefono.Contains(buscar))
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
            ClienteDto dto)
        {
            var documento =
                string.IsNullOrWhiteSpace(dto.Documento)
                    ? null
                    : dto.Documento.Trim();

            if (
                documento != null &&
                await _context.Clientes
                    .AnyAsync(
                        c => c.Documento == documento
                    )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El documento ya se encuentra registrado."
                    }
                );
            }

            var idUsuario = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                )!
            );

            var cliente = new Cliente
            {
                Nombre = dto.Nombre.Trim(),
                Documento = documento,
                Telefono = dto.Telefono?.Trim(),
                Email = dto.Email?.Trim(),
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                IdUsuarioCreacion = idUsuario
            };

            _context.Clientes.Add(cliente);

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Cliente creado correctamente.",

                    cliente.IdCliente
                }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(
            int id,
            ClienteDto dto)
        {
            var cliente =
                await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Cliente no encontrado."
                    }
                );
            }

            var documento =
                string.IsNullOrWhiteSpace(dto.Documento)
                    ? null
                    : dto.Documento.Trim();

            if (
                documento != null &&
                await _context.Clientes.AnyAsync(c =>
                    c.Documento == documento &&
                    c.IdCliente != id)
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El documento ya se encuentra registrado."
                    }
                );
            }

            cliente.Nombre =
                dto.Nombre.Trim();

            cliente.Documento =
                documento;

            cliente.Telefono =
                dto.Telefono?.Trim();

            cliente.Email =
                dto.Email?.Trim();

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Cliente actualizado correctamente."
                }
            );
        }

        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> Estado(
            int id,
            EstadoDto dto)
        {
            var cliente =
                await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Cliente no encontrado."
                    }
                );
            }

            cliente.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Estado del cliente actualizado."
                }
            );
        }
    }
}