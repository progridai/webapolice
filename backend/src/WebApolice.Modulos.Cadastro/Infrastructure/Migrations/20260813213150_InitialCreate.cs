using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cadastro");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "agenciador",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    coordenador_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    tipo = table.Column<short>(type: "smallint", nullable: true),
                    cpf = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    orgao_rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_emissao_rg = table.Column<DateOnly>(type: "date", nullable: true),
                    susep = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inss = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    issqn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    telefone = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    complemento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    numero_dependentes = table.Column<int>(type: "integer", nullable: true),
                    data_inscricao = table.Column<DateOnly>(type: "date", nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    credenciado = table.Column<bool>(type: "boolean", nullable: true),
                    desativado = table.Column<bool>(type: "boolean", nullable: false),
                    data_desativado = table.Column<DateOnly>(type: "date", nullable: true),
                    agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    conta_corrente = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_ant_ven = table.Column<int>(type: "integer", nullable: true),
                    legado_ant_ger = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("agenciador_pkey", x => x.id);
                    table.ForeignKey(
                        name: "agenciador_coordenador_id_fkey",
                        column: x => x.coordenador_id,
                        principalSchema: "cadastro",
                        principalTable: "agenciador",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cliente_status",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_status_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "corretora",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    cep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefone = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    codigo_protheus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    caminho_logotipo_legado = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    logotipo_arquivo_id = table.Column<long>(type: "bigint", nullable: true),
                    possui_logotipo_legado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("corretora_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupo",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("grupo_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lotacao",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("lotacao_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seguradora",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    susep = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cnpj_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("seguradora_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<short>(type: "smallint", nullable: false),
                    falecido = table.Column<bool>(type: "boolean", nullable: false),
                    data_obito = table.Column<DateOnly>(type: "date", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    data_cadastro_legado = table.Column<DateOnly>(type: "date", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    re = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cliente_status_id_fkey",
                        column: x => x.status_id,
                        principalSchema: "cadastro",
                        principalTable: "cliente_status",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subgrupo",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    grupo_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("subgrupo_pkey", x => x.id);
                    table.ForeignKey(
                        name: "subgrupo_grupo_id_fkey",
                        column: x => x.grupo_id,
                        principalSchema: "cadastro",
                        principalTable: "grupo",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "estipulante",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome_formatado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    tipo_pessoa = table.Column<short>(type: "smallint", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cnpj_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    grupo_id = table.Column<long>(type: "bigint", nullable: true),
                    seguradora_id = table.Column<long>(type: "bigint", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("estipulante_pkey", x => x.id);
                    table.ForeignKey(
                        name: "estipulante_grupo_id_fkey",
                        column: x => x.grupo_id,
                        principalSchema: "cadastro",
                        principalTable: "grupo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "estipulante_seguradora_id_fkey",
                        column: x => x.seguradora_id,
                        principalSchema: "cadastro",
                        principalTable: "seguradora",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cliente_dependente",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    tipo_relacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cpf = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    orgao_rg = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_emissao_rg = table.Column<DateOnly>(type: "date", nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    legado_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_dependente_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cliente_dependente_cliente_id_fkey",
                        column: x => x.cliente_id,
                        principalSchema: "cadastro",
                        principalTable: "cliente",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cliente_vinculo",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    grupo_id = table.Column<long>(type: "bigint", nullable: true),
                    subgrupo_id = table.Column<long>(type: "bigint", nullable: true),
                    lotacao_id = table.Column<long>(type: "bigint", nullable: true),
                    matricula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    matricula_normalizada = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    conta_corrente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    criterio_criacao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_vinculo_pkey", x => x.id);
                    table.ForeignKey(
                        name: "cliente_vinculo_cliente_id_fkey",
                        column: x => x.cliente_id,
                        principalSchema: "cadastro",
                        principalTable: "cliente",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_grupo_id_fkey",
                        column: x => x.grupo_id,
                        principalSchema: "cadastro",
                        principalTable: "grupo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_lotacao_id_fkey",
                        column: x => x.lotacao_id,
                        principalSchema: "cadastro",
                        principalTable: "lotacao",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "cliente_vinculo_subgrupo_id_fkey",
                        column: x => x.subgrupo_id,
                        principalSchema: "cadastro",
                        principalTable: "subgrupo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cliente_vinculo_estipulante",
                        column: x => x.estipulante_id,
                        principalSchema: "cadastro",
                        principalTable: "estipulante",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "estipulante_configuracao",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    tabela_legado_id = table.Column<int>(type: "integer", nullable: true),
                    permite_propostas = table.Column<bool>(type: "boolean", nullable: true),
                    controla_comissao = table.Column<bool>(type: "boolean", nullable: true),
                    data_inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    data_fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    data_aniversario = table.Column<DateOnly>(type: "date", nullable: true),
                    data_ultimo_reajuste = table.Column<DateOnly>(type: "date", nullable: true),
                    data_base_reajuste = table.Column<int>(type: "integer", nullable: true),
                    data_limite_reajuste = table.Column<DateOnly>(type: "date", nullable: true),
                    dias_aviso_reajuste = table.Column<int>(type: "integer", nullable: true),
                    carencia = table.Column<int>(type: "integer", nullable: true),
                    adesao_por = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    custeio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    adesao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    faixa_etaria_inicio = table.Column<int>(type: "integer", nullable: true),
                    faixa_etaria_fim = table.Column<int>(type: "integer", nullable: true),
                    cancela_estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    indice_legado_id = table.Column<int>(type: "integer", nullable: true),
                    percentual_indice = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    ajuste_indice = table.Column<bool>(type: "boolean", nullable: true),
                    ajuste_fator = table.Column<bool>(type: "boolean", nullable: true),
                    reajuste = table.Column<int>(type: "integer", nullable: true),
                    tipo_cobertura_conjuge = table.Column<int>(type: "integer", nullable: true),
                    percentual_tipo_cobertura_conjuge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    possui_excedente = table.Column<bool>(type: "boolean", nullable: true),
                    data_limite_excedente = table.Column<DateOnly>(type: "date", nullable: true),
                    dias_aviso_excedente = table.Column<int>(type: "integer", nullable: true),
                    prazo_regulacao = table.Column<int>(type: "integer", nullable: true),
                    dia_corte = table.Column<int>(type: "integer", nullable: true),
                    desconsiderar_proposta_ativa = table.Column<bool>(type: "boolean", nullable: false),
                    permitir_protocolo_duplicado = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("estipulante_configuracao_pkey", x => x.id);
                    table.ForeignKey(
                        name: "estipulante_configuracao_cancela_estipulante_id_fkey",
                        column: x => x.cancela_estipulante_id,
                        principalSchema: "cadastro",
                        principalTable: "estipulante",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "estipulante_configuracao_estipulante_id_fkey",
                        column: x => x.estipulante_id,
                        principalSchema: "cadastro",
                        principalTable: "estipulante",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subestipulante",
                schema: "cadastro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cidade_id = table.Column<long>(type: "bigint", nullable: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    cnpj = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cnpj_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("subestipulante_pkey", x => x.id);
                    table.ForeignKey(
                        name: "subestipulante_estipulante_id_fkey",
                        column: x => x.estipulante_id,
                        principalSchema: "cadastro",
                        principalTable: "estipulante",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_coordenador",
                schema: "cadastro",
                table: "agenciador",
                column: "coordenador_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_cpf",
                schema: "cadastro",
                table: "agenciador",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_desativado",
                schema: "cadastro",
                table: "agenciador",
                column: "desativado");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_nome_trgm",
                schema: "cadastro",
                table: "agenciador",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_pessoa",
                schema: "cadastro",
                table: "agenciador",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ux_agenciador_legado",
                schema: "cadastro",
                table: "agenciador",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_pessoa",
                schema: "cadastro",
                table: "cliente",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_status",
                schema: "cadastro",
                table: "cliente",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ux_cliente_legado",
                schema: "cadastro",
                table: "cliente",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_dependente_cliente",
                schema: "cadastro",
                table: "cliente_dependente",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "cliente_status_codigo_key",
                schema: "cadastro",
                table: "cliente_status",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_cliente",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_estipulante",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_grupo_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_lotacao_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "lotacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_pessoa",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vinculo_pessoa_estip_matricula",
                schema: "cadastro",
                table: "cliente_vinculo",
                columns: new[] { "pessoa_id", "estipulante_id", "matricula_normalizada" });

            migrationBuilder.CreateIndex(
                name: "IX_cliente_vinculo_subgrupo_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "subgrupo_id");

            migrationBuilder.CreateIndex(
                name: "ux_cliente_vinculo_legado_cliente",
                schema: "cadastro",
                table: "cliente_vinculo",
                column: "legado_cliente_id",
                unique: true,
                filter: "(legado_cliente_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_corretora_logotipo_arquivo",
                schema: "cadastro",
                table: "corretora",
                column: "logotipo_arquivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_corretora_nome_trgm",
                schema: "cadastro",
                table: "corretora",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_corretora_legado",
                schema: "cadastro",
                table: "corretora",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_ativo",
                schema: "cadastro",
                table: "estipulante",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_grupo",
                schema: "cadastro",
                table: "estipulante",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_nome_trgm",
                schema: "cadastro",
                table: "estipulante",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_seguradora",
                schema: "cadastro",
                table: "estipulante",
                column: "seguradora_id");

            migrationBuilder.CreateIndex(
                name: "ux_estipulante_legado",
                schema: "cadastro",
                table: "estipulante",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_config_reajuste",
                schema: "cadastro",
                table: "estipulante_configuracao",
                column: "data_limite_reajuste");

            migrationBuilder.CreateIndex(
                name: "IX_estipulante_configuracao_cancela_estipulante_id",
                schema: "cadastro",
                table: "estipulante_configuracao",
                column: "cancela_estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ux_estipulante_config_estipulante",
                schema: "cadastro",
                table: "estipulante_configuracao",
                column: "estipulante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_grupo_legado",
                schema: "cadastro",
                table: "grupo",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_lotacao_legado",
                schema: "cadastro",
                table: "lotacao",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_seguradora_nome_trgm",
                schema: "cadastro",
                table: "seguradora",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_seguradora_legado",
                schema: "cadastro",
                table: "seguradora",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_subestipulante_estipulante",
                schema: "cadastro",
                table: "subestipulante",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_subestipulante_nome_trgm",
                schema: "cadastro",
                table: "subestipulante",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_subestipulante_legado",
                schema: "cadastro",
                table: "subestipulante",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_subgrupo_grupo_id",
                schema: "cadastro",
                table: "subgrupo",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ux_subgrupo_legado",
                schema: "cadastro",
                table: "subgrupo",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agenciador",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente_dependente",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente_vinculo",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "corretora",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "estipulante_configuracao",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "subestipulante",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "lotacao",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "subgrupo",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "estipulante",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "cliente_status",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "grupo",
                schema: "cadastro");

            migrationBuilder.DropTable(
                name: "seguradora",
                schema: "cadastro");
        }
    }
}
