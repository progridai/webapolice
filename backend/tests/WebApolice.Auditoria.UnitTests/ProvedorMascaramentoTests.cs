using System.Text.Json;
using WebApolice.Auditoria.Domain.Exceptions;
using WebApolice.Auditoria.Infrastructure.Mascaramento;
using Xunit;

namespace WebApolice.Auditoria.UnitTests;

public class ProvedorMascaramentoTests
{
    [Fact]
    public void ValidarERejeitarSegredos_ShouldThrowException_WhenSecretInRootObject()
    {
        // Arrange
        var jsonStr = """{ "nome": "João", "senha": "senha-super-secreta" }""";
        var doc = JsonDocument.Parse(jsonStr);

        // Act & Assert
        var ex = Assert.Throws<ValidacaoAuditoriaException>(() => ProvedorMascaramento.ValidarERejeitarSegredos(doc));
        Assert.Contains("senha", ex.Message);
    }

    [Fact]
    public void ValidarERejeitarSegredos_ShouldThrowException_WhenSecretInNestedObject()
    {
        // Arrange
        var jsonStr = """
        {
            "endereco": {
                "rua": "Rua X",
                "numero_cartao": "1234-5678-9012-3456"
            }
        }
        """;
        var doc = JsonDocument.Parse(jsonStr);

        // Act & Assert
        var ex = Assert.Throws<ValidacaoAuditoriaException>(() => ProvedorMascaramento.ValidarERejeitarSegredos(doc));
        Assert.Contains("numero_cartao", ex.Message);
    }

    [Fact]
    public void ValidarERejeitarSegredos_ShouldThrowException_WhenSecretInArray()
    {
        // Arrange
        var jsonStr = """
        [
            { "id": 1, "Authorization": "Bearer 123" }
        ]
        """;
        var doc = JsonDocument.Parse(jsonStr);

        // Act & Assert
        var ex = Assert.Throws<ValidacaoAuditoriaException>(() => ProvedorMascaramento.ValidarERejeitarSegredos(doc));
        // Testando case-insensitive
        Assert.Contains("Authorization", ex.Message); 
    }

    [Fact]
    public void ValidarERejeitarSegredos_ShouldReturnSameObject_IfNoSensitiveData()
    {
        // Arrange
        var jsonStr = """{ "nome": "João", "idade": 30 }""";
        var doc = JsonDocument.Parse(jsonStr);

        // Act
        var mascarado = ProvedorMascaramento.ValidarERejeitarSegredos(doc);

        // Assert
        Assert.Same(doc, mascarado);
    }

    [Fact]
    public void ValidarERejeitarSegredos_ShouldThrowException_WhenMaxDepthExceeded()
    {
        // Arrange
        var jsonStr = "{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{\"a\":{}}}}}}}}}}}}}}}}}}}}}}}";
        var doc = JsonDocument.Parse(jsonStr);

        // Act & Assert
        var ex = Assert.Throws<ValidacaoAuditoriaException>(() => ProvedorMascaramento.ValidarERejeitarSegredos(doc));
        Assert.Contains("Profundidade máxima", ex.Message);
    }
}
