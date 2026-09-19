using Canchas.Api.Data;
using Canchas.Api.DTOs;
using Canchas.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly CanchasDbContext _context;

        public UsuariosController(CanchasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.NombreCompleto,
                    u.Correo,
                    u.Activo,

                    Roles = u.UsuarioRoles
                        .Select(ur => ur.Rol.Nombre)
                        .ToList()
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPost]
        public async Task<IActionResult> Post(UsuarioCrearDto dto)
        {
            var correo =
                dto.Correo.Trim().ToLowerInvariant();

            var nombreRol =
                dto.Rol.Trim().ToUpperInvariant();

            if (
                await _context.Usuarios
                    .AnyAsync(u => u.Correo == correo)
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje = "El correo ya está registrado."
                    }
                );
            }

            var rol = await _context.Roles
                .FirstOrDefaultAsync(
                    r =>
                        r.Nombre == nombreRol &&
                        r.Activo
                );

            if (rol == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El rol indicado no existe o está inactivo."
                    }
                );
            }

            var usuario = new Usuario
            {
                NombreCompleto =
                    dto.NombreCompleto.Trim(),

                Correo = correo,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password
                    ),

                Activo = true,
                IntentosFallidos = 0,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            _context.UsuarioRoles.Add(
                new UsuarioRol
                {
                    IdUsuario = usuario.IdUsuario,
                    IdRol = rol.IdRol,
                    FechaAsignacion = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Usuario creado correctamente.",

                    usuario.IdUsuario
                }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(
            int id,
            UsuarioActualizarDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == id
                );

            if (usuario == null)
            {
                return NotFound(
                    new
                    {
                        mensaje = "Usuario no encontrado."
                    }
                );
            }

            var nombreRol =
                dto.Rol.Trim().ToUpperInvariant();

            var rol = await _context.Roles
                .FirstOrDefaultAsync(
                    r =>
                        r.Nombre == nombreRol &&
                        r.Activo
                );

            if (rol == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El rol indicado no existe."
                    }
                );
            }

            usuario.NombreCompleto =
                dto.NombreCompleto.Trim();

            usuario.Activo =
                dto.Activo;

            var tieneRol =
                usuario.UsuarioRoles
                    .Any(
                        ur => ur.IdRol == rol.IdRol
                    );

            if (!tieneRol)
            {
                _context.UsuarioRoles
                    .RemoveRange(
                        usuario.UsuarioRoles
                    );

                await _context.SaveChangesAsync();

                _context.UsuarioRoles.Add(
                    new UsuarioRol
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdRol = rol.IdRol,
                        FechaAsignacion = DateTime.UtcNow
                    }
                );
            }

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Usuario actualizado correctamente."
                }
            );
        }

        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> CambiarPassword(
            int id,
            UsuarioPasswordDto dto)
        {
            var usuario =
                await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(
                    new
                    {
                        mensaje = "Usuario no encontrado."
                    }
                );
            }

            usuario.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.Password
                );

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    mensaje =
                        "Contraseña actualizada correctamente."
                }
            );
        }
    }
}