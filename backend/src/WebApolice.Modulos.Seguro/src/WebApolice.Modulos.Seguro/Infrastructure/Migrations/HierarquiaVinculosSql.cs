namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

internal static class HierarquiaVinculosSql
{
    internal const string Up = """
        CREATE FUNCTION seguro.validar_pais_vinculo_apolice() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE v_apolice_id bigint; removida timestamptz; valido boolean;
        BEGIN
          IF TG_TABLE_NAME NOT IN ('proposta','proposta_item','proposta_cobertura')
             AND to_jsonb(NEW)->>'deleted_at' IS NOT NULL THEN RETURN NEW; END IF;
          CASE TG_TABLE_NAME
            WHEN 'apolice_modulo_plano' THEN SELECT apolice_id INTO v_apolice_id FROM seguro.apolice_modulo WHERE id=NEW.apolice_modulo_id;
            WHEN 'apolice_modulo_cobertura' THEN SELECT m.apolice_id INTO v_apolice_id FROM seguro.apolice_modulo_plano p JOIN seguro.apolice_modulo m ON m.id=p.apolice_modulo_id WHERE p.id=NEW.apolice_modulo_plano_id;
            WHEN 'apolice_plano' THEN SELECT apolice_id INTO v_apolice_id FROM seguro.apolice_produto WHERE id=NEW.apolice_produto_id;
            WHEN 'apolice_cobertura' THEN SELECT p.apolice_id INTO v_apolice_id FROM seguro.apolice_plano l JOIN seguro.apolice_produto p ON p.id=l.apolice_produto_id WHERE l.id=NEW.apolice_plano_id;
            WHEN 'proposta_item' THEN SELECT apolice_id INTO v_apolice_id FROM seguro.proposta WHERE id=NEW.proposta_id;
            WHEN 'proposta_cobertura' THEN SELECT apolice_id INTO v_apolice_id FROM seguro.proposta WHERE id=NEW.proposta_id;
            ELSE v_apolice_id := NEW.apolice_id;
          END CASE;
          -- Propostas anteriores sem contexto de Apólice preservam seu contrato.
          IF v_apolice_id IS NULL THEN RETURN NEW; END IF;
          SELECT deleted_at INTO removida FROM seguro.apolice WHERE id=v_apolice_id FOR KEY SHARE;
          IF NOT FOUND OR removida IS NOT NULL THEN
            RAISE EXCEPTION 'A apólice não está disponível para este vínculo.' USING ERRCODE='23514',CONSTRAINT='ck_hierarquia_vinculo_apolice';
          END IF;
          valido := true;
          CASE TG_TABLE_NAME
            WHEN 'apolice_modulo_plano' THEN
              SELECT EXISTS(SELECT 1 FROM seguro.apolice_modulo WHERE id=NEW.apolice_modulo_id AND deleted_at IS NULL) INTO valido;
            WHEN 'apolice_modulo_cobertura' THEN
              SELECT EXISTS(SELECT 1 FROM seguro.apolice_modulo_plano p JOIN seguro.apolice_modulo m ON m.id=p.apolice_modulo_id WHERE p.id=NEW.apolice_modulo_plano_id AND p.deleted_at IS NULL AND m.deleted_at IS NULL) INTO valido;
            WHEN 'apolice_plano' THEN
              SELECT EXISTS(SELECT 1 FROM seguro.apolice_produto WHERE id=NEW.apolice_produto_id AND deleted_at IS NULL) INTO valido;
            WHEN 'apolice_cobertura' THEN
              SELECT EXISTS(SELECT 1 FROM seguro.apolice_plano l JOIN seguro.apolice_produto p ON p.id=l.apolice_produto_id WHERE l.id=NEW.apolice_plano_id AND l.deleted_at IS NULL AND p.deleted_at IS NULL) INTO valido;
            WHEN 'apolice_vida' THEN
              valido := (NEW.apolice_modulo_id IS NULL OR EXISTS(SELECT 1 FROM seguro.apolice_modulo WHERE id=NEW.apolice_modulo_id AND apolice_id=v_apolice_id AND deleted_at IS NULL))
                AND (NEW.apolice_subgrupo_id IS NULL OR EXISTS(SELECT 1 FROM seguro.apolice_subgrupo WHERE id=NEW.apolice_subgrupo_id AND apolice_id=v_apolice_id AND deleted_at IS NULL));
            ELSE NULL;
          END CASE;
          IF NOT valido THEN
            RAISE EXCEPTION 'O vínculo pai foi excluído ou não pertence à apólice. Atualize a tela e selecione um vínculo disponível.' USING ERRCODE='23514',CONSTRAINT='ck_hierarquia_vinculo_apolice';
          END IF;
          RETURN NEW;
        END $$;
        DO $$ DECLARE tabela text; BEGIN
          FOREACH tabela IN ARRAY ARRAY['apolice_ramo','apolice_subgrupo','apolice_subestipulante','apolice_modulo','apolice_modulo_plano','apolice_modulo_cobertura','apolice_produto','apolice_plano','apolice_cobertura','apolice_vida','proposta','proposta_item','proposta_cobertura'] LOOP
            EXECUTE format('CREATE TRIGGER hierarquia_vinculo_apolice BEFORE INSERT OR UPDATE ON seguro.%I FOR EACH ROW EXECUTE FUNCTION seguro.validar_pais_vinculo_apolice()',tabela);
          END LOOP;
        END $$;
        """;

    internal const string Down = """
        DO $$ DECLARE tabela text; BEGIN
          FOREACH tabela IN ARRAY ARRAY['apolice_ramo','apolice_subgrupo','apolice_subestipulante','apolice_modulo','apolice_modulo_plano','apolice_modulo_cobertura','apolice_produto','apolice_plano','apolice_cobertura','apolice_vida','proposta','proposta_item','proposta_cobertura'] LOOP
            EXECUTE format('DROP TRIGGER IF EXISTS hierarquia_vinculo_apolice ON seguro.%I',tabela);
          END LOOP;
        END $$;
        DROP FUNCTION IF EXISTS seguro.validar_pais_vinculo_apolice();
        """;
}
