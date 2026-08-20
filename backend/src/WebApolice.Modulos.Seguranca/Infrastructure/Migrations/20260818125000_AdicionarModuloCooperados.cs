using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20260818125000_AdicionarModuloCooperados")]
    public partial class AdicionarModuloCooperados : Migration
    {
        private readonly Guid ModuloId = new Guid("81820610-0000-0000-0000-000000000001");
        private readonly Guid RecursoId = new Guid("81820610-0000-0000-0000-000000000002");
        
        private readonly Guid PermVisId = new Guid("81820610-0000-0000-0000-000000000003");
        private readonly Guid PermInsId = new Guid("81820610-0000-0000-0000-000000000004");
        private readonly Guid PermAltId = new Guid("81820610-0000-0000-0000-000000000005");
        private readonly Guid PermInaId = new Guid("81820610-0000-0000-0000-000000000006");
        private readonly Guid PermReaId = new Guid("81820610-0000-0000-0000-000000000007");

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DO $$
DECLARE
    v_modulo_id bigint;
    v_recurso_id bigint;
BEGIN
    INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
    VALUES ('{ModuloId}', 'Cooperados', 'COOPERADOS', true, true, 15)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_modulo_id;

    INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo)
    VALUES ('{RecursoId}', v_modulo_id, 'Cooperados', 'COOPERADOS', true)
    ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_recurso_id;

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
    VALUES 
        ('{PermVisId}', v_recurso_id, 'Visualizar', 'cooperados.visualizar', true),
        ('{PermInsId}', v_recurso_id, 'Inserir', 'cooperados.inserir', true),
        ('{PermAltId}', v_recurso_id, 'Alterar', 'cooperados.alterar', true),
        ('{PermInaId}', v_recurso_id, 'Inativar', 'cooperados.inativar', true),
        ('{PermReaId}', v_recurso_id, 'Reativar', 'cooperados.reativar', true)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM seguranca.permissao WHERE public_id IN ('{PermVisId}', '{PermInsId}', '{PermAltId}', '{PermInaId}', '{PermReaId}');
DELETE FROM seguranca.recurso WHERE public_id = '{RecursoId}';
DELETE FROM seguranca.modulo WHERE public_id = '{ModuloId}';
            ");
        }
    }
}
