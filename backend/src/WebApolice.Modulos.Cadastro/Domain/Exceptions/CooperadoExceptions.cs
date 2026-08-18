using System;

namespace WebApolice.Modulos.Cadastro.Domain.Exceptions;

public class CooperadoInvalidoException : Exception
{
    public CooperadoInvalidoException(string message) : base(message) { }
}

public class CooperadoNaoEncontradoException : Exception
{
    public CooperadoNaoEncontradoException(string message) : base(message) { }
}

public class CooperadoJaCadastradoException : Exception
{
    public CooperadoJaCadastradoException(string message) : base(message) { }
}
