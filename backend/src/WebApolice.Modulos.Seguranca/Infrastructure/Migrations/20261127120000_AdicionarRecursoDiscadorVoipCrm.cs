using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261127120000_AdicionarRecursoDiscadorVoipCrm")]
public sealed class AdicionarRecursoDiscadorVoipCrm : Migration
{
    private static readonly Guid RecursoId = Guid.Parse("e5852e1d-5695-418c-814f-cca02b169f61");
    private static readonly Guid PermissaoId = Guid.Parse("cf094d8e-32a7-4dce-8889-331d4ec254dc");

    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql($"""
        DO $$
        DECLARE
            modulo_crm_id bigint;
            recurso_discador_id bigint;
        BEGIN
            SELECT id INTO STRICT modulo_crm_id
            FROM seguranca.modulo WHERE codigo = 'CRM';

            INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ordem, ativo, habilitado)
            VALUES ('{RecursoId}', modulo_crm_id, 'Discador VoIP', 'DISCADOR_VOIP', 20, true, false)
            ON CONFLICT (public_id) DO NOTHING;

            SELECT id INTO STRICT recurso_discador_id
            FROM seguranca.recurso
            WHERE public_id = '{RecursoId}' AND modulo_id = modulo_crm_id AND codigo = 'DISCADOR_VOIP';

            INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, descricao, ativo)
            VALUES ('{PermissaoId}', recurso_discador_id, 'Utilizar', 'crm.discador_voip.utilizar',
                    'Permite utilizar o Discador VoIP no CRM.', true)
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
