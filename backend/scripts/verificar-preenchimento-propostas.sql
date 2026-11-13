BEGIN READ ONLY;
SET LOCAL statement_timeout='15s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Verificação restrita à base de teste.'; END IF;
  IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE migration_id='20261112104220_PreenchimentoRascunhoPropostas') THEN
    RAISE EXCEPTION 'Migration de preenchimento ausente.';
  END IF;
  IF NOT EXISTS(SELECT 1 FROM pg_indexes WHERE schemaname='seguro' AND tablename='proposta_beneficiario' AND indexname='ix_proposta_beneficiario_public_id' AND indexdef LIKE 'CREATE UNIQUE INDEX%')
    OR EXISTS(SELECT 1 FROM seguro.proposta_beneficiario WHERE public_id IS NULL) THEN
    RAISE EXCEPTION 'Identidade pública de beneficiários não instalada.';
  END IF;
  IF (SELECT count(*) FROM pg_trigger t JOIN pg_class c ON c.oid=t.tgrelid JOIN pg_namespace n ON n.oid=c.relnamespace
      WHERE n.nspname='seguro' AND t.tgname='webapolice_title_case' AND t.tgenabled='O'
      AND ((c.relname='proposta_beneficiario' AND position('parentesco_registrado' in encode(t.tgargs,'escape'))>0)
        OR (c.relname='proposta_cobertura' AND position('justificativa_ajuste_premio' in encode(t.tgargs,'escape'))>0)))<>2 THEN
    RAISE EXCEPTION 'Title Case dos campos de preenchimento não instalado.';
  END IF;
  IF public.webapolice_title_case('FILHA DO TITULAR')<>'Filha do Titular'
    OR public.webapolice_title_case('AJUSTE DE PROTEÇÃO')<>'Ajuste de Proteção' THEN
    RAISE EXCEPTION 'Capitalização divergente.';
  END IF;
END $$;
SELECT current_database() AS banco, count(*) AS propostas, count(*) FILTER(WHERE revisao_preenchimento>0) AS preenchidas FROM seguro.proposta;
SELECT count(*) AS beneficiarios FROM seguro.proposta_beneficiario;
ROLLBACK;
