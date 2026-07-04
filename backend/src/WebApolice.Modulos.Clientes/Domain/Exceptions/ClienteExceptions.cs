using System;

namespace WebApolice.Modulos.Clientes.Domain.Exceptions;

public class ClienteInvalidoException : Exception
{
    public ClienteInvalidoException(string message) : base(message) { }
}

public class ClienteNaoEncontradoException : Exception
{
    public ClienteNaoEncontradoException(string message) : base(message) { }
}

public class ClienteJaCadastradoException : Exception
{
    public ClienteJaCadastradoException(string message) : base(message) { }
}
