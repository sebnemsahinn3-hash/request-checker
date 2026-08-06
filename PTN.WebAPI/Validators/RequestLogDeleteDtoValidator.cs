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

            // ExceptionCodes Sabiti Üzerinden Çoklu Dil Destekli Validasyon Mesajı
            RuleFor(x => x.Count)
                .GreaterThanOrEqualTo(0)
                .WithMessage(x => loc[ExceptionCodes.CountInvalid].Value)
                .When(x => x.Count.HasValue);
        }
    }
}