using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261111224800_DefinirRascunhoProposta")]
public sealed class DefinirRascunhoProposta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        LOCK TABLE seguro.proposta_status IN SHARE ROW EXCLUSIVE MODE;
        DO $$ DECLARE novo_id smallint; BEGIN
          IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO') THEN
            SELECT COALESCE(MAX(id)::int,0)+1 INTO novo_id FROM seguro.proposta_status;
            INSERT INTO seguro.proposta_status(id,codigo,nome,permite_movimentacao,visivel_operacional,finalizador)
              VALUES(novo_id,'RASCUNHO','Rascunho',false,true,false);
          ELSIF EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO' AND (permite_movimentacao OR NOT visivel_operacional OR finalizador)) THEN
            RAISE EXCEPTION 'O código RASCUNHO já existe com configuração incompatível. Revisar o catálogo antes de continuar.';
          END IF;
        END $$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$ BEGIN
          IF EXISTS(SELECT 1 FROM seguro.proposta p JOIN seguro.proposta_status s ON s.id=p.status_id WHERE s.codigo='RASCUNHO') THEN
            RAISE EXCEPTION 'Rollback bloqueado: o status RASCUNHO possui propostas.';
          END IF;
        END $$;
        -- Preservar o catálogo aprovado: não há remoção automática de situação de negócio.
        """);
}
