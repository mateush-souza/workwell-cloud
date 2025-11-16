using FluentValidation;
using WorkWell.Application.DTOs;
using System.Text.RegularExpressions;

namespace WorkWell.Application.Validators;

public class CreateEmpresaRequestValidator : AbstractValidator<CreateEmpresaRequest>
{
    public CreateEmpresaRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("CNPJ é obrigatório")
            .Must(BeValidCnpj).WithMessage("CNPJ inválido")
            .Length(14).WithMessage("CNPJ deve ter 14 dígitos");

        RuleFor(x => x.Setor)
            .MaximumLength(100).WithMessage("Setor deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Setor));
    }

    private bool BeValidCnpj(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj))
            return false;

        // Remove caracteres não numéricos
        cnpj = Regex.Replace(cnpj, @"[^\d]", "");

        if (cnpj.Length != 14)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cnpj.Distinct().Count() == 1)
            return false;

        // Validação dos dígitos verificadores
        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = cnpj.Substring(0, 12);
        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        int resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();
        tempCnpj += digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito += resto.ToString();

        return cnpj.EndsWith(digito);
    }
}

