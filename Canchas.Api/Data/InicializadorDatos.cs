using Canchas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.Data
{
    public static class InicializadorDatos
    {
        public static async Task InicializarAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<CanchasDbContext>();

            // =====================================================
            // CREAR ROLES
            // =====================================================

            var rolAdministrador =
                await context.Roles
                    .FirstOrDefaultAsync(
                        r => r.Nombre == "ADMINISTRADOR"
                    );

            if (rolAdministrador == null)
            {
                rolAdministrador = new Rol
                {
                    Nombre = "ADMINISTRADOR",
                    Descripcion = "Acceso completo al sistema",
                    Activo = true
                };

                context.Roles.Add(rolAdministrador);
            }

            var rolEmpleado =
                await context.Roles
                    .FirstOrDefaultAsync(
                        r => r.Nombre == "EMPLEADO"
                    );

            if (rolEmpleado == null)
            {
                rolEmpleado = new Rol
                {
                    Nombre = "EMPLEADO",
                    Descripcion = "Usuario operativo del sistema",
                    Activo = true
                };

                context.Roles.Add(rolEmpleado);
            }

            await context.SaveChangesAsync();

            // =====================================================
            // CREAR ESTADOS DE RESERVACIÓN
            // =====================================================

            var estados = new[]
            {
                new EstadoReservacion
                {
                    Nombre = "RESERVADA",
                    Descripcion = "Reservación registrada",
                    Activo = true
                },

                new EstadoReservacion
                {
                    Nombre = "UTILIZADA",
                    Descripcion = "Reservación utilizada",
                    Activo = true
                },

                new EstadoReservacion
                {
                    Nombre = "CANCELADA",
                    Descripcion = "Reservación cancelada",
                    Activo = true
                }
            };

            foreach (var estado in estados)
            {
                var existe =
                    await context.EstadosReservacion
                        .AnyAsync(
                            e => e.Nombre == estado.Nombre
                        );

                if (!existe)
                {
                    context.EstadosReservacion.Add(estado);
                }
            }

            await context.SaveChangesAsync();

            // =====================================================
            // DATOS DEL ADMINISTRADOR DESDE CONFIGURACIÓN SEGURA
            // =====================================================

            var nombreAdministrador =
                configuration["SeedAdmin:NombreCompleto"]
                ?? throw new InvalidOperationException(
                    "No se encontró SeedAdmin:NombreCompleto."
                );

            var correoAdministrador =
                configuration["SeedAdmin:Correo"]
                ?? throw new InvalidOperationException(
                    "No se encontró SeedAdmin:Correo."
                );

            var passwordAdministrador =
                configuration["SeedAdmin:Password"]
                ?? throw new InvalidOperationException(
                    "No se encontró SeedAdmin:Password."
                );

            correoAdministrador =
                correoAdministrador.Trim().ToLowerInvariant();

            // =====================================================
            // CREAR USUARIO ADMINISTRADOR
            // =====================================================

            var usuarioAdministrador =
                await context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Correo == correoAdministrador
                    );

            if (usuarioAdministrador == null)
            {
                usuarioAdministrador = new Usuario
                {
                    NombreCompleto = nombreAdministrador,
                    Correo = correoAdministrador,

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            passwordAdministrador
                        ),

                    Activo = true,
                    IntentosFallidos = 0,
                    BloqueadoHasta = null,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Usuarios.Add(
                    usuarioAdministrador
                );

                await context.SaveChangesAsync();
            }

            // =====================================================
            // ASIGNAR ROL ADMINISTRADOR
            // =====================================================

            var tieneRolAdministrador =
                await context.UsuarioRoles
                    .AnyAsync(
                        ur =>
                            ur.IdUsuario ==
                            usuarioAdministrador.IdUsuario
                            &&
                            ur.IdRol ==
                            rolAdministrador.IdRol
                    );

            if (!tieneRolAdministrador)
            {
                context.UsuarioRoles.Add(
                    new UsuarioRol
                    {
                        IdUsuario =
                            usuarioAdministrador.IdUsuario,

                        IdRol =
                            rolAdministrador.IdRol,

                        FechaAsignacion =
                            DateTime.UtcNow
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
