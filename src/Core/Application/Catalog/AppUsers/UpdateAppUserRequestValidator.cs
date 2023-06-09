namespace UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;

public class UpdateAppUserRequestValidator : CustomValidator<UpdateAppUserRequest>
{
    public UpdateAppUserRequestValidator(IReadRepository<AppUser> appUserRepository, IStringLocalizer<UpdateAppUserRequestValidator> T)
    {
        RuleFor(au => au.ApplicationUserId)
          .NotEmpty()
          .MustAsync(async (applicationUserId, ct) => await appUserRepository.GetBySpecAsync(new AppUserByApplicationUserIdSpec(applicationUserId), ct) is not null)
              .WithMessage((appUser) => T["Application details for User {0} does not xist.", appUser]);

        RuleFor(au => au.HomeAddress)
            .MaximumLength(256);

        RuleFor(au => au.HomeCity)
            .MaximumLength(256);

        RuleFor(au => au.HomeRegion)
            .MaximumLength(256);

        RuleFor(au => au.HomeCountry)
            .MaximumLength(256);

        RuleFor(au => au.RoleId)
          .MaximumLength(80);

        RuleFor(au => au.RoleName)
          .MaximumLength(25);

        RuleFor(au => au.RaffleUserId)
          .MaximumLength(50);

        RuleFor(au => au.RaffleUserId747)
          .MaximumLength(50);

        RuleFor(au => au.RaffleUsername747)
          .MaximumLength(50);
    }
}