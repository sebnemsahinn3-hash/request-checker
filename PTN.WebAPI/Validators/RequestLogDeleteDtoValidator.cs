using FluentValidation;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Localization;

namespace PTN.WebAPI.Validators
{
    public class RequestLogDeleteDtoValidator : AbstractValidator<RequestLogDeleteDto>
    {
        public RequestLogDeleteDtoValidator(IStringLocalizer? localizer = null)
        {
            var loc = localizer ?? new JsonStringLocalizer();

            // 1. Kural: IsAllDelete alanı boş geçilemez
            RuleFor(x => x.IsAllDelete)
                .NotNull()
                .WithMessage(x => loc[ExceptionCodes.IsAllDeleteRequired].Value);

            // 2. Kural: Count eksi (-) olamaz
            RuleFor(x => x.Count)
                .GreaterThanOrEqualTo(0)
                .WithMessage(x => loc[ExceptionCodes.CountInvalid].Value)
                .When(x => x.Count.HasValue);

            // 3. Kural: Üst Sınır Kısıtı (Tek seferde en fazla 1000 kayıt)
            RuleFor(x => x.Count)
                .LessThanOrEqualTo(1000)
                .WithMessage(x => loc[ExceptionCodes.CountTooLarge].Value)
                .When(x => x.Count.HasValue);

            // 4. Kural: IsAllDelete false ise Count sayısı girilmesi zorunludur
            RuleFor(x => x.Count)
                .NotNull()
                .GreaterThan(0)
                .WithMessage(x => loc[ExceptionCodes.CountRequired].Value)
                .When(x => !x.IsAllDelete);

            // 5. Kural: IsAllDelete true ise Count alanı boş (null) bırakılmalıdır
            RuleFor(x => x.Count)
                .Null()
                .WithMessage(x => loc[ExceptionCodes.CountNullWhenIsAllDelete].Value)
                .When(x => x.IsAllDelete);
        }
    }
}