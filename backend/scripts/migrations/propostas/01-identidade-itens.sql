
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261023000100_AdicionarPublicIdPropostaItem') THEN
    ALTER TABLE seguro.proposta ALTER COLUMN legado_id TYPE integer;
    ALTER TABLE seguro.proposta ALTER COLUMN legado_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261023000100_AdicionarPublicIdPropostaItem') THEN
    ALTER TABLE seguro.proposta_item ALTER COLUMN legado_id TYPE integer;
    ALTER TABLE seguro.proposta_item ALTER COLUMN legado_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261023000100_AdicionarPublicIdPropostaItem') THEN
    ALTER TABLE seguro.proposta_item ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261023000100_AdicionarPublicIdPropostaItem') THEN
    CREATE UNIQUE INDEX ix_proposta_item_public_id ON seguro.proposta_item (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261023000100_AdicionarPublicIdPropostaItem') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261023000100_AdicionarPublicIdPropostaItem', '10.0.11');
    END IF;
END $EF$;
