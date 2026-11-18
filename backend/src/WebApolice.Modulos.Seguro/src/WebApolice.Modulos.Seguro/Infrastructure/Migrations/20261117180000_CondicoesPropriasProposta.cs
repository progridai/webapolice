using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261117180000_CondicoesPropriasProposta")]
public sealed class CondicoesPropriasProposta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
        name: "condicoes_proprias", schema: "seguro", table: "proposta", type: "boolean", nullable: false, defaultValue: false);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$ BEGIN
          IF EXISTS(SELECT 1 FROM seguro.proposta WHERE condicoes_proprias) THEN
            RAISE EXCEPTION 'Rollback bloqueado: existem propostas com condições próprias.';
          END IF;
        END $$;
        ALTER TABLE seguro.proposta DROP COLUMN condicoes_proprias;
        """);
}
