using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Comissao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "comissao");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "agenciador_comissao_config",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: false),
                    percentual_padrao = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    percentual_repasse = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, defaultValueSql: "'legado'::character varying"),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("agenciador_comissao_config_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agenciamento_corretora_lancamento",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    movimento_tipo_id = table.Column<long>(type: "bigint", nullable: true),
                    percentual = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    valor_premio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_agenciamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    parcela_inicial = table.Column<int>(type: "integer", nullable: true),
                    parcela_final = table.Column<int>(type: "integer", nullable: true),
                    status_legado = table.Column<int>(type: "integer", nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    gerou_fatura = table.Column<bool>(type: "boolean", nullable: true),
                    data_cadastro = table.Column<DateOnly>(type: "date", nullable: true),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    legado_corretora_id = table.Column<int>(type: "integer", nullable: true),
                    legado_movimento_id = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("agenciamento_corretora_lancamento_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "corretora_agenciador",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: true),
                    percentual_agenciamento = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    percentual_repasse = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("corretora_agenciador_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estipulante_comissao_config",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    percentual_comissao = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    percentual_agenciamento = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    percentual_bonus = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    comissao_apartir_parcela = table.Column<int>(type: "integer", nullable: true),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: true),
                    agenciador_percentual_repasse = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("estipulante_comissao_config_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatura_comissao_resumo",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    mes = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ano = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    competencia_int = table.Column<int>(type: "integer", nullable: true),
                    premio_pagamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("fatura_comissao_resumo_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatura_integracao",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    seguradora_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_codigo_original = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    seguradora_codigo_original = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    data_lancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: true),
                    data_recebimento = table.Column<DateOnly>(type: "date", nullable: true),
                    valor_receber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_fatura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    situacao_legado = table.Column<int>(type: "integer", nullable: true),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mes = table.Column<int>(type: "integer", nullable: true),
                    ano = table.Column<int>(type: "integer", nullable: true),
                    competencia_int = table.Column<int>(type: "integer", nullable: true),
                    gerou_arquivo = table.Column<bool>(type: "boolean", nullable: true),
                    alterado = table.Column<int>(type: "integer", nullable: true),
                    percentual_agenciamento = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    percentual_corretagem = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("fatura_integracao_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatura_vida_agenciamento",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    origem_legado = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    premio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    iof = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    premio_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_agenciamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_recebido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_diferenca = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    codigo_cooperado_original = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    codigo_corretora_original = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tipo_agenciamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    numero_nf = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    data_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("fatura_vida_agenciamento_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lancamento_comissao",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_movimento_id = table.Column<long>(type: "bigint", nullable: true),
                    titulo_id = table.Column<long>(type: "bigint", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    competencia_ano = table.Column<int>(type: "integer", nullable: true),
                    competencia_mes = table.Column<int>(type: "integer", nullable: true),
                    competencia_int = table.Column<int>(type: "integer", nullable: true),
                    valor_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    gerado = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'pendente'::character varying"),
                    origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "'movimento_proposta_legado'::character varying"),
                    legado_movimento_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("lancamento_comissao_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lancamento_fatura_estipulante",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    competencia_original = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    competencia_mes = table.Column<int>(type: "integer", nullable: true),
                    competencia_ano = table.Column<int>(type: "integer", nullable: true),
                    competencia_int = table.Column<int>(type: "integer", nullable: true),
                    premio_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_faturado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_corretagem = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    comissao_recebida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_vencimento_fatura = table.Column<DateOnly>(type: "date", nullable: true),
                    data_recebimento = table.Column<DateOnly>(type: "date", nullable: true),
                    lancamento_manual = table.Column<bool>(type: "boolean", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    legado_corretora_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("lancamento_fatura_estipulante_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_participante",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    participante_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    participante_id = table.Column<long>(type: "bigint", nullable: true),
                    codigo_agenciamento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    percentual_agenciamento = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    agenciamento_parcela_inicial = table.Column<int>(type: "integer", nullable: true),
                    agenciamento_parcela_final = table.Column<int>(type: "integer", nullable: true),
                    bonus = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_carteira = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    carteira_parcela_inicial = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_campo_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    codigo_legado_participante = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_participante_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatura_vida_recebimento",
                schema: "comissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fatura_vida_agenciamento_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_fatura_vida_id = table.Column<int>(type: "integer", nullable: true),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("fatura_vida_recebimento_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fatura_vida_recebimento_fatura_vida_agenciamento_id_fkey",
                        column: x => x.fatura_vida_agenciamento_id,
                        principalSchema: "comissao",
                        principalTable: "fatura_vida_agenciamento",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_comissao_config_agenciador",
                schema: "comissao",
                table: "agenciador_comissao_config",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_comissao_config_vigencia",
                schema: "comissao",
                table: "agenciador_comissao_config",
                columns: new[] { "inicio_vigencia", "fim_vigencia" });

            migrationBuilder.CreateIndex(
                name: "ux_agenciador_comissao_config_legado",
                schema: "comissao",
                table: "agenciador_comissao_config",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_corretora",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_movimento",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "movimento_tipo_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_pagamento",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "data_pagamento",
                filter: "(data_pagamento IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_proposta",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_vencimento",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "data_vencimento");

            migrationBuilder.CreateIndex(
                name: "ux_agenciamento_corretora_lancamento_legado",
                schema: "comissao",
                table: "agenciamento_corretora_lancamento",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_corretora_agenciador_agenciador",
                schema: "comissao",
                table: "corretora_agenciador",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ix_corretora_agenciador_corretora",
                schema: "comissao",
                table: "corretora_agenciador",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ux_corretora_agenciador_legado",
                schema: "comissao",
                table: "corretora_agenciador",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_comissao_agenciador",
                schema: "comissao",
                table: "estipulante_comissao_config",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ux_estipulante_comissao_config",
                schema: "comissao",
                table: "estipulante_comissao_config",
                column: "estipulante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_comissao_resumo_competencia",
                schema: "comissao",
                table: "fatura_comissao_resumo",
                column: "competencia_int");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_comissao_resumo_estipulante",
                schema: "comissao",
                table: "fatura_comissao_resumo",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ux_fatura_comissao_resumo_legado",
                schema: "comissao",
                table: "fatura_comissao_resumo",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_competencia",
                schema: "comissao",
                table: "fatura_integracao",
                columns: new[] { "ano", "mes" });

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_corretora",
                schema: "comissao",
                table: "fatura_integracao",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_estipulante",
                schema: "comissao",
                table: "fatura_integracao",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_seguradora",
                schema: "comissao",
                table: "fatura_integracao",
                column: "seguradora_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_tipo_situacao",
                schema: "comissao",
                table: "fatura_integracao",
                columns: new[] { "tipo", "situacao_legado" });

            migrationBuilder.CreateIndex(
                name: "ix_fatura_integracao_vencimento",
                schema: "comissao",
                table: "fatura_integracao",
                column: "data_vencimento");

            migrationBuilder.CreateIndex(
                name: "ux_fatura_integracao_legado",
                schema: "comissao",
                table: "fatura_integracao",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_agenciamento_data_inclusao",
                schema: "comissao",
                table: "fatura_vida_agenciamento",
                column: "data_inclusao");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_agenciamento_origem",
                schema: "comissao",
                table: "fatura_vida_agenciamento",
                column: "origem_legado");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_agenciamento_proposta",
                schema: "comissao",
                table: "fatura_vida_agenciamento",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ux_fatura_vida_agenciamento_legado",
                schema: "comissao",
                table: "fatura_vida_agenciamento",
                columns: new[] { "origem_legado", "legado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_recebimento_data",
                schema: "comissao",
                table: "fatura_vida_recebimento",
                column: "data_pagamento");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_recebimento_estipulante",
                schema: "comissao",
                table: "fatura_vida_recebimento",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_vida_recebimento_fatura_vida",
                schema: "comissao",
                table: "fatura_vida_recebimento",
                column: "fatura_vida_agenciamento_id");

            migrationBuilder.CreateIndex(
                name: "ux_fatura_vida_recebimento_legado",
                schema: "comissao",
                table: "fatura_vida_recebimento",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_comissao_competencia",
                schema: "comissao",
                table: "lancamento_comissao",
                columns: new[] { "competencia_ano", "competencia_mes" });

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_comissao_movimento",
                schema: "comissao",
                table: "lancamento_comissao",
                column: "proposta_movimento_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_comissao_proposta",
                schema: "comissao",
                table: "lancamento_comissao",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_comissao_titulo",
                schema: "comissao",
                table: "lancamento_comissao",
                column: "titulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_fatura_estipulante_competencia",
                schema: "comissao",
                table: "lancamento_fatura_estipulante",
                column: "competencia_int");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_fatura_estipulante_corretora",
                schema: "comissao",
                table: "lancamento_fatura_estipulante",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamento_fatura_estipulante_estipulante",
                schema: "comissao",
                table: "lancamento_fatura_estipulante",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ux_lancamento_fatura_estipulante_legado",
                schema: "comissao",
                table: "lancamento_fatura_estipulante",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_agenciador",
                schema: "comissao",
                table: "proposta_participante",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_codigo_legado",
                schema: "comissao",
                table: "proposta_participante",
                column: "codigo_legado_participante");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_corretora",
                schema: "comissao",
                table: "proposta_participante",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_proposta",
                schema: "comissao",
                table: "proposta_participante",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_tipo",
                schema: "comissao",
                table: "proposta_participante",
                column: "participante_tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agenciador_comissao_config",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "agenciamento_corretora_lancamento",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "corretora_agenciador",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "estipulante_comissao_config",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "fatura_comissao_resumo",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "fatura_integracao",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "fatura_vida_recebimento",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "lancamento_comissao",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "lancamento_fatura_estipulante",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "proposta_participante",
                schema: "comissao");

            migrationBuilder.DropTable(
                name: "fatura_vida_agenciamento",
                schema: "comissao");
        }
    }
}
