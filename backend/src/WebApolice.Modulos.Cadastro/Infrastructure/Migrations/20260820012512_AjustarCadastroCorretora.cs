using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustarCadastroCorretora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_corretora_pessoas_pessoa_id",
                schema: "cadastro",
                table: "corretora",
                column: "pessoa_id",
                principalSchema: "core",
                principalTable: "pessoa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(name: "nome", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "cidade_id", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "cep", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "logradouro", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "numero", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "complemento", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "bairro", schema: "cadastro", table: "corretora");
            migrationBuilder.DropColumn(name: "telefone", schema: "cadastro", table: "corretora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_corretora_pessoas_pessoa_id",
                schema: "cadastro",
                table: "corretora");

            migrationBuilder.AddColumn<string>(name: "nome", schema: "cadastro", table: "corretora", type: "character varying(150)", maxLength: 150, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<long>(name: "cidade_id", schema: "cadastro", table: "corretora", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "cep", schema: "cadastro", table: "corretora", type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "logradouro", schema: "cadastro", table: "corretora", type: "character varying(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "numero", schema: "cadastro", table: "corretora", type: "character varying(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "complemento", schema: "cadastro", table: "corretora", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "bairro", schema: "cadastro", table: "corretora", type: "character varying(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "telefone", schema: "cadastro", table: "corretora", type: "character varying(120)", maxLength: 120, nullable: true);
        }
    }
}
