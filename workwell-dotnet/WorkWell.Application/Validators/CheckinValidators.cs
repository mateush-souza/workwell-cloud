using FluentValidation;
using WorkWell.Application.DTOs;

namespace WorkWell.Application.Validators;

public class CreateCheckinRequestValidator : AbstractValidator<CreateCheckinRequest>
{
    public CreateCheckinRequestValidator()
    {
        RuleFor(x => x.NivelStress)
            .InclusiveBetween(1, 10).WithMessage("Nível de stress deve estar entre 1 e 10");

        RuleFor(x => x.HorasTrabalhadas)
            .GreaterThanOrEqualTo(0).WithMessage("Horas trabalhadas deve ser maior ou igual a 0")
            .LessThanOrEqualTo(24).WithMessage("Horas trabalhadas não pode exceder 24 horas");

        RuleFor(x => x.HorasSono)
            .GreaterThanOrEqualTo(0).WithMessage("Horas de sono deve ser maior ou igual a 0")
            .LessThanOrEqualTo(24).WithMessage("Horas de sono não pode exceder 24 horas")
            .When(x => x.HorasSono.HasValue);

        RuleFor(x => x.Sentimento)
            .MaximumLength(50).WithMessage("Sentimento deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Sentimento));

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000).WithMessage("Observações deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observacoes));

        RuleFor(x => x.DataCheckin)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Data do check-in não pode ser no futuro")
            .When(x => x.DataCheckin.HasValue);
    }
}

