using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PropostasOperacionaisModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "plano_nome_registrado",
                schema: "seguro",
                table: "proposta_item",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "plano_paga",
                schema: "seguro",
                table: "proposta_item",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "plano_reajuste",
                schema: "seguro",
                table: "proposta_item",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "legado_id",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "apolice_modulo_cobertura_id",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cobertura_basica_registrada",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cobertura_nome_reduzido_registrado",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cobertura_nome_registrado",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "cobertura_reajuste",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<long>(
                name: "apolice_modulo_id",
                schema: "seguro",
                table: "proposta",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modulo_nome_registrado",
                schema: "seguro",
                table: "proposta",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "subgrupo_nome_registrado",
                schema: "seguro",
                table: "proposta",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item",
                column: "apolice_modulo_plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_apolice_modulo_cobertura_id",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "apolice_modulo_cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_public_id",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_apolice_modulo_id",
                schema: "seguro",
                table: "proposta",
                column: "apolice_modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta",
                column: "apolice_subgrupo_id");

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_apolice_modulos_apolice_modulo_id",
                schema: "seguro",
                table: "proposta",
                column: "apolice_modulo_id",
                principalSchema: "seguro",
                principalTable: "apolice_modulo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_apolice_subgrupos_apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta",
                column: "apolice_subgrupo_id",
                principalSchema: "seguro",
                principalTable: "apolice_subgrupo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_cobertura_apolice_modulo_coberturas_apolice_modulo",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "apolice_modulo_cobertura_id",
                principalSchema: "seguro",
                principalTable: "apolice_modulo_cobertura",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_item_apolice_modulo_planos_apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item",
                column: "apolice_modulo_plano_id",
                principalSchema: "seguro",
                principalTable: "apolice_modulo_plano",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql(PropostasContextoSql.Up);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS(SELECT 1 FROM seguro.proposta WHERE apolice_modulo_id IS NOT NULL OR apolice_subgrupo_id IS NOT NULL OR modulo_nome_registrado IS NOT NULL OR subgrupo_nome_registrado IS NOT NULL)
                    OR EXISTS(SELECT 1 FROM seguro.proposta_item WHERE apolice_modulo_plano_id IS NOT NULL OR plano_nome_registrado IS NOT NULL OR plano_paga IS NOT NULL OR plano_reajuste IS NOT NULL)
                    OR EXISTS(SELECT 1 FROM seguro.proposta_cobertura WHERE legado_id IS NULL OR apolice_modulo_cobertura_id IS NOT NULL OR cobertura_nome_registrado IS NOT NULL OR cobertura_nome_reduzido_registrado IS NOT NULL OR cobertura_basica_registrada IS NOT NULL OR cobertura_reajuste IS NOT NULL) THEN
                    RAISE EXCEPTION 'Rollback bloqueado: existem condições nativas registradas em propostas.';
                  END IF;
                END $$;
                """);
            migrationBuilder.Sql(PropostasContextoSql.Down);
            migrationBuilder.DropForeignKey(
                name: "fk_proposta_apolice_modulos_apolice_modulo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropForeignKey(
                name: "fk_proposta_apolice_subgrupos_apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropForeignKey(
                name: "fk_proposta_cobertura_apolice_modulo_coberturas_apolice_modulo",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropForeignKey(
                name: "fk_proposta_item_apolice_modulo_planos_apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropIndex(
                name: "ix_proposta_item_apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropIndex(
                name: "ix_proposta_cobertura_apolice_modulo_cobertura_id",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropIndex(
                name: "ix_proposta_cobertura_public_id",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropIndex(
                name: "ix_proposta_apolice_modulo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropIndex(
                name: "ix_proposta_apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "apolice_modulo_plano_id",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropColumn(
                name: "plano_nome_registrado",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropColumn(
                name: "plano_paga",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropColumn(
                name: "plano_reajuste",
                schema: "seguro",
                table: "proposta_item");

            migrationBuilder.DropColumn(
                name: "apolice_modulo_cobertura_id",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "cobertura_basica_registrada",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "cobertura_nome_reduzido_registrado",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "cobertura_nome_registrado",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "cobertura_reajuste",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "apolice_modulo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "apolice_subgrupo_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "modulo_nome_registrado",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "subgrupo_nome_registrado",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.AlterColumn<int>(
                name: "legado_id",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
