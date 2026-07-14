--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 16.3

-- Started on 2026-07-06 21:25:53

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 19 (class 2615 OID 8862187)
-- Name: atendimento; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA atendimento;


ALTER SCHEMA atendimento OWNER TO postgres;

--
-- TOC entry 10 (class 2615 OID 8860074)
-- Name: cadastro; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA cadastro;


ALTER SCHEMA cadastro OWNER TO postgres;

--
-- TOC entry 15 (class 2615 OID 8860622)
-- Name: comissao; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA comissao;


ALTER SCHEMA comissao OWNER TO postgres;

--
-- TOC entry 11 (class 2615 OID 8860075)
-- Name: convenio; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA convenio;


ALTER SCHEMA convenio OWNER TO postgres;

--
-- TOC entry 9 (class 2615 OID 8860073)
-- Name: core; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA core;


ALTER SCHEMA core OWNER TO postgres;

--
-- TOC entry 18 (class 2615 OID 8862015)
-- Name: documento; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA documento;


ALTER SCHEMA documento OWNER TO postgres;

--
-- TOC entry 12 (class 2615 OID 8860076)
-- Name: financeiro; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA financeiro;


ALTER SCHEMA financeiro OWNER TO postgres;

--
-- TOC entry 16 (class 2615 OID 8860623)
-- Name: integracao; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA integracao;


ALTER SCHEMA integracao OWNER TO postgres;

--
-- TOC entry 14 (class 2615 OID 8860078)
-- Name: legado; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA legado;


ALTER SCHEMA legado OWNER TO postgres;

--
-- TOC entry 13 (class 2615 OID 8860077)
-- Name: seguro; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA seguro;


ALTER SCHEMA seguro OWNER TO postgres;

--
-- TOC entry 17 (class 2615 OID 8861792)
-- Name: sinistro; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA sinistro;


ALTER SCHEMA sinistro OWNER TO postgres;

--
-- TOC entry 2 (class 3079 OID 8860079)
-- Name: pg_trgm; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;


--
-- TOC entry 6740 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pg_trgm; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';


--
-- TOC entry 4 (class 3079 OID 8860167)
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- TOC entry 6741 (class 0 OID 0)
-- Dependencies: 4
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- TOC entry 3 (class 3079 OID 8860160)
-- Name: unaccent; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;


--
-- TOC entry 6742 (class 0 OID 0)
-- Dependencies: 3
-- Name: EXTENSION unaccent; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION unaccent IS 'text search dictionary that removes accents';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 386 (class 1259 OID 8862250)
-- Name: protocolo_acompanhamento; Type: TABLE; Schema: atendimento; Owner: postgres
--

CREATE TABLE atendimento.protocolo_acompanhamento (
    id bigint NOT NULL,
    protocolo_lote_id bigint,
    data_acompanhamento date,
    hora_original character varying(30),
    contato character varying(150),
    descricao text,
    usuario_legado_id integer,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE atendimento.protocolo_acompanhamento OWNER TO postgres;

--
-- TOC entry 385 (class 1259 OID 8862249)
-- Name: protocolo_acompanhamento_id_seq; Type: SEQUENCE; Schema: atendimento; Owner: postgres
--

CREATE SEQUENCE atendimento.protocolo_acompanhamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE atendimento.protocolo_acompanhamento_id_seq OWNER TO postgres;

--
-- TOC entry 6743 (class 0 OID 0)
-- Dependencies: 385
-- Name: protocolo_acompanhamento_id_seq; Type: SEQUENCE OWNED BY; Schema: atendimento; Owner: postgres
--

ALTER SEQUENCE atendimento.protocolo_acompanhamento_id_seq OWNED BY atendimento.protocolo_acompanhamento.id;


--
-- TOC entry 384 (class 1259 OID 8862206)
-- Name: protocolo_item; Type: TABLE; Schema: atendimento; Owner: postgres
--

CREATE TABLE atendimento.protocolo_item (
    id bigint NOT NULL,
    protocolo_lote_id bigint NOT NULL,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    estipulante_id bigint,
    premio numeric(18,2),
    data_vigencia date,
    equipe character varying(100),
    matricula character varying(80),
    tipo_item character varying(40) DEFAULT 'titular'::character varying NOT NULL,
    nome_conjuge character varying(150),
    origem_legado character varying(80) NOT NULL,
    legado_id integer NOT NULL,
    legado_cliente_id integer,
    legado_estipulante_id integer,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE atendimento.protocolo_item OWNER TO postgres;

--
-- TOC entry 383 (class 1259 OID 8862205)
-- Name: protocolo_item_id_seq; Type: SEQUENCE; Schema: atendimento; Owner: postgres
--

CREATE SEQUENCE atendimento.protocolo_item_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE atendimento.protocolo_item_id_seq OWNER TO postgres;

--
-- TOC entry 6744 (class 0 OID 0)
-- Dependencies: 383
-- Name: protocolo_item_id_seq; Type: SEQUENCE OWNED BY; Schema: atendimento; Owner: postgres
--

ALTER SEQUENCE atendimento.protocolo_item_id_seq OWNED BY atendimento.protocolo_item.id;


--
-- TOC entry 382 (class 1259 OID 8862189)
-- Name: protocolo_lote; Type: TABLE; Schema: atendimento; Owner: postgres
--

CREATE TABLE atendimento.protocolo_lote (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    numero_protocolo integer,
    data_protocolo timestamp with time zone,
    consultor_legado_id integer,
    usuario_legado_id integer,
    anexo_consultor boolean,
    anexo_seguradora boolean,
    status character varying(40) DEFAULT 'ativo'::character varying NOT NULL,
    observacao text,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE atendimento.protocolo_lote OWNER TO postgres;

--
-- TOC entry 381 (class 1259 OID 8862188)
-- Name: protocolo_lote_id_seq; Type: SEQUENCE; Schema: atendimento; Owner: postgres
--

CREATE SEQUENCE atendimento.protocolo_lote_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE atendimento.protocolo_lote_id_seq OWNER TO postgres;

--
-- TOC entry 6745 (class 0 OID 0)
-- Dependencies: 381
-- Name: protocolo_lote_id_seq; Type: SEQUENCE OWNED BY; Schema: atendimento; Owner: postgres
--

ALTER SEQUENCE atendimento.protocolo_lote_id_seq OWNED BY atendimento.protocolo_lote.id;


--
-- TOC entry 394 (class 1259 OID 8862354)
-- Name: protocolo_relatorio_seguradora; Type: TABLE; Schema: atendimento; Owner: postgres
--

CREATE TABLE atendimento.protocolo_relatorio_seguradora (
    id bigint NOT NULL,
    data_relatorio timestamp with time zone,
    observacao text,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE atendimento.protocolo_relatorio_seguradora OWNER TO postgres;

--
-- TOC entry 393 (class 1259 OID 8862353)
-- Name: protocolo_relatorio_seguradora_id_seq; Type: SEQUENCE; Schema: atendimento; Owner: postgres
--

CREATE SEQUENCE atendimento.protocolo_relatorio_seguradora_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE atendimento.protocolo_relatorio_seguradora_id_seq OWNER TO postgres;

--
-- TOC entry 6746 (class 0 OID 0)
-- Dependencies: 393
-- Name: protocolo_relatorio_seguradora_id_seq; Type: SEQUENCE OWNED BY; Schema: atendimento; Owner: postgres
--

ALTER SEQUENCE atendimento.protocolo_relatorio_seguradora_id_seq OWNED BY atendimento.protocolo_relatorio_seguradora.id;


--
-- TOC entry 396 (class 1259 OID 8862366)
-- Name: protocolo_relatorio_seguradora_item; Type: TABLE; Schema: atendimento; Owner: postgres
--

CREATE TABLE atendimento.protocolo_relatorio_seguradora_item (
    id bigint NOT NULL,
    relatorio_id bigint NOT NULL,
    protocolo_lote_id bigint,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    legado_cliente_id integer,
    legado_protocolo_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE atendimento.protocolo_relatorio_seguradora_item OWNER TO postgres;

--
-- TOC entry 395 (class 1259 OID 8862365)
-- Name: protocolo_relatorio_seguradora_item_id_seq; Type: SEQUENCE; Schema: atendimento; Owner: postgres
--

CREATE SEQUENCE atendimento.protocolo_relatorio_seguradora_item_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE atendimento.protocolo_relatorio_seguradora_item_id_seq OWNER TO postgres;

--
-- TOC entry 6747 (class 0 OID 0)
-- Dependencies: 395
-- Name: protocolo_relatorio_seguradora_item_id_seq; Type: SEQUENCE OWNED BY; Schema: atendimento; Owner: postgres
--

ALTER SEQUENCE atendimento.protocolo_relatorio_seguradora_item_id_seq OWNED BY atendimento.protocolo_relatorio_seguradora_item.id;


--
-- TOC entry 398 (class 1259 OID 8862402)
-- Name: agenciador; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.agenciador (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    cidade_id bigint,
    banco_id bigint,
    coordenador_id bigint,
    nome character varying(150) NOT NULL,
    codigo character varying(80),
    tipo smallint,
    cpf character varying(30),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    rg character varying(30),
    orgao_rg character varying(30),
    data_emissao_rg date,
    susep character varying(50),
    inss character varying(50),
    issqn character varying(50),
    telefone character varying(150),
    email character varying(150),
    cep character varying(20),
    logradouro character varying(150),
    numero character varying(30),
    complemento character varying(80),
    bairro character varying(100),
    numero_dependentes integer,
    data_inscricao date,
    data_nascimento date,
    credenciado boolean,
    desativado boolean DEFAULT false NOT NULL,
    data_desativado date,
    agencia character varying(30),
    conta_corrente character varying(40),
    observacao text,
    legado_id integer NOT NULL,
    legado_ant_ven integer,
    legado_ant_ger integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE cadastro.agenciador OWNER TO postgres;

--
-- TOC entry 397 (class 1259 OID 8862401)
-- Name: agenciador_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.agenciador_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.agenciador_id_seq OWNER TO postgres;

--
-- TOC entry 6748 (class 0 OID 0)
-- Dependencies: 397
-- Name: agenciador_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.agenciador_id_seq OWNED BY cadastro.agenciador.id;


--
-- TOC entry 252 (class 1259 OID 8860360)
-- Name: cliente; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.cliente (
    id bigint NOT NULL,
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


ALTER TABLE cadastro.cliente OWNER TO postgres;

--
-- TOC entry 254 (class 1259 OID 8860386)
-- Name: cliente_dependente; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.cliente_dependente (
    id bigint NOT NULL,
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


ALTER TABLE cadastro.cliente_dependente OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 8860385)
-- Name: cliente_dependente_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.cliente_dependente_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.cliente_dependente_id_seq OWNER TO postgres;

--
-- TOC entry 6749 (class 0 OID 0)
-- Dependencies: 253
-- Name: cliente_dependente_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.cliente_dependente_id_seq OWNED BY cadastro.cliente_dependente.id;


--
-- TOC entry 251 (class 1259 OID 8860359)
-- Name: cliente_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.cliente_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.cliente_id_seq OWNER TO postgres;

--
-- TOC entry 6750 (class 0 OID 0)
-- Dependencies: 251
-- Name: cliente_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.cliente_id_seq OWNED BY cadastro.cliente.id;


--
-- TOC entry 244 (class 1259 OID 8860313)
-- Name: cliente_status; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.cliente_status (
    id smallint NOT NULL,
    codigo character varying(30) NOT NULL,
    nome character varying(80) NOT NULL,
    ativo boolean DEFAULT true NOT NULL
);


ALTER TABLE cadastro.cliente_status OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 8860312)
-- Name: cliente_status_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.cliente_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.cliente_status_id_seq OWNER TO postgres;

--
-- TOC entry 6751 (class 0 OID 0)
-- Dependencies: 243
-- Name: cliente_status_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.cliente_status_id_seq OWNED BY cadastro.cliente_status.id;


--
-- TOC entry 256 (class 1259 OID 8860406)
-- Name: cliente_vinculo; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.cliente_vinculo (
    id bigint NOT NULL,
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


ALTER TABLE cadastro.cliente_vinculo OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 8860405)
-- Name: cliente_vinculo_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.cliente_vinculo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.cliente_vinculo_id_seq OWNER TO postgres;

--
-- TOC entry 6752 (class 0 OID 0)
-- Dependencies: 255
-- Name: cliente_vinculo_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.cliente_vinculo_id_seq OWNED BY cadastro.cliente_vinculo.id;


--
-- TOC entry 291 (class 1259 OID 8860826)
-- Name: corretora; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.corretora (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    nome character varying(150) NOT NULL,
    codigo character varying(80),
    cidade_id bigint,
    cep character varying(20),
    logradouro character varying(150),
    numero character varying(30),
    complemento character varying(100),
    bairro character varying(100),
    telefone character varying(120),
    codigo_protheus character varying(50),
    ativo boolean DEFAULT true NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone,
    caminho_logotipo_legado character varying(300),
    logotipo_arquivo_id bigint,
    possui_logotipo_legado boolean DEFAULT false NOT NULL
);


ALTER TABLE cadastro.corretora OWNER TO postgres;

--
-- TOC entry 290 (class 1259 OID 8860825)
-- Name: corretora_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.corretora_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.corretora_id_seq OWNER TO postgres;

--
-- TOC entry 6753 (class 0 OID 0)
-- Dependencies: 290
-- Name: corretora_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.corretora_id_seq OWNED BY cadastro.corretora.id;


--
-- TOC entry 279 (class 1259 OID 8860666)
-- Name: estipulante; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.estipulante (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    nome character varying(150) NOT NULL,
    nome_formatado character varying(200),
    codigo character varying(80),
    tipo_pessoa smallint,
    cnpj character varying(30),
    cnpj_limpo character varying(20),
    cidade_id bigint,
    grupo_id bigint,
    seguradora_id bigint,
    ativo boolean DEFAULT true NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE cadastro.estipulante OWNER TO postgres;

--
-- TOC entry 281 (class 1259 OID 8860704)
-- Name: estipulante_configuracao; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.estipulante_configuracao (
    id bigint NOT NULL,
    estipulante_id bigint NOT NULL,
    tabela_legado_id integer,
    permite_propostas boolean,
    controla_comissao boolean,
    data_inicio_vigencia date,
    data_fim_vigencia date,
    data_aniversario date,
    data_ultimo_reajuste date,
    data_base_reajuste integer,
    data_limite_reajuste date,
    dias_aviso_reajuste integer,
    carencia integer,
    adesao_por character varying(80),
    custeio character varying(80),
    adesao character varying(80),
    faixa_etaria_inicio integer,
    faixa_etaria_fim integer,
    cancela_estipulante_id bigint,
    indice_legado_id integer,
    percentual_indice numeric(10,4),
    ajuste_indice boolean,
    ajuste_fator boolean,
    reajuste integer,
    tipo_cobertura_conjuge integer,
    percentual_tipo_cobertura_conjuge numeric(18,2),
    possui_excedente boolean,
    data_limite_excedente date,
    dias_aviso_excedente integer,
    prazo_regulacao integer,
    dia_corte integer,
    desconsiderar_proposta_ativa boolean DEFAULT false NOT NULL,
    permitir_protocolo_duplicado boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE cadastro.estipulante_configuracao OWNER TO postgres;

--
-- TOC entry 280 (class 1259 OID 8860703)
-- Name: estipulante_configuracao_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.estipulante_configuracao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.estipulante_configuracao_id_seq OWNER TO postgres;

--
-- TOC entry 6754 (class 0 OID 0)
-- Dependencies: 280
-- Name: estipulante_configuracao_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.estipulante_configuracao_id_seq OWNED BY cadastro.estipulante_configuracao.id;


--
-- TOC entry 278 (class 1259 OID 8860665)
-- Name: estipulante_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.estipulante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.estipulante_id_seq OWNER TO postgres;

--
-- TOC entry 6755 (class 0 OID 0)
-- Dependencies: 278
-- Name: estipulante_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.estipulante_id_seq OWNED BY cadastro.estipulante.id;


--
-- TOC entry 246 (class 1259 OID 8860323)
-- Name: grupo; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.grupo (
    id bigint NOT NULL,
    nome character varying(100) NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE cadastro.grupo OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 8860322)
-- Name: grupo_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.grupo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.grupo_id_seq OWNER TO postgres;

--
-- TOC entry 6756 (class 0 OID 0)
-- Dependencies: 245
-- Name: grupo_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.grupo_id_seq OWNED BY cadastro.grupo.id;


--
-- TOC entry 250 (class 1259 OID 8860346)
-- Name: lotacao; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.lotacao (
    id bigint NOT NULL,
    cidade_id bigint,
    nome character varying(100) NOT NULL,
    codigo character varying(50),
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE cadastro.lotacao OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 8860345)
-- Name: lotacao_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.lotacao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.lotacao_id_seq OWNER TO postgres;

--
-- TOC entry 6757 (class 0 OID 0)
-- Dependencies: 249
-- Name: lotacao_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.lotacao_id_seq OWNED BY cadastro.lotacao.id;


--
-- TOC entry 273 (class 1259 OID 8860625)
-- Name: seguradora; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.seguradora (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    nome character varying(150) NOT NULL,
    codigo character varying(50),
    susep character varying(50),
    cnpj character varying(30),
    cnpj_limpo character varying(20),
    ativo boolean DEFAULT true NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE cadastro.seguradora OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 8860624)
-- Name: seguradora_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.seguradora_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.seguradora_id_seq OWNER TO postgres;

--
-- TOC entry 6758 (class 0 OID 0)
-- Dependencies: 272
-- Name: seguradora_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.seguradora_id_seq OWNED BY cadastro.seguradora.id;


--
-- TOC entry 293 (class 1259 OID 8860851)
-- Name: subestipulante; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.subestipulante (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint,
    estipulante_id bigint,
    nome character varying(150) NOT NULL,
    codigo character varying(80),
    cidade_id bigint,
    banco_id bigint,
    cnpj character varying(30),
    cnpj_limpo character varying(20),
    ativo boolean DEFAULT true NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE cadastro.subestipulante OWNER TO postgres;

--
-- TOC entry 292 (class 1259 OID 8860850)
-- Name: subestipulante_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.subestipulante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.subestipulante_id_seq OWNER TO postgres;

--
-- TOC entry 6759 (class 0 OID 0)
-- Dependencies: 292
-- Name: subestipulante_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.subestipulante_id_seq OWNED BY cadastro.subestipulante.id;


--
-- TOC entry 248 (class 1259 OID 8860332)
-- Name: subgrupo; Type: TABLE; Schema: cadastro; Owner: postgres
--

CREATE TABLE cadastro.subgrupo (
    id bigint NOT NULL,
    grupo_id bigint,
    nome character varying(100) NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE cadastro.subgrupo OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 8860331)
-- Name: subgrupo_id_seq; Type: SEQUENCE; Schema: cadastro; Owner: postgres
--

CREATE SEQUENCE cadastro.subgrupo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE cadastro.subgrupo_id_seq OWNER TO postgres;

--
-- TOC entry 6760 (class 0 OID 0)
-- Dependencies: 247
-- Name: subgrupo_id_seq; Type: SEQUENCE OWNED BY; Schema: cadastro; Owner: postgres
--

ALTER SEQUENCE cadastro.subgrupo_id_seq OWNED BY cadastro.subgrupo.id;


--
-- TOC entry 400 (class 1259 OID 8862449)
-- Name: agenciador_comissao_config; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.agenciador_comissao_config (
    id bigint NOT NULL,
    agenciador_id bigint NOT NULL,
    percentual_padrao numeric(10,4),
    percentual_repasse numeric(10,4),
    inicio_vigencia date,
    fim_vigencia date,
    ativo boolean DEFAULT true NOT NULL,
    origem character varying(80) DEFAULT 'legado'::character varying NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.agenciador_comissao_config OWNER TO postgres;

--
-- TOC entry 399 (class 1259 OID 8862448)
-- Name: agenciador_comissao_config_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.agenciador_comissao_config_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.agenciador_comissao_config_id_seq OWNER TO postgres;

--
-- TOC entry 6761 (class 0 OID 0)
-- Dependencies: 399
-- Name: agenciador_comissao_config_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.agenciador_comissao_config_id_seq OWNED BY comissao.agenciador_comissao_config.id;


--
-- TOC entry 410 (class 1259 OID 8862592)
-- Name: agenciamento_corretora_lancamento; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.agenciamento_corretora_lancamento (
    id bigint NOT NULL,
    proposta_id bigint,
    corretora_id bigint,
    movimento_tipo_id bigint,
    percentual numeric(10,4),
    valor_premio numeric(18,2),
    valor_agenciamento numeric(18,2),
    parcela_inicial integer,
    parcela_final integer,
    status_legado integer,
    valor_pago numeric(18,2),
    data_pagamento date,
    gerou_fatura boolean,
    data_cadastro date,
    data_vencimento date,
    legado_id integer NOT NULL,
    legado_proposta_id integer,
    legado_corretora_id integer,
    legado_movimento_id integer,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.agenciamento_corretora_lancamento OWNER TO postgres;

--
-- TOC entry 409 (class 1259 OID 8862591)
-- Name: agenciamento_corretora_lancamento_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.agenciamento_corretora_lancamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.agenciamento_corretora_lancamento_id_seq OWNER TO postgres;

--
-- TOC entry 6762 (class 0 OID 0)
-- Dependencies: 409
-- Name: agenciamento_corretora_lancamento_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.agenciamento_corretora_lancamento_id_seq OWNED BY comissao.agenciamento_corretora_lancamento.id;


--
-- TOC entry 402 (class 1259 OID 8862467)
-- Name: corretora_agenciador; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.corretora_agenciador (
    id bigint NOT NULL,
    corretora_id bigint,
    agenciador_id bigint,
    percentual_agenciamento numeric(10,4),
    percentual_repasse numeric(10,4),
    inicio_vigencia date,
    fim_vigencia date,
    ativo boolean DEFAULT true NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.corretora_agenciador OWNER TO postgres;

--
-- TOC entry 401 (class 1259 OID 8862466)
-- Name: corretora_agenciador_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.corretora_agenciador_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.corretora_agenciador_id_seq OWNER TO postgres;

--
-- TOC entry 6763 (class 0 OID 0)
-- Dependencies: 401
-- Name: corretora_agenciador_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.corretora_agenciador_id_seq OWNED BY comissao.corretora_agenciador.id;


--
-- TOC entry 285 (class 1259 OID 8860764)
-- Name: estipulante_comissao_config; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.estipulante_comissao_config (
    id bigint NOT NULL,
    estipulante_id bigint NOT NULL,
    percentual_comissao numeric(10,4),
    percentual_agenciamento numeric(10,4),
    percentual_bonus numeric(10,4),
    comissao_apartir_parcela integer,
    agenciador_id bigint,
    agenciador_percentual_repasse numeric(10,4),
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.estipulante_comissao_config OWNER TO postgres;

--
-- TOC entry 284 (class 1259 OID 8860763)
-- Name: estipulante_comissao_config_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.estipulante_comissao_config_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.estipulante_comissao_config_id_seq OWNER TO postgres;

--
-- TOC entry 6764 (class 0 OID 0)
-- Dependencies: 284
-- Name: estipulante_comissao_config_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.estipulante_comissao_config_id_seq OWNED BY comissao.estipulante_comissao_config.id;


--
-- TOC entry 430 (class 1259 OID 8862819)
-- Name: fatura_comissao_resumo; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.fatura_comissao_resumo (
    id bigint NOT NULL,
    estipulante_id bigint,
    mes character varying(10),
    ano character varying(10),
    competencia_int integer,
    premio_pagamento numeric(18,2),
    valor_pago numeric(18,2),
    data_pagamento date,
    legado_id integer NOT NULL,
    legado_estipulante_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.fatura_comissao_resumo OWNER TO postgres;

--
-- TOC entry 429 (class 1259 OID 8862818)
-- Name: fatura_comissao_resumo_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.fatura_comissao_resumo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.fatura_comissao_resumo_id_seq OWNER TO postgres;

--
-- TOC entry 6765 (class 0 OID 0)
-- Dependencies: 429
-- Name: fatura_comissao_resumo_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.fatura_comissao_resumo_id_seq OWNED BY comissao.fatura_comissao_resumo.id;


--
-- TOC entry 424 (class 1259 OID 8862746)
-- Name: fatura_integracao; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.fatura_integracao (
    id bigint NOT NULL,
    corretora_id bigint,
    seguradora_id bigint,
    estipulante_id bigint,
    corretora_codigo_original character varying(40),
    seguradora_codigo_original character varying(40),
    data_lancamento timestamp with time zone,
    data_vencimento date,
    data_recebimento date,
    valor_receber numeric(18,2),
    valor_recebido numeric(18,2),
    valor_fatura numeric(18,2),
    situacao_legado integer,
    tipo character varying(20),
    mes integer,
    ano integer,
    competencia_int integer,
    gerou_arquivo boolean,
    alterado integer,
    percentual_agenciamento numeric(10,4),
    percentual_corretagem numeric(10,4),
    legado_id integer NOT NULL,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.fatura_integracao OWNER TO postgres;

--
-- TOC entry 423 (class 1259 OID 8862745)
-- Name: fatura_integracao_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.fatura_integracao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.fatura_integracao_id_seq OWNER TO postgres;

--
-- TOC entry 6766 (class 0 OID 0)
-- Dependencies: 423
-- Name: fatura_integracao_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.fatura_integracao_id_seq OWNED BY comissao.fatura_integracao.id;


--
-- TOC entry 426 (class 1259 OID 8862779)
-- Name: fatura_vida_agenciamento; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.fatura_vida_agenciamento (
    id bigint NOT NULL,
    origem_legado character varying(80) NOT NULL,
    proposta_id bigint,
    premio numeric(18,2),
    iof numeric(18,2),
    premio_liquido numeric(18,2),
    valor_agenciamento numeric(18,2),
    valor_recebido numeric(18,2),
    valor_diferenca numeric(18,2),
    codigo_cooperado_original character varying(40),
    codigo_corretora_original character varying(40),
    tipo_agenciamento character varying(60),
    numero_nf character varying(120),
    data_inclusao timestamp with time zone,
    data_registro timestamp with time zone,
    legado_id integer NOT NULL,
    legado_proposta_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.fatura_vida_agenciamento OWNER TO postgres;

--
-- TOC entry 425 (class 1259 OID 8862778)
-- Name: fatura_vida_agenciamento_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.fatura_vida_agenciamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.fatura_vida_agenciamento_id_seq OWNER TO postgres;

--
-- TOC entry 6767 (class 0 OID 0)
-- Dependencies: 425
-- Name: fatura_vida_agenciamento_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.fatura_vida_agenciamento_id_seq OWNED BY comissao.fatura_vida_agenciamento.id;


--
-- TOC entry 428 (class 1259 OID 8862797)
-- Name: fatura_vida_recebimento; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.fatura_vida_recebimento (
    id bigint NOT NULL,
    fatura_vida_agenciamento_id bigint,
    estipulante_id bigint,
    data_pagamento date,
    valor numeric(18,2),
    observacao character varying(150),
    legado_id integer NOT NULL,
    legado_fatura_vida_id integer,
    legado_estipulante_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.fatura_vida_recebimento OWNER TO postgres;

--
-- TOC entry 427 (class 1259 OID 8862796)
-- Name: fatura_vida_recebimento_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.fatura_vida_recebimento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.fatura_vida_recebimento_id_seq OWNER TO postgres;

--
-- TOC entry 6768 (class 0 OID 0)
-- Dependencies: 427
-- Name: fatura_vida_recebimento_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.fatura_vida_recebimento_id_seq OWNED BY comissao.fatura_vida_recebimento.id;


--
-- TOC entry 347 (class 1259 OID 8861616)
-- Name: lancamento_comissao; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.lancamento_comissao (
    id bigint NOT NULL,
    proposta_movimento_id bigint,
    titulo_id bigint,
    proposta_id bigint,
    pessoa_id bigint,
    cliente_id bigint,
    estipulante_id bigint,
    competencia_ano integer,
    competencia_mes integer,
    competencia_int integer,
    valor_base numeric(18,2),
    valor_bruto numeric(18,2),
    valor_liquido numeric(18,2),
    gerado character(1),
    status character varying(40) DEFAULT 'pendente'::character varying NOT NULL,
    origem character varying(50) DEFAULT 'movimento_proposta_legado'::character varying NOT NULL,
    legado_movimento_proposta_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.lancamento_comissao OWNER TO postgres;

--
-- TOC entry 346 (class 1259 OID 8861615)
-- Name: lancamento_comissao_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.lancamento_comissao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.lancamento_comissao_id_seq OWNER TO postgres;

--
-- TOC entry 6769 (class 0 OID 0)
-- Dependencies: 346
-- Name: lancamento_comissao_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.lancamento_comissao_id_seq OWNED BY comissao.lancamento_comissao.id;


--
-- TOC entry 432 (class 1259 OID 8862835)
-- Name: lancamento_fatura_estipulante; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.lancamento_fatura_estipulante (
    id bigint NOT NULL,
    estipulante_id bigint,
    corretora_id bigint,
    competencia_original character varying(40),
    competencia_mes integer,
    competencia_ano integer,
    competencia_int integer,
    premio_total numeric(18,2),
    valor_faturado numeric(18,2),
    percentual_corretagem numeric(10,4),
    comissao_recebida numeric(18,2),
    data_vencimento_fatura date,
    data_recebimento date,
    lancamento_manual boolean,
    legado_id integer NOT NULL,
    legado_estipulante_id integer,
    legado_corretora_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE comissao.lancamento_fatura_estipulante OWNER TO postgres;

--
-- TOC entry 431 (class 1259 OID 8862834)
-- Name: lancamento_fatura_estipulante_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.lancamento_fatura_estipulante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.lancamento_fatura_estipulante_id_seq OWNER TO postgres;

--
-- TOC entry 6770 (class 0 OID 0)
-- Dependencies: 431
-- Name: lancamento_fatura_estipulante_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.lancamento_fatura_estipulante_id_seq OWNED BY comissao.lancamento_fatura_estipulante.id;


--
-- TOC entry 301 (class 1259 OID 8861032)
-- Name: proposta_participante; Type: TABLE; Schema: comissao; Owner: postgres
--

CREATE TABLE comissao.proposta_participante (
    id bigint NOT NULL,
    proposta_id bigint NOT NULL,
    participante_tipo character varying(40) NOT NULL,
    participante_id bigint,
    codigo_agenciamento character varying(80),
    percentual_agenciamento numeric(18,4),
    agenciamento_parcela_inicial integer,
    agenciamento_parcela_final integer,
    bonus numeric(18,2),
    percentual_carteira numeric(18,4),
    carteira_parcela_inicial integer,
    ativo boolean DEFAULT true NOT NULL,
    legado_campo_origem character varying(80),
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    agenciador_id bigint,
    corretora_id bigint,
    codigo_legado_participante integer
);


ALTER TABLE comissao.proposta_participante OWNER TO postgres;

--
-- TOC entry 300 (class 1259 OID 8861031)
-- Name: proposta_participante_id_seq; Type: SEQUENCE; Schema: comissao; Owner: postgres
--

CREATE SEQUENCE comissao.proposta_participante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE comissao.proposta_participante_id_seq OWNER TO postgres;

--
-- TOC entry 6771 (class 0 OID 0)
-- Dependencies: 300
-- Name: proposta_participante_id_seq; Type: SEQUENCE OWNED BY; Schema: comissao; Owner: postgres
--

ALTER SEQUENCE comissao.proposta_participante_id_seq OWNED BY comissao.proposta_participante.id;


--
-- TOC entry 262 (class 1259 OID 8860491)
-- Name: corsan_cliente; Type: TABLE; Schema: convenio; Owner: postgres
--

CREATE TABLE convenio.corsan_cliente (
    id bigint NOT NULL,
    cliente_id bigint NOT NULL,
    cliente_vinculo_id bigint,
    pessoa_id bigint NOT NULL,
    empresa character varying(100),
    rubrica character varying(100),
    grupo character varying(100),
    funcionario boolean,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE convenio.corsan_cliente OWNER TO postgres;

--
-- TOC entry 261 (class 1259 OID 8860490)
-- Name: corsan_cliente_id_seq; Type: SEQUENCE; Schema: convenio; Owner: postgres
--

CREATE SEQUENCE convenio.corsan_cliente_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE convenio.corsan_cliente_id_seq OWNER TO postgres;

--
-- TOC entry 6772 (class 0 OID 0)
-- Dependencies: 261
-- Name: corsan_cliente_id_seq; Type: SEQUENCE OWNED BY; Schema: convenio; Owner: postgres
--

ALTER SEQUENCE convenio.corsan_cliente_id_seq OWNED BY convenio.corsan_cliente.id;


--
-- TOC entry 299 (class 1259 OID 8861001)
-- Name: corsan_proposta; Type: TABLE; Schema: convenio; Owner: postgres
--

CREATE TABLE convenio.corsan_proposta (
    id bigint NOT NULL,
    proposta_id bigint NOT NULL,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    pessoa_id bigint,
    empresa character varying(20),
    rubrica character varying(20),
    grupo character varying(20),
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE convenio.corsan_proposta OWNER TO postgres;

--
-- TOC entry 298 (class 1259 OID 8861000)
-- Name: corsan_proposta_id_seq; Type: SEQUENCE; Schema: convenio; Owner: postgres
--

CREATE SEQUENCE convenio.corsan_proposta_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE convenio.corsan_proposta_id_seq OWNER TO postgres;

--
-- TOC entry 6773 (class 0 OID 0)
-- Dependencies: 298
-- Name: corsan_proposta_id_seq; Type: SEQUENCE OWNED BY; Schema: convenio; Owner: postgres
--

ALTER SEQUENCE convenio.corsan_proposta_id_seq OWNED BY convenio.corsan_proposta.id;


--
-- TOC entry 260 (class 1259 OID 8860460)
-- Name: siape_cliente; Type: TABLE; Schema: convenio; Owner: postgres
--

CREATE TABLE convenio.siape_cliente (
    id bigint NOT NULL,
    cliente_id bigint NOT NULL,
    cliente_vinculo_id bigint,
    pessoa_id bigint NOT NULL,
    siape character varying(100),
    orgao_id bigint,
    categoria character varying(30),
    setor character varying(30),
    instituto character varying(30),
    agencia character varying(30),
    funcao character varying(30),
    contrato character varying(100),
    digito_verificador character varying(10),
    instituidor character varying(100),
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE convenio.siape_cliente OWNER TO postgres;

--
-- TOC entry 259 (class 1259 OID 8860459)
-- Name: siape_cliente_id_seq; Type: SEQUENCE; Schema: convenio; Owner: postgres
--

CREATE SEQUENCE convenio.siape_cliente_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE convenio.siape_cliente_id_seq OWNER TO postgres;

--
-- TOC entry 6774 (class 0 OID 0)
-- Dependencies: 259
-- Name: siape_cliente_id_seq; Type: SEQUENCE OWNED BY; Schema: convenio; Owner: postgres
--

ALTER SEQUENCE convenio.siape_cliente_id_seq OWNED BY convenio.siape_cliente.id;


--
-- TOC entry 258 (class 1259 OID 8860451)
-- Name: siape_orgao; Type: TABLE; Schema: convenio; Owner: postgres
--

CREATE TABLE convenio.siape_orgao (
    id bigint NOT NULL,
    codigo character varying(50),
    nome character varying(150),
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE convenio.siape_orgao OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 8860450)
-- Name: siape_orgao_id_seq; Type: SEQUENCE; Schema: convenio; Owner: postgres
--

CREATE SEQUENCE convenio.siape_orgao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE convenio.siape_orgao_id_seq OWNER TO postgres;

--
-- TOC entry 6775 (class 0 OID 0)
-- Dependencies: 257
-- Name: siape_orgao_id_seq; Type: SEQUENCE OWNED BY; Schema: convenio; Owner: postgres
--

ALTER SEQUENCE convenio.siape_orgao_id_seq OWNED BY convenio.siape_orgao.id;


--
-- TOC entry 277 (class 1259 OID 8860655)
-- Name: siape_parametro; Type: TABLE; Schema: convenio; Owner: postgres
--

CREATE TABLE convenio.siape_parametro (
    id bigint NOT NULL,
    empresa character varying(100),
    cgc character varying(30),
    cgc_limpo character varying(20),
    rubrica character varying(50),
    comando character varying(50),
    custo_linha numeric(18,2),
    calculo_parametro character varying(50),
    legado_id integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE convenio.siape_parametro OWNER TO postgres;

--
-- TOC entry 276 (class 1259 OID 8860654)
-- Name: siape_parametro_id_seq; Type: SEQUENCE; Schema: convenio; Owner: postgres
--

CREATE SEQUENCE convenio.siape_parametro_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE convenio.siape_parametro_id_seq OWNER TO postgres;

--
-- TOC entry 6776 (class 0 OID 0)
-- Dependencies: 276
-- Name: siape_parametro_id_seq; Type: SEQUENCE OWNED BY; Schema: convenio; Owner: postgres
--

ALTER SEQUENCE convenio.siape_parametro_id_seq OWNED BY convenio.siape_parametro.id;


--
-- TOC entry 242 (class 1259 OID 8860301)
-- Name: banco; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.banco (
    id bigint NOT NULL,
    codigo character varying(20),
    nome character varying(100) NOT NULL,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE core.banco OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 8860300)
-- Name: banco_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.banco_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.banco_id_seq OWNER TO postgres;

--
-- TOC entry 6777 (class 0 OID 0)
-- Dependencies: 241
-- Name: banco_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.banco_id_seq OWNED BY core.banco.id;


--
-- TOC entry 238 (class 1259 OID 8860264)
-- Name: cidade; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.cidade (
    id bigint NOT NULL,
    estado_id bigint,
    nome character varying(100) NOT NULL,
    nome_normalizado character varying(100),
    uf character(2),
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE core.cidade OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 8860263)
-- Name: cidade_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.cidade_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.cidade_id_seq OWNER TO postgres;

--
-- TOC entry 6778 (class 0 OID 0)
-- Dependencies: 237
-- Name: cidade_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.cidade_id_seq OWNED BY core.cidade.id;


--
-- TOC entry 236 (class 1259 OID 8860255)
-- Name: estado; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.estado (
    id bigint NOT NULL,
    uf character(2) NOT NULL,
    nome character varying(100)
);


ALTER TABLE core.estado OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 8860254)
-- Name: estado_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.estado_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.estado_id_seq OWNER TO postgres;

--
-- TOC entry 6779 (class 0 OID 0)
-- Dependencies: 235
-- Name: estado_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.estado_id_seq OWNED BY core.estado.id;


--
-- TOC entry 230 (class 1259 OID 8860205)
-- Name: pessoa; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.pessoa (
    id bigint NOT NULL,
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


ALTER TABLE core.pessoa OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 8860238)
-- Name: pessoa_contato; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.pessoa_contato (
    id bigint NOT NULL,
    pessoa_id bigint NOT NULL,
    tipo_contato character varying(30) NOT NULL,
    valor character varying(150) NOT NULL,
    valor_normalizado character varying(150),
    principal boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE core.pessoa_contato OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 8860237)
-- Name: pessoa_contato_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.pessoa_contato_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.pessoa_contato_id_seq OWNER TO postgres;

--
-- TOC entry 6780 (class 0 OID 0)
-- Dependencies: 233
-- Name: pessoa_contato_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.pessoa_contato_id_seq OWNED BY core.pessoa_contato.id;


--
-- TOC entry 232 (class 1259 OID 8860222)
-- Name: pessoa_documento; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.pessoa_documento (
    id bigint NOT NULL,
    pessoa_id bigint NOT NULL,
    tipo_documento character varying(30) NOT NULL,
    numero character varying(50),
    numero_limpo character varying(50),
    orgao_emissor character varying(50),
    data_emissao date,
    principal boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE core.pessoa_documento OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 8860221)
-- Name: pessoa_documento_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.pessoa_documento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.pessoa_documento_id_seq OWNER TO postgres;

--
-- TOC entry 6781 (class 0 OID 0)
-- Dependencies: 231
-- Name: pessoa_documento_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.pessoa_documento_id_seq OWNED BY core.pessoa_documento.id;


--
-- TOC entry 240 (class 1259 OID 8860279)
-- Name: pessoa_endereco; Type: TABLE; Schema: core; Owner: postgres
--

CREATE TABLE core.pessoa_endereco (
    id bigint NOT NULL,
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


ALTER TABLE core.pessoa_endereco OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 8860278)
-- Name: pessoa_endereco_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.pessoa_endereco_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.pessoa_endereco_id_seq OWNER TO postgres;

--
-- TOC entry 6782 (class 0 OID 0)
-- Dependencies: 239
-- Name: pessoa_endereco_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.pessoa_endereco_id_seq OWNED BY core.pessoa_endereco.id;


--
-- TOC entry 229 (class 1259 OID 8860204)
-- Name: pessoa_id_seq; Type: SEQUENCE; Schema: core; Owner: postgres
--

CREATE SEQUENCE core.pessoa_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE core.pessoa_id_seq OWNER TO postgres;

--
-- TOC entry 6783 (class 0 OID 0)
-- Dependencies: 229
-- Name: pessoa_id_seq; Type: SEQUENCE OWNED BY; Schema: core; Owner: postgres
--

ALTER SEQUENCE core.pessoa_id_seq OWNED BY core.pessoa.id;


--
-- TOC entry 372 (class 1259 OID 8862046)
-- Name: arquivo; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.arquivo (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    storage_provider_id smallint,
    bucket character varying(120),
    storage_key text,
    storage_path text,
    nome_original character varying(255),
    nome_armazenado character varying(255),
    titulo character varying(300),
    descricao text,
    extensao character varying(20),
    mime_type character varying(120),
    tamanho_bytes bigint,
    hash_sha256 character varying(64),
    data_documento date,
    data_upload timestamp with time zone,
    hora_original character varying(20),
    origem character varying(50) DEFAULT 'legado'::character varying NOT NULL,
    caminho_legado text,
    arquivo_legado character varying(255),
    status character varying(40) DEFAULT 'ativo'::character varying NOT NULL,
    criado_por_usuario_id bigint,
    criado_por_usuario_legado_id integer,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone,
    extensao_original character varying(50),
    extensao_normalizada character varying(20),
    extensao_confiavel boolean DEFAULT true NOT NULL,
    migracao_status character varying(40) DEFAULT 'pendente'::character varying NOT NULL,
    migracao_erro text
);


ALTER TABLE documento.arquivo OWNER TO postgres;

--
-- TOC entry 378 (class 1259 OID 8862122)
-- Name: arquivo_acesso_log; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.arquivo_acesso_log (
    id bigint NOT NULL,
    arquivo_id bigint NOT NULL,
    usuario_id bigint,
    usuario_legado_id integer,
    acao character varying(40) NOT NULL,
    ip_origem character varying(80),
    user_agent text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE documento.arquivo_acesso_log OWNER TO postgres;

--
-- TOC entry 377 (class 1259 OID 8862121)
-- Name: arquivo_acesso_log_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.arquivo_acesso_log_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.arquivo_acesso_log_id_seq OWNER TO postgres;

--
-- TOC entry 6784 (class 0 OID 0)
-- Dependencies: 377
-- Name: arquivo_acesso_log_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.arquivo_acesso_log_id_seq OWNED BY documento.arquivo_acesso_log.id;


--
-- TOC entry 371 (class 1259 OID 8862045)
-- Name: arquivo_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.arquivo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.arquivo_id_seq OWNER TO postgres;

--
-- TOC entry 6785 (class 0 OID 0)
-- Dependencies: 371
-- Name: arquivo_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.arquivo_id_seq OWNED BY documento.arquivo.id;


--
-- TOC entry 376 (class 1259 OID 8862098)
-- Name: arquivo_versao; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.arquivo_versao (
    id bigint NOT NULL,
    arquivo_id bigint NOT NULL,
    versao integer DEFAULT 1 NOT NULL,
    storage_provider_id smallint,
    bucket character varying(120),
    storage_key text,
    storage_path text,
    nome_original character varying(255),
    extensao character varying(20),
    mime_type character varying(120),
    tamanho_bytes bigint,
    hash_sha256 character varying(64),
    motivo character varying(150),
    criado_por_usuario_id bigint,
    criado_por_usuario_legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE documento.arquivo_versao OWNER TO postgres;

--
-- TOC entry 375 (class 1259 OID 8862097)
-- Name: arquivo_versao_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.arquivo_versao_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.arquivo_versao_id_seq OWNER TO postgres;

--
-- TOC entry 6786 (class 0 OID 0)
-- Dependencies: 375
-- Name: arquivo_versao_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.arquivo_versao_id_seq OWNED BY documento.arquivo_versao.id;


--
-- TOC entry 374 (class 1259 OID 8862072)
-- Name: arquivo_vinculo; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.arquivo_vinculo (
    id bigint NOT NULL,
    arquivo_id bigint NOT NULL,
    tipo_anexo_id bigint,
    entidade_tipo character varying(50) NOT NULL,
    entidade_id bigint NOT NULL,
    entidade_legado_id integer,
    principal boolean DEFAULT false NOT NULL,
    obrigatorio boolean DEFAULT false NOT NULL,
    observacao text,
    legado_origem_coluna character varying(80),
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone,
    entidade_legado_tipo character varying(50),
    entidade_legado_chave_1 character varying(80),
    entidade_legado_chave_2 character varying(80),
    criterio_resolucao character varying(100),
    vinculo_resolvido boolean DEFAULT true NOT NULL,
    entidade_legado_chave_concatenada character varying(120)
);


ALTER TABLE documento.arquivo_vinculo OWNER TO postgres;

--
-- TOC entry 373 (class 1259 OID 8862071)
-- Name: arquivo_vinculo_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.arquivo_vinculo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.arquivo_vinculo_id_seq OWNER TO postgres;

--
-- TOC entry 6787 (class 0 OID 0)
-- Dependencies: 373
-- Name: arquivo_vinculo_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.arquivo_vinculo_id_seq OWNED BY documento.arquivo_vinculo.id;


--
-- TOC entry 368 (class 1259 OID 8862017)
-- Name: storage_provider; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.storage_provider (
    id smallint NOT NULL,
    codigo character varying(50) NOT NULL,
    nome character varying(100) NOT NULL,
    descricao text,
    ativo boolean DEFAULT true NOT NULL
);


ALTER TABLE documento.storage_provider OWNER TO postgres;

--
-- TOC entry 367 (class 1259 OID 8862016)
-- Name: storage_provider_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.storage_provider_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.storage_provider_id_seq OWNER TO postgres;

--
-- TOC entry 6788 (class 0 OID 0)
-- Dependencies: 367
-- Name: storage_provider_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.storage_provider_id_seq OWNED BY documento.storage_provider.id;


--
-- TOC entry 370 (class 1259 OID 8862029)
-- Name: tipo_anexo; Type: TABLE; Schema: documento; Owner: postgres
--

CREATE TABLE documento.tipo_anexo (
    id bigint NOT NULL,
    codigo character varying(80),
    nome character varying(120) NOT NULL,
    categoria character varying(60),
    descricao text,
    exige_validade boolean DEFAULT false NOT NULL,
    exige_assinatura boolean DEFAULT false NOT NULL,
    sensivel boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    legado_valor_original character varying(120),
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE documento.tipo_anexo OWNER TO postgres;

--
-- TOC entry 369 (class 1259 OID 8862028)
-- Name: tipo_anexo_id_seq; Type: SEQUENCE; Schema: documento; Owner: postgres
--

CREATE SEQUENCE documento.tipo_anexo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE documento.tipo_anexo_id_seq OWNER TO postgres;

--
-- TOC entry 6789 (class 0 OID 0)
-- Dependencies: 369
-- Name: tipo_anexo_id_seq; Type: SEQUENCE OWNED BY; Schema: documento; Owner: postgres
--

ALTER SEQUENCE documento.tipo_anexo_id_seq OWNED BY documento.tipo_anexo.id;


--
-- TOC entry 422 (class 1259 OID 8862723)
-- Name: cobranca_acompanhamento; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.cobranca_acompanhamento (
    id bigint NOT NULL,
    pessoa_id bigint,
    cliente_id bigint,
    data_acompanhamento date,
    hora_original character varying(30),
    contato character varying(150),
    descricao text,
    usuario_legado_id integer,
    legado_id integer NOT NULL,
    legado_cliente_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.cobranca_acompanhamento OWNER TO postgres;

--
-- TOC entry 421 (class 1259 OID 8862722)
-- Name: cobranca_acompanhamento_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.cobranca_acompanhamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.cobranca_acompanhamento_id_seq OWNER TO postgres;

--
-- TOC entry 6790 (class 0 OID 0)
-- Dependencies: 421
-- Name: cobranca_acompanhamento_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.cobranca_acompanhamento_id_seq OWNED BY financeiro.cobranca_acompanhamento.id;


--
-- TOC entry 268 (class 1259 OID 8860544)
-- Name: conta_cobranca; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.conta_cobranca (
    id bigint NOT NULL,
    pessoa_id bigint NOT NULL,
    cliente_id bigint NOT NULL,
    cliente_vinculo_id bigint NOT NULL,
    estipulante_id bigint,
    subestipulante_id bigint,
    convenio_cobranca_id bigint,
    regra_agrupamento_id smallint NOT NULL,
    identificador_agrupamento character varying(160) NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.conta_cobranca OWNER TO postgres;

--
-- TOC entry 267 (class 1259 OID 8860543)
-- Name: conta_cobranca_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.conta_cobranca_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.conta_cobranca_id_seq OWNER TO postgres;

--
-- TOC entry 6791 (class 0 OID 0)
-- Dependencies: 267
-- Name: conta_cobranca_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.conta_cobranca_id_seq OWNED BY financeiro.conta_cobranca.id;


--
-- TOC entry 266 (class 1259 OID 8860527)
-- Name: convenio_cobranca; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.convenio_cobranca (
    id bigint NOT NULL,
    banco_id bigint,
    nome character varying(150),
    agencia character varying(30),
    conta_corrente character varying(30),
    nome_empresa character varying(150),
    codigo_empresa character varying(80),
    numero_arquivo integer,
    nome_inicial_arquivo character varying(80),
    extensao_arquivo character varying(10),
    layout_arquivo smallint,
    local_remessa_arquivo text,
    local_retorno_arquivo text,
    comunica_vindi boolean,
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    inscricao_estadual character varying(40),
    est_endereco character varying(150),
    est_numero character varying(100),
    est_bairro character varying(100),
    est_complemento character varying(100),
    est_cep character varying(20),
    est_cidade character varying(100),
    est_uf character varying(4),
    est_nome character varying(120)
);


ALTER TABLE financeiro.convenio_cobranca OWNER TO postgres;

--
-- TOC entry 265 (class 1259 OID 8860526)
-- Name: convenio_cobranca_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.convenio_cobranca_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.convenio_cobranca_id_seq OWNER TO postgres;

--
-- TOC entry 6792 (class 0 OID 0)
-- Dependencies: 265
-- Name: convenio_cobranca_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.convenio_cobranca_id_seq OWNED BY financeiro.convenio_cobranca.id;


--
-- TOC entry 283 (class 1259 OID 8860727)
-- Name: estipulante_faturamento_config; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.estipulante_faturamento_config (
    id bigint NOT NULL,
    estipulante_id bigint NOT NULL,
    forma_pagamento_id bigint,
    convenio_cobranca_id bigint,
    regra_agrupamento_fatura_id smallint,
    dia_debito integer,
    iof_vg numeric(10,4),
    iof_inc numeric(10,4),
    iof_ap numeric(10,4),
    numero_proposta_vg character varying(50),
    numero_proposta_inc character varying(50),
    numero_proposta_ap character varying(50),
    sorteio_valor numeric(18,2),
    saf character varying(80),
    campanha integer,
    parametro_siape_id bigint,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.estipulante_faturamento_config OWNER TO postgres;

--
-- TOC entry 282 (class 1259 OID 8860726)
-- Name: estipulante_faturamento_config_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.estipulante_faturamento_config_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.estipulante_faturamento_config_id_seq OWNER TO postgres;

--
-- TOC entry 6793 (class 0 OID 0)
-- Dependencies: 282
-- Name: estipulante_faturamento_config_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.estipulante_faturamento_config_id_seq OWNED BY financeiro.estipulante_faturamento_config.id;


--
-- TOC entry 275 (class 1259 OID 8860645)
-- Name: forma_pagamento_estipulante; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.forma_pagamento_estipulante (
    id bigint NOT NULL,
    nome character varying(100) NOT NULL,
    codigo character varying(50),
    legado_id integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.forma_pagamento_estipulante OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 8860644)
-- Name: forma_pagamento_estipulante_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.forma_pagamento_estipulante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.forma_pagamento_estipulante_id_seq OWNER TO postgres;

--
-- TOC entry 6794 (class 0 OID 0)
-- Dependencies: 274
-- Name: forma_pagamento_estipulante_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.forma_pagamento_estipulante_id_seq OWNED BY financeiro.forma_pagamento_estipulante.id;


--
-- TOC entry 414 (class 1259 OID 8862659)
-- Name: forma_retorno; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.forma_retorno (
    id bigint NOT NULL,
    nome character varying(150) NOT NULL,
    legado_id integer NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.forma_retorno OWNER TO postgres;

--
-- TOC entry 416 (class 1259 OID 8862670)
-- Name: forma_retorno_estipulante; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.forma_retorno_estipulante (
    id bigint NOT NULL,
    forma_retorno_id bigint NOT NULL,
    estipulante_id bigint,
    legado_id integer NOT NULL,
    legado_forma_retorno_id integer,
    legado_estipulante_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.forma_retorno_estipulante OWNER TO postgres;

--
-- TOC entry 415 (class 1259 OID 8862669)
-- Name: forma_retorno_estipulante_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.forma_retorno_estipulante_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.forma_retorno_estipulante_id_seq OWNER TO postgres;

--
-- TOC entry 6795 (class 0 OID 0)
-- Dependencies: 415
-- Name: forma_retorno_estipulante_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.forma_retorno_estipulante_id_seq OWNED BY financeiro.forma_retorno_estipulante.id;


--
-- TOC entry 413 (class 1259 OID 8862658)
-- Name: forma_retorno_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.forma_retorno_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.forma_retorno_id_seq OWNER TO postgres;

--
-- TOC entry 6796 (class 0 OID 0)
-- Dependencies: 413
-- Name: forma_retorno_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.forma_retorno_id_seq OWNED BY financeiro.forma_retorno.id;


--
-- TOC entry 418 (class 1259 OID 8862691)
-- Name: identificador_remessa_api; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.identificador_remessa_api (
    id bigint NOT NULL,
    usuario_legado_id integer,
    datahora timestamp with time zone,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.identificador_remessa_api OWNER TO postgres;

--
-- TOC entry 417 (class 1259 OID 8862690)
-- Name: identificador_remessa_api_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.identificador_remessa_api_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.identificador_remessa_api_id_seq OWNER TO postgres;

--
-- TOC entry 6797 (class 0 OID 0)
-- Dependencies: 417
-- Name: identificador_remessa_api_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.identificador_remessa_api_id_seq OWNED BY financeiro.identificador_remessa_api.id;


--
-- TOC entry 420 (class 1259 OID 8862701)
-- Name: movimento_cobranca_log; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.movimento_cobranca_log (
    id bigint NOT NULL,
    proposta_movimento_id bigint,
    titulo_id bigint,
    usuario_legado_id integer,
    data_movimento timestamp with time zone,
    data_pagamento date,
    valor_pagamento numeric(18,2),
    data_alteracao timestamp with time zone,
    legado_movimento_proposta_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.movimento_cobranca_log OWNER TO postgres;

--
-- TOC entry 419 (class 1259 OID 8862700)
-- Name: movimento_cobranca_log_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.movimento_cobranca_log_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.movimento_cobranca_log_id_seq OWNER TO postgres;

--
-- TOC entry 6798 (class 0 OID 0)
-- Dependencies: 419
-- Name: movimento_cobranca_log_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.movimento_cobranca_log_id_seq OWNED BY financeiro.movimento_cobranca_log.id;


--
-- TOC entry 264 (class 1259 OID 8860516)
-- Name: regra_agrupamento_fatura; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.regra_agrupamento_fatura (
    id smallint NOT NULL,
    codigo character varying(50) NOT NULL,
    nome character varying(100) NOT NULL,
    descricao text
);


ALTER TABLE financeiro.regra_agrupamento_fatura OWNER TO postgres;

--
-- TOC entry 263 (class 1259 OID 8860515)
-- Name: regra_agrupamento_fatura_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.regra_agrupamento_fatura_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.regra_agrupamento_fatura_id_seq OWNER TO postgres;

--
-- TOC entry 6799 (class 0 OID 0)
-- Dependencies: 263
-- Name: regra_agrupamento_fatura_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.regra_agrupamento_fatura_id_seq OWNED BY financeiro.regra_agrupamento_fatura.id;


--
-- TOC entry 343 (class 1259 OID 8861572)
-- Name: retorno_bancario_codigo; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.retorno_bancario_codigo (
    id bigint NOT NULL,
    codigo character varying(20),
    descricao character varying(200) NOT NULL,
    tipo character varying(40) DEFAULT 'indefinido'::character varying NOT NULL,
    gera_baixa boolean DEFAULT false NOT NULL,
    gera_rejeicao boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.retorno_bancario_codigo OWNER TO postgres;

--
-- TOC entry 342 (class 1259 OID 8861571)
-- Name: retorno_bancario_codigo_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.retorno_bancario_codigo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.retorno_bancario_codigo_id_seq OWNER TO postgres;

--
-- TOC entry 6800 (class 0 OID 0)
-- Dependencies: 342
-- Name: retorno_bancario_codigo_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.retorno_bancario_codigo_id_seq OWNED BY financeiro.retorno_bancario_codigo.id;


--
-- TOC entry 339 (class 1259 OID 8861483)
-- Name: titulo; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.titulo (
    id bigint NOT NULL,
    proposta_movimento_id bigint,
    proposta_id bigint,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    estipulante_id bigint,
    convenio_cobranca_id bigint,
    conta_cobranca_id bigint,
    status_id smallint NOT NULL,
    competencia_ano integer,
    competencia_mes integer,
    competencia_int integer,
    data_vencimento date,
    data_lancamento date,
    parcela integer,
    sequencia integer,
    premio_anterior numeric(18,2),
    premio_atual numeric(18,2),
    premio_liquido numeric(18,2),
    premio_diferenca numeric(18,2),
    premio_total numeric(18,2),
    premio_total_original numeric(18,2),
    premio_fatura numeric(18,2),
    iof numeric(18,2),
    valor_original numeric(18,2),
    valor_atual numeric(18,2),
    valor_pago numeric(18,2),
    data_pagamento date,
    data_vencimento_fatura date,
    data_recebimento_fatura date,
    id_fatura_cartao character varying(100),
    cobrar_na_fatura boolean,
    observacao text,
    legado_movimento_proposta_id integer,
    legado_proposta_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE financeiro.titulo OWNER TO postgres;

--
-- TOC entry 338 (class 1259 OID 8861482)
-- Name: titulo_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.titulo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.titulo_id_seq OWNER TO postgres;

--
-- TOC entry 6801 (class 0 OID 0)
-- Dependencies: 338
-- Name: titulo_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.titulo_id_seq OWNED BY financeiro.titulo.id;


--
-- TOC entry 341 (class 1259 OID 8861548)
-- Name: titulo_pagamento; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.titulo_pagamento (
    id bigint NOT NULL,
    titulo_id bigint NOT NULL,
    proposta_movimento_id bigint,
    data_pagamento date,
    valor_pago numeric(18,2) DEFAULT 0 NOT NULL,
    forma_pagamento character varying(50),
    origem character varying(50) DEFAULT 'legado'::character varying NOT NULL,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.titulo_pagamento OWNER TO postgres;

--
-- TOC entry 340 (class 1259 OID 8861547)
-- Name: titulo_pagamento_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.titulo_pagamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.titulo_pagamento_id_seq OWNER TO postgres;

--
-- TOC entry 6802 (class 0 OID 0)
-- Dependencies: 340
-- Name: titulo_pagamento_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.titulo_pagamento_id_seq OWNED BY financeiro.titulo_pagamento.id;


--
-- TOC entry 345 (class 1259 OID 8861588)
-- Name: titulo_retorno_bancario; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.titulo_retorno_bancario (
    id bigint NOT NULL,
    titulo_id bigint,
    proposta_movimento_id bigint,
    retorno_codigo_id bigint,
    codigo_original character varying(20),
    descricao_original character varying(200),
    data_retorno date,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE financeiro.titulo_retorno_bancario OWNER TO postgres;

--
-- TOC entry 344 (class 1259 OID 8861587)
-- Name: titulo_retorno_bancario_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.titulo_retorno_bancario_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.titulo_retorno_bancario_id_seq OWNER TO postgres;

--
-- TOC entry 6803 (class 0 OID 0)
-- Dependencies: 344
-- Name: titulo_retorno_bancario_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.titulo_retorno_bancario_id_seq OWNED BY financeiro.titulo_retorno_bancario.id;


--
-- TOC entry 337 (class 1259 OID 8861470)
-- Name: titulo_status; Type: TABLE; Schema: financeiro; Owner: postgres
--

CREATE TABLE financeiro.titulo_status (
    id smallint NOT NULL,
    codigo character varying(40) NOT NULL,
    nome character varying(100) NOT NULL,
    finalizador boolean DEFAULT false NOT NULL,
    permite_cobranca boolean DEFAULT true NOT NULL,
    inadimplente boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL
);


ALTER TABLE financeiro.titulo_status OWNER TO postgres;

--
-- TOC entry 336 (class 1259 OID 8861469)
-- Name: titulo_status_id_seq; Type: SEQUENCE; Schema: financeiro; Owner: postgres
--

CREATE SEQUENCE financeiro.titulo_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE financeiro.titulo_status_id_seq OWNER TO postgres;

--
-- TOC entry 6804 (class 0 OID 0)
-- Dependencies: 336
-- Name: titulo_status_id_seq; Type: SEQUENCE OWNED BY; Schema: financeiro; Owner: postgres
--

ALTER SEQUENCE financeiro.titulo_status_id_seq OWNED BY financeiro.titulo_status.id;


--
-- TOC entry 287 (class 1259 OID 8860781)
-- Name: referencia_externa; Type: TABLE; Schema: integracao; Owner: postgres
--

CREATE TABLE integracao.referencia_externa (
    id bigint NOT NULL,
    sistema character varying(50) NOT NULL,
    entidade_tipo character varying(50) NOT NULL,
    entidade_id bigint NOT NULL,
    chave_externa character varying(150) NOT NULL,
    dados jsonb,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE integracao.referencia_externa OWNER TO postgres;

--
-- TOC entry 286 (class 1259 OID 8860780)
-- Name: referencia_externa_id_seq; Type: SEQUENCE; Schema: integracao; Owner: postgres
--

CREATE SEQUENCE integracao.referencia_externa_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE integracao.referencia_externa_id_seq OWNER TO postgres;

--
-- TOC entry 6805 (class 0 OID 0)
-- Dependencies: 286
-- Name: referencia_externa_id_seq; Type: SEQUENCE OWNED BY; Schema: integracao; Owner: postgres
--

ALTER SEQUENCE integracao.referencia_externa_id_seq OWNED BY integracao.referencia_externa.id;


--
-- TOC entry 404 (class 1259 OID 8862503)
-- Name: agenciador_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.agenciador_migration_map (
    id bigint NOT NULL,
    legado_agenciador_id integer NOT NULL,
    agenciador_id bigint NOT NULL,
    pessoa_id bigint,
    nome_original character varying(150),
    cpf_original character varying(30),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    legado_coordenador_id integer,
    coordenador_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.agenciador_migration_map OWNER TO postgres;

--
-- TOC entry 403 (class 1259 OID 8862502)
-- Name: agenciador_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.agenciador_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.agenciador_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6806 (class 0 OID 0)
-- Dependencies: 403
-- Name: agenciador_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.agenciador_migration_map_id_seq OWNED BY legado.agenciador_migration_map.id;


--
-- TOC entry 412 (class 1259 OID 8862624)
-- Name: agenciamento_corretora_lancamento_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.agenciamento_corretora_lancamento_migration_map (
    id bigint NOT NULL,
    legado_agenciamento_id integer NOT NULL,
    agenciamento_corretora_lancamento_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    legado_corretora_id integer,
    corretora_id bigint,
    legado_movimento_id integer,
    movimento_tipo_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.agenciamento_corretora_lancamento_migration_map OWNER TO postgres;

--
-- TOC entry 411 (class 1259 OID 8862623)
-- Name: agenciamento_corretora_lancamento_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.agenciamento_corretora_lancamento_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.agenciamento_corretora_lancamento_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6807 (class 0 OID 0)
-- Dependencies: 411
-- Name: agenciamento_corretora_lancamento_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.agenciamento_corretora_lancamento_migration_map_id_seq OWNED BY legado.agenciamento_corretora_lancamento_migration_map.id;


--
-- TOC entry 271 (class 1259 OID 8860592)
-- Name: cliente_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.cliente_migration_map (
    id bigint NOT NULL,
    legado_cliente_id integer NOT NULL,
    pessoa_id bigint NOT NULL,
    cliente_id bigint NOT NULL,
    cliente_vinculo_id bigint,
    cpf_original character varying(30),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    nome_original character varying(150),
    matricula_original character varying(50),
    criterio_unificacao_pessoa character varying(80) NOT NULL,
    criterio_criacao_vinculo character varying(80) NOT NULL,
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.cliente_migration_map OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 8860591)
-- Name: cliente_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.cliente_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.cliente_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6808 (class 0 OID 0)
-- Dependencies: 270
-- Name: cliente_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.cliente_migration_map_id_seq OWNED BY legado.cliente_migration_map.id;


--
-- TOC entry 327 (class 1259 OID 8861301)
-- Name: cobertura_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.cobertura_migration_map (
    id bigint NOT NULL,
    legado_cobertura_id integer NOT NULL,
    cobertura_id bigint NOT NULL,
    nome_original character varying(150),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.cobertura_migration_map OWNER TO postgres;

--
-- TOC entry 326 (class 1259 OID 8861300)
-- Name: cobertura_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.cobertura_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.cobertura_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6809 (class 0 OID 0)
-- Dependencies: 326
-- Name: cobertura_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.cobertura_migration_map_id_seq OWNED BY legado.cobertura_migration_map.id;


--
-- TOC entry 406 (class 1259 OID 8862534)
-- Name: corretora_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.corretora_migration_map (
    id bigint NOT NULL,
    legado_corretora_id integer NOT NULL,
    corretora_id bigint NOT NULL,
    pessoa_id bigint,
    nome_original character varying(150),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.corretora_migration_map OWNER TO postgres;

--
-- TOC entry 405 (class 1259 OID 8862533)
-- Name: corretora_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.corretora_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.corretora_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6810 (class 0 OID 0)
-- Dependencies: 405
-- Name: corretora_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.corretora_migration_map_id_seq OWNED BY legado.corretora_migration_map.id;


--
-- TOC entry 380 (class 1259 OID 8862139)
-- Name: documento_anexo_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.documento_anexo_migration_map (
    id bigint NOT NULL,
    legado_documento_id integer NOT NULL,
    arquivo_id bigint NOT NULL,
    titulo_original character varying(300),
    tipo_anexo_original character varying(120),
    extensao_original character varying(20),
    arquivo_original character varying(255),
    pk_cliente integer,
    cliente_id bigint,
    pk_proposta integer,
    proposta_id bigint,
    pk_sinistro integer,
    sinistro_id bigint,
    pk_estipulante integer,
    estipulante_id bigint,
    pk_protocolo integer,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.documento_anexo_migration_map OWNER TO postgres;

--
-- TOC entry 379 (class 1259 OID 8862138)
-- Name: documento_anexo_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.documento_anexo_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.documento_anexo_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6811 (class 0 OID 0)
-- Dependencies: 379
-- Name: documento_anexo_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.documento_anexo_migration_map_id_seq OWNED BY legado.documento_anexo_migration_map.id;


--
-- TOC entry 289 (class 1259 OID 8860796)
-- Name: estipulante_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.estipulante_migration_map (
    id bigint NOT NULL,
    legado_estipulante_id integer NOT NULL,
    pessoa_id bigint,
    estipulante_id bigint NOT NULL,
    cnpj_original character varying(30),
    cnpj_limpo character varying(20),
    cnpj_valido boolean DEFAULT false NOT NULL,
    nome_original character varying(150),
    criterio_unificacao_pessoa character varying(80),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.estipulante_migration_map OWNER TO postgres;

--
-- TOC entry 288 (class 1259 OID 8860795)
-- Name: estipulante_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.estipulante_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.estipulante_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6812 (class 0 OID 0)
-- Dependencies: 288
-- Name: estipulante_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.estipulante_migration_map_id_seq OWNED BY legado.estipulante_migration_map.id;


--
-- TOC entry 349 (class 1259 OID 8861661)
-- Name: movimento_proposta_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.movimento_proposta_migration_map (
    id bigint NOT NULL,
    legado_movimento_proposta_id integer NOT NULL,
    proposta_movimento_id bigint NOT NULL,
    titulo_id bigint,
    titulo_pagamento_id bigint,
    titulo_retorno_bancario_id bigint,
    lancamento_comissao_id bigint,
    legado_proposta_id integer,
    proposta_id bigint,
    legado_cliente_id integer,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    pessoa_id bigint,
    legado_estipulante_id integer,
    estipulante_id bigint,
    legado_movimento_id integer,
    movimento_tipo_id bigint,
    classificacao character varying(40),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.movimento_proposta_migration_map OWNER TO postgres;

--
-- TOC entry 348 (class 1259 OID 8861660)
-- Name: movimento_proposta_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.movimento_proposta_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.movimento_proposta_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6813 (class 0 OID 0)
-- Dependencies: 348
-- Name: movimento_proposta_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.movimento_proposta_migration_map_id_seq OWNED BY legado.movimento_proposta_migration_map.id;


--
-- TOC entry 321 (class 1259 OID 8861256)
-- Name: plano_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.plano_migration_map (
    id bigint NOT NULL,
    legado_plano_id integer NOT NULL,
    plano_id bigint NOT NULL,
    nome_original character varying(150),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.plano_migration_map OWNER TO postgres;

--
-- TOC entry 320 (class 1259 OID 8861255)
-- Name: plano_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.plano_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.plano_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6814 (class 0 OID 0)
-- Dependencies: 320
-- Name: plano_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.plano_migration_map_id_seq OWNED BY legado.plano_migration_map.id;


--
-- TOC entry 325 (class 1259 OID 8861286)
-- Name: produto_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.produto_migration_map (
    id bigint NOT NULL,
    legado_produto_id integer NOT NULL,
    produto_id bigint NOT NULL,
    codigo_referencia_original character varying(80),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.produto_migration_map OWNER TO postgres;

--
-- TOC entry 324 (class 1259 OID 8861285)
-- Name: produto_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.produto_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.produto_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6815 (class 0 OID 0)
-- Dependencies: 324
-- Name: produto_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.produto_migration_map_id_seq OWNED BY legado.produto_migration_map.id;


--
-- TOC entry 353 (class 1259 OID 8861761)
-- Name: proposta_beneficiario_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.proposta_beneficiario_migration_map (
    id bigint NOT NULL,
    legado_beneficiario_id integer NOT NULL,
    proposta_beneficiario_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    pessoa_id bigint,
    nome_original character varying(150),
    cpf_original character varying(50),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    parentesco_original character varying(100),
    parentesco_normalizado character varying(60),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.proposta_beneficiario_migration_map OWNER TO postgres;

--
-- TOC entry 352 (class 1259 OID 8861760)
-- Name: proposta_beneficiario_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.proposta_beneficiario_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.proposta_beneficiario_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6816 (class 0 OID 0)
-- Dependencies: 352
-- Name: proposta_beneficiario_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.proposta_beneficiario_migration_map_id_seq OWNED BY legado.proposta_beneficiario_migration_map.id;


--
-- TOC entry 331 (class 1259 OID 8861361)
-- Name: proposta_cobertura_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.proposta_cobertura_migration_map (
    id bigint NOT NULL,
    legado_proposta_cobertura_id integer NOT NULL,
    proposta_cobertura_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    legado_proposta_tipo_id integer,
    proposta_item_id bigint,
    legado_cobertura_id integer,
    cobertura_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.proposta_cobertura_migration_map OWNER TO postgres;

--
-- TOC entry 330 (class 1259 OID 8861360)
-- Name: proposta_cobertura_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.proposta_cobertura_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.proposta_cobertura_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6817 (class 0 OID 0)
-- Dependencies: 330
-- Name: proposta_cobertura_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.proposta_cobertura_migration_map_id_seq OWNED BY legado.proposta_cobertura_migration_map.id;


--
-- TOC entry 329 (class 1259 OID 8861316)
-- Name: proposta_item_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.proposta_item_migration_map (
    id bigint NOT NULL,
    legado_proposta_tipo_id integer NOT NULL,
    proposta_item_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    legado_tipo_id integer,
    tipo_produto_id bigint,
    legado_produto_id integer,
    produto_id bigint,
    legado_plano_original character varying(100),
    plano_id bigint,
    legado_tabela_id integer,
    tabela_preco_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.proposta_item_migration_map OWNER TO postgres;

--
-- TOC entry 328 (class 1259 OID 8861315)
-- Name: proposta_item_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.proposta_item_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.proposta_item_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6818 (class 0 OID 0)
-- Dependencies: 328
-- Name: proposta_item_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.proposta_item_migration_map_id_seq OWNED BY legado.proposta_item_migration_map.id;


--
-- TOC entry 303 (class 1259 OID 8861048)
-- Name: proposta_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.proposta_migration_map (
    id bigint NOT NULL,
    legado_proposta_id integer NOT NULL,
    proposta_id bigint NOT NULL,
    legado_cliente_id integer,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    pessoa_id bigint,
    legado_estipulante_id integer,
    estipulante_id bigint,
    legado_subestipulante_id integer,
    subestipulante_id bigint,
    legado_status integer,
    status_id smallint,
    numero_original character varying(100),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.proposta_migration_map OWNER TO postgres;

--
-- TOC entry 302 (class 1259 OID 8861047)
-- Name: proposta_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.proposta_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.proposta_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6819 (class 0 OID 0)
-- Dependencies: 302
-- Name: proposta_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.proposta_migration_map_id_seq OWNED BY legado.proposta_migration_map.id;


--
-- TOC entry 408 (class 1259 OID 8862557)
-- Name: proposta_participante_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.proposta_participante_migration_map (
    id bigint NOT NULL,
    proposta_participante_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    participante_tipo character varying(40) NOT NULL,
    codigo_legado_participante integer,
    agenciador_id bigint,
    corretora_id bigint,
    campo_origem character varying(80),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.proposta_participante_migration_map OWNER TO postgres;

--
-- TOC entry 407 (class 1259 OID 8862556)
-- Name: proposta_participante_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.proposta_participante_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.proposta_participante_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6820 (class 0 OID 0)
-- Dependencies: 407
-- Name: proposta_participante_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.proposta_participante_migration_map_id_seq OWNED BY legado.proposta_participante_migration_map.id;


--
-- TOC entry 392 (class 1259 OID 8862331)
-- Name: protocolo_acompanhamento_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.protocolo_acompanhamento_migration_map (
    id bigint NOT NULL,
    legado_acompanhamento_id integer NOT NULL,
    protocolo_acompanhamento_id bigint NOT NULL,
    legado_protocolo_id integer,
    protocolo_lote_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.protocolo_acompanhamento_migration_map OWNER TO postgres;

--
-- TOC entry 391 (class 1259 OID 8862330)
-- Name: protocolo_acompanhamento_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.protocolo_acompanhamento_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.protocolo_acompanhamento_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6821 (class 0 OID 0)
-- Dependencies: 391
-- Name: protocolo_acompanhamento_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.protocolo_acompanhamento_migration_map_id_seq OWNED BY legado.protocolo_acompanhamento_migration_map.id;


--
-- TOC entry 390 (class 1259 OID 8862286)
-- Name: protocolo_item_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.protocolo_item_migration_map (
    id bigint NOT NULL,
    origem_legado character varying(80) NOT NULL,
    legado_cliente_protocolo_id integer NOT NULL,
    protocolo_item_id bigint NOT NULL,
    legado_protocolo_id integer,
    protocolo_lote_id bigint,
    legado_cliente_id integer,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    pessoa_id bigint,
    legado_estipulante_id integer,
    estipulante_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.protocolo_item_migration_map OWNER TO postgres;

--
-- TOC entry 389 (class 1259 OID 8862285)
-- Name: protocolo_item_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.protocolo_item_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.protocolo_item_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6822 (class 0 OID 0)
-- Dependencies: 389
-- Name: protocolo_item_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.protocolo_item_migration_map_id_seq OWNED BY legado.protocolo_item_migration_map.id;


--
-- TOC entry 388 (class 1259 OID 8862268)
-- Name: protocolo_lote_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.protocolo_lote_migration_map (
    id bigint NOT NULL,
    legado_protocolo_id integer NOT NULL,
    protocolo_lote_id bigint NOT NULL,
    numero_protocolo_original integer,
    data_protocolo_original timestamp with time zone,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.protocolo_lote_migration_map OWNER TO postgres;

--
-- TOC entry 387 (class 1259 OID 8862267)
-- Name: protocolo_lote_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.protocolo_lote_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.protocolo_lote_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6823 (class 0 OID 0)
-- Dependencies: 387
-- Name: protocolo_lote_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.protocolo_lote_migration_map_id_seq OWNED BY legado.protocolo_lote_migration_map.id;


--
-- TOC entry 366 (class 1259 OID 8861991)
-- Name: sinistro_acompanhamento_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.sinistro_acompanhamento_migration_map (
    id bigint NOT NULL,
    legado_acompanhamento_id integer NOT NULL,
    acompanhamento_id bigint NOT NULL,
    legado_sinistro_id integer,
    sinistro_id bigint,
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.sinistro_acompanhamento_migration_map OWNER TO postgres;

--
-- TOC entry 365 (class 1259 OID 8861990)
-- Name: sinistro_acompanhamento_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.sinistro_acompanhamento_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.sinistro_acompanhamento_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6824 (class 0 OID 0)
-- Dependencies: 365
-- Name: sinistro_acompanhamento_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.sinistro_acompanhamento_migration_map_id_seq OWNED BY legado.sinistro_acompanhamento_migration_map.id;


--
-- TOC entry 364 (class 1259 OID 8861946)
-- Name: sinistro_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.sinistro_migration_map (
    id bigint NOT NULL,
    legado_sinistro_id integer NOT NULL,
    sinistro_id bigint NOT NULL,
    legado_proposta_id integer,
    proposta_id bigint,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    legado_status integer,
    status_id smallint,
    numero_sinistro_original character varying(80),
    criterio_migracao character varying(100),
    observacao text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.sinistro_migration_map OWNER TO postgres;

--
-- TOC entry 363 (class 1259 OID 8861945)
-- Name: sinistro_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.sinistro_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.sinistro_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6825 (class 0 OID 0)
-- Dependencies: 363
-- Name: sinistro_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.sinistro_migration_map_id_seq OWNED BY legado.sinistro_migration_map.id;


--
-- TOC entry 323 (class 1259 OID 8861271)
-- Name: tabela_preco_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.tabela_preco_migration_map (
    id bigint NOT NULL,
    legado_tabela_id integer NOT NULL,
    tabela_preco_id bigint NOT NULL,
    nome_original character varying(150),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.tabela_preco_migration_map OWNER TO postgres;

--
-- TOC entry 322 (class 1259 OID 8861270)
-- Name: tabela_preco_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.tabela_preco_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.tabela_preco_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6826 (class 0 OID 0)
-- Dependencies: 322
-- Name: tabela_preco_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.tabela_preco_migration_map_id_seq OWNED BY legado.tabela_preco_migration_map.id;


--
-- TOC entry 319 (class 1259 OID 8861241)
-- Name: tipo_produto_migration_map; Type: TABLE; Schema: legado; Owner: postgres
--

CREATE TABLE legado.tipo_produto_migration_map (
    id bigint NOT NULL,
    legado_tipo_id integer NOT NULL,
    tipo_produto_id bigint NOT NULL,
    nome_original character varying(100),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE legado.tipo_produto_migration_map OWNER TO postgres;

--
-- TOC entry 318 (class 1259 OID 8861240)
-- Name: tipo_produto_migration_map_id_seq; Type: SEQUENCE; Schema: legado; Owner: postgres
--

CREATE SEQUENCE legado.tipo_produto_migration_map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE legado.tipo_produto_migration_map_id_seq OWNER TO postgres;

--
-- TOC entry 6827 (class 0 OID 0)
-- Dependencies: 318
-- Name: tipo_produto_migration_map_id_seq; Type: SEQUENCE OWNED BY; Schema: legado; Owner: postgres
--

ALTER SEQUENCE legado.tipo_produto_migration_map_id_seq OWNED BY legado.tipo_produto_migration_map.id;


--
-- TOC entry 313 (class 1259 OID 8861159)
-- Name: cobertura; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.cobertura (
    id bigint NOT NULL,
    nome character varying(150),
    nome_reduzido character varying(30),
    basica character varying(50),
    reajuste boolean,
    legado_id integer,
    legado_cobertura_ant integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.cobertura OWNER TO postgres;

--
-- TOC entry 312 (class 1259 OID 8861158)
-- Name: cobertura_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.cobertura_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.cobertura_id_seq OWNER TO postgres;

--
-- TOC entry 6828 (class 0 OID 0)
-- Dependencies: 312
-- Name: cobertura_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.cobertura_id_seq OWNED BY seguro.cobertura.id;


--
-- TOC entry 333 (class 1259 OID 8861395)
-- Name: movimento_tipo; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.movimento_tipo (
    id bigint NOT NULL,
    nome character varying(120) NOT NULL,
    gera_titulo boolean DEFAULT false NOT NULL,
    classificacao character varying(40) DEFAULT 'avaliar'::character varying NOT NULL,
    ativo boolean DEFAULT true NOT NULL,
    altera_proposta boolean DEFAULT false NOT NULL,
    financeiro boolean DEFAULT false NOT NULL,
    cancelamento boolean DEFAULT false NOT NULL,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.movimento_tipo OWNER TO postgres;

--
-- TOC entry 332 (class 1259 OID 8861394)
-- Name: movimento_tipo_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.movimento_tipo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.movimento_tipo_id_seq OWNER TO postgres;

--
-- TOC entry 6829 (class 0 OID 0)
-- Dependencies: 332
-- Name: movimento_tipo_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.movimento_tipo_id_seq OWNED BY seguro.movimento_tipo.id;


--
-- TOC entry 307 (class 1259 OID 8861111)
-- Name: plano; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.plano (
    id bigint NOT NULL,
    nome character varying(150),
    ramo character varying(80),
    paga boolean,
    reajuste boolean,
    legado_id integer,
    legado_plano_ant integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.plano OWNER TO postgres;

--
-- TOC entry 306 (class 1259 OID 8861110)
-- Name: plano_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.plano_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.plano_id_seq OWNER TO postgres;

--
-- TOC entry 6830 (class 0 OID 0)
-- Dependencies: 306
-- Name: plano_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.plano_id_seq OWNED BY seguro.plano.id;


--
-- TOC entry 311 (class 1259 OID 8861135)
-- Name: produto; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.produto (
    id bigint NOT NULL,
    tabela_preco_id bigint,
    plano_id bigint,
    nome character varying(150),
    codigo_referencia character varying(80),
    ramo character varying(80),
    gera_conjuge boolean,
    paga_comissao boolean,
    legado_id integer,
    legado_produto_ant integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.produto OWNER TO postgres;

--
-- TOC entry 310 (class 1259 OID 8861134)
-- Name: produto_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.produto_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.produto_id_seq OWNER TO postgres;

--
-- TOC entry 6831 (class 0 OID 0)
-- Dependencies: 310
-- Name: produto_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.produto_id_seq OWNED BY seguro.produto.id;


--
-- TOC entry 295 (class 1259 OID 8860887)
-- Name: proposta; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    pessoa_id bigint NOT NULL,
    cliente_id bigint NOT NULL,
    cliente_vinculo_id bigint NOT NULL,
    estipulante_id bigint NOT NULL,
    subestipulante_id bigint,
    seguradora_id bigint,
    corretora_id bigint,
    convenio_cobranca_id bigint,
    conta_cobranca_id bigint,
    status_id smallint NOT NULL,
    movimento_tipo_id bigint,
    numero character varying(100),
    data_inclusao date,
    data_movimento date,
    data_primeiro_vencimento date,
    data_proximo_vencimento date,
    banco_agencia character varying(30),
    banco_conta_corrente character varying(30),
    banco_data_debito date,
    banco_dia_debito character varying(10),
    premio_liquido numeric(18,2),
    iof_percentual numeric(18,4),
    iof_valor numeric(18,2),
    valor_parcela numeric(18,2),
    movimento_fatura_mes integer,
    movimento_fatura_ano integer,
    subgrupo_id bigint,
    lotacao_id bigint,
    data_ultimo_ajuste_indice date,
    comissao_estornada boolean,
    data_estorno_comissao date,
    protocolo_cliente_legado_id integer,
    protocolo_status integer,
    competencia_inclusao_int integer,
    situacao_proposta integer,
    data_alteracao_situacao timestamp with time zone,
    data_processamento_funpresp timestamp with time zone,
    possui_bonus_funpresp boolean,
    observacao text,
    legado_id integer NOT NULL,
    legado_proposta_ant integer,
    legado_movimento_ini character varying(50),
    legado_movimento_fim character varying(50),
    vigente boolean DEFAULT false NOT NULL,
    visivel_operacional boolean DEFAULT true NOT NULL,
    proposta_origem_id bigint,
    versao integer DEFAULT 1 NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE seguro.proposta OWNER TO postgres;

--
-- TOC entry 351 (class 1259 OID 8861733)
-- Name: proposta_beneficiario; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_beneficiario (
    id bigint NOT NULL,
    proposta_id bigint NOT NULL,
    pessoa_id bigint,
    nome character varying(150),
    nome_normalizado character varying(150),
    cpf_original character varying(50),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    parentesco_original character varying(100),
    parentesco_normalizado character varying(60),
    percentual_participacao numeric(10,4),
    ordem integer,
    observacao text,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE seguro.proposta_beneficiario OWNER TO postgres;

--
-- TOC entry 350 (class 1259 OID 8861732)
-- Name: proposta_beneficiario_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_beneficiario_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_beneficiario_id_seq OWNER TO postgres;

--
-- TOC entry 6832 (class 0 OID 0)
-- Dependencies: 350
-- Name: proposta_beneficiario_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_beneficiario_id_seq OWNED BY seguro.proposta_beneficiario.id;


--
-- TOC entry 317 (class 1259 OID 8861212)
-- Name: proposta_cobertura; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_cobertura (
    id bigint NOT NULL,
    proposta_id bigint NOT NULL,
    proposta_item_id bigint,
    cobertura_id bigint,
    premio_titular numeric(18,2),
    premio_conjuge numeric(18,2),
    basica boolean,
    cobertura_nome_legado character varying(150),
    legado_id integer NOT NULL,
    legado_proposta_cobertura_ant integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.proposta_cobertura OWNER TO postgres;

--
-- TOC entry 316 (class 1259 OID 8861211)
-- Name: proposta_cobertura_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_cobertura_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_cobertura_id_seq OWNER TO postgres;

--
-- TOC entry 6833 (class 0 OID 0)
-- Dependencies: 316
-- Name: proposta_cobertura_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_cobertura_id_seq OWNED BY seguro.proposta_cobertura.id;


--
-- TOC entry 297 (class 1259 OID 8860977)
-- Name: proposta_historico; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_historico (
    id bigint NOT NULL,
    proposta_anterior_id bigint NOT NULL,
    proposta_nova_id bigint NOT NULL,
    motivo character varying(150),
    observacao text,
    data_alteracao timestamp with time zone DEFAULT now() NOT NULL,
    legado_origem character varying(80),
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.proposta_historico OWNER TO postgres;

--
-- TOC entry 296 (class 1259 OID 8860976)
-- Name: proposta_historico_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_historico_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_historico_id_seq OWNER TO postgres;

--
-- TOC entry 6834 (class 0 OID 0)
-- Dependencies: 296
-- Name: proposta_historico_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_historico_id_seq OWNED BY seguro.proposta_historico.id;


--
-- TOC entry 294 (class 1259 OID 8860886)
-- Name: proposta_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_id_seq OWNER TO postgres;

--
-- TOC entry 6835 (class 0 OID 0)
-- Dependencies: 294
-- Name: proposta_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_id_seq OWNED BY seguro.proposta.id;


--
-- TOC entry 315 (class 1259 OID 8861171)
-- Name: proposta_item; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_item (
    id bigint NOT NULL,
    proposta_id bigint NOT NULL,
    tipo_produto_id bigint,
    tabela_preco_id bigint,
    produto_id bigint,
    plano_id bigint,
    plano_codigo_legado character varying(100),
    plano_nome_legado character varying(150),
    ramo character varying(100),
    valor numeric(18,2),
    paga_comissao boolean,
    codigo_legado integer,
    cd_mov_vid integer,
    ultima_faixa_etaria integer,
    legado_id integer NOT NULL,
    legado_proposta_tipo_ant integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.proposta_item OWNER TO postgres;

--
-- TOC entry 314 (class 1259 OID 8861170)
-- Name: proposta_item_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_item_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_item_id_seq OWNER TO postgres;

--
-- TOC entry 6836 (class 0 OID 0)
-- Dependencies: 314
-- Name: proposta_item_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_item_id_seq OWNED BY seguro.proposta_item.id;


--
-- TOC entry 335 (class 1259 OID 8861413)
-- Name: proposta_movimento; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_movimento (
    id bigint NOT NULL,
    proposta_id bigint,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    estipulante_id bigint,
    convenio_cobranca_id bigint,
    movimento_tipo_id bigint,
    classificacao character varying(40) DEFAULT 'avaliar'::character varying NOT NULL,
    data_vencimento date,
    data_lancamento date,
    data_pagamento date,
    dia integer,
    mes integer,
    ano integer,
    competencia_int integer,
    premio_anterior numeric(18,2),
    premio_atual numeric(18,2),
    premio_liquido numeric(18,2),
    premio_diferenca numeric(18,2),
    premio_total numeric(18,2),
    premio_total_original numeric(18,2),
    premio_fatura numeric(18,2),
    valor_pago numeric(18,2),
    iof numeric(18,2),
    comissao_base numeric(18,2),
    comissao_liquida numeric(18,2),
    comissao_bruta numeric(18,2),
    situacao_codigo integer,
    situacao_descricao character varying(200),
    gerado character(1),
    comissao_gerado character(1),
    titulo_gerado character(1),
    parcela integer,
    sequencia integer,
    data_vencimento_fatura date,
    data_recebimento_fatura date,
    id_fatura_cartao character varying(100),
    cobrar_na_fatura boolean,
    usuario_cobrador_legado_id integer,
    observacao text,
    legado_id integer NOT NULL,
    legado_mov_ant integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.proposta_movimento OWNER TO postgres;

--
-- TOC entry 334 (class 1259 OID 8861412)
-- Name: proposta_movimento_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.proposta_movimento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.proposta_movimento_id_seq OWNER TO postgres;

--
-- TOC entry 6837 (class 0 OID 0)
-- Dependencies: 334
-- Name: proposta_movimento_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.proposta_movimento_id_seq OWNED BY seguro.proposta_movimento.id;


--
-- TOC entry 269 (class 1259 OID 8860581)
-- Name: proposta_status; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.proposta_status (
    id smallint NOT NULL,
    codigo character varying(30) NOT NULL,
    nome character varying(80) NOT NULL,
    permite_movimentacao boolean DEFAULT true NOT NULL,
    visivel_operacional boolean DEFAULT true NOT NULL,
    finalizador boolean DEFAULT false NOT NULL
);


ALTER TABLE seguro.proposta_status OWNER TO postgres;

--
-- TOC entry 309 (class 1259 OID 8861124)
-- Name: tabela_preco; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.tabela_preco (
    id bigint NOT NULL,
    nome character varying(150),
    codigo character varying(80),
    legado_id integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.tabela_preco OWNER TO postgres;

--
-- TOC entry 308 (class 1259 OID 8861123)
-- Name: tabela_preco_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.tabela_preco_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.tabela_preco_id_seq OWNER TO postgres;

--
-- TOC entry 6838 (class 0 OID 0)
-- Dependencies: 308
-- Name: tabela_preco_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.tabela_preco_id_seq OWNED BY seguro.tabela_preco.id;


--
-- TOC entry 305 (class 1259 OID 8861099)
-- Name: tipo_produto; Type: TABLE; Schema: seguro; Owner: postgres
--

CREATE TABLE seguro.tipo_produto (
    id bigint NOT NULL,
    nome character varying(100) NOT NULL,
    legado_id integer,
    ativo boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE seguro.tipo_produto OWNER TO postgres;

--
-- TOC entry 304 (class 1259 OID 8861098)
-- Name: tipo_produto_id_seq; Type: SEQUENCE; Schema: seguro; Owner: postgres
--

CREATE SEQUENCE seguro.tipo_produto_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE seguro.tipo_produto_id_seq OWNER TO postgres;

--
-- TOC entry 6839 (class 0 OID 0)
-- Dependencies: 304
-- Name: tipo_produto_id_seq; Type: SEQUENCE OWNED BY; Schema: seguro; Owner: postgres
--

ALTER SEQUENCE seguro.tipo_produto_id_seq OWNED BY seguro.tipo_produto.id;


--
-- TOC entry 358 (class 1259 OID 8861860)
-- Name: acompanhamento; Type: TABLE; Schema: sinistro; Owner: postgres
--

CREATE TABLE sinistro.acompanhamento (
    id bigint NOT NULL,
    sinistro_id bigint,
    data_acompanhamento date,
    hora_original character varying(30),
    contato character varying(150),
    descricao text,
    usuario_legado_id integer,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE sinistro.acompanhamento OWNER TO postgres;

--
-- TOC entry 357 (class 1259 OID 8861859)
-- Name: acompanhamento_id_seq; Type: SEQUENCE; Schema: sinistro; Owner: postgres
--

CREATE SEQUENCE sinistro.acompanhamento_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE sinistro.acompanhamento_id_seq OWNER TO postgres;

--
-- TOC entry 6840 (class 0 OID 0)
-- Dependencies: 357
-- Name: acompanhamento_id_seq; Type: SEQUENCE OWNED BY; Schema: sinistro; Owner: postgres
--

ALTER SEQUENCE sinistro.acompanhamento_id_seq OWNED BY sinistro.acompanhamento.id;


--
-- TOC entry 356 (class 1259 OID 8861803)
-- Name: sinistro; Type: TABLE; Schema: sinistro; Owner: postgres
--

CREATE TABLE sinistro.sinistro (
    id bigint NOT NULL,
    public_id uuid DEFAULT gen_random_uuid() NOT NULL,
    proposta_id bigint NOT NULL,
    pessoa_id bigint,
    cliente_id bigint,
    cliente_vinculo_id bigint,
    estipulante_id bigint,
    seguradora_id bigint,
    status_id smallint,
    numero_sinistro character varying(80),
    situacao_original character varying(80),
    data_ocorrencia date,
    data_aviso date,
    data_envio_seguradora date,
    data_encerramento date,
    data_protocolo timestamp with time zone,
    data_carta timestamp with time zone,
    data_relacao_familia timestamp with time zone,
    data_regulacao timestamp with time zone,
    valor_avisado numeric(18,2),
    valor_importancia numeric(18,2),
    valor_auxilio_funeral numeric(18,2),
    valor_cesta_basica numeric(18,2),
    valor_indenizacao numeric(18,2),
    tipo_plano_legado_id integer,
    cpf_sinistrado_original character varying(30),
    cpf_sinistrado_limpo character varying(20),
    cpf_sinistrado_valido boolean DEFAULT false NOT NULL,
    causa text,
    observacao text,
    legado_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone
);


ALTER TABLE sinistro.sinistro OWNER TO postgres;

--
-- TOC entry 360 (class 1259 OID 8861878)
-- Name: sinistro_beneficiario; Type: TABLE; Schema: sinistro; Owner: postgres
--

CREATE TABLE sinistro.sinistro_beneficiario (
    id bigint NOT NULL,
    sinistro_id bigint NOT NULL,
    proposta_id bigint,
    proposta_beneficiario_id bigint,
    pessoa_id bigint,
    nome character varying(150),
    cpf_original character varying(50),
    cpf_limpo character varying(20),
    cpf_valido boolean DEFAULT false NOT NULL,
    parentesco_original character varying(100),
    percentual_participacao numeric(10,4),
    valor_pago numeric(18,2),
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE sinistro.sinistro_beneficiario OWNER TO postgres;

--
-- TOC entry 359 (class 1259 OID 8861877)
-- Name: sinistro_beneficiario_id_seq; Type: SEQUENCE; Schema: sinistro; Owner: postgres
--

CREATE SEQUENCE sinistro.sinistro_beneficiario_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE sinistro.sinistro_beneficiario_id_seq OWNER TO postgres;

--
-- TOC entry 6841 (class 0 OID 0)
-- Dependencies: 359
-- Name: sinistro_beneficiario_id_seq; Type: SEQUENCE OWNED BY; Schema: sinistro; Owner: postgres
--

ALTER SEQUENCE sinistro.sinistro_beneficiario_id_seq OWNED BY sinistro.sinistro_beneficiario.id;


--
-- TOC entry 362 (class 1259 OID 8861912)
-- Name: sinistro_cobertura; Type: TABLE; Schema: sinistro; Owner: postgres
--

CREATE TABLE sinistro.sinistro_cobertura (
    id bigint NOT NULL,
    sinistro_id bigint NOT NULL,
    proposta_id bigint,
    proposta_cobertura_id bigint,
    cobertura_id bigint,
    valor_estimado numeric(18,2),
    valor_pago numeric(18,2),
    observacao text,
    legado_id integer,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    cobertura_sinistro_legado_id integer,
    premio_titular numeric(18,2),
    premio_conjuge numeric(18,2)
);


ALTER TABLE sinistro.sinistro_cobertura OWNER TO postgres;

--
-- TOC entry 361 (class 1259 OID 8861911)
-- Name: sinistro_cobertura_id_seq; Type: SEQUENCE; Schema: sinistro; Owner: postgres
--

CREATE SEQUENCE sinistro.sinistro_cobertura_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE sinistro.sinistro_cobertura_id_seq OWNER TO postgres;

--
-- TOC entry 6842 (class 0 OID 0)
-- Dependencies: 361
-- Name: sinistro_cobertura_id_seq; Type: SEQUENCE OWNED BY; Schema: sinistro; Owner: postgres
--

ALTER SEQUENCE sinistro.sinistro_cobertura_id_seq OWNED BY sinistro.sinistro_cobertura.id;


--
-- TOC entry 355 (class 1259 OID 8861802)
-- Name: sinistro_id_seq; Type: SEQUENCE; Schema: sinistro; Owner: postgres
--

CREATE SEQUENCE sinistro.sinistro_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE sinistro.sinistro_id_seq OWNER TO postgres;

--
-- TOC entry 6843 (class 0 OID 0)
-- Dependencies: 355
-- Name: sinistro_id_seq; Type: SEQUENCE OWNED BY; Schema: sinistro; Owner: postgres
--

ALTER SEQUENCE sinistro.sinistro_id_seq OWNED BY sinistro.sinistro.id;


--
-- TOC entry 354 (class 1259 OID 8861793)
-- Name: sinistro_status; Type: TABLE; Schema: sinistro; Owner: postgres
--

CREATE TABLE sinistro.sinistro_status (
    id smallint NOT NULL,
    codigo character varying(40) NOT NULL,
    nome character varying(100) NOT NULL,
    finalizador boolean DEFAULT false NOT NULL,
    ativo boolean DEFAULT true NOT NULL
);


ALTER TABLE sinistro.sinistro_status OWNER TO postgres;

--
-- TOC entry 5519 (class 2604 OID 8862253)
-- Name: protocolo_acompanhamento id; Type: DEFAULT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_acompanhamento ALTER COLUMN id SET DEFAULT nextval('atendimento.protocolo_acompanhamento_id_seq'::regclass);


--
-- TOC entry 5515 (class 2604 OID 8862209)
-- Name: protocolo_item id; Type: DEFAULT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item ALTER COLUMN id SET DEFAULT nextval('atendimento.protocolo_item_id_seq'::regclass);


--
-- TOC entry 5510 (class 2604 OID 8862192)
-- Name: protocolo_lote id; Type: DEFAULT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_lote ALTER COLUMN id SET DEFAULT nextval('atendimento.protocolo_lote_id_seq'::regclass);


--
-- TOC entry 5527 (class 2604 OID 8862357)
-- Name: protocolo_relatorio_seguradora id; Type: DEFAULT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora ALTER COLUMN id SET DEFAULT nextval('atendimento.protocolo_relatorio_seguradora_id_seq'::regclass);


--
-- TOC entry 5529 (class 2604 OID 8862369)
-- Name: protocolo_relatorio_seguradora_item id; Type: DEFAULT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item ALTER COLUMN id SET DEFAULT nextval('atendimento.protocolo_relatorio_seguradora_item_id_seq'::regclass);


--
-- TOC entry 5531 (class 2604 OID 8862405)
-- Name: agenciador id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador ALTER COLUMN id SET DEFAULT nextval('cadastro.agenciador_id_seq'::regclass);


--
-- TOC entry 5277 (class 2604 OID 8860363)
-- Name: cliente id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente ALTER COLUMN id SET DEFAULT nextval('cadastro.cliente_id_seq'::regclass);


--
-- TOC entry 5282 (class 2604 OID 8860389)
-- Name: cliente_dependente id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_dependente ALTER COLUMN id SET DEFAULT nextval('cadastro.cliente_dependente_id_seq'::regclass);


--
-- TOC entry 5269 (class 2604 OID 8860316)
-- Name: cliente_status id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_status ALTER COLUMN id SET DEFAULT nextval('cadastro.cliente_status_id_seq'::regclass);


--
-- TOC entry 5284 (class 2604 OID 8860409)
-- Name: cliente_vinculo id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo ALTER COLUMN id SET DEFAULT nextval('cadastro.cliente_vinculo_id_seq'::regclass);


--
-- TOC entry 5345 (class 2604 OID 8860829)
-- Name: corretora id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.corretora ALTER COLUMN id SET DEFAULT nextval('cadastro.corretora_id_seq'::regclass);


--
-- TOC entry 5321 (class 2604 OID 8860669)
-- Name: estipulante id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante ALTER COLUMN id SET DEFAULT nextval('cadastro.estipulante_id_seq'::regclass);


--
-- TOC entry 5326 (class 2604 OID 8860707)
-- Name: estipulante_configuracao id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante_configuracao ALTER COLUMN id SET DEFAULT nextval('cadastro.estipulante_configuracao_id_seq'::regclass);


--
-- TOC entry 5271 (class 2604 OID 8860326)
-- Name: grupo id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.grupo ALTER COLUMN id SET DEFAULT nextval('cadastro.grupo_id_seq'::regclass);


--
-- TOC entry 5275 (class 2604 OID 8860349)
-- Name: lotacao id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.lotacao ALTER COLUMN id SET DEFAULT nextval('cadastro.lotacao_id_seq'::regclass);


--
-- TOC entry 5310 (class 2604 OID 8860628)
-- Name: seguradora id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.seguradora ALTER COLUMN id SET DEFAULT nextval('cadastro.seguradora_id_seq'::regclass);


--
-- TOC entry 5351 (class 2604 OID 8860854)
-- Name: subestipulante id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante ALTER COLUMN id SET DEFAULT nextval('cadastro.subestipulante_id_seq'::regclass);


--
-- TOC entry 5273 (class 2604 OID 8860335)
-- Name: subgrupo id; Type: DEFAULT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subgrupo ALTER COLUMN id SET DEFAULT nextval('cadastro.subgrupo_id_seq'::regclass);


--
-- TOC entry 5537 (class 2604 OID 8862452)
-- Name: agenciador_comissao_config id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciador_comissao_config ALTER COLUMN id SET DEFAULT nextval('comissao.agenciador_comissao_config_id_seq'::regclass);


--
-- TOC entry 5553 (class 2604 OID 8862595)
-- Name: agenciamento_corretora_lancamento id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciamento_corretora_lancamento ALTER COLUMN id SET DEFAULT nextval('comissao.agenciamento_corretora_lancamento_id_seq'::regclass);


--
-- TOC entry 5542 (class 2604 OID 8862470)
-- Name: corretora_agenciador id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.corretora_agenciador ALTER COLUMN id SET DEFAULT nextval('comissao.corretora_agenciador_id_seq'::regclass);


--
-- TOC entry 5334 (class 2604 OID 8860767)
-- Name: estipulante_comissao_config id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.estipulante_comissao_config ALTER COLUMN id SET DEFAULT nextval('comissao.estipulante_comissao_config_id_seq'::regclass);


--
-- TOC entry 5578 (class 2604 OID 8862822)
-- Name: fatura_comissao_resumo id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_comissao_resumo ALTER COLUMN id SET DEFAULT nextval('comissao.fatura_comissao_resumo_id_seq'::regclass);


--
-- TOC entry 5570 (class 2604 OID 8862749)
-- Name: fatura_integracao id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_integracao ALTER COLUMN id SET DEFAULT nextval('comissao.fatura_integracao_id_seq'::regclass);


--
-- TOC entry 5573 (class 2604 OID 8862782)
-- Name: fatura_vida_agenciamento id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_agenciamento ALTER COLUMN id SET DEFAULT nextval('comissao.fatura_vida_agenciamento_id_seq'::regclass);


--
-- TOC entry 5576 (class 2604 OID 8862800)
-- Name: fatura_vida_recebimento id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_recebimento ALTER COLUMN id SET DEFAULT nextval('comissao.fatura_vida_recebimento_id_seq'::regclass);


--
-- TOC entry 5449 (class 2604 OID 8861619)
-- Name: lancamento_comissao id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao ALTER COLUMN id SET DEFAULT nextval('comissao.lancamento_comissao_id_seq'::regclass);


--
-- TOC entry 5580 (class 2604 OID 8862838)
-- Name: lancamento_fatura_estipulante id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_fatura_estipulante ALTER COLUMN id SET DEFAULT nextval('comissao.lancamento_fatura_estipulante_id_seq'::regclass);


--
-- TOC entry 5369 (class 2604 OID 8861035)
-- Name: proposta_participante id; Type: DEFAULT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.proposta_participante ALTER COLUMN id SET DEFAULT nextval('comissao.proposta_participante_id_seq'::regclass);


--
-- TOC entry 5293 (class 2604 OID 8860494)
-- Name: corsan_cliente id; Type: DEFAULT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_cliente ALTER COLUMN id SET DEFAULT nextval('convenio.corsan_cliente_id_seq'::regclass);


--
-- TOC entry 5366 (class 2604 OID 8861004)
-- Name: corsan_proposta id; Type: DEFAULT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta ALTER COLUMN id SET DEFAULT nextval('convenio.corsan_proposta_id_seq'::regclass);


--
-- TOC entry 5290 (class 2604 OID 8860463)
-- Name: siape_cliente id; Type: DEFAULT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente ALTER COLUMN id SET DEFAULT nextval('convenio.siape_cliente_id_seq'::regclass);


--
-- TOC entry 5288 (class 2604 OID 8860454)
-- Name: siape_orgao id; Type: DEFAULT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_orgao ALTER COLUMN id SET DEFAULT nextval('convenio.siape_orgao_id_seq'::regclass);


--
-- TOC entry 5318 (class 2604 OID 8860658)
-- Name: siape_parametro id; Type: DEFAULT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_parametro ALTER COLUMN id SET DEFAULT nextval('convenio.siape_parametro_id_seq'::regclass);


--
-- TOC entry 5267 (class 2604 OID 8860304)
-- Name: banco id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.banco ALTER COLUMN id SET DEFAULT nextval('core.banco_id_seq'::regclass);


--
-- TOC entry 5260 (class 2604 OID 8860267)
-- Name: cidade id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.cidade ALTER COLUMN id SET DEFAULT nextval('core.cidade_id_seq'::regclass);


--
-- TOC entry 5259 (class 2604 OID 8860258)
-- Name: estado id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.estado ALTER COLUMN id SET DEFAULT nextval('core.estado_id_seq'::regclass);


--
-- TOC entry 5246 (class 2604 OID 8860208)
-- Name: pessoa id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa ALTER COLUMN id SET DEFAULT nextval('core.pessoa_id_seq'::regclass);


--
-- TOC entry 5255 (class 2604 OID 8860241)
-- Name: pessoa_contato id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_contato ALTER COLUMN id SET DEFAULT nextval('core.pessoa_contato_id_seq'::regclass);


--
-- TOC entry 5252 (class 2604 OID 8860225)
-- Name: pessoa_documento id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_documento ALTER COLUMN id SET DEFAULT nextval('core.pessoa_documento_id_seq'::regclass);


--
-- TOC entry 5262 (class 2604 OID 8860282)
-- Name: pessoa_endereco id; Type: DEFAULT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_endereco ALTER COLUMN id SET DEFAULT nextval('core.pessoa_endereco_id_seq'::regclass);


--
-- TOC entry 5490 (class 2604 OID 8862049)
-- Name: arquivo id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo ALTER COLUMN id SET DEFAULT nextval('documento.arquivo_id_seq'::regclass);


--
-- TOC entry 5506 (class 2604 OID 8862125)
-- Name: arquivo_acesso_log id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_acesso_log ALTER COLUMN id SET DEFAULT nextval('documento.arquivo_acesso_log_id_seq'::regclass);


--
-- TOC entry 5503 (class 2604 OID 8862101)
-- Name: arquivo_versao id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_versao ALTER COLUMN id SET DEFAULT nextval('documento.arquivo_versao_id_seq'::regclass);


--
-- TOC entry 5498 (class 2604 OID 8862075)
-- Name: arquivo_vinculo id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_vinculo ALTER COLUMN id SET DEFAULT nextval('documento.arquivo_vinculo_id_seq'::regclass);


--
-- TOC entry 5481 (class 2604 OID 8862020)
-- Name: storage_provider id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.storage_provider ALTER COLUMN id SET DEFAULT nextval('documento.storage_provider_id_seq'::regclass);


--
-- TOC entry 5483 (class 2604 OID 8862032)
-- Name: tipo_anexo id; Type: DEFAULT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.tipo_anexo ALTER COLUMN id SET DEFAULT nextval('documento.tipo_anexo_id_seq'::regclass);


--
-- TOC entry 5568 (class 2604 OID 8862726)
-- Name: cobranca_acompanhamento id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.cobranca_acompanhamento ALTER COLUMN id SET DEFAULT nextval('financeiro.cobranca_acompanhamento_id_seq'::regclass);


--
-- TOC entry 5300 (class 2604 OID 8860547)
-- Name: conta_cobranca id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca ALTER COLUMN id SET DEFAULT nextval('financeiro.conta_cobranca_id_seq'::regclass);


--
-- TOC entry 5297 (class 2604 OID 8860530)
-- Name: convenio_cobranca id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.convenio_cobranca ALTER COLUMN id SET DEFAULT nextval('financeiro.convenio_cobranca_id_seq'::regclass);


--
-- TOC entry 5331 (class 2604 OID 8860730)
-- Name: estipulante_faturamento_config id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config ALTER COLUMN id SET DEFAULT nextval('financeiro.estipulante_faturamento_config_id_seq'::regclass);


--
-- TOC entry 5315 (class 2604 OID 8860648)
-- Name: forma_pagamento_estipulante id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_pagamento_estipulante ALTER COLUMN id SET DEFAULT nextval('financeiro.forma_pagamento_estipulante_id_seq'::regclass);


--
-- TOC entry 5558 (class 2604 OID 8862662)
-- Name: forma_retorno id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno ALTER COLUMN id SET DEFAULT nextval('financeiro.forma_retorno_id_seq'::regclass);


--
-- TOC entry 5562 (class 2604 OID 8862673)
-- Name: forma_retorno_estipulante id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno_estipulante ALTER COLUMN id SET DEFAULT nextval('financeiro.forma_retorno_estipulante_id_seq'::regclass);


--
-- TOC entry 5564 (class 2604 OID 8862694)
-- Name: identificador_remessa_api id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.identificador_remessa_api ALTER COLUMN id SET DEFAULT nextval('financeiro.identificador_remessa_api_id_seq'::regclass);


--
-- TOC entry 5566 (class 2604 OID 8862704)
-- Name: movimento_cobranca_log id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.movimento_cobranca_log ALTER COLUMN id SET DEFAULT nextval('financeiro.movimento_cobranca_log_id_seq'::regclass);


--
-- TOC entry 5296 (class 2604 OID 8860519)
-- Name: regra_agrupamento_fatura id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.regra_agrupamento_fatura ALTER COLUMN id SET DEFAULT nextval('financeiro.regra_agrupamento_fatura_id_seq'::regclass);


--
-- TOC entry 5441 (class 2604 OID 8861575)
-- Name: retorno_bancario_codigo id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.retorno_bancario_codigo ALTER COLUMN id SET DEFAULT nextval('financeiro.retorno_bancario_codigo_id_seq'::regclass);


--
-- TOC entry 5434 (class 2604 OID 8861486)
-- Name: titulo id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo ALTER COLUMN id SET DEFAULT nextval('financeiro.titulo_id_seq'::regclass);


--
-- TOC entry 5437 (class 2604 OID 8861551)
-- Name: titulo_pagamento id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_pagamento ALTER COLUMN id SET DEFAULT nextval('financeiro.titulo_pagamento_id_seq'::regclass);


--
-- TOC entry 5447 (class 2604 OID 8861591)
-- Name: titulo_retorno_bancario id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_retorno_bancario ALTER COLUMN id SET DEFAULT nextval('financeiro.titulo_retorno_bancario_id_seq'::regclass);


--
-- TOC entry 5429 (class 2604 OID 8861473)
-- Name: titulo_status id; Type: DEFAULT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_status ALTER COLUMN id SET DEFAULT nextval('financeiro.titulo_status_id_seq'::regclass);


--
-- TOC entry 5338 (class 2604 OID 8860784)
-- Name: referencia_externa id; Type: DEFAULT; Schema: integracao; Owner: postgres
--

ALTER TABLE ONLY integracao.referencia_externa ALTER COLUMN id SET DEFAULT nextval('integracao.referencia_externa_id_seq'::regclass);


--
-- TOC entry 5546 (class 2604 OID 8862506)
-- Name: agenciador_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.agenciador_migration_map_id_seq'::regclass);


--
-- TOC entry 5556 (class 2604 OID 8862627)
-- Name: agenciamento_corretora_lancamento_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.agenciamento_corretora_lancamento_migration_map_id_seq'::regclass);


--
-- TOC entry 5307 (class 2604 OID 8860595)
-- Name: cliente_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.cliente_migration_map_id_seq'::regclass);


--
-- TOC entry 5410 (class 2604 OID 8861304)
-- Name: cobertura_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cobertura_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.cobertura_migration_map_id_seq'::regclass);


--
-- TOC entry 5549 (class 2604 OID 8862537)
-- Name: corretora_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.corretora_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.corretora_migration_map_id_seq'::regclass);


--
-- TOC entry 5508 (class 2604 OID 8862142)
-- Name: documento_anexo_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.documento_anexo_migration_map_id_seq'::regclass);


--
-- TOC entry 5342 (class 2604 OID 8860799)
-- Name: estipulante_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.estipulante_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.estipulante_migration_map_id_seq'::regclass);


--
-- TOC entry 5454 (class 2604 OID 8861664)
-- Name: movimento_proposta_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.movimento_proposta_migration_map_id_seq'::regclass);


--
-- TOC entry 5404 (class 2604 OID 8861259)
-- Name: plano_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.plano_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.plano_migration_map_id_seq'::regclass);


--
-- TOC entry 5408 (class 2604 OID 8861289)
-- Name: produto_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.produto_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.produto_migration_map_id_seq'::regclass);


--
-- TOC entry 5460 (class 2604 OID 8861764)
-- Name: proposta_beneficiario_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.proposta_beneficiario_migration_map_id_seq'::regclass);


--
-- TOC entry 5414 (class 2604 OID 8861364)
-- Name: proposta_cobertura_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.proposta_cobertura_migration_map_id_seq'::regclass);


--
-- TOC entry 5412 (class 2604 OID 8861319)
-- Name: proposta_item_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.proposta_item_migration_map_id_seq'::regclass);


--
-- TOC entry 5372 (class 2604 OID 8861051)
-- Name: proposta_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.proposta_migration_map_id_seq'::regclass);


--
-- TOC entry 5551 (class 2604 OID 8862560)
-- Name: proposta_participante_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.proposta_participante_migration_map_id_seq'::regclass);


--
-- TOC entry 5525 (class 2604 OID 8862334)
-- Name: protocolo_acompanhamento_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_acompanhamento_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.protocolo_acompanhamento_migration_map_id_seq'::regclass);


--
-- TOC entry 5523 (class 2604 OID 8862289)
-- Name: protocolo_item_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.protocolo_item_migration_map_id_seq'::regclass);


--
-- TOC entry 5521 (class 2604 OID 8862271)
-- Name: protocolo_lote_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_lote_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.protocolo_lote_migration_map_id_seq'::regclass);


--
-- TOC entry 5479 (class 2604 OID 8861994)
-- Name: sinistro_acompanhamento_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_acompanhamento_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.sinistro_acompanhamento_migration_map_id_seq'::regclass);


--
-- TOC entry 5477 (class 2604 OID 8861949)
-- Name: sinistro_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.sinistro_migration_map_id_seq'::regclass);


--
-- TOC entry 5406 (class 2604 OID 8861274)
-- Name: tabela_preco_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tabela_preco_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.tabela_preco_migration_map_id_seq'::regclass);


--
-- TOC entry 5402 (class 2604 OID 8861244)
-- Name: tipo_produto_migration_map id; Type: DEFAULT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tipo_produto_migration_map ALTER COLUMN id SET DEFAULT nextval('legado.tipo_produto_migration_map_id_seq'::regclass);


--
-- TOC entry 5390 (class 2604 OID 8861162)
-- Name: cobertura id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.cobertura ALTER COLUMN id SET DEFAULT nextval('seguro.cobertura_id_seq'::regclass);


--
-- TOC entry 5416 (class 2604 OID 8861398)
-- Name: movimento_tipo id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.movimento_tipo ALTER COLUMN id SET DEFAULT nextval('seguro.movimento_tipo_id_seq'::regclass);


--
-- TOC entry 5378 (class 2604 OID 8861114)
-- Name: plano id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.plano ALTER COLUMN id SET DEFAULT nextval('seguro.plano_id_seq'::regclass);


--
-- TOC entry 5386 (class 2604 OID 8861138)
-- Name: produto id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.produto ALTER COLUMN id SET DEFAULT nextval('seguro.produto_id_seq'::regclass);


--
-- TOC entry 5356 (class 2604 OID 8860890)
-- Name: proposta id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_id_seq'::regclass);


--
-- TOC entry 5456 (class 2604 OID 8861736)
-- Name: proposta_beneficiario id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_beneficiario ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_beneficiario_id_seq'::regclass);


--
-- TOC entry 5398 (class 2604 OID 8861215)
-- Name: proposta_cobertura id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_cobertura ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_cobertura_id_seq'::regclass);


--
-- TOC entry 5363 (class 2604 OID 8860980)
-- Name: proposta_historico id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_historico ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_historico_id_seq'::regclass);


--
-- TOC entry 5394 (class 2604 OID 8861174)
-- Name: proposta_item id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_item_id_seq'::regclass);


--
-- TOC entry 5425 (class 2604 OID 8861416)
-- Name: proposta_movimento id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento ALTER COLUMN id SET DEFAULT nextval('seguro.proposta_movimento_id_seq'::regclass);


--
-- TOC entry 5382 (class 2604 OID 8861127)
-- Name: tabela_preco id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.tabela_preco ALTER COLUMN id SET DEFAULT nextval('seguro.tabela_preco_id_seq'::regclass);


--
-- TOC entry 5374 (class 2604 OID 8861102)
-- Name: tipo_produto id; Type: DEFAULT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.tipo_produto ALTER COLUMN id SET DEFAULT nextval('seguro.tipo_produto_id_seq'::regclass);


--
-- TOC entry 5470 (class 2604 OID 8861863)
-- Name: acompanhamento id; Type: DEFAULT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.acompanhamento ALTER COLUMN id SET DEFAULT nextval('sinistro.acompanhamento_id_seq'::regclass);


--
-- TOC entry 5465 (class 2604 OID 8861806)
-- Name: sinistro id; Type: DEFAULT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro ALTER COLUMN id SET DEFAULT nextval('sinistro.sinistro_id_seq'::regclass);


--
-- TOC entry 5472 (class 2604 OID 8861881)
-- Name: sinistro_beneficiario id; Type: DEFAULT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario ALTER COLUMN id SET DEFAULT nextval('sinistro.sinistro_beneficiario_id_seq'::regclass);


--
-- TOC entry 5475 (class 2604 OID 8861915)
-- Name: sinistro_cobertura id; Type: DEFAULT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura ALTER COLUMN id SET DEFAULT nextval('sinistro.sinistro_cobertura_id_seq'::regclass);


--
-- TOC entry 6688 (class 0 OID 8862250)
-- Dependencies: 386
-- Data for Name: protocolo_acompanhamento; Type: TABLE DATA; Schema: atendimento; Owner: postgres
--

COPY atendimento.protocolo_acompanhamento (id, protocolo_lote_id, data_acompanhamento, hora_original, contato, descricao, usuario_legado_id, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6686 (class 0 OID 8862206)
-- Dependencies: 384
-- Data for Name: protocolo_item; Type: TABLE DATA; Schema: atendimento; Owner: postgres
--

COPY atendimento.protocolo_item (id, protocolo_lote_id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, premio, data_vigencia, equipe, matricula, tipo_item, nome_conjuge, origem_legado, legado_id, legado_cliente_id, legado_estipulante_id, observacao, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6684 (class 0 OID 8862189)
-- Dependencies: 382
-- Data for Name: protocolo_lote; Type: TABLE DATA; Schema: atendimento; Owner: postgres
--

COPY atendimento.protocolo_lote (id, public_id, numero_protocolo, data_protocolo, consultor_legado_id, usuario_legado_id, anexo_consultor, anexo_seguradora, status, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6696 (class 0 OID 8862354)
-- Dependencies: 394
-- Data for Name: protocolo_relatorio_seguradora; Type: TABLE DATA; Schema: atendimento; Owner: postgres
--

COPY atendimento.protocolo_relatorio_seguradora (id, data_relatorio, observacao, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6698 (class 0 OID 8862366)
-- Dependencies: 396
-- Data for Name: protocolo_relatorio_seguradora_item; Type: TABLE DATA; Schema: atendimento; Owner: postgres
--

COPY atendimento.protocolo_relatorio_seguradora_item (id, relatorio_id, protocolo_lote_id, pessoa_id, cliente_id, cliente_vinculo_id, legado_cliente_id, legado_protocolo_id, created_at) FROM stdin;
\.


--
-- TOC entry 6700 (class 0 OID 8862402)
-- Dependencies: 398
-- Data for Name: agenciador; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.agenciador (id, public_id, pessoa_id, cidade_id, banco_id, coordenador_id, nome, codigo, tipo, cpf, cpf_limpo, cpf_valido, rg, orgao_rg, data_emissao_rg, susep, inss, issqn, telefone, email, cep, logradouro, numero, complemento, bairro, numero_dependentes, data_inscricao, data_nascimento, credenciado, desativado, data_desativado, agencia, conta_corrente, observacao, legado_id, legado_ant_ven, legado_ant_ger, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6554 (class 0 OID 8860360)
-- Dependencies: 252
-- Data for Name: cliente; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.cliente (id, public_id, pessoa_id, status_id, falecido, data_obito, observacao, data_cadastro_legado, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6556 (class 0 OID 8860386)
-- Dependencies: 254
-- Data for Name: cliente_dependente; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.cliente_dependente (id, cliente_id, pessoa_id, tipo_relacao, nome, cpf, cpf_limpo, rg, orgao_rg, data_emissao_rg, data_nascimento, legado_origem, created_at) FROM stdin;
\.


--
-- TOC entry 6546 (class 0 OID 8860313)
-- Dependencies: 244
-- Data for Name: cliente_status; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.cliente_status (id, codigo, nome, ativo) FROM stdin;
1	ativo	Ativo	t
2	inativo	Inativo	t
\.


--
-- TOC entry 6558 (class 0 OID 8860406)
-- Dependencies: 256
-- Data for Name: cliente_vinculo; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.cliente_vinculo (id, cliente_id, pessoa_id, estipulante_id, subestipulante_id, grupo_id, subgrupo_id, lotacao_id, matricula, matricula_normalizada, banco_id, agencia, conta_corrente, legado_cliente_id, criterio_criacao, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6593 (class 0 OID 8860826)
-- Dependencies: 291
-- Data for Name: corretora; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.corretora (id, public_id, pessoa_id, nome, codigo, cidade_id, cep, logradouro, numero, complemento, bairro, telefone, codigo_protheus, ativo, observacao, legado_id, created_at, updated_at, deleted_at, caminho_logotipo_legado, logotipo_arquivo_id, possui_logotipo_legado) FROM stdin;
\.


--
-- TOC entry 6581 (class 0 OID 8860666)
-- Dependencies: 279
-- Data for Name: estipulante; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.estipulante (id, public_id, pessoa_id, nome, nome_formatado, codigo, tipo_pessoa, cnpj, cnpj_limpo, cidade_id, grupo_id, seguradora_id, ativo, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6583 (class 0 OID 8860704)
-- Dependencies: 281
-- Data for Name: estipulante_configuracao; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.estipulante_configuracao (id, estipulante_id, tabela_legado_id, permite_propostas, controla_comissao, data_inicio_vigencia, data_fim_vigencia, data_aniversario, data_ultimo_reajuste, data_base_reajuste, data_limite_reajuste, dias_aviso_reajuste, carencia, adesao_por, custeio, adesao, faixa_etaria_inicio, faixa_etaria_fim, cancela_estipulante_id, indice_legado_id, percentual_indice, ajuste_indice, ajuste_fator, reajuste, tipo_cobertura_conjuge, percentual_tipo_cobertura_conjuge, possui_excedente, data_limite_excedente, dias_aviso_excedente, prazo_regulacao, dia_corte, desconsiderar_proposta_ativa, permitir_protocolo_duplicado, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6548 (class 0 OID 8860323)
-- Dependencies: 246
-- Data for Name: grupo; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.grupo (id, nome, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6552 (class 0 OID 8860346)
-- Dependencies: 250
-- Data for Name: lotacao; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.lotacao (id, cidade_id, nome, codigo, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6575 (class 0 OID 8860625)
-- Dependencies: 273
-- Data for Name: seguradora; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.seguradora (id, public_id, pessoa_id, nome, codigo, susep, cnpj, cnpj_limpo, ativo, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6595 (class 0 OID 8860851)
-- Dependencies: 293
-- Data for Name: subestipulante; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.subestipulante (id, public_id, pessoa_id, estipulante_id, nome, codigo, cidade_id, banco_id, cnpj, cnpj_limpo, ativo, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6550 (class 0 OID 8860332)
-- Dependencies: 248
-- Data for Name: subgrupo; Type: TABLE DATA; Schema: cadastro; Owner: postgres
--

COPY cadastro.subgrupo (id, grupo_id, nome, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6702 (class 0 OID 8862449)
-- Dependencies: 400
-- Data for Name: agenciador_comissao_config; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.agenciador_comissao_config (id, agenciador_id, percentual_padrao, percentual_repasse, inicio_vigencia, fim_vigencia, ativo, origem, legado_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6712 (class 0 OID 8862592)
-- Dependencies: 410
-- Data for Name: agenciamento_corretora_lancamento; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.agenciamento_corretora_lancamento (id, proposta_id, corretora_id, movimento_tipo_id, percentual, valor_premio, valor_agenciamento, parcela_inicial, parcela_final, status_legado, valor_pago, data_pagamento, gerou_fatura, data_cadastro, data_vencimento, legado_id, legado_proposta_id, legado_corretora_id, legado_movimento_id, observacao, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6704 (class 0 OID 8862467)
-- Dependencies: 402
-- Data for Name: corretora_agenciador; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.corretora_agenciador (id, corretora_id, agenciador_id, percentual_agenciamento, percentual_repasse, inicio_vigencia, fim_vigencia, ativo, legado_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6587 (class 0 OID 8860764)
-- Dependencies: 285
-- Data for Name: estipulante_comissao_config; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.estipulante_comissao_config (id, estipulante_id, percentual_comissao, percentual_agenciamento, percentual_bonus, comissao_apartir_parcela, agenciador_id, agenciador_percentual_repasse, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6732 (class 0 OID 8862819)
-- Dependencies: 430
-- Data for Name: fatura_comissao_resumo; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.fatura_comissao_resumo (id, estipulante_id, mes, ano, competencia_int, premio_pagamento, valor_pago, data_pagamento, legado_id, legado_estipulante_id, created_at) FROM stdin;
\.


--
-- TOC entry 6726 (class 0 OID 8862746)
-- Dependencies: 424
-- Data for Name: fatura_integracao; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.fatura_integracao (id, corretora_id, seguradora_id, estipulante_id, corretora_codigo_original, seguradora_codigo_original, data_lancamento, data_vencimento, data_recebimento, valor_receber, valor_recebido, valor_fatura, situacao_legado, tipo, mes, ano, competencia_int, gerou_arquivo, alterado, percentual_agenciamento, percentual_corretagem, legado_id, observacao, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6728 (class 0 OID 8862779)
-- Dependencies: 426
-- Data for Name: fatura_vida_agenciamento; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.fatura_vida_agenciamento (id, origem_legado, proposta_id, premio, iof, premio_liquido, valor_agenciamento, valor_recebido, valor_diferenca, codigo_cooperado_original, codigo_corretora_original, tipo_agenciamento, numero_nf, data_inclusao, data_registro, legado_id, legado_proposta_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6730 (class 0 OID 8862797)
-- Dependencies: 428
-- Data for Name: fatura_vida_recebimento; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.fatura_vida_recebimento (id, fatura_vida_agenciamento_id, estipulante_id, data_pagamento, valor, observacao, legado_id, legado_fatura_vida_id, legado_estipulante_id, created_at) FROM stdin;
\.


--
-- TOC entry 6649 (class 0 OID 8861616)
-- Dependencies: 347
-- Data for Name: lancamento_comissao; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.lancamento_comissao (id, proposta_movimento_id, titulo_id, proposta_id, pessoa_id, cliente_id, estipulante_id, competencia_ano, competencia_mes, competencia_int, valor_base, valor_bruto, valor_liquido, gerado, status, origem, legado_movimento_proposta_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6734 (class 0 OID 8862835)
-- Dependencies: 432
-- Data for Name: lancamento_fatura_estipulante; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.lancamento_fatura_estipulante (id, estipulante_id, corretora_id, competencia_original, competencia_mes, competencia_ano, competencia_int, premio_total, valor_faturado, percentual_corretagem, comissao_recebida, data_vencimento_fatura, data_recebimento, lancamento_manual, legado_id, legado_estipulante_id, legado_corretora_id, created_at) FROM stdin;
\.


--
-- TOC entry 6603 (class 0 OID 8861032)
-- Dependencies: 301
-- Data for Name: proposta_participante; Type: TABLE DATA; Schema: comissao; Owner: postgres
--

COPY comissao.proposta_participante (id, proposta_id, participante_tipo, participante_id, codigo_agenciamento, percentual_agenciamento, agenciamento_parcela_inicial, agenciamento_parcela_final, bonus, percentual_carteira, carteira_parcela_inicial, ativo, legado_campo_origem, created_at, agenciador_id, corretora_id, codigo_legado_participante) FROM stdin;
\.


--
-- TOC entry 6564 (class 0 OID 8860491)
-- Dependencies: 262
-- Data for Name: corsan_cliente; Type: TABLE DATA; Schema: convenio; Owner: postgres
--

COPY convenio.corsan_cliente (id, cliente_id, cliente_vinculo_id, pessoa_id, empresa, rubrica, grupo, funcionario, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6601 (class 0 OID 8861001)
-- Dependencies: 299
-- Data for Name: corsan_proposta; Type: TABLE DATA; Schema: convenio; Owner: postgres
--

COPY convenio.corsan_proposta (id, proposta_id, cliente_id, cliente_vinculo_id, pessoa_id, empresa, rubrica, grupo, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6562 (class 0 OID 8860460)
-- Dependencies: 260
-- Data for Name: siape_cliente; Type: TABLE DATA; Schema: convenio; Owner: postgres
--

COPY convenio.siape_cliente (id, cliente_id, cliente_vinculo_id, pessoa_id, siape, orgao_id, categoria, setor, instituto, agencia, funcao, contrato, digito_verificador, instituidor, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6560 (class 0 OID 8860451)
-- Dependencies: 258
-- Data for Name: siape_orgao; Type: TABLE DATA; Schema: convenio; Owner: postgres
--

COPY convenio.siape_orgao (id, codigo, nome, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6579 (class 0 OID 8860655)
-- Dependencies: 277
-- Data for Name: siape_parametro; Type: TABLE DATA; Schema: convenio; Owner: postgres
--

COPY convenio.siape_parametro (id, empresa, cgc, cgc_limpo, rubrica, comando, custo_linha, calculo_parametro, legado_id, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6544 (class 0 OID 8860301)
-- Dependencies: 242
-- Data for Name: banco; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.banco (id, codigo, nome, observacao, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6540 (class 0 OID 8860264)
-- Dependencies: 238
-- Data for Name: cidade; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.cidade (id, estado_id, nome, nome_normalizado, uf, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6538 (class 0 OID 8860255)
-- Dependencies: 236
-- Data for Name: estado; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.estado (id, uf, nome) FROM stdin;
\.


--
-- TOC entry 6532 (class 0 OID 8860205)
-- Dependencies: 230
-- Data for Name: pessoa; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.pessoa (id, public_id, tipo_pessoa, nome, nome_normalizado, documento_principal, documento_principal_limpo, documento_valido, data_nascimento, sexo, observacao, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6536 (class 0 OID 8860238)
-- Dependencies: 234
-- Data for Name: pessoa_contato; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.pessoa_contato (id, pessoa_id, tipo_contato, valor, valor_normalizado, principal, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6534 (class 0 OID 8860222)
-- Dependencies: 232
-- Data for Name: pessoa_documento; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.pessoa_documento (id, pessoa_id, tipo_documento, numero, numero_limpo, orgao_emissor, data_emissao, principal, created_at) FROM stdin;
\.


--
-- TOC entry 6542 (class 0 OID 8860279)
-- Dependencies: 240
-- Data for Name: pessoa_endereco; Type: TABLE DATA; Schema: core; Owner: postgres
--

COPY core.pessoa_endereco (id, pessoa_id, cidade_id, tipo_endereco, cep, logradouro, numero, complemento, bairro, uf, principal, ativo, legado_situacao_endereco, created_at) FROM stdin;
\.


--
-- TOC entry 6674 (class 0 OID 8862046)
-- Dependencies: 372
-- Data for Name: arquivo; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.arquivo (id, public_id, storage_provider_id, bucket, storage_key, storage_path, nome_original, nome_armazenado, titulo, descricao, extensao, mime_type, tamanho_bytes, hash_sha256, data_documento, data_upload, hora_original, origem, caminho_legado, arquivo_legado, status, criado_por_usuario_id, criado_por_usuario_legado_id, legado_id, created_at, updated_at, deleted_at, extensao_original, extensao_normalizada, extensao_confiavel, migracao_status, migracao_erro) FROM stdin;
\.


--
-- TOC entry 6680 (class 0 OID 8862122)
-- Dependencies: 378
-- Data for Name: arquivo_acesso_log; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.arquivo_acesso_log (id, arquivo_id, usuario_id, usuario_legado_id, acao, ip_origem, user_agent, created_at) FROM stdin;
\.


--
-- TOC entry 6678 (class 0 OID 8862098)
-- Dependencies: 376
-- Data for Name: arquivo_versao; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.arquivo_versao (id, arquivo_id, versao, storage_provider_id, bucket, storage_key, storage_path, nome_original, extensao, mime_type, tamanho_bytes, hash_sha256, motivo, criado_por_usuario_id, criado_por_usuario_legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6676 (class 0 OID 8862072)
-- Dependencies: 374
-- Data for Name: arquivo_vinculo; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.arquivo_vinculo (id, arquivo_id, tipo_anexo_id, entidade_tipo, entidade_id, entidade_legado_id, principal, obrigatorio, observacao, legado_origem_coluna, created_at, deleted_at, entidade_legado_tipo, entidade_legado_chave_1, entidade_legado_chave_2, criterio_resolucao, vinculo_resolvido, entidade_legado_chave_concatenada) FROM stdin;
\.


--
-- TOC entry 6670 (class 0 OID 8862017)
-- Dependencies: 368
-- Data for Name: storage_provider; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.storage_provider (id, codigo, nome, descricao, ativo) FROM stdin;
1	local	Storage local	Arquivos armazenados em disco/local path controlado pela aplicação.	t
2	s3	Amazon S3	Arquivos armazenados em bucket S3.	t
3	minio	MinIO	Arquivos armazenados em storage compatível com S3.	t
4	azure_blob	Azure Blob Storage	Arquivos armazenados em container Azure Blob.	t
5	supabase_storage	Supabase Storage	Arquivos armazenados no Supabase Storage.	t
\.


--
-- TOC entry 6672 (class 0 OID 8862029)
-- Dependencies: 370
-- Data for Name: tipo_anexo; Type: TABLE DATA; Schema: documento; Owner: postgres
--

COPY documento.tipo_anexo (id, codigo, nome, categoria, descricao, exige_validade, exige_assinatura, sensivel, ativo, legado_valor_original, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6724 (class 0 OID 8862723)
-- Dependencies: 422
-- Data for Name: cobranca_acompanhamento; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.cobranca_acompanhamento (id, pessoa_id, cliente_id, data_acompanhamento, hora_original, contato, descricao, usuario_legado_id, legado_id, legado_cliente_id, created_at) FROM stdin;
\.


--
-- TOC entry 6570 (class 0 OID 8860544)
-- Dependencies: 268
-- Data for Name: conta_cobranca; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.conta_cobranca (id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, subestipulante_id, convenio_cobranca_id, regra_agrupamento_id, identificador_agrupamento, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6568 (class 0 OID 8860527)
-- Dependencies: 266
-- Data for Name: convenio_cobranca; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.convenio_cobranca (id, banco_id, nome, agencia, conta_corrente, nome_empresa, codigo_empresa, numero_arquivo, nome_inicial_arquivo, extensao_arquivo, layout_arquivo, local_remessa_arquivo, local_retorno_arquivo, comunica_vindi, observacao, legado_id, created_at, updated_at, inscricao_estadual, est_endereco, est_numero, est_bairro, est_complemento, est_cep, est_cidade, est_uf, est_nome) FROM stdin;
\.


--
-- TOC entry 6585 (class 0 OID 8860727)
-- Dependencies: 283
-- Data for Name: estipulante_faturamento_config; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.estipulante_faturamento_config (id, estipulante_id, forma_pagamento_id, convenio_cobranca_id, regra_agrupamento_fatura_id, dia_debito, iof_vg, iof_inc, iof_ap, numero_proposta_vg, numero_proposta_inc, numero_proposta_ap, sorteio_valor, saf, campanha, parametro_siape_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6577 (class 0 OID 8860645)
-- Dependencies: 275
-- Data for Name: forma_pagamento_estipulante; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.forma_pagamento_estipulante (id, nome, codigo, legado_id, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6716 (class 0 OID 8862659)
-- Dependencies: 414
-- Data for Name: forma_retorno; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.forma_retorno (id, nome, legado_id, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6718 (class 0 OID 8862670)
-- Dependencies: 416
-- Data for Name: forma_retorno_estipulante; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.forma_retorno_estipulante (id, forma_retorno_id, estipulante_id, legado_id, legado_forma_retorno_id, legado_estipulante_id, created_at) FROM stdin;
\.


--
-- TOC entry 6720 (class 0 OID 8862691)
-- Dependencies: 418
-- Data for Name: identificador_remessa_api; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.identificador_remessa_api (id, usuario_legado_id, datahora, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6722 (class 0 OID 8862701)
-- Dependencies: 420
-- Data for Name: movimento_cobranca_log; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.movimento_cobranca_log (id, proposta_movimento_id, titulo_id, usuario_legado_id, data_movimento, data_pagamento, valor_pagamento, data_alteracao, legado_movimento_proposta_id, created_at) FROM stdin;
\.


--
-- TOC entry 6566 (class 0 OID 8860516)
-- Dependencies: 264
-- Data for Name: regra_agrupamento_fatura; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.regra_agrupamento_fatura (id, codigo, nome, descricao) FROM stdin;
1	por_pessoa	Unificar por pessoa	Agrupa cobrança/fatura por pessoa física/documento.
2	por_cliente_vinculo	Separar por vínculo do cliente	Agrupa por vínculo operacional, normalmente pessoa + estipulante + matrícula.
3	por_estipulante	Separar por estipulante	Agrupa por estipulante.
4	por_subestipulante	Separar por subestipulante	Agrupa por subestipulante.
5	por_convenio_cobranca	Separar por convênio de cobrança	Agrupa por convênio de cobrança.
6	por_proposta	Separar por proposta	Cada proposta gera cobrança/fatura separada.
7	customizado	Customizado	Regra específica tratada por configuração ou rotina própria.
\.


--
-- TOC entry 6645 (class 0 OID 8861572)
-- Dependencies: 343
-- Data for Name: retorno_bancario_codigo; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.retorno_bancario_codigo (id, codigo, descricao, tipo, gera_baixa, gera_rejeicao, ativo, created_at) FROM stdin;
\.


--
-- TOC entry 6641 (class 0 OID 8861483)
-- Dependencies: 339
-- Data for Name: titulo; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.titulo (id, proposta_movimento_id, proposta_id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, convenio_cobranca_id, conta_cobranca_id, status_id, competencia_ano, competencia_mes, competencia_int, data_vencimento, data_lancamento, parcela, sequencia, premio_anterior, premio_atual, premio_liquido, premio_diferenca, premio_total, premio_total_original, premio_fatura, iof, valor_original, valor_atual, valor_pago, data_pagamento, data_vencimento_fatura, data_recebimento_fatura, id_fatura_cartao, cobrar_na_fatura, observacao, legado_movimento_proposta_id, legado_proposta_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6643 (class 0 OID 8861548)
-- Dependencies: 341
-- Data for Name: titulo_pagamento; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.titulo_pagamento (id, titulo_id, proposta_movimento_id, data_pagamento, valor_pago, forma_pagamento, origem, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6647 (class 0 OID 8861588)
-- Dependencies: 345
-- Data for Name: titulo_retorno_bancario; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.titulo_retorno_bancario (id, titulo_id, proposta_movimento_id, retorno_codigo_id, codigo_original, descricao_original, data_retorno, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6639 (class 0 OID 8861470)
-- Dependencies: 337
-- Data for Name: titulo_status; Type: TABLE DATA; Schema: financeiro; Owner: postgres
--

COPY financeiro.titulo_status (id, codigo, nome, finalizador, permite_cobranca, inadimplente, ativo) FROM stdin;
1	aberto	Aberto	f	t	f	t
2	pago	Pago	t	f	f	t
3	nao_descontou	Não descontou	f	t	t	t
4	recuperado	Recuperado	t	f	f	t
5	cancelado	Cancelado	t	f	f	t
6	acerto	Acerto	f	t	f	t
7	indefinido_legado	Indefinido no legado	f	t	f	t
\.


--
-- TOC entry 6589 (class 0 OID 8860781)
-- Dependencies: 287
-- Data for Name: referencia_externa; Type: TABLE DATA; Schema: integracao; Owner: postgres
--

COPY integracao.referencia_externa (id, sistema, entidade_tipo, entidade_id, chave_externa, dados, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6706 (class 0 OID 8862503)
-- Dependencies: 404
-- Data for Name: agenciador_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.agenciador_migration_map (id, legado_agenciador_id, agenciador_id, pessoa_id, nome_original, cpf_original, cpf_limpo, cpf_valido, legado_coordenador_id, coordenador_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6714 (class 0 OID 8862624)
-- Dependencies: 412
-- Data for Name: agenciamento_corretora_lancamento_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.agenciamento_corretora_lancamento_migration_map (id, legado_agenciamento_id, agenciamento_corretora_lancamento_id, legado_proposta_id, proposta_id, legado_corretora_id, corretora_id, legado_movimento_id, movimento_tipo_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6573 (class 0 OID 8860592)
-- Dependencies: 271
-- Data for Name: cliente_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.cliente_migration_map (id, legado_cliente_id, pessoa_id, cliente_id, cliente_vinculo_id, cpf_original, cpf_limpo, cpf_valido, nome_original, matricula_original, criterio_unificacao_pessoa, criterio_criacao_vinculo, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6629 (class 0 OID 8861301)
-- Dependencies: 327
-- Data for Name: cobertura_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.cobertura_migration_map (id, legado_cobertura_id, cobertura_id, nome_original, created_at) FROM stdin;
\.


--
-- TOC entry 6708 (class 0 OID 8862534)
-- Dependencies: 406
-- Data for Name: corretora_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.corretora_migration_map (id, legado_corretora_id, corretora_id, pessoa_id, nome_original, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6682 (class 0 OID 8862139)
-- Dependencies: 380
-- Data for Name: documento_anexo_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.documento_anexo_migration_map (id, legado_documento_id, arquivo_id, titulo_original, tipo_anexo_original, extensao_original, arquivo_original, pk_cliente, cliente_id, pk_proposta, proposta_id, pk_sinistro, sinistro_id, pk_estipulante, estipulante_id, pk_protocolo, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6591 (class 0 OID 8860796)
-- Dependencies: 289
-- Data for Name: estipulante_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.estipulante_migration_map (id, legado_estipulante_id, pessoa_id, estipulante_id, cnpj_original, cnpj_limpo, cnpj_valido, nome_original, criterio_unificacao_pessoa, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6651 (class 0 OID 8861661)
-- Dependencies: 349
-- Data for Name: movimento_proposta_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.movimento_proposta_migration_map (id, legado_movimento_proposta_id, proposta_movimento_id, titulo_id, titulo_pagamento_id, titulo_retorno_bancario_id, lancamento_comissao_id, legado_proposta_id, proposta_id, legado_cliente_id, cliente_id, cliente_vinculo_id, pessoa_id, legado_estipulante_id, estipulante_id, legado_movimento_id, movimento_tipo_id, classificacao, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6623 (class 0 OID 8861256)
-- Dependencies: 321
-- Data for Name: plano_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.plano_migration_map (id, legado_plano_id, plano_id, nome_original, created_at) FROM stdin;
\.


--
-- TOC entry 6627 (class 0 OID 8861286)
-- Dependencies: 325
-- Data for Name: produto_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.produto_migration_map (id, legado_produto_id, produto_id, codigo_referencia_original, created_at) FROM stdin;
\.


--
-- TOC entry 6655 (class 0 OID 8861761)
-- Dependencies: 353
-- Data for Name: proposta_beneficiario_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.proposta_beneficiario_migration_map (id, legado_beneficiario_id, proposta_beneficiario_id, legado_proposta_id, proposta_id, pessoa_id, nome_original, cpf_original, cpf_limpo, cpf_valido, parentesco_original, parentesco_normalizado, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6633 (class 0 OID 8861361)
-- Dependencies: 331
-- Data for Name: proposta_cobertura_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.proposta_cobertura_migration_map (id, legado_proposta_cobertura_id, proposta_cobertura_id, legado_proposta_id, proposta_id, legado_proposta_tipo_id, proposta_item_id, legado_cobertura_id, cobertura_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6631 (class 0 OID 8861316)
-- Dependencies: 329
-- Data for Name: proposta_item_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.proposta_item_migration_map (id, legado_proposta_tipo_id, proposta_item_id, legado_proposta_id, proposta_id, legado_tipo_id, tipo_produto_id, legado_produto_id, produto_id, legado_plano_original, plano_id, legado_tabela_id, tabela_preco_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6605 (class 0 OID 8861048)
-- Dependencies: 303
-- Data for Name: proposta_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.proposta_migration_map (id, legado_proposta_id, proposta_id, legado_cliente_id, cliente_id, cliente_vinculo_id, pessoa_id, legado_estipulante_id, estipulante_id, legado_subestipulante_id, subestipulante_id, legado_status, status_id, numero_original, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6710 (class 0 OID 8862557)
-- Dependencies: 408
-- Data for Name: proposta_participante_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.proposta_participante_migration_map (id, proposta_participante_id, legado_proposta_id, proposta_id, participante_tipo, codigo_legado_participante, agenciador_id, corretora_id, campo_origem, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6694 (class 0 OID 8862331)
-- Dependencies: 392
-- Data for Name: protocolo_acompanhamento_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.protocolo_acompanhamento_migration_map (id, legado_acompanhamento_id, protocolo_acompanhamento_id, legado_protocolo_id, protocolo_lote_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6692 (class 0 OID 8862286)
-- Dependencies: 390
-- Data for Name: protocolo_item_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.protocolo_item_migration_map (id, origem_legado, legado_cliente_protocolo_id, protocolo_item_id, legado_protocolo_id, protocolo_lote_id, legado_cliente_id, cliente_id, cliente_vinculo_id, pessoa_id, legado_estipulante_id, estipulante_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6690 (class 0 OID 8862268)
-- Dependencies: 388
-- Data for Name: protocolo_lote_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.protocolo_lote_migration_map (id, legado_protocolo_id, protocolo_lote_id, numero_protocolo_original, data_protocolo_original, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6668 (class 0 OID 8861991)
-- Dependencies: 366
-- Data for Name: sinistro_acompanhamento_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.sinistro_acompanhamento_migration_map (id, legado_acompanhamento_id, acompanhamento_id, legado_sinistro_id, sinistro_id, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6666 (class 0 OID 8861946)
-- Dependencies: 364
-- Data for Name: sinistro_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.sinistro_migration_map (id, legado_sinistro_id, sinistro_id, legado_proposta_id, proposta_id, pessoa_id, cliente_id, cliente_vinculo_id, legado_status, status_id, numero_sinistro_original, criterio_migracao, observacao, created_at) FROM stdin;
\.


--
-- TOC entry 6625 (class 0 OID 8861271)
-- Dependencies: 323
-- Data for Name: tabela_preco_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.tabela_preco_migration_map (id, legado_tabela_id, tabela_preco_id, nome_original, created_at) FROM stdin;
\.


--
-- TOC entry 6621 (class 0 OID 8861241)
-- Dependencies: 319
-- Data for Name: tipo_produto_migration_map; Type: TABLE DATA; Schema: legado; Owner: postgres
--

COPY legado.tipo_produto_migration_map (id, legado_tipo_id, tipo_produto_id, nome_original, created_at) FROM stdin;
\.


--
-- TOC entry 6615 (class 0 OID 8861159)
-- Dependencies: 313
-- Data for Name: cobertura; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.cobertura (id, nome, nome_reduzido, basica, reajuste, legado_id, legado_cobertura_ant, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6635 (class 0 OID 8861395)
-- Dependencies: 333
-- Data for Name: movimento_tipo; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.movimento_tipo (id, nome, gera_titulo, classificacao, ativo, altera_proposta, financeiro, cancelamento, legado_id, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6609 (class 0 OID 8861111)
-- Dependencies: 307
-- Data for Name: plano; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.plano (id, nome, ramo, paga, reajuste, legado_id, legado_plano_ant, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6613 (class 0 OID 8861135)
-- Dependencies: 311
-- Data for Name: produto; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.produto (id, tabela_preco_id, plano_id, nome, codigo_referencia, ramo, gera_conjuge, paga_comissao, legado_id, legado_produto_ant, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6597 (class 0 OID 8860887)
-- Dependencies: 295
-- Data for Name: proposta; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta (id, public_id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, subestipulante_id, seguradora_id, corretora_id, convenio_cobranca_id, conta_cobranca_id, status_id, movimento_tipo_id, numero, data_inclusao, data_movimento, data_primeiro_vencimento, data_proximo_vencimento, banco_agencia, banco_conta_corrente, banco_data_debito, banco_dia_debito, premio_liquido, iof_percentual, iof_valor, valor_parcela, movimento_fatura_mes, movimento_fatura_ano, subgrupo_id, lotacao_id, data_ultimo_ajuste_indice, comissao_estornada, data_estorno_comissao, protocolo_cliente_legado_id, protocolo_status, competencia_inclusao_int, situacao_proposta, data_alteracao_situacao, data_processamento_funpresp, possui_bonus_funpresp, observacao, legado_id, legado_proposta_ant, legado_movimento_ini, legado_movimento_fim, vigente, visivel_operacional, proposta_origem_id, versao, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6653 (class 0 OID 8861733)
-- Dependencies: 351
-- Data for Name: proposta_beneficiario; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_beneficiario (id, proposta_id, pessoa_id, nome, nome_normalizado, cpf_original, cpf_limpo, cpf_valido, parentesco_original, parentesco_normalizado, percentual_participacao, ordem, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6619 (class 0 OID 8861212)
-- Dependencies: 317
-- Data for Name: proposta_cobertura; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_cobertura (id, proposta_id, proposta_item_id, cobertura_id, premio_titular, premio_conjuge, basica, cobertura_nome_legado, legado_id, legado_proposta_cobertura_ant, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6599 (class 0 OID 8860977)
-- Dependencies: 297
-- Data for Name: proposta_historico; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_historico (id, proposta_anterior_id, proposta_nova_id, motivo, observacao, data_alteracao, legado_origem, created_at) FROM stdin;
\.


--
-- TOC entry 6617 (class 0 OID 8861171)
-- Dependencies: 315
-- Data for Name: proposta_item; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_item (id, proposta_id, tipo_produto_id, tabela_preco_id, produto_id, plano_id, plano_codigo_legado, plano_nome_legado, ramo, valor, paga_comissao, codigo_legado, cd_mov_vid, ultima_faixa_etaria, legado_id, legado_proposta_tipo_ant, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6637 (class 0 OID 8861413)
-- Dependencies: 335
-- Data for Name: proposta_movimento; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_movimento (id, proposta_id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, convenio_cobranca_id, movimento_tipo_id, classificacao, data_vencimento, data_lancamento, data_pagamento, dia, mes, ano, competencia_int, premio_anterior, premio_atual, premio_liquido, premio_diferenca, premio_total, premio_total_original, premio_fatura, valor_pago, iof, comissao_base, comissao_liquida, comissao_bruta, situacao_codigo, situacao_descricao, gerado, comissao_gerado, titulo_gerado, parcela, sequencia, data_vencimento_fatura, data_recebimento_fatura, id_fatura_cartao, cobrar_na_fatura, usuario_cobrador_legado_id, observacao, legado_id, legado_mov_ant, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6571 (class 0 OID 8860581)
-- Dependencies: 269
-- Data for Name: proposta_status; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.proposta_status (id, codigo, nome, permite_movimentacao, visivel_operacional, finalizador) FROM stdin;
1	ativa	Ativa	t	t	f
2	cancelada	Cancelada	f	t	t
5	oculta_historico	Oculta / Histórico	f	f	t
\.


--
-- TOC entry 6611 (class 0 OID 8861124)
-- Dependencies: 309
-- Data for Name: tabela_preco; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.tabela_preco (id, nome, codigo, legado_id, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6607 (class 0 OID 8861099)
-- Dependencies: 305
-- Data for Name: tipo_produto; Type: TABLE DATA; Schema: seguro; Owner: postgres
--

COPY seguro.tipo_produto (id, nome, legado_id, ativo, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 6660 (class 0 OID 8861860)
-- Dependencies: 358
-- Data for Name: acompanhamento; Type: TABLE DATA; Schema: sinistro; Owner: postgres
--

COPY sinistro.acompanhamento (id, sinistro_id, data_acompanhamento, hora_original, contato, descricao, usuario_legado_id, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6658 (class 0 OID 8861803)
-- Dependencies: 356
-- Data for Name: sinistro; Type: TABLE DATA; Schema: sinistro; Owner: postgres
--

COPY sinistro.sinistro (id, public_id, proposta_id, pessoa_id, cliente_id, cliente_vinculo_id, estipulante_id, seguradora_id, status_id, numero_sinistro, situacao_original, data_ocorrencia, data_aviso, data_envio_seguradora, data_encerramento, data_protocolo, data_carta, data_relacao_familia, data_regulacao, valor_avisado, valor_importancia, valor_auxilio_funeral, valor_cesta_basica, valor_indenizacao, tipo_plano_legado_id, cpf_sinistrado_original, cpf_sinistrado_limpo, cpf_sinistrado_valido, causa, observacao, legado_id, created_at, updated_at, deleted_at) FROM stdin;
\.


--
-- TOC entry 6662 (class 0 OID 8861878)
-- Dependencies: 360
-- Data for Name: sinistro_beneficiario; Type: TABLE DATA; Schema: sinistro; Owner: postgres
--

COPY sinistro.sinistro_beneficiario (id, sinistro_id, proposta_id, proposta_beneficiario_id, pessoa_id, nome, cpf_original, cpf_limpo, cpf_valido, parentesco_original, percentual_participacao, valor_pago, observacao, legado_id, created_at) FROM stdin;
\.


--
-- TOC entry 6664 (class 0 OID 8861912)
-- Dependencies: 362
-- Data for Name: sinistro_cobertura; Type: TABLE DATA; Schema: sinistro; Owner: postgres
--

COPY sinistro.sinistro_cobertura (id, sinistro_id, proposta_id, proposta_cobertura_id, cobertura_id, valor_estimado, valor_pago, observacao, legado_id, created_at, cobertura_sinistro_legado_id, premio_titular, premio_conjuge) FROM stdin;
\.


--
-- TOC entry 6656 (class 0 OID 8861793)
-- Dependencies: 354
-- Data for Name: sinistro_status; Type: TABLE DATA; Schema: sinistro; Owner: postgres
--

COPY sinistro.sinistro_status (id, codigo, nome, finalizador, ativo) FROM stdin;
1	status_1	Status 1 - Legado	f	t
2	status_2	Status 2 - Legado	f	t
3	status_3	Status 3 - Legado	f	t
4	status_4	Status 4 - Legado	f	t
5	status_5	Status 5 - Legado	f	t
\.


--
-- TOC entry 6844 (class 0 OID 0)
-- Dependencies: 385
-- Name: protocolo_acompanhamento_id_seq; Type: SEQUENCE SET; Schema: atendimento; Owner: postgres
--

SELECT pg_catalog.setval('atendimento.protocolo_acompanhamento_id_seq', 1, false);


--
-- TOC entry 6845 (class 0 OID 0)
-- Dependencies: 383
-- Name: protocolo_item_id_seq; Type: SEQUENCE SET; Schema: atendimento; Owner: postgres
--

SELECT pg_catalog.setval('atendimento.protocolo_item_id_seq', 1, false);


--
-- TOC entry 6846 (class 0 OID 0)
-- Dependencies: 381
-- Name: protocolo_lote_id_seq; Type: SEQUENCE SET; Schema: atendimento; Owner: postgres
--

SELECT pg_catalog.setval('atendimento.protocolo_lote_id_seq', 1, false);


--
-- TOC entry 6847 (class 0 OID 0)
-- Dependencies: 393
-- Name: protocolo_relatorio_seguradora_id_seq; Type: SEQUENCE SET; Schema: atendimento; Owner: postgres
--

SELECT pg_catalog.setval('atendimento.protocolo_relatorio_seguradora_id_seq', 1, false);


--
-- TOC entry 6848 (class 0 OID 0)
-- Dependencies: 395
-- Name: protocolo_relatorio_seguradora_item_id_seq; Type: SEQUENCE SET; Schema: atendimento; Owner: postgres
--

SELECT pg_catalog.setval('atendimento.protocolo_relatorio_seguradora_item_id_seq', 1, false);


--
-- TOC entry 6849 (class 0 OID 0)
-- Dependencies: 397
-- Name: agenciador_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.agenciador_id_seq', 1, false);


--
-- TOC entry 6850 (class 0 OID 0)
-- Dependencies: 253
-- Name: cliente_dependente_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.cliente_dependente_id_seq', 1, false);


--
-- TOC entry 6851 (class 0 OID 0)
-- Dependencies: 251
-- Name: cliente_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.cliente_id_seq', 1, false);


--
-- TOC entry 6852 (class 0 OID 0)
-- Dependencies: 243
-- Name: cliente_status_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.cliente_status_id_seq', 2, true);


--
-- TOC entry 6853 (class 0 OID 0)
-- Dependencies: 255
-- Name: cliente_vinculo_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.cliente_vinculo_id_seq', 1, false);


--
-- TOC entry 6854 (class 0 OID 0)
-- Dependencies: 290
-- Name: corretora_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.corretora_id_seq', 1, false);


--
-- TOC entry 6855 (class 0 OID 0)
-- Dependencies: 280
-- Name: estipulante_configuracao_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.estipulante_configuracao_id_seq', 1, false);


--
-- TOC entry 6856 (class 0 OID 0)
-- Dependencies: 278
-- Name: estipulante_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.estipulante_id_seq', 1, false);


--
-- TOC entry 6857 (class 0 OID 0)
-- Dependencies: 245
-- Name: grupo_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.grupo_id_seq', 1, false);


--
-- TOC entry 6858 (class 0 OID 0)
-- Dependencies: 249
-- Name: lotacao_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.lotacao_id_seq', 1, false);


--
-- TOC entry 6859 (class 0 OID 0)
-- Dependencies: 272
-- Name: seguradora_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.seguradora_id_seq', 1, false);


--
-- TOC entry 6860 (class 0 OID 0)
-- Dependencies: 292
-- Name: subestipulante_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.subestipulante_id_seq', 1, false);


--
-- TOC entry 6861 (class 0 OID 0)
-- Dependencies: 247
-- Name: subgrupo_id_seq; Type: SEQUENCE SET; Schema: cadastro; Owner: postgres
--

SELECT pg_catalog.setval('cadastro.subgrupo_id_seq', 1, false);


--
-- TOC entry 6862 (class 0 OID 0)
-- Dependencies: 399
-- Name: agenciador_comissao_config_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.agenciador_comissao_config_id_seq', 1, false);


--
-- TOC entry 6863 (class 0 OID 0)
-- Dependencies: 409
-- Name: agenciamento_corretora_lancamento_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.agenciamento_corretora_lancamento_id_seq', 1, false);


--
-- TOC entry 6864 (class 0 OID 0)
-- Dependencies: 401
-- Name: corretora_agenciador_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.corretora_agenciador_id_seq', 1, false);


--
-- TOC entry 6865 (class 0 OID 0)
-- Dependencies: 284
-- Name: estipulante_comissao_config_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.estipulante_comissao_config_id_seq', 1, false);


--
-- TOC entry 6866 (class 0 OID 0)
-- Dependencies: 429
-- Name: fatura_comissao_resumo_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.fatura_comissao_resumo_id_seq', 1, false);


--
-- TOC entry 6867 (class 0 OID 0)
-- Dependencies: 423
-- Name: fatura_integracao_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.fatura_integracao_id_seq', 1, false);


--
-- TOC entry 6868 (class 0 OID 0)
-- Dependencies: 425
-- Name: fatura_vida_agenciamento_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.fatura_vida_agenciamento_id_seq', 1, false);


--
-- TOC entry 6869 (class 0 OID 0)
-- Dependencies: 427
-- Name: fatura_vida_recebimento_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.fatura_vida_recebimento_id_seq', 1, false);


--
-- TOC entry 6870 (class 0 OID 0)
-- Dependencies: 346
-- Name: lancamento_comissao_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.lancamento_comissao_id_seq', 1, false);


--
-- TOC entry 6871 (class 0 OID 0)
-- Dependencies: 431
-- Name: lancamento_fatura_estipulante_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.lancamento_fatura_estipulante_id_seq', 1, false);


--
-- TOC entry 6872 (class 0 OID 0)
-- Dependencies: 300
-- Name: proposta_participante_id_seq; Type: SEQUENCE SET; Schema: comissao; Owner: postgres
--

SELECT pg_catalog.setval('comissao.proposta_participante_id_seq', 1, false);


--
-- TOC entry 6873 (class 0 OID 0)
-- Dependencies: 261
-- Name: corsan_cliente_id_seq; Type: SEQUENCE SET; Schema: convenio; Owner: postgres
--

SELECT pg_catalog.setval('convenio.corsan_cliente_id_seq', 1, false);


--
-- TOC entry 6874 (class 0 OID 0)
-- Dependencies: 298
-- Name: corsan_proposta_id_seq; Type: SEQUENCE SET; Schema: convenio; Owner: postgres
--

SELECT pg_catalog.setval('convenio.corsan_proposta_id_seq', 1, false);


--
-- TOC entry 6875 (class 0 OID 0)
-- Dependencies: 259
-- Name: siape_cliente_id_seq; Type: SEQUENCE SET; Schema: convenio; Owner: postgres
--

SELECT pg_catalog.setval('convenio.siape_cliente_id_seq', 1, false);


--
-- TOC entry 6876 (class 0 OID 0)
-- Dependencies: 257
-- Name: siape_orgao_id_seq; Type: SEQUENCE SET; Schema: convenio; Owner: postgres
--

SELECT pg_catalog.setval('convenio.siape_orgao_id_seq', 1, false);


--
-- TOC entry 6877 (class 0 OID 0)
-- Dependencies: 276
-- Name: siape_parametro_id_seq; Type: SEQUENCE SET; Schema: convenio; Owner: postgres
--

SELECT pg_catalog.setval('convenio.siape_parametro_id_seq', 1, false);


--
-- TOC entry 6878 (class 0 OID 0)
-- Dependencies: 241
-- Name: banco_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.banco_id_seq', 1, false);


--
-- TOC entry 6879 (class 0 OID 0)
-- Dependencies: 237
-- Name: cidade_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.cidade_id_seq', 1, false);


--
-- TOC entry 6880 (class 0 OID 0)
-- Dependencies: 235
-- Name: estado_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.estado_id_seq', 1, false);


--
-- TOC entry 6881 (class 0 OID 0)
-- Dependencies: 233
-- Name: pessoa_contato_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.pessoa_contato_id_seq', 1, false);


--
-- TOC entry 6882 (class 0 OID 0)
-- Dependencies: 231
-- Name: pessoa_documento_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.pessoa_documento_id_seq', 1, false);


--
-- TOC entry 6883 (class 0 OID 0)
-- Dependencies: 239
-- Name: pessoa_endereco_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.pessoa_endereco_id_seq', 1, false);


--
-- TOC entry 6884 (class 0 OID 0)
-- Dependencies: 229
-- Name: pessoa_id_seq; Type: SEQUENCE SET; Schema: core; Owner: postgres
--

SELECT pg_catalog.setval('core.pessoa_id_seq', 1, false);


--
-- TOC entry 6885 (class 0 OID 0)
-- Dependencies: 377
-- Name: arquivo_acesso_log_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.arquivo_acesso_log_id_seq', 1, false);


--
-- TOC entry 6886 (class 0 OID 0)
-- Dependencies: 371
-- Name: arquivo_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.arquivo_id_seq', 1, false);


--
-- TOC entry 6887 (class 0 OID 0)
-- Dependencies: 375
-- Name: arquivo_versao_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.arquivo_versao_id_seq', 1, false);


--
-- TOC entry 6888 (class 0 OID 0)
-- Dependencies: 373
-- Name: arquivo_vinculo_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.arquivo_vinculo_id_seq', 1, false);


--
-- TOC entry 6889 (class 0 OID 0)
-- Dependencies: 367
-- Name: storage_provider_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.storage_provider_id_seq', 5, true);


--
-- TOC entry 6890 (class 0 OID 0)
-- Dependencies: 369
-- Name: tipo_anexo_id_seq; Type: SEQUENCE SET; Schema: documento; Owner: postgres
--

SELECT pg_catalog.setval('documento.tipo_anexo_id_seq', 1, false);


--
-- TOC entry 6891 (class 0 OID 0)
-- Dependencies: 421
-- Name: cobranca_acompanhamento_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.cobranca_acompanhamento_id_seq', 1, false);


--
-- TOC entry 6892 (class 0 OID 0)
-- Dependencies: 267
-- Name: conta_cobranca_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.conta_cobranca_id_seq', 1, false);


--
-- TOC entry 6893 (class 0 OID 0)
-- Dependencies: 265
-- Name: convenio_cobranca_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.convenio_cobranca_id_seq', 1, false);


--
-- TOC entry 6894 (class 0 OID 0)
-- Dependencies: 282
-- Name: estipulante_faturamento_config_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.estipulante_faturamento_config_id_seq', 1, false);


--
-- TOC entry 6895 (class 0 OID 0)
-- Dependencies: 274
-- Name: forma_pagamento_estipulante_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.forma_pagamento_estipulante_id_seq', 1, false);


--
-- TOC entry 6896 (class 0 OID 0)
-- Dependencies: 415
-- Name: forma_retorno_estipulante_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.forma_retorno_estipulante_id_seq', 1, false);


--
-- TOC entry 6897 (class 0 OID 0)
-- Dependencies: 413
-- Name: forma_retorno_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.forma_retorno_id_seq', 1, false);


--
-- TOC entry 6898 (class 0 OID 0)
-- Dependencies: 417
-- Name: identificador_remessa_api_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.identificador_remessa_api_id_seq', 1, false);


--
-- TOC entry 6899 (class 0 OID 0)
-- Dependencies: 419
-- Name: movimento_cobranca_log_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.movimento_cobranca_log_id_seq', 1, false);


--
-- TOC entry 6900 (class 0 OID 0)
-- Dependencies: 263
-- Name: regra_agrupamento_fatura_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.regra_agrupamento_fatura_id_seq', 7, true);


--
-- TOC entry 6901 (class 0 OID 0)
-- Dependencies: 342
-- Name: retorno_bancario_codigo_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.retorno_bancario_codigo_id_seq', 1, false);


--
-- TOC entry 6902 (class 0 OID 0)
-- Dependencies: 338
-- Name: titulo_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.titulo_id_seq', 1, false);


--
-- TOC entry 6903 (class 0 OID 0)
-- Dependencies: 340
-- Name: titulo_pagamento_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.titulo_pagamento_id_seq', 1, false);


--
-- TOC entry 6904 (class 0 OID 0)
-- Dependencies: 344
-- Name: titulo_retorno_bancario_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.titulo_retorno_bancario_id_seq', 1, false);


--
-- TOC entry 6905 (class 0 OID 0)
-- Dependencies: 336
-- Name: titulo_status_id_seq; Type: SEQUENCE SET; Schema: financeiro; Owner: postgres
--

SELECT pg_catalog.setval('financeiro.titulo_status_id_seq', 7, true);


--
-- TOC entry 6906 (class 0 OID 0)
-- Dependencies: 286
-- Name: referencia_externa_id_seq; Type: SEQUENCE SET; Schema: integracao; Owner: postgres
--

SELECT pg_catalog.setval('integracao.referencia_externa_id_seq', 1, false);


--
-- TOC entry 6907 (class 0 OID 0)
-- Dependencies: 403
-- Name: agenciador_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.agenciador_migration_map_id_seq', 1, false);


--
-- TOC entry 6908 (class 0 OID 0)
-- Dependencies: 411
-- Name: agenciamento_corretora_lancamento_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.agenciamento_corretora_lancamento_migration_map_id_seq', 1, false);


--
-- TOC entry 6909 (class 0 OID 0)
-- Dependencies: 270
-- Name: cliente_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.cliente_migration_map_id_seq', 1, false);


--
-- TOC entry 6910 (class 0 OID 0)
-- Dependencies: 326
-- Name: cobertura_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.cobertura_migration_map_id_seq', 1, false);


--
-- TOC entry 6911 (class 0 OID 0)
-- Dependencies: 405
-- Name: corretora_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.corretora_migration_map_id_seq', 1, false);


--
-- TOC entry 6912 (class 0 OID 0)
-- Dependencies: 379
-- Name: documento_anexo_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.documento_anexo_migration_map_id_seq', 1, false);


--
-- TOC entry 6913 (class 0 OID 0)
-- Dependencies: 288
-- Name: estipulante_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.estipulante_migration_map_id_seq', 1, false);


--
-- TOC entry 6914 (class 0 OID 0)
-- Dependencies: 348
-- Name: movimento_proposta_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.movimento_proposta_migration_map_id_seq', 1, false);


--
-- TOC entry 6915 (class 0 OID 0)
-- Dependencies: 320
-- Name: plano_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.plano_migration_map_id_seq', 1, false);


--
-- TOC entry 6916 (class 0 OID 0)
-- Dependencies: 324
-- Name: produto_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.produto_migration_map_id_seq', 1, false);


--
-- TOC entry 6917 (class 0 OID 0)
-- Dependencies: 352
-- Name: proposta_beneficiario_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.proposta_beneficiario_migration_map_id_seq', 1, false);


--
-- TOC entry 6918 (class 0 OID 0)
-- Dependencies: 330
-- Name: proposta_cobertura_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.proposta_cobertura_migration_map_id_seq', 1, false);


--
-- TOC entry 6919 (class 0 OID 0)
-- Dependencies: 328
-- Name: proposta_item_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.proposta_item_migration_map_id_seq', 1, false);


--
-- TOC entry 6920 (class 0 OID 0)
-- Dependencies: 302
-- Name: proposta_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.proposta_migration_map_id_seq', 1, false);


--
-- TOC entry 6921 (class 0 OID 0)
-- Dependencies: 407
-- Name: proposta_participante_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.proposta_participante_migration_map_id_seq', 1, false);


--
-- TOC entry 6922 (class 0 OID 0)
-- Dependencies: 391
-- Name: protocolo_acompanhamento_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.protocolo_acompanhamento_migration_map_id_seq', 1, false);


--
-- TOC entry 6923 (class 0 OID 0)
-- Dependencies: 389
-- Name: protocolo_item_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.protocolo_item_migration_map_id_seq', 1, false);


--
-- TOC entry 6924 (class 0 OID 0)
-- Dependencies: 387
-- Name: protocolo_lote_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.protocolo_lote_migration_map_id_seq', 1, false);


--
-- TOC entry 6925 (class 0 OID 0)
-- Dependencies: 365
-- Name: sinistro_acompanhamento_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.sinistro_acompanhamento_migration_map_id_seq', 1, false);


--
-- TOC entry 6926 (class 0 OID 0)
-- Dependencies: 363
-- Name: sinistro_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.sinistro_migration_map_id_seq', 1, false);


--
-- TOC entry 6927 (class 0 OID 0)
-- Dependencies: 322
-- Name: tabela_preco_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.tabela_preco_migration_map_id_seq', 1, false);


--
-- TOC entry 6928 (class 0 OID 0)
-- Dependencies: 318
-- Name: tipo_produto_migration_map_id_seq; Type: SEQUENCE SET; Schema: legado; Owner: postgres
--

SELECT pg_catalog.setval('legado.tipo_produto_migration_map_id_seq', 1, false);


--
-- TOC entry 6929 (class 0 OID 0)
-- Dependencies: 312
-- Name: cobertura_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.cobertura_id_seq', 1, false);


--
-- TOC entry 6930 (class 0 OID 0)
-- Dependencies: 332
-- Name: movimento_tipo_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.movimento_tipo_id_seq', 1, false);


--
-- TOC entry 6931 (class 0 OID 0)
-- Dependencies: 306
-- Name: plano_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.plano_id_seq', 1, false);


--
-- TOC entry 6932 (class 0 OID 0)
-- Dependencies: 310
-- Name: produto_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.produto_id_seq', 1, false);


--
-- TOC entry 6933 (class 0 OID 0)
-- Dependencies: 350
-- Name: proposta_beneficiario_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_beneficiario_id_seq', 1, false);


--
-- TOC entry 6934 (class 0 OID 0)
-- Dependencies: 316
-- Name: proposta_cobertura_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_cobertura_id_seq', 1, false);


--
-- TOC entry 6935 (class 0 OID 0)
-- Dependencies: 296
-- Name: proposta_historico_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_historico_id_seq', 1, false);


--
-- TOC entry 6936 (class 0 OID 0)
-- Dependencies: 294
-- Name: proposta_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_id_seq', 1, false);


--
-- TOC entry 6937 (class 0 OID 0)
-- Dependencies: 314
-- Name: proposta_item_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_item_id_seq', 1, false);


--
-- TOC entry 6938 (class 0 OID 0)
-- Dependencies: 334
-- Name: proposta_movimento_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.proposta_movimento_id_seq', 1, false);


--
-- TOC entry 6939 (class 0 OID 0)
-- Dependencies: 308
-- Name: tabela_preco_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.tabela_preco_id_seq', 1, false);


--
-- TOC entry 6940 (class 0 OID 0)
-- Dependencies: 304
-- Name: tipo_produto_id_seq; Type: SEQUENCE SET; Schema: seguro; Owner: postgres
--

SELECT pg_catalog.setval('seguro.tipo_produto_id_seq', 1, false);


--
-- TOC entry 6941 (class 0 OID 0)
-- Dependencies: 357
-- Name: acompanhamento_id_seq; Type: SEQUENCE SET; Schema: sinistro; Owner: postgres
--

SELECT pg_catalog.setval('sinistro.acompanhamento_id_seq', 1, false);


--
-- TOC entry 6942 (class 0 OID 0)
-- Dependencies: 359
-- Name: sinistro_beneficiario_id_seq; Type: SEQUENCE SET; Schema: sinistro; Owner: postgres
--

SELECT pg_catalog.setval('sinistro.sinistro_beneficiario_id_seq', 1, false);


--
-- TOC entry 6943 (class 0 OID 0)
-- Dependencies: 361
-- Name: sinistro_cobertura_id_seq; Type: SEQUENCE SET; Schema: sinistro; Owner: postgres
--

SELECT pg_catalog.setval('sinistro.sinistro_cobertura_id_seq', 1, false);


--
-- TOC entry 6944 (class 0 OID 0)
-- Dependencies: 355
-- Name: sinistro_id_seq; Type: SEQUENCE SET; Schema: sinistro; Owner: postgres
--

SELECT pg_catalog.setval('sinistro.sinistro_id_seq', 1, false);


--
-- TOC entry 6010 (class 2606 OID 8862258)
-- Name: protocolo_acompanhamento protocolo_acompanhamento_pkey; Type: CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_acompanhamento
    ADD CONSTRAINT protocolo_acompanhamento_pkey PRIMARY KEY (id);


--
-- TOC entry 6005 (class 2606 OID 8862216)
-- Name: protocolo_item protocolo_item_pkey; Type: CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_pkey PRIMARY KEY (id);


--
-- TOC entry 5996 (class 2606 OID 8862200)
-- Name: protocolo_lote protocolo_lote_pkey; Type: CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_lote
    ADD CONSTRAINT protocolo_lote_pkey PRIMARY KEY (id);


--
-- TOC entry 6037 (class 2606 OID 8862372)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_pkey; Type: CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_pkey PRIMARY KEY (id);


--
-- TOC entry 6031 (class 2606 OID 8862362)
-- Name: protocolo_relatorio_seguradora protocolo_relatorio_seguradora_pkey; Type: CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora
    ADD CONSTRAINT protocolo_relatorio_seguradora_pkey PRIMARY KEY (id);


--
-- TOC entry 6039 (class 2606 OID 8862414)
-- Name: agenciador agenciador_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador
    ADD CONSTRAINT agenciador_pkey PRIMARY KEY (id);


--
-- TOC entry 5629 (class 2606 OID 8860392)
-- Name: cliente_dependente cliente_dependente_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_dependente
    ADD CONSTRAINT cliente_dependente_pkey PRIMARY KEY (id);


--
-- TOC entry 5624 (class 2606 OID 8860371)
-- Name: cliente cliente_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente
    ADD CONSTRAINT cliente_pkey PRIMARY KEY (id);


--
-- TOC entry 5611 (class 2606 OID 8860321)
-- Name: cliente_status cliente_status_codigo_key; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_status
    ADD CONSTRAINT cliente_status_codigo_key UNIQUE (codigo);


--
-- TOC entry 5613 (class 2606 OID 8860319)
-- Name: cliente_status cliente_status_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_status
    ADD CONSTRAINT cliente_status_pkey PRIMARY KEY (id);


--
-- TOC entry 5632 (class 2606 OID 8860414)
-- Name: cliente_vinculo cliente_vinculo_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_pkey PRIMARY KEY (id);


--
-- TOC entry 5714 (class 2606 OID 8860837)
-- Name: corretora corretora_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.corretora
    ADD CONSTRAINT corretora_pkey PRIMARY KEY (id);


--
-- TOC entry 5690 (class 2606 OID 8860713)
-- Name: estipulante_configuracao estipulante_configuracao_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante_configuracao
    ADD CONSTRAINT estipulante_configuracao_pkey PRIMARY KEY (id);


--
-- TOC entry 5683 (class 2606 OID 8860677)
-- Name: estipulante estipulante_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante
    ADD CONSTRAINT estipulante_pkey PRIMARY KEY (id);


--
-- TOC entry 5615 (class 2606 OID 8860329)
-- Name: grupo grupo_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.grupo
    ADD CONSTRAINT grupo_pkey PRIMARY KEY (id);


--
-- TOC entry 5621 (class 2606 OID 8860352)
-- Name: lotacao lotacao_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.lotacao
    ADD CONSTRAINT lotacao_pkey PRIMARY KEY (id);


--
-- TOC entry 5674 (class 2606 OID 8860636)
-- Name: seguradora seguradora_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.seguradora
    ADD CONSTRAINT seguradora_pkey PRIMARY KEY (id);


--
-- TOC entry 5721 (class 2606 OID 8860862)
-- Name: subestipulante subestipulante_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante
    ADD CONSTRAINT subestipulante_pkey PRIMARY KEY (id);


--
-- TOC entry 5618 (class 2606 OID 8860338)
-- Name: subgrupo subgrupo_pkey; Type: CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subgrupo
    ADD CONSTRAINT subgrupo_pkey PRIMARY KEY (id);


--
-- TOC entry 6047 (class 2606 OID 8862458)
-- Name: agenciador_comissao_config agenciador_comissao_config_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciador_comissao_config
    ADD CONSTRAINT agenciador_comissao_config_pkey PRIMARY KEY (id);


--
-- TOC entry 6074 (class 2606 OID 8862601)
-- Name: agenciamento_corretora_lancamento agenciamento_corretora_lancamento_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciamento_corretora_lancamento
    ADD CONSTRAINT agenciamento_corretora_lancamento_pkey PRIMARY KEY (id);


--
-- TOC entry 6052 (class 2606 OID 8862475)
-- Name: corretora_agenciador corretora_agenciador_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.corretora_agenciador
    ADD CONSTRAINT corretora_agenciador_pkey PRIMARY KEY (id);


--
-- TOC entry 5699 (class 2606 OID 8860772)
-- Name: estipulante_comissao_config estipulante_comissao_config_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.estipulante_comissao_config
    ADD CONSTRAINT estipulante_comissao_config_pkey PRIMARY KEY (id);


--
-- TOC entry 6132 (class 2606 OID 8862825)
-- Name: fatura_comissao_resumo fatura_comissao_resumo_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_comissao_resumo
    ADD CONSTRAINT fatura_comissao_resumo_pkey PRIMARY KEY (id);


--
-- TOC entry 6111 (class 2606 OID 8862755)
-- Name: fatura_integracao fatura_integracao_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_integracao
    ADD CONSTRAINT fatura_integracao_pkey PRIMARY KEY (id);


--
-- TOC entry 6120 (class 2606 OID 8862786)
-- Name: fatura_vida_agenciamento fatura_vida_agenciamento_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_agenciamento
    ADD CONSTRAINT fatura_vida_agenciamento_pkey PRIMARY KEY (id);


--
-- TOC entry 6126 (class 2606 OID 8862803)
-- Name: fatura_vida_recebimento fatura_vida_recebimento_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_recebimento
    ADD CONSTRAINT fatura_vida_recebimento_pkey PRIMARY KEY (id);


--
-- TOC entry 5878 (class 2606 OID 8861625)
-- Name: lancamento_comissao lancamento_comissao_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_pkey PRIMARY KEY (id);


--
-- TOC entry 6140 (class 2606 OID 8862841)
-- Name: lancamento_fatura_estipulante lancamento_fatura_estipulante_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_fatura_estipulante
    ADD CONSTRAINT lancamento_fatura_estipulante_pkey PRIMARY KEY (id);


--
-- TOC entry 5749 (class 2606 OID 8861039)
-- Name: proposta_participante proposta_participante_pkey; Type: CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.proposta_participante
    ADD CONSTRAINT proposta_participante_pkey PRIMARY KEY (id);


--
-- TOC entry 5646 (class 2606 OID 8860498)
-- Name: corsan_cliente corsan_cliente_pkey; Type: CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_cliente
    ADD CONSTRAINT corsan_cliente_pkey PRIMARY KEY (id);


--
-- TOC entry 5740 (class 2606 OID 8861008)
-- Name: corsan_proposta corsan_proposta_pkey; Type: CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta
    ADD CONSTRAINT corsan_proposta_pkey PRIMARY KEY (id);


--
-- TOC entry 5644 (class 2606 OID 8860467)
-- Name: siape_cliente siape_cliente_pkey; Type: CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente
    ADD CONSTRAINT siape_cliente_pkey PRIMARY KEY (id);


--
-- TOC entry 5639 (class 2606 OID 8860457)
-- Name: siape_orgao siape_orgao_pkey; Type: CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_orgao
    ADD CONSTRAINT siape_orgao_pkey PRIMARY KEY (id);


--
-- TOC entry 5680 (class 2606 OID 8860662)
-- Name: siape_parametro siape_parametro_pkey; Type: CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_parametro
    ADD CONSTRAINT siape_parametro_pkey PRIMARY KEY (id);


--
-- TOC entry 5607 (class 2606 OID 8860309)
-- Name: banco banco_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.banco
    ADD CONSTRAINT banco_pkey PRIMARY KEY (id);


--
-- TOC entry 5600 (class 2606 OID 8860270)
-- Name: cidade cidade_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.cidade
    ADD CONSTRAINT cidade_pkey PRIMARY KEY (id);


--
-- TOC entry 5596 (class 2606 OID 8860260)
-- Name: estado estado_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.estado
    ADD CONSTRAINT estado_pkey PRIMARY KEY (id);


--
-- TOC entry 5598 (class 2606 OID 8860262)
-- Name: estado estado_uf_key; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.estado
    ADD CONSTRAINT estado_uf_key UNIQUE (uf);


--
-- TOC entry 5594 (class 2606 OID 8860246)
-- Name: pessoa_contato pessoa_contato_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_contato
    ADD CONSTRAINT pessoa_contato_pkey PRIMARY KEY (id);


--
-- TOC entry 5590 (class 2606 OID 8860229)
-- Name: pessoa_documento pessoa_documento_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_documento
    ADD CONSTRAINT pessoa_documento_pkey PRIMARY KEY (id);


--
-- TOC entry 5605 (class 2606 OID 8860288)
-- Name: pessoa_endereco pessoa_endereco_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_endereco
    ADD CONSTRAINT pessoa_endereco_pkey PRIMARY KEY (id);


--
-- TOC entry 5586 (class 2606 OID 8860217)
-- Name: pessoa pessoa_pkey; Type: CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa
    ADD CONSTRAINT pessoa_pkey PRIMARY KEY (id);


--
-- TOC entry 5980 (class 2606 OID 8862130)
-- Name: arquivo_acesso_log arquivo_acesso_log_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_acesso_log
    ADD CONSTRAINT arquivo_acesso_log_pkey PRIMARY KEY (id);


--
-- TOC entry 5957 (class 2606 OID 8862058)
-- Name: arquivo arquivo_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo
    ADD CONSTRAINT arquivo_pkey PRIMARY KEY (id);


--
-- TOC entry 5975 (class 2606 OID 8862109)
-- Name: arquivo_versao arquivo_versao_arquivo_id_versao_key; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_versao
    ADD CONSTRAINT arquivo_versao_arquivo_id_versao_key UNIQUE (arquivo_id, versao);


--
-- TOC entry 5977 (class 2606 OID 8862107)
-- Name: arquivo_versao arquivo_versao_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_versao
    ADD CONSTRAINT arquivo_versao_pkey PRIMARY KEY (id);


--
-- TOC entry 5968 (class 2606 OID 8862082)
-- Name: arquivo_vinculo arquivo_vinculo_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_vinculo
    ADD CONSTRAINT arquivo_vinculo_pkey PRIMARY KEY (id);


--
-- TOC entry 5949 (class 2606 OID 8862027)
-- Name: storage_provider storage_provider_codigo_key; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.storage_provider
    ADD CONSTRAINT storage_provider_codigo_key UNIQUE (codigo);


--
-- TOC entry 5951 (class 2606 OID 8862025)
-- Name: storage_provider storage_provider_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.storage_provider
    ADD CONSTRAINT storage_provider_pkey PRIMARY KEY (id);


--
-- TOC entry 5954 (class 2606 OID 8862042)
-- Name: tipo_anexo tipo_anexo_pkey; Type: CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.tipo_anexo
    ADD CONSTRAINT tipo_anexo_pkey PRIMARY KEY (id);


--
-- TOC entry 6106 (class 2606 OID 8862731)
-- Name: cobranca_acompanhamento cobranca_acompanhamento_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.cobranca_acompanhamento
    ADD CONSTRAINT cobranca_acompanhamento_pkey PRIMARY KEY (id);


--
-- TOC entry 5657 (class 2606 OID 8860552)
-- Name: conta_cobranca conta_cobranca_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_pkey PRIMARY KEY (id);


--
-- TOC entry 5653 (class 2606 OID 8860536)
-- Name: convenio_cobranca convenio_cobranca_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.convenio_cobranca
    ADD CONSTRAINT convenio_cobranca_pkey PRIMARY KEY (id);


--
-- TOC entry 5694 (class 2606 OID 8860734)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_pkey PRIMARY KEY (id);


--
-- TOC entry 5677 (class 2606 OID 8860652)
-- Name: forma_pagamento_estipulante forma_pagamento_estipulante_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_pagamento_estipulante
    ADD CONSTRAINT forma_pagamento_estipulante_pkey PRIMARY KEY (id);


--
-- TOC entry 6091 (class 2606 OID 8862676)
-- Name: forma_retorno_estipulante forma_retorno_estipulante_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno_estipulante
    ADD CONSTRAINT forma_retorno_estipulante_pkey PRIMARY KEY (id);


--
-- TOC entry 6088 (class 2606 OID 8862667)
-- Name: forma_retorno forma_retorno_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno
    ADD CONSTRAINT forma_retorno_pkey PRIMARY KEY (id);


--
-- TOC entry 6096 (class 2606 OID 8862697)
-- Name: identificador_remessa_api identificador_remessa_api_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.identificador_remessa_api
    ADD CONSTRAINT identificador_remessa_api_pkey PRIMARY KEY (id);


--
-- TOC entry 6104 (class 2606 OID 8862707)
-- Name: movimento_cobranca_log movimento_cobranca_log_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.movimento_cobranca_log
    ADD CONSTRAINT movimento_cobranca_log_pkey PRIMARY KEY (id);


--
-- TOC entry 5649 (class 2606 OID 8860525)
-- Name: regra_agrupamento_fatura regra_agrupamento_fatura_codigo_key; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.regra_agrupamento_fatura
    ADD CONSTRAINT regra_agrupamento_fatura_codigo_key UNIQUE (codigo);


--
-- TOC entry 5651 (class 2606 OID 8860523)
-- Name: regra_agrupamento_fatura regra_agrupamento_fatura_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.regra_agrupamento_fatura
    ADD CONSTRAINT regra_agrupamento_fatura_pkey PRIMARY KEY (id);


--
-- TOC entry 5865 (class 2606 OID 8861584)
-- Name: retorno_bancario_codigo retorno_bancario_codigo_codigo_descricao_key; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.retorno_bancario_codigo
    ADD CONSTRAINT retorno_bancario_codigo_codigo_descricao_key UNIQUE (codigo, descricao);


--
-- TOC entry 5867 (class 2606 OID 8861582)
-- Name: retorno_bancario_codigo retorno_bancario_codigo_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.retorno_bancario_codigo
    ADD CONSTRAINT retorno_bancario_codigo_pkey PRIMARY KEY (id);


--
-- TOC entry 5861 (class 2606 OID 8861558)
-- Name: titulo_pagamento titulo_pagamento_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_pagamento
    ADD CONSTRAINT titulo_pagamento_pkey PRIMARY KEY (id);


--
-- TOC entry 5856 (class 2606 OID 8861492)
-- Name: titulo titulo_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_pkey PRIMARY KEY (id);


--
-- TOC entry 5872 (class 2606 OID 8861596)
-- Name: titulo_retorno_bancario titulo_retorno_bancario_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_retorno_bancario
    ADD CONSTRAINT titulo_retorno_bancario_pkey PRIMARY KEY (id);


--
-- TOC entry 5844 (class 2606 OID 8861481)
-- Name: titulo_status titulo_status_codigo_key; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_status
    ADD CONSTRAINT titulo_status_codigo_key UNIQUE (codigo);


--
-- TOC entry 5846 (class 2606 OID 8861479)
-- Name: titulo_status titulo_status_pkey; Type: CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_status
    ADD CONSTRAINT titulo_status_pkey PRIMARY KEY (id);


--
-- TOC entry 5704 (class 2606 OID 8860791)
-- Name: referencia_externa referencia_externa_pkey; Type: CONSTRAINT; Schema: integracao; Owner: postgres
--

ALTER TABLE ONLY integracao.referencia_externa
    ADD CONSTRAINT referencia_externa_pkey PRIMARY KEY (id);


--
-- TOC entry 5706 (class 2606 OID 8860793)
-- Name: referencia_externa referencia_externa_sistema_entidade_tipo_chave_externa_key; Type: CONSTRAINT; Schema: integracao; Owner: postgres
--

ALTER TABLE ONLY integracao.referencia_externa
    ADD CONSTRAINT referencia_externa_sistema_entidade_tipo_chave_externa_key UNIQUE (sistema, entidade_tipo, chave_externa);


--
-- TOC entry 6057 (class 2606 OID 8862514)
-- Name: agenciador_migration_map agenciador_migration_map_legado_agenciador_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map
    ADD CONSTRAINT agenciador_migration_map_legado_agenciador_id_key UNIQUE (legado_agenciador_id);


--
-- TOC entry 6059 (class 2606 OID 8862512)
-- Name: agenciador_migration_map agenciador_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map
    ADD CONSTRAINT agenciador_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6082 (class 2606 OID 8862634)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancamento_mi_legado_agenciamento_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancamento_mi_legado_agenciamento_id_key UNIQUE (legado_agenciamento_id);


--
-- TOC entry 6084 (class 2606 OID 8862632)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancamento_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancamento_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5666 (class 2606 OID 8860603)
-- Name: cliente_migration_map cliente_migration_map_legado_cliente_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map
    ADD CONSTRAINT cliente_migration_map_legado_cliente_id_key UNIQUE (legado_cliente_id);


--
-- TOC entry 5668 (class 2606 OID 8860601)
-- Name: cliente_migration_map cliente_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map
    ADD CONSTRAINT cliente_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5811 (class 2606 OID 8861309)
-- Name: cobertura_migration_map cobertura_migration_map_legado_cobertura_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cobertura_migration_map
    ADD CONSTRAINT cobertura_migration_map_legado_cobertura_id_key UNIQUE (legado_cobertura_id);


--
-- TOC entry 5813 (class 2606 OID 8861307)
-- Name: cobertura_migration_map cobertura_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cobertura_migration_map
    ADD CONSTRAINT cobertura_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6064 (class 2606 OID 8862544)
-- Name: corretora_migration_map corretora_migration_map_legado_corretora_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.corretora_migration_map
    ADD CONSTRAINT corretora_migration_map_legado_corretora_id_key UNIQUE (legado_corretora_id);


--
-- TOC entry 6066 (class 2606 OID 8862542)
-- Name: corretora_migration_map corretora_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.corretora_migration_map
    ADD CONSTRAINT corretora_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5984 (class 2606 OID 8862149)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_legado_documento_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_legado_documento_id_key UNIQUE (legado_documento_id);


--
-- TOC entry 5986 (class 2606 OID 8862147)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5708 (class 2606 OID 8860807)
-- Name: estipulante_migration_map estipulante_migration_map_legado_estipulante_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.estipulante_migration_map
    ADD CONSTRAINT estipulante_migration_map_legado_estipulante_id_key UNIQUE (legado_estipulante_id);


--
-- TOC entry 5710 (class 2606 OID 8860805)
-- Name: estipulante_migration_map estipulante_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.estipulante_migration_map
    ADD CONSTRAINT estipulante_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5885 (class 2606 OID 8861671)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_m_legado_movimento_proposta_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_m_legado_movimento_proposta_id_key UNIQUE (legado_movimento_proposta_id);


--
-- TOC entry 5887 (class 2606 OID 8861669)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5799 (class 2606 OID 8861264)
-- Name: plano_migration_map plano_migration_map_legado_plano_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.plano_migration_map
    ADD CONSTRAINT plano_migration_map_legado_plano_id_key UNIQUE (legado_plano_id);


--
-- TOC entry 5801 (class 2606 OID 8861262)
-- Name: plano_migration_map plano_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.plano_migration_map
    ADD CONSTRAINT plano_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5807 (class 2606 OID 8861294)
-- Name: produto_migration_map produto_migration_map_legado_produto_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.produto_migration_map
    ADD CONSTRAINT produto_migration_map_legado_produto_id_key UNIQUE (legado_produto_id);


--
-- TOC entry 5809 (class 2606 OID 8861292)
-- Name: produto_migration_map produto_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.produto_migration_map
    ADD CONSTRAINT produto_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5900 (class 2606 OID 8861772)
-- Name: proposta_beneficiario_migration_map proposta_beneficiario_migration_map_legado_beneficiario_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map
    ADD CONSTRAINT proposta_beneficiario_migration_map_legado_beneficiario_id_key UNIQUE (legado_beneficiario_id);


--
-- TOC entry 5902 (class 2606 OID 8861770)
-- Name: proposta_beneficiario_migration_map proposta_beneficiario_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map
    ADD CONSTRAINT proposta_beneficiario_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5823 (class 2606 OID 8861371)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_m_legado_proposta_cobertura_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_m_legado_proposta_cobertura_id_key UNIQUE (legado_proposta_cobertura_id);


--
-- TOC entry 5825 (class 2606 OID 8861369)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5817 (class 2606 OID 8861326)
-- Name: proposta_item_migration_map proposta_item_migration_map_legado_proposta_tipo_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_legado_proposta_tipo_id_key UNIQUE (legado_proposta_tipo_id);


--
-- TOC entry 5819 (class 2606 OID 8861324)
-- Name: proposta_item_migration_map proposta_item_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5755 (class 2606 OID 8861058)
-- Name: proposta_migration_map proposta_migration_map_legado_proposta_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_legado_proposta_id_key UNIQUE (legado_proposta_id);


--
-- TOC entry 5757 (class 2606 OID 8861056)
-- Name: proposta_migration_map proposta_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6072 (class 2606 OID 8862565)
-- Name: proposta_participante_migration_map proposta_participante_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map
    ADD CONSTRAINT proposta_participante_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6026 (class 2606 OID 8862341)
-- Name: protocolo_acompanhamento_migration_map protocolo_acompanhamento_migration_legado_acompanhamento_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_acompanhamento_migration_map
    ADD CONSTRAINT protocolo_acompanhamento_migration_legado_acompanhamento_id_key UNIQUE (legado_acompanhamento_id);


--
-- TOC entry 6028 (class 2606 OID 8862339)
-- Name: protocolo_acompanhamento_migration_map protocolo_acompanhamento_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_acompanhamento_migration_map
    ADD CONSTRAINT protocolo_acompanhamento_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6021 (class 2606 OID 8862296)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_origem_legado_legado_cliente_p_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_origem_legado_legado_cliente_p_key UNIQUE (origem_legado, legado_cliente_protocolo_id);


--
-- TOC entry 6023 (class 2606 OID 8862294)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 6014 (class 2606 OID 8862278)
-- Name: protocolo_lote_migration_map protocolo_lote_migration_map_legado_protocolo_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_lote_migration_map
    ADD CONSTRAINT protocolo_lote_migration_map_legado_protocolo_id_key UNIQUE (legado_protocolo_id);


--
-- TOC entry 6016 (class 2606 OID 8862276)
-- Name: protocolo_lote_migration_map protocolo_lote_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_lote_migration_map
    ADD CONSTRAINT protocolo_lote_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5945 (class 2606 OID 8862001)
-- Name: sinistro_acompanhamento_migration_map sinistro_acompanhamento_migration__legado_acompanhamento_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_acompanhamento_migration_map
    ADD CONSTRAINT sinistro_acompanhamento_migration__legado_acompanhamento_id_key UNIQUE (legado_acompanhamento_id);


--
-- TOC entry 5947 (class 2606 OID 8861999)
-- Name: sinistro_acompanhamento_migration_map sinistro_acompanhamento_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_acompanhamento_migration_map
    ADD CONSTRAINT sinistro_acompanhamento_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5940 (class 2606 OID 8861956)
-- Name: sinistro_migration_map sinistro_migration_map_legado_sinistro_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_legado_sinistro_id_key UNIQUE (legado_sinistro_id);


--
-- TOC entry 5942 (class 2606 OID 8861954)
-- Name: sinistro_migration_map sinistro_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5803 (class 2606 OID 8861279)
-- Name: tabela_preco_migration_map tabela_preco_migration_map_legado_tabela_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tabela_preco_migration_map
    ADD CONSTRAINT tabela_preco_migration_map_legado_tabela_id_key UNIQUE (legado_tabela_id);


--
-- TOC entry 5805 (class 2606 OID 8861277)
-- Name: tabela_preco_migration_map tabela_preco_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tabela_preco_migration_map
    ADD CONSTRAINT tabela_preco_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5795 (class 2606 OID 8861249)
-- Name: tipo_produto_migration_map tipo_produto_migration_map_legado_tipo_id_key; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tipo_produto_migration_map
    ADD CONSTRAINT tipo_produto_migration_map_legado_tipo_id_key UNIQUE (legado_tipo_id);


--
-- TOC entry 5797 (class 2606 OID 8861247)
-- Name: tipo_produto_migration_map tipo_produto_migration_map_pkey; Type: CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tipo_produto_migration_map
    ADD CONSTRAINT tipo_produto_migration_map_pkey PRIMARY KEY (id);


--
-- TOC entry 5777 (class 2606 OID 8861167)
-- Name: cobertura cobertura_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.cobertura
    ADD CONSTRAINT cobertura_pkey PRIMARY KEY (id);


--
-- TOC entry 5829 (class 2606 OID 8861408)
-- Name: movimento_tipo movimento_tipo_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.movimento_tipo
    ADD CONSTRAINT movimento_tipo_pkey PRIMARY KEY (id);


--
-- TOC entry 5765 (class 2606 OID 8861119)
-- Name: plano plano_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.plano
    ADD CONSTRAINT plano_pkey PRIMARY KEY (id);


--
-- TOC entry 5774 (class 2606 OID 8861143)
-- Name: produto produto_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.produto
    ADD CONSTRAINT produto_pkey PRIMARY KEY (id);


--
-- TOC entry 5894 (class 2606 OID 8861743)
-- Name: proposta_beneficiario proposta_beneficiario_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_beneficiario
    ADD CONSTRAINT proposta_beneficiario_pkey PRIMARY KEY (id);


--
-- TOC entry 5792 (class 2606 OID 8861220)
-- Name: proposta_cobertura proposta_cobertura_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_cobertura
    ADD CONSTRAINT proposta_cobertura_pkey PRIMARY KEY (id);


--
-- TOC entry 5738 (class 2606 OID 8860986)
-- Name: proposta_historico proposta_historico_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_historico
    ADD CONSTRAINT proposta_historico_pkey PRIMARY KEY (id);


--
-- TOC entry 5786 (class 2606 OID 8861179)
-- Name: proposta_item proposta_item_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_pkey PRIMARY KEY (id);


--
-- TOC entry 5841 (class 2606 OID 8861423)
-- Name: proposta_movimento proposta_movimento_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_pkey PRIMARY KEY (id);


--
-- TOC entry 5733 (class 2606 OID 8860900)
-- Name: proposta proposta_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_pkey PRIMARY KEY (id);


--
-- TOC entry 5662 (class 2606 OID 8860590)
-- Name: proposta_status proposta_status_codigo_key; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_status
    ADD CONSTRAINT proposta_status_codigo_key UNIQUE (codigo);


--
-- TOC entry 5664 (class 2606 OID 8860588)
-- Name: proposta_status proposta_status_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_status
    ADD CONSTRAINT proposta_status_pkey PRIMARY KEY (id);


--
-- TOC entry 5768 (class 2606 OID 8861132)
-- Name: tabela_preco tabela_preco_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.tabela_preco
    ADD CONSTRAINT tabela_preco_pkey PRIMARY KEY (id);


--
-- TOC entry 5760 (class 2606 OID 8861107)
-- Name: tipo_produto tipo_produto_pkey; Type: CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.tipo_produto
    ADD CONSTRAINT tipo_produto_pkey PRIMARY KEY (id);


--
-- TOC entry 5919 (class 2606 OID 8861868)
-- Name: acompanhamento acompanhamento_pkey; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.acompanhamento
    ADD CONSTRAINT acompanhamento_pkey PRIMARY KEY (id);


--
-- TOC entry 5927 (class 2606 OID 8861887)
-- Name: sinistro_beneficiario sinistro_beneficiario_pkey; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario
    ADD CONSTRAINT sinistro_beneficiario_pkey PRIMARY KEY (id);


--
-- TOC entry 5933 (class 2606 OID 8861920)
-- Name: sinistro_cobertura sinistro_cobertura_pkey; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura
    ADD CONSTRAINT sinistro_cobertura_pkey PRIMARY KEY (id);


--
-- TOC entry 5916 (class 2606 OID 8861814)
-- Name: sinistro sinistro_pkey; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_pkey PRIMARY KEY (id);


--
-- TOC entry 5904 (class 2606 OID 8861801)
-- Name: sinistro_status sinistro_status_codigo_key; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_status
    ADD CONSTRAINT sinistro_status_codigo_key UNIQUE (codigo);


--
-- TOC entry 5906 (class 2606 OID 8861799)
-- Name: sinistro_status sinistro_status_pkey; Type: CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_status
    ADD CONSTRAINT sinistro_status_pkey PRIMARY KEY (id);


--
-- TOC entry 6007 (class 1259 OID 8862266)
-- Name: ix_protocolo_acompanhamento_data; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_acompanhamento_data ON atendimento.protocolo_acompanhamento USING btree (data_acompanhamento);


--
-- TOC entry 6008 (class 1259 OID 8862265)
-- Name: ix_protocolo_acompanhamento_lote; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_acompanhamento_lote ON atendimento.protocolo_acompanhamento USING btree (protocolo_lote_id);


--
-- TOC entry 5998 (class 1259 OID 8862244)
-- Name: ix_protocolo_item_cliente; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_cliente ON atendimento.protocolo_item USING btree (cliente_id);


--
-- TOC entry 5999 (class 1259 OID 8862246)
-- Name: ix_protocolo_item_estipulante; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_estipulante ON atendimento.protocolo_item USING btree (estipulante_id);


--
-- TOC entry 6000 (class 1259 OID 8862243)
-- Name: ix_protocolo_item_lote; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_lote ON atendimento.protocolo_item USING btree (protocolo_lote_id);


--
-- TOC entry 6001 (class 1259 OID 8862248)
-- Name: ix_protocolo_item_matricula; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_matricula ON atendimento.protocolo_item USING btree (matricula);


--
-- TOC entry 6002 (class 1259 OID 8862247)
-- Name: ix_protocolo_item_tipo; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_tipo ON atendimento.protocolo_item USING btree (tipo_item);


--
-- TOC entry 6003 (class 1259 OID 8862245)
-- Name: ix_protocolo_item_vinculo; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_item_vinculo ON atendimento.protocolo_item USING btree (cliente_vinculo_id);


--
-- TOC entry 5992 (class 1259 OID 8862204)
-- Name: ix_protocolo_lote_consultor; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_lote_consultor ON atendimento.protocolo_lote USING btree (consultor_legado_id);


--
-- TOC entry 5993 (class 1259 OID 8862203)
-- Name: ix_protocolo_lote_data; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_lote_data ON atendimento.protocolo_lote USING btree (data_protocolo);


--
-- TOC entry 5994 (class 1259 OID 8862202)
-- Name: ix_protocolo_lote_numero; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_lote_numero ON atendimento.protocolo_lote USING btree (numero_protocolo);


--
-- TOC entry 6029 (class 1259 OID 8862364)
-- Name: ix_protocolo_rel_seg_data; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_rel_seg_data ON atendimento.protocolo_relatorio_seguradora USING btree (data_relatorio);


--
-- TOC entry 6033 (class 1259 OID 8862400)
-- Name: ix_protocolo_rel_seg_item_cliente; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_rel_seg_item_cliente ON atendimento.protocolo_relatorio_seguradora_item USING btree (cliente_id);


--
-- TOC entry 6034 (class 1259 OID 8862399)
-- Name: ix_protocolo_rel_seg_item_protocolo; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_rel_seg_item_protocolo ON atendimento.protocolo_relatorio_seguradora_item USING btree (protocolo_lote_id);


--
-- TOC entry 6035 (class 1259 OID 8862398)
-- Name: ix_protocolo_rel_seg_item_relatorio; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE INDEX ix_protocolo_rel_seg_item_relatorio ON atendimento.protocolo_relatorio_seguradora_item USING btree (relatorio_id);


--
-- TOC entry 6011 (class 1259 OID 8862264)
-- Name: ux_protocolo_acompanhamento_legado; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE UNIQUE INDEX ux_protocolo_acompanhamento_legado ON atendimento.protocolo_acompanhamento USING btree (legado_id);


--
-- TOC entry 6006 (class 1259 OID 8862242)
-- Name: ux_protocolo_item_legado_origem; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE UNIQUE INDEX ux_protocolo_item_legado_origem ON atendimento.protocolo_item USING btree (origem_legado, legado_id);


--
-- TOC entry 5997 (class 1259 OID 8862201)
-- Name: ux_protocolo_lote_legado; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE UNIQUE INDEX ux_protocolo_lote_legado ON atendimento.protocolo_lote USING btree (legado_id);


--
-- TOC entry 6032 (class 1259 OID 8862363)
-- Name: ux_protocolo_rel_seg_legado; Type: INDEX; Schema: atendimento; Owner: postgres
--

CREATE UNIQUE INDEX ux_protocolo_rel_seg_legado ON atendimento.protocolo_relatorio_seguradora USING btree (legado_id);


--
-- TOC entry 6040 (class 1259 OID 8862439)
-- Name: ix_agenciador_coordenador; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_agenciador_coordenador ON cadastro.agenciador USING btree (coordenador_id);


--
-- TOC entry 6041 (class 1259 OID 8862438)
-- Name: ix_agenciador_cpf; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_agenciador_cpf ON cadastro.agenciador USING btree (cpf_limpo);


--
-- TOC entry 6042 (class 1259 OID 8862440)
-- Name: ix_agenciador_desativado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_agenciador_desativado ON cadastro.agenciador USING btree (desativado);


--
-- TOC entry 6043 (class 1259 OID 8862437)
-- Name: ix_agenciador_nome_trgm; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_agenciador_nome_trgm ON cadastro.agenciador USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 6044 (class 1259 OID 8862436)
-- Name: ix_agenciador_pessoa; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_agenciador_pessoa ON cadastro.agenciador USING btree (pessoa_id);


--
-- TOC entry 5630 (class 1259 OID 8860403)
-- Name: ix_cliente_dependente_cliente; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_dependente_cliente ON cadastro.cliente_dependente USING btree (cliente_id);


--
-- TOC entry 5625 (class 1259 OID 8860382)
-- Name: ix_cliente_pessoa; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_pessoa ON cadastro.cliente USING btree (pessoa_id);


--
-- TOC entry 5626 (class 1259 OID 8860383)
-- Name: ix_cliente_status; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_status ON cadastro.cliente USING btree (status_id);


--
-- TOC entry 5633 (class 1259 OID 8860445)
-- Name: ix_cliente_vinculo_cliente; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_vinculo_cliente ON cadastro.cliente_vinculo USING btree (cliente_id);


--
-- TOC entry 5634 (class 1259 OID 8860447)
-- Name: ix_cliente_vinculo_estipulante; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_vinculo_estipulante ON cadastro.cliente_vinculo USING btree (estipulante_id);


--
-- TOC entry 5635 (class 1259 OID 8860446)
-- Name: ix_cliente_vinculo_pessoa; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_vinculo_pessoa ON cadastro.cliente_vinculo USING btree (pessoa_id);


--
-- TOC entry 5636 (class 1259 OID 8860448)
-- Name: ix_cliente_vinculo_pessoa_estip_matricula; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_cliente_vinculo_pessoa_estip_matricula ON cadastro.cliente_vinculo USING btree (pessoa_id, estipulante_id, matricula_normalizada);


--
-- TOC entry 5715 (class 1259 OID 8862447)
-- Name: ix_corretora_logotipo_arquivo; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_corretora_logotipo_arquivo ON cadastro.corretora USING btree (logotipo_arquivo_id);


--
-- TOC entry 5716 (class 1259 OID 8860849)
-- Name: ix_corretora_nome_trgm; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_corretora_nome_trgm ON cadastro.corretora USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5684 (class 1259 OID 8860702)
-- Name: ix_estipulante_ativo; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_estipulante_ativo ON cadastro.estipulante USING btree (ativo);


--
-- TOC entry 5691 (class 1259 OID 8860725)
-- Name: ix_estipulante_config_reajuste; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_estipulante_config_reajuste ON cadastro.estipulante_configuracao USING btree (data_limite_reajuste);


--
-- TOC entry 5685 (class 1259 OID 8860700)
-- Name: ix_estipulante_grupo; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_estipulante_grupo ON cadastro.estipulante USING btree (grupo_id);


--
-- TOC entry 5686 (class 1259 OID 8860699)
-- Name: ix_estipulante_nome_trgm; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_estipulante_nome_trgm ON cadastro.estipulante USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5687 (class 1259 OID 8860701)
-- Name: ix_estipulante_seguradora; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_estipulante_seguradora ON cadastro.estipulante USING btree (seguradora_id);


--
-- TOC entry 5672 (class 1259 OID 8860643)
-- Name: ix_seguradora_nome_trgm; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_seguradora_nome_trgm ON cadastro.seguradora USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5718 (class 1259 OID 8860884)
-- Name: ix_subestipulante_estipulante; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_subestipulante_estipulante ON cadastro.subestipulante USING btree (estipulante_id);


--
-- TOC entry 5719 (class 1259 OID 8860885)
-- Name: ix_subestipulante_nome_trgm; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE INDEX ix_subestipulante_nome_trgm ON cadastro.subestipulante USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 6045 (class 1259 OID 8862435)
-- Name: ux_agenciador_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_agenciador_legado ON cadastro.agenciador USING btree (legado_id);


--
-- TOC entry 5627 (class 1259 OID 8860384)
-- Name: ux_cliente_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_cliente_legado ON cadastro.cliente USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5637 (class 1259 OID 8860449)
-- Name: ux_cliente_vinculo_legado_cliente; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_cliente_vinculo_legado_cliente ON cadastro.cliente_vinculo USING btree (legado_cliente_id) WHERE (legado_cliente_id IS NOT NULL);


--
-- TOC entry 5717 (class 1259 OID 8860848)
-- Name: ux_corretora_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_corretora_legado ON cadastro.corretora USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5692 (class 1259 OID 8860724)
-- Name: ux_estipulante_config_estipulante; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_estipulante_config_estipulante ON cadastro.estipulante_configuracao USING btree (estipulante_id);


--
-- TOC entry 5688 (class 1259 OID 8860698)
-- Name: ux_estipulante_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_estipulante_legado ON cadastro.estipulante USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5616 (class 1259 OID 8860330)
-- Name: ux_grupo_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_grupo_legado ON cadastro.grupo USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5622 (class 1259 OID 8860358)
-- Name: ux_lotacao_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_lotacao_legado ON cadastro.lotacao USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5675 (class 1259 OID 8860642)
-- Name: ux_seguradora_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_seguradora_legado ON cadastro.seguradora USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5722 (class 1259 OID 8860883)
-- Name: ux_subestipulante_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_subestipulante_legado ON cadastro.subestipulante USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5619 (class 1259 OID 8860344)
-- Name: ux_subgrupo_legado; Type: INDEX; Schema: cadastro; Owner: postgres
--

CREATE UNIQUE INDEX ux_subgrupo_legado ON cadastro.subgrupo USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 6048 (class 1259 OID 8862464)
-- Name: ix_agenciador_comissao_config_agenciador; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciador_comissao_config_agenciador ON comissao.agenciador_comissao_config USING btree (agenciador_id);


--
-- TOC entry 6049 (class 1259 OID 8862465)
-- Name: ix_agenciador_comissao_config_vigencia; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciador_comissao_config_vigencia ON comissao.agenciador_comissao_config USING btree (inicio_vigencia, fim_vigencia);


--
-- TOC entry 6075 (class 1259 OID 8862619)
-- Name: ix_agenciamento_corretora_lancamento_corretora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_lancamento_corretora ON comissao.agenciamento_corretora_lancamento USING btree (corretora_id);


--
-- TOC entry 6076 (class 1259 OID 8862620)
-- Name: ix_agenciamento_corretora_lancamento_movimento; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_lancamento_movimento ON comissao.agenciamento_corretora_lancamento USING btree (movimento_tipo_id);


--
-- TOC entry 6077 (class 1259 OID 8862622)
-- Name: ix_agenciamento_corretora_lancamento_pagamento; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_lancamento_pagamento ON comissao.agenciamento_corretora_lancamento USING btree (data_pagamento) WHERE (data_pagamento IS NOT NULL);


--
-- TOC entry 6078 (class 1259 OID 8862618)
-- Name: ix_agenciamento_corretora_lancamento_proposta; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_lancamento_proposta ON comissao.agenciamento_corretora_lancamento USING btree (proposta_id);


--
-- TOC entry 6079 (class 1259 OID 8862621)
-- Name: ix_agenciamento_corretora_lancamento_vencimento; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_lancamento_vencimento ON comissao.agenciamento_corretora_lancamento USING btree (data_vencimento);


--
-- TOC entry 6053 (class 1259 OID 8862488)
-- Name: ix_corretora_agenciador_agenciador; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_corretora_agenciador_agenciador ON comissao.corretora_agenciador USING btree (agenciador_id);


--
-- TOC entry 6054 (class 1259 OID 8862487)
-- Name: ix_corretora_agenciador_corretora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_corretora_agenciador_corretora ON comissao.corretora_agenciador USING btree (corretora_id);


--
-- TOC entry 5700 (class 1259 OID 8860779)
-- Name: ix_estipulante_comissao_agenciador; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_estipulante_comissao_agenciador ON comissao.estipulante_comissao_config USING btree (agenciador_id);


--
-- TOC entry 6133 (class 1259 OID 8862833)
-- Name: ix_fatura_comissao_resumo_competencia; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_comissao_resumo_competencia ON comissao.fatura_comissao_resumo USING btree (competencia_int);


--
-- TOC entry 6134 (class 1259 OID 8862832)
-- Name: ix_fatura_comissao_resumo_estipulante; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_comissao_resumo_estipulante ON comissao.fatura_comissao_resumo USING btree (estipulante_id);


--
-- TOC entry 6112 (class 1259 OID 8862775)
-- Name: ix_fatura_integracao_competencia; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_competencia ON comissao.fatura_integracao USING btree (ano, mes);


--
-- TOC entry 6113 (class 1259 OID 8862774)
-- Name: ix_fatura_integracao_corretora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_corretora ON comissao.fatura_integracao USING btree (corretora_id);


--
-- TOC entry 6114 (class 1259 OID 8862772)
-- Name: ix_fatura_integracao_estipulante; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_estipulante ON comissao.fatura_integracao USING btree (estipulante_id);


--
-- TOC entry 6115 (class 1259 OID 8862773)
-- Name: ix_fatura_integracao_seguradora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_seguradora ON comissao.fatura_integracao USING btree (seguradora_id);


--
-- TOC entry 6116 (class 1259 OID 8862776)
-- Name: ix_fatura_integracao_tipo_situacao; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_tipo_situacao ON comissao.fatura_integracao USING btree (tipo, situacao_legado);


--
-- TOC entry 6117 (class 1259 OID 8862777)
-- Name: ix_fatura_integracao_vencimento; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_integracao_vencimento ON comissao.fatura_integracao USING btree (data_vencimento);


--
-- TOC entry 6121 (class 1259 OID 8862795)
-- Name: ix_fatura_vida_agenciamento_data_inclusao; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_agenciamento_data_inclusao ON comissao.fatura_vida_agenciamento USING btree (data_inclusao);


--
-- TOC entry 6122 (class 1259 OID 8862794)
-- Name: ix_fatura_vida_agenciamento_origem; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_agenciamento_origem ON comissao.fatura_vida_agenciamento USING btree (origem_legado);


--
-- TOC entry 6123 (class 1259 OID 8862793)
-- Name: ix_fatura_vida_agenciamento_proposta; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_agenciamento_proposta ON comissao.fatura_vida_agenciamento USING btree (proposta_id);


--
-- TOC entry 6127 (class 1259 OID 8862817)
-- Name: ix_fatura_vida_recebimento_data; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_recebimento_data ON comissao.fatura_vida_recebimento USING btree (data_pagamento);


--
-- TOC entry 6128 (class 1259 OID 8862816)
-- Name: ix_fatura_vida_recebimento_estipulante; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_recebimento_estipulante ON comissao.fatura_vida_recebimento USING btree (estipulante_id);


--
-- TOC entry 6129 (class 1259 OID 8862815)
-- Name: ix_fatura_vida_recebimento_fatura_vida; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_fatura_vida_recebimento_fatura_vida ON comissao.fatura_vida_recebimento USING btree (fatura_vida_agenciamento_id);


--
-- TOC entry 5873 (class 1259 OID 8861659)
-- Name: ix_lancamento_comissao_competencia; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_comissao_competencia ON comissao.lancamento_comissao USING btree (competencia_ano, competencia_mes);


--
-- TOC entry 5874 (class 1259 OID 8861656)
-- Name: ix_lancamento_comissao_movimento; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_comissao_movimento ON comissao.lancamento_comissao USING btree (proposta_movimento_id);


--
-- TOC entry 5875 (class 1259 OID 8861658)
-- Name: ix_lancamento_comissao_proposta; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_comissao_proposta ON comissao.lancamento_comissao USING btree (proposta_id);


--
-- TOC entry 5876 (class 1259 OID 8861657)
-- Name: ix_lancamento_comissao_titulo; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_comissao_titulo ON comissao.lancamento_comissao USING btree (titulo_id);


--
-- TOC entry 6136 (class 1259 OID 8862855)
-- Name: ix_lancamento_fatura_estipulante_competencia; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_fatura_estipulante_competencia ON comissao.lancamento_fatura_estipulante USING btree (competencia_int);


--
-- TOC entry 6137 (class 1259 OID 8862854)
-- Name: ix_lancamento_fatura_estipulante_corretora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_fatura_estipulante_corretora ON comissao.lancamento_fatura_estipulante USING btree (corretora_id);


--
-- TOC entry 6138 (class 1259 OID 8862853)
-- Name: ix_lancamento_fatura_estipulante_estipulante; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_lancamento_fatura_estipulante_estipulante ON comissao.lancamento_fatura_estipulante USING btree (estipulante_id);


--
-- TOC entry 5743 (class 1259 OID 8862499)
-- Name: ix_proposta_participante_agenciador; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_proposta_participante_agenciador ON comissao.proposta_participante USING btree (agenciador_id);


--
-- TOC entry 5744 (class 1259 OID 8862501)
-- Name: ix_proposta_participante_codigo_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_proposta_participante_codigo_legado ON comissao.proposta_participante USING btree (codigo_legado_participante);


--
-- TOC entry 5745 (class 1259 OID 8862500)
-- Name: ix_proposta_participante_corretora; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_proposta_participante_corretora ON comissao.proposta_participante USING btree (corretora_id);


--
-- TOC entry 5746 (class 1259 OID 8861045)
-- Name: ix_proposta_participante_proposta; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_proposta_participante_proposta ON comissao.proposta_participante USING btree (proposta_id);


--
-- TOC entry 5747 (class 1259 OID 8861046)
-- Name: ix_proposta_participante_tipo; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE INDEX ix_proposta_participante_tipo ON comissao.proposta_participante USING btree (participante_tipo);


--
-- TOC entry 6050 (class 1259 OID 8862590)
-- Name: ux_agenciador_comissao_config_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_agenciador_comissao_config_legado ON comissao.agenciador_comissao_config USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 6080 (class 1259 OID 8862617)
-- Name: ux_agenciamento_corretora_lancamento_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_agenciamento_corretora_lancamento_legado ON comissao.agenciamento_corretora_lancamento USING btree (legado_id);


--
-- TOC entry 6055 (class 1259 OID 8862486)
-- Name: ux_corretora_agenciador_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_corretora_agenciador_legado ON comissao.corretora_agenciador USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5701 (class 1259 OID 8860778)
-- Name: ux_estipulante_comissao_config; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_estipulante_comissao_config ON comissao.estipulante_comissao_config USING btree (estipulante_id);


--
-- TOC entry 6135 (class 1259 OID 8862831)
-- Name: ux_fatura_comissao_resumo_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_fatura_comissao_resumo_legado ON comissao.fatura_comissao_resumo USING btree (legado_id);


--
-- TOC entry 6118 (class 1259 OID 8862771)
-- Name: ux_fatura_integracao_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_fatura_integracao_legado ON comissao.fatura_integracao USING btree (legado_id);


--
-- TOC entry 6124 (class 1259 OID 8862792)
-- Name: ux_fatura_vida_agenciamento_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_fatura_vida_agenciamento_legado ON comissao.fatura_vida_agenciamento USING btree (origem_legado, legado_id);


--
-- TOC entry 6130 (class 1259 OID 8862814)
-- Name: ux_fatura_vida_recebimento_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_fatura_vida_recebimento_legado ON comissao.fatura_vida_recebimento USING btree (legado_id);


--
-- TOC entry 6141 (class 1259 OID 8862852)
-- Name: ux_lancamento_fatura_estipulante_legado; Type: INDEX; Schema: comissao; Owner: postgres
--

CREATE UNIQUE INDEX ux_lancamento_fatura_estipulante_legado ON comissao.lancamento_fatura_estipulante USING btree (legado_id);


--
-- TOC entry 5647 (class 1259 OID 8860514)
-- Name: ix_corsan_cliente_cliente; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE INDEX ix_corsan_cliente_cliente ON convenio.corsan_cliente USING btree (cliente_id);


--
-- TOC entry 5741 (class 1259 OID 8861030)
-- Name: ix_corsan_proposta_cliente; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE INDEX ix_corsan_proposta_cliente ON convenio.corsan_proposta USING btree (cliente_id);


--
-- TOC entry 5641 (class 1259 OID 8860488)
-- Name: ix_siape_cliente_cliente; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE INDEX ix_siape_cliente_cliente ON convenio.siape_cliente USING btree (cliente_id);


--
-- TOC entry 5642 (class 1259 OID 8860489)
-- Name: ix_siape_cliente_siape; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE INDEX ix_siape_cliente_siape ON convenio.siape_cliente USING btree (siape);


--
-- TOC entry 5742 (class 1259 OID 8861029)
-- Name: ux_corsan_proposta; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE UNIQUE INDEX ux_corsan_proposta ON convenio.corsan_proposta USING btree (proposta_id);


--
-- TOC entry 5640 (class 1259 OID 8860458)
-- Name: ux_siape_orgao_legado; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE UNIQUE INDEX ux_siape_orgao_legado ON convenio.siape_orgao USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5681 (class 1259 OID 8860663)
-- Name: ux_siape_parametro_legado; Type: INDEX; Schema: convenio; Owner: postgres
--

CREATE UNIQUE INDEX ux_siape_parametro_legado ON convenio.siape_parametro USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5608 (class 1259 OID 8860311)
-- Name: ix_banco_codigo; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_banco_codigo ON core.banco USING btree (codigo);


--
-- TOC entry 5601 (class 1259 OID 8860276)
-- Name: ix_cidade_nome_trgm; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_cidade_nome_trgm ON core.cidade USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5591 (class 1259 OID 8860252)
-- Name: ix_pessoa_contato_pessoa; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_contato_pessoa ON core.pessoa_contato USING btree (pessoa_id);


--
-- TOC entry 5592 (class 1259 OID 8860253)
-- Name: ix_pessoa_contato_tipo; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_contato_tipo ON core.pessoa_contato USING btree (tipo_contato);


--
-- TOC entry 5582 (class 1259 OID 8860219)
-- Name: ix_pessoa_documento_limpo; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_documento_limpo ON core.pessoa USING btree (documento_principal_limpo);


--
-- TOC entry 5587 (class 1259 OID 8860236)
-- Name: ix_pessoa_documento_numero; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_documento_numero ON core.pessoa_documento USING btree (numero_limpo);


--
-- TOC entry 5588 (class 1259 OID 8860235)
-- Name: ix_pessoa_documento_pessoa; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_documento_pessoa ON core.pessoa_documento USING btree (pessoa_id);


--
-- TOC entry 5583 (class 1259 OID 8860220)
-- Name: ix_pessoa_documento_valido; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_documento_valido ON core.pessoa USING btree (documento_valido);


--
-- TOC entry 5603 (class 1259 OID 8860299)
-- Name: ix_pessoa_endereco_pessoa; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_endereco_pessoa ON core.pessoa_endereco USING btree (pessoa_id);


--
-- TOC entry 5584 (class 1259 OID 8860218)
-- Name: ix_pessoa_nome_trgm; Type: INDEX; Schema: core; Owner: postgres
--

CREATE INDEX ix_pessoa_nome_trgm ON core.pessoa USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5609 (class 1259 OID 8860310)
-- Name: ux_banco_legado; Type: INDEX; Schema: core; Owner: postgres
--

CREATE UNIQUE INDEX ux_banco_legado ON core.banco USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5602 (class 1259 OID 8860277)
-- Name: ux_cidade_legado; Type: INDEX; Schema: core; Owner: postgres
--

CREATE UNIQUE INDEX ux_cidade_legado ON core.cidade USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5981 (class 1259 OID 8862136)
-- Name: ix_arquivo_acesso_log_arquivo; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_acesso_log_arquivo ON documento.arquivo_acesso_log USING btree (arquivo_id);


--
-- TOC entry 5982 (class 1259 OID 8862137)
-- Name: ix_arquivo_acesso_log_data; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_acesso_log_data ON documento.arquivo_acesso_log USING btree (created_at);


--
-- TOC entry 5958 (class 1259 OID 8862070)
-- Name: ix_arquivo_data_documento; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_data_documento ON documento.arquivo USING btree (data_documento);


--
-- TOC entry 5959 (class 1259 OID 8862068)
-- Name: ix_arquivo_extensao; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_extensao ON documento.arquivo USING btree (extensao);


--
-- TOC entry 5960 (class 1259 OID 8862183)
-- Name: ix_arquivo_extensao_normalizada; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_extensao_normalizada ON documento.arquivo USING btree (extensao_normalizada);


--
-- TOC entry 5961 (class 1259 OID 8862067)
-- Name: ix_arquivo_hash; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_hash ON documento.arquivo USING btree (hash_sha256);


--
-- TOC entry 5962 (class 1259 OID 8862184)
-- Name: ix_arquivo_migracao_status; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_migracao_status ON documento.arquivo USING btree (migracao_status);


--
-- TOC entry 5963 (class 1259 OID 8862065)
-- Name: ix_arquivo_public_id; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_public_id ON documento.arquivo USING btree (public_id);


--
-- TOC entry 5964 (class 1259 OID 8862069)
-- Name: ix_arquivo_status; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_status ON documento.arquivo USING btree (status);


--
-- TOC entry 5965 (class 1259 OID 8862066)
-- Name: ix_arquivo_storage_key; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_storage_key ON documento.arquivo USING btree (storage_key);


--
-- TOC entry 5978 (class 1259 OID 8862120)
-- Name: ix_arquivo_versao_arquivo; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_versao_arquivo ON documento.arquivo_versao USING btree (arquivo_id);


--
-- TOC entry 5969 (class 1259 OID 8862093)
-- Name: ix_arquivo_vinculo_arquivo; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_vinculo_arquivo ON documento.arquivo_vinculo USING btree (arquivo_id);


--
-- TOC entry 5970 (class 1259 OID 8862094)
-- Name: ix_arquivo_vinculo_entidade; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_vinculo_entidade ON documento.arquivo_vinculo USING btree (entidade_tipo, entidade_id);


--
-- TOC entry 5971 (class 1259 OID 8862096)
-- Name: ix_arquivo_vinculo_legado; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_vinculo_legado ON documento.arquivo_vinculo USING btree (entidade_tipo, entidade_legado_id);


--
-- TOC entry 5972 (class 1259 OID 8862186)
-- Name: ix_arquivo_vinculo_legado_chaves; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_vinculo_legado_chaves ON documento.arquivo_vinculo USING btree (entidade_legado_tipo, entidade_legado_chave_1, entidade_legado_chave_2);


--
-- TOC entry 5973 (class 1259 OID 8862095)
-- Name: ix_arquivo_vinculo_tipo_anexo; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_arquivo_vinculo_tipo_anexo ON documento.arquivo_vinculo USING btree (tipo_anexo_id);


--
-- TOC entry 5952 (class 1259 OID 8862044)
-- Name: ix_tipo_anexo_nome; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE INDEX ix_tipo_anexo_nome ON documento.tipo_anexo USING btree (nome);


--
-- TOC entry 5966 (class 1259 OID 8862064)
-- Name: ux_arquivo_legado; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE UNIQUE INDEX ux_arquivo_legado ON documento.arquivo USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5955 (class 1259 OID 8862043)
-- Name: ux_tipo_anexo_codigo; Type: INDEX; Schema: documento; Owner: postgres
--

CREATE UNIQUE INDEX ux_tipo_anexo_codigo ON documento.tipo_anexo USING btree (codigo) WHERE (codigo IS NOT NULL);


--
-- TOC entry 6107 (class 1259 OID 8862743)
-- Name: ix_cobranca_acompanhamento_cliente; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_cobranca_acompanhamento_cliente ON financeiro.cobranca_acompanhamento USING btree (cliente_id);


--
-- TOC entry 6108 (class 1259 OID 8862744)
-- Name: ix_cobranca_acompanhamento_data; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_cobranca_acompanhamento_data ON financeiro.cobranca_acompanhamento USING btree (data_acompanhamento);


--
-- TOC entry 5658 (class 1259 OID 8860580)
-- Name: ix_conta_cobranca_agrupamento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_conta_cobranca_agrupamento ON financeiro.conta_cobranca USING btree (identificador_agrupamento);


--
-- TOC entry 5659 (class 1259 OID 8860579)
-- Name: ix_conta_cobranca_pessoa; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_conta_cobranca_pessoa ON financeiro.conta_cobranca USING btree (pessoa_id);


--
-- TOC entry 5660 (class 1259 OID 8860578)
-- Name: ix_conta_cobranca_vinculo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_conta_cobranca_vinculo ON financeiro.conta_cobranca USING btree (cliente_vinculo_id);


--
-- TOC entry 5654 (class 1259 OID 8862657)
-- Name: ix_convenio_cobranca_banco; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_convenio_cobranca_banco ON financeiro.convenio_cobranca USING btree (banco_id);


--
-- TOC entry 5695 (class 1259 OID 8860762)
-- Name: ix_estipulante_faturamento_convenio; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_estipulante_faturamento_convenio ON financeiro.estipulante_faturamento_config USING btree (convenio_cobranca_id);


--
-- TOC entry 5696 (class 1259 OID 8860761)
-- Name: ix_estipulante_faturamento_forma; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_estipulante_faturamento_forma ON financeiro.estipulante_faturamento_config USING btree (forma_pagamento_id);


--
-- TOC entry 6092 (class 1259 OID 8862689)
-- Name: ix_forma_retorno_estipulante_estipulante; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_forma_retorno_estipulante_estipulante ON financeiro.forma_retorno_estipulante USING btree (estipulante_id);


--
-- TOC entry 6093 (class 1259 OID 8862688)
-- Name: ix_forma_retorno_estipulante_forma; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_forma_retorno_estipulante_forma ON financeiro.forma_retorno_estipulante USING btree (forma_retorno_id);


--
-- TOC entry 6097 (class 1259 OID 8862699)
-- Name: ix_identificador_remessa_api_datahora; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_identificador_remessa_api_datahora ON financeiro.identificador_remessa_api USING btree (datahora);


--
-- TOC entry 6099 (class 1259 OID 8862721)
-- Name: ix_movimento_cobranca_log_data; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_movimento_cobranca_log_data ON financeiro.movimento_cobranca_log USING btree (data_movimento);


--
-- TOC entry 6100 (class 1259 OID 8862720)
-- Name: ix_movimento_cobranca_log_legado_movimento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_movimento_cobranca_log_legado_movimento ON financeiro.movimento_cobranca_log USING btree (legado_movimento_proposta_id);


--
-- TOC entry 6101 (class 1259 OID 8862718)
-- Name: ix_movimento_cobranca_log_movimento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_movimento_cobranca_log_movimento ON financeiro.movimento_cobranca_log USING btree (proposta_movimento_id);


--
-- TOC entry 6102 (class 1259 OID 8862719)
-- Name: ix_movimento_cobranca_log_titulo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_movimento_cobranca_log_titulo ON financeiro.movimento_cobranca_log USING btree (titulo_id);


--
-- TOC entry 5862 (class 1259 OID 8861585)
-- Name: ix_retorno_bancario_codigo_codigo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_retorno_bancario_codigo_codigo ON financeiro.retorno_bancario_codigo USING btree (codigo);


--
-- TOC entry 5863 (class 1259 OID 8861586)
-- Name: ix_retorno_bancario_codigo_tipo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_retorno_bancario_codigo_tipo ON financeiro.retorno_bancario_codigo USING btree (tipo);


--
-- TOC entry 5847 (class 1259 OID 8861540)
-- Name: ix_titulo_cliente; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_cliente ON financeiro.titulo USING btree (cliente_id);


--
-- TOC entry 5848 (class 1259 OID 8861544)
-- Name: ix_titulo_competencia; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_competencia ON financeiro.titulo USING btree (competencia_ano, competencia_mes);


--
-- TOC entry 5849 (class 1259 OID 8861545)
-- Name: ix_titulo_competencia_int; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_competencia_int ON financeiro.titulo USING btree (competencia_int);


--
-- TOC entry 5850 (class 1259 OID 8861542)
-- Name: ix_titulo_estipulante; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_estipulante ON financeiro.titulo USING btree (estipulante_id);


--
-- TOC entry 5851 (class 1259 OID 8861546)
-- Name: ix_titulo_pagamento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_pagamento ON financeiro.titulo USING btree (data_pagamento) WHERE (data_pagamento IS NOT NULL);


--
-- TOC entry 5858 (class 1259 OID 8861570)
-- Name: ix_titulo_pagamento_data; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_pagamento_data ON financeiro.titulo_pagamento USING btree (data_pagamento);


--
-- TOC entry 5859 (class 1259 OID 8861569)
-- Name: ix_titulo_pagamento_titulo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_pagamento_titulo ON financeiro.titulo_pagamento USING btree (titulo_id);


--
-- TOC entry 5852 (class 1259 OID 8861539)
-- Name: ix_titulo_proposta; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_proposta ON financeiro.titulo USING btree (proposta_id);


--
-- TOC entry 5868 (class 1259 OID 8861614)
-- Name: ix_titulo_retorno_codigo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_retorno_codigo ON financeiro.titulo_retorno_bancario USING btree (retorno_codigo_id);


--
-- TOC entry 5869 (class 1259 OID 8861613)
-- Name: ix_titulo_retorno_movimento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_retorno_movimento ON financeiro.titulo_retorno_bancario USING btree (proposta_movimento_id);


--
-- TOC entry 5870 (class 1259 OID 8861612)
-- Name: ix_titulo_retorno_titulo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_retorno_titulo ON financeiro.titulo_retorno_bancario USING btree (titulo_id);


--
-- TOC entry 5853 (class 1259 OID 8861543)
-- Name: ix_titulo_status; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_status ON financeiro.titulo USING btree (status_id);


--
-- TOC entry 5854 (class 1259 OID 8861541)
-- Name: ix_titulo_vinculo; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE INDEX ix_titulo_vinculo ON financeiro.titulo USING btree (cliente_vinculo_id);


--
-- TOC entry 6109 (class 1259 OID 8862742)
-- Name: ux_cobranca_acompanhamento_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_cobranca_acompanhamento_legado ON financeiro.cobranca_acompanhamento USING btree (legado_id);


--
-- TOC entry 5655 (class 1259 OID 8860542)
-- Name: ux_convenio_cobranca_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_convenio_cobranca_legado ON financeiro.convenio_cobranca USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5697 (class 1259 OID 8860760)
-- Name: ux_estipulante_faturamento_config; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_estipulante_faturamento_config ON financeiro.estipulante_faturamento_config USING btree (estipulante_id);


--
-- TOC entry 5678 (class 1259 OID 8860653)
-- Name: ux_forma_pagamento_estipulante_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_forma_pagamento_estipulante_legado ON financeiro.forma_pagamento_estipulante USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 6094 (class 1259 OID 8862687)
-- Name: ux_forma_retorno_estipulante_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_forma_retorno_estipulante_legado ON financeiro.forma_retorno_estipulante USING btree (legado_id);


--
-- TOC entry 6089 (class 1259 OID 8862668)
-- Name: ux_forma_retorno_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_forma_retorno_legado ON financeiro.forma_retorno USING btree (legado_id);


--
-- TOC entry 6098 (class 1259 OID 8862698)
-- Name: ux_identificador_remessa_api_legado; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_identificador_remessa_api_legado ON financeiro.identificador_remessa_api USING btree (legado_id);


--
-- TOC entry 5857 (class 1259 OID 8861538)
-- Name: ux_titulo_legado_movimento; Type: INDEX; Schema: financeiro; Owner: postgres
--

CREATE UNIQUE INDEX ux_titulo_legado_movimento ON financeiro.titulo USING btree (legado_movimento_proposta_id) WHERE (legado_movimento_proposta_id IS NOT NULL);


--
-- TOC entry 5702 (class 1259 OID 8860794)
-- Name: ix_referencia_externa_entidade; Type: INDEX; Schema: integracao; Owner: postgres
--

CREATE INDEX ix_referencia_externa_entidade ON integracao.referencia_externa USING btree (entidade_tipo, entidade_id);


--
-- TOC entry 6060 (class 1259 OID 8862530)
-- Name: ix_agenciador_map_agenciador; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_agenciador_map_agenciador ON legado.agenciador_migration_map USING btree (agenciador_id);


--
-- TOC entry 6061 (class 1259 OID 8862532)
-- Name: ix_agenciador_map_cpf; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_agenciador_map_cpf ON legado.agenciador_migration_map USING btree (cpf_limpo);


--
-- TOC entry 6062 (class 1259 OID 8862531)
-- Name: ix_agenciador_map_pessoa; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_agenciador_map_pessoa ON legado.agenciador_migration_map USING btree (pessoa_id);


--
-- TOC entry 6085 (class 1259 OID 8862656)
-- Name: ix_agenciamento_corretora_map_corretora; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_map_corretora ON legado.agenciamento_corretora_lancamento_migration_map USING btree (corretora_id);


--
-- TOC entry 6086 (class 1259 OID 8862655)
-- Name: ix_agenciamento_corretora_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_agenciamento_corretora_map_proposta ON legado.agenciamento_corretora_lancamento_migration_map USING btree (proposta_id);


--
-- TOC entry 5669 (class 1259 OID 8860620)
-- Name: ix_cliente_migration_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_cliente_migration_map_cliente ON legado.cliente_migration_map USING btree (cliente_id);


--
-- TOC entry 5670 (class 1259 OID 8860621)
-- Name: ix_cliente_migration_map_cpf; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_cliente_migration_map_cpf ON legado.cliente_migration_map USING btree (cpf_limpo);


--
-- TOC entry 5671 (class 1259 OID 8860619)
-- Name: ix_cliente_migration_map_pessoa; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_cliente_migration_map_pessoa ON legado.cliente_migration_map USING btree (pessoa_id);


--
-- TOC entry 6067 (class 1259 OID 8862555)
-- Name: ix_corretora_map_corretora; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_corretora_map_corretora ON legado.corretora_migration_map USING btree (corretora_id);


--
-- TOC entry 5987 (class 1259 OID 8862175)
-- Name: ix_documento_anexo_map_arquivo; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_documento_anexo_map_arquivo ON legado.documento_anexo_migration_map USING btree (arquivo_id);


--
-- TOC entry 5988 (class 1259 OID 8862176)
-- Name: ix_documento_anexo_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_documento_anexo_map_cliente ON legado.documento_anexo_migration_map USING btree (cliente_id);


--
-- TOC entry 5989 (class 1259 OID 8862179)
-- Name: ix_documento_anexo_map_estipulante; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_documento_anexo_map_estipulante ON legado.documento_anexo_migration_map USING btree (estipulante_id);


--
-- TOC entry 5990 (class 1259 OID 8862177)
-- Name: ix_documento_anexo_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_documento_anexo_map_proposta ON legado.documento_anexo_migration_map USING btree (proposta_id);


--
-- TOC entry 5991 (class 1259 OID 8862178)
-- Name: ix_documento_anexo_map_sinistro; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_documento_anexo_map_sinistro ON legado.documento_anexo_migration_map USING btree (sinistro_id);


--
-- TOC entry 5711 (class 1259 OID 8860819)
-- Name: ix_estipulante_migration_map_cnpj; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_estipulante_migration_map_cnpj ON legado.estipulante_migration_map USING btree (cnpj_limpo);


--
-- TOC entry 5712 (class 1259 OID 8860818)
-- Name: ix_estipulante_migration_map_pessoa; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_estipulante_migration_map_pessoa ON legado.estipulante_migration_map USING btree (pessoa_id);


--
-- TOC entry 5879 (class 1259 OID 8861730)
-- Name: ix_movimento_proposta_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_movimento_proposta_map_cliente ON legado.movimento_proposta_migration_map USING btree (cliente_id);


--
-- TOC entry 5880 (class 1259 OID 8861731)
-- Name: ix_movimento_proposta_map_estipulante; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_movimento_proposta_map_estipulante ON legado.movimento_proposta_migration_map USING btree (estipulante_id);


--
-- TOC entry 5881 (class 1259 OID 8861727)
-- Name: ix_movimento_proposta_map_movimento; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_movimento_proposta_map_movimento ON legado.movimento_proposta_migration_map USING btree (proposta_movimento_id);


--
-- TOC entry 5882 (class 1259 OID 8861729)
-- Name: ix_movimento_proposta_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_movimento_proposta_map_proposta ON legado.movimento_proposta_migration_map USING btree (proposta_id);


--
-- TOC entry 5883 (class 1259 OID 8861728)
-- Name: ix_movimento_proposta_map_titulo; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_movimento_proposta_map_titulo ON legado.movimento_proposta_migration_map USING btree (titulo_id);


--
-- TOC entry 5896 (class 1259 OID 8861790)
-- Name: ix_proposta_beneficiario_map_cpf; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_map_cpf ON legado.proposta_beneficiario_migration_map USING btree (cpf_limpo);


--
-- TOC entry 5897 (class 1259 OID 8861789)
-- Name: ix_proposta_beneficiario_map_pessoa; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_map_pessoa ON legado.proposta_beneficiario_migration_map USING btree (pessoa_id);


--
-- TOC entry 5898 (class 1259 OID 8861788)
-- Name: ix_proposta_beneficiario_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_map_proposta ON legado.proposta_beneficiario_migration_map USING btree (proposta_id);


--
-- TOC entry 5820 (class 1259 OID 8861393)
-- Name: ix_proposta_cobertura_migration_map_item; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_cobertura_migration_map_item ON legado.proposta_cobertura_migration_map USING btree (proposta_item_id);


--
-- TOC entry 5821 (class 1259 OID 8861392)
-- Name: ix_proposta_cobertura_migration_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_cobertura_migration_map_proposta ON legado.proposta_cobertura_migration_map USING btree (proposta_id);


--
-- TOC entry 5814 (class 1259 OID 8861358)
-- Name: ix_proposta_item_migration_map_item; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_item_migration_map_item ON legado.proposta_item_migration_map USING btree (proposta_item_id);


--
-- TOC entry 5815 (class 1259 OID 8861357)
-- Name: ix_proposta_item_migration_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_item_migration_map_proposta ON legado.proposta_item_migration_map USING btree (proposta_id);


--
-- TOC entry 5750 (class 1259 OID 8861095)
-- Name: ix_proposta_migration_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_migration_map_cliente ON legado.proposta_migration_map USING btree (cliente_id);


--
-- TOC entry 5751 (class 1259 OID 8861097)
-- Name: ix_proposta_migration_map_estipulante; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_migration_map_estipulante ON legado.proposta_migration_map USING btree (estipulante_id);


--
-- TOC entry 5752 (class 1259 OID 8861094)
-- Name: ix_proposta_migration_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_migration_map_proposta ON legado.proposta_migration_map USING btree (proposta_id);


--
-- TOC entry 5753 (class 1259 OID 8861096)
-- Name: ix_proposta_migration_map_vinculo; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_migration_map_vinculo ON legado.proposta_migration_map USING btree (cliente_vinculo_id);


--
-- TOC entry 6068 (class 1259 OID 8862587)
-- Name: ix_proposta_participante_map_agenciador; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_participante_map_agenciador ON legado.proposta_participante_migration_map USING btree (agenciador_id);


--
-- TOC entry 6069 (class 1259 OID 8862588)
-- Name: ix_proposta_participante_map_corretora; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_participante_map_corretora ON legado.proposta_participante_migration_map USING btree (corretora_id);


--
-- TOC entry 6070 (class 1259 OID 8862586)
-- Name: ix_proposta_participante_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_proposta_participante_map_proposta ON legado.proposta_participante_migration_map USING btree (proposta_id);


--
-- TOC entry 6024 (class 1259 OID 8862352)
-- Name: ix_protocolo_acompanhamento_map_lote; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_protocolo_acompanhamento_map_lote ON legado.protocolo_acompanhamento_migration_map USING btree (protocolo_lote_id);


--
-- TOC entry 6017 (class 1259 OID 8862329)
-- Name: ix_protocolo_item_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_protocolo_item_map_cliente ON legado.protocolo_item_migration_map USING btree (cliente_id);


--
-- TOC entry 6018 (class 1259 OID 8862327)
-- Name: ix_protocolo_item_map_item; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_protocolo_item_map_item ON legado.protocolo_item_migration_map USING btree (protocolo_item_id);


--
-- TOC entry 6019 (class 1259 OID 8862328)
-- Name: ix_protocolo_item_map_lote; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_protocolo_item_map_lote ON legado.protocolo_item_migration_map USING btree (protocolo_lote_id);


--
-- TOC entry 6012 (class 1259 OID 8862284)
-- Name: ix_protocolo_lote_map_lote; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_protocolo_lote_map_lote ON legado.protocolo_lote_migration_map USING btree (protocolo_lote_id);


--
-- TOC entry 5943 (class 1259 OID 8862012)
-- Name: ix_sinistro_acompanhamento_map_sinistro; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_sinistro_acompanhamento_map_sinistro ON legado.sinistro_acompanhamento_migration_map USING btree (sinistro_id);


--
-- TOC entry 5936 (class 1259 OID 8861989)
-- Name: ix_sinistro_migration_map_cliente; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_sinistro_migration_map_cliente ON legado.sinistro_migration_map USING btree (cliente_id);


--
-- TOC entry 5937 (class 1259 OID 8861988)
-- Name: ix_sinistro_migration_map_proposta; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_sinistro_migration_map_proposta ON legado.sinistro_migration_map USING btree (proposta_id);


--
-- TOC entry 5938 (class 1259 OID 8861987)
-- Name: ix_sinistro_migration_map_sinistro; Type: INDEX; Schema: legado; Owner: postgres
--

CREATE INDEX ix_sinistro_migration_map_sinistro ON legado.sinistro_migration_map USING btree (sinistro_id);


--
-- TOC entry 5778 (class 1259 OID 8861169)
-- Name: ix_cobertura_nome_trgm; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_cobertura_nome_trgm ON seguro.cobertura USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5826 (class 1259 OID 8861410)
-- Name: ix_movimento_tipo_classificacao; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_movimento_tipo_classificacao ON seguro.movimento_tipo USING btree (classificacao);


--
-- TOC entry 5827 (class 1259 OID 8861411)
-- Name: ix_movimento_tipo_financeiro; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_movimento_tipo_financeiro ON seguro.movimento_tipo USING btree (financeiro);


--
-- TOC entry 5762 (class 1259 OID 8861121)
-- Name: ix_plano_nome_trgm; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_plano_nome_trgm ON seguro.plano USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5763 (class 1259 OID 8861122)
-- Name: ix_plano_ramo; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_plano_ramo ON seguro.plano USING btree (ramo);


--
-- TOC entry 5770 (class 1259 OID 8861155)
-- Name: ix_produto_codigo_referencia; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_produto_codigo_referencia ON seguro.produto USING btree (codigo_referencia);


--
-- TOC entry 5771 (class 1259 OID 8861156)
-- Name: ix_produto_plano; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_produto_plano ON seguro.produto USING btree (plano_id);


--
-- TOC entry 5772 (class 1259 OID 8861157)
-- Name: ix_produto_tabela_preco; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_produto_tabela_preco ON seguro.produto USING btree (tabela_preco_id);


--
-- TOC entry 5888 (class 1259 OID 8861757)
-- Name: ix_proposta_beneficiario_cpf; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_cpf ON seguro.proposta_beneficiario USING btree (cpf_limpo);


--
-- TOC entry 5889 (class 1259 OID 8861758)
-- Name: ix_proposta_beneficiario_nome_trgm; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_nome_trgm ON seguro.proposta_beneficiario USING gin (nome public.gin_trgm_ops);


--
-- TOC entry 5890 (class 1259 OID 8861759)
-- Name: ix_proposta_beneficiario_parentesco; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_parentesco ON seguro.proposta_beneficiario USING btree (parentesco_normalizado);


--
-- TOC entry 5891 (class 1259 OID 8861756)
-- Name: ix_proposta_beneficiario_pessoa; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_pessoa ON seguro.proposta_beneficiario USING btree (pessoa_id);


--
-- TOC entry 5892 (class 1259 OID 8861755)
-- Name: ix_proposta_beneficiario_proposta; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_beneficiario_proposta ON seguro.proposta_beneficiario USING btree (proposta_id);


--
-- TOC entry 5723 (class 1259 OID 8860968)
-- Name: ix_proposta_cliente; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_cliente ON seguro.proposta USING btree (cliente_id);


--
-- TOC entry 5724 (class 1259 OID 8860969)
-- Name: ix_proposta_cliente_vinculo; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_cliente_vinculo ON seguro.proposta USING btree (cliente_vinculo_id);


--
-- TOC entry 5788 (class 1259 OID 8861239)
-- Name: ix_proposta_cobertura_cobertura; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_cobertura_cobertura ON seguro.proposta_cobertura USING btree (cobertura_id);


--
-- TOC entry 5789 (class 1259 OID 8861238)
-- Name: ix_proposta_cobertura_item; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_cobertura_item ON seguro.proposta_cobertura USING btree (proposta_item_id);


--
-- TOC entry 5790 (class 1259 OID 8861237)
-- Name: ix_proposta_cobertura_proposta; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_cobertura_proposta ON seguro.proposta_cobertura USING btree (proposta_id);


--
-- TOC entry 5725 (class 1259 OID 8860973)
-- Name: ix_proposta_data_inclusao; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_data_inclusao ON seguro.proposta USING btree (data_inclusao);


--
-- TOC entry 5726 (class 1259 OID 8860970)
-- Name: ix_proposta_estipulante; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_estipulante ON seguro.proposta USING btree (estipulante_id);


--
-- TOC entry 5727 (class 1259 OID 8860975)
-- Name: ix_proposta_estipulante_status; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_estipulante_status ON seguro.proposta USING btree (estipulante_id, status_id);


--
-- TOC entry 5735 (class 1259 OID 8860997)
-- Name: ix_proposta_historico_anterior; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_historico_anterior ON seguro.proposta_historico USING btree (proposta_anterior_id);


--
-- TOC entry 5736 (class 1259 OID 8860998)
-- Name: ix_proposta_historico_nova; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_historico_nova ON seguro.proposta_historico USING btree (proposta_nova_id);


--
-- TOC entry 5780 (class 1259 OID 8861208)
-- Name: ix_proposta_item_plano; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_item_plano ON seguro.proposta_item USING btree (plano_id);


--
-- TOC entry 5781 (class 1259 OID 8861207)
-- Name: ix_proposta_item_produto; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_item_produto ON seguro.proposta_item USING btree (produto_id);


--
-- TOC entry 5782 (class 1259 OID 8861206)
-- Name: ix_proposta_item_proposta; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_item_proposta ON seguro.proposta_item USING btree (proposta_id);


--
-- TOC entry 5783 (class 1259 OID 8861210)
-- Name: ix_proposta_item_tabela; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_item_tabela ON seguro.proposta_item USING btree (tabela_preco_id);


--
-- TOC entry 5784 (class 1259 OID 8861209)
-- Name: ix_proposta_item_tipo; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_item_tipo ON seguro.proposta_item USING btree (tipo_produto_id);


--
-- TOC entry 5831 (class 1259 OID 8861467)
-- Name: ix_proposta_movimento_classificacao; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_classificacao ON seguro.proposta_movimento USING btree (classificacao);


--
-- TOC entry 5832 (class 1259 OID 8861461)
-- Name: ix_proposta_movimento_cliente; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_cliente ON seguro.proposta_movimento USING btree (cliente_id);


--
-- TOC entry 5833 (class 1259 OID 8861465)
-- Name: ix_proposta_movimento_competencia; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_competencia ON seguro.proposta_movimento USING btree (ano, mes);


--
-- TOC entry 5834 (class 1259 OID 8861466)
-- Name: ix_proposta_movimento_competencia_int; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_competencia_int ON seguro.proposta_movimento USING btree (competencia_int);


--
-- TOC entry 5835 (class 1259 OID 8861468)
-- Name: ix_proposta_movimento_data_pagamento; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_data_pagamento ON seguro.proposta_movimento USING btree (data_pagamento) WHERE (data_pagamento IS NOT NULL);


--
-- TOC entry 5836 (class 1259 OID 8861463)
-- Name: ix_proposta_movimento_estipulante; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_estipulante ON seguro.proposta_movimento USING btree (estipulante_id);


--
-- TOC entry 5837 (class 1259 OID 8861460)
-- Name: ix_proposta_movimento_proposta; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_proposta ON seguro.proposta_movimento USING btree (proposta_id);


--
-- TOC entry 5838 (class 1259 OID 8861464)
-- Name: ix_proposta_movimento_tipo; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_tipo ON seguro.proposta_movimento USING btree (movimento_tipo_id);


--
-- TOC entry 5839 (class 1259 OID 8861462)
-- Name: ix_proposta_movimento_vinculo; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_movimento_vinculo ON seguro.proposta_movimento USING btree (cliente_vinculo_id);


--
-- TOC entry 5728 (class 1259 OID 8860972)
-- Name: ix_proposta_numero; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_numero ON seguro.proposta USING btree (numero);


--
-- TOC entry 5729 (class 1259 OID 8860967)
-- Name: ix_proposta_pessoa; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_pessoa ON seguro.proposta USING btree (pessoa_id);


--
-- TOC entry 5730 (class 1259 OID 8860971)
-- Name: ix_proposta_status; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_status ON seguro.proposta USING btree (status_id);


--
-- TOC entry 5731 (class 1259 OID 8860974)
-- Name: ix_proposta_vigente; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_proposta_vigente ON seguro.proposta USING btree (vigente);


--
-- TOC entry 5758 (class 1259 OID 8861109)
-- Name: ix_tipo_produto_nome; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE INDEX ix_tipo_produto_nome ON seguro.tipo_produto USING btree (nome);


--
-- TOC entry 5779 (class 1259 OID 8861168)
-- Name: ux_cobertura_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_cobertura_legado ON seguro.cobertura USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5830 (class 1259 OID 8861409)
-- Name: ux_movimento_tipo_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_movimento_tipo_legado ON seguro.movimento_tipo USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5766 (class 1259 OID 8861120)
-- Name: ux_plano_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_plano_legado ON seguro.plano USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5775 (class 1259 OID 8861154)
-- Name: ux_produto_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_produto_legado ON seguro.produto USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5895 (class 1259 OID 8861754)
-- Name: ux_proposta_beneficiario_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_proposta_beneficiario_legado ON seguro.proposta_beneficiario USING btree (legado_id);


--
-- TOC entry 5793 (class 1259 OID 8861236)
-- Name: ux_proposta_cobertura_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_proposta_cobertura_legado ON seguro.proposta_cobertura USING btree (legado_id);


--
-- TOC entry 5787 (class 1259 OID 8861205)
-- Name: ux_proposta_item_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_proposta_item_legado ON seguro.proposta_item USING btree (legado_id);


--
-- TOC entry 5734 (class 1259 OID 8860966)
-- Name: ux_proposta_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_proposta_legado ON seguro.proposta USING btree (legado_id);


--
-- TOC entry 5842 (class 1259 OID 8861459)
-- Name: ux_proposta_movimento_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_proposta_movimento_legado ON seguro.proposta_movimento USING btree (legado_id);


--
-- TOC entry 5769 (class 1259 OID 8861133)
-- Name: ux_tabela_preco_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_tabela_preco_legado ON seguro.tabela_preco USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5761 (class 1259 OID 8861108)
-- Name: ux_tipo_produto_legado; Type: INDEX; Schema: seguro; Owner: postgres
--

CREATE UNIQUE INDEX ux_tipo_produto_legado ON seguro.tipo_produto USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5920 (class 1259 OID 8861876)
-- Name: ix_sinistro_acompanhamento_data; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_acompanhamento_data ON sinistro.acompanhamento USING btree (data_acompanhamento);


--
-- TOC entry 5921 (class 1259 OID 8861875)
-- Name: ix_sinistro_acompanhamento_sinistro; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_acompanhamento_sinistro ON sinistro.acompanhamento USING btree (sinistro_id);


--
-- TOC entry 5923 (class 1259 OID 8862013)
-- Name: ix_sinistro_beneficiario_cpf; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_beneficiario_cpf ON sinistro.sinistro_beneficiario USING btree (cpf_limpo);


--
-- TOC entry 5924 (class 1259 OID 8861910)
-- Name: ix_sinistro_beneficiario_proposta_beneficiario; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_beneficiario_proposta_beneficiario ON sinistro.sinistro_beneficiario USING btree (proposta_beneficiario_id);


--
-- TOC entry 5925 (class 1259 OID 8861909)
-- Name: ix_sinistro_beneficiario_sinistro; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_beneficiario_sinistro ON sinistro.sinistro_beneficiario USING btree (sinistro_id);


--
-- TOC entry 5907 (class 1259 OID 8861852)
-- Name: ix_sinistro_cliente; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_cliente ON sinistro.sinistro USING btree (cliente_id);


--
-- TOC entry 5929 (class 1259 OID 8861944)
-- Name: ix_sinistro_cobertura_cobertura; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_cobertura_cobertura ON sinistro.sinistro_cobertura USING btree (cobertura_id);


--
-- TOC entry 5930 (class 1259 OID 8861943)
-- Name: ix_sinistro_cobertura_proposta_cobertura; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_cobertura_proposta_cobertura ON sinistro.sinistro_cobertura USING btree (proposta_cobertura_id);


--
-- TOC entry 5931 (class 1259 OID 8861942)
-- Name: ix_sinistro_cobertura_sinistro; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_cobertura_sinistro ON sinistro.sinistro_cobertura USING btree (sinistro_id);


--
-- TOC entry 5908 (class 1259 OID 8861858)
-- Name: ix_sinistro_cpf_sinistrado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_cpf_sinistrado ON sinistro.sinistro USING btree (cpf_sinistrado_limpo);


--
-- TOC entry 5909 (class 1259 OID 8861857)
-- Name: ix_sinistro_data_aviso; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_data_aviso ON sinistro.sinistro USING btree (data_aviso);


--
-- TOC entry 5910 (class 1259 OID 8861856)
-- Name: ix_sinistro_data_ocorrencia; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_data_ocorrencia ON sinistro.sinistro USING btree (data_ocorrencia);


--
-- TOC entry 5911 (class 1259 OID 8861854)
-- Name: ix_sinistro_estipulante; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_estipulante ON sinistro.sinistro USING btree (estipulante_id);


--
-- TOC entry 5912 (class 1259 OID 8861851)
-- Name: ix_sinistro_proposta; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_proposta ON sinistro.sinistro USING btree (proposta_id);


--
-- TOC entry 5913 (class 1259 OID 8861855)
-- Name: ix_sinistro_status; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_status ON sinistro.sinistro USING btree (status_id);


--
-- TOC entry 5914 (class 1259 OID 8861853)
-- Name: ix_sinistro_vinculo; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE INDEX ix_sinistro_vinculo ON sinistro.sinistro USING btree (cliente_vinculo_id);


--
-- TOC entry 5922 (class 1259 OID 8861874)
-- Name: ux_sinistro_acompanhamento_legado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE UNIQUE INDEX ux_sinistro_acompanhamento_legado ON sinistro.acompanhamento USING btree (legado_id);


--
-- TOC entry 5928 (class 1259 OID 8861908)
-- Name: ux_sinistro_beneficiario_legado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE UNIQUE INDEX ux_sinistro_beneficiario_legado ON sinistro.sinistro_beneficiario USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5934 (class 1259 OID 8862014)
-- Name: ux_sinistro_cobertura_cobertura_legado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE UNIQUE INDEX ux_sinistro_cobertura_cobertura_legado ON sinistro.sinistro_cobertura USING btree (cobertura_sinistro_legado_id) WHERE (cobertura_sinistro_legado_id IS NOT NULL);


--
-- TOC entry 5935 (class 1259 OID 8861941)
-- Name: ux_sinistro_cobertura_legado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE UNIQUE INDEX ux_sinistro_cobertura_legado ON sinistro.sinistro_cobertura USING btree (legado_id) WHERE (legado_id IS NOT NULL);


--
-- TOC entry 5917 (class 1259 OID 8861850)
-- Name: ux_sinistro_legado; Type: INDEX; Schema: sinistro; Owner: postgres
--

CREATE UNIQUE INDEX ux_sinistro_legado ON sinistro.sinistro USING btree (legado_id);


--
-- TOC entry 6335 (class 2606 OID 8862259)
-- Name: protocolo_acompanhamento protocolo_acompanhamento_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_acompanhamento
    ADD CONSTRAINT protocolo_acompanhamento_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6330 (class 2606 OID 8862227)
-- Name: protocolo_item protocolo_item_cliente_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6331 (class 2606 OID 8862232)
-- Name: protocolo_item protocolo_item_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6332 (class 2606 OID 8862237)
-- Name: protocolo_item protocolo_item_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6333 (class 2606 OID 8862222)
-- Name: protocolo_item protocolo_item_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6334 (class 2606 OID 8862217)
-- Name: protocolo_item protocolo_item_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_item
    ADD CONSTRAINT protocolo_item_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6345 (class 2606 OID 8862388)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_cliente_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6346 (class 2606 OID 8862393)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6347 (class 2606 OID 8862383)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6348 (class 2606 OID 8862378)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6349 (class 2606 OID 8862373)
-- Name: protocolo_relatorio_seguradora_item protocolo_relatorio_seguradora_item_relatorio_id_fkey; Type: FK CONSTRAINT; Schema: atendimento; Owner: postgres
--

ALTER TABLE ONLY atendimento.protocolo_relatorio_seguradora_item
    ADD CONSTRAINT protocolo_relatorio_seguradora_item_relatorio_id_fkey FOREIGN KEY (relatorio_id) REFERENCES atendimento.protocolo_relatorio_seguradora(id);


--
-- TOC entry 6350 (class 2606 OID 8862425)
-- Name: agenciador agenciador_banco_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador
    ADD CONSTRAINT agenciador_banco_id_fkey FOREIGN KEY (banco_id) REFERENCES core.banco(id);


--
-- TOC entry 6351 (class 2606 OID 8862420)
-- Name: agenciador agenciador_cidade_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador
    ADD CONSTRAINT agenciador_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6352 (class 2606 OID 8862430)
-- Name: agenciador agenciador_coordenador_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador
    ADD CONSTRAINT agenciador_coordenador_id_fkey FOREIGN KEY (coordenador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6353 (class 2606 OID 8862415)
-- Name: agenciador agenciador_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.agenciador
    ADD CONSTRAINT agenciador_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6151 (class 2606 OID 8860393)
-- Name: cliente_dependente cliente_dependente_cliente_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_dependente
    ADD CONSTRAINT cliente_dependente_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6152 (class 2606 OID 8860398)
-- Name: cliente_dependente cliente_dependente_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_dependente
    ADD CONSTRAINT cliente_dependente_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6149 (class 2606 OID 8860372)
-- Name: cliente cliente_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente
    ADD CONSTRAINT cliente_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6150 (class 2606 OID 8860377)
-- Name: cliente cliente_status_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente
    ADD CONSTRAINT cliente_status_id_fkey FOREIGN KEY (status_id) REFERENCES cadastro.cliente_status(id);


--
-- TOC entry 6153 (class 2606 OID 8860440)
-- Name: cliente_vinculo cliente_vinculo_banco_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_banco_id_fkey FOREIGN KEY (banco_id) REFERENCES core.banco(id);


--
-- TOC entry 6154 (class 2606 OID 8860415)
-- Name: cliente_vinculo cliente_vinculo_cliente_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6155 (class 2606 OID 8860425)
-- Name: cliente_vinculo cliente_vinculo_grupo_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_grupo_id_fkey FOREIGN KEY (grupo_id) REFERENCES cadastro.grupo(id);


--
-- TOC entry 6156 (class 2606 OID 8860435)
-- Name: cliente_vinculo cliente_vinculo_lotacao_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_lotacao_id_fkey FOREIGN KEY (lotacao_id) REFERENCES cadastro.lotacao(id);


--
-- TOC entry 6157 (class 2606 OID 8860420)
-- Name: cliente_vinculo cliente_vinculo_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6158 (class 2606 OID 8860430)
-- Name: cliente_vinculo cliente_vinculo_subgrupo_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT cliente_vinculo_subgrupo_id_fkey FOREIGN KEY (subgrupo_id) REFERENCES cadastro.subgrupo(id);


--
-- TOC entry 6191 (class 2606 OID 8860843)
-- Name: corretora corretora_cidade_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.corretora
    ADD CONSTRAINT corretora_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6192 (class 2606 OID 8862442)
-- Name: corretora corretora_logotipo_arquivo_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.corretora
    ADD CONSTRAINT corretora_logotipo_arquivo_id_fkey FOREIGN KEY (logotipo_arquivo_id) REFERENCES documento.arquivo(id);


--
-- TOC entry 6193 (class 2606 OID 8860838)
-- Name: corretora corretora_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.corretora
    ADD CONSTRAINT corretora_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6177 (class 2606 OID 8860683)
-- Name: estipulante estipulante_cidade_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante
    ADD CONSTRAINT estipulante_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6181 (class 2606 OID 8860719)
-- Name: estipulante_configuracao estipulante_configuracao_cancela_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante_configuracao
    ADD CONSTRAINT estipulante_configuracao_cancela_estipulante_id_fkey FOREIGN KEY (cancela_estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6182 (class 2606 OID 8860714)
-- Name: estipulante_configuracao estipulante_configuracao_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante_configuracao
    ADD CONSTRAINT estipulante_configuracao_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6178 (class 2606 OID 8860688)
-- Name: estipulante estipulante_grupo_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante
    ADD CONSTRAINT estipulante_grupo_id_fkey FOREIGN KEY (grupo_id) REFERENCES cadastro.grupo(id);


--
-- TOC entry 6179 (class 2606 OID 8860678)
-- Name: estipulante estipulante_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante
    ADD CONSTRAINT estipulante_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6180 (class 2606 OID 8860693)
-- Name: estipulante estipulante_seguradora_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.estipulante
    ADD CONSTRAINT estipulante_seguradora_id_fkey FOREIGN KEY (seguradora_id) REFERENCES cadastro.seguradora(id);


--
-- TOC entry 6159 (class 2606 OID 8860820)
-- Name: cliente_vinculo fk_cliente_vinculo_estipulante; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.cliente_vinculo
    ADD CONSTRAINT fk_cliente_vinculo_estipulante FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6148 (class 2606 OID 8860353)
-- Name: lotacao lotacao_cidade_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.lotacao
    ADD CONSTRAINT lotacao_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6176 (class 2606 OID 8860637)
-- Name: seguradora seguradora_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.seguradora
    ADD CONSTRAINT seguradora_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6194 (class 2606 OID 8860878)
-- Name: subestipulante subestipulante_banco_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante
    ADD CONSTRAINT subestipulante_banco_id_fkey FOREIGN KEY (banco_id) REFERENCES core.banco(id);


--
-- TOC entry 6195 (class 2606 OID 8860873)
-- Name: subestipulante subestipulante_cidade_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante
    ADD CONSTRAINT subestipulante_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6196 (class 2606 OID 8860868)
-- Name: subestipulante subestipulante_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante
    ADD CONSTRAINT subestipulante_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6197 (class 2606 OID 8860863)
-- Name: subestipulante subestipulante_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subestipulante
    ADD CONSTRAINT subestipulante_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6147 (class 2606 OID 8860339)
-- Name: subgrupo subgrupo_grupo_id_fkey; Type: FK CONSTRAINT; Schema: cadastro; Owner: postgres
--

ALTER TABLE ONLY cadastro.subgrupo
    ADD CONSTRAINT subgrupo_grupo_id_fkey FOREIGN KEY (grupo_id) REFERENCES cadastro.grupo(id);


--
-- TOC entry 6354 (class 2606 OID 8862459)
-- Name: agenciador_comissao_config agenciador_comissao_config_agenciador_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciador_comissao_config
    ADD CONSTRAINT agenciador_comissao_config_agenciador_id_fkey FOREIGN KEY (agenciador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6366 (class 2606 OID 8862607)
-- Name: agenciamento_corretora_lancamento agenciamento_corretora_lancamento_corretora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciamento_corretora_lancamento
    ADD CONSTRAINT agenciamento_corretora_lancamento_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6367 (class 2606 OID 8862612)
-- Name: agenciamento_corretora_lancamento agenciamento_corretora_lancamento_movimento_tipo_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciamento_corretora_lancamento
    ADD CONSTRAINT agenciamento_corretora_lancamento_movimento_tipo_id_fkey FOREIGN KEY (movimento_tipo_id) REFERENCES seguro.movimento_tipo(id);


--
-- TOC entry 6368 (class 2606 OID 8862602)
-- Name: agenciamento_corretora_lancamento agenciamento_corretora_lancamento_proposta_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.agenciamento_corretora_lancamento
    ADD CONSTRAINT agenciamento_corretora_lancamento_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6355 (class 2606 OID 8862481)
-- Name: corretora_agenciador corretora_agenciador_agenciador_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.corretora_agenciador
    ADD CONSTRAINT corretora_agenciador_agenciador_id_fkey FOREIGN KEY (agenciador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6356 (class 2606 OID 8862476)
-- Name: corretora_agenciador corretora_agenciador_corretora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.corretora_agenciador
    ADD CONSTRAINT corretora_agenciador_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6188 (class 2606 OID 8860773)
-- Name: estipulante_comissao_config estipulante_comissao_config_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.estipulante_comissao_config
    ADD CONSTRAINT estipulante_comissao_config_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6385 (class 2606 OID 8862826)
-- Name: fatura_comissao_resumo fatura_comissao_resumo_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_comissao_resumo
    ADD CONSTRAINT fatura_comissao_resumo_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6379 (class 2606 OID 8862756)
-- Name: fatura_integracao fatura_integracao_corretora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_integracao
    ADD CONSTRAINT fatura_integracao_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6380 (class 2606 OID 8862766)
-- Name: fatura_integracao fatura_integracao_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_integracao
    ADD CONSTRAINT fatura_integracao_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6381 (class 2606 OID 8862761)
-- Name: fatura_integracao fatura_integracao_seguradora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_integracao
    ADD CONSTRAINT fatura_integracao_seguradora_id_fkey FOREIGN KEY (seguradora_id) REFERENCES cadastro.seguradora(id);


--
-- TOC entry 6382 (class 2606 OID 8862787)
-- Name: fatura_vida_agenciamento fatura_vida_agenciamento_proposta_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_agenciamento
    ADD CONSTRAINT fatura_vida_agenciamento_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6383 (class 2606 OID 8862809)
-- Name: fatura_vida_recebimento fatura_vida_recebimento_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_recebimento
    ADD CONSTRAINT fatura_vida_recebimento_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6384 (class 2606 OID 8862804)
-- Name: fatura_vida_recebimento fatura_vida_recebimento_fatura_vida_agenciamento_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.fatura_vida_recebimento
    ADD CONSTRAINT fatura_vida_recebimento_fatura_vida_agenciamento_id_fkey FOREIGN KEY (fatura_vida_agenciamento_id) REFERENCES comissao.fatura_vida_agenciamento(id);


--
-- TOC entry 6273 (class 2606 OID 8861646)
-- Name: lancamento_comissao lancamento_comissao_cliente_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6274 (class 2606 OID 8861651)
-- Name: lancamento_comissao lancamento_comissao_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6275 (class 2606 OID 8861641)
-- Name: lancamento_comissao lancamento_comissao_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6276 (class 2606 OID 8861636)
-- Name: lancamento_comissao lancamento_comissao_proposta_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6277 (class 2606 OID 8861626)
-- Name: lancamento_comissao lancamento_comissao_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6278 (class 2606 OID 8861631)
-- Name: lancamento_comissao lancamento_comissao_titulo_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_comissao
    ADD CONSTRAINT lancamento_comissao_titulo_id_fkey FOREIGN KEY (titulo_id) REFERENCES financeiro.titulo(id);


--
-- TOC entry 6386 (class 2606 OID 8862847)
-- Name: lancamento_fatura_estipulante lancamento_fatura_estipulante_corretora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_fatura_estipulante
    ADD CONSTRAINT lancamento_fatura_estipulante_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6387 (class 2606 OID 8862842)
-- Name: lancamento_fatura_estipulante lancamento_fatura_estipulante_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.lancamento_fatura_estipulante
    ADD CONSTRAINT lancamento_fatura_estipulante_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6217 (class 2606 OID 8862489)
-- Name: proposta_participante proposta_participante_agenciador_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.proposta_participante
    ADD CONSTRAINT proposta_participante_agenciador_id_fkey FOREIGN KEY (agenciador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6218 (class 2606 OID 8862494)
-- Name: proposta_participante proposta_participante_corretora_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.proposta_participante
    ADD CONSTRAINT proposta_participante_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6219 (class 2606 OID 8861040)
-- Name: proposta_participante proposta_participante_proposta_id_fkey; Type: FK CONSTRAINT; Schema: comissao; Owner: postgres
--

ALTER TABLE ONLY comissao.proposta_participante
    ADD CONSTRAINT proposta_participante_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6164 (class 2606 OID 8860499)
-- Name: corsan_cliente corsan_cliente_cliente_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_cliente
    ADD CONSTRAINT corsan_cliente_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6165 (class 2606 OID 8860504)
-- Name: corsan_cliente corsan_cliente_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_cliente
    ADD CONSTRAINT corsan_cliente_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6166 (class 2606 OID 8860509)
-- Name: corsan_cliente corsan_cliente_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_cliente
    ADD CONSTRAINT corsan_cliente_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6213 (class 2606 OID 8861014)
-- Name: corsan_proposta corsan_proposta_cliente_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta
    ADD CONSTRAINT corsan_proposta_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6214 (class 2606 OID 8861019)
-- Name: corsan_proposta corsan_proposta_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta
    ADD CONSTRAINT corsan_proposta_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6215 (class 2606 OID 8861024)
-- Name: corsan_proposta corsan_proposta_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta
    ADD CONSTRAINT corsan_proposta_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6216 (class 2606 OID 8861009)
-- Name: corsan_proposta corsan_proposta_proposta_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.corsan_proposta
    ADD CONSTRAINT corsan_proposta_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6160 (class 2606 OID 8860468)
-- Name: siape_cliente siape_cliente_cliente_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente
    ADD CONSTRAINT siape_cliente_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6161 (class 2606 OID 8860473)
-- Name: siape_cliente siape_cliente_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente
    ADD CONSTRAINT siape_cliente_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6162 (class 2606 OID 8860483)
-- Name: siape_cliente siape_cliente_orgao_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente
    ADD CONSTRAINT siape_cliente_orgao_id_fkey FOREIGN KEY (orgao_id) REFERENCES convenio.siape_orgao(id);


--
-- TOC entry 6163 (class 2606 OID 8860478)
-- Name: siape_cliente siape_cliente_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: convenio; Owner: postgres
--

ALTER TABLE ONLY convenio.siape_cliente
    ADD CONSTRAINT siape_cliente_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6144 (class 2606 OID 8860271)
-- Name: cidade cidade_estado_id_fkey; Type: FK CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.cidade
    ADD CONSTRAINT cidade_estado_id_fkey FOREIGN KEY (estado_id) REFERENCES core.estado(id);


--
-- TOC entry 6143 (class 2606 OID 8860247)
-- Name: pessoa_contato pessoa_contato_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_contato
    ADD CONSTRAINT pessoa_contato_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6142 (class 2606 OID 8860230)
-- Name: pessoa_documento pessoa_documento_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_documento
    ADD CONSTRAINT pessoa_documento_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6145 (class 2606 OID 8860294)
-- Name: pessoa_endereco pessoa_endereco_cidade_id_fkey; Type: FK CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_endereco
    ADD CONSTRAINT pessoa_endereco_cidade_id_fkey FOREIGN KEY (cidade_id) REFERENCES core.cidade(id);


--
-- TOC entry 6146 (class 2606 OID 8860289)
-- Name: pessoa_endereco pessoa_endereco_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: core; Owner: postgres
--

ALTER TABLE ONLY core.pessoa_endereco
    ADD CONSTRAINT pessoa_endereco_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6324 (class 2606 OID 8862131)
-- Name: arquivo_acesso_log arquivo_acesso_log_arquivo_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_acesso_log
    ADD CONSTRAINT arquivo_acesso_log_arquivo_id_fkey FOREIGN KEY (arquivo_id) REFERENCES documento.arquivo(id);


--
-- TOC entry 6319 (class 2606 OID 8862059)
-- Name: arquivo arquivo_storage_provider_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo
    ADD CONSTRAINT arquivo_storage_provider_id_fkey FOREIGN KEY (storage_provider_id) REFERENCES documento.storage_provider(id);


--
-- TOC entry 6322 (class 2606 OID 8862110)
-- Name: arquivo_versao arquivo_versao_arquivo_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_versao
    ADD CONSTRAINT arquivo_versao_arquivo_id_fkey FOREIGN KEY (arquivo_id) REFERENCES documento.arquivo(id);


--
-- TOC entry 6323 (class 2606 OID 8862115)
-- Name: arquivo_versao arquivo_versao_storage_provider_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_versao
    ADD CONSTRAINT arquivo_versao_storage_provider_id_fkey FOREIGN KEY (storage_provider_id) REFERENCES documento.storage_provider(id);


--
-- TOC entry 6320 (class 2606 OID 8862083)
-- Name: arquivo_vinculo arquivo_vinculo_arquivo_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_vinculo
    ADD CONSTRAINT arquivo_vinculo_arquivo_id_fkey FOREIGN KEY (arquivo_id) REFERENCES documento.arquivo(id);


--
-- TOC entry 6321 (class 2606 OID 8862088)
-- Name: arquivo_vinculo arquivo_vinculo_tipo_anexo_id_fkey; Type: FK CONSTRAINT; Schema: documento; Owner: postgres
--

ALTER TABLE ONLY documento.arquivo_vinculo
    ADD CONSTRAINT arquivo_vinculo_tipo_anexo_id_fkey FOREIGN KEY (tipo_anexo_id) REFERENCES documento.tipo_anexo(id);


--
-- TOC entry 6377 (class 2606 OID 8862737)
-- Name: cobranca_acompanhamento cobranca_acompanhamento_cliente_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.cobranca_acompanhamento
    ADD CONSTRAINT cobranca_acompanhamento_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6378 (class 2606 OID 8862732)
-- Name: cobranca_acompanhamento cobranca_acompanhamento_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.cobranca_acompanhamento
    ADD CONSTRAINT cobranca_acompanhamento_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6168 (class 2606 OID 8860558)
-- Name: conta_cobranca conta_cobranca_cliente_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6169 (class 2606 OID 8860563)
-- Name: conta_cobranca conta_cobranca_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6170 (class 2606 OID 8860568)
-- Name: conta_cobranca conta_cobranca_convenio_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_convenio_cobranca_id_fkey FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca(id);


--
-- TOC entry 6171 (class 2606 OID 8860553)
-- Name: conta_cobranca conta_cobranca_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6172 (class 2606 OID 8860573)
-- Name: conta_cobranca conta_cobranca_regra_agrupamento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.conta_cobranca
    ADD CONSTRAINT conta_cobranca_regra_agrupamento_id_fkey FOREIGN KEY (regra_agrupamento_id) REFERENCES financeiro.regra_agrupamento_fatura(id);


--
-- TOC entry 6167 (class 2606 OID 8860537)
-- Name: convenio_cobranca convenio_cobranca_banco_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.convenio_cobranca
    ADD CONSTRAINT convenio_cobranca_banco_id_fkey FOREIGN KEY (banco_id) REFERENCES core.banco(id);


--
-- TOC entry 6183 (class 2606 OID 8860745)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_convenio_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_convenio_cobranca_id_fkey FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca(id);


--
-- TOC entry 6184 (class 2606 OID 8860735)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6185 (class 2606 OID 8860740)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_forma_pagamento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_forma_pagamento_id_fkey FOREIGN KEY (forma_pagamento_id) REFERENCES financeiro.forma_pagamento_estipulante(id);


--
-- TOC entry 6186 (class 2606 OID 8860755)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_parametro_siape_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_parametro_siape_id_fkey FOREIGN KEY (parametro_siape_id) REFERENCES convenio.siape_parametro(id);


--
-- TOC entry 6187 (class 2606 OID 8860750)
-- Name: estipulante_faturamento_config estipulante_faturamento_config_regra_agrupamento_fatura_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.estipulante_faturamento_config
    ADD CONSTRAINT estipulante_faturamento_config_regra_agrupamento_fatura_id_fkey FOREIGN KEY (regra_agrupamento_fatura_id) REFERENCES financeiro.regra_agrupamento_fatura(id);


--
-- TOC entry 6373 (class 2606 OID 8862682)
-- Name: forma_retorno_estipulante forma_retorno_estipulante_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno_estipulante
    ADD CONSTRAINT forma_retorno_estipulante_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6374 (class 2606 OID 8862677)
-- Name: forma_retorno_estipulante forma_retorno_estipulante_forma_retorno_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.forma_retorno_estipulante
    ADD CONSTRAINT forma_retorno_estipulante_forma_retorno_id_fkey FOREIGN KEY (forma_retorno_id) REFERENCES financeiro.forma_retorno(id);


--
-- TOC entry 6375 (class 2606 OID 8862708)
-- Name: movimento_cobranca_log movimento_cobranca_log_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.movimento_cobranca_log
    ADD CONSTRAINT movimento_cobranca_log_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6376 (class 2606 OID 8862713)
-- Name: movimento_cobranca_log movimento_cobranca_log_titulo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.movimento_cobranca_log
    ADD CONSTRAINT movimento_cobranca_log_titulo_id_fkey FOREIGN KEY (titulo_id) REFERENCES financeiro.titulo(id);


--
-- TOC entry 6259 (class 2606 OID 8861508)
-- Name: titulo titulo_cliente_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6260 (class 2606 OID 8861513)
-- Name: titulo titulo_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6261 (class 2606 OID 8861528)
-- Name: titulo titulo_conta_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_conta_cobranca_id_fkey FOREIGN KEY (conta_cobranca_id) REFERENCES financeiro.conta_cobranca(id);


--
-- TOC entry 6262 (class 2606 OID 8861523)
-- Name: titulo titulo_convenio_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_convenio_cobranca_id_fkey FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca(id);


--
-- TOC entry 6263 (class 2606 OID 8861518)
-- Name: titulo titulo_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6268 (class 2606 OID 8861564)
-- Name: titulo_pagamento titulo_pagamento_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_pagamento
    ADD CONSTRAINT titulo_pagamento_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6269 (class 2606 OID 8861559)
-- Name: titulo_pagamento titulo_pagamento_titulo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_pagamento
    ADD CONSTRAINT titulo_pagamento_titulo_id_fkey FOREIGN KEY (titulo_id) REFERENCES financeiro.titulo(id);


--
-- TOC entry 6264 (class 2606 OID 8861503)
-- Name: titulo titulo_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6265 (class 2606 OID 8861498)
-- Name: titulo titulo_proposta_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6266 (class 2606 OID 8861493)
-- Name: titulo titulo_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6270 (class 2606 OID 8861602)
-- Name: titulo_retorno_bancario titulo_retorno_bancario_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_retorno_bancario
    ADD CONSTRAINT titulo_retorno_bancario_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6271 (class 2606 OID 8861607)
-- Name: titulo_retorno_bancario titulo_retorno_bancario_retorno_codigo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_retorno_bancario
    ADD CONSTRAINT titulo_retorno_bancario_retorno_codigo_id_fkey FOREIGN KEY (retorno_codigo_id) REFERENCES financeiro.retorno_bancario_codigo(id);


--
-- TOC entry 6272 (class 2606 OID 8861597)
-- Name: titulo_retorno_bancario titulo_retorno_bancario_titulo_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo_retorno_bancario
    ADD CONSTRAINT titulo_retorno_bancario_titulo_id_fkey FOREIGN KEY (titulo_id) REFERENCES financeiro.titulo(id);


--
-- TOC entry 6267 (class 2606 OID 8861533)
-- Name: titulo titulo_status_id_fkey; Type: FK CONSTRAINT; Schema: financeiro; Owner: postgres
--

ALTER TABLE ONLY financeiro.titulo
    ADD CONSTRAINT titulo_status_id_fkey FOREIGN KEY (status_id) REFERENCES financeiro.titulo_status(id);


--
-- TOC entry 6357 (class 2606 OID 8862515)
-- Name: agenciador_migration_map agenciador_migration_map_agenciador_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map
    ADD CONSTRAINT agenciador_migration_map_agenciador_id_fkey FOREIGN KEY (agenciador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6358 (class 2606 OID 8862525)
-- Name: agenciador_migration_map agenciador_migration_map_coordenador_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map
    ADD CONSTRAINT agenciador_migration_map_coordenador_id_fkey FOREIGN KEY (coordenador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6359 (class 2606 OID 8862520)
-- Name: agenciador_migration_map agenciador_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciador_migration_map
    ADD CONSTRAINT agenciador_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6369 (class 2606 OID 8862635)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancam_agenciamento_corretora_lanca_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancam_agenciamento_corretora_lanca_fkey FOREIGN KEY (agenciamento_corretora_lancamento_id) REFERENCES comissao.agenciamento_corretora_lancamento(id);


--
-- TOC entry 6370 (class 2606 OID 8862650)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancamento_migrat_movimento_tipo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancamento_migrat_movimento_tipo_id_fkey FOREIGN KEY (movimento_tipo_id) REFERENCES seguro.movimento_tipo(id);


--
-- TOC entry 6371 (class 2606 OID 8862645)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancamento_migration_m_corretora_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancamento_migration_m_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6372 (class 2606 OID 8862640)
-- Name: agenciamento_corretora_lancamento_migration_map agenciamento_corretora_lancamento_migration_ma_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.agenciamento_corretora_lancamento_migration_map
    ADD CONSTRAINT agenciamento_corretora_lancamento_migration_ma_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6173 (class 2606 OID 8860609)
-- Name: cliente_migration_map cliente_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map
    ADD CONSTRAINT cliente_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6174 (class 2606 OID 8860614)
-- Name: cliente_migration_map cliente_migration_map_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map
    ADD CONSTRAINT cliente_migration_map_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6175 (class 2606 OID 8860604)
-- Name: cliente_migration_map cliente_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cliente_migration_map
    ADD CONSTRAINT cliente_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6241 (class 2606 OID 8861310)
-- Name: cobertura_migration_map cobertura_migration_map_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.cobertura_migration_map
    ADD CONSTRAINT cobertura_migration_map_cobertura_id_fkey FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura(id);


--
-- TOC entry 6360 (class 2606 OID 8862545)
-- Name: corretora_migration_map corretora_migration_map_corretora_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.corretora_migration_map
    ADD CONSTRAINT corretora_migration_map_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6361 (class 2606 OID 8862550)
-- Name: corretora_migration_map corretora_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.corretora_migration_map
    ADD CONSTRAINT corretora_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6325 (class 2606 OID 8862150)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_arquivo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_arquivo_id_fkey FOREIGN KEY (arquivo_id) REFERENCES documento.arquivo(id);


--
-- TOC entry 6326 (class 2606 OID 8862155)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6327 (class 2606 OID 8862170)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6328 (class 2606 OID 8862160)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6329 (class 2606 OID 8862165)
-- Name: documento_anexo_migration_map documento_anexo_migration_map_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.documento_anexo_migration_map
    ADD CONSTRAINT documento_anexo_migration_map_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6189 (class 2606 OID 8860813)
-- Name: estipulante_migration_map estipulante_migration_map_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.estipulante_migration_map
    ADD CONSTRAINT estipulante_migration_map_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6190 (class 2606 OID 8860808)
-- Name: estipulante_migration_map estipulante_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.estipulante_migration_map
    ADD CONSTRAINT estipulante_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6279 (class 2606 OID 8861687)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_ma_titulo_retorno_bancario_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_ma_titulo_retorno_bancario_id_fkey FOREIGN KEY (titulo_retorno_bancario_id) REFERENCES financeiro.titulo_retorno_bancario(id);


--
-- TOC entry 6280 (class 2606 OID 8861702)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6281 (class 2606 OID 8861707)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6282 (class 2606 OID 8861717)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6283 (class 2606 OID 8861692)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_lancamento_comissao_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_lancamento_comissao_id_fkey FOREIGN KEY (lancamento_comissao_id) REFERENCES comissao.lancamento_comissao(id);


--
-- TOC entry 6284 (class 2606 OID 8861722)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_movimento_tipo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_movimento_tipo_id_fkey FOREIGN KEY (movimento_tipo_id) REFERENCES seguro.movimento_tipo(id);


--
-- TOC entry 6285 (class 2606 OID 8861712)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6286 (class 2606 OID 8861697)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6287 (class 2606 OID 8861672)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_proposta_movimento_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_proposta_movimento_id_fkey FOREIGN KEY (proposta_movimento_id) REFERENCES seguro.proposta_movimento(id);


--
-- TOC entry 6288 (class 2606 OID 8861677)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_titulo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_titulo_id_fkey FOREIGN KEY (titulo_id) REFERENCES financeiro.titulo(id);


--
-- TOC entry 6289 (class 2606 OID 8861682)
-- Name: movimento_proposta_migration_map movimento_proposta_migration_map_titulo_pagamento_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.movimento_proposta_migration_map
    ADD CONSTRAINT movimento_proposta_migration_map_titulo_pagamento_id_fkey FOREIGN KEY (titulo_pagamento_id) REFERENCES financeiro.titulo_pagamento(id);


--
-- TOC entry 6238 (class 2606 OID 8861265)
-- Name: plano_migration_map plano_migration_map_plano_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.plano_migration_map
    ADD CONSTRAINT plano_migration_map_plano_id_fkey FOREIGN KEY (plano_id) REFERENCES seguro.plano(id);


--
-- TOC entry 6240 (class 2606 OID 8861295)
-- Name: produto_migration_map produto_migration_map_produto_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.produto_migration_map
    ADD CONSTRAINT produto_migration_map_produto_id_fkey FOREIGN KEY (produto_id) REFERENCES seguro.produto(id);


--
-- TOC entry 6292 (class 2606 OID 8861773)
-- Name: proposta_beneficiario_migration_map proposta_beneficiario_migration_m_proposta_beneficiario_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map
    ADD CONSTRAINT proposta_beneficiario_migration_m_proposta_beneficiario_id_fkey FOREIGN KEY (proposta_beneficiario_id) REFERENCES seguro.proposta_beneficiario(id);


--
-- TOC entry 6293 (class 2606 OID 8861783)
-- Name: proposta_beneficiario_migration_map proposta_beneficiario_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map
    ADD CONSTRAINT proposta_beneficiario_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6294 (class 2606 OID 8861778)
-- Name: proposta_beneficiario_migration_map proposta_beneficiario_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_beneficiario_migration_map
    ADD CONSTRAINT proposta_beneficiario_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6248 (class 2606 OID 8861387)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_map_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_map_cobertura_id_fkey FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura(id);


--
-- TOC entry 6249 (class 2606 OID 8861372)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_map_proposta_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_map_proposta_cobertura_id_fkey FOREIGN KEY (proposta_cobertura_id) REFERENCES seguro.proposta_cobertura(id);


--
-- TOC entry 6250 (class 2606 OID 8861377)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6251 (class 2606 OID 8861382)
-- Name: proposta_cobertura_migration_map proposta_cobertura_migration_map_proposta_item_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_cobertura_migration_map
    ADD CONSTRAINT proposta_cobertura_migration_map_proposta_item_id_fkey FOREIGN KEY (proposta_item_id) REFERENCES seguro.proposta_item(id);


--
-- TOC entry 6242 (class 2606 OID 8861347)
-- Name: proposta_item_migration_map proposta_item_migration_map_plano_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_plano_id_fkey FOREIGN KEY (plano_id) REFERENCES seguro.plano(id);


--
-- TOC entry 6243 (class 2606 OID 8861342)
-- Name: proposta_item_migration_map proposta_item_migration_map_produto_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_produto_id_fkey FOREIGN KEY (produto_id) REFERENCES seguro.produto(id);


--
-- TOC entry 6244 (class 2606 OID 8861332)
-- Name: proposta_item_migration_map proposta_item_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6245 (class 2606 OID 8861327)
-- Name: proposta_item_migration_map proposta_item_migration_map_proposta_item_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_proposta_item_id_fkey FOREIGN KEY (proposta_item_id) REFERENCES seguro.proposta_item(id);


--
-- TOC entry 6246 (class 2606 OID 8861352)
-- Name: proposta_item_migration_map proposta_item_migration_map_tabela_preco_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_tabela_preco_id_fkey FOREIGN KEY (tabela_preco_id) REFERENCES seguro.tabela_preco(id);


--
-- TOC entry 6247 (class 2606 OID 8861337)
-- Name: proposta_item_migration_map proposta_item_migration_map_tipo_produto_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_item_migration_map
    ADD CONSTRAINT proposta_item_migration_map_tipo_produto_id_fkey FOREIGN KEY (tipo_produto_id) REFERENCES seguro.tipo_produto(id);


--
-- TOC entry 6220 (class 2606 OID 8861064)
-- Name: proposta_migration_map proposta_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6221 (class 2606 OID 8861069)
-- Name: proposta_migration_map proposta_migration_map_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6222 (class 2606 OID 8861079)
-- Name: proposta_migration_map proposta_migration_map_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6223 (class 2606 OID 8861074)
-- Name: proposta_migration_map proposta_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6224 (class 2606 OID 8861059)
-- Name: proposta_migration_map proposta_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6225 (class 2606 OID 8861089)
-- Name: proposta_migration_map proposta_migration_map_status_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_status_id_fkey FOREIGN KEY (status_id) REFERENCES seguro.proposta_status(id);


--
-- TOC entry 6226 (class 2606 OID 8861084)
-- Name: proposta_migration_map proposta_migration_map_subestipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_migration_map
    ADD CONSTRAINT proposta_migration_map_subestipulante_id_fkey FOREIGN KEY (subestipulante_id) REFERENCES cadastro.subestipulante(id);


--
-- TOC entry 6362 (class 2606 OID 8862566)
-- Name: proposta_participante_migration_map proposta_participante_migration_m_proposta_participante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map
    ADD CONSTRAINT proposta_participante_migration_m_proposta_participante_id_fkey FOREIGN KEY (proposta_participante_id) REFERENCES comissao.proposta_participante(id);


--
-- TOC entry 6363 (class 2606 OID 8862576)
-- Name: proposta_participante_migration_map proposta_participante_migration_map_agenciador_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map
    ADD CONSTRAINT proposta_participante_migration_map_agenciador_id_fkey FOREIGN KEY (agenciador_id) REFERENCES cadastro.agenciador(id);


--
-- TOC entry 6364 (class 2606 OID 8862581)
-- Name: proposta_participante_migration_map proposta_participante_migration_map_corretora_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map
    ADD CONSTRAINT proposta_participante_migration_map_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6365 (class 2606 OID 8862571)
-- Name: proposta_participante_migration_map proposta_participante_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.proposta_participante_migration_map
    ADD CONSTRAINT proposta_participante_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6343 (class 2606 OID 8862342)
-- Name: protocolo_acompanhamento_migration_map protocolo_acompanhamento_migra_protocolo_acompanhamento_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_acompanhamento_migration_map
    ADD CONSTRAINT protocolo_acompanhamento_migra_protocolo_acompanhamento_id_fkey FOREIGN KEY (protocolo_acompanhamento_id) REFERENCES atendimento.protocolo_acompanhamento(id);


--
-- TOC entry 6344 (class 2606 OID 8862347)
-- Name: protocolo_acompanhamento_migration_map protocolo_acompanhamento_migration_map_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_acompanhamento_migration_map
    ADD CONSTRAINT protocolo_acompanhamento_migration_map_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6337 (class 2606 OID 8862307)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6338 (class 2606 OID 8862312)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6339 (class 2606 OID 8862322)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6340 (class 2606 OID 8862317)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6341 (class 2606 OID 8862297)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_protocolo_item_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_protocolo_item_id_fkey FOREIGN KEY (protocolo_item_id) REFERENCES atendimento.protocolo_item(id);


--
-- TOC entry 6342 (class 2606 OID 8862302)
-- Name: protocolo_item_migration_map protocolo_item_migration_map_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_item_migration_map
    ADD CONSTRAINT protocolo_item_migration_map_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6336 (class 2606 OID 8862279)
-- Name: protocolo_lote_migration_map protocolo_lote_migration_map_protocolo_lote_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.protocolo_lote_migration_map
    ADD CONSTRAINT protocolo_lote_migration_map_protocolo_lote_id_fkey FOREIGN KEY (protocolo_lote_id) REFERENCES atendimento.protocolo_lote(id);


--
-- TOC entry 6317 (class 2606 OID 8862002)
-- Name: sinistro_acompanhamento_migration_map sinistro_acompanhamento_migration_map_acompanhamento_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_acompanhamento_migration_map
    ADD CONSTRAINT sinistro_acompanhamento_migration_map_acompanhamento_id_fkey FOREIGN KEY (acompanhamento_id) REFERENCES sinistro.acompanhamento(id);


--
-- TOC entry 6318 (class 2606 OID 8862007)
-- Name: sinistro_acompanhamento_migration_map sinistro_acompanhamento_migration_map_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_acompanhamento_migration_map
    ADD CONSTRAINT sinistro_acompanhamento_migration_map_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6311 (class 2606 OID 8861972)
-- Name: sinistro_migration_map sinistro_migration_map_cliente_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6312 (class 2606 OID 8861977)
-- Name: sinistro_migration_map sinistro_migration_map_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6313 (class 2606 OID 8861967)
-- Name: sinistro_migration_map sinistro_migration_map_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6314 (class 2606 OID 8861962)
-- Name: sinistro_migration_map sinistro_migration_map_proposta_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6315 (class 2606 OID 8861957)
-- Name: sinistro_migration_map sinistro_migration_map_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6316 (class 2606 OID 8861982)
-- Name: sinistro_migration_map sinistro_migration_map_status_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.sinistro_migration_map
    ADD CONSTRAINT sinistro_migration_map_status_id_fkey FOREIGN KEY (status_id) REFERENCES sinistro.sinistro_status(id);


--
-- TOC entry 6239 (class 2606 OID 8861280)
-- Name: tabela_preco_migration_map tabela_preco_migration_map_tabela_preco_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tabela_preco_migration_map
    ADD CONSTRAINT tabela_preco_migration_map_tabela_preco_id_fkey FOREIGN KEY (tabela_preco_id) REFERENCES seguro.tabela_preco(id);


--
-- TOC entry 6237 (class 2606 OID 8861250)
-- Name: tipo_produto_migration_map tipo_produto_migration_map_tipo_produto_id_fkey; Type: FK CONSTRAINT; Schema: legado; Owner: postgres
--

ALTER TABLE ONLY legado.tipo_produto_migration_map
    ADD CONSTRAINT tipo_produto_migration_map_tipo_produto_id_fkey FOREIGN KEY (tipo_produto_id) REFERENCES seguro.tipo_produto(id);


--
-- TOC entry 6227 (class 2606 OID 8861149)
-- Name: produto produto_plano_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.produto
    ADD CONSTRAINT produto_plano_id_fkey FOREIGN KEY (plano_id) REFERENCES seguro.plano(id);


--
-- TOC entry 6228 (class 2606 OID 8861144)
-- Name: produto produto_tabela_preco_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.produto
    ADD CONSTRAINT produto_tabela_preco_id_fkey FOREIGN KEY (tabela_preco_id) REFERENCES seguro.tabela_preco(id);


--
-- TOC entry 6290 (class 2606 OID 8861749)
-- Name: proposta_beneficiario proposta_beneficiario_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_beneficiario
    ADD CONSTRAINT proposta_beneficiario_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6291 (class 2606 OID 8861744)
-- Name: proposta_beneficiario proposta_beneficiario_proposta_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_beneficiario
    ADD CONSTRAINT proposta_beneficiario_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6198 (class 2606 OID 8860906)
-- Name: proposta proposta_cliente_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6199 (class 2606 OID 8860911)
-- Name: proposta proposta_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6234 (class 2606 OID 8861231)
-- Name: proposta_cobertura proposta_cobertura_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_cobertura
    ADD CONSTRAINT proposta_cobertura_cobertura_id_fkey FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura(id);


--
-- TOC entry 6235 (class 2606 OID 8861221)
-- Name: proposta_cobertura proposta_cobertura_proposta_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_cobertura
    ADD CONSTRAINT proposta_cobertura_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6236 (class 2606 OID 8861226)
-- Name: proposta_cobertura proposta_cobertura_proposta_item_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_cobertura
    ADD CONSTRAINT proposta_cobertura_proposta_item_id_fkey FOREIGN KEY (proposta_item_id) REFERENCES seguro.proposta_item(id);


--
-- TOC entry 6200 (class 2606 OID 8860941)
-- Name: proposta proposta_conta_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_conta_cobranca_id_fkey FOREIGN KEY (conta_cobranca_id) REFERENCES financeiro.conta_cobranca(id);


--
-- TOC entry 6201 (class 2606 OID 8860936)
-- Name: proposta proposta_convenio_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_convenio_cobranca_id_fkey FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca(id);


--
-- TOC entry 6202 (class 2606 OID 8860931)
-- Name: proposta proposta_corretora_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_corretora_id_fkey FOREIGN KEY (corretora_id) REFERENCES cadastro.corretora(id);


--
-- TOC entry 6203 (class 2606 OID 8860916)
-- Name: proposta proposta_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6211 (class 2606 OID 8860987)
-- Name: proposta_historico proposta_historico_proposta_anterior_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_historico
    ADD CONSTRAINT proposta_historico_proposta_anterior_id_fkey FOREIGN KEY (proposta_anterior_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6212 (class 2606 OID 8860992)
-- Name: proposta_historico proposta_historico_proposta_nova_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_historico
    ADD CONSTRAINT proposta_historico_proposta_nova_id_fkey FOREIGN KEY (proposta_nova_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6229 (class 2606 OID 8861200)
-- Name: proposta_item proposta_item_plano_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_plano_id_fkey FOREIGN KEY (plano_id) REFERENCES seguro.plano(id);


--
-- TOC entry 6230 (class 2606 OID 8861195)
-- Name: proposta_item proposta_item_produto_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_produto_id_fkey FOREIGN KEY (produto_id) REFERENCES seguro.produto(id);


--
-- TOC entry 6231 (class 2606 OID 8861180)
-- Name: proposta_item proposta_item_proposta_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6232 (class 2606 OID 8861190)
-- Name: proposta_item proposta_item_tabela_preco_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_tabela_preco_id_fkey FOREIGN KEY (tabela_preco_id) REFERENCES seguro.tabela_preco(id);


--
-- TOC entry 6233 (class 2606 OID 8861185)
-- Name: proposta_item proposta_item_tipo_produto_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_item
    ADD CONSTRAINT proposta_item_tipo_produto_id_fkey FOREIGN KEY (tipo_produto_id) REFERENCES seguro.tipo_produto(id);


--
-- TOC entry 6204 (class 2606 OID 8860956)
-- Name: proposta proposta_lotacao_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_lotacao_id_fkey FOREIGN KEY (lotacao_id) REFERENCES cadastro.lotacao(id);


--
-- TOC entry 6252 (class 2606 OID 8861434)
-- Name: proposta_movimento proposta_movimento_cliente_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6253 (class 2606 OID 8861439)
-- Name: proposta_movimento proposta_movimento_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6254 (class 2606 OID 8861449)
-- Name: proposta_movimento proposta_movimento_convenio_cobranca_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_convenio_cobranca_id_fkey FOREIGN KEY (convenio_cobranca_id) REFERENCES financeiro.convenio_cobranca(id);


--
-- TOC entry 6255 (class 2606 OID 8861444)
-- Name: proposta_movimento proposta_movimento_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6256 (class 2606 OID 8861454)
-- Name: proposta_movimento proposta_movimento_movimento_tipo_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_movimento_tipo_id_fkey FOREIGN KEY (movimento_tipo_id) REFERENCES seguro.movimento_tipo(id);


--
-- TOC entry 6257 (class 2606 OID 8861429)
-- Name: proposta_movimento proposta_movimento_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6258 (class 2606 OID 8861424)
-- Name: proposta_movimento proposta_movimento_proposta_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta_movimento
    ADD CONSTRAINT proposta_movimento_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6205 (class 2606 OID 8860901)
-- Name: proposta proposta_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6206 (class 2606 OID 8860961)
-- Name: proposta proposta_proposta_origem_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_proposta_origem_id_fkey FOREIGN KEY (proposta_origem_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6207 (class 2606 OID 8860926)
-- Name: proposta proposta_seguradora_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_seguradora_id_fkey FOREIGN KEY (seguradora_id) REFERENCES cadastro.seguradora(id);


--
-- TOC entry 6208 (class 2606 OID 8860946)
-- Name: proposta proposta_status_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_status_id_fkey FOREIGN KEY (status_id) REFERENCES seguro.proposta_status(id);


--
-- TOC entry 6209 (class 2606 OID 8860921)
-- Name: proposta proposta_subestipulante_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_subestipulante_id_fkey FOREIGN KEY (subestipulante_id) REFERENCES cadastro.subestipulante(id);


--
-- TOC entry 6210 (class 2606 OID 8860951)
-- Name: proposta proposta_subgrupo_id_fkey; Type: FK CONSTRAINT; Schema: seguro; Owner: postgres
--

ALTER TABLE ONLY seguro.proposta
    ADD CONSTRAINT proposta_subgrupo_id_fkey FOREIGN KEY (subgrupo_id) REFERENCES cadastro.subgrupo(id);


--
-- TOC entry 6302 (class 2606 OID 8861869)
-- Name: acompanhamento acompanhamento_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.acompanhamento
    ADD CONSTRAINT acompanhamento_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6303 (class 2606 OID 8861903)
-- Name: sinistro_beneficiario sinistro_beneficiario_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario
    ADD CONSTRAINT sinistro_beneficiario_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6304 (class 2606 OID 8861898)
-- Name: sinistro_beneficiario sinistro_beneficiario_proposta_beneficiario_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario
    ADD CONSTRAINT sinistro_beneficiario_proposta_beneficiario_id_fkey FOREIGN KEY (proposta_beneficiario_id) REFERENCES seguro.proposta_beneficiario(id);


--
-- TOC entry 6305 (class 2606 OID 8861893)
-- Name: sinistro_beneficiario sinistro_beneficiario_proposta_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario
    ADD CONSTRAINT sinistro_beneficiario_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6306 (class 2606 OID 8861888)
-- Name: sinistro_beneficiario sinistro_beneficiario_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_beneficiario
    ADD CONSTRAINT sinistro_beneficiario_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6295 (class 2606 OID 8861825)
-- Name: sinistro sinistro_cliente_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_cliente_id_fkey FOREIGN KEY (cliente_id) REFERENCES cadastro.cliente(id);


--
-- TOC entry 6296 (class 2606 OID 8861830)
-- Name: sinistro sinistro_cliente_vinculo_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_cliente_vinculo_id_fkey FOREIGN KEY (cliente_vinculo_id) REFERENCES cadastro.cliente_vinculo(id);


--
-- TOC entry 6307 (class 2606 OID 8861936)
-- Name: sinistro_cobertura sinistro_cobertura_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura
    ADD CONSTRAINT sinistro_cobertura_cobertura_id_fkey FOREIGN KEY (cobertura_id) REFERENCES seguro.cobertura(id);


--
-- TOC entry 6308 (class 2606 OID 8861931)
-- Name: sinistro_cobertura sinistro_cobertura_proposta_cobertura_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura
    ADD CONSTRAINT sinistro_cobertura_proposta_cobertura_id_fkey FOREIGN KEY (proposta_cobertura_id) REFERENCES seguro.proposta_cobertura(id);


--
-- TOC entry 6309 (class 2606 OID 8861926)
-- Name: sinistro_cobertura sinistro_cobertura_proposta_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura
    ADD CONSTRAINT sinistro_cobertura_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6310 (class 2606 OID 8861921)
-- Name: sinistro_cobertura sinistro_cobertura_sinistro_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro_cobertura
    ADD CONSTRAINT sinistro_cobertura_sinistro_id_fkey FOREIGN KEY (sinistro_id) REFERENCES sinistro.sinistro(id);


--
-- TOC entry 6297 (class 2606 OID 8861835)
-- Name: sinistro sinistro_estipulante_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_estipulante_id_fkey FOREIGN KEY (estipulante_id) REFERENCES cadastro.estipulante(id);


--
-- TOC entry 6298 (class 2606 OID 8861820)
-- Name: sinistro sinistro_pessoa_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_pessoa_id_fkey FOREIGN KEY (pessoa_id) REFERENCES core.pessoa(id);


--
-- TOC entry 6299 (class 2606 OID 8861815)
-- Name: sinistro sinistro_proposta_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_proposta_id_fkey FOREIGN KEY (proposta_id) REFERENCES seguro.proposta(id);


--
-- TOC entry 6300 (class 2606 OID 8861840)
-- Name: sinistro sinistro_seguradora_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_seguradora_id_fkey FOREIGN KEY (seguradora_id) REFERENCES cadastro.seguradora(id);


--
-- TOC entry 6301 (class 2606 OID 8861845)
-- Name: sinistro sinistro_status_id_fkey; Type: FK CONSTRAINT; Schema: sinistro; Owner: postgres
--

ALTER TABLE ONLY sinistro.sinistro
    ADD CONSTRAINT sinistro_status_id_fkey FOREIGN KEY (status_id) REFERENCES sinistro.sinistro_status(id);


-- Completed on 2026-07-06 21:25:55

--
-- PostgreSQL database dump complete
--

