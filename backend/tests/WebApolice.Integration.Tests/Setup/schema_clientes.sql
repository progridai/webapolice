CREATE SCHEMA IF NOT EXISTS core;
CREATE SCHEMA IF NOT EXISTS cadastro;

CREATE EXTENSION IF NOT EXISTS "pgcrypto"; -- para gen_random_uuid()

CREATE TABLE core.pessoa (
    id bigint NOT NULL PRIMARY KEY,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    tipo_pessoa smallint DEFAULT 1 NOT NULL,
    nome character varying(150) NOT NULL,
    nome_normalizado character varying(150),
    documento_principal character varying(30),
    documento_principal_limpo character varying(20),
    documento_valido boolean DEFAULT false NOT NULL,
    data_nascimento date,
    sexo smallint,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);

CREATE TABLE core.pessoa_contato (
    id bigint NOT NULL PRIMARY KEY,
    pessoa_id bigint NOT NULL,
    tipo_contato character varying(30) NOT NULL,
    valor character varying(150) NOT NULL,
    valor_normalizado character varying(150),
    principal boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE core.pessoa_endereco (
    id bigint NOT NULL PRIMARY KEY,
    pessoa_id bigint NOT NULL,
    cidade_id bigint,
    tipo_endereco character varying(30) DEFAULT 'principal'::character varying NOT NULL,
    cep character varying(20),
    logradouro character varying(150),
    numero character varying(30),
    complemento character varying(150),
    bairro character varying(100),
    uf character(2),
    principal boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    legado_situacao_endereco integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE core.banco (
    id bigint NOT NULL PRIMARY KEY,
    codigo character varying(20),
    nome character varying(100) NOT NULL,
    observacao text,
    legado_id integer
);

CREATE TABLE cadastro.cliente_status (
    id smallint NOT NULL PRIMARY KEY,
    codigo character varying(30) NOT NULL,
    nome character varying(80) NOT NULL,
    ativo boolean DEFAULT true NOT NULL
);

INSERT INTO cadastro.cliente_status (id, codigo, nome, ativo) VALUES (1, 'ativo', 'Ativo', true);
INSERT INTO cadastro.cliente_status (id, codigo, nome, ativo) VALUES (2, 'inativo', 'Inativo', true);

CREATE TABLE cadastro.cliente (
    id bigint NOT NULL PRIMARY KEY,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint NOT NULL,
    status_id smallint NOT NULL,
    falecido boolean DEFAULT false NOT NULL,
    data_obito date,
    observacao text,
    data_cadastro_legado date,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.cliente_vinculo (
    id bigint NOT NULL PRIMARY KEY,
    cliente_id bigint NOT NULL,
    pessoa_id bigint NOT NULL,
    estipulante_id bigint,
    subestipulante_id bigint,
    grupo_id bigint,
    subgrupo_id bigint,
    lotacao_id bigint,
    matricula character varying(50),
    matricula_normalizada character varying(50),
    banco_id bigint,
    agencia character varying(30),
    conta_corrente character varying(30),
    legado_cliente_id integer,
    criterio_criacao character varying(80) NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE cadastro.cliente_dependente (
    id bigint NOT NULL PRIMARY KEY,
    cliente_id bigint NOT NULL,
    pessoa_id bigint,
    tipo_relacao character varying(30) NOT NULL,
    nome character varying(150) NOT NULL,
    cpf character varying(30),
    cpf_limpo character varying(20),
    rg character varying(30),
    orgao_rg character varying(30),
    data_emissao_rg date,
    data_nascimento date,
    legado_origem character varying(80),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE cadastro.estipulante (
    id bigint NOT NULL PRIMARY KEY,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    nome character varying(150) NOT NULL,
    nome_formatado character varying(200),
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.subestipulante (
    id bigint NOT NULL PRIMARY KEY,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint NOT NULL,
    codigo character varying(80),
    ativo boolean DEFAULT true NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.corretora (
    id bigint NOT NULL PRIMARY KEY,
    pessoa_id bigint,
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.seguradora (
    id bigint NOT NULL PRIMARY KEY,
    pessoa_id bigint,
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.agenciador (
    id bigint NOT NULL PRIMARY KEY,
    pessoa_id bigint,
    deleted_at timestamp with time zone
);

CREATE TABLE cadastro.grupo (
    id bigint NOT NULL PRIMARY KEY,
    nome character varying(100) NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE cadastro.subgrupo (
    id bigint NOT NULL PRIMARY KEY,
    grupo_id bigint,
    nome character varying(100) NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);

CREATE TABLE cadastro.lotacao (
    id bigint NOT NULL PRIMARY KEY,
    cidade_id bigint,
    nome character varying(100) NOT NULL,
    codigo character varying(50),
    legado_id integer
);
