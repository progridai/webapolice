BEGIN READ ONLY;
SET LOCAL statement_timeout='15s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Verificação restrita à base de teste.'; END IF;
  IF (SELECT count(*) FROM seguro."__EFMigrationsHistory" WHERE migration_id IN ('20261112183644_GestaoAgendamentosPropostas','20261112183700_DefinirCanceladaProposta'))<>2 THEN
    RAISE EXCEPTION 'Migrations dos agendamentos não instaladas integralmente.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema='seguro' AND table_name='proposta' AND column_name='cancelada_em' AND is_nullable='YES' AND data_type='timestamp with time zone') THEN
    RAISE EXCEPTION 'Metadado de cancelamento ausente ou incompatível.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='CANCELADA' AND visivel_operacional AND finalizador AND NOT permite_movimentacao) THEN
    RAISE EXCEPTION 'Situação CANCELADA incompatível com esta etapa.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM pg_trigger t JOIN pg_class c ON c.oid=t.tgrelid JOIN pg_namespace n ON n.oid=c.relnamespace
      WHERE n.nspname='seguro' AND c.relname='proposta_historico' AND t.tgname='webapolice_title_case' AND t.tgenabled='O'
      AND position('observacao' in encode(t.tgargs,'escape'))>0 AND position('motivo' in encode(t.tgargs,'escape'))>0) THEN
    RAISE EXCEPTION 'Padronização dos textos do histórico não instalada.';
  END IF;
END $$;
SELECT current_database() AS banco,count(*) AS propostas,
  count(*) FILTER(WHERE cancelada_em IS NOT NULL) AS agendamentos_cancelados,
  count(*) FILTER(WHERE confirmada_em IS NOT NULL AND cancelada_em IS NULL AND NOT vigente AND substituida_em IS NULL) AS agendadas_ou_pendentes
FROM seguro.proposta;
SELECT codigo,nome,permite_movimentacao,finalizador,visivel_operacional FROM seguro.proposta_status WHERE codigo IN ('RASCUNHO','CONFIRMADA','CANCELADA') ORDER BY codigo;
ROLLBACK;
