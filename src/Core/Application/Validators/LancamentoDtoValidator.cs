using Core.Application.DTOs;
using FluentValidation;


namespace Core.Application.Validators;
public class LancamentoDtoValidator : AbstractValidator<LancamentoDto>
{
    public LancamentoDtoValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("O valor deve ser maior que zero")
            .NotEmpty()
            .WithMessage("O valor é obrigatório");

        RuleFor(x => x.Tipo)
            .NotEmpty()
            .WithMessage("O tipo é obrigatório")
            .Must(x => x == "CREDITO" || x == "DEBITO")
            .WithMessage("O tipo deve ser 'CREDITO' ou 'DEBITO'");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("A descrição é obrigatória")
            .MaximumLength(255)
            .WithMessage("A descrição deve ter no máximo 255 caracteres");
    }
}