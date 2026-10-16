using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations;

public partial class AdicionarRgClienteDependente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "rg", schema: "cadastro", table: "cliente_dependente", type: "text", nullable: true);
        migrationBuilder.AddColumn<string>(name: "orgao_rg", schema: "cadastro", table: "cliente_dependente", type: "text", nullable: true);
        migrationBuilder.AddColumn<DateOnly>(name: "data_emissao_rg", schema: "cadastro", table: "cliente_dependente", type: "date", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "rg", schema: "cadastro", table: "cliente_dependente");
        migrationBuilder.DropColumn(name: "orgao_rg", schema: "cadastro", table: "cliente_dependente");
        migrationBuilder.DropColumn(name: "data_emissao_rg", schema: "cadastro", table: "cliente_dependente");
    }
}
