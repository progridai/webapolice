using WebApolice.Modulos.Cadastro.Domain.Clientes;
using Microsoft.EntityFrameworkCore;
using WebApolice.SharedKernel.Text;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Domain.Modelos;

namespace WebApolice.Modulos.Cadastro.Tests.Persistence;

public class PadronizacaoTextoTests
{
    [Theory]
    [InlineData("JOÃO DA SILVA", "João da Silva")]
    [InlineData("mARIA d'ÁVILA-SOUZA", "Maria d'Ávila-Souza")]
    [InlineData("  AÇÃO  E SAÚDE\nSÃO PAULO ", "  Ação  e Saúde\nSão Paulo ")]
    [InlineData("SEGURO DE VIDA E ACIDENTES", "Seguro de Vida e Acidentes")]
    [InlineData("O PLANO PARA A FAMÍLIA", "o Plano para a Família")]
    [InlineData("PELA SAÚDE OU PELO BEM", "pela Saúde ou pelo Bem")]
    [InlineData("JOÃO DAS NEVES COM MARIA DOS SANTOS", "João das Neves com Maria dos Santos")]
    [InlineData("AÇÃO ÀS EMPRESAS SEM CUSTO", "Ação às Empresas sem Custo")]
    [InlineData("123abc", "123Abc")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void ConverteComAcentosEDeFormaIdempotente(string? input, string? expected)
    {
        Assert.Equal(expected, PadronizacaoTexto.TitleCase(input));
        Assert.Equal(expected, PadronizacaoTexto.TitleCase(expected));
    }

    [Theory]
    [InlineData("MotivoPendenciaVigencia", true)]
    [InlineData("ConfirmadaEm", false)]
    [InlineData("CanceladaEm", false)]
    [InlineData("DescartadaEm", false)]
    [InlineData("DataCancelamentoVigencia", false)]
    [InlineData("CancelamentoSolicitadoEm", false)]
    [InlineData("ParentescoRegistrado", true)]
    [InlineData("JustificativaAjustePremio", true)]
    [InlineData("ParentescoOriginal", false)]
    [InlineData("RevisaoPreenchimento", false)]
    [InlineData("Nome", true)]
    [InlineData("nome_fantasia", true)]
    [InlineData("NomeNormalizado", false)]
    [InlineData("Email", false)]
    [InlineData("Uf", false)]
    [InlineData("Codigo", false)]
    [InlineData("Username", false)]
    [InlineData("NovaMatricula", false)]
    [InlineData("NomeOriginal", false)]
    [InlineData("SubgrupoNomeRegistrado", true)]
    [InlineData("modulo_nome_registrado", true)]
    [InlineData("PlanoNomeRegistrado", true)]
    [InlineData("CoberturaNomeRegistrado", true)]
    [InlineData("CoberturaNomeReduzidoRegistrado", true)]
    [InlineData("CoberturaBasicaRegistrada", false)]
    [InlineData("PlanoNomeLegado", false)]
    [InlineData("CoberturaNomeLegado", false)]
    public void DistingueTextoHumanoDeCampoTecnico(string field, bool expected) =>
        Assert.Equal(expected, PadronizacaoTexto.CampoHumano(field));

    [Fact]
    public void NormalizaInclusoesEAlteracoesSemModificarChavesDeBusca()
    {
        using var db = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql("Host=localhost;Database=model_test").UseSnakeCaseNamingConvention().Options);
        var pessoa = new PessoaModel { Id = 1, Nome = "JOÃO SILVA", NomeNormalizado = "JOAO SILVA", DocumentoPrincipal = "123X", Observacao = "TEXTO LIVRE" };
        db.Add(pessoa);
        db.PadronizarTextos();
        Assert.Equal("João Silva", pessoa.Nome);
        Assert.Equal("Texto Livre", pessoa.Observacao);
        Assert.Equal("JOAO SILVA", pessoa.NomeNormalizado);
        Assert.Equal("123X", pessoa.DocumentoPrincipal);
        db.Entry(pessoa).State = EntityState.Unchanged;
        pessoa.Nome = "MARIA SOUZA";
        db.PadronizarTextos();
        Assert.Equal("Maria Souza", pessoa.Nome);
    }
    [Fact]
    public void AlteracaoTecnicaPreservaTextoHistoricoIntocadoMasNormalizaNovaEdicaoHumana()
    {
        using var db = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql("Host=localhost;Database=model_test").UseSnakeCaseNamingConvention().Options);
        var pessoa = new PessoaModel { Id = 7, Nome = "EVIDÊNCIA HISTÓRICA", Observacao = "OBSERVAÇÃO ORIGINAL", DocumentoPrincipal = "ABC" };
        db.Attach(pessoa);
        pessoa.DocumentoPrincipal = "ABC-2";
        db.PadronizarTextos();
        Assert.Equal("EVIDÊNCIA HISTÓRICA", pessoa.Nome); Assert.Equal("OBSERVAÇÃO ORIGINAL", pessoa.Observacao);
        Assert.False(db.Entry(pessoa).Property(p => p.Nome).IsModified);
        pessoa.Nome = "MARIA d’ÁVILA";
        db.PadronizarTextos();
        Assert.Equal("Maria d’Ávila", pessoa.Nome); Assert.Equal("OBSERVAÇÃO ORIGINAL", pessoa.Observacao);
    }

    private sealed class SuppressSave : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> SavingChanges(
            Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData data,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result) =>
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>.SuppressWithResult(1);

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>> SavingChangesAsync(
            Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData data,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result, CancellationToken token = default) =>
            ValueTask.FromResult(Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>.SuppressWithResult(1));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task TodosOsOverloadsDeSaveChangesAplicamRegra(bool async, bool acceptChanges)
    {
        using var db = new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql("Host=localhost;Database=model_test").UseSnakeCaseNamingConvention()
            .AddInterceptors(new SuppressSave()).Options);
        var pessoa = new PessoaModel { Nome = "JOÃO DA SILVA" };
        db.Add(pessoa);
        if (async) await db.SaveChangesAsync(acceptChanges);
        else db.SaveChanges(acceptChanges);
        Assert.Equal("João da Silva", pessoa.Nome);
    }

}
