BEGIN READ ONLY;
SET LOCAL statement_timeout='15s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Verificação restrita à base de teste.'; END IF;
  IF (SELECT count(*) FROM seguro."__EFMigrationsHistory" WHERE migration_id IN ('20261112194158_EncerramentoVigenciaDescartePropostas','20261112194300_DefinirDescartadaProposta'))<>2 THEN
    RAISE EXCEPTION 'Migrations do encerramento não instaladas integralmente.';
  END IF;
  IF (SELECT count(*) FROM information_schema.columns WHERE table_schema='seguro' AND table_name='proposta' AND is_nullable='YES' AND
      ((column_name IN ('descartada_em','cancelamento_solicitado_em') AND data_type='timestamp with time zone') OR (column_name='data_cancelamento_vigencia' AND data_type='date')))<>3 THEN
    RAISE EXCEPTION 'Metadados de encerramento ausentes ou incompatíveis.';
  END IF;
  IF (SELECT count(*) FROM seguro.proposta_status WHERE codigo IN ('DESCARTADA','CANCELADA') AND visivel_operacional AND finalizador AND NOT permite_movimentacao)<>2 THEN
    RAISE EXCEPTION 'Situações de encerramento incompatíveis.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM pg_trigger t JOIN pg_class c ON c.oid=t.tgrelid JOIN pg_namespace n ON n.oid=c.relnamespace
      WHERE n.nspname='seguro' AND c.relname='proposta_historico' AND t.tgname='webapolice_title_case' AND t.tgenabled='O'
      AND position('observacao' in encode(t.tgargs,'escape'))>0 AND position('motivo' in encode(t.tgargs,'escape'))>0) THEN
    RAISE EXCEPTION 'Padronização dos textos do histórico não instalada.';
  END IF;
END $$;
SELECT current_database() AS banco,count(*) AS propostas,
  count(*) FILTER(WHERE descartada_em IS NOT NULL) AS descartadas,
  count(*) FILTER(WHERE cancelamento_solicitado_em IS NOT NULL AND cancelada_em IS NULL) AS cancelamentos_programados,
  count(*) FILTER(WHERE cancelamento_solicitado_em IS NOT NULL AND cancelada_em IS NOT NULL) AS vigencias_canceladas
FROM seguro.proposta;
SELECT codigo,nome,permite_movimentacao,finalizador,visivel_operacional FROM seguro.proposta_status WHERE codigo IN ('RASCUNHO','CONFIRMADA','CANCELADA','DESCARTADA') ORDER BY codigo;
ROLLBACK;
