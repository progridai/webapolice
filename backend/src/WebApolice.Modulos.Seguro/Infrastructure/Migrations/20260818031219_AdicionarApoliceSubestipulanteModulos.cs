using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarApoliceSubestipulanteModulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_apolice_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice");

            migrationBuilder.DropForeignKey(
                name: "FK_apolice_ramo_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropForeignKey(
                name: "FK_apolice_subestipulante_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropForeignKey(
                name: "FK_apolice_vida_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropForeignKey(
                name: "FK_apolice_vida_apolice_subestipulante_apolice_subestipulante_~",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_apolice_vida",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_apolice_subestipulante",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropPrimaryKey(
                name: "PK_apolice_ramo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_apolice",
                schema: "seguro",
                table: "apolice");

            migrationBuilder.RenameIndex(
                name: "ux_tipo_produto_legado",
                schema: "seguro",
                table: "tipo_produto",
                newName: "ix_tipo_produto_legado_id");

            migrationBuilder.RenameIndex(
                name: "ux_tabela_preco_legado",
                schema: "seguro",
                table: "tabela_preco",
                newName: "ix_tabela_preco_legado_id");

            migrationBuilder.RenameIndex(
                name: "proposta_status_codigo_key",
                schema: "seguro",
                table: "proposta_status",
                newName: "ix_proposta_status_codigo");

            migrationBuilder.RenameIndex(
                name: "ux_proposta_movimento_legado",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_vinculo",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_cliente_vinculo_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_tipo",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_movimento_tipo_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_proposta",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_proposta_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_estipulante",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_estipulante_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_competencia",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_ano_mes");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_cliente",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_cliente_id");

            migrationBuilder.RenameIndex(
                name: "ux_proposta_item_legado",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_tipo",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_tipo_produto_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_tabela",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_tabela_preco_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_proposta",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_proposta_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_produto",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_produto_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_plano",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_plano_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_historico_nova",
                schema: "seguro",
                table: "proposta_historico",
                newName: "ix_proposta_historico_proposta_nova_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_historico_anterior",
                schema: "seguro",
                table: "proposta_historico",
                newName: "ix_proposta_historico_proposta_anterior_id");

            migrationBuilder.RenameIndex(
                name: "ux_proposta_cobertura_legado",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_proposta",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_proposta_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_item",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_proposta_item_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_cobertura",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_cobertura_id");

            migrationBuilder.RenameIndex(
                name: "ux_proposta_beneficiario_legado",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_proposta",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_proposta_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_pessoa",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_pessoa_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_parentesco",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_parentesco_normalizado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_nome_trgm",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_nome");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_cpf",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_cpf_limpo");

            migrationBuilder.RenameIndex(
                name: "IX_proposta_proposta_origem_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_proposta_origem_id");

            migrationBuilder.RenameIndex(
                name: "ux_proposta_legado",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_status",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_status_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_pessoa",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_pessoa_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_estipulante_status",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_estipulante_id_status_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_estipulante",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_estipulante_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cliente_vinculo",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_cliente_vinculo_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cliente",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_cliente_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_apolice_vida",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_apolice_vida_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_apolice",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_apolice_id");

            migrationBuilder.RenameIndex(
                name: "ux_produto_legado",
                schema: "seguro",
                table: "produto",
                newName: "ix_produto_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_produto_tabela_preco",
                schema: "seguro",
                table: "produto",
                newName: "ix_produto_tabela_preco_id");

            migrationBuilder.RenameIndex(
                name: "ix_produto_plano",
                schema: "seguro",
                table: "produto",
                newName: "ix_produto_plano_id");

            migrationBuilder.RenameIndex(
                name: "ux_plano_legado",
                schema: "seguro",
                table: "plano",
                newName: "ix_plano_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_plano_nome_trgm",
                schema: "seguro",
                table: "plano",
                newName: "ix_plano_nome");

            migrationBuilder.RenameIndex(
                name: "ux_movimento_tipo_legado",
                schema: "seguro",
                table: "movimento_tipo",
                newName: "ix_movimento_tipo_legado_id");

            migrationBuilder.RenameIndex(
                name: "ux_cobertura_legado",
                schema: "seguro",
                table: "cobertura",
                newName: "ix_cobertura_legado_id");

            migrationBuilder.RenameIndex(
                name: "ix_cobertura_nome_trgm",
                schema: "seguro",
                table: "cobertura",
                newName: "ix_cobertura_nome");

            migrationBuilder.RenameIndex(
                name: "IX_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice",
                newName: "ix_apolice_apolice_origem_id");

            migrationBuilder.AddColumn<long>(
                name: "apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_apolice_vida",
                schema: "seguro",
                table: "apolice_vida",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_apolice_subestipulante",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_apolice_ramo",
                schema: "seguro",
                table: "apolice_ramo",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_apolice",
                schema: "seguro",
                table: "apolice",
                column: "id");

            migrationBuilder.CreateTable(
                name: "apolice_subestipulante_modulo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_subestipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    modulo_id = table.Column<long>(type: "bigint", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_subestipulante_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_subestipulante_modulo_apolice_subestipulante_apolic",
                        column: x => x.apolice_subestipulante_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_subestipulante",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_subestipulante_modulo_apolice_subestipulante_id_mod",
                schema: "seguro",
                table: "apolice_subestipulante_modulo",
                columns: new[] { "apolice_subestipulante_id", "modulo_id" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice",
                column: "apolice_origem_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_ramo_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_ramo",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_subestipulante_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_apolice_subestipulante_",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_id",
                principalSchema: "seguro",
                principalTable: "apolice_subestipulante",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_modulo_apolice_subestip",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_modulo_id",
                principalSchema: "seguro",
                principalTable: "apolice_subestipulante_modulo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apolice_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_ramo_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_subestipulante_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_apolice_subestipulante_",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_modulo_apolice_subestip",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropTable(
                name: "apolice_subestipulante_modulo",
                schema: "seguro");

            migrationBuilder.DropPrimaryKey(
                name: "pk_apolice_vida",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropIndex(
                name: "ix_apolice_vida_apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropPrimaryKey(
                name: "pk_apolice_subestipulante",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropPrimaryKey(
                name: "pk_apolice_ramo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropPrimaryKey(
                name: "pk_apolice",
                schema: "seguro",
                table: "apolice");

            migrationBuilder.DropColumn(
                name: "apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.RenameIndex(
                name: "ix_tipo_produto_legado_id",
                schema: "seguro",
                table: "tipo_produto",
                newName: "ux_tipo_produto_legado");

            migrationBuilder.RenameIndex(
                name: "ix_tabela_preco_legado_id",
                schema: "seguro",
                table: "tabela_preco",
                newName: "ux_tabela_preco_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_status_codigo",
                schema: "seguro",
                table: "proposta_status",
                newName: "proposta_status_codigo_key");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_proposta_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_proposta");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_movimento_tipo_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_tipo");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_legado_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ux_proposta_movimento_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_estipulante_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_estipulante");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_cliente_vinculo_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_vinculo");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_cliente_id",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_cliente");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_movimento_ano_mes",
                schema: "seguro",
                table: "proposta_movimento",
                newName: "ix_proposta_movimento_competencia");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_tipo_produto_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_tipo");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_tabela_preco_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_tabela");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_proposta_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_proposta");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_produto_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_produto");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_plano_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ix_proposta_item_plano");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_item_legado_id",
                schema: "seguro",
                table: "proposta_item",
                newName: "ux_proposta_item_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_historico_proposta_nova_id",
                schema: "seguro",
                table: "proposta_historico",
                newName: "ix_proposta_historico_nova");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_historico_proposta_anterior_id",
                schema: "seguro",
                table: "proposta_historico",
                newName: "ix_proposta_historico_anterior");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_proposta_item_id",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_item");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_proposta_id",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_proposta");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_legado_id",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ux_proposta_cobertura_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cobertura_cobertura_id",
                schema: "seguro",
                table: "proposta_cobertura",
                newName: "ix_proposta_cobertura_cobertura");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_proposta_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_proposta");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_pessoa_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_pessoa");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_parentesco_normalizado",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_parentesco");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_nome",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_nome_trgm");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_legado_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ux_proposta_beneficiario_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_beneficiario_cpf_limpo",
                schema: "seguro",
                table: "proposta_beneficiario",
                newName: "ix_proposta_beneficiario_cpf");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_proposta_origem_id",
                schema: "seguro",
                table: "proposta",
                newName: "IX_proposta_proposta_origem_id");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_status_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_status");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_pessoa_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_pessoa");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_legado_id",
                schema: "seguro",
                table: "proposta",
                newName: "ux_proposta_legado");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_estipulante_id_status_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_estipulante_status");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_estipulante_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_estipulante");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cliente_vinculo_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_cliente_vinculo");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_cliente_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_cliente");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_apolice_vida_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_apolice_vida");

            migrationBuilder.RenameIndex(
                name: "ix_proposta_apolice_id",
                schema: "seguro",
                table: "proposta",
                newName: "ix_proposta_apolice");

            migrationBuilder.RenameIndex(
                name: "ix_produto_tabela_preco_id",
                schema: "seguro",
                table: "produto",
                newName: "ix_produto_tabela_preco");

            migrationBuilder.RenameIndex(
                name: "ix_produto_plano_id",
                schema: "seguro",
                table: "produto",
                newName: "ix_produto_plano");

            migrationBuilder.RenameIndex(
                name: "ix_produto_legado_id",
                schema: "seguro",
                table: "produto",
                newName: "ux_produto_legado");

            migrationBuilder.RenameIndex(
                name: "ix_plano_nome",
                schema: "seguro",
                table: "plano",
                newName: "ix_plano_nome_trgm");

            migrationBuilder.RenameIndex(
                name: "ix_plano_legado_id",
                schema: "seguro",
                table: "plano",
                newName: "ux_plano_legado");

            migrationBuilder.RenameIndex(
                name: "ix_movimento_tipo_legado_id",
                schema: "seguro",
                table: "movimento_tipo",
                newName: "ux_movimento_tipo_legado");

            migrationBuilder.RenameIndex(
                name: "ix_cobertura_nome",
                schema: "seguro",
                table: "cobertura",
                newName: "ix_cobertura_nome_trgm");

            migrationBuilder.RenameIndex(
                name: "ix_cobertura_legado_id",
                schema: "seguro",
                table: "cobertura",
                newName: "ux_cobertura_legado");

            migrationBuilder.RenameIndex(
                name: "ix_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice",
                newName: "IX_apolice_apolice_origem_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_apolice_vida",
                schema: "seguro",
                table: "apolice_vida",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_apolice_subestipulante",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_apolice_ramo",
                schema: "seguro",
                table: "apolice_ramo",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_apolice",
                schema: "seguro",
                table: "apolice",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice",
                column: "apolice_origem_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_ramo_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_ramo",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_subestipulante_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_vida_apolice_apolice_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_vida_apolice_subestipulante_apolice_subestipulante_~",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_id",
                principalSchema: "seguro",
                principalTable: "apolice_subestipulante",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
