DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 s LEFT JOIN core.pessoa_contato c ON c.id=s.contato_origem_id
 WHERE s.situacao='PLANEJADO' AND (c.id IS NULL OR to_jsonb(c)<>s.original))
 THEN RAISE EXCEPTION 'Origem V2 mudou desde o planejamento'; END IF;
END $$;
CREATE TEMP TABLE telefone_v2_partes ON COMMIT DROP AS
SELECT s.contato_origem_id,s.pessoa_id,(p->>'ordem')::int ordem,p->>'valor' valor,
p->>'normalizado' normalizado,p->>'tipo' tipo,p->>'situacao' situacao,
(s.original->>'principal')::boolean era_principal
FROM legado.telefone_saneamento_v2 s CROSS JOIN LATERAL jsonb_array_elements(s.propostas) p
WHERE s.situacao='PLANEJADO';
CREATE INDEX ON telefone_v2_partes(pessoa_id,tipo,normalizado);
CREATE INDEX ON telefone_v2_partes(contato_origem_id,ordem);
ANALYZE telefone_v2_partes;
CREATE TEMP TABLE telefone_v2_aposentar ON COMMIT DROP AS
SELECT s.contato_origem_id FROM legado.telefone_saneamento_v2 s
WHERE s.situacao='PLANEJADO' AND (jsonb_array_length(s.propostas)<>1
 OR s.propostas->0->>'valor' IS DISTINCT FROM s.original->>'valor'
 OR s.propostas->0->>'tipo' IS DISTINCT FROM s.original->>'tipo_contato');
CREATE UNIQUE INDEX ON telefone_v2_aposentar(contato_origem_id);
CREATE TEMP TABLE telefone_v2_antes ON COMMIT DROP AS
SELECT c.id,to_jsonb(c) dados FROM core.pessoa_contato c
JOIN (SELECT DISTINCT pessoa_id FROM telefone_v2_partes) p ON p.pessoa_id=c.pessoa_id;
CREATE UNIQUE INDEX ON telefone_v2_antes(id);
UPDATE core.pessoa_contato c SET ativo=false,principal=false
FROM telefone_v2_aposentar a WHERE c.id=a.contato_origem_id;
CREATE TEMP TABLE telefone_v2_unicos ON COMMIT DROP AS
SELECT DISTINCT pessoa_id,tipo,valor,normalizado,situacao FROM telefone_v2_partes;
ANALYZE telefone_v2_unicos;
-- Complete numbers deduplicate by national number; incomplete fragments by exact preserved value.
INSERT INTO core.pessoa_contato(pessoa_id,tipo_contato,valor,valor_normalizado,principal,ativo,created_at)
SELECT n.pessoa_id,n.tipo,n.valor,n.normalizado,false,true,now()
FROM telefone_v2_unicos n
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa_contato c WHERE c.pessoa_id=n.pessoa_id AND c.ativo AND c.tipo_contato=n.tipo
 AND CASE WHEN n.situacao='COMPLETO' THEN c.valor_normalizado=n.normalizado ELSE c.valor=n.valor END);
INSERT INTO legado.telefone_saneamento_destino_v2(contato_origem_id,ordem,contato_destino_id,situacao)
SELECT DISTINCT ON(n.contato_origem_id,n.ordem) n.contato_origem_id,n.ordem,c.id,n.situacao
FROM telefone_v2_partes n JOIN core.pessoa_contato c ON c.pessoa_id=n.pessoa_id AND c.ativo AND c.tipo_contato=n.tipo
 AND CASE WHEN n.situacao='COMPLETO' THEN c.valor_normalizado=n.normalizado ELSE c.valor=n.valor END
ORDER BY n.contato_origem_id,n.ordem,(c.id=n.contato_origem_id) DESC,c.principal DESC,c.id;
WITH preferred AS (
 SELECT DISTINCT ON(n.pessoa_id) n.pessoa_id,d.contato_destino_id
 FROM telefone_v2_partes n JOIN legado.telefone_saneamento_destino_v2 d USING(contato_origem_id,ordem)
 WHERE n.era_principal AND NOT EXISTS(SELECT 1 FROM core.pessoa_contato c WHERE c.pessoa_id=n.pessoa_id AND c.ativo AND c.principal AND c.tipo_contato IN ('TELEFONE','CELULAR'))
 ORDER BY n.pessoa_id,(n.situacao='COMPLETO') DESC,n.contato_origem_id,n.ordem,d.contato_destino_id
)
UPDATE core.pessoa_contato c SET principal=true FROM preferred p WHERE p.contato_destino_id=c.id;
INSERT INTO legado.telefone_saneamento_alteracao_v2(contato_id,antes,depois)
SELECT c.id,a.dados,to_jsonb(c) FROM core.pessoa_contato c JOIN telefone_v2_antes a ON a.id=c.id WHERE a.dados<>to_jsonb(c)
UNION ALL
SELECT c.id,NULL,to_jsonb(c) FROM core.pessoa_contato c
JOIN (SELECT DISTINCT d.contato_destino_id FROM legado.telefone_saneamento_destino_v2 d
JOIN telefone_v2_partes n USING(contato_origem_id,ordem)) d ON d.contato_destino_id=c.id
LEFT JOIN telefone_v2_antes a ON a.id=c.id WHERE a.id IS NULL
ON CONFLICT(contato_id) DO NOTHING;
DO $$ BEGIN
 IF EXISTS(SELECT 1 FROM telefone_v2_partes n WHERE NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_destino_v2 d
 WHERE d.contato_origem_id=n.contato_origem_id AND d.ordem=n.ordem))
 THEN RAISE EXCEPTION 'Trecho V2 sem destino'; END IF;
END $$;
UPDATE legado.telefone_saneamento_v2 SET situacao='APLICADO' WHERE situacao='PLANEJADO';
SELECT count(DISTINCT contato_origem_id) origens_processadas_v2,count(*) trechos_processados_v2 FROM telefone_v2_partes;
SELECT count(*) origens_substituidas_v2 FROM telefone_v2_aposentar;
