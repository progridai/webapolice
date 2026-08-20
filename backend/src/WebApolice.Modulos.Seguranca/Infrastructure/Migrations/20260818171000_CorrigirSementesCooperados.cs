using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20260818171000_CorrigirSementesCooperados")]
    public partial class CorrigirSementesCooperados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O catálogo técnico de segurança não deve sobrescrever definições 
            // administrativas mutáveis (como 'ativo', 'habilitado' e 'ordem').
            // Por isso, utilizamos UPSERT que atualiza estritamente o 'nome' e garante idempotência.
            
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_modulo_id bigint;
    v_recurso_id bigint;
BEGIN
    INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
    VALUES ('81820610-0000-0000-0000-000000000001', 'Cooperados', 'COOPERADOS', true, true, 15)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_modulo_id;

    INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo)
    VALUES ('81820610-0000-0000-0000-000000000002', v_modulo_id, 'Cooperados', 'COOPERADOS', true)
    ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_recurso_id;

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
    VALUES 
        ('81820610-0000-0000-0000-000000000003', v_recurso_id, 'Visualizar', 'cooperados.visualizar', true),
        ('81820610-0000-0000-0000-000000000004', v_recurso_id, 'Inserir', 'cooperados.inserir', true),
        ('81820610-0000-0000-0000-000000000005', v_recurso_id, 'Alterar', 'cooperados.alterar', true),
        ('81820610-0000-0000-0000-000000000006', v_recurso_id, 'Inativar', 'cooperados.inativar', true),
        ('81820610-0000-0000-0000-000000000007', v_recurso_id, 'Reativar', 'cooperados.reativar', true)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Como é uma migration corretiva de idempotência, não faz sentido deletar os registros
            // no down, pois eles pertencem à migration original (20260818125000_AdicionarModuloCooperados).
            // Deixamos vazio.
        }
    }
}
