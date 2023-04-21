namespace RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;

public record InkMeLiveTokenRequest(string ClientId, string ClientSecret);

public class InkMeLiveTokenRequestValidator : CustomValidator<InkMeLiveTokenRequest>
{
    public InkMeLiveTokenRequestValidator(IStringLocalizer<InkMeLiveTokenRequestValidator> T)
    {
        RuleFor(b => b.ClientId).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.ClientSecret).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}