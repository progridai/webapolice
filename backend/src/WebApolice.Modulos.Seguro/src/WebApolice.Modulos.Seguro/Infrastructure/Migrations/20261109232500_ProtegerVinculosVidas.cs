using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261109232500_ProtegerVinculosVidas")]
public sealed class ProtegerVinculosVidas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE OR REPLACE FUNCTION seguro.validar_vinculo_vida() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE v cadastro.cliente_vinculo%ROWTYPE; estip bigint;
        BEGIN
          IF TG_OP = 'UPDATE' THEN
            IF NEW.cliente_vinculo_id IS NOT DISTINCT FROM OLD.cliente_vinculo_id
               AND NEW.cliente_id = OLD.cliente_id AND NEW.apolice_id = OLD.apolice_id THEN RETURN NEW; END IF;
            IF EXISTS(SELECT 1 FROM seguro.proposta WHERE apolice_vida_id = OLD.id) THEN
              RAISE EXCEPTION 'Esta participação já possui propostas. O vínculo não pode ser alterado.' USING ERRCODE='23514', CONSTRAINT='ck_apolice_vida_vinculo';
            END IF;
          END IF;
          SELECT * INTO v FROM cadastro.cliente_vinculo WHERE id = NEW.cliente_vinculo_id FOR SHARE;
          SELECT estipulante_id INTO estip FROM seguro.apolice WHERE id = NEW.apolice_id;
          IF v.id IS NULL OR NOT v.ativo OR v.cliente_id <> NEW.cliente_id OR v.estipulante_id IS DISTINCT FROM estip
             OR NOT EXISTS(SELECT 1 FROM cadastro.cliente c JOIN core.pessoa p ON p.id=c.pessoa_id
                 WHERE c.id=NEW.cliente_id AND c.pessoa_id=v.pessoa_id AND c.deleted_at IS NULL AND p.deleted_at IS NULL)
             OR NOT EXISTS(SELECT 1 FROM cadastro.estipulante e WHERE e.id=estip AND e.ativo AND e.deleted_at IS NULL) THEN
            RAISE EXCEPTION 'Selecione um vínculo ativo do cliente no estipulante desta apólice.' USING ERRCODE='23514', CONSTRAINT='ck_apolice_vida_vinculo';
          END IF;
          RETURN NEW;
        END $$;
        CREATE TRIGGER validar_vinculo_vida BEFORE INSERT OR UPDATE OF cliente_vinculo_id,cliente_id,apolice_id ON seguro.apolice_vida
          FOR EACH ROW EXECUTE FUNCTION seguro.validar_vinculo_vida();

        CREATE OR REPLACE FUNCTION seguro.validar_vinculo_proposta() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE vida seguro.apolice_vida%ROWTYPE; v cadastro.cliente_vinculo%ROWTYPE; estip bigint;
        BEGIN
          -- Propostas legadas sem Vida mantêm seu contrato anterior.
          IF NEW.apolice_vida_id IS NULL THEN RETURN NEW; END IF;
          SELECT * INTO vida FROM seguro.apolice_vida WHERE id=NEW.apolice_vida_id FOR UPDATE;
          SELECT * INTO v FROM cadastro.cliente_vinculo WHERE id=NEW.cliente_vinculo_id FOR SHARE;
          SELECT estipulante_id INTO estip FROM seguro.apolice WHERE id=NEW.apolice_id;
          IF vida.id IS NULL OR NOT vida.ativo OR vida.deleted_at IS NOT NULL OR vida.apolice_id IS DISTINCT FROM NEW.apolice_id
             OR vida.cliente_id IS DISTINCT FROM NEW.cliente_id OR vida.cliente_vinculo_id IS DISTINCT FROM NEW.cliente_vinculo_id
             OR v.id IS NULL OR NOT v.ativo OR v.cliente_id IS DISTINCT FROM NEW.cliente_id OR v.pessoa_id IS DISTINCT FROM NEW.pessoa_id
             OR v.estipulante_id IS DISTINCT FROM estip OR NEW.estipulante_id IS DISTINCT FROM estip THEN
            RAISE EXCEPTION 'O vínculo da proposta está inativo ou incompatível com a participação e o estipulante.' USING ERRCODE='23514', CONSTRAINT='ck_proposta_vinculo';
          END IF;
          RETURN NEW;
        END $$;
        CREATE TRIGGER validar_vinculo_proposta BEFORE INSERT OR UPDATE OF apolice_vida_id,apolice_id,cliente_id,pessoa_id,cliente_vinculo_id,estipulante_id ON seguro.proposta
          FOR EACH ROW EXECUTE FUNCTION seguro.validar_vinculo_proposta();

        CREATE OR REPLACE FUNCTION seguro.preservar_identidade_vinculo() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF (NEW.cliente_id,NEW.pessoa_id,NEW.estipulante_id,NEW.matricula) IS DISTINCT FROM (OLD.cliente_id,OLD.pessoa_id,OLD.estipulante_id,OLD.matricula)
             AND (EXISTS(SELECT 1 FROM seguro.apolice_vida WHERE cliente_vinculo_id=OLD.id)
                  OR EXISTS(SELECT 1 FROM seguro.proposta WHERE cliente_vinculo_id=OLD.id)) THEN
            RAISE EXCEPTION 'Este vínculo já está em uso. Cadastre outro vínculo para mudar matrícula ou estipulante.' USING ERRCODE='23514', CONSTRAINT='ck_cliente_vinculo_identidade_em_uso';
          END IF;
          RETURN NEW;
        END $$;
        CREATE TRIGGER preservar_identidade_vinculo BEFORE UPDATE OF cliente_id,pessoa_id,estipulante_id,matricula ON cadastro.cliente_vinculo
          FOR EACH ROW EXECUTE FUNCTION seguro.preservar_identidade_vinculo();
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TRIGGER IF EXISTS preservar_identidade_vinculo ON cadastro.cliente_vinculo;
        DROP FUNCTION IF EXISTS seguro.preservar_identidade_vinculo();
        DROP TRIGGER IF EXISTS validar_vinculo_proposta ON seguro.proposta;
        DROP FUNCTION IF EXISTS seguro.validar_vinculo_proposta();
        DROP TRIGGER IF EXISTS validar_vinculo_vida ON seguro.apolice_vida;
        DROP FUNCTION IF EXISTS seguro.validar_vinculo_vida();
        """);
}
