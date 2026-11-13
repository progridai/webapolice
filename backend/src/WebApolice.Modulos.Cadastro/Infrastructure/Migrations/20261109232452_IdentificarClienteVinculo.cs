using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations;

public partial class IdentificarClienteVinculo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // As demais propriedades adicionadas ao modelo já existem desde a migration inicial.
        migrationBuilder.AddColumn<Guid>(name: "public_id", schema: "cadastro", table: "cliente_vinculo",
            type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()");
        migrationBuilder.CreateIndex(name: "ix_cliente_vinculo_public_id", schema: "cadastro", table: "cliente_vinculo", column: "public_id", unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_cliente_vinculo_public_id", schema: "cadastro", table: "cliente_vinculo");
        migrationBuilder.DropColumn(name: "public_id", schema: "cadastro", table: "cliente_vinculo");
    }
}
