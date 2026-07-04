using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clientes");

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "clientes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_cadastro_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    codigo_legado = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                    table.CheckConstraint("ck_clientes_status", "status IN ('Ativo', 'Inativo')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_clientes_codigo_legado",
                schema: "clientes",
                table: "clientes",
                column: "codigo_legado",
                unique: true,
                filter: "codigo_legado IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_nome",
                schema: "clientes",
                table: "clientes",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "uk_clientes_cpf",
                schema: "clientes",
                table: "clientes",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes",
                schema: "clientes");
        }
    }
}
