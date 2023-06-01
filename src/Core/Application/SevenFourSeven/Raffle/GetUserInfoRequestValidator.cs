using UNIFIEDDASHBOARD.WebApi.Application.Identity.Users;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;

public class GetUserInfoRequestValidator : CustomValidator<GetUserInfoRequest>
{
    public GetUserInfoRequestValidator(IUserService userService, IStringLocalizer<UpdateUserRequestValidator> T)
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(r => r.Surname)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid Email Address."])
            .MustAsync(async (user, email, _) => !await userService.ExistsWithEmailAsync(email!))
                .WithMessage((_, email) => string.Format(T["Email {0} is already registered."], email));

        RuleFor(u => u.Phone).Cascade(CascadeMode.Stop)
            .MustAsync(async (user, phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!))
                .WithMessage((_, phone) => string.Format(T["Phone number {0} is already registered."], phone))
            .Unless(u => string.IsNullOrWhiteSpace(u.Phone));

        RuleFor(u => u.UserName747).Cascade(CascadeMode.Stop)
            .NotEmpty();

    }
}