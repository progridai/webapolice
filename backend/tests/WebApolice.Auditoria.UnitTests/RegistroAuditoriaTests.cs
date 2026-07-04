using WebApolice.Auditoria.Domain;
using Xunit;

namespace WebApolice.Auditoria.UnitTests;

public class RegistroAuditoriaTests
{
    [Fact]
    public void Pode_Criar_Registro_Sem_Usuario()
    {
        // Arrange & Act
        var registro = new RegistroAuditoria
        {
            Acao = "consultar",
            Modulo = "geral",
            Recurso = "sistema",
            Resultado = ResultadoAuditoria.Sucesso,
            DataHoraUtc = DateTime.UtcNow
        };

        // Assert
        Assert.Null(registro.UsuarioIdExterno);
        Assert.Null(registro.UsuarioNome);
        Assert.Equal("consultar", registro.Acao);
    }
}
