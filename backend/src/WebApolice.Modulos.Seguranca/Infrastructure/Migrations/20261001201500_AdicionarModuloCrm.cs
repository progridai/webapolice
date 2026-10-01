using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20261001201500_AdicionarModuloCrm")]
    public partial class AdicionarModuloCrm : Migration
    {
        // UUIDs fixos — garantem idempotência entre ambientes (dev, staging, produção).
        // Prefixo 20261001 identifica a data desta migration.
        private readonly Guid ModuloId    = new Guid("20261001-0000-0000-0000-000000000001");
        private readonly Guid RecursoId   = new Guid("20261001-0000-0000-0000-000000000002");
        private readonly Guid PermAcessarId = new Guid("20261001-0000-0000-0001-000000000001");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DO $$
DECLARE
    v_modulo_id  bigint;
    v_recurso_id bigint;
BEGIN
    -- Módulo CRM
    -- ordem 50: posicionado após os módulos operacionais internos do WebApólice.
    -- habilitado=false: módulo externo — não exibe menu no WebApólice, mas
    -- permanece ativo no catálogo para que a permissão possa ser atribuída a perfis.
    INSERT INTO seguranca.modulo (public_id, nome, codigo, descricao, ativo, habilitado, ordem)
    VALUES (
        '{ModuloId}',
        'CRM',
        'CRM',
        'Sistema CRM Progrid+ — acesso controlado pelo catálogo de segurança do WebApólice.',
        true,
        false,
        50
    )
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome, descricao = EXCLUDED.descricao
    RETURNING id INTO v_modulo_id;

    -- Recurso CRM (recurso raiz do módulo)
    INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo)
    VALUES ('{RecursoId}', v_modulo_id, 'CRM', 'CRM', true)
    ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome
    RETURNING id INTO v_recurso_id;

    -- Permissão de entrada: crm.acessar
    -- Não atribuída a nenhum perfil nesta migration.
    -- O acesso deve ser concedido pelo mecanismo de perfis/permissões existente.
    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, descricao, ativo)
    VALUES (
        '{PermAcessarId}',
        v_recurso_id,
        'Acessar',
        'crm.acessar',
        'Permite o acesso ao sistema CRM Progrid+.',
        true
    )
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome, descricao = EXCLUDED.descricao;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM seguranca.permissao WHERE public_id = '{PermAcessarId}';
DELETE FROM seguranca.recurso    WHERE public_id = '{RecursoId}';
DELETE FROM seguranca.modulo     WHERE public_id = '{ModuloId}';
            ");
        }
    }
}
