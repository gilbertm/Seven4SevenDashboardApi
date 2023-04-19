using RAFFLE.WebApi.Application.Common.Persistence;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.Bridge;
using RAFFLE.WebApi.Application.SevenFourSeven.Raffle;
using RAFFLE.WebApi.Domain.Catalog;
using RAFFLE.WebApi.Domain.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace RAFFLE.WebApi.Host.Controllers.Identity;

public class SevenFourSevenController : VersionNeutralApiController
{
    private readonly IUserService _userService;

    private readonly IRepositoryWithEvents<AppUser> _repoAppUser;

    private readonly IConfiguration _config;

    public SevenFourSevenController(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser)
    {
        _userService = userService;
        _config = config;
        _repoAppUser = repoAppUser;
    }

    /*
 * The user is a bridge player.
 */
    [HttpPost("bridge-user")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 dashboard checks the bridge if the user is a user", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<BridgeHierarchyResponse> BridgeUserAsync([FromBody] BridgeRequest bridgeRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Brdge:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync($"{_config.GetSection("SevenFourSevenAPIs:Brdge:GetHierarchyUrl").Value!}?username={bridgeRequest.UserName}&isAgent={bridgeRequest.IsAgent}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BridgeHierarchyResponse>();

                if (result is { })
                {
                    return result;
                }
            }
        }

        return new BridgeHierarchyResponse { Status = 500, Message = "Unknown error." };
    }

    /*
     * The user is a bridge player. Eligibile to be in the Raffle system.
     *
     * 1. Check the raffle user info. If true, get the info.
     *    If false, make a record in the raffle system
     *
     */
    [HttpPost("user-info")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 dashboard checks the raffle system if the user is raffle ready. Return email", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<GetUserInfoResponse> RaffleUserInfoAsync([FromBody] GetUserInfoRequest getUserInfoRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            getUserInfoRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            string json = JsonSerializer.Serialize(getUserInfoRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:GetUserInfoUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                GetUserInfoResponse? result = await response.Content.ReadFromJsonAsync<GetUserInfoResponse>();

                if (result is { })
                {
                    if (result.ErorrCode != 0)
                    {
                        return new GetUserInfoResponse
                        {
                            ErorrCode = result.ErorrCode,
                            Message = result.Message
                        };
                    }

                    return result;
                }
            }
        }

        return new GetUserInfoResponse
        {
            ErorrCode = 1,
            Message = "Unsuccessful request. Generic error."
        };
    }

    // self register
    // provides a one time password intiutive approach
    // to allow user's to receive a dashboard generated password one time
    // this allows the dashboard to authenticate and
    // redirect back to the client's wherein the client will
    // call the dashboard api to be given a token
    // to go around the dashboard system
    [HttpPost("self-register")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 dashboard creates a dashboard user", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<GenericResponse> SelfRegisterAsync([FromBody] RegisterUserRequest registerUserRequest)
    {
        // we have sufficient data here to register the user

        // if player
        if (!registerUserRequest.IsAgent)
        {
            bool isPlayerInRaffleSystem = await IsPlayerInRaffleSystemAsync(new CheckUserRequest
            {
                AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
                Email = registerUserRequest.Email,
                Phone = registerUserRequest.Phone,
                UserId747 = registerUserRequest.Info747.UserId747,
                UserName747 = registerUserRequest.Info747.Username747

            });

            // craete the non player/affiliated user
            if (!isPlayerInRaffleSystem)
            {
                registerUserRequest.SocialProfiles.FacebookUrl = string.Empty;
                registerUserRequest.SocialProfiles.InstagramUrl = string.Empty;
                registerUserRequest.SocialProfiles.TwitterUrl = string.Empty;
                _ = await RegisterPlayerToRaffleSystemAsync(registerUserRequest);
            }

            // recheck if player in the raffle system, after operation
            isPlayerInRaffleSystem = await IsPlayerInRaffleSystemAsync(new CheckUserRequest
            {
                AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
                Email = registerUserRequest.Email,
                Phone = registerUserRequest.Phone,
                UserId747 = registerUserRequest.Info747.UserId747,
                UserName747 = registerUserRequest.Info747.Username747
            });

            if (isPlayerInRaffleSystem)
            {
                string? password = $"{getValueMiddleOfCustomString(registerUserRequest.Info747.UniqueCode)}{_config.GetSection("SevenFourSevenAPIs:PasswordSecretHash").Value!}";
                string? confirmPassword = $"{getValueMiddleOfCustomString(registerUserRequest.Info747.UniqueCode)}{_config.GetSection("SevenFourSevenAPIs:PasswordSecretHash").Value!}";

                // the user is successfully registered on our raffle system
                // it is safe to give him access to the dashboard now
                CreateUserRequest? createUserRequest = new CreateUserRequest
                {
                    Email = registerUserRequest.Email,
                    FirstName = registerUserRequest.Name,
                    LastName = registerUserRequest.Surname,
                    // unique code is the verification code from sendgrid
                    // we make use of this for one time password creation
                    // for the user to be able to go around to this dashboard system.
                    Password = password,
                    ConfirmPassword = confirmPassword,
                    UserName = registerUserRequest.Info747.Username747,
                    PhoneNumber = registerUserRequest.Phone
                };

                UserDetailsDto? updatedOrcreatedUser = await _userService.CreateSevenFourSevenAsync(createUserRequest, string.Empty, GetOriginFromRequest());

                if (updatedOrcreatedUser is { } && updatedOrcreatedUser.Id != default)
                {
                    AppUser? appUser = await _repoAppUser.GetByIdAsync(updatedOrcreatedUser.Id);

                    if (appUser is { } && appUser.ApplicationUserId != default)
                    {
                        await _repoAppUser.UpdateAsync(new AppUser(applicationUserId: updatedOrcreatedUser.Id.ToString(), homeAddress: default, homeCity: default, homeRegion: default, homeCountry:default, roleId: default, roleName: default, raffleUserId:default, raffleUserId747: registerUserRequest.Info747.UserId747, raffleUsername747: registerUserRequest.Info747.Username747));

                    } else
                    {
                        appUser = await _repoAppUser.AddAsync(new AppUser(applicationUserId: updatedOrcreatedUser.Id.ToString(), homeAddress: default, homeCity: default, homeRegion: default, homeCountry: default, roleId: default, roleName: default, raffleUserId: default, raffleUserId747: registerUserRequest.Info747.UserId747, raffleUsername747: registerUserRequest.Info747.Username747));
                    }
                }

                // successfully created
                if (updatedOrcreatedUser != default)
                {
                    return new GenericResponse
                    {
                        ErorrCode = 0,
                        Message = createUserRequest.Password
                    };
                }
            }
        }
        else
        {
            // agent here
        }

        return new GenericResponse
        {
            ErorrCode = 1,
            Message = "Generic error, unable to include in the reward and dashboard system."
        };
    }

    // check if player exists
    // phone field in request - insignificant
    private async Task<bool> IsPlayerInRaffleSystemAsync(CheckUserRequest checkUserRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            checkUserRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            string json = JsonSerializer.Serialize(checkUserRequest);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckPlayerExists").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                GenericResponse? result = await response.Content.ReadFromJsonAsync<GenericResponse>();

                if (result is { })
                {
                    if (result.ErorrCode != 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    // register the user, after past arengu's validation
    // check if the user's system affiliation, either agent or player
    private async Task<bool> RegisterPlayerToRaffleSystemAsync(RegisterUserRequest registerUserRequest)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            registerUserRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            string json = JsonSerializer.Serialize(registerUserRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:RegisterPlayerUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                GetUserInfoResponse? result = await response.Content.ReadFromJsonAsync<GetUserInfoResponse>();

                if (result is { })
                {
                    if (result.ErorrCode != 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;
    }

    private string getValueMiddleOfCustomString(string customString)
    {
        int firstStringPosition = customString.IndexOf("747--");
        int secondStringPosition = customString.IndexOf("--747");

        // 5 is the length of the reference string
        return customString.Substring(firstStringPosition + 5, firstStringPosition + secondStringPosition - 5);
    }

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}
