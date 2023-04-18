namespace RAFFLE.WebApi.Application.Catalog.AppUsers;

public class CreateAppUserRequestValidator : CustomValidator<CreateAppUserRequest>
{
    public CreateAppUserRequestValidator(IReadRepository<AppUser> appUserRepository, ICurrentUser currentUser, IStringLocalizer<CreateAppUserRequestValidator> T)
    {
        RuleFor(au => au.ApplicationUserId)
            .NotEmpty()
            .MustAsync(async (applicationUserId, ct) => await appUserRepository.GetBySpecAsync(new AppUserByApplicationUserIdSpec(applicationUserId), ct) is null)
            .WithMessage((appUser) => T["Application details for user: '{0}' already exists.", appUser.ApplicationUserId ?? default!]);

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
            .MustAsync(async (packageId, ct) => await packageRepository.GetByIdAsync(packageId, ct) is not null)
            .WithMessage(_ => T["Application package details for User does not exists. Package Id is needed.", _.PackageId]); */

    }
}