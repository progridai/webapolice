START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE "migration_id" = '20261117160039_AdicionarAtalhosHomeUsuario') THEN
    ALTER TABLE seguranca.usuario ADD atalhos_home text[];
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE "migration_id" = '20261117160039_AdicionarAtalhosHomeUsuario') THEN
    INSERT INTO seguranca."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261117160039_AdicionarAtalhosHomeUsuario', '10.0.11');
    END IF;
END $EF$;
COMMIT;
