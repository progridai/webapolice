using System;

namespace WebApolice.Modulos.Seguranca.Domain.Exceptions;

public sealed class ConcorrenciaProvisionamentoException(Exception innerException)
    : Exception("O usuário foi provisionado por uma operação concorrente.", innerException);
