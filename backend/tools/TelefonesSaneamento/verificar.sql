DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s JOIN core.pessoa_contato c ON c.id=s.contato_origem_id
 WHERE s.situacao='APLICADO' AND (c.ativo OR c.principal OR (to_jsonb(c)-'ativo'-'principal')<>(s.original-'ativo'-'principal')))
 THEN RAISE EXCEPTION 'Origem aplicada diverge do esperado'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s LEFT JOIN core.pessoa_contato c ON c.id=s.contato_origem_id
 WHERE s.situacao='REVISAO' AND (c.id IS NULL OR to_jsonb(c)<>s.original))
 THEN RAISE EXCEPTION 'Pendencia foi modificada'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_destino_v1 d JOIN core.pessoa_contato c ON c.id=d.contato_destino_id
 WHERE NOT c.ativo OR c.valor_normalizado<>d.numero OR c.tipo_contato<>CASE d.classificacao WHEN 'FIXO' THEN 'TELEFONE' ELSE 'CELULAR' END)
 THEN RAISE EXCEPTION 'Destino invalido'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_alteracao_v1 a LEFT JOIN core.pessoa_contato c ON c.id=a.contato_id
 WHERE c.id IS NULL OR to_jsonb(c)<>a.depois)
 THEN RAISE EXCEPTION 'Imagem posterior divergente'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 WHERE situacao='APTO')
 THEN RAISE EXCEPTION 'Restam origens aptas nao aplicadas'; END IF;
END $$;
SELECT 'Verificacao aprovada' resultado;
SELECT current_database() banco,tipo_contato,ativo,count(*) quantidade FROM core.pessoa_contato GROUP BY 1,2,3 ORDER BY 2,3;
SELECT situacao,count(*) origens FROM legado.telefone_saneamento_v1 GROUP BY 1 ORDER BY 1;
SELECT classificacao,count(DISTINCT contato_destino_id) contatos FROM legado.telefone_saneamento_destino_v1 GROUP BY 1 ORDER BY 1;
SELECT count(*) alteracoes, count(*) FILTER(WHERE antes IS NULL) contatos_criados FROM legado.telefone_saneamento_alteracao_v1;
