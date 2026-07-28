using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class InicialSeguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seguranca");

            migrationBuilder.CreateTable(
                name: "modulo",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modulo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfil",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    perfil_sistema = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    acesso_total = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    keycloak_sub = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ultimo_login_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recurso",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    modulo_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rota_frontend = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurso", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurso_modulo_modulo_id",
                        column: x => x.modulo_id,
                        principalSchema: "seguranca",
                        principalTable: "modulo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_perfil",
                schema: "seguranca",
                columns: table => new
                {
                    usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    perfil_id = table.Column<long>(type: "bigint", nullable: false),
                    atribuido_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_perfil", x => new { x.usuario_id, x.perfil_id });
                    table.ForeignKey(
                        name: "fk_usuario_perfil_perfil_perfil_id",
                        column: x => x.perfil_id,
                        principalSchema: "seguranca",
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_perfil_usuario_atribuido_por_usuario_id",
                        column: x => x.atribuido_por_usuario_id,
                        principalSchema: "seguranca",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_usuario_perfil_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguranca",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permissao",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    recurso_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao", x => x.id);
                    table.ForeignKey(
                        name: "fk_permissao_recursos_recurso_id",
                        column: x => x.recurso_id,
                        principalSchema: "seguranca",
                        principalTable: "recurso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_permissao",
                schema: "seguranca",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_executor_id = table.Column<long>(type: "bigint", nullable: true),
                    acao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade_id = table.Column<long>(type: "bigint", nullable: false),
                    usuario_afetado_id = table.Column<long>(type: "bigint", nullable: true),
                    perfil_id = table.Column<long>(type: "bigint", nullable: true),
                    permissao_id = table.Column<long>(type: "bigint", nullable: true),
                    dados_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    dados_novos = table.Column<string>(type: "jsonb", nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoria_permissao", x => x.id);
                    table.ForeignKey(
                        name: "fk_auditoria_permissao_perfis_perfil_id",
                        column: x => x.perfil_id,
                        principalSchema: "seguranca",
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_permissao_permissoes_permissao_id",
                        column: x => x.permissao_id,
                        principalSchema: "seguranca",
                        principalTable: "permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_permissao_usuarios_usuario_afetado_id",
                        column: x => x.usuario_afetado_id,
                        principalSchema: "seguranca",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_permissao_usuarios_usuario_executor_id",
                        column: x => x.usuario_executor_id,
                        principalSchema: "seguranca",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfil_permissao",
                schema: "seguranca",
                columns: table => new
                {
                    perfil_id = table.Column<long>(type: "bigint", nullable: false),
                    permissao_id = table.Column<long>(type: "bigint", nullable: false),
                    atribuido_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil_permissao", x => new { x.perfil_id, x.permissao_id });
                    table.ForeignKey(
                        name: "fk_perfil_permissao_perfil_perfil_id",
                        column: x => x.perfil_id,
                        principalSchema: "seguranca",
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_perfil_permissao_permissoes_permissao_id",
                        column: x => x.permissao_id,
                        principalSchema: "seguranca",
                        principalTable: "permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_perfil_permissao_usuarios_atribuido_por_usuario_id",
                        column: x => x.atribuido_por_usuario_id,
                        principalSchema: "seguranca",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_acao",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "acao");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_correlation_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_created_at",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_perfil_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_permissao_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "permissao_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_public_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_usuario_afetado_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "usuario_afetado_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_permissao_usuario_executor_id",
                schema: "seguranca",
                table: "auditoria_permissao",
                column: "usuario_executor_id");

            migrationBuilder.CreateIndex(
                name: "ix_modulo_codigo",
                schema: "seguranca",
                table: "modulo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modulo_public_id",
                schema: "seguranca",
                table: "modulo",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_perfil_codigo",
                schema: "seguranca",
                table: "perfil",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_perfil_public_id",
                schema: "seguranca",
                table: "perfil",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_perfil_permissao_atribuido_por_usuario_id",
                schema: "seguranca",
                table: "perfil_permissao",
                column: "atribuido_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfil_permissao_permissao_id",
                schema: "seguranca",
                table: "perfil_permissao",
                column: "permissao_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissao_codigo",
                schema: "seguranca",
                table: "permissao",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissao_public_id",
                schema: "seguranca",
                table: "permissao",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissao_recurso_id",
                schema: "seguranca",
                table: "permissao",
                column: "recurso_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurso_modulo_id_codigo",
                schema: "seguranca",
                table: "recurso",
                columns: new[] { "modulo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recurso_public_id",
                schema: "seguranca",
                table: "recurso",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_keycloak_sub",
                schema: "seguranca",
                table: "usuario",
                column: "keycloak_sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_public_id",
                schema: "seguranca",
                table: "usuario",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_perfil_atribuido_por_usuario_id",
                schema: "seguranca",
                table: "usuario_perfil",
                column: "atribuido_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_perfil_perfil_id",
                schema: "seguranca",
                table: "usuario_perfil",
                column: "perfil_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_permissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "perfil_permissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "usuario_perfil",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "permissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "perfil",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "usuario",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "recurso",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "modulo",
                schema: "seguranca");
        }
    }
}
