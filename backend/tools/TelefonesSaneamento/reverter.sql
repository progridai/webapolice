SELECT pg_advisory_xact_lock(20261015,2);
LOCK TABLE core.pessoa_contato IN SHARE ROW EXCLUSIVE MODE;
DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_alteracao_v1 a
 LEFT JOIN core.pessoa_contato c ON c.id=a.contato_id WHERE c.id IS NULL OR to_jsonb(c)<>a.depois)
 THEN RAISE EXCEPTION 'Houve edicoes posteriores; reversao automatica bloqueada'; END IF;
END $$;
DELETE FROM legado.telefone_saneamento_destino_v1;
DELETE FROM core.pessoa_contato c USING legado.telefone_saneamento_alteracao_v1 a
WHERE a.antes IS NULL AND c.id=a.contato_id;
UPDATE core.pessoa_contato c SET ativo=(a.antes->>'ativo')::boolean,principal=(a.antes->>'principal')::boolean
FROM legado.telefone_saneamento_alteracao_v1 a WHERE a.antes IS NOT NULL AND c.id=a.contato_id;
DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_alteracao_v1 a JOIN core.pessoa_contato c ON c.id=a.contato_id
 WHERE a.antes IS NOT NULL AND to_jsonb(c)<>a.antes)
 THEN RAISE EXCEPTION 'Reversao diverge da imagem original'; END IF;
END $$;
UPDATE legado.telefone_saneamento_v1 SET situacao='APTO' WHERE situacao='APLICADO';
SELECT count(*) alteracoes_revertidas FROM legado.telefone_saneamento_alteracao_v1;
DELETE FROM legado.telefone_saneamento_alteracao_v1;
