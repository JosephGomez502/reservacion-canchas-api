using Canchas.Api.Data;
using Canchas.Api.DTOs;
using Canchas.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Canchas.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservacionesController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public ReservacionesController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? fecha,
            [FromQuery] int? idCancha,
            [FromQuery] string? estado)
        {
            var consulta = _context.Reservaciones
                .AsNoTracking()
                .Include(r => r.Cliente)
                .Include(r => r.Cancha)
                .Include(r => r.EstadoReservacion)
                .Include(r => r.Usuario)
                .AsQueryable();

            if (fecha.HasValue)
            {
                var inicio = fecha.Value.Date;
                var fin = inicio.AddDays(1);

                consulta = consulta.Where(r =>
                    r.FechaInicio >= inicio &&
                    r.FechaInicio < fin);
            }

            if (idCancha.HasValue)
            {
                consulta = consulta.Where(r =>
                    r.IdCancha == idCancha.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var nombreEstado =
                    estado.Trim().ToUpperInvariant();

                consulta = consulta.Where(r =>
                    r.EstadoReservacion.Nombre ==
                    nombreEstado);
            }

            var resultado = await consulta
                .OrderBy(r => r.FechaInicio)
                .Select(r => new
                {
                    r.IdReservacion,
                    r.FechaInicio,
                    r.FechaFin,
                    r.DuracionHoras,
                    r.PrecioHora,
                    r.Total,
                    r.Observaciones,
                    r.IdCliente,
                    Cliente = r.Cliente.Nombre,
                    r.IdCancha,
                    Cancha = r.Cancha.Nombre,
                    TipoCancha = r.Cancha.Tipo,
                    Estado = r.EstadoReservacion.Nombre,
                    Usuario = r.Usuario.NombreCompleto
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservacion =
                await _context.Reservaciones
                    .AsNoTracking()
                    .Where(r =>
                        r.IdReservacion == id)
                    .Select(r => new
                    {
                        r.IdReservacion,
                        r.IdCliente,
                        Cliente = r.Cliente.Nombre,
                        r.IdCancha,
                        Cancha = r.Cancha.Nombre,
                        TipoCancha = r.Cancha.Tipo,
                        Estado =
                            r.EstadoReservacion.Nombre,
                        r.FechaInicio,
                        r.FechaFin,
                        r.DuracionHoras,
                        r.PrecioHora,
                        r.Total,
                        r.Observaciones,
                        Usuario =
                            r.Usuario.NombreCompleto,
                        r.FechaCreacion
                    })
                    .FirstOrDefaultAsync();

            if (reservacion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Reservación no encontrada."
                    }
                );
            }

            return Ok(reservacion);
        }

        [HttpGet("disponibilidad")]
        public async Task<IActionResult> Disponibilidad(
            int idCancha,
            DateTime inicio,
            DateTime fin)
        {
            if (inicio >= fin)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La fecha y hora final deben ser mayores a la inicial."
                    }
                );
            }

            var cancha =
                await _context.Canchas
                    .FindAsync(idCancha);

            if (cancha == null || !cancha.Activo)
            {
                return Ok(
                    new
                    {
                        disponible = false,
                        mensaje =
                            "La cancha no existe o se encuentra inactiva."
                    }
                );
            }

            var estadoCancelado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "CANCELADA"
                    );

            var existeCruce =
                await _context.Reservaciones
                    .AnyAsync(r =>
                        r.IdCancha == idCancha &&
                        r.IdEstadoReservacion !=
                            estadoCancelado
                                .IdEstadoReservacion &&
                        inicio < r.FechaFin &&
                        fin > r.FechaInicio
                    );

            return Ok(
                new
                {
                    disponible = !existeCruce,

                    mensaje = existeCruce
                        ? "La cancha ya se encuentra ocupada dentro del horario seleccionado."
                        : "Horario disponible."
                }
            );
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            ReservacionDto dto)
        {
            if (dto.FechaInicio >= dto.FechaFin)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La fecha y hora final deben ser mayores a la inicial."
                    }
                );
            }

            var cliente =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == dto.IdCliente &&
                        c.Activo);

            if (cliente == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Cliente inexistente o inactivo."
                    }
                );
            }

            var cancha =
                await _context.Canchas
                    .FirstOrDefaultAsync(c =>
                        c.IdCancha == dto.IdCancha &&
                        c.Activo);

            if (cancha == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Cancha inexistente o inactiva."
                    }
                );
            }

            var estadoReservado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "RESERVADA"
                    );

            var estadoCancelado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "CANCELADA"
                    );

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable
                    );

            var existeCruce =
                await _context.Reservaciones
                    .AnyAsync(r =>
                        r.IdCancha == dto.IdCancha &&
                        r.IdEstadoReservacion !=
                            estadoCancelado
                                .IdEstadoReservacion &&
                        dto.FechaInicio < r.FechaFin &&
                        dto.FechaFin > r.FechaInicio
                    );

            if (existeCruce)
            {
                await transaccion.RollbackAsync();

                return Conflict(
                    new
                    {
                        mensaje =
                            "La cancha ya se encuentra ocupada dentro del horario seleccionado."
                    }
                );
            }

            var duracionHoras =
                Math.Round(
                    (decimal)
                    (dto.FechaFin - dto.FechaInicio)
                        .TotalMinutes / 60m,
                    2
                );

            var idUsuario = int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                )!
            );

            var reservacion = new Reservacion
            {
                IdCliente = dto.IdCliente,
                IdCancha = dto.IdCancha,

                IdEstadoReservacion =
                    estadoReservado
                        .IdEstadoReservacion,

                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                DuracionHoras = duracionHoras,
                PrecioHora = cancha.PrecioHora,

                Total = Math.Round(
                    duracionHoras *
                    cancha.PrecioHora,
                    2
                ),

                Observaciones =
                    dto.Observaciones?.Trim(),

                IdUsuario = idUsuario,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Reservaciones.Add(reservacion);

            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Reservación registrada correctamente.",

                    reservacion.IdReservacion,
                    reservacion.DuracionHoras,
                    reservacion.PrecioHora,
                    reservacion.Total
                }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(
            int id,
            ReservacionDto dto)
        {
            if (dto.FechaInicio >= dto.FechaFin)
            {
                return BadRequest(
                    new
                    {
                        mensaje = "Horario inválido."
                    }
                );
            }

            var reservacion =
                await _context.Reservaciones
                    .Include(
                        r => r.EstadoReservacion
                    )
                    .FirstOrDefaultAsync(
                        r => r.IdReservacion == id
                    );

            if (reservacion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Reservación no encontrada."
                    }
                );
            }

            if (
                reservacion.EstadoReservacion.Nombre
                != "RESERVADA"
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Solo una reservación RESERVADA puede modificarse."
                    }
                );
            }

            var cliente =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == dto.IdCliente &&
                        c.Activo);

            if (cliente == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Cliente inexistente o inactivo."
                    }
                );
            }

            var cancha =
                await _context.Canchas
                    .FirstOrDefaultAsync(c =>
                        c.IdCancha == dto.IdCancha &&
                        c.Activo);

            if (cancha == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Cancha inexistente o inactiva."
                    }
                );
            }

            var estadoCancelado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "CANCELADA"
                    );

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable
                    );

            var existeCruce =
                await _context.Reservaciones
                    .AnyAsync(r =>
                        r.IdReservacion != id &&
                        r.IdCancha == dto.IdCancha &&
                        r.IdEstadoReservacion !=
                            estadoCancelado
                                .IdEstadoReservacion &&
                        dto.FechaInicio < r.FechaFin &&
                        dto.FechaFin > r.FechaInicio
                    );

            if (existeCruce)
            {
                await transaccion.RollbackAsync();

                return Conflict(
                    new
                    {
                        mensaje =
                            "La cancha ya se encuentra ocupada dentro del horario seleccionado."
                    }
                );
            }

            var duracionHoras =
                Math.Round(
                    (decimal)
                    (dto.FechaFin - dto.FechaInicio)
                        .TotalMinutes / 60m,
                    2
                );

            reservacion.IdCliente =
                dto.IdCliente;

            reservacion.IdCancha =
                dto.IdCancha;

            reservacion.FechaInicio =
                dto.FechaInicio;

            reservacion.FechaFin =
                dto.FechaFin;

            reservacion.DuracionHoras =
                duracionHoras;

            reservacion.PrecioHora =
                cancha.PrecioHora;

            reservacion.Total =
                Math.Round(
                    duracionHoras *
                    cancha.PrecioHora,
                    2
                );

            reservacion.Observaciones =
                dto.Observaciones?.Trim();

            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Reservación actualizada correctamente.",

                    reservacion.DuracionHoras,
                    reservacion.Total
                }
            );
        }

        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(
            int id)
        {
            var reservacion =
                await _context.Reservaciones
                    .Include(
                        r => r.EstadoReservacion
                    )
                    .FirstOrDefaultAsync(
                        r => r.IdReservacion == id
                    );

            if (reservacion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Reservación no encontrada."
                    }
                );
            }

            if (
                reservacion.EstadoReservacion.Nombre
                != "RESERVADA"
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Solo una reservación RESERVADA puede cancelarse."
                    }
                );
            }

            var estadoCancelado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "CANCELADA"
                    );

            reservacion.IdEstadoReservacion =
                estadoCancelado
                    .IdEstadoReservacion;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Reservación cancelada. El horario quedó liberado."
                }
            );
        }

        [HttpPut("{id:int}/utilizar")]
        public async Task<IActionResult> Utilizar(
            int id)
        {
            var reservacion =
                await _context.Reservaciones
                    .Include(
                        r => r.EstadoReservacion
                    )
                    .FirstOrDefaultAsync(
                        r => r.IdReservacion == id
                    );

            if (reservacion == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "Reservación no encontrada."
                    }
                );
            }

            if (
                reservacion.EstadoReservacion.Nombre
                != "RESERVADA"
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Solo una reservación RESERVADA puede marcarse como utilizada."
                    }
                );
            }

            var estadoUtilizado =
                await _context.EstadosReservacion
                    .SingleAsync(
                        e => e.Nombre == "UTILIZADA"
                    );

            reservacion.IdEstadoReservacion =
                estadoUtilizado
                    .IdEstadoReservacion;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Reservación marcada como utilizada."
                }
            );
        }
    }
}