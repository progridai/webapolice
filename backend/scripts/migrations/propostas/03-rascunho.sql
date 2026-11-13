
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224800_DefinirRascunhoProposta') THEN
    LOCK TABLE seguro.proposta_status IN SHARE ROW EXCLUSIVE MODE;
    DO $$ DECLARE novo_id smallint; BEGIN
      IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO') THEN
        SELECT COALESCE(MAX(id)::int,0)+1 INTO novo_id FROM seguro.proposta_status;
        INSERT INTO seguro.proposta_status(id,codigo,nome,permite_movimentacao,visivel_operacional,finalizador)
          VALUES(novo_id,'RASCUNHO','Rascunho',false,true,false);
      ELSIF EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='RASCUNHO' AND (permite_movimentacao OR NOT visivel_operacional OR finalizador)) THEN
        RAISE EXCEPTION 'O código RASCUNHO já existe com configuração incompatível. Revisar o catálogo antes de continuar.';
      END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261111224800_DefinirRascunhoProposta') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261111224800_DefinirRascunhoProposta', '10.0.11');
    END IF;
END $EF$;
