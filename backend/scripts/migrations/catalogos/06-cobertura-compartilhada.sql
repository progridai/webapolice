START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura ADD cobertura_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura ALTER COLUMN cobertura_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    DROP INDEX seguro.ix_apolice_modulo_cobertura_apolice_modulo_plano_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura DROP CONSTRAINT ck_modulo_cobertura_nome;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura DROP COLUMN basica;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura DROP COLUMN nome;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura DROP COLUMN nome_reduzido;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura DROP COLUMN reajuste;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    CREATE UNIQUE INDEX ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_ ON seguro.apolice_modulo_cobertura (apolice_modulo_plano_id, cobertura_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    CREATE INDEX ix_apolice_modulo_cobertura_cobertura_id ON seguro.apolice_modulo_cobertura (cobertura_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    ALTER TABLE seguro.apolice_modulo_cobertura ADD CONSTRAINT fk_apolice_modulo_cobertura_cobertura_cobertura_id FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007012211_CompartilharCoberturaNoPlanoModulo') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261007012211_CompartilharCoberturaNoPlanoModulo', '10.0.11');
    END IF;
END $EF$;
COMMIT;
