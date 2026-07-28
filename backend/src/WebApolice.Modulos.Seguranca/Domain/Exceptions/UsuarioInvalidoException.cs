using System;

namespace WebApolice.Modulos.Seguranca.Domain.Exceptions;

public class UsuarioInvalidoException : Exception
{
    public UsuarioInvalidoException(string message) : base(message)
    {
    }

    public UsuarioInvalidoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
