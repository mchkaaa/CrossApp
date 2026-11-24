using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Ідентифікатор клієнта обов'язковий.")
            .MaximumLength(50).WithMessage("Ідентифікатор клієнта не може перевищувати 50 символів.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Замовлення має містити хоча б один рядок (Lines не може бути порожнім).");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("Код товару обов'язковий.")
                .MaximumLength(50).WithMessage("Код товару не може перевищувати 50 символів.");

            line.RuleFor(l => l.Price)
                .GreaterThan(0).WithMessage("Ціна має бути більшою за 0.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Кількість має бути більшою за 0.");
        });
    }
}