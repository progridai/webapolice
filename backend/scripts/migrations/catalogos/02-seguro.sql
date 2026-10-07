START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    ALTER TABLE seguro.plano ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    ALTER TABLE seguro.cobertura ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    ALTER TABLE seguro.apolice_subgrupo ADD convenio_cobranca_id bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    ALTER TABLE seguro.apolice_subgrupo ADD CONSTRAINT fk_apolice_subgrupo_convenio_cobranca FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE TABLE seguro.plano_cobertura (
        id bigint GENERATED ALWAYS AS IDENTITY,
        plano_id bigint NOT NULL,
        cobertura_id bigint NOT NULL,
        ativo boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_plano_cobertura PRIMARY KEY (id),
        CONSTRAINT fk_plano_cobertura_cobertura_cobertura_id FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura (id) ON DELETE RESTRICT,
        CONSTRAINT fk_plano_cobertura_plano_plano_id FOREIGN KEY (plano_id) REFERENCES seguro.plano (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE UNIQUE INDEX ix_plano_public_id ON seguro.plano (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE UNIQUE INDEX ix_cobertura_public_id ON seguro.cobertura (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE INDEX ix_apolice_subgrupo_convenio_cobranca_id ON seguro.apolice_subgrupo (convenio_cobranca_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE INDEX ix_plano_cobertura_cobertura_id ON seguro.plano_cobertura (cobertura_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    CREATE UNIQUE INDEX ix_plano_cobertura_plano_id_cobertura_id ON seguro.plano_cobertura (plano_id, cobertura_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261006224716_CatalogosCoberturasPlanosEConvenioSubgrupo', '10.0.11');
    END IF;
END $EF$;
COMMIT;
