using System.Text.RegularExpressions;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

namespace WebApolice.Modulos.Cadastro.Domain.Validators;

public static class RegrasCadastraisValidator
{
    public static void ValidarRg(string? rg)
    {
        if (!string.IsNullOrWhiteSpace(rg) && !Regex.IsMatch(rg, @"^[a-zA-Z0-9\- ]*$"))
            throw new CooperadoInvalidoException("RG contém caracteres inválidos. Apenas letras, números, espaços e hifens são permitidos.");
    }

    public static void ValidarAgencia(string? agencia)
    {
        if (!string.IsNullOrWhiteSpace(agencia) && !Regex.IsMatch(agencia, @"^[a-zA-Z0-9\-]*$"))
            throw new CooperadoInvalidoException("Agência contém caracteres inválidos. Apenas letras, números e hifens são permitidos.");
    }

    public static void ValidarContaCorrente(string? conta)
    {
        if (!string.IsNullOrWhiteSpace(conta) && !Regex.IsMatch(conta, @"^[a-zA-Z0-9\-]*$"))
            throw new CooperadoInvalidoException("Conta Corrente contém caracteres inválidos. Apenas letras, números e hifens são permitidos.");
    }
}
