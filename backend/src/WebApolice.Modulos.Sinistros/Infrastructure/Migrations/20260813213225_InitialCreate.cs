using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Sinistros.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sinistro");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "sinistro_status",
                schema: "sinistro",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    finalizador = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_status_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sinistro",
                schema: "sinistro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    seguradora_id = table.Column<long>(type: "bigint", nullable: true),
                    status_id = table.Column<short>(type: "smallint", nullable: true),
                    numero_sinistro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    situacao_original = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    data_ocorrencia = table.Column<DateOnly>(type: "date", nullable: true),
                    data_aviso = table.Column<DateOnly>(type: "date", nullable: true),
                    data_envio_seguradora = table.Column<DateOnly>(type: "date", nullable: true),
                    data_encerramento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_protocolo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_carta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_relacao_familia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_regulacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_avisado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_importancia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_auxilio_funeral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_cesta_basica = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_indenizacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_plano_legado_id = table.Column<int>(type: "integer", nullable: true),
                    cpf_sinistrado_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_sinistrado_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_sinistrado_valido = table.Column<bool>(type: "boolean", nullable: false),
                    causa = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sinistro_status_id_fkey",
                        column: x => x.status_id,
                        principalSchema: "sinistro",
                        principalTable: "sinistro_status",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "acompanhamento",
                schema: "sinistro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: true),
                    data_acompanhamento = table.Column<DateOnly>(type: "date", nullable: true),
                    hora_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contato = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("acompanhamento_pkey", x => x.id);
                    table.ForeignKey(
                        name: "acompanhamento_sinistro_id_fkey",
                        column: x => x.sinistro_id,
                        principalSchema: "sinistro",
                        principalTable: "sinistro",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sinistro_beneficiario",
                schema: "sinistro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: false),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    proposta_beneficiario_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cpf_original = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    parentesco_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    percentual_participacao = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_beneficiario_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sinistro_beneficiario_sinistro_id_fkey",
                        column: x => x.sinistro_id,
                        principalSchema: "sinistro",
                        principalTable: "sinistro",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sinistro_cobertura",
                schema: "sinistro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: false),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    proposta_cobertura_id = table.Column<long>(type: "bigint", nullable: true),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: true),
                    valor_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    cobertura_sinistro_legado_id = table.Column<int>(type: "integer", nullable: true),
                    premio_titular = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_conjuge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_cobertura_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sinistro_cobertura_sinistro_id_fkey",
                        column: x => x.sinistro_id,
                        principalSchema: "sinistro",
                        principalTable: "sinistro",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_acompanhamento_data",
                schema: "sinistro",
                table: "acompanhamento",
                column: "data_acompanhamento");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_acompanhamento_sinistro",
                schema: "sinistro",
                table: "acompanhamento",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ux_sinistro_acompanhamento_legado",
                schema: "sinistro",
                table: "acompanhamento",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_cliente",
                schema: "sinistro",
                table: "sinistro",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_cpf_sinistrado",
                schema: "sinistro",
                table: "sinistro",
                column: "cpf_sinistrado_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_data_aviso",
                schema: "sinistro",
                table: "sinistro",
                column: "data_aviso");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_data_ocorrencia",
                schema: "sinistro",
                table: "sinistro",
                column: "data_ocorrencia");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_estipulante",
                schema: "sinistro",
                table: "sinistro",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_proposta",
                schema: "sinistro",
                table: "sinistro",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_status",
                schema: "sinistro",
                table: "sinistro",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_vinculo",
                schema: "sinistro",
                table: "sinistro",
                column: "cliente_vinculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_sinistro_legado",
                schema: "sinistro",
                table: "sinistro",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_beneficiario_cpf",
                schema: "sinistro",
                table: "sinistro_beneficiario",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_beneficiario_proposta_beneficiario",
                schema: "sinistro",
                table: "sinistro_beneficiario",
                column: "proposta_beneficiario_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_beneficiario_sinistro",
                schema: "sinistro",
                table: "sinistro_beneficiario",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ux_sinistro_beneficiario_legado",
                schema: "sinistro",
                table: "sinistro_beneficiario",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_cobertura_cobertura",
                schema: "sinistro",
                table: "sinistro_cobertura",
                column: "cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_cobertura_proposta_cobertura",
                schema: "sinistro",
                table: "sinistro_cobertura",
                column: "proposta_cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_cobertura_sinistro",
                schema: "sinistro",
                table: "sinistro_cobertura",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ux_sinistro_cobertura_cobertura_legado",
                schema: "sinistro",
                table: "sinistro_cobertura",
                column: "cobertura_sinistro_legado_id",
                unique: true,
                filter: "(cobertura_sinistro_legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_sinistro_cobertura_legado",
                schema: "sinistro",
                table: "sinistro_cobertura",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "sinistro_status_codigo_key",
                schema: "sinistro",
                table: "sinistro_status",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acompanhamento",
                schema: "sinistro");

            migrationBuilder.DropTable(
                name: "sinistro_beneficiario",
                schema: "sinistro");

            migrationBuilder.DropTable(
                name: "sinistro_cobertura",
                schema: "sinistro");

            migrationBuilder.DropTable(
                name: "sinistro",
                schema: "sinistro");

            migrationBuilder.DropTable(
                name: "sinistro_status",
                schema: "sinistro");
        }
    }
}
