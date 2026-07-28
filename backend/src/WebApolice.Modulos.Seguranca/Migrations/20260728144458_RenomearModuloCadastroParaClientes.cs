using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class RenomearModuloCadastroParaClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE seguranca.modulo SET codigo = 'CLIENTES', nome = 'Clientes', descricao = 'Cadastro e administração de clientes' WHERE codigo = 'CADASTRO';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE seguranca.modulo SET codigo = 'CADASTRO', nome = 'Cadastros', descricao = 'Módulo destinado às rotinas cadastrais do WebApólice' WHERE codigo = 'CLIENTES';");
        }
    }
}
