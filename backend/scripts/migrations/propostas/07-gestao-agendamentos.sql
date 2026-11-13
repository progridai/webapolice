\set ON_ERROR_STOP on
BEGIN;
SET LOCAL lock_timeout='10s';
SET LOCAL statement_timeout='60s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Instalação restrita a webapolice_teste.'; END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE migration_id='20261112111200_DefinirConfirmadaProposta') THEN
    RAISE EXCEPTION 'Baseline da confirmação ausente. Não aplicar migrations gerais.';
  END IF;
END $$;
LOCK TABLE seguro.proposta,seguro.proposta_item,seguro.proposta_cobertura,seguro.proposta_beneficiario,seguro.proposta_historico,seguro.proposta_movimento,seguro.proposta_status IN SHARE ROW EXCLUSIVE MODE;
CREATE TEMP VIEW agendamentos_assinaturas AS
  SELECT 'propostas' AS conjunto,md5(coalesce(string_agg((to_jsonb(p)-'cancelada_em')::text,'|' ORDER BY id),'')) AS assinatura FROM seguro.proposta p
  UNION ALL SELECT 'itens',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_item p
  UNION ALL SELECT 'coberturas',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_cobertura p
  UNION ALL SELECT 'beneficiarios',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_beneficiario p
  UNION ALL SELECT 'historico',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_historico p
  UNION ALL SELECT 'movimentos',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguro.proposta_movimento p
  UNION ALL SELECT 'perfis',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY id),'')) FROM seguranca.perfil p
  UNION ALL SELECT 'perfil_permissao',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.perfil_permissao p
  UNION ALL SELECT 'usuario_perfil',md5(coalesce(string_agg(to_jsonb(p)::text,'|' ORDER BY to_jsonb(p)::text),'')) FROM seguranca.usuario_perfil p;
CREATE TEMP TABLE agendamentos_antes AS SELECT * FROM agendamentos_assinaturas;
CREATE TEMP TABLE agendamentos_status_antes AS SELECT id,to_jsonb(s)::text AS registro FROM seguro.proposta_status s;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112183644_GestaoAgendamentosPropostas') THEN
    ALTER TABLE seguro.proposta ADD cancelada_em timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112183644_GestaoAgendamentosPropostas') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261112183644_GestaoAgendamentosPropostas', '10.0.11');
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112183700_DefinirCanceladaProposta') THEN
    LOCK TABLE seguro.proposta_status IN SHARE ROW EXCLUSIVE MODE;
    DO $$ DECLARE novo_id smallint; BEGIN
      IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='CANCELADA') THEN
        SELECT COALESCE(MAX(id)::int,0)+1 INTO novo_id FROM seguro.proposta_status;
        INSERT INTO seguro.proposta_status(id,codigo,nome,permite_movimentacao,visivel_operacional,finalizador)
          VALUES(novo_id,'CANCELADA','Cancelada',false,true,true);
      ELSIF EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='CANCELADA' AND (permite_movimentacao OR NOT visivel_operacional OR NOT finalizador)) THEN
        RAISE EXCEPTION 'O código CANCELADA possui configuração incompatível. Revisar o catálogo antes de continuar.';
      END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE "migration_id" = '20261112183700_DefinirCanceladaProposta') THEN
    INSERT INTO seguro."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261112183700_DefinirCanceladaProposta', '10.0.11');
    END IF;
END $EF$;

DO $$ BEGIN
  IF EXISTS(SELECT 1 FROM agendamentos_antes a FULL JOIN agendamentos_assinaturas d USING(conjunto) WHERE a.assinatura IS DISTINCT FROM d.assinatura)
    OR EXISTS(SELECT 1 FROM agendamentos_status_antes a LEFT JOIN seguro.proposta_status s USING(id) WHERE a.registro IS DISTINCT FROM to_jsonb(s)::text) THEN
    RAISE EXCEPTION 'Dados anteriores, catálogo existente ou concessões divergiram. Revertendo a instalação.';
  END IF;
END $$;
DROP VIEW agendamentos_assinaturas;
DROP TABLE agendamentos_antes;
DROP TABLE agendamentos_status_antes;
COMMIT;
