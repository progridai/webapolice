BEGIN READ ONLY;
SET LOCAL statement_timeout='15s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Verificação restrita à base de teste.'; END IF;
  IF (SELECT count(*) FROM seguro."__EFMigrationsHistory" WHERE migration_id IN ('20261112111100_ConfirmacaoVigenciaPropostas','20261112111200_DefinirConfirmadaProposta'))<>2 THEN
    RAISE EXCEPTION 'Migrations de confirmação não instaladas integralmente.';
  END IF;
  IF (SELECT count(*) FROM information_schema.columns WHERE table_schema='seguro' AND table_name='proposta' AND is_nullable='YES'
      AND column_name IN ('confirmada_em','substituida_em','data_inicio_vigencia','data_fim_vigencia','motivo_pendencia_vigencia','pendencia_vigencia_em'))<>6 THEN
    RAISE EXCEPTION 'Metadados de confirmação/vigência ausentes.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM pg_indexes WHERE schemaname='seguro' AND tablename='proposta' AND indexname='ux_proposta_nativa_vigente_vida' AND indexdef LIKE 'CREATE UNIQUE INDEX%') THEN
    RAISE EXCEPTION 'Unicidade de vigência nativa não instalada.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro.proposta_status WHERE codigo='CONFIRMADA' AND visivel_operacional AND NOT finalizador AND NOT permite_movimentacao) THEN
    RAISE EXCEPTION 'Situação CONFIRMADA incompatível com esta etapa.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM pg_trigger t JOIN pg_class c ON c.oid=t.tgrelid JOIN pg_namespace n ON n.oid=c.relnamespace
      WHERE n.nspname='seguro' AND c.relname='proposta' AND t.tgname='webapolice_title_case' AND t.tgenabled='O'
      AND position('motivo_pendencia_vigencia' in encode(t.tgargs,'escape'))>0)
    OR position('to_jsonb(OLD)' in pg_get_functiondef('public.webapolice_normalizar_textos()'::regprocedure))=0 THEN
    RAISE EXCEPTION 'Title Case do motivo ou preservação de textos intocados não instalada.';
  END IF;
  IF public.webapolice_title_case('AÇÃO DE PROTEÇÃO')<>'Ação de Proteção' THEN RAISE EXCEPTION 'Capitalização divergente.'; END IF;
END $$;
SELECT current_database() AS banco,count(*) AS propostas,count(*) FILTER(WHERE confirmada_em IS NOT NULL) AS confirmadas,
  count(*) FILTER(WHERE confirmada_em IS NOT NULL AND NOT vigente AND substituida_em IS NULL) AS agendadas_ou_pendentes,
  count(*) FILTER(WHERE motivo_pendencia_vigencia IS NOT NULL) AS com_pendencia
FROM seguro.proposta;
SELECT codigo,nome,permite_movimentacao,finalizador,visivel_operacional FROM seguro.proposta_status WHERE codigo IN ('RASCUNHO','CONFIRMADA') ORDER BY codigo;
ROLLBACK;
