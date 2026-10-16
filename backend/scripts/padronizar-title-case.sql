-- Executar integralmente na base de destino. Transação atômica; preservar backup antes da aplicação.
-- Nenhum segredo, campo técnico, tabela de auditoria ou migration map é alterado.
BEGIN;
SET LOCAL lock_timeout = '10s';
CREATE OR REPLACE FUNCTION public.webapolice_title_case(value text)
RETURNS text LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE AS $function$
    SELECT coalesce(string_agg(
        CASE WHEN parts[1] = ANY(ARRAY['a', 'o', 'as', 'os', 'um', 'uma', 'uns', 'umas', 'de', 'da', 'do', 'das', 'dos', 'em', 'na', 'no', 'nas', 'nos', 'por', 'para', 'com', 'sem', 'sob', 'sobre', 'entre', 'até', 'ante', 'após', 'contra', 'desde', 'perante', 'trás', 'durante', 'mediante', 'e', 'ou', 'nem', 'mas', 'porém', 'contudo', 'todavia', 'entretanto', 'pois', 'porque', 'portanto', 'logo', 'que', 'se', 'como', 'quando', 'enquanto', 'embora', 'caso', 'conquanto', 'senão', 'à', 'às', 'ao', 'aos', 'pelo', 'pela', 'pelos', 'pelas', 'num', 'numa', 'nuns', 'numas', 'd']) THEN parts[1]
             WHEN parts[1] ~ '^[[:alpha:]]' THEN upper(left(parts[1], 1)) || lower(substr(parts[1], 2))
             ELSE parts[1] END, '' ORDER BY ord), '')
    FROM regexp_matches(lower(value), '([[:alpha:]]+|[^[:alpha:]]+)', 'g') WITH ORDINALITY AS t(parts, ord);
$function$;

CREATE OR REPLACE FUNCTION public.webapolice_normalizar_textos()
RETURNS trigger LANGUAGE plpgsql AS $function$
DECLARE payload jsonb := to_jsonb(NEW); field text;
BEGIN
    FOREACH field IN ARRAY TG_ARGV LOOP
        IF jsonb_typeof(payload -> field) = 'string' THEN
            payload := jsonb_set(payload, ARRAY[field], to_jsonb(public.webapolice_title_case(payload ->> field)));
        END IF;
    END LOOP;
    NEW := jsonb_populate_record(NEW, payload);
    RETURN NEW;
END;
$function$;

-- Descobrir apenas colunas humanas conhecidas, nas tabelas de negócio.
-- Instala trigger para INSERT/UPDATE, incluindo COPY e SQL que não usam EF.
DO $block$
DECLARE target record; assignments text; arguments text; changed bigint;
BEGIN
    FOR target IN
        SELECT c.table_schema, c.table_name, array_agg(c.column_name ORDER BY c.ordinal_position) AS fields
        FROM information_schema.columns c
        JOIN information_schema.tables t USING (table_schema, table_name)
        WHERE t.table_type = 'BASE TABLE'
          AND c.table_schema IN ('core','cadastro','seguro','financeiro','convenio','atendimento','comissao','sinistros','documentos','seguranca')
          AND c.table_name NOT ILIKE '%auditoria%'
          AND c.table_name NOT ILIKE '%migration%'
          AND c.data_type IN ('text', 'character varying', 'character')
          AND c.is_generated = 'NEVER'
          AND c.column_name = ANY(ARRAY['nome','nome_fantasia','razao_social','nome_conjuge','nome_empresa','nome_reduzido','nome_formatado','descricao','observacao','titulo','logradouro','complemento','bairro','departamento','contato','empresa','equipe','funcao','setor','motivo','causa','instituidor','instituto','est_nome','est_cidade','est_endereco','est_complemento','est_bairro'])
        GROUP BY c.table_schema, c.table_name
        ORDER BY c.table_schema, c.table_name
    LOOP
        SELECT string_agg(format('%I = public.webapolice_title_case(%I)', f, f), ', '),
               string_agg(quote_literal(f), ', ')
          INTO assignments, arguments FROM unnest(target.fields) f;
        EXECUTE format('DROP TRIGGER IF EXISTS webapolice_title_case ON %I.%I', target.table_schema, target.table_name);
        EXECUTE format('CREATE TRIGGER webapolice_title_case BEFORE INSERT OR UPDATE ON %I.%I FOR EACH ROW EXECUTE FUNCTION public.webapolice_normalizar_textos(%s)', target.table_schema, target.table_name, arguments);
        EXECUTE format('UPDATE %I.%I SET %s WHERE %s', target.table_schema, target.table_name, assignments,
            (SELECT string_agg(format('%I IS DISTINCT FROM public.webapolice_title_case(%I)', f, f), ' OR ') FROM unnest(target.fields) f));
        GET DIAGNOSTICS changed = ROW_COUNT;
        RAISE NOTICE 'Title Case: %.%: % registros alterados', target.table_schema, target.table_name, changed;
    END LOOP;
END;
$block$;
COMMIT;
