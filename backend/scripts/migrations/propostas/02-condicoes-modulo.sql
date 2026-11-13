
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_item ADD apolice_modulo_plano_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_item ADD plano_nome_registrado character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_item ADD plano_paga boolean;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_item ADD plano_reajuste boolean;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ALTER COLUMN legado_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD apolice_modulo_cobertura_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD cobertura_basica_registrada character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD cobertura_nome_reduzido_registrado character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD cobertura_nome_registrado character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD cobertura_reajuste boolean;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD apolice_modulo_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD apolice_subgrupo_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD modulo_nome_registrado character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD subgrupo_nome_registrado character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    CREATE INDEX ix_proposta_item_apolice_modulo_plano_id ON seguro.proposta_item (apolice_modulo_plano_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    CREATE INDEX ix_proposta_cobertura_apolice_modulo_cobertura_id ON seguro.proposta_cobertura (apolice_modulo_cobertura_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    CREATE UNIQUE INDEX ix_proposta_cobertura_public_id ON seguro.proposta_cobertura (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    CREATE INDEX ix_proposta_apolice_modulo_id ON seguro.proposta (apolice_modulo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    CREATE INDEX ix_proposta_apolice_subgrupo_id ON seguro.proposta (apolice_subgrupo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD CONSTRAINT fk_proposta_apolice_modulos_apolice_modulo_id FOREIGN KEY (apolice_modulo_id) REFERENCES seguro.apolice_modulo (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta ADD CONSTRAINT fk_proposta_apolice_subgrupos_apolice_subgrupo_id FOREIGN KEY (apolice_subgrupo_id) REFERENCES seguro.apolice_subgrupo (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_cobertura ADD CONSTRAINT fk_proposta_cobertura_apolice_modulo_coberturas_apolice_modulo FOREIGN KEY (apolice_modulo_cobertura_id) REFERENCES seguro.apolice_modulo_cobertura (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    ALTER TABLE seguro.proposta_item ADD CONSTRAINT fk_proposta_item_apolice_modulo_planos_apolice_modulo_plano_id FOREIGN KEY (apolice_modulo_plano_id) REFERENCES seguro.apolice_modulo_plano (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224713_PropostasOperacionaisModulo') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261111224713_PropostasOperacionaisModulo', '10.0.11');
    END IF;
END $EF$;
