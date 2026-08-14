using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seguro");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "cobertura",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nome_reduzido = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    basica = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reajuste = table.Column<bool>(type: "boolean", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_cobertura_ant = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cobertura_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimento_tipo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    gera_titulo = table.Column<bool>(type: "boolean", nullable: false),
                    classificacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'avaliar'::character varying"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    altera_proposta = table.Column<bool>(type: "boolean", nullable: false),
                    financeiro = table.Column<bool>(type: "boolean", nullable: false),
                    cancelamento = table.Column<bool>(type: "boolean", nullable: false),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("movimento_tipo_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plano",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ramo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    paga = table.Column<bool>(type: "boolean", nullable: true),
                    reajuste = table.Column<bool>(type: "boolean", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_plano_ant = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("plano_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_status",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    permite_movimentacao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    visivel_operacional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    finalizador = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_status_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tabela_preco",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tabela_preco_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_produto",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tipo_produto_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: false),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    seguradora_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    convenio_cobranca_id = table.Column<long>(type: "bigint", nullable: true),
                    conta_cobranca_id = table.Column<long>(type: "bigint", nullable: true),
                    status_id = table.Column<short>(type: "smallint", nullable: false),
                    movimento_tipo_id = table.Column<long>(type: "bigint", nullable: true),
                    numero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_inclusao = table.Column<DateOnly>(type: "date", nullable: true),
                    data_movimento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_primeiro_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_proximo_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    banco_agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    banco_conta_corrente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    banco_data_debito = table.Column<DateOnly>(type: "date", nullable: true),
                    banco_dia_debito = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    premio_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    iof_percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    iof_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_parcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    movimento_fatura_mes = table.Column<int>(type: "integer", nullable: true),
                    movimento_fatura_ano = table.Column<int>(type: "integer", nullable: true),
                    subgrupo_id = table.Column<long>(type: "bigint", nullable: true),
                    lotacao_id = table.Column<long>(type: "bigint", nullable: true),
                    data_ultimo_ajuste_indice = table.Column<DateOnly>(type: "date", nullable: true),
                    comissao_estornada = table.Column<bool>(type: "boolean", nullable: true),
                    data_estorno_comissao = table.Column<DateOnly>(type: "date", nullable: true),
                    protocolo_cliente_legado_id = table.Column<int>(type: "integer", nullable: true),
                    protocolo_status = table.Column<int>(type: "integer", nullable: true),
                    competencia_inclusao_int = table.Column<int>(type: "integer", nullable: true),
                    situacao_proposta = table.Column<int>(type: "integer", nullable: true),
                    data_alteracao_situacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_processamento_funpresp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    possui_bonus_funpresp = table.Column<bool>(type: "boolean", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_proposta_ant = table.Column<int>(type: "integer", nullable: true),
                    legado_movimento_ini = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    legado_movimento_fim = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vigente = table.Column<bool>(type: "boolean", nullable: false),
                    visivel_operacional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    proposta_origem_id = table.Column<long>(type: "bigint", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_proposta_origem_id_fkey",
                        column: x => x.proposta_origem_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_status_id_fkey",
                        column: x => x.status_id,
                        principalSchema: "seguro",
                        principalTable: "proposta_status",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "produto",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tabela_preco_id = table.Column<long>(type: "bigint", nullable: true),
                    plano_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    codigo_referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ramo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    gera_conjuge = table.Column<bool>(type: "boolean", nullable: true),
                    paga_comissao = table.Column<bool>(type: "boolean", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_produto_ant = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("produto_pkey", x => x.id);
                    table.ForeignKey(
                        name: "produto_plano_id_fkey",
                        column: x => x.plano_id,
                        principalSchema: "seguro",
                        principalTable: "plano",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "produto_tabela_preco_id_fkey",
                        column: x => x.tabela_preco_id,
                        principalSchema: "seguro",
                        principalTable: "tabela_preco",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "proposta_beneficiario",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nome_normalizado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cpf_original = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    parentesco_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parentesco_normalizado = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    percentual_participacao = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_beneficiario_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_beneficiario_proposta_id_fkey",
                        column: x => x.proposta_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "proposta_historico",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_anterior_id = table.Column<long>(type: "bigint", nullable: false),
                    proposta_nova_id = table.Column<long>(type: "bigint", nullable: false),
                    motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    legado_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_historico_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_historico_proposta_anterior_id_fkey",
                        column: x => x.proposta_anterior_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_historico_proposta_nova_id_fkey",
                        column: x => x.proposta_nova_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "proposta_movimento",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    convenio_cobranca_id = table.Column<long>(type: "bigint", nullable: true),
                    movimento_tipo_id = table.Column<long>(type: "bigint", nullable: true),
                    classificacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'avaliar'::character varying"),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_lancamento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    dia = table.Column<int>(type: "integer", nullable: true),
                    mes = table.Column<int>(type: "integer", nullable: true),
                    ano = table.Column<int>(type: "integer", nullable: true),
                    competencia_int = table.Column<int>(type: "integer", nullable: true),
                    premio_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_diferenca = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_total_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_fatura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    iof = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    comissao_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    comissao_liquida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    comissao_bruta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    situacao_codigo = table.Column<int>(type: "integer", nullable: true),
                    situacao_descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gerado = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    comissao_gerado = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    titulo_gerado = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    parcela = table.Column<int>(type: "integer", nullable: true),
                    sequencia = table.Column<int>(type: "integer", nullable: true),
                    data_vencimento_fatura = table.Column<DateOnly>(type: "date", nullable: true),
                    data_recebimento_fatura = table.Column<DateOnly>(type: "date", nullable: true),
                    id_fatura_cartao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cobrar_na_fatura = table.Column<bool>(type: "boolean", nullable: true),
                    usuario_cobrador_legado_id = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_mov_ant = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_movimento_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_movimento_movimento_tipo_id_fkey",
                        column: x => x.movimento_tipo_id,
                        principalSchema: "seguro",
                        principalTable: "movimento_tipo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_movimento_proposta_id_fkey",
                        column: x => x.proposta_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "proposta_item",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_produto_id = table.Column<long>(type: "bigint", nullable: true),
                    tabela_preco_id = table.Column<long>(type: "bigint", nullable: true),
                    produto_id = table.Column<long>(type: "bigint", nullable: true),
                    plano_id = table.Column<long>(type: "bigint", nullable: true),
                    plano_codigo_legado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    plano_nome_legado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ramo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    paga_comissao = table.Column<bool>(type: "boolean", nullable: true),
                    codigo_legado = table.Column<int>(type: "integer", nullable: true),
                    cd_mov_vid = table.Column<int>(type: "integer", nullable: true),
                    ultima_faixa_etaria = table.Column<int>(type: "integer", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_proposta_tipo_ant = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_item_plano_id_fkey",
                        column: x => x.plano_id,
                        principalSchema: "seguro",
                        principalTable: "plano",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_item_produto_id_fkey",
                        column: x => x.produto_id,
                        principalSchema: "seguro",
                        principalTable: "produto",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_item_proposta_id_fkey",
                        column: x => x.proposta_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_item_tabela_preco_id_fkey",
                        column: x => x.tabela_preco_id,
                        principalSchema: "seguro",
                        principalTable: "tabela_preco",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_item_tipo_produto_id_fkey",
                        column: x => x.tipo_produto_id,
                        principalSchema: "seguro",
                        principalTable: "tipo_produto",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "proposta_cobertura",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    proposta_item_id = table.Column<long>(type: "bigint", nullable: true),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: true),
                    premio_titular = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_conjuge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    basica = table.Column<bool>(type: "boolean", nullable: true),
                    cobertura_nome_legado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_proposta_cobertura_ant = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_cobertura_pkey", x => x.id);
                    table.ForeignKey(
                        name: "proposta_cobertura_cobertura_id_fkey",
                        column: x => x.cobertura_id,
                        principalSchema: "seguro",
                        principalTable: "cobertura",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_cobertura_proposta_id_fkey",
                        column: x => x.proposta_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "proposta_cobertura_proposta_item_id_fkey",
                        column: x => x.proposta_item_id,
                        principalSchema: "seguro",
                        principalTable: "proposta_item",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_cobertura_nome_trgm",
                schema: "seguro",
                table: "cobertura",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_cobertura_legado",
                schema: "seguro",
                table: "cobertura",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_tipo_classificacao",
                schema: "seguro",
                table: "movimento_tipo",
                column: "classificacao");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_tipo_financeiro",
                schema: "seguro",
                table: "movimento_tipo",
                column: "financeiro");

            migrationBuilder.CreateIndex(
                name: "ux_movimento_tipo_legado",
                schema: "seguro",
                table: "movimento_tipo",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_plano_nome_trgm",
                schema: "seguro",
                table: "plano",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_plano_ramo",
                schema: "seguro",
                table: "plano",
                column: "ramo");

            migrationBuilder.CreateIndex(
                name: "ux_plano_legado",
                schema: "seguro",
                table: "plano",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_produto_codigo_referencia",
                schema: "seguro",
                table: "produto",
                column: "codigo_referencia");

            migrationBuilder.CreateIndex(
                name: "ix_produto_plano",
                schema: "seguro",
                table: "produto",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_tabela_preco",
                schema: "seguro",
                table: "produto",
                column: "tabela_preco_id");

            migrationBuilder.CreateIndex(
                name: "ux_produto_legado",
                schema: "seguro",
                table: "produto",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cliente",
                schema: "seguro",
                table: "proposta",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cliente_vinculo",
                schema: "seguro",
                table: "proposta",
                column: "cliente_vinculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_data_inclusao",
                schema: "seguro",
                table: "proposta",
                column: "data_inclusao");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_estipulante",
                schema: "seguro",
                table: "proposta",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_estipulante_status",
                schema: "seguro",
                table: "proposta",
                columns: new[] { "estipulante_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "ix_proposta_numero",
                schema: "seguro",
                table: "proposta",
                column: "numero");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_pessoa",
                schema: "seguro",
                table: "proposta",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_proposta_proposta_origem_id",
                schema: "seguro",
                table: "proposta",
                column: "proposta_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_status",
                schema: "seguro",
                table: "proposta",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_vigente",
                schema: "seguro",
                table: "proposta",
                column: "vigente");

            migrationBuilder.CreateIndex(
                name: "ux_proposta_legado",
                schema: "seguro",
                table: "proposta",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_cpf",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_nome_trgm",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "nome")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_parentesco",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "parentesco_normalizado");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_pessoa",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_proposta",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ux_proposta_beneficiario_legado",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_cobertura",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_item",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "proposta_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_proposta",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ux_proposta_cobertura_legado",
                schema: "seguro",
                table: "proposta_cobertura",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_historico_anterior",
                schema: "seguro",
                table: "proposta_historico",
                column: "proposta_anterior_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_historico_nova",
                schema: "seguro",
                table: "proposta_historico",
                column: "proposta_nova_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_plano",
                schema: "seguro",
                table: "proposta_item",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_produto",
                schema: "seguro",
                table: "proposta_item",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_proposta",
                schema: "seguro",
                table: "proposta_item",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_tabela",
                schema: "seguro",
                table: "proposta_item",
                column: "tabela_preco_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_tipo",
                schema: "seguro",
                table: "proposta_item",
                column: "tipo_produto_id");

            migrationBuilder.CreateIndex(
                name: "ux_proposta_item_legado",
                schema: "seguro",
                table: "proposta_item",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_classificacao",
                schema: "seguro",
                table: "proposta_movimento",
                column: "classificacao");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_cliente",
                schema: "seguro",
                table: "proposta_movimento",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_competencia",
                schema: "seguro",
                table: "proposta_movimento",
                columns: new[] { "ano", "mes" });

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_competencia_int",
                schema: "seguro",
                table: "proposta_movimento",
                column: "competencia_int");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_data_pagamento",
                schema: "seguro",
                table: "proposta_movimento",
                column: "data_pagamento",
                filter: "(data_pagamento IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_estipulante",
                schema: "seguro",
                table: "proposta_movimento",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_proposta",
                schema: "seguro",
                table: "proposta_movimento",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_tipo",
                schema: "seguro",
                table: "proposta_movimento",
                column: "movimento_tipo_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_vinculo",
                schema: "seguro",
                table: "proposta_movimento",
                column: "cliente_vinculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_proposta_movimento_legado",
                schema: "seguro",
                table: "proposta_movimento",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "proposta_status_codigo_key",
                schema: "seguro",
                table: "proposta_status",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tabela_preco_legado",
                schema: "seguro",
                table: "tabela_preco",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_produto_nome",
                schema: "seguro",
                table: "tipo_produto",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "ux_tipo_produto_legado",
                schema: "seguro",
                table: "tipo_produto",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proposta_beneficiario",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_cobertura",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_historico",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_movimento",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "cobertura",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_item",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "movimento_tipo",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "produto",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "tipo_produto",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "plano",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "tabela_preco",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_status",
                schema: "seguro");
        }
    }
}
