\set ON_ERROR_STOP on
START TRANSACTION;

-- Instalação específica; SQL EF gerado somente entre DefinirRascunhoProposta e PreenchimentoRascunhoPropostas.
SET LOCAL lock_timeout='10s';
SET LOCAL statement_timeout='60s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Instalação restrita a webapolice_teste.'; END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE migration_id='20261111224800_DefinirRascunhoProposta') THEN
    RAISE EXCEPTION 'Baseline de Propostas ausente. Não aplicar migrations gerais.';
  END IF;
END $$;
LOCK TABLE seguro.proposta,seguro.proposta_item,seguro.proposta_cobertura,seguro.proposta_beneficiario IN SHARE ROW EXCLUSIVE MODE;
CREATE TEMP VIEW preenchimento_assinaturas AS
  SELECT 'proposta' AS conjunto,md5(coalesce(string_agg((to_jsonb(p)-'revisao_preenchimento')::text,'|' ORDER BY id),'')) AS assinatura FROM seguro.proposta p
  UNION ALL SELECT 'itens',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_item p
  UNION ALL SELECT 'coberturas',md5(coalesce(string_agg((to_jsonb(p)-'premio_ajustado'-'premio_titular_inicial'-'premio_conjuge_inicial'-'justificativa_ajuste_premio')::text,'|' ORDER BY id),'')) FROM seguro.proposta_cobertura p
  UNION ALL SELECT 'beneficiarios',md5(coalesce(string_agg((to_jsonb(p)-'public_id'-'parentesco_registrado')::text,'|' ORDER BY id),'')) FROM seguro.proposta_beneficiario p
  UNION ALL SELECT 'perfis',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguranca.perfil p
  UNION ALL SELECT 'perfil_permissao',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.perfil_permissao p
  UNION ALL SELECT 'usuario_perfil',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.usuario_perfil p;
CREATE TEMP TABLE preenchimento_antes AS SELECT * FROM preenchimento_assinaturas;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_cobertura ADD justificativa_ajuste_premio character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_cobertura ADD premio_ajustado boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_cobertura ADD premio_conjuge_inicial numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_cobertura ADD premio_titular_inicial numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_beneficiario ALTER COLUMN legado_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_beneficiario ADD parentesco_registrado character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta_beneficiario ADD public_id uuid NOT NULL DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    ALTER TABLE seguro.proposta ADD revisao_preenchimento integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    CREATE UNIQUE INDEX ix_proposta_beneficiario_public_id ON seguro.proposta_beneficiario (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112104220_PreenchimentoRascunhoPropostas') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261112104220_PreenchimentoRascunhoPropostas', '10.0.11');
    END IF;
END $EF$;

DO $$ BEGIN
  IF EXISTS(SELECT 1 FROM preenchimento_antes a FULL JOIN preenchimento_assinaturas d USING(conjunto) WHERE a.assinatura IS DISTINCT FROM d.assinatura) THEN
    RAISE EXCEPTION 'Dados anteriores ou concessões divergiram. Revertendo a instalação.';
  END IF;
END $$;
DROP VIEW preenchimento_assinaturas;
DROP TABLE preenchimento_antes;

COMMIT;
