\set ON_ERROR_STOP on
BEGIN;
SET LOCAL lock_timeout='10s';
SET LOCAL statement_timeout='30s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Instalação restrita a webapolice_teste.'; END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE migration_id='20261111213321_ExclusaoHierarquicaVinculosApolice')
    OR NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE migration_id='20261111220000_AdicionarExclusaoVinculosApolice') THEN
    RAISE EXCEPTION 'Baseline de hierarquia não confirmado. Não aplicar migrations gerais.';
  END IF;
  IF EXISTS(SELECT 1 FROM seguro.proposta) OR EXISTS(SELECT 1 FROM seguro.proposta_item) OR EXISTS(SELECT 1 FROM seguro.proposta_cobertura) THEN
    RAISE EXCEPTION 'Instalador inicial exige tabelas de Propostas vazias. Revisar registros antes de aplicar.';
  END IF;
END $$;
CREATE TEMP TABLE propostas_guardas AS
  SELECT 'perfis' AS conjunto,md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) AS assinatura FROM seguranca.perfil p
  UNION ALL SELECT 'perfil_permissao',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.perfil_permissao p
  UNION ALL SELECT 'usuario_perfil',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.usuario_perfil p;
\ir 01-identidade-itens.sql
\ir 02-condicoes-modulo.sql
\ir 03-rascunho.sql
\ir 04-permissoes.sql
DO $$ BEGIN
  IF (SELECT assinatura FROM propostas_guardas WHERE conjunto='perfis') IS DISTINCT FROM
      (SELECT md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.perfil p)
    OR (SELECT assinatura FROM propostas_guardas WHERE conjunto='perfil_permissao') IS DISTINCT FROM
      (SELECT md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.perfil_permissao p)
    OR (SELECT assinatura FROM propostas_guardas WHERE conjunto='usuario_perfil') IS DISTINCT FROM
      (SELECT md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.usuario_perfil p) THEN
    RAISE EXCEPTION 'Perfis ou concessões divergiram. Revertendo instalação.';
  END IF;
  IF EXISTS(SELECT 1 FROM seguro.proposta) OR EXISTS(SELECT 1 FROM seguro.proposta_item) OR EXISTS(SELECT 1 FROM seguro.proposta_cobertura) THEN
    RAISE EXCEPTION 'Registros de Propostas divergiram. Revertendo instalação.';
  END IF;
END $$;
DROP TABLE propostas_guardas;
COMMIT;
