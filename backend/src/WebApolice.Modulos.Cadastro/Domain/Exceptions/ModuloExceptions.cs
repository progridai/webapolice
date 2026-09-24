using System;

namespace WebApolice.Modulos.Cadastro.Domain.Exceptions;

/// <summary>
/// Lançada quando um Módulo do catálogo global não é encontrado pelo publicId informado.
/// Mapeada para HTTP 404 Not Found pelo GlobalExceptionHandler.
/// </summary>
public class ModuloNaoEncontradoException : Exception
{
    public ModuloNaoEncontradoException(string message) : base(message) { }
}
