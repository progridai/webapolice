using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261118140000_IdentidadeMovimentoNativo")]
public sealed class IdentidadeMovimentoNativo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "public_id", schema: "seguro", table: "proposta_movimento", type: "uuid", nullable: true);
        migrationBuilder.AlterColumn<int>(name: "legado_id", schema: "seguro", table: "proposta_movimento", type: "integer", nullable: true,
            oldClrType: typeof(int), oldType: "integer");
        migrationBuilder.CreateIndex(name: "ux_proposta_movimento_public_id", schema: "seguro", table: "proposta_movimento",
            column: "public_id", unique: true, filter: "public_id IS NOT NULL");
        migrationBuilder.AddCheckConstraint(name: "ck_proposta_movimento_identidade", schema: "seguro", table: "proposta_movimento",
            sql: "(legado_id IS NOT NULL OR public_id IS NOT NULL) AND (public_id IS NULL OR public_id <> '00000000-0000-0000-0000-000000000000'::uuid)");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$ BEGIN
          IF EXISTS(SELECT 1 FROM seguro.proposta_movimento WHERE public_id IS NOT NULL OR legado_id IS NULL) THEN
            RAISE EXCEPTION 'Rollback bloqueado: existem movimentos com identidade nativa registrada.';
          END IF;
        END $$;
        ALTER TABLE seguro.proposta_movimento DROP CONSTRAINT ck_proposta_movimento_identidade;
        DROP INDEX seguro.ux_proposta_movimento_public_id;
        ALTER TABLE seguro.proposta_movimento DROP COLUMN public_id;
        ALTER TABLE seguro.proposta_movimento ALTER COLUMN legado_id SET NOT NULL;
        """);
}
