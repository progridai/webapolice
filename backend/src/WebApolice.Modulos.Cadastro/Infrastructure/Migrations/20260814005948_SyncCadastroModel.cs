using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCadastroModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "agenciador_coordenador_id_fkey",
                schema: "cadastro",
                table: "agenciador");

            migrationBuilder.DropForeignKey(
                name: "cliente_status_id_fkey",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.DropForeignKey(
                name: "estipulante_grupo_id_fkey",
                schema: "cadastro",
                table: "estipulante");

            migrationBuilder.DropForeignKey(
                name: "estipulante_seguradora_id_fkey",
                schema: "cadastro",
                table: "estipulante");

            migrationBuilder.DropForeignKey(
                name: "estipulante_configuracao_cancela_estipulante_id_fkey",
                schema: "cadastro",
                table: "estipulante_configuracao");

            migrationBuilder.DropForeignKey(
                name: "estipulante_configuracao_estipulante_id_fkey",
                schema: "cadastro",
                table: "estipulante_configuracao");

            migrationBuilder.DropForeignKey(
                name: "subestipulante_estipulante_id_fkey",
                schema: "cadastro",
                table: "subestipulante");

            migrationBuilder.DropForeignKey(
                name: "subgrupo_grupo_id_fkey",
                schema: "cadastro",
                table: "subgrupo");

            migrationBuilder.DropTable(
                name: "cliente_dependente",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente_status",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente_vinculo",
                schema: "cadastro");

            migrationBuilder.DropPrimaryKey(
                name: "cliente_pkey",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_pessoa",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_status",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ux_cliente_legado",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "cadastro",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<Guid>(
                name: "public_id",
                schema: "cadastro",
                table: "cliente",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "cadastro",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cliente",
                schema: "cadastro",
                table: "cliente",
                column: "id");

            migrationBuilder.CreateTable(
                name: "dependentes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    tipo_relacao = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    cpf = table.Column<string>(type: "text", nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dependentes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vinculos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    grupo_id = table.Column<long>(type: "bigint", nullable: true),
                    subgrupo_id = table.Column<long>(type: "bigint", nullable: true),
                    lotacao_id = table.Column<long>(type: "bigint", nullable: true),
                    matricula = table.Column<string>(type: "text", nullable: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    agencia = table.Column<string>(type: "text", nullable: true),
                    conta_corrente = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vinculos", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dependentes");

            migrationBuilder.DropTable(
                name: "status");

            migrationBuilder.DropTable(
                name: "vinculos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cliente",
                schema: "cadastro",
                table: "cliente");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "cadastro",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "public_id",
                schema: "cadastro",
                table: "cliente",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "cadastro",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "cliente_pkey",
                schema: "cadastro",
                table: "cliente",
                column: "id");

            migrationBuilder.CreateTable(
                name: "cliente_dependente",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cpf = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    data_emissao_rg = table.Column<DateOnly>(type: "date", nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    legado_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    orgao_rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tipo_relacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_dependente_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cliente_dependente_cliente_id_fkey",
                        column: x => x.cliente_id,
                        principalSchema: "cadastro",
                        principalTable: "cliente",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cliente_status",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_status_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cliente_vinculo",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    grupo_id = table.Column<long>(type: "bigint", nullable: true),
                    lotacao_id = table.Column<long>(type: "bigint", nullable: true),
                    subgrupo_id = table.Column<long>(type: "bigint", nullable: true),
                    agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    conta_corrente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    criterio_criacao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    matricula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    matricula_normalizada = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_vinculo_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cliente_vinculo_cliente_id_fkey",
                        column: x => x.cliente_id,
                        principalSchema: "cadastro",
                        principalTable: "cliente",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_grupo_id_fkey",
                        column: x => x.grupo_id,
                        principalSchema: "cadastro",
                        principalTable: "grupo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_lotacao_id_fkey",
                        column: x => x.lotacao_id,
                        principalSchema: "cadastro",
                        principalTable: "lotacao",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_subgrupo_id_fkey",
                        column: x => x.subgrupo_id,
                        principalSchema: "cadastro",
                        principalTable: "subgrupo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cliente_vinculo_estipulante",
                        column: x => x.estipulante_id,
                        principalSchema: "cadastro",
                        principalTable: "estipulante",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_pessoa",
                schema: "cadastro",
                table: "cliente",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_status",
                schema: "cadastro",
                table: "cliente",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ux_cliente_legado",
                schema: "cadastro",
                table: "cliente",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_dependente_cliente",
                schema: "cadastro",
                table: "cliente_dependente",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "cliente_status_codigo_key",
                schema: "cadastro",
                table: "cliente_status",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_cliente",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_estipulante",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_grupo_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_lotacao_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "lotacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_pessoa",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_pessoa_estip_matricula",
                schema: "cadastro",
                table: "cliente_vinculo",
                columns: new[] { "pessoa_id", "estipulante_id", "matricula_normalizada" });

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_subgrupo_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "subgrupo_id");

            migrationBuilder.CreateIndex(
                name: "ux_cliente_vinculo_legado_cliente",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "legado_cliente_id",
                unique: true,
                filter: "(legado_cliente_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "agenciador_coordenador_id_fkey",
                schema: "cadastro",
                table: "agenciador",
                column: "coordenador_id",
                principalSchema: "cadastro",
                principalTable: "agenciador",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "cliente_status_id_fkey",
                schema: "cadastro",
                table: "cliente",
                column: "status_id",
                principalSchema: "cadastro",
                principalTable: "cliente_status",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "estipulante_grupo_id_fkey",
                schema: "cadastro",
                table: "estipulante",
                column: "grupo_id",
                principalSchema: "cadastro",
                principalTable: "grupo",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "estipulante_seguradora_id_fkey",
                schema: "cadastro",
                table: "estipulante",
                column: "seguradora_id",
                principalSchema: "cadastro",
                principalTable: "seguradora",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "estipulante_configuracao_cancela_estipulante_id_fkey",
                schema: "cadastro",
                table: "estipulante_configuracao",
                column: "cancela_estipulante_id",
                principalSchema: "cadastro",
                principalTable: "estipulante",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "estipulante_configuracao_estipulante_id_fkey",
                schema: "cadastro",
                table: "estipulante_configuracao",
                column: "estipulante_id",
                principalSchema: "cadastro",
                principalTable: "estipulante",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "subestipulante_estipulante_id_fkey",
                schema: "cadastro",
                table: "subestipulante",
                column: "estipulante_id",
                principalSchema: "cadastro",
                principalTable: "estipulante",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "subgrupo_grupo_id_fkey",
                schema: "cadastro",
                table: "subgrupo",
                column: "grupo_id",
                principalSchema: "cadastro",
                principalTable: "grupo",
                principalColumn: "id");
        }
    }
}
