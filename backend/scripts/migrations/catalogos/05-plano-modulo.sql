START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE TABLE seguro.apolice_modulo_plano (
        id bigint GENERATED ALWAYS AS IDENTITY,
        public_id uuid NOT NULL DEFAULT (gen_random_uuid()),
        apolice_modulo_id bigint NOT NULL,
        nome character varying(150) NOT NULL,
        ramo character varying(80),
        paga boolean,
        reajuste boolean,
        ativo boolean NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_apolice_modulo_plano PRIMARY KEY (id),
        CONSTRAINT ck_modulo_plano_nome CHECK (length(btrim(nome)) > 0),
        CONSTRAINT fk_apolice_modulo_plano_apolice_modulo_apolice_modulo_id FOREIGN KEY (apolice_modulo_id) REFERENCES seguro.apolice_modulo (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE TABLE seguro.apolice_modulo_cobertura (
        id bigint GENERATED ALWAYS AS IDENTITY,
        public_id uuid NOT NULL DEFAULT (gen_random_uuid()),
        apolice_modulo_plano_id bigint NOT NULL,
        nome character varying(150) NOT NULL,
        nome_reduzido character varying(30),
        basica character varying(50),
        reajuste boolean,
        premio_titular numeric(18,2) NOT NULL,
        premio_conjuge numeric(18,2) NOT NULL,
        ativo boolean NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_apolice_modulo_cobertura PRIMARY KEY (id),
        CONSTRAINT ck_modulo_cobertura_nome CHECK (length(btrim(nome)) > 0),
        CONSTRAINT ck_modulo_cobertura_premios CHECK (premio_titular >= 0 AND premio_conjuge >= 0),
        CONSTRAINT fk_apolice_modulo_cobertura_apolice_modulo_plano_apolice_modul FOREIGN KEY (apolice_modulo_plano_id) REFERENCES seguro.apolice_modulo_plano (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE INDEX ix_apolice_modulo_cobertura_apolice_modulo_plano_id ON seguro.apolice_modulo_cobertura (apolice_modulo_plano_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE UNIQUE INDEX ix_apolice_modulo_cobertura_public_id ON seguro.apolice_modulo_cobertura (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE UNIQUE INDEX ix_apolice_modulo_plano_apolice_modulo_id ON seguro.apolice_modulo_plano (apolice_modulo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    CREATE UNIQUE INDEX ix_apolice_modulo_plano_public_id ON seguro.apolice_modulo_plano (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261007010053_PlanoECoberturasExclusivosModuloApolice') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261007010053_PlanoECoberturasExclusivosModuloApolice', '10.0.11');
    END IF;
END $EF$;
COMMIT;
