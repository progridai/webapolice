using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    public partial class RefatorarSeguradoraPessoa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Criar as pessoas correspondentes para as seguradoras existentes
            migrationBuilder.Sql(@"
                INSERT INTO core.pessoa (tipo_pessoa, nome, documento_principal, documento_principal_limpo, documento_valido, created_at, updated_at)
                SELECT 
                    2, -- Pessoa Jurídica
                    COALESCE(nome, 'Seguradora Sem Nome'),
                    cnpj,
                    cnpj_limpo,
                    CASE WHEN cnpj_limpo IS NOT NULL THEN TRUE ELSE FALSE END,
                    NOW(),
                    NOW()
                FROM cadastro.seguradora
                WHERE deleted_at IS NULL;
            ");

            // 2. Atualizar pessoa_id nas seguradoras associando com o CNPJ limpo
            migrationBuilder.Sql(@"
                UPDATE cadastro.seguradora s
                SET pessoa_id = p.id
                FROM core.pessoa p
                WHERE p.documento_principal_limpo = s.cnpj_limpo
                  AND s.cnpj_limpo IS NOT NULL
                  AND s.deleted_at IS NULL;
            ");

            // 2.1 Para as seguradoras sem CNPJ, associar pelo nome
            migrationBuilder.Sql(@"
                UPDATE cadastro.seguradora s
                SET pessoa_id = p.id
                FROM core.pessoa p
                WHERE p.nome = s.nome
                  AND s.cnpj_limpo IS NULL
                  AND s.deleted_at IS NULL
                  AND s.pessoa_id IS NULL;
            ");

            // 3. Garantir que todas as seguradoras ativas tenham pessoa_id
            // (Para não quebrar a FK restrita. Seguradoras excluídas que ficarem null podem ser tratadas depois)
            migrationBuilder.Sql(@"
                DELETE FROM cadastro.seguradora WHERE pessoa_id IS NULL AND deleted_at IS NULL;
            ");

            // 4. Agora podemos criar a FK, alterar pessoa_id para NOT NULL e dropar colunas
            migrationBuilder.AlterColumn<long>(
                name: "pessoa_id",
                schema: "cadastro",
                table: "seguradora",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_seguradora_pessoa_pessoa_id",
                schema: "cadastro",
                table: "seguradora",
                column: "pessoa_id",
                principalSchema: "core",
                principalTable: "pessoa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Remover campos legados
            migrationBuilder.DropColumn(
                name: "nome",
                schema: "cadastro",
                table: "seguradora");

            migrationBuilder.DropColumn(
                name: "cnpj",
                schema: "cadastro",
                table: "seguradora");

            migrationBuilder.DropColumn(
                name: "cnpj_limpo",
                schema: "cadastro",
                table: "seguradora");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Adicionar colunas de volta
            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "cadastro",
                table: "seguradora",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cnpj",
                schema: "cadastro",
                table: "seguradora",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cnpj_limpo",
                schema: "cadastro",
                table: "seguradora",
                type: "text",
                nullable: true);

            // Reverter os dados a partir da pessoa
            migrationBuilder.Sql(@"
                UPDATE cadastro.seguradora s
                SET nome = p.nome,
                    cnpj = p.documento_principal,
                    cnpj_limpo = p.documento_principal_limpo
                FROM core.pessoa p
                WHERE p.id = s.pessoa_id;
            ");

            migrationBuilder.DropForeignKey(
                name: "fk_seguradora_pessoa_pessoa_id",
                schema: "cadastro",
                table: "seguradora");

            migrationBuilder.AlterColumn<long>(
                name: "pessoa_id",
                schema: "cadastro",
                table: "seguradora",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
