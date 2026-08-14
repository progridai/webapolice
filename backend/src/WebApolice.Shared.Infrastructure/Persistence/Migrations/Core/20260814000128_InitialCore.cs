using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Shared.Infrastructure.Persistence.Migrations.Core
{
    /// <inheritdoc />
    public partial class InitialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "banco",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("banco_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estado",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("estado_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoa",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tipo_pessoa = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome_normalizado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    documento_principal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    documento_principal_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    documento_valido = table.Column<bool>(type: "boolean", nullable: false),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    sexo = table.Column<short>(type: "smallint", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pessoa_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cidade",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estado_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome_normalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cidade_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cidade_estado_id_fkey",
                        column: x => x.estado_id,
                        principalSchema: "core",
                        principalTable: "estado",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pessoa_contato",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_contato = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_normalizado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pessoa_contato_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pessoa_contato_pessoa_id_fkey",
                        column: x => x.pessoa_id,
                        principalSchema: "core",
                        principalTable: "pessoa",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pessoa_contato_institucional",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ramal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pessoa_contato_institucional_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pessoa_contato_institucional_pessoa_id_fkey",
                        column: x => x.pessoa_id,
                        principalSchema: "core",
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoa_documento",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    numero_limpo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    orgao_emissor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_emissao = table.Column<DateOnly>(type: "date", nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pessoa_documento_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pessoa_documento_pessoa_id_fkey",
                        column: x => x.pessoa_id,
                        principalSchema: "core",
                        principalTable: "pessoa",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pessoa_endereco",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    tipo_endereco = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValueSql: "'principal'::character varying"),
                    cep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    complemento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_situacao_endereco = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pessoa_endereco_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pessoa_endereco_cidade_id_fkey",
                        column: x => x.cidade_id,
                        principalSchema: "core",
                        principalTable: "cidade",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "pessoa_endereco_pessoa_id_fkey",
                        column: x => x.pessoa_id,
                        principalSchema: "core",
                        principalTable: "pessoa",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_banco_codigo",
                schema: "core",
                table: "banco",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "ix_banco_legado_id",
                schema: "core",
                table: "banco",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_cidade_estado_id",
                schema: "core",
                table: "cidade",
                column: "estado_id");

            migrationBuilder.CreateIndex(
                name: "ix_cidade_legado_id",
                schema: "core",
                table: "cidade",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_cidade_nome",
                schema: "core",
                table: "cidade",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_estado_uf",
                schema: "core",
                table: "estado",
                column: "uf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_documento_principal_limpo",
                schema: "core",
                table: "pessoa",
                column: "documento_principal_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_documento_valido",
                schema: "core",
                table: "pessoa",
                column: "documento_valido");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_nome",
                schema: "core",
                table: "pessoa",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_contato_pessoa_id",
                schema: "core",
                table: "pessoa_contato",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_contato_tipo_contato",
                schema: "core",
                table: "pessoa_contato",
                column: "tipo_contato");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_contato_institucional_pessoa_id",
                schema: "core",
                table: "pessoa_contato_institucional",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_documento_numero_limpo",
                schema: "core",
                table: "pessoa_documento",
                column: "numero_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_documento_pessoa_id",
                schema: "core",
                table: "pessoa_documento",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_endereco_cidade_id",
                schema: "core",
                table: "pessoa_endereco",
                column: "cidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoa_endereco_pessoa_id",
                schema: "core",
                table: "pessoa_endereco",
                column: "pessoa_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banco",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pessoa_contato",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pessoa_contato_institucional",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pessoa_documento",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pessoa_endereco",
                schema: "core");

            migrationBuilder.DropTable(
                name: "cidade",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pessoa",
                schema: "core");

            migrationBuilder.DropTable(
                name: "estado",
                schema: "core");
        }
    }
}
