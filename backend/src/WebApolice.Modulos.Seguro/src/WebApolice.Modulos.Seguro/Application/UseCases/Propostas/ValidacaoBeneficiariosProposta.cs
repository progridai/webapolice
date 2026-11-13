using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas;

internal static class ValidacaoBeneficiariosProposta
{
    internal static void ValidarIndicacao(string? nome, string? parentesco, decimal? percentual)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 150 ||
            string.IsNullOrWhiteSpace(parentesco) || parentesco.Trim().Length > 100 ||
            percentual is null or <= 0 or > 100 || decimal.Round(percentual.Value, 4) != percentual.Value)
            throw new ValidacaoException("Informe nome, parentesco e percentual válido (maior que zero, até 100%, com até quatro casas decimais).");
    }

    internal static void ValidarTotal(decimal total, bool exigirCem = false)
    {
        if (total > 100) throw new ValidacaoException("A soma dos percentuais dos beneficiários não pode ultrapassar 100%.");
        if (exigirCem && total != 100) throw new ValidacaoException("Os beneficiários devem somar exatamente 100% para confirmar a proposta.");
    }
}
