SELECT pg_advisory_xact_lock(20261015,2);
LOCK TABLE core.pessoa_contato IN SHARE ROW EXCLUSIVE MODE;
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_v2 (
 contato_origem_id bigint PRIMARY KEY,
 pessoa_id bigint NOT NULL,
 original jsonb NOT NULL,
 propostas jsonb NOT NULL,
 situacao text NOT NULL DEFAULT 'PLANEJADO' CHECK(situacao IN ('PLANEJADO','APLICADO')),
 created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_destino_v2 (
 contato_origem_id bigint NOT NULL REFERENCES legado.telefone_saneamento_v2(contato_origem_id),
 ordem integer NOT NULL,
 contato_destino_id bigint NOT NULL REFERENCES core.pessoa_contato(id),
 situacao text NOT NULL CHECK(situacao IN ('COMPLETO','INCOMPLETO','SEM_NUMERO')),
 PRIMARY KEY(contato_origem_id,ordem)
);
CREATE INDEX IF NOT EXISTS ix_telefone_saneamento_destino_v2_contato ON legado.telefone_saneamento_destino_v2(contato_destino_id);
CREATE TABLE IF NOT EXISTS legado.telefone_saneamento_alteracao_v2 (
 contato_id bigint PRIMARY KEY,antes jsonb,depois jsonb NOT NULL
);
