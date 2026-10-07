START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    ALTER TABLE seguro.plano_cobertura ADD premio_conjuge numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    ALTER TABLE seguro.plano_cobertura ADD premio_titular numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    ALTER TABLE seguro.apolice_cobertura ADD premio_conjuge_override numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    ALTER TABLE seguro.apolice_cobertura ADD premio_titular_override numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    ALTER TABLE seguro.apolice_cobertura ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    CREATE UNIQUE INDEX ix_apolice_cobertura_public_id ON seguro.apolice_cobertura (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007000150_PremiosPlanoCoberturaEAjustesApolice') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261007000150_PremiosPlanoCoberturaEAjustesApolice', '10.0.11');
    END IF;
END $EF$;
COMMIT;
