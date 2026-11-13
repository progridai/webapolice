using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261111190000_AdicionarPermissaoExcluirVida")]
public sealed class AdicionarPermissaoExcluirVida : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$
        DECLARE recurso_apolices bigint;
        BEGIN
          SELECT id INTO STRICT recurso_apolices FROM seguranca.recurso WHERE codigo = 'APOLICES';
          INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
          VALUES (gen_random_uuid(), recurso_apolices, 'Excluir Participação de Vida', 'apolices.vidas.excluir', true)
          ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
        END $$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM seguranca.permissao WHERE codigo = 'apolices.vidas.excluir';
        """);
}
