using System;

namespace WebApolice.Modulos.Cadastro.Domain.Exceptions;

public class EstipulanteDominioException : Exception
{
    public EstipulanteDominioException(string message) : base(message)
    {
    }
}

public class EstipulanteConflitoException : EstipulanteDominioException
{
    public EstipulanteConflitoException(string message) : base(message)
    {
    }
}

public class EstipulanteInvalidoException : EstipulanteDominioException
{
    public EstipulanteInvalidoException(string message) : base(message)
    {
    }
}
