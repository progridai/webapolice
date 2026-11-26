using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261126130000_AdicionarPermissaoInserirUsuarios")]
public sealed class AdicionarPermissaoInserirUsuarios : Migration
{
    private static readonly Guid PermissaoId = Guid.Parse("81820602-0001-0000-0001-000000000004");

    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql($"""
        DO $$
        DECLARE
            v_recurso_usuarios_id bigint;
        BEGIN
            SELECT r.id INTO STRICT v_recurso_usuarios_id
            FROM seguranca.recurso r
            INNER JOIN seguranca.modulo m ON m.id = r.modulo_id
            WHERE m.codigo = 'SEGURANCA'
              AND r.codigo IN ('SEGURANCA_USUARIOS', 'USUARIOS')
            ORDER BY CASE WHEN r.codigo = 'SEGURANCA_USUARIOS' THEN 0 ELSE 1 END
            LIMIT 1;

            INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, descricao, ativo)
            VALUES (
                '{PermissaoId}',
                v_recurso_usuarios_id,
                'Inserir',
                'seguranca.usuarios.inserir',
                'Permite cadastrar novos usuários e definir senha temporária',
                true
            )
            ON CONFLICT (codigo) DO UPDATE SET
                nome = EXCLUDED.nome,
                descricao = COALESCE(seguranca.permissao.descricao, EXCLUDED.descricao);
        END $$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM seguranca.perfil_permissao pp
        USING seguranca.permissao p
        WHERE pp.permissao_id = p.id
          AND p.codigo = 'seguranca.usuarios.inserir';

        DELETE FROM seguranca.permissao
        WHERE codigo = 'seguranca.usuarios.inserir';
        """);
}
