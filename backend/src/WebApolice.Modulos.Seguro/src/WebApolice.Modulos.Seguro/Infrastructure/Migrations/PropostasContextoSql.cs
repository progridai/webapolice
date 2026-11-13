namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

internal static class PropostasContextoSql
{
    internal const string Up = """
        CREATE FUNCTION seguro.validar_contexto_proposta() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE apolice_contexto bigint; modulo_contexto bigint; proposta_contexto bigint; plano_contexto bigint; valido boolean;
        BEGIN
          IF TG_TABLE_NAME='proposta_item' THEN
            IF NEW.apolice_modulo_plano_id IS NULL THEN RETURN NEW; END IF;
          ELSIF TG_TABLE_NAME='proposta_cobertura' THEN
            IF NEW.apolice_modulo_cobertura_id IS NULL THEN RETURN NEW; END IF;
          END IF;
          IF TG_TABLE_NAME = 'proposta' THEN
            IF TG_OP = 'UPDATE' AND NEW.apolice_id IS NOT DISTINCT FROM OLD.apolice_id
              AND NEW.apolice_subgrupo_id IS NOT DISTINCT FROM OLD.apolice_subgrupo_id
              AND NEW.apolice_modulo_id IS NOT DISTINCT FROM OLD.apolice_modulo_id THEN RETURN NEW; END IF;
            IF NEW.apolice_subgrupo_id IS NULL AND NEW.apolice_modulo_id IS NULL THEN RETURN NEW; END IF;
            apolice_contexto := NEW.apolice_id;
          ELSE
            SELECT apolice_id, apolice_modulo_id INTO apolice_contexto, modulo_contexto FROM seguro.proposta WHERE id=NEW.proposta_id;
          END IF;
          PERFORM 1 FROM seguro.apolice WHERE id=apolice_contexto AND deleted_at IS NULL FOR KEY SHARE;
          IF NOT FOUND THEN RAISE EXCEPTION 'Apólice indisponível para a proposta.' USING ERRCODE='23514',CONSTRAINT='ck_proposta_contexto'; END IF;
          valido := true;
          IF TG_TABLE_NAME='proposta' THEN
            valido := (NEW.apolice_subgrupo_id IS NULL OR EXISTS(SELECT 1 FROM seguro.apolice_subgrupo WHERE id=NEW.apolice_subgrupo_id AND apolice_id=apolice_contexto AND deleted_at IS NULL))
              AND (NEW.apolice_modulo_id IS NULL OR EXISTS(SELECT 1 FROM seguro.apolice_modulo WHERE id=NEW.apolice_modulo_id AND apolice_id=apolice_contexto AND deleted_at IS NULL));
          ELSIF TG_TABLE_NAME='proposta_item' THEN
            IF NEW.apolice_modulo_plano_id IS NULL THEN RETURN NEW; END IF;
            valido := EXISTS(SELECT 1 FROM seguro.apolice_modulo_plano p JOIN seguro.apolice_modulo m ON m.id=p.apolice_modulo_id
              WHERE p.id=NEW.apolice_modulo_plano_id AND m.id=modulo_contexto AND m.apolice_id=apolice_contexto AND p.deleted_at IS NULL AND m.deleted_at IS NULL);
          ELSE
            IF NEW.apolice_modulo_cobertura_id IS NULL THEN RETURN NEW; END IF;
            SELECT proposta_id,apolice_modulo_plano_id INTO proposta_contexto,plano_contexto FROM seguro.proposta_item WHERE id=NEW.proposta_item_id;
            valido := proposta_contexto=NEW.proposta_id AND EXISTS(SELECT 1 FROM seguro.apolice_modulo_cobertura c
              JOIN seguro.apolice_modulo_plano p ON p.id=c.apolice_modulo_plano_id
              JOIN seguro.apolice_modulo m ON m.id=p.apolice_modulo_id
              WHERE c.id=NEW.apolice_modulo_cobertura_id AND c.cobertura_id=NEW.cobertura_id AND p.id=plano_contexto
                AND m.id=modulo_contexto AND m.apolice_id=apolice_contexto AND c.deleted_at IS NULL AND p.deleted_at IS NULL AND m.deleted_at IS NULL);
          END IF;
          IF valido IS DISTINCT FROM true THEN RAISE EXCEPTION 'Plano, cobertura ou vínculo incompatível com o contexto da proposta.' USING ERRCODE='23514',CONSTRAINT='ck_proposta_contexto'; END IF;
          RETURN NEW;
        END $$;
        CREATE TRIGGER contexto_proposta BEFORE INSERT OR UPDATE ON seguro.proposta FOR EACH ROW EXECUTE FUNCTION seguro.validar_contexto_proposta();
        CREATE TRIGGER contexto_proposta BEFORE INSERT OR UPDATE ON seguro.proposta_item FOR EACH ROW EXECUTE FUNCTION seguro.validar_contexto_proposta();
        CREATE TRIGGER contexto_proposta BEFORE INSERT OR UPDATE ON seguro.proposta_cobertura FOR EACH ROW EXECUTE FUNCTION seguro.validar_contexto_proposta();

        CREATE FUNCTION seguro.proteger_contexto_proposta() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE usado boolean; apolice_contexto bigint;
        BEGIN
          IF OLD.deleted_at IS NOT NULL OR NEW.deleted_at IS NULL THEN RETURN NEW; END IF;
          IF TG_TABLE_NAME IN ('apolice_subgrupo','apolice_modulo') THEN apolice_contexto:=NEW.apolice_id;
          ELSIF TG_TABLE_NAME='apolice_modulo_plano' THEN SELECT apolice_id INTO apolice_contexto FROM seguro.apolice_modulo WHERE id=NEW.apolice_modulo_id;
          ELSE SELECT m.apolice_id INTO apolice_contexto FROM seguro.apolice_modulo_plano p JOIN seguro.apolice_modulo m ON m.id=p.apolice_modulo_id WHERE p.id=NEW.apolice_modulo_plano_id; END IF;
          PERFORM 1 FROM seguro.apolice WHERE id=apolice_contexto FOR UPDATE;
          CASE TG_TABLE_NAME
            WHEN 'apolice_subgrupo' THEN SELECT EXISTS(SELECT 1 FROM seguro.proposta WHERE apolice_subgrupo_id=NEW.id) INTO usado;
            WHEN 'apolice_modulo' THEN SELECT EXISTS(SELECT 1 FROM seguro.proposta WHERE apolice_modulo_id=NEW.id) INTO usado;
            WHEN 'apolice_modulo_plano' THEN SELECT EXISTS(SELECT 1 FROM seguro.proposta_item WHERE apolice_modulo_plano_id=NEW.id) INTO usado;
            ELSE SELECT EXISTS(SELECT 1 FROM seguro.proposta_cobertura WHERE apolice_modulo_cobertura_id=NEW.id) INTO usado;
          END CASE;
          IF usado THEN RAISE EXCEPTION 'O vínculo possui condições registradas em propostas e não pode ser excluído.' USING ERRCODE='23514',CONSTRAINT='ck_proposta_contexto'; END IF;
          RETURN NEW;
        END $$;
        CREATE TRIGGER proteger_contexto_proposta BEFORE UPDATE OF deleted_at ON seguro.apolice_subgrupo FOR EACH ROW EXECUTE FUNCTION seguro.proteger_contexto_proposta();
        CREATE TRIGGER proteger_contexto_proposta BEFORE UPDATE OF deleted_at ON seguro.apolice_modulo FOR EACH ROW EXECUTE FUNCTION seguro.proteger_contexto_proposta();
        CREATE TRIGGER proteger_contexto_proposta BEFORE UPDATE OF deleted_at ON seguro.apolice_modulo_plano FOR EACH ROW EXECUTE FUNCTION seguro.proteger_contexto_proposta();
        CREATE TRIGGER proteger_contexto_proposta BEFORE UPDATE OF deleted_at ON seguro.apolice_modulo_cobertura FOR EACH ROW EXECUTE FUNCTION seguro.proteger_contexto_proposta();
        """;
    internal const string Down = """
        DROP TRIGGER contexto_proposta ON seguro.proposta;
        DROP TRIGGER contexto_proposta ON seguro.proposta_item;
        DROP TRIGGER contexto_proposta ON seguro.proposta_cobertura;
        DROP TRIGGER proteger_contexto_proposta ON seguro.apolice_subgrupo;
        DROP TRIGGER proteger_contexto_proposta ON seguro.apolice_modulo;
        DROP TRIGGER proteger_contexto_proposta ON seguro.apolice_modulo_plano;
        DROP TRIGGER proteger_contexto_proposta ON seguro.apolice_modulo_cobertura;
        DROP FUNCTION seguro.validar_contexto_proposta();
        DROP FUNCTION seguro.proteger_contexto_proposta();
        """;
}
