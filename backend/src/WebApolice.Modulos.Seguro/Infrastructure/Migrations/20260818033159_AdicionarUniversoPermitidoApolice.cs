using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUniversoPermitidoApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apolice_produto",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    produto_id = table.Column<long>(type: "bigint", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_produto_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_apolice_produto_produto_produto_id",
                        column: x => x.produto_id,
                        principalSchema: "seguro",
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "apolice_plano",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_produto_id = table.Column<long>(type: "bigint", nullable: false),
                    plano_id = table.Column<long>(type: "bigint", nullable: false),
                    tabela_preco_id = table.Column<long>(type: "bigint", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_plano", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_plano_apolice_produto_apolice_produto_id",
                        column: x => x.apolice_produto_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_apolice_plano_plano_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "seguro",
                        principalTable: "plano",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_apolice_plano_tabela_preco_tabela_preco_id",
                        column: x => x.tabela_preco_id,
                        principalSchema: "seguro",
                        principalTable: "tabela_preco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "apolice_cobertura",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_plano_id = table.Column<long>(type: "bigint", nullable: false),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: false),
                    importancia_segurada_override = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_override = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_cobertura", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_cobertura_apolice_plano_apolice_plano_id",
                        column: x => x.apolice_plano_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_plano",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_apolice_cobertura_cobertura_cobertura_id",
                        column: x => x.cobertura_id,
                        principalSchema: "seguro",
                        principalTable: "cobertura",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_cobertura_apolice_plano_id",
                schema: "seguro",
                table: "apolice_cobertura",
                column: "apolice_plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_cobertura_cobertura_id",
                schema: "seguro",
                table: "apolice_cobertura",
                column: "cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_plano_apolice_produto_id",
                schema: "seguro",
                table: "apolice_plano",
                column: "apolice_produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_plano_plano_id",
                schema: "seguro",
                table: "apolice_plano",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_plano_tabela_preco_id",
                schema: "seguro",
                table: "apolice_plano",
                column: "tabela_preco_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_produto_apolice_id",
                schema: "seguro",
                table: "apolice_produto",
                column: "apolice_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_produto_produto_id",
                schema: "seguro",
                table: "apolice_produto",
                column: "produto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apolice_cobertura",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_plano",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_produto",
                schema: "seguro");
        }
    }
}
