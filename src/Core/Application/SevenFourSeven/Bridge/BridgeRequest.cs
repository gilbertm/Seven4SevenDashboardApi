namespace RAFFLE.WebApi.Application.SevenFourSeven.Bridge;

public record BridgeRequest(string UserName, bool IsAgent);

public class BridgeRequestValidator : CustomValidator<BridgeRequest>
{
    public BridgeRequestValidator(IStringLocalizer<BridgeRequestValidator> T)
    {
        RuleFor(b => b.UserName).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.IsAgent).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}