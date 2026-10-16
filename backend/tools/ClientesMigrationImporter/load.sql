CREATE TEMP TABLE migration_groups ON COMMIT DROP AS
SELECT group_key,max(preferred) preferred,max(person_id) person_id,max(client_id) client_id
FROM migration_source GROUP BY group_key;
CREATE UNIQUE INDEX ON migration_groups(group_key);
UPDATE migration_groups SET person_id=COALESCE(person_id,nextval(pg_get_serial_sequence('core.pessoa','id'))),
 client_id=COALESCE(client_id,nextval(pg_get_serial_sequence('cadastro.cliente','id')));
CREATE UNIQUE INDEX ON migration_groups(person_id);
CREATE UNIQUE INDEX ON migration_groups(client_id);
ANALYZE migration_source;
ANALYZE migration_groups;

INSERT INTO core.pessoa(id,public_id,tipo_pessoa,nome,nome_normalizado,documento_principal,documento_principal_limpo,documento_valido,data_nascimento,sexo,created_at,updated_at)
OVERRIDING SYSTEM VALUE
SELECT g.person_id,gen_random_uuid(),1,s.data->>'Cli_Nome',upper(s.data->>'Cli_Nome'),s.data->>'Cli_CPF_11',s.data->>'Cli_CPF_11',
 (s.data->>'cpfValido')::boolean,(s.data->>'Cli_Data_Nascimento')::date,s.sex,now(),now()
FROM migration_groups g JOIN migration_source s ON s.legacy_id=g.preferred
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa p WHERE p.id=g.person_id);

UPDATE core.pessoa p SET nome=s.data->>'Cli_Nome',nome_normalizado=upper(s.data->>'Cli_Nome'),
 documento_principal=s.data->>'Cli_CPF_11',documento_principal_limpo=s.data->>'Cli_CPF_11',
 documento_valido=(s.data->>'cpfValido')::boolean,data_nascimento=(s.data->>'Cli_Data_Nascimento')::date,sexo=s.sex,updated_at=now()
FROM migration_groups g JOIN migration_source s ON s.legacy_id=g.preferred
WHERE p.id=g.person_id AND p.id=1 AND g.preferred=435698;

INSERT INTO cadastro.cliente(id,public_id,pessoa_id,status_id,falecido,data_obito,observacao,data_cadastro_legado,legado_id,created_at,updated_at)
OVERRIDING SYSTEM VALUE
SELECT g.client_id,gen_random_uuid(),g.person_id,(SELECT id FROM cadastro.cliente_status WHERE lower(codigo)='ativo'),
 s.deceased,(s.data->>'Cli_Data_Obito')::date,s.data->>'Cli_Observacoes',(s.data->>'Cli_Data_Cadastro')::date,g.preferred,now(),now()
FROM migration_groups g JOIN migration_source s ON s.legacy_id=g.preferred
WHERE NOT EXISTS(SELECT 1 FROM cadastro.cliente c WHERE c.id=g.client_id);

UPDATE cadastro.cliente c SET status_id=(SELECT id FROM cadastro.cliente_status WHERE lower(codigo)='ativo'),falecido=s.deceased,
 data_obito=(s.data->>'Cli_Data_Obito')::date,observacao=s.data->>'Cli_Observacoes',data_cadastro_legado=(s.data->>'Cli_Data_Cadastro')::date,
 legado_id=g.preferred,updated_at=now()
FROM migration_groups g JOIN migration_source s ON s.legacy_id=g.preferred
WHERE c.id=g.client_id AND c.pessoa_id=1 AND g.preferred=435698;

CREATE TEMP TABLE migration_links ON COMMIT DROP AS
SELECT DISTINCT ON(g.client_id,s.data->'linkKey') g.client_id,g.person_id,s.data->'linkKey' link_key,
 s.legacy_id,s.data->>'Cli_Matricula' matricula,NULL::bigint id
FROM migration_source s JOIN migration_groups g USING(group_key)
WHERE s.data->'linkKey' IS NOT NULL AND s.data->'linkKey'<>'null'::jsonb
ORDER BY g.client_id,s.data->'linkKey',s.legacy_id DESC;
CREATE UNIQUE INDEX ON migration_links(client_id,link_key);
UPDATE migration_links l SET id=COALESCE((SELECT min(v.id) FROM cadastro.cliente_vinculo v
 WHERE v.cliente_id=l.client_id AND COALESCE(v.matricula_normalizada,upper(btrim(v.matricula)),'')=l.link_key->>0
 AND v.lotacao_id IS NOT DISTINCT FROM (l.link_key->>1)::bigint AND v.banco_id IS NOT DISTINCT FROM (l.link_key->>2)::bigint
 AND v.agencia IS NOT DISTINCT FROM l.link_key->>3 AND v.conta_corrente IS NOT DISTINCT FROM l.link_key->>4),
 nextval(pg_get_serial_sequence('cadastro.cliente_vinculo','id')));
INSERT INTO cadastro.cliente_vinculo(id,cliente_id,pessoa_id,lotacao_id,matricula,matricula_normalizada,banco_id,agencia,conta_corrente,legado_cliente_id,criterio_criacao,ativo,created_at,updated_at)
OVERRIDING SYSTEM VALUE
SELECT id,client_id,person_id,(link_key->>1)::bigint,matricula,NULLIF(link_key->>0,''),(link_key->>2)::bigint,
 link_key->>3,link_key->>4,legacy_id,'MIGRACAO_VINCULO_DISTINTO',true,now(),now()
FROM migration_links l WHERE NOT EXISTS(SELECT 1 FROM cadastro.cliente_vinculo v WHERE v.id=l.id);
ANALYZE migration_links;

CREATE TEMP TABLE migration_documents ON COMMIT DROP AS
SELECT DISTINCT ON(g.person_id,s.data->>'Cli_RG',s.data->>'Cli_Orgao_RG',s.data->>'Cli_Data_Emissao_RG')
 g.person_id,s.legacy_id,s.data->>'Cli_RG' numero,
 regexp_replace(upper(s.data->>'Cli_RG'),'[^[:alnum:]]','','g') limpo,
 s.data->>'Cli_Orgao_RG' orgao,(s.data->>'Cli_Data_Emissao_RG')::date emissao
FROM migration_source s JOIN migration_groups g USING(group_key)
WHERE s.data->>'Cli_RG' IS NOT NULL
ORDER BY g.person_id,s.data->>'Cli_RG',s.data->>'Cli_Orgao_RG',s.data->>'Cli_Data_Emissao_RG',s.legacy_id DESC;
INSERT INTO core.pessoa_documento(pessoa_id,tipo_documento,numero,numero_limpo,orgao_emissor,data_emissao,principal,created_at)
SELECT d.person_id,'RG',d.numero,d.limpo,d.orgao,d.emissao,
 d.rn=1 AND NOT EXISTS(SELECT 1 FROM core.pessoa_documento x WHERE x.pessoa_id=d.person_id AND x.principal),now()
FROM (SELECT *,row_number() OVER(PARTITION BY person_id ORDER BY legacy_id DESC,numero) rn FROM migration_documents) d
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa_documento x WHERE x.pessoa_id=d.person_id AND x.tipo_documento='RG'
 AND x.numero IS NOT DISTINCT FROM d.numero AND x.orgao_emissor IS NOT DISTINCT FROM d.orgao AND x.data_emissao IS NOT DISTINCT FROM d.emissao);

CREATE TEMP TABLE migration_contacts ON COMMIT DROP AS
SELECT DISTINCT ON(g.person_id,c.type,CASE WHEN c.type='EMAIL' THEN upper(c.value) ELSE c.normalized END)
 g.person_id,s.legacy_id,c.type,c.value,CASE WHEN c.type='EMAIL' THEN upper(c.value) ELSE c.normalized END normalized,c."order" ord
FROM migration_source s JOIN migration_groups g USING(group_key)
CROSS JOIN LATERAL jsonb_to_recordset(s.data->'contacts') c(type text,value text,normalized text,"order" integer)
ORDER BY g.person_id,c.type,CASE WHEN c.type='EMAIL' THEN upper(c.value) ELSE c.normalized END,s.legacy_id DESC;
INSERT INTO core.pessoa_contato(pessoa_id,tipo_contato,valor,valor_normalizado,principal,ativo,created_at)
SELECT c.person_id,c.type,c.value,c.normalized,c.rn=1 AND NOT EXISTS(SELECT 1 FROM core.pessoa_contato x WHERE x.pessoa_id=c.person_id AND x.tipo_contato=c.type AND x.principal AND x.ativo),true,now()
FROM (SELECT *,row_number() OVER(PARTITION BY person_id,type ORDER BY legacy_id DESC,value) rn FROM migration_contacts) c
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa_contato x WHERE x.pessoa_id=c.person_id AND x.tipo_contato=c.type AND x.valor_normalizado=c.normalized);

CREATE TEMP TABLE migration_addresses ON COMMIT DROP AS
SELECT DISTINCT ON(g.person_id,a.type,a."cityId",a.cep,a.street,a.number,a.complement,a.neighborhood)
 g.person_id,s.legacy_id,a.*
FROM migration_source s JOIN migration_groups g USING(group_key)
CROSS JOIN LATERAL jsonb_to_recordset(s.data->'addresses') a(type text,"cityId" bigint,cep text,street text,number text,complement text,neighborhood text,principal boolean,state integer)
ORDER BY g.person_id,a.type,a."cityId",a.cep,a.street,a.number,a.complement,a.neighborhood,a.principal DESC,s.legacy_id;
INSERT INTO core.pessoa_endereco(pessoa_id,cidade_id,tipo_endereco,cep,logradouro,numero,complemento,bairro,uf,principal,ativo,legado_situacao_endereco,created_at)
SELECT a.person_id,a."cityId",a.type,a.cep,a.street,a.number,a.complement,a.neighborhood,c.uf,
 a.principal AND NOT EXISTS(SELECT 1 FROM core.pessoa_endereco x WHERE x.pessoa_id=a.person_id AND x.principal AND x.ativo),true,a.state,now()
FROM migration_addresses a LEFT JOIN core.cidade c ON c.id=a."cityId"
WHERE NOT EXISTS(SELECT 1 FROM core.pessoa_endereco x WHERE x.pessoa_id=a.person_id AND x.tipo_endereco=a.type
 AND x.cidade_id IS NOT DISTINCT FROM a."cityId" AND x.cep IS NOT DISTINCT FROM a.cep AND x.logradouro IS NOT DISTINCT FROM a.street
 AND x.numero IS NOT DISTINCT FROM a.number AND x.complemento IS NOT DISTINCT FROM a.complement AND x.bairro IS NOT DISTINCT FROM a.neighborhood)
ORDER BY a.person_id,a.legacy_id,CASE WHEN a.type='RESIDENCIAL' THEN 0 ELSE 1 END;
UPDATE core.pessoa_endereco x SET principal=true FROM migration_addresses a
WHERE a.principal AND x.pessoa_id=a.person_id AND x.tipo_endereco=a.type AND x.ativo
 AND x.cidade_id IS NOT DISTINCT FROM a."cityId" AND x.cep IS NOT DISTINCT FROM a.cep AND x.logradouro IS NOT DISTINCT FROM a.street
 AND x.numero IS NOT DISTINCT FROM a.number AND x.complemento IS NOT DISTINCT FROM a.complement AND x.bairro IS NOT DISTINCT FROM a.neighborhood
 AND NOT EXISTS(SELECT 1 FROM core.pessoa_endereco e WHERE e.pessoa_id=x.pessoa_id AND e.principal AND e.ativo);
UPDATE core.pessoa_endereco x SET principal=true WHERE x.id IN(
 SELECT min(e.id) FROM core.pessoa_endereco e JOIN migration_groups g ON e.pessoa_id=g.person_id
 WHERE e.ativo GROUP BY e.pessoa_id HAVING count(*) FILTER(WHERE e.principal)=0);

CREATE TEMP TABLE migration_dependents ON COMMIT DROP AS
SELECT DISTINCT ON(g.client_id,s.data->>'spouseKey') g.client_id,s.legacy_id,s.data
FROM migration_source s JOIN migration_groups g USING(group_key)
WHERE s.data->>'Cli_Nome_Conjuge' IS NOT NULL
ORDER BY g.client_id,s.data->>'spouseKey',s.legacy_id DESC;
INSERT INTO cadastro.cliente_dependente(cliente_id,tipo_relacao,nome,cpf,data_nascimento,rg,orgao_rg,data_emissao_rg,created_at)
SELECT d.client_id,'CONJUGE',d.data->>'Cli_Nome_Conjuge',d.data->>'Cli_CPF_Conjuge',(d.data->>'Cli_Data_Nascimento_Conjuge')::date,
 d.data->>'Cli_RG_Conjuge',d.data->>'Cli_Orgao_RG_Conjuge',(d.data->>'Cli_Data_Emissao_RG_Conjuge')::date,now()
FROM migration_dependents d
WHERE NOT EXISTS(SELECT 1 FROM cadastro.cliente_dependente x WHERE x.cliente_id=d.client_id AND x.tipo_relacao='CONJUGE'
 AND x.nome=d.data->>'Cli_Nome_Conjuge' AND x.cpf IS NOT DISTINCT FROM d.data->>'Cli_CPF_Conjuge'
 AND x.data_nascimento IS NOT DISTINCT FROM (d.data->>'Cli_Data_Nascimento_Conjuge')::date
 AND x.rg IS NOT DISTINCT FROM d.data->>'Cli_RG_Conjuge' AND x.orgao_rg IS NOT DISTINCT FROM d.data->>'Cli_Orgao_RG_Conjuge'
 AND x.data_emissao_rg IS NOT DISTINCT FROM (d.data->>'Cli_Data_Emissao_RG_Conjuge')::date);

CREATE TEMP TABLE migration_siape ON COMMIT DROP AS
SELECT DISTINCT ON(g.client_id,l.id,s.data->>'Cli_Siape',s.data->>'orgaoId',s.data->>'Cli_Siape_Categoria',s.data->>'Cli_Siape_Setor',s.data->>'Cli_Siape_Instituto',s.data->>'Cli_Siape_Agencia',s.data->>'Cli_Siape_Funcao',s.data->>'Cli_Siape_Contrato',s.data->>'Cli_Digito_Verificador_Siape',s.data->>'Cli_Siape_Instituidor')
g.client_id,g.person_id,l.id link_id,s.data
FROM migration_source s JOIN migration_groups g USING(group_key)
LEFT JOIN migration_links l ON l.client_id=g.client_id AND l.link_key=s.data->'linkKey'
WHERE (s.data->>'hasSiape')::boolean;
INSERT INTO convenio.siape_cliente(cliente_id,pessoa_id,cliente_vinculo_id,siape,orgao_id,categoria,setor,instituto,agencia,funcao,contrato,digito_verificador,instituidor,ativo,created_at)
SELECT client_id,person_id,link_id,data->>'Cli_Siape',(data->>'orgaoId')::bigint,data->>'Cli_Siape_Categoria',data->>'Cli_Siape_Setor',data->>'Cli_Siape_Instituto',data->>'Cli_Siape_Agencia',data->>'Cli_Siape_Funcao',data->>'Cli_Siape_Contrato',data->>'Cli_Digito_Verificador_Siape',data->>'Cli_Siape_Instituidor',true,now()
FROM migration_siape;
CREATE TEMP TABLE migration_corsan ON COMMIT DROP AS
SELECT DISTINCT ON(g.client_id,l.id,s.data->>'Cli_Empresa_Corsan',s.data->>'Cli_Rubrica_Corsan',s.data->>'Cli_Grupo_Corsan',s.data->>'Cli_Func_Corsan')
g.client_id,g.person_id,l.id link_id,s.data
FROM migration_source s JOIN migration_groups g USING(group_key)
LEFT JOIN migration_links l ON l.client_id=g.client_id AND l.link_key=s.data->'linkKey'
WHERE (s.data->>'hasCorsan')::boolean;
INSERT INTO convenio.corsan_cliente(cliente_id,pessoa_id,cliente_vinculo_id,empresa,rubrica,grupo,funcionario,ativo,created_at)
SELECT client_id,person_id,link_id,data->>'Cli_Empresa_Corsan',data->>'Cli_Rubrica_Corsan',data->>'Cli_Grupo_Corsan',(data->>'Cli_Func_Corsan')::boolean,true,now()
FROM migration_corsan;

INSERT INTO legado.cliente_migration_map(legado_cliente_id,pessoa_id,cliente_id,cliente_vinculo_id,cpf_original,cpf_limpo,cpf_valido,nome_original,matricula_original,criterio_unificacao_pessoa,criterio_criacao_vinculo,created_at)
SELECT s.legacy_id,g.person_id,g.client_id,l.id,s.data->>'Cli_CPF',s.data->>'Cli_CPF_11',(s.data->>'cpfValido')::boolean,
s.data->>'Cli_Nome',s.data->>'Cli_Matricula',CASE WHEN (s.data->>'cpfValido')::boolean THEN 'CPF_VALIDO_MAIOR_CLIENTE' ELSE 'CPF_INVALIDO_ISOLADO' END,
CASE WHEN l.id IS NOT NULL THEN 'VINCULO_DISTINTO' ELSE 'SEM_DADOS_VINCULO' END,now()
FROM migration_source s JOIN migration_groups g USING(group_key)
LEFT JOIN migration_links l ON l.client_id=g.client_id AND l.link_key=s.data->'linkKey';
