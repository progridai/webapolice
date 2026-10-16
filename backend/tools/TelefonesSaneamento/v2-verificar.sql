DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 s WHERE s.situacao<>'APLICADO')
 THEN RAISE EXCEPTION 'V2 nao aplicada integralmente'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 s CROSS JOIN LATERAL jsonb_array_elements(s.propostas) p
 LEFT JOIN legado.telefone_saneamento_destino_v2 d ON d.contato_origem_id=s.contato_origem_id AND d.ordem=(p->>'ordem')::int
 LEFT JOIN core.pessoa_contato c ON c.id=d.contato_destino_id
 WHERE c.id IS NULL OR NOT c.ativo OR c.tipo_contato<>p->>'tipo'
 OR (p->>'situacao'='COMPLETO' AND c.valor_normalizado<>p->>'normalizado')
 OR (p->>'situacao'<>'COMPLETO' AND c.valor<>p->>'valor'))
 THEN RAISE EXCEPTION 'Destino V2 divergente'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_alteracao_v2 a LEFT JOIN core.pessoa_contato c ON c.id=a.contato_id
 WHERE c.id IS NULL OR to_jsonb(c)<>a.depois)
 THEN RAISE EXCEPTION 'Imagem posterior V2 divergente'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_alteracao_v2 a WHERE a.antes IS NOT NULL
 AND ((a.antes->>'tipo_contato') NOT IN ('TELEFONE','CELULAR') OR (a.antes-'ativo'-'principal')<>(a.depois-'ativo'-'principal')))
 THEN RAISE EXCEPTION 'V2 alterou conteudo original ou contato nao telefonico'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 s WHERE (s.original->>'principal')::boolean
 AND NOT EXISTS(SELECT 1 FROM core.pessoa_contato c WHERE c.pessoa_id=s.pessoa_id AND c.ativo AND c.principal AND c.tipo_contato IN ('TELEFONE','CELULAR')))
 THEN RAISE EXCEPTION 'Pessoa perdeu telefone principal'; END IF;
END $$;
SELECT 'V2 verificada' resultado;
SELECT d.situacao,c.tipo_contato,count(*) vinculos,count(DISTINCT c.id) contatos
FROM legado.telefone_saneamento_destino_v2 d JOIN core.pessoa_contato c ON c.id=d.contato_destino_id GROUP BY 1,2 ORDER BY 1,2;
SELECT count(*) alteracoes,count(*) FILTER(WHERE antes IS NULL) contatos_criados FROM legado.telefone_saneamento_alteracao_v2;
SELECT count(*) origens FROM legado.telefone_saneamento_v2;
