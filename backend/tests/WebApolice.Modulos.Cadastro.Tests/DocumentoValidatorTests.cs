using WebApolice.Modulos.Cadastro.Domain.Validators;

namespace WebApolice.Modulos.Cadastro.Tests;

public class DocumentoValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", 1, true)]
    [InlineData("52998224724", 1, false)]
    [InlineData("00000000000", 1, false)]
    [InlineData("11111111111", 1, false)]
    [InlineData("123", 1, false)]
    [InlineData(null, 1, false)]
    [InlineData("11.222.333/0001-81", 2, true)]
    [InlineData("11222333000180", 2, false)]
    [InlineData("00000000000000", 2, false)]
    [InlineData("11111111111111", 2, false)]
    [InlineData("52998224725", 2, false)]
    [InlineData("11222333000181", 1, false)]
    [InlineData("52998224725", 0, false)]
    public void ValidaTipoComprimentoEDigitos(string? documento, short tipo, bool valido)
    {
        Assert.Equal(valido, DocumentoValidator.IsValid(documento, tipo));
    }
}
