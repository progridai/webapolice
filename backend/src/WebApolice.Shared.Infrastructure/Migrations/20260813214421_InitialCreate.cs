using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "legado");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "agenciador_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_agenciador_id = table.Column<int>(type: "integer", nullable: false),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cpf_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    legado_coordenador_id = table.Column<int>(type: "integer", nullable: true),
                    coordenador_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("agenciador_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agenciamento_corretora_lancamento_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_agenciamento_id = table.Column<int>(type: "integer", nullable: false),
                    agenciamento_corretora_lancamento_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_corretora_id = table.Column<int>(type: "integer", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_movimento_id = table.Column<int>(type: "integer", nullable: true),
                    movimento_tipo_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("agenciamento_corretora_lancamento_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cliente_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    cpf_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    matricula_original = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    criterio_unificacao_pessoa = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    criterio_criacao_vinculo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cliente_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cobertura_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_cobertura_id = table.Column<int>(type: "integer", nullable: false),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cobertura_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "corretora_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_corretora_id = table.Column<int>(type: "integer", nullable: false),
                    corretora_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("corretora_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documento_anexo_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_documento_id = table.Column<int>(type: "integer", nullable: false),
                    arquivo_id = table.Column<long>(type: "bigint", nullable: false),
                    titulo_original = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    tipo_anexo_original = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    extensao_original = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    arquivo_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    pk_cliente = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    pk_proposta = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    pk_sinistro = table.Column<int>(type: "integer", nullable: true),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: true),
                    pk_estipulante = table.Column<int>(type: "integer", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    pk_protocolo = table.Column<int>(type: "integer", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("documento_anexo_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estipulante_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    cnpj_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cnpj_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cnpj_valido = table.Column<bool>(type: "boolean", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    criterio_unificacao_pessoa = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("estipulante_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimento_proposta_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_movimento_proposta_id = table.Column<int>(type: "integer", nullable: false),
                    proposta_movimento_id = table.Column<long>(type: "bigint", nullable: false),
                    titulo_id = table.Column<long>(type: "bigint", nullable: true),
                    titulo_pagamento_id = table.Column<long>(type: "bigint", nullable: true),
                    titulo_retorno_bancario_id = table.Column<long>(type: "bigint", nullable: true),
                    lancamento_comissao_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_movimento_id = table.Column<int>(type: "integer", nullable: true),
                    movimento_tipo_id = table.Column<long>(type: "bigint", nullable: true),
                    classificacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("movimento_proposta_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plano_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_plano_id = table.Column<int>(type: "integer", nullable: false),
                    plano_id = table.Column<long>(type: "bigint", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("plano_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "produto_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_produto_id = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo_referencia_original = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("produto_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_beneficiario_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_beneficiario_id = table.Column<int>(type: "integer", nullable: false),
                    proposta_beneficiario_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cpf_original = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cpf_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf_valido = table.Column<bool>(type: "boolean", nullable: false),
                    parentesco_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parentesco_normalizado = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_beneficiario_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_cobertura_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_proposta_cobertura_id = table.Column<int>(type: "integer", nullable: false),
                    proposta_cobertura_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_proposta_tipo_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_item_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_cobertura_id = table.Column<int>(type: "integer", nullable: true),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_cobertura_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_item_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_proposta_tipo_id = table.Column<int>(type: "integer", nullable: false),
                    proposta_item_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_tipo_id = table.Column<int>(type: "integer", nullable: true),
                    tipo_produto_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_produto_id = table.Column<int>(type: "integer", nullable: true),
                    produto_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_plano_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    plano_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_tabela_id = table.Column<int>(type: "integer", nullable: true),
                    tabela_preco_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_item_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: false),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_subestipulante_id = table.Column<int>(type: "integer", nullable: true),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_status = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<short>(type: "smallint", nullable: true),
                    numero_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proposta_participante_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_participante_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    participante_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    codigo_legado_participante = table.Column<int>(type: "integer", nullable: true),
                    agenciador_id = table.Column<long>(type: "bigint", nullable: true),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    campo_origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("proposta_participante_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "protocolo_acompanhamento_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_acompanhamento_id = table.Column<int>(type: "integer", nullable: false),
                    protocolo_acompanhamento_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_protocolo_id = table.Column<int>(type: "integer", nullable: true),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_acompanhamento_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "protocolo_item_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    origem_legado = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    legado_cliente_protocolo_id = table.Column<int>(type: "integer", nullable: false),
                    protocolo_item_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_protocolo_id = table.Column<int>(type: "integer", nullable: true),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_item_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "protocolo_lote_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_protocolo_id = table.Column<int>(type: "integer", nullable: false),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_protocolo_original = table.Column<int>(type: "integer", nullable: true),
                    data_protocolo_original = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_lote_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sinistro_acompanhamento_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_acompanhamento_id = table.Column<int>(type: "integer", nullable: false),
                    acompanhamento_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_sinistro_id = table.Column<int>(type: "integer", nullable: true),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_acompanhamento_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sinistro_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_sinistro_id = table.Column<int>(type: "integer", nullable: false),
                    sinistro_id = table.Column<long>(type: "bigint", nullable: false),
                    legado_proposta_id = table.Column<int>(type: "integer", nullable: true),
                    proposta_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_status = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<short>(type: "smallint", nullable: true),
                    numero_sinistro_original = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    criterio_migracao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("sinistro_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tabela_preco_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_tabela_id = table.Column<int>(type: "integer", nullable: false),
                    tabela_preco_id = table.Column<long>(type: "bigint", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tabela_preco_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_produto_migration_map",
                schema: "legado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legado_tipo_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_produto_id = table.Column<long>(type: "bigint", nullable: false),
                    nome_original = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tipo_produto_migration_map_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_migration_map_agenciador_id",
                schema: "legado",
                table: "agenciador_migration_map",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_migration_map_cpf_limpo",
                schema: "legado",
                table: "agenciador_migration_map",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_migration_map_legado_agenciador_id",
                schema: "legado",
                table: "agenciador_migration_map",
                column: "legado_agenciador_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agenciador_migration_map_pessoa_id",
                schema: "legado",
                table: "agenciador_migration_map",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_migration_map_corretora_id",
                schema: "legado",
                table: "agenciamento_corretora_lancamento_migration_map",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_migration_map_legado_agen",
                schema: "legado",
                table: "agenciamento_corretora_lancamento_migration_map",
                column: "legado_agenciamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agenciamento_corretora_lancamento_migration_map_proposta_id",
                schema: "legado",
                table: "agenciamento_corretora_lancamento_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_migration_map_cliente_id",
                schema: "legado",
                table: "cliente_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_migration_map_cpf_limpo",
                schema: "legado",
                table: "cliente_migration_map",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_migration_map_legado_cliente_id",
                schema: "legado",
                table: "cliente_migration_map",
                column: "legado_cliente_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_migration_map_pessoa_id",
                schema: "legado",
                table: "cliente_migration_map",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cobertura_migration_map_legado_cobertura_id",
                schema: "legado",
                table: "cobertura_migration_map",
                column: "legado_cobertura_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_corretora_migration_map_corretora_id",
                schema: "legado",
                table: "corretora_migration_map",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_corretora_migration_map_legado_corretora_id",
                schema: "legado",
                table: "corretora_migration_map",
                column: "legado_corretora_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_arquivo_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "arquivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_cliente_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_estipulante_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_legado_documento_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "legado_documento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_proposta_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_anexo_migration_map_sinistro_id",
                schema: "legado",
                table: "documento_anexo_migration_map",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_migration_map_cnpj_limpo",
                schema: "legado",
                table: "estipulante_migration_map",
                column: "cnpj_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_migration_map_legado_estipulante_id",
                schema: "legado",
                table: "estipulante_migration_map",
                column: "legado_estipulante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estipulante_migration_map_pessoa_id",
                schema: "legado",
                table: "estipulante_migration_map",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_cliente_id",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_estipulante_id",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_legado_movimento_proposta_",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "legado_movimento_proposta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_proposta_id",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_proposta_movimento_id",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "proposta_movimento_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_proposta_migration_map_titulo_id",
                schema: "legado",
                table: "movimento_proposta_migration_map",
                column: "titulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_migration_map_legado_plano_id",
                schema: "legado",
                table: "plano_migration_map",
                column: "legado_plano_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_migration_map_legado_produto_id",
                schema: "legado",
                table: "produto_migration_map",
                column: "legado_produto_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_migration_map_cpf_limpo",
                schema: "legado",
                table: "proposta_beneficiario_migration_map",
                column: "cpf_limpo");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_migration_map_legado_beneficiario_id",
                schema: "legado",
                table: "proposta_beneficiario_migration_map",
                column: "legado_beneficiario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_migration_map_pessoa_id",
                schema: "legado",
                table: "proposta_beneficiario_migration_map",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_migration_map_proposta_id",
                schema: "legado",
                table: "proposta_beneficiario_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_migration_map_legado_proposta_cobertura_",
                schema: "legado",
                table: "proposta_cobertura_migration_map",
                column: "legado_proposta_cobertura_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_migration_map_proposta_id",
                schema: "legado",
                table: "proposta_cobertura_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_cobertura_migration_map_proposta_item_id",
                schema: "legado",
                table: "proposta_cobertura_migration_map",
                column: "proposta_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_migration_map_legado_proposta_tipo_id",
                schema: "legado",
                table: "proposta_item_migration_map",
                column: "legado_proposta_tipo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_migration_map_proposta_id",
                schema: "legado",
                table: "proposta_item_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_item_migration_map_proposta_item_id",
                schema: "legado",
                table: "proposta_item_migration_map",
                column: "proposta_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_migration_map_cliente_id",
                schema: "legado",
                table: "proposta_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_migration_map_cliente_vinculo_id",
                schema: "legado",
                table: "proposta_migration_map",
                column: "cliente_vinculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_migration_map_estipulante_id",
                schema: "legado",
                table: "proposta_migration_map",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_migration_map_legado_proposta_id",
                schema: "legado",
                table: "proposta_migration_map",
                column: "legado_proposta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_migration_map_proposta_id",
                schema: "legado",
                table: "proposta_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_migration_map_agenciador_id",
                schema: "legado",
                table: "proposta_participante_migration_map",
                column: "agenciador_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_migration_map_corretora_id",
                schema: "legado",
                table: "proposta_participante_migration_map",
                column: "corretora_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_participante_migration_map_proposta_id",
                schema: "legado",
                table: "proposta_participante_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_acompanhamento_migration_map_legado_acompanhament",
                schema: "legado",
                table: "protocolo_acompanhamento_migration_map",
                column: "legado_acompanhamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_acompanhamento_migration_map_protocolo_lote_id",
                schema: "legado",
                table: "protocolo_acompanhamento_migration_map",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_migration_map_cliente_id",
                schema: "legado",
                table: "protocolo_item_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_migration_map_origem_legado_legado_cliente_p",
                schema: "legado",
                table: "protocolo_item_migration_map",
                columns: new[] { "origem_legado", "legado_cliente_protocolo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_migration_map_protocolo_item_id",
                schema: "legado",
                table: "protocolo_item_migration_map",
                column: "protocolo_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_migration_map_protocolo_lote_id",
                schema: "legado",
                table: "protocolo_item_migration_map",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_lote_migration_map_legado_protocolo_id",
                schema: "legado",
                table: "protocolo_lote_migration_map",
                column: "legado_protocolo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_lote_migration_map_protocolo_lote_id",
                schema: "legado",
                table: "protocolo_lote_migration_map",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_acompanhamento_migration_map_legado_acompanhamento",
                schema: "legado",
                table: "sinistro_acompanhamento_migration_map",
                column: "legado_acompanhamento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_acompanhamento_migration_map_sinistro_id",
                schema: "legado",
                table: "sinistro_acompanhamento_migration_map",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_migration_map_cliente_id",
                schema: "legado",
                table: "sinistro_migration_map",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_migration_map_legado_sinistro_id",
                schema: "legado",
                table: "sinistro_migration_map",
                column: "legado_sinistro_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_migration_map_proposta_id",
                schema: "legado",
                table: "sinistro_migration_map",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinistro_migration_map_sinistro_id",
                schema: "legado",
                table: "sinistro_migration_map",
                column: "sinistro_id");

            migrationBuilder.CreateIndex(
                name: "ix_tabela_preco_migration_map_legado_tabela_id",
                schema: "legado",
                table: "tabela_preco_migration_map",
                column: "legado_tabela_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_produto_migration_map_legado_tipo_id",
                schema: "legado",
                table: "tipo_produto_migration_map",
                column: "legado_tipo_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agenciador_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "agenciamento_corretora_lancamento_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "cliente_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "cobertura_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "corretora_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "documento_anexo_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "estipulante_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "movimento_proposta_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "plano_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "produto_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "proposta_beneficiario_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "proposta_cobertura_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "proposta_item_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "proposta_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "proposta_participante_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "protocolo_acompanhamento_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "protocolo_item_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "protocolo_lote_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "sinistro_acompanhamento_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "sinistro_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "tabela_preco_migration_map",
                schema: "legado");

            migrationBuilder.DropTable(
                name: "tipo_produto_migration_map",
                schema: "legado");
        }
    }
}
