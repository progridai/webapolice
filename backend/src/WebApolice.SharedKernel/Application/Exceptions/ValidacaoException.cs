using System;

namespace WebApolice.SharedKernel.Application.Exceptions;

public class ValidacaoException : Exception
{
    public ValidacaoException(string message) : base(message)
    {
    }
}
