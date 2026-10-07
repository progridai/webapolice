using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompartilharCoberturaNoPlanoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "cobertura_id", schema: "seguro",
                table: "apolice_modulo_cobertura", type: "bigint", nullable: true);

            // Preserva os dados locais criados na entrega anterior. Não presume
            // identidade pelo nome: cada registro é convertido em um cadastro base.
            migrationBuilder.Sql("""
                DO $backfill$
                DECLARE item record; cadastro_id bigint;
                BEGIN
                    FOR item IN SELECT * FROM seguro.apolice_modulo_cobertura WHERE cobertura_id IS NULL LOOP
                        INSERT INTO seguro.cobertura(public_id, nome, nome_reduzido, basica, reajuste, ativo, created_at, updated_at)
                        VALUES(gen_random_uuid(), item.nome, item.nome_reduzido, item.basica, item.reajuste, true, item.created_at, item.updated_at)
                        RETURNING id INTO cadastro_id;
                        UPDATE seguro.apolice_modulo_cobertura SET cobertura_id = cadastro_id WHERE id = item.id;
                    END LOOP;
                END $backfill$;
                """);
            migrationBuilder.AlterColumn<long>(name: "cobertura_id", schema: "seguro",
                table: "apolice_modulo_cobertura", type: "bigint", nullable: false,
                oldClrType: typeof(long), oldType: "bigint", oldNullable: true);

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropCheckConstraint(
                name: "ck_modulo_cobertura_nome",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "basica",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "nome",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "nome_reduzido",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "reajuste",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                columns: new[] { "apolice_modulo_plano_id", "cobertura_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_cobertura_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                column: "cobertura_id");

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_modulo_cobertura_cobertura_cobertura_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                column: "cobertura_id",
                principalSchema: "seguro",
                principalTable: "cobertura",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apolice_modulo_cobertura_cobertura_cobertura_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_cobertura_cobertura_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.AddColumn<string>(
                name: "basica",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "nome_reduzido",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "reajuste",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE seguro.apolice_modulo_cobertura vinculo
                SET nome = cadastro.nome, nome_reduzido = cadastro.nome_reduzido,
                    basica = cadastro.basica, reajuste = cadastro.reajuste
                FROM seguro.cobertura cadastro WHERE cadastro.id = vinculo.cobertura_id;
                """);
            migrationBuilder.DropColumn(name: "cobertura_id", schema: "seguro", table: "apolice_modulo_cobertura");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                column: "apolice_modulo_plano_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_modulo_cobertura_nome",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                sql: "length(btrim(nome)) > 0");
        }
    }
}
