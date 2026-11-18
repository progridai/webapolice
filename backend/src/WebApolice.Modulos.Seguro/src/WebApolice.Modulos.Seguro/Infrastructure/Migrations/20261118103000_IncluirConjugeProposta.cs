using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261118103000_IncluirConjugeProposta")]
public sealed class IncluirConjugeProposta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
        name: "inclui_conjuge", schema: "seguro", table: "proposta", type: "boolean", nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$ BEGIN
          IF EXISTS(SELECT 1 FROM seguro.proposta WHERE inclui_conjuge IS NOT NULL) THEN
            RAISE EXCEPTION 'Rollback bloqueado: existem escolhas de contratação de cônjuge registradas.';
          END IF;
        END $$;
        ALTER TABLE seguro.proposta DROP COLUMN inclui_conjuge;
        """);
}
