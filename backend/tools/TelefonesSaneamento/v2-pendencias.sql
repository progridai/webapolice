CREATE OR REPLACE VIEW legado.telefone_saneamento_pendencia_v2 AS
SELECT s.contato_origem_id,s.pessoa_id,d.ordem,d.contato_destino_id,d.situacao,
c.tipo_contato,c.valor,c.valor_normalizado,p->>'trecho' trecho_original
FROM legado.telefone_saneamento_v2 s
JOIN legado.telefone_saneamento_destino_v2 d USING(contato_origem_id)
JOIN core.pessoa_contato c ON c.id=d.contato_destino_id
JOIN LATERAL jsonb_array_elements(s.propostas) p ON (p->>'ordem')::int=d.ordem
WHERE s.situacao='APLICADO' AND d.situacao<>'COMPLETO';
-- Supersede the operational view only; V1 evidence and its historical situation remain immutable.
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
WHERE s.situacao='REVISAO' AND (p IS NULL OR jsonb_array_length(p->'candidatos')<>1)
AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 v2 WHERE v2.contato_origem_id=s.contato_origem_id AND v2.situacao='APLICADO');
SELECT situacao,count(*) trechos_pendentes FROM legado.telefone_saneamento_pendencia_v2 GROUP BY 1;
