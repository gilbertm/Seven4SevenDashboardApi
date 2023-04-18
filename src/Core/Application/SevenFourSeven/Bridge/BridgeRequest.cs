namespace RAFFLE.WebApi.Application.SevenFourSeven.Bridge;

public record BridgeRequest(string Email, string UserName, string UserId, bool IsAgent);

public class BridgeRequestValidator : CustomValidator<BridgeRequest>
{
    public BridgeRequestValidator(IStringLocalizer<BridgeRequestValidator> T)
    {
        RuleFor(b => b.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid Email Address."]);

        RuleFor(b => b.UserName).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.UserId).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.IsAgent).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}