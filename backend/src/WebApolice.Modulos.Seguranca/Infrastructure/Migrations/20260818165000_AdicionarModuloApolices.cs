using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20260818165000_AdicionarModuloApolices")]
    public partial class AdicionarModuloApolices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_modulo_id bigint;
    v_recurso_id bigint;
BEGIN
    INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
    VALUES ('81820611-0000-0000-0000-000000000001', 'Apólices', 'APOLICES', true, true, 20)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_modulo_id;

    INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo)
    VALUES ('81820611-0000-0000-0000-000000000002', v_modulo_id, 'Apólices', 'APOLICES', true)
    ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_recurso_id;

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
    VALUES 
        ('81820611-0000-0000-0000-000000000003', v_recurso_id, 'Visualizar', 'apolices.visualizar', true)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE recurso_id IN (SELECT id FROM seguranca.recurso WHERE codigo = 'APOLICES');
DELETE FROM seguranca.recurso WHERE codigo = 'APOLICES';
DELETE FROM seguranca.modulo WHERE codigo = 'APOLICES';
            ");
        }
    }
}
