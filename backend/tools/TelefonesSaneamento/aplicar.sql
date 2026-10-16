-- Execute after plano.sql in the same transaction; abort on changed source rows.
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_alteracao_v1 (
 contato_id bigint PRIMARY KEY,
 antes jsonb,
 depois jsonb NOT NULL
);
DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s
 LEFT JOIN core.pessoa_contato c ON c.id=s.contato_origem_id
 WHERE s.situacao='APTO' AND (c.id IS NULL OR to_jsonb(c)<>s.original))
 THEN RAISE EXCEPTION 'Contato alterado desde o plano; revisar antes de aplicar'; END IF;
END $$;
CREATE TEMP TABLE saneamento_numeros ON COMMIT DROP AS
SELECT DISTINCT s.contato_origem_id,s.pessoa_id,p->'candidatos'->>0 numero,
CASE WHEN length(p->'candidatos'->>0)=10 THEN 'TELEFONE' ELSE 'CELULAR' END tipo,
(s.original->>'principal')::boolean era_principal,(p->>'ordem')::int ordem
FROM legado.telefone_saneamento_v1 s CROSS JOIN LATERAL jsonb_array_elements(s.propostas) p
WHERE s.situacao='APTO';
CREATE INDEX ON saneamento_numeros(pessoa_id,numero);
CREATE INDEX ON saneamento_numeros(contato_origem_id);
ANALYZE saneamento_numeros;
CREATE TEMP TABLE saneamento_antes ON COMMIT DROP AS
SELECT c.id,to_jsonb(c) dados FROM core.pessoa_contato c
JOIN (SELECT DISTINCT contato_origem_id FROM saneamento_numeros) n ON n.contato_origem_id=c.id
UNION
SELECT c.id,to_jsonb(c) dados FROM core.pessoa_contato c
JOIN (SELECT DISTINCT pessoa_id,tipo,numero FROM saneamento_numeros) n ON n.pessoa_id=c.pessoa_id
 AND n.tipo=c.tipo_contato AND n.numero=regexp_replace(c.valor,'[^0-9]','','g')
WHERE c.ativo;
CREATE UNIQUE INDEX ON saneamento_antes(id);
UPDATE core.pessoa_contato c SET ativo=false,principal=false
FROM legado.telefone_saneamento_v1 s WHERE s.situacao='APTO' AND c.id=s.contato_origem_id;
CREATE TEMP TABLE saneamento_unicos ON COMMIT DROP AS
SELECT pessoa_id,numero,tipo,bool_or(era_principal) era_principal,min(contato_origem_id) origem,min(ordem) ordem
FROM saneamento_numeros GROUP BY pessoa_id,numero,tipo;
INSERT INTO core.pessoa_contato(pessoa_id,tipo_contato,valor,valor_normalizado,principal,ativo,created_at)
SELECT n.pessoa_id,n.tipo,
'('||substr(n.numero,1,2)||') '||substr(n.numero,3,length(n.numero)-6)||'-'||right(n.numero,4),
n.numero,false,true,now()
FROM saneamento_unicos n
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa_contato c WHERE c.pessoa_id=n.pessoa_id
AND c.ativo AND c.tipo_contato=n.tipo AND regexp_replace(c.valor,'[^0-9]','','g')=n.numero
AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s WHERE s.contato_origem_id=c.id AND s.situacao='REVISAO'));
INSERT INTO legado.telefone_saneamento_destino_v1(contato_origem_id,contato_destino_id,numero,classificacao)
SELECT DISTINCT ON(n.contato_origem_id,n.numero) n.contato_origem_id,c.id,n.numero,
CASE n.tipo WHEN 'TELEFONE' THEN 'FIXO' ELSE 'CELULAR' END
FROM saneamento_numeros n JOIN core.pessoa_contato c ON c.pessoa_id=n.pessoa_id AND c.ativo
AND c.tipo_contato=n.tipo AND regexp_replace(c.valor,'[^0-9]','','g')=n.numero
AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s WHERE s.contato_origem_id=c.id AND s.situacao='REVISAO')
ORDER BY n.contato_origem_id,n.numero,c.principal DESC,c.id;
-- Transfer an existing primary choice only; do not override another active primary.
WITH preferred AS (
SELECT DISTINCT ON(n.pessoa_id) n.pessoa_id,d.contato_destino_id
FROM saneamento_numeros n JOIN legado.telefone_saneamento_destino_v1 d USING(contato_origem_id,numero)
WHERE n.era_principal AND NOT EXISTS(SELECT 1 FROM core.pessoa_contato c WHERE c.pessoa_id=n.pessoa_id AND c.ativo AND c.principal)
ORDER BY n.pessoa_id,n.contato_origem_id,n.ordem,d.contato_destino_id
)
UPDATE core.pessoa_contato c SET principal=true FROM preferred p WHERE c.id=p.contato_destino_id;
INSERT INTO legado.telefone_saneamento_alteracao_v1(contato_id,antes,depois)
SELECT c.id,a.dados,to_jsonb(c) FROM core.pessoa_contato c JOIN saneamento_antes a ON a.id=c.id
WHERE a.dados<>to_jsonb(c)
UNION ALL
SELECT c.id,NULL,to_jsonb(c) FROM core.pessoa_contato c
JOIN (SELECT DISTINCT d.contato_destino_id FROM legado.telefone_saneamento_destino_v1 d
 JOIN saneamento_numeros n USING(contato_origem_id,numero)) d ON d.contato_destino_id=c.id
LEFT JOIN saneamento_antes a ON a.id=c.id WHERE a.id IS NULL
ON CONFLICT(contato_id) DO NOTHING;
DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM saneamento_numeros n WHERE NOT EXISTS(
 SELECT 1 FROM legado.telefone_saneamento_destino_v1 d WHERE d.contato_origem_id=n.contato_origem_id AND d.numero=n.numero))
 THEN RAISE EXCEPTION 'Numero sem destino'; END IF;
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 s JOIN core.pessoa_contato c ON c.id=s.contato_origem_id
 WHERE s.situacao='REVISAO' AND to_jsonb(c)<>s.original)
 THEN RAISE EXCEPTION 'Contato pendente foi alterado'; END IF;
END $$;
UPDATE legado.telefone_saneamento_v1 SET situacao='APLICADO' WHERE situacao='APTO';
SELECT situacao,count(*) origens FROM legado.telefone_saneamento_v1 GROUP BY 1 ORDER BY 1;
SELECT classificacao,count(*) vinculos,count(DISTINCT contato_destino_id) contatos FROM legado.telefone_saneamento_destino_v1 GROUP BY 1;
SELECT count(DISTINCT contato_origem_id) origens_processadas_nesta_execucao,
count(*) numeros_processados_nesta_execucao FROM saneamento_numeros;
