using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Documentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "documento");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "storage_provider",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_provider_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_anexo",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    categoria = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    exige_validade = table.Column<bool>(type: "boolean", nullable: false),
                    exige_assinatura = table.Column<bool>(type: "boolean", nullable: false),
                    sensivel = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_valor_original = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tipo_anexo_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "arquivo",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    storage_provider_id = table.Column<short>(type: "smallint", nullable: true),
                    bucket = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    storage_key = table.Column<string>(type: "text", nullable: true),
                    storage_path = table.Column<string>(type: "text", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nome_armazenado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    extensao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mime_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: true),
                    hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    data_documento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_original = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "'legado'::character varying"),
                    caminho_legado = table.Column<string>(type: "text", nullable: true),
                    arquivo_legado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'ativo'::character varying"),
                    criado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    criado_por_usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    extensao_original = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    extensao_normalizada = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    extensao_confiavel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    migracao_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'pendente'::character varying"),
                    migracao_erro = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("arquivo_pkey", x => x.id);
                    table.ForeignKey(
                        name: "arquivo_storage_provider_id_fkey",
                        column: x => x.storage_provider_id,
                        principalSchema: "documento",
                        principalTable: "storage_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "arquivo_acesso_log",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    arquivo_id = table.Column<long>(type: "bigint", nullable: false),
                    usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    acao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ip_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("arquivo_acesso_log_pkey", x => x.id);
                    table.ForeignKey(
                        name: "arquivo_acesso_log_arquivo_id_fkey",
                        column: x => x.arquivo_id,
                        principalSchema: "documento",
                        principalTable: "arquivo",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "arquivo_versao",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    arquivo_id = table.Column<long>(type: "bigint", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    storage_provider_id = table.Column<short>(type: "smallint", nullable: true),
                    bucket = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    storage_key = table.Column<string>(type: "text", nullable: true),
                    storage_path = table.Column<string>(type: "text", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    extensao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mime_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: true),
                    hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    criado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    criado_por_usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("arquivo_versao_pkey", x => x.id);
                    table.ForeignKey(
                        name: "arquivo_versao_arquivo_id_fkey",
                        column: x => x.arquivo_id,
                        principalSchema: "documento",
                        principalTable: "arquivo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "arquivo_versao_storage_provider_id_fkey",
                        column: x => x.storage_provider_id,
                        principalSchema: "documento",
                        principalTable: "storage_provider",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "arquivo_vinculo",
                schema: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    arquivo_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_anexo_id = table.Column<long>(type: "bigint", nullable: true),
                    entidade_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade_id = table.Column<long>(type: "bigint", nullable: false),
                    entidade_legado_id = table.Column<int>(type: "integer", nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_origem_coluna = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    entidade_legado_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entidade_legado_chave_1 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    entidade_legado_chave_2 = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    criterio_resolucao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vinculo_resolvido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    entidade_legado_chave_concatenada = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("arquivo_vinculo_pkey", x => x.id);
                    table.ForeignKey(
                        name: "arquivo_vinculo_arquivo_id_fkey",
                        column: x => x.arquivo_id,
                        principalSchema: "documento",
                        principalTable: "arquivo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "arquivo_vinculo_tipo_anexo_id_fkey",
                        column: x => x.tipo_anexo_id,
                        principalSchema: "documento",
                        principalTable: "tipo_anexo",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_data_documento",
                schema: "documento",
                table: "arquivo",
                column: "data_documento");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_extensao",
                schema: "documento",
                table: "arquivo",
                column: "extensao");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_extensao_normalizada",
                schema: "documento",
                table: "arquivo",
                column: "extensao_normalizada");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_hash",
                schema: "documento",
                table: "arquivo",
                column: "hash_sha256");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_migracao_status",
                schema: "documento",
                table: "arquivo",
                column: "migracao_status");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_public_id",
                schema: "documento",
                table: "arquivo",
                column: "public_id");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_status",
                schema: "documento",
                table: "arquivo",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_storage_key",
                schema: "documento",
                table: "arquivo",
                column: "storage_key");

            migrationBuilder.CreateIndex(
                name: "IX_arquivo_storage_provider_id",
                schema: "documento",
                table: "arquivo",
                column: "storage_provider_id");

            migrationBuilder.CreateIndex(
                name: "ux_arquivo_legado",
                schema: "documento",
                table: "arquivo",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_acesso_log_arquivo",
                schema: "documento",
                table: "arquivo_acesso_log",
                column: "arquivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_acesso_log_data",
                schema: "documento",
                table: "arquivo_acesso_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "arquivo_versao_arquivo_id_versao_key",
                schema: "documento",
                table: "arquivo_versao",
                columns: new[] { "arquivo_id", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_versao_arquivo",
                schema: "documento",
                table: "arquivo_versao",
                column: "arquivo_id");

            migrationBuilder.CreateIndex(
                name: "IX_arquivo_versao_storage_provider_id",
                schema: "documento",
                table: "arquivo_versao",
                column: "storage_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_vinculo_arquivo",
                schema: "documento",
                table: "arquivo_vinculo",
                column: "arquivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_vinculo_entidade",
                schema: "documento",
                table: "arquivo_vinculo",
                columns: new[] { "entidade_tipo", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_vinculo_legado",
                schema: "documento",
                table: "arquivo_vinculo",
                columns: new[] { "entidade_tipo", "entidade_legado_id" });

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_vinculo_legado_chaves",
                schema: "documento",
                table: "arquivo_vinculo",
                columns: new[] { "entidade_legado_tipo", "entidade_legado_chave_1", "entidade_legado_chave_2" });

            migrationBuilder.CreateIndex(
                name: "ix_arquivo_vinculo_tipo_anexo",
                schema: "documento",
                table: "arquivo_vinculo",
                column: "tipo_anexo_id");

            migrationBuilder.CreateIndex(
                name: "storage_provider_codigo_key",
                schema: "documento",
                table: "storage_provider",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_anexo_nome",
                schema: "documento",
                table: "tipo_anexo",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "ux_tipo_anexo_codigo",
                schema: "documento",
                table: "tipo_anexo",
                column: "codigo",
                unique: true,
                filter: "(codigo IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arquivo_acesso_log",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "arquivo_versao",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "arquivo_vinculo",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "arquivo",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "tipo_anexo",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "storage_provider",
                schema: "documento");
        }
    }
}
