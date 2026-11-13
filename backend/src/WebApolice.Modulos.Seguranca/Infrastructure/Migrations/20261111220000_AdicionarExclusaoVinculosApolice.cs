using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations;

[DbContext(typeof(SegurancaDbContext))]
[Migration("20261111220000_AdicionarExclusaoVinculosApolice")]
public sealed class AdicionarExclusaoVinculosApolice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$ DECLARE recurso_apolices bigint; BEGIN
          SELECT id INTO STRICT recurso_apolices FROM seguranca.recurso WHERE codigo = 'APOLICES';
          INSERT INTO seguranca.permissao(public_id,recurso_id,nome,codigo,ativo) VALUES
            (gen_random_uuid(),recurso_apolices,'Excluir Ramos da Apólice','apolices.ramos.excluir',true),
            (gen_random_uuid(),recurso_apolices,'Excluir Subgrupos da Apólice','apolices.subgrupos.excluir',true),
            (gen_random_uuid(),recurso_apolices,'Excluir Subestipulantes da Apólice','apolices.subestipulantes.excluir',true),
            (gen_random_uuid(),recurso_apolices,'Excluir Módulos e Subvínculos da Apólice','apolices.modulos.excluir',true),
            (gen_random_uuid(),recurso_apolices,'Excluir Vínculos do Universo Anterior','apolices.universo.excluir',true)
          ON CONFLICT(codigo) DO UPDATE SET nome=EXCLUDED.nome;
        END $$;
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE FROM seguranca.permissao WHERE codigo IN ('apolices.ramos.excluir','apolices.subgrupos.excluir','apolices.subestipulantes.excluir','apolices.modulos.excluir','apolices.universo.excluir');
        """);
}
