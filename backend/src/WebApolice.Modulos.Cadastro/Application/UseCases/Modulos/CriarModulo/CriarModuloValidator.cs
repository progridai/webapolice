using FluentValidation;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;

public class CriarModuloValidator : AbstractValidator<CriarModuloCommand>
{
    public CriarModuloValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O Nome do Módulo é obrigatório.")
            .MaximumLength(150).WithMessage("O Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A Descrição deve ter no máximo 500 caracteres.");
    }
}
