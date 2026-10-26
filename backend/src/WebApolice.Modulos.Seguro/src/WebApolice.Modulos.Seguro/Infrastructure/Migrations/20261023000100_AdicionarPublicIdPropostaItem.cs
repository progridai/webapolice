using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations;

[DbContext(typeof(SeguroDbContext))]
[Migration("20261023000100_AdicionarPublicIdPropostaItem")]
public sealed class AdicionarPublicIdPropostaItem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "proposta", "proposta_item" })
            migrationBuilder.AlterColumn<int>(name: "legado_id", schema: "seguro", table: table,
                type: "integer", nullable: true, oldClrType: typeof(int), oldType: "integer");
        migrationBuilder.AddColumn<Guid>(name: "public_id", schema: "seguro", table: "proposta_item",
            type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()");
        migrationBuilder.CreateIndex(name: "ix_proposta_item_public_id", schema: "seguro", table: "proposta_item",
            column: "public_id", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM seguro.proposta WHERE legado_id IS NULL)
                   OR EXISTS (SELECT 1 FROM seguro.proposta_item WHERE legado_id IS NULL) THEN
                    RAISE EXCEPTION 'Rollback bloqueado: existem registros nativos sem identificador legado.';
                END IF;
            END $$;
            """);
        foreach (var table in new[] { "proposta", "proposta_item" })
            migrationBuilder.AlterColumn<int>(name: "legado_id", schema: "seguro", table: table,
                type: "integer", nullable: false, oldClrType: typeof(int), oldType: "integer", oldNullable: true);
        migrationBuilder.DropIndex(name: "ix_proposta_item_public_id", schema: "seguro", table: "proposta_item");
        migrationBuilder.DropColumn(name: "public_id", schema: "seguro", table: "proposta_item");
    }
}
