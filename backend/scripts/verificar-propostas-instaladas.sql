BEGIN READ ONLY;
SET LOCAL statement_timeout='15s';
DO $$ BEGIN
  IF current_database()<>'webapolice_teste' THEN RAISE EXCEPTION 'Verificação restrita à base de teste.'; END IF;
  IF (SELECT count(*) FROM pg_trigger WHERE NOT tgisinternal AND tgenabled='O' AND tgname IN ('contexto_proposta','proteger_contexto_proposta')) <> 7 THEN
    RAISE EXCEPTION 'Triggers de contexto de Propostas ausentes ou desabilitados.';
  END IF;
  IF (SELECT count(*) FROM pg_trigger t JOIN pg_class c ON c.oid=t.tgrelid JOIN pg_namespace n ON n.oid=c.relnamespace
      WHERE n.nspname='seguro' AND t.tgname='webapolice_title_case' AND t.tgenabled='O'
      AND ((c.relname='proposta' AND position('modulo_nome_registrado' in encode(t.tgargs,'escape'))>0 AND position('subgrupo_nome_registrado' in encode(t.tgargs,'escape'))>0)
        OR (c.relname='proposta_item' AND position('plano_nome_registrado' in encode(t.tgargs,'escape'))>0)
        OR (c.relname='proposta_cobertura' AND position('cobertura_nome_registrado' in encode(t.tgargs,'escape'))>0 AND position('cobertura_nome_reduzido_registrado' in encode(t.tgargs,'escape'))>0)))<>3 THEN
    RAISE EXCEPTION 'Classificação Title Case dos novos campos não foi instalada integralmente.';
  END IF;
  IF public.webapolice_title_case('AÇÃO DE PROTEÇÃO')<>'Ação de Proteção' THEN RAISE EXCEPTION 'Capitalização com acentos divergente.'; END IF;
END $$;
SELECT codigo,nome,permite_movimentacao,finalizador,visivel_operacional FROM seguro.proposta_status WHERE codigo='RASCUNHO';
SELECT m.codigo AS modulo,m.ativo AND m.habilitado AS modulo_disponivel,r.codigo AS recurso,r.ativo AND r.habilitado AS recurso_disponivel,count(*) AS permissoes
FROM seguranca.permissao p JOIN seguranca.recurso r ON r.id=p.recurso_id JOIN seguranca.modulo m ON m.id=r.modulo_id
WHERE p.codigo IN ('propostas.visualizar','propostas.inserir','propostas.alterar') GROUP BY m.codigo,m.ativo,m.habilitado,r.codigo,r.ativo,r.habilitado;
SELECT count(*) AS propostas FROM seguro.proposta;
ROLLBACK;
