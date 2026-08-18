using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    [DbContext(typeof(SegurancaDbContext))]
    [Migration("20260818125000_AdicionarModuloCooperados")]
    public partial class AdicionarModuloCooperados : Migration
    {
        private readonly Guid ModuloId = new Guid("81820610-0000-0000-0000-000000000001");
        private readonly Guid RecursoId = new Guid("81820610-0000-0000-0000-000000000002");
        
        private readonly Guid PermVisId = new Guid("81820610-0000-0000-0000-000000000003");
        private readonly Guid PermInsId = new Guid("81820610-0000-0000-0000-000000000004");
        private readonly Guid PermAltId = new Guid("81820610-0000-0000-0000-000000000005");
        private readonly Guid PermInaId = new Guid("81820610-0000-0000-0000-000000000006");
        private readonly Guid PermReaId = new Guid("81820610-0000-0000-0000-000000000007");

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Modulo
            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "modulo",
                columns: new[] { "id", "nome", "codigo", "ativo", "habilitado", "ordem" },
                values: new object[] { ModuloId, "Cooperados", "COOPERADOS", true, true, 15 }
            );

            // Recurso
            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "recurso",
                columns: new[] { "id", "modulo_id", "nome", "codigo", "ativo" },
                values: new object[] { RecursoId, ModuloId, "Cooperados", "COOPERADOS", true }
            );

            // Permissoes
            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "permissao",
                columns: new[] { "id", "recurso_id", "nome", "codigo", "ativo" },
                values: new object[,]
                {
                    { PermVisId, RecursoId, "Visualizar", "cooperados.visualizar", true },
                    { PermInsId, RecursoId, "Inserir", "cooperados.inserir", true },
                    { PermAltId, RecursoId, "Alterar", "cooperados.alterar", true },
                    { PermInaId, RecursoId, "Inativar", "cooperados.inativar", true },
                    { PermReaId, RecursoId, "Reativar", "cooperados.reativar", true }
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "permissao",
                keyColumn: "id",
                keyValues: new object[] { PermVisId, PermInsId, PermAltId, PermInaId, PermReaId }
            );

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "recurso",
                keyColumn: "id",
                keyValue: RecursoId
            );

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "modulo",
                keyColumn: "id",
                keyValue: ModuloId
            );
        }
    }
}
