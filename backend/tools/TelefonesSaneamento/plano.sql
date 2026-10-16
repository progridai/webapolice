-- Technical staging and immutable before-images. No names or observations are rewritten.
SELECT pg_advisory_xact_lock(20261015, 2);
LOCK TABLE core.pessoa_contato IN SHARE ROW EXCLUSIVE MODE;
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_v1 (
 contato_origem_id bigint PRIMARY KEY,
 pessoa_id bigint NOT NULL,
 original jsonb NOT NULL,
 propostas jsonb NOT NULL,
 situacao text NOT NULL CHECK (situacao IN ('APTO','REVISAO','APLICADO')),
 created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_destino_v1 (
 contato_origem_id bigint NOT NULL REFERENCES legado.telefone_saneamento_v1(contato_origem_id),
 contato_destino_id bigint NOT NULL REFERENCES core.pessoa_contato(id),
 numero text NOT NULL,
 classificacao text NOT NULL CHECK (classificacao IN ('FIXO','CELULAR')),
 PRIMARY KEY(contato_origem_id,numero)
);
CREATE INDEX IF NOT EXISTS ix_telefone_saneamento_destino_v1_contato
 ON legado.telefone_saneamento_destino_v1(contato_destino_id);
CREATE OR REPLACE FUNCTION pg_temp.telefone_br_candidatos(t text)
RETURNS text[] LANGUAGE sql IMMUTABLE AS $$
 WITH raw AS (SELECT regexp_replace(t,'[^0-9]','','g') d
 WHERE t ~ '^[[:space:]0-9().+*-]+$'
 AND (t NOT LIKE '%+%' OR (btrim(t) ~ '^\+55' AND length(regexp_replace(t,'[^0-9]','','g')) IN (12,13)))),
 possibilities AS (
 SELECT d n FROM raw WHERE length(d) IN (10,11)
 UNION SELECT substr(d,2) FROM raw WHERE d LIKE '0%' AND length(d) IN (11,12)
 UNION SELECT substr(d,3) FROM raw WHERE d LIKE '00%' AND length(d) IN (12,13)
 UNION SELECT substr(d,3) FROM raw WHERE d LIKE '55%' AND length(d) IN (12,13)
 UNION SELECT substr(d,5) FROM raw WHERE d LIKE '0055%' AND length(d) IN (14,15)
 ), valid AS (
 SELECT DISTINCT n FROM possibilities
 WHERE substr(n,1,2) = ANY(ARRAY['11','12','13','14','15','16','17','18','19','21','22','24','27','28','31','32','33','34','35','37','38','41','42','43','44','45','46','47','48','49','51','53','54','55','61','62','63','64','65','66','67','68','69','71','73','74','75','77','79','81','82','83','84','85','86','87','88','89','91','92','93','94','95','96','97','98','99'])
 AND (n ~ '^[0-9]{2}[2-5][0-9]{7}$' OR n ~ '^[0-9]{2}9[0-9]{8}$')
 ) SELECT coalesce(array_agg(n ORDER BY n),ARRAY[]::text[]) FROM valid;
$$;
INSERT INTO legado.telefone_saneamento_v1(contato_origem_id,pessoa_id,original,propostas,situacao)
SELECT c.id,c.pessoa_id,to_jsonb(c),coalesce(s.propostas,'[]'::jsonb),CASE WHEN s.apto THEN 'APTO' ELSE 'REVISAO' END
FROM core.pessoa_contato c
CROSS JOIN LATERAL (
 SELECT jsonb_agg(jsonb_build_object('ordem',ord,'trecho_original',part,'candidatos',candidates) ORDER BY ord) propostas,
 bool_and(cardinality(candidates)=1) apto
 FROM (
 SELECT part,ord,pg_temp.telefone_br_candidatos(part) candidates
 FROM regexp_split_to_table(c.valor,'[/;|]+') WITH ORDINALITY a(part,ord)
 WHERE btrim(part)<>''
 ) pieces
) s
WHERE c.ativo AND c.tipo_contato IN ('TELEFONE','CELULAR')
AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_v1 previous WHERE previous.contato_origem_id=c.id)
AND EXISTS(SELECT 1 FROM cadastro.cliente cl WHERE cl.pessoa_id=c.pessoa_id)
AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_destino_v1 d WHERE d.contato_destino_id=c.id)
ON CONFLICT(contato_origem_id) DO NOTHING;
SELECT situacao,count(*) origens,sum(jsonb_array_length(propostas)) trechos
FROM legado.telefone_saneamento_v1 GROUP BY situacao ORDER BY situacao;
SELECT CASE WHEN length(p->'candidatos'->>0)=10 THEN 'FIXO' ELSE 'CELULAR' END classificacao,
count(*) candidatos FROM legado.telefone_saneamento_v1 s CROSS JOIN LATERAL jsonb_array_elements(s.propostas) p
WHERE situacao='APTO' GROUP BY 1;
SELECT count(*) sem_candidato FROM legado.telefone_saneamento_v1 s
WHERE NOT EXISTS(SELECT 1 FROM jsonb_array_elements(s.propostas) p WHERE jsonb_array_length(p->'candidatos')>0);
