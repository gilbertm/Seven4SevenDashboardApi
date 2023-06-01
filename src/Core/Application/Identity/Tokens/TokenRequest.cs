namespace UNIFIEDDASHBOARD.WebApi.Application.Identity.Tokens;

public class TokenRequest {
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class TokenRequestValidator : CustomValidator<TokenRequest>
{
    public TokenRequestValidator(IStringLocalizer<TokenRequestValidator> T)
    {
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid Email Address."]);

        RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}