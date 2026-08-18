using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCadastroModulosGlobais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_vinculos",
                table: "vinculos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_status",
                table: "status");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dependentes",
                table: "dependentes");

            migrationBuilder.RenameTable(
                name: "vinculos",
                newName: "cliente_vinculo",
                newSchema: "cadastro");

            migrationBuilder.RenameTable(
                name: "status",
                newName: "cliente_status",
                newSchema: "cadastro");

            migrationBuilder.RenameTable(
                name: "dependentes",
                newName: "cliente_dependente",
                newSchema: "cadastro");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cliente_vinculo",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cliente_status",
                schema: "cadastro",
                table: "cliente_status",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cliente_dependente",
                schema: "cadastro",
                table: "cliente_dependente",
                column: "id");

            migrationBuilder.CreateTable(
                name: "modulo",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modulo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_modulo_ativo",
                schema: "cadastro",
                table: "modulo",
                column: "ativo",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_modulo_nome",
                schema: "cadastro",
                table: "modulo",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "ix_modulo_public_id",
                schema: "cadastro",
                table: "modulo",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modulo",
                schema: "cadastro");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cliente_vinculo",
                schema: "cadastro",
                table: "cliente_vinculo");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cliente_status",
                schema: "cadastro",
                table: "cliente_status");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cliente_dependente",
                schema: "cadastro",
                table: "cliente_dependente");

            migrationBuilder.RenameTable(
                name: "cliente_vinculo",
                schema: "cadastro",
                newName: "vinculos");

            migrationBuilder.RenameTable(
                name: "cliente_status",
                schema: "cadastro",
                newName: "status");

            migrationBuilder.RenameTable(
                name: "cliente_dependente",
                schema: "cadastro",
                newName: "dependentes");

            migrationBuilder.AddPrimaryKey(
                name: "pk_vinculos",
                table: "vinculos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_status",
                table: "status",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_dependentes",
                table: "dependentes",
                column: "id");
        }
    }
}
