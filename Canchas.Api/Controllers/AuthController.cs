using Canchas.Api.Data;
using Canchas.Api.DTOs;
using Canchas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Canchas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CanchasDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(
            CanchasDbContext context,
            TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var correo = request.Correo
                .Trim()
                .ToLowerInvariant();

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(
                    u => u.Correo == correo
                );

            if (usuario == null || !usuario.Activo)
            {
                return Unauthorized(
                    new
                    {
                        mensaje = "Credenciales inválidas."
                    }
                );
            }

            if (
                usuario.BloqueadoHasta.HasValue &&
                usuario.BloqueadoHasta.Value > DateTime.UtcNow
            )
            {
                return StatusCode(
                    423,
                    new
                    {
                        mensaje = "Usuario bloqueado temporalmente.",
                        usuario.BloqueadoHasta
                    }
                );
            }

            if (
                usuario.BloqueadoHasta.HasValue &&
                usuario.BloqueadoHasta.Value <= DateTime.UtcNow
            )
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
            }

            if (
                !BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    usuario.PasswordHash
                )
            )
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 5)
                {
                    usuario.BloqueadoHasta =
                        DateTime.UtcNow.AddMinutes(15);

                    await _context.SaveChangesAsync();

                    return StatusCode(
                        423,
                        new
                        {
                            mensaje =
                                "Usuario bloqueado durante 15 minutos por múltiples intentos fallidos.",

                            usuario.BloqueadoHasta
                        }
                    );
                }

                await _context.SaveChangesAsync();

                return Unauthorized(
                    new
                    {
                        mensaje = "Credenciales inválidas.",

                        intentosRestantes =
                            5 - usuario.IntentosFallidos
                    }
                );
            }

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;

            await _context.SaveChangesAsync();

            var roles = usuario.UsuarioRoles
                .Where(ur => ur.Rol.Activo)
                .Select(ur => ur.Rol.Nombre)
                .Distinct()
                .ToList();

            var token =
                _tokenService.GenerarToken(
                    usuario,
                    roles
                );

            return Ok(
                new LoginResponseDto
                {
                    Token = token.Token,
                    ExpiraEn = token.ExpiraEn,
                    IdUsuario = usuario.IdUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Correo,
                    Roles = roles
                }
            );
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(claim, out var idUsuario))
            {
                return Unauthorized(
                    new
                    {
                        mensaje = "Token inválido."
                    }
                );
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(
                    u =>
                        u.IdUsuario == idUsuario &&
                        u.Activo
                );

            if (usuario == null)
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "Usuario inexistente o inactivo."
                    }
                );
            }

            return Ok(
                new
                {
                    usuario.IdUsuario,
                    usuario.NombreCompleto,
                    usuario.Correo,

                    Roles = usuario.UsuarioRoles
                        .Where(ur => ur.Rol.Activo)
                        .Select(ur => ur.Rol.Nombre)
                        .ToList()
                }
            );
        }
    }
}