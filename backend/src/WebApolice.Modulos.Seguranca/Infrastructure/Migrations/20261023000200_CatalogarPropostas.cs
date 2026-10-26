using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261023000200_CatalogarPropostas")]
public sealed class CatalogarPropostas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Matriz aprovada em 23/10/2026. Não atribui permissões a perfis comuns.
        migrationBuilder.Sql("""
            DO $$ BEGIN
                IF NOT EXISTS (SELECT 1 FROM seguranca.modulo WHERE codigo = 'APOLICES') THEN
                    RAISE EXCEPTION 'Módulo APOLICES ausente: aplicar baseline de Segurança antes de Propostas.';
                END IF;
            END $$;
            INSERT INTO seguranca.recurso (public_id,modulo_id,codigo,nome,ordem,ativo,habilitado)
            SELECT 'de7b863b-0b22-49b5-a7f2-f030b426b871'::uuid,id,'PROPOSTAS','Propostas',20,true,true
            FROM seguranca.modulo WHERE codigo='APOLICES'
            ON CONFLICT (modulo_id,codigo) DO NOTHING;

            DO $$ BEGIN
                IF EXISTS (
                    SELECT 1 FROM seguranca.permissao p
                    JOIN seguranca.recurso r ON r.id=p.recurso_id
                    JOIN seguranca.modulo m ON m.id=r.modulo_id
                    WHERE p.codigo IN ('propostas.visualizar','propostas.inserir','propostas.alterar')
                      AND (m.codigo <> 'APOLICES' OR r.codigo <> 'PROPOSTAS')
                ) THEN
                    RAISE EXCEPTION 'Catálogo de Propostas conflitante: revisar associação existente antes da migration.';
                END IF;
            END $$;

            INSERT INTO seguranca.permissao (public_id,recurso_id,codigo,nome,ativo)
            SELECT v.public_id::uuid,r.id,v.codigo,v.nome,true
            FROM seguranca.recurso r JOIN seguranca.modulo m ON m.id=r.modulo_id
            CROSS JOIN (VALUES
                ('d1a14918-5ed6-49e0-9991-0167e419b6eb','propostas.visualizar','Visualizar Propostas'),
                ('56f6fb70-c014-4b65-8a6e-90feafc9b39d','propostas.inserir','Inserir Propostas'),
                ('b2fd7c8f-fcf2-44c8-966c-0bbc99ae790e','propostas.alterar','Alterar Propostas')
            ) v(public_id,codigo,nome)
            WHERE m.codigo='APOLICES' AND r.codigo='PROPOSTAS'
            ON CONFLICT (codigo) DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM seguranca.permissao WHERE public_id IN (
                'd1a14918-5ed6-49e0-9991-0167e419b6eb','56f6fb70-c014-4b65-8a6e-90feafc9b39d','b2fd7c8f-fcf2-44c8-966c-0bbc99ae790e');
            DELETE FROM seguranca.recurso WHERE public_id='de7b863b-0b22-49b5-a7f2-f030b426b871';
            """);
    }
}
