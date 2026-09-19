using Canchas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.Data;

public class CanchasDbContext : DbContext
{
    public CanchasDbContext(DbContextOptions<CanchasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Cancha> Canchas => Set<Cancha>();
    public DbSet<EstadoReservacion> EstadosReservacion => Set<EstadoReservacion>();
    public DbSet<Reservacion> Reservaciones => Set<Reservacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("TB_USUARIOS");
            e.HasKey(x => x.IdUsuario);

            e.Property(x => x.NombreCompleto)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.Correo)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            e.HasIndex(x => x.Correo)
                .IsUnique();
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("TB_ROLES");
            e.HasKey(x => x.IdRol);

            e.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            e.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        modelBuilder.Entity<UsuarioRol>(e =>
        {
            e.ToTable("TB_USUARIO_ROLES");

            e.HasKey(x => new
            {
                x.IdUsuario,
                x.IdRol
            });

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Rol)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("TB_CLIENTES");
            e.HasKey(x => x.IdCliente);

            e.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.Documento)
                .HasMaxLength(30);

            e.Property(x => x.Telefono)
                .HasMaxLength(20);

            e.Property(x => x.Email)
                .HasMaxLength(150);

            e.HasIndex(x => x.Documento)
                .IsUnique()
                .HasFilter("[Documento] IS NOT NULL AND [Documento] <> ''");

            e.HasOne(x => x.UsuarioCreacion)
                .WithMany()
                .HasForeignKey(x => x.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cancha>(e =>
        {
            e.ToTable("TB_CANCHAS");
            e.HasKey(x => x.IdCancha);

            e.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Tipo)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.PrecioHora)
                .HasPrecision(10, 2);

            e.HasIndex(x => x.Nombre)
                .IsUnique();

            e.HasOne(x => x.UsuarioCreacion)
                .WithMany()
                .HasForeignKey(x => x.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EstadoReservacion>(e =>
        {
            e.ToTable("TB_ESTADOS_RESERVACION");
            e.HasKey(x => x.IdEstadoReservacion);

            e.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            e.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        modelBuilder.Entity<Reservacion>(e =>
        {
            e.ToTable("TB_RESERVACIONES");
            e.HasKey(x => x.IdReservacion);

            e.Property(x => x.DuracionHoras)
                .HasPrecision(10, 2);

            e.Property(x => x.PrecioHora)
                .HasPrecision(10, 2);

            e.Property(x => x.Total)
                .HasPrecision(12, 2);

            e.Property(x => x.Observaciones)
                .HasMaxLength(500);

            e.HasIndex(x => new
            {
                x.IdCancha,
                x.FechaInicio,
                x.FechaFin
            });

            e.HasOne(x => x.Cliente)
                .WithMany(x => x.Reservaciones)
                .HasForeignKey(x => x.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Cancha)
                .WithMany(x => x.Reservaciones)
                .HasForeignKey(x => x.IdCancha)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EstadoReservacion)
                .WithMany(x => x.Reservaciones)
                .HasForeignKey(x => x.IdEstadoReservacion)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
