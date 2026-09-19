using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canchas.Api.Migrations
{
    /// <inheritdoc />
    public partial class InicialReservacionCanchas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_ESTADOS_RESERVACION",
                columns: table => new
                {
                    IdEstadoReservacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESTADOS_RESERVACION", x => x.IdEstadoReservacion);
                });

            migrationBuilder.CreateTable(
                name: "TB_ROLES",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ROLES", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "TB_USUARIOS",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIOS", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "TB_CANCHAS",
                columns: table => new
                {
                    IdCancha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrecioHora = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CANCHAS", x => x.IdCancha);
                    table.ForeignKey(
                        name: "FK_TB_CANCHAS_TB_USUARIOS_IdUsuarioCreacion",
                        column: x => x.IdUsuarioCreacion,
                        principalTable: "TB_USUARIOS",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLIENTES",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLIENTES", x => x.IdCliente);
                    table.ForeignKey(
                        name: "FK_TB_CLIENTES_TB_USUARIOS_IdUsuarioCreacion",
                        column: x => x.IdUsuarioCreacion,
                        principalTable: "TB_USUARIOS",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_USUARIO_ROLES",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO_ROLES", x => new { x.IdUsuario, x.IdRol });
                    table.ForeignKey(
                        name: "FK_TB_USUARIO_ROLES_TB_ROLES_IdRol",
                        column: x => x.IdRol,
                        principalTable: "TB_ROLES",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_USUARIO_ROLES_TB_USUARIOS_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "TB_USUARIOS",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_RESERVACIONES",
                columns: table => new
                {
                    IdReservacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdCancha = table.Column<int>(type: "int", nullable: false),
                    IdEstadoReservacion = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracionHoras = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PrecioHora = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_RESERVACIONES", x => x.IdReservacion);
                    table.ForeignKey(
                        name: "FK_TB_RESERVACIONES_TB_CANCHAS_IdCancha",
                        column: x => x.IdCancha,
                        principalTable: "TB_CANCHAS",
                        principalColumn: "IdCancha",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_RESERVACIONES_TB_CLIENTES_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "TB_CLIENTES",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_RESERVACIONES_TB_ESTADOS_RESERVACION_IdEstadoReservacion",
                        column: x => x.IdEstadoReservacion,
                        principalTable: "TB_ESTADOS_RESERVACION",
                        principalColumn: "IdEstadoReservacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_RESERVACIONES_TB_USUARIOS_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "TB_USUARIOS",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CANCHAS_IdUsuarioCreacion",
                table: "TB_CANCHAS",
                column: "IdUsuarioCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CANCHAS_Nombre",
                table: "TB_CANCHAS",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLIENTES_Documento",
                table: "TB_CLIENTES",
                column: "Documento",
                unique: true,
                filter: "[Documento] IS NOT NULL AND [Documento] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLIENTES_IdUsuarioCreacion",
                table: "TB_CLIENTES",
                column: "IdUsuarioCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ESTADOS_RESERVACION_Nombre",
                table: "TB_ESTADOS_RESERVACION",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_RESERVACIONES_IdCancha_FechaInicio_FechaFin",
                table: "TB_RESERVACIONES",
                columns: new[] { "IdCancha", "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_RESERVACIONES_IdCliente",
                table: "TB_RESERVACIONES",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_TB_RESERVACIONES_IdEstadoReservacion",
                table: "TB_RESERVACIONES",
                column: "IdEstadoReservacion");

            migrationBuilder.CreateIndex(
                name: "IX_TB_RESERVACIONES_IdUsuario",
                table: "TB_RESERVACIONES",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ROLES_Nombre",
                table: "TB_ROLES",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_ROLES_IdRol",
                table: "TB_USUARIO_ROLES",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIOS_Correo",
                table: "TB_USUARIOS",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_RESERVACIONES");

            migrationBuilder.DropTable(
                name: "TB_USUARIO_ROLES");

            migrationBuilder.DropTable(
                name: "TB_CANCHAS");

            migrationBuilder.DropTable(
                name: "TB_CLIENTES");

            migrationBuilder.DropTable(
                name: "TB_ESTADOS_RESERVACION");

            migrationBuilder.DropTable(
                name: "TB_ROLES");

            migrationBuilder.DropTable(
                name: "TB_USUARIOS");
        }
    }
}
