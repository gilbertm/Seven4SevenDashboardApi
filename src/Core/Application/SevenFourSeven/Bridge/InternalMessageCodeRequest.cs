using System.Numerics;

namespace RAFFLE.WebApi.Application.SevenFourSeven.Bridge;

public class InternalMessageCodeRequest
{
    public string AuthToken { get; set; } = default!;
    public int Platform { get; set; }
    public string Username { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Message { get; set; } = default!;
}

public class InternalMessageCodeRequestValidator : CustomValidator<InternalMessageCodeRequest>
{
    public InternalMessageCodeRequestValidator()
    {
        RuleFor(b => b.AuthToken).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.Platform).Cascade(CascadeMode.Stop)
            .NotEqual(0)
            .NotEmpty();

        RuleFor(b => b.Username).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(b => b.Subject).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(b => b.Message).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}