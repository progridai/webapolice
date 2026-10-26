using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Domain.Modelos;

namespace WebApolice.Modulos.Cadastro.Tests.Persistence;

public class ClienteDependenteRgTests
{
    private static CadastroDbContext Context() => new(new DbContextOptionsBuilder<CadastroDbContext>()
        .UseNpgsql("Host=localhost;Database=model_test")
        .UseSnakeCaseNamingConvention().Options);

    [Fact]
    public void CamposRgSaoOpcionaisComNomesETiposCorretos()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(ClienteDependenteModel))!;
        var table = StoreObjectIdentifier.Table("cliente_dependente", "cadastro");
        foreach (var (property, column, type) in new[]
        {
            ("Rg", "rg", "text"), ("OrgaoRg", "orgao_rg", "text"), ("DataEmissaoRg", "data_emissao_rg", "date")
        })
        {
            var metadata = entity.FindProperty(property)!;
            Assert.True(metadata.IsNullable);
            Assert.Equal(column, metadata.GetColumnName(table));
            Assert.Equal(type, metadata.GetColumnType());
        }
    }

    [Fact]
    public void MigrationAdicionaSomenteTresColunasSemPreencherDadosExistentes()
    {
        using var context = Context();
        var assembly = context.GetService<IMigrationsAssembly>();
        var migration = assembly.CreateMigration(assembly.Migrations["20261014010000_AdicionarRgClienteDependente"],
            "Npgsql.EntityFrameworkCore.PostgreSQL");
        Assert.Equal(3, migration.UpOperations.Count);
        foreach (var operation in migration.UpOperations)
        {
            var column = Assert.IsType<AddColumnOperation>(operation);
            Assert.Equal("cadastro", column.Schema);
            Assert.Equal("cliente_dependente", column.Table);
            Assert.True(column.IsNullable);
            Assert.Null(column.DefaultValue);
        }
        Assert.Equal(3, migration.DownOperations.Count);
        Assert.All(migration.DownOperations, operation => Assert.IsType<DropColumnOperation>(operation));
        var target = Assert.Single(migration.TargetModel.GetEntityTypes(), e => e.GetSchema() == "cadastro" && e.GetTableName() == "cliente_dependente");
        Assert.NotNull(target.FindProperty("Rg"));
        Assert.NotNull(target.FindProperty("OrgaoRg"));
        Assert.NotNull(target.FindProperty("DataEmissaoRg"));
    }
}
