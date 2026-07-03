namespace WebApolice.Api.Infrastructure.Errors;

// ATENÇÃO: Estas classes são estritamente para demonstração do GlobalExceptionHandler e dos 
// mapeamentos HTTP (404, 409, 422) enquanto não temos módulos de negócio reais definidos.
// Quando o primeiro módulo real for criado, ele definirá suas próprias exceções puras (sem 
// acoplamento HTTP) e o GlobalExceptionHandler será adaptado para capturá-las.
// O SharedKernel não deve ser usado como depósito de exceções genéricas.

public class DemoRecursoNaoEncontradoException : Exception
{
    public DemoRecursoNaoEncontradoException(string message) : base(message) { }
}

public class DemoConflitoDeNegocioException : Exception
{
    public DemoConflitoDeNegocioException(string message) : base(message) { }
}

public class DemoRegraDeNegocioException : Exception
{
    public DemoRegraDeNegocioException(string message) : base(message) { }
}
