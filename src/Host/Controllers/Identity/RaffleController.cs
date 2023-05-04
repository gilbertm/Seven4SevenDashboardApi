using RAFFLE.WebApi.Application.Common.Persistence;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.Raffle;
using RAFFLE.WebApi.Domain.Catalog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAFFLE.WebApi.Host.Controllers.Identity;

public class RaffleController : VersionNeutralApiController
{
    private readonly IUserService _userService;

    private readonly IRepositoryWithEvents<AppUser> _repoAppUser;

    private readonly IConfiguration _config;

    public RaffleController(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser)
    {
        _userService = userService;
        _config = config;
        _repoAppUser = repoAppUser;
    }

    [HttpPost("get-entries")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Update a ledger record.", "")]
    public async Task<GetEntriesResponse> GetRaffleEntriesAsync([FromBody] GetEntriesRequest getEntriesRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonSerializer.Serialize(getEntriesRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:GetEntriesUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                GetEntriesResponse? result = await response.Content.ReadFromJsonAsync<GetEntriesResponse>();

                if (result is { })
                {
                    return result;
                }
            }
        }

        return new GetEntriesResponse
        {
            ErorrCode = 1,
            Message = "Unsuccessful request. Generic error."
        };
    }
}
