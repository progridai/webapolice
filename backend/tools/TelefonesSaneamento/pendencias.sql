CREATE OR REPLACE VIEW legado.telefone_saneamento_pendencia_v1 AS
SELECT s.contato_origem_id,s.pessoa_id,p->>'trecho_original' trecho_original,
CASE
 WHEN p IS NULL THEN 'VAZIO'
 WHEN jsonb_array_length(p->'candidatos')>1 THEN 'INTERPRETACAO_AMBIGUA'
 WHEN p->>'trecho_original' ~ '[[:alpha:]]' THEN 'ANOTACAO_OU_TEXTO'
 WHEN length(regexp_replace(p->>'trecho_original','[^0-9]','','g')) IN (7,8,9) THEN 'SEM_DDD_OU_NUMERACAO_ANTIGA'
 WHEN p->>'trecho_original' LIKE '%0000%' THEN 'ZEROS_OU_PREENCHIMENTO'
 ELSE 'FORMATO_OU_NUMERACAO_INDETERMINADA'
END motivo
FROM legado.telefone_saneamento_v1 s
LEFT JOIN LATERAL jsonb_array_elements(s.propostas) p ON true
WHERE s.situacao='REVISAO' AND (p IS NULL OR jsonb_array_length(p->'candidatos')<>1);
SELECT motivo,count(*) trechos,count(DISTINCT contato_origem_id) origens
FROM legado.telefone_saneamento_pendencia_v1 GROUP BY 1 ORDER BY 2 DESC;
