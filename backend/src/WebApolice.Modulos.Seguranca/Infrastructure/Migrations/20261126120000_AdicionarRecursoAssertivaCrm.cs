using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261126120000_AdicionarRecursoAssertivaCrm")]
public sealed class AdicionarRecursoAssertivaCrm : Migration
{
    private static readonly Guid RecursoId = Guid.Parse("dfbd1214-323b-4fac-a49e-4fb9764d067c");
    private static readonly Guid PermissaoId = Guid.Parse("7ba80b6c-b3eb-420f-8d23-ee2f81261e75");

    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql($"""
        DO $$
        DECLARE
            modulo_crm_id bigint;
            recurso_assertiva_id bigint;
        BEGIN
            SELECT id INTO STRICT modulo_crm_id
            FROM seguranca.modulo WHERE codigo = 'CRM';

            INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ordem, ativo, habilitado)
            VALUES ('{RecursoId}', modulo_crm_id, 'Assertiva', 'ASSERTIVA', 10, true, false)
            ON CONFLICT (public_id) DO NOTHING;

            SELECT id INTO STRICT recurso_assertiva_id
            FROM seguranca.recurso
            WHERE public_id = '{RecursoId}' AND modulo_id = modulo_crm_id AND codigo = 'ASSERTIVA';

            INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, descricao, ativo)
            VALUES ('{PermissaoId}', recurso_assertiva_id, 'Utilizar', 'crm.assertiva.utilizar',
                    'Permite utilizar a integração com a Assertiva no CRM.', true)
            ON CONFLICT (public_id) DO NOTHING;
        END $$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql($"""
        DELETE FROM seguranca.perfil_permissao pp
        USING seguranca.permissao p
        WHERE pp.permissao_id = p.id AND p.public_id = '{PermissaoId}';

        DELETE FROM seguranca.permissao WHERE public_id = '{PermissaoId}';
        DELETE FROM seguranca.recurso WHERE public_id = '{RecursoId}';
        """);
}
