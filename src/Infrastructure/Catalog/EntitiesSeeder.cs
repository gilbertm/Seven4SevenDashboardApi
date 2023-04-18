using System.Reflection;
using RAFFLE.WebApi.Application.Common.Interfaces;
using RAFFLE.WebApi.Domain.Catalog;
using RAFFLE.WebApi.Infrastructure.Identity;
using RAFFLE.WebApi.Infrastructure.Persistence.Context;
using RAFFLE.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;
using RAFFLE.WebApi.Domain.Common;

namespace RAFFLE.WebApi.Infrastructure.Catalog;

public class EntitiesSeeder : ICustomSeeder
{
    private readonly ISerializerService _serializerService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EntitiesSeeder> _logger;

    public EntitiesSeeder(ISerializerService serializerService, ILogger<EntitiesSeeder> logger, ApplicationDbContext db)
    {
        _serializerService = serializerService;
        _logger = logger;
        _db = db;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!_db.AppUsers.Any())
        {
            _logger.LogInformation("Started to Seed AppUsers.");

            // Here you can use your own logic to populate the database.
            // As an example, I am using a JSON file to populate the database.
            string appUserData = await File.ReadAllTextAsync(path + "/Catalog/appusers.json", cancellationToken);
            var appUsers = _serializerService.Deserialize<List<AppUser>>(appUserData);
            var applicationUsers = _db.Users.AsQueryable().ToList();

            if (applicationUsers != null)
            {
                var appUserDefault = appUsers.FirstOrDefault();

                foreach (var applicationUser in applicationUsers)
                {

                    if (appUserDefault is not null)
                    {
                        // var appUserDefaultForSave = new AppUser(applicationUser.Id, appUserDefault.HomeAddress, appUserDefault.HomeCity, appUserDefault.HomeRegion, appUserDefault.HomeCountry, appUserDefault.Longitude, appUserDefault.Latitude, false, VerificationStatus.Initial, "Basic");
                        // await _db.AppUsers.AddAsync(appUserDefaultForSave ?? default!, cancellationToken);
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded AppUsers.");
        }
    }
}