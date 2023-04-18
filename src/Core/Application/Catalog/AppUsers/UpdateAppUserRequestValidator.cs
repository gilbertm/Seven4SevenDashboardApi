namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

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

        RuleFor(au => au.Latitude)
            .MaximumLength(256);

        RuleFor(au => au.Longitude)
            .MaximumLength(256);

        /* RuleFor(au => au.PackageId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await packageRepository.GetByIdAsync(id, ct) is not null)
            .WithMessage(_ => T["Application package details for User does not exist. Package Id is needed.", _.PackageId]); */
    }
}