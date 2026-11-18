using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261117120000_NomePreProposta")]
public sealed class NomePreProposta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        LOCK TABLE seguro.proposta_status IN SHARE ROW EXCLUSIVE MODE;
        DO $$ BEGIN
          IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO') THEN
            RAISE EXCEPTION 'Situação inicial ausente. Revisar o catálogo antes de continuar.';
          END IF;
          IF EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO'
            AND (permite_movimentacao OR NOT visivel_operacional OR finalizador)) THEN
            RAISE EXCEPTION 'Situação inicial incompatível. Revisar o catálogo antes de continuar.';
          END IF;
        END $$;
        UPDATE seguro.proposta_status SET nome='Pré Proposta'
          WHERE codigo='RASCUNHO' AND nome IS DISTINCT FROM 'Pré Proposta';
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        -- Preservar a nomenclatura de negócio aprovada, sem reescrever o catálogo no rollback.
        SELECT 1;
        """);
}
