using RAFFLE.WebApi.Application.Common.Persistence;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;
using RAFFLE.WebApi.Domain.Catalog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAFFLE.WebApi.Host.Controllers.Identity;

public class InkMeLiveController : VersionNeutralApiController
{
    private readonly IUserService _userService;

    private readonly IRepositoryWithEvents<AppUser> _repoAppUser;

    private readonly IConfiguration _config;

    public InkMeLiveController(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser)
    {
        _userService = userService;
        _config = config;
        _repoAppUser = repoAppUser;
    }

    [HttpPost("get-token")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Get an ink me live token.", "")]
    public async Task<InkMeLiveTokenResponse> GetTokenAsync()
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            InkMeLiveTokenRequest tokenRequest = new(_config.GetSection("SevenFourSevenAPIs:InkMeLive:ClientId").Value!, _config.GetSection("SevenFourSevenAPIs:InkMeLive:ClientSecret").Value!);

            string json = JsonSerializer.Serialize(tokenRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:TokenUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                InkMeLiveTokenResponse? result = await response.Content.ReadFromJsonAsync<InkMeLiveTokenResponse>();

                if (result is { })
                {
                    return result;
                }
            }
        }

        return new InkMeLiveTokenResponse
        {
            AuthToken = string.Empty,
            StatusCode = 425,
            TokenType = string.Empty,
            IsSuccess = false,
            Message = "Unsuccessful request. Generic error."
        };
    }

    [HttpPost("create-applicant")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Get an ink me live token.", "")]
    public async Task<string> CreateApplicantDetailsAsync([FromBody] InkMeLivePlayerDetailsRequest inkMeLivePlayerDetailsRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            InkMeLiveTokenRequest tokenRequest = new(_config.GetSection("SevenFourSevenAPIs:InkMeLive:ClientId").Value!, _config.GetSection("SevenFourSevenAPIs:InkMeLive:ClientSecret").Value!);

            string json = JsonSerializer.Serialize(tokenRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:TokenUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                return string.Empty;
            }
        }

        return string.Empty;
    }
}