using RAFFLE.WebApi.Application.Catalog.AppUsers;
using RAFFLE.WebApi.Application.Common.Persistence;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.Bridge;
using RAFFLE.WebApi.Application.SevenFourSeven.Raffle;
using RAFFLE.WebApi.Domain.Catalog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Twilio.TwiML.Messaging;

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
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Bridge:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync($"{_config.GetSection("SevenFourSevenAPIs:Bridge:GetHierarchyUrl").Value!}?username={bridgeRequest.UserName}&isAgent={bridgeRequest.IsAgent}");

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
     * send an internal otp using the bridge going to 747live.net
     * this otp will ensure the owner of the user
     */
    [HttpPost("bridge-user-account-ownership-verification")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 bridge checks if the user really owns the account thru internal OTP messaging", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<BridgeGenericResponse> BridgeAccountOwnershipVerificationUserAsync([FromBody] InternalMessageCodeRequest internalMessageCodeRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Bridge:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            internalMessageCodeRequest.AuthToken = _config.GetSection("SevenFourSevenAPIs:Bridge:AuthToken").Value!;

            // from client
            switch (internalMessageCodeRequest.Platform)
            {
                // maybe there's a change in internal mapping
                // reconciling here.
                // agent
                case 1:
                    internalMessageCodeRequest.Platform = int.Parse(_config.GetSection("SevenFourSevenAPIs:Bridge:Platform:Agent").Value!);
                    break;

                // player
                case 2:
                    internalMessageCodeRequest.Platform = int.Parse(_config.GetSection("SevenFourSevenAPIs:Bridge:Platform:Player").Value!);
                    break;
            }

            string json = JsonSerializer.Serialize(internalMessageCodeRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Bridge:SendMessageUrl").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                BridgeGenericResponse? result = await response.Content.ReadFromJsonAsync<BridgeGenericResponse>();

                if (result is { })
                {
                    if (result.Status != 0)
                    {
                        return new BridgeGenericResponse
                        {
                            Status = result.Status,
                            Message = result.Message ?? string.Empty
                        };
                    }

                    result.Message ??= string.Empty;
                    return result;
                }
            }
        }

        return new BridgeGenericResponse { Status = 500, Message = "Unknown error." };
    }

    // check if the user exists
    // this can be used to check
    // each fields can be sent and checked invidually
    // 
    // username
    // email
    // password
    [HttpPost("check-user")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 raffle will be used to check the important fields, if the user actually owns them", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<CheckUserResponse> CheckUserFromRaffleImportantFields([FromBody] CheckUserRequest checkUserRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            checkUserRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            checkUserRequest.Email ??= string.Empty;
            checkUserRequest.Phone ??= string.Empty;
            checkUserRequest.UserName747 ??= string.Empty;

            // number string Zero
            checkUserRequest.UserId747 ??= "0";

            string json = JsonSerializer.Serialize(checkUserRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = new();

            bool isAgent = checkUserRequest.IsAgent ?? false;

            if (isAgent)
                response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckAgentExists").Value!}", data);
            else
                response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckPlayerExists").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                CheckUserResponse? result = await response.Content.ReadFromJsonAsync<CheckUserResponse>();

                if (result is { })
                {
                    return result;
                }
            }
        }

        return new CheckUserResponse { ErorrCode = 500, Message = "Unknown error.", ExistCount = 0 };
    }

    /*
     * Get the user info using the AuthCode during initial registration or reset. This
     * AuthCode is normally sent Eligibile to be in the Raffle system.
     *
     * 1. Check the raffle user info. If true, get the info.
     *    If false, make a record in the raffle system
     *
     */
    [HttpPost("user-info-using-own-auth-code")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 dashboard checks the raffle system if the user is raffle ready. Return email", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<GetUserInfoResponse> RaffleUserInfoUsingOwnAuthCodeAsync([FromBody] GetUserInfoOwnAuthRequest getUserInfoOwnAuthRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonSerializer.Serialize(getUserInfoOwnAuthRequest);

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

    /// <summary>
    /// This is connecting to Raffle API to get the raffle information
    ///
    /// The user is a bridge player. Eligibile to be in the Raffle system.
    ///
    /// 1. Check the raffle user info. If true, get the info.
    ///    If false, make a record in the raffle system
    ///
    /// </summary>
    /// <param name="getUserInfoRequest"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Check the Reward System if he is already part of it, this to be able to get token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("username")]
    [TenantIdHeader]
    [AllowAnonymous]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Users)]
    [OpenApiOperation("Get a user's details.", "")]
    public async Task<UserDetailsDto> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _userService.GetByUsernameAsync(username, cancellationToken) ?? new UserDetailsDto();
    }

    /// <summary>
    /// Self registration on both raffle API and reward system, if the user is not registered yet.
    /// provides a one time password intiutive approach
    /// to allow user's to receive a dashboard generated password one time
    /// this allows the dashboard to authenticate and
    /// redirect back to the client's wherein the client will
    /// call the dashboard api to be given a token
    /// to go around the dashboard system
    /// 
    /// </summary>
    /// <param name="registerUserRequest"></param>
    /// <returns></returns>
    [HttpPost("self-register")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("747 dashboard creates a dashboard user", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<GenericResponse> SelfRegisterAsync([FromBody] RegisterUserRequest registerUserRequest)
    {
        CancellationToken cancellationToken = default(CancellationToken);

        // we have sufficient data here to register the user

        // if user
        bool isUserInRaffleSystem = await IsLegitimateUserInRaffleSystemAsync(new CheckUserRequest
        {
            AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
            Email = registerUserRequest.Email,
            Phone = registerUserRequest.Phone,
            UserId747 = registerUserRequest.Info747.UserId747,
            UserName747 = registerUserRequest.Info747.Username747,
            IsAgent = registerUserRequest.IsAgent
        });

        // create the non player/affiliated user
        if (!isUserInRaffleSystem)
        {
            // hard code sample
            // nothing get's pushed on the main raffle system
            // Production TODO:// enable this.
            // GetUserInfoResponse userInfoResponse = await RegisterUserToRaffleSystemAsync(registerUserRequest);
            GetUserInfoResponse userInfoResponse = new()
            {
                Email = "pahanimg10@gmail.com",
                Phone = registerUserRequest.Phone,
                CanLinkPlayer = registerUserRequest.CanLinkPlayer,
                CanLinkAgent = registerUserRequest.CanLinkAgent,
                AuthCode = "705fed6f-278f-4b6c-aa09-a98c3359998e",
                AgentInfo = new()
                {
                    UniqueCode = registerUserRequest.Info747.UniqueCode,
                    FacebookUrl = registerUserRequest.SocialProfiles.FacebookUrl,
                    InstagramUrl = registerUserRequest.SocialProfiles.InstagramUrl,
                    TwitterUrl = registerUserRequest.SocialProfiles.TwitterUrl,
                    UserId747 = 772027571,
                    Username747 = "pahanimg10"
                },
                CreatedUtc = DateTime.UtcNow,
                ErorrCode = 0,
                Message = "ok",
                Name = registerUserRequest.Name,
                PlayerInfo = new()
                {
                    UniqueCode = registerUserRequest.Info747.UniqueCode,
                    FacebookUrl = registerUserRequest.SocialProfiles.FacebookUrl,
                    InstagramUrl = registerUserRequest.SocialProfiles.InstagramUrl,
                    TwitterUrl = registerUserRequest.SocialProfiles.TwitterUrl,
                    UserId747 = 772027571,
                    Username747 = "pahanimg10"
                },
                SocialCode = "K8LAZN",
                Surname = registerUserRequest.Surname
            };

            if (userInfoResponse.ErorrCode == 0)
            {
                // use the raffle auth code as the password
                // for the universal dashboard
                string? password = userInfoResponse.AuthCode!;

                // the user is successfully registered on our raffle system
                // it is safe to give him access to the dashboard now
                CreateUserRequest createUserRequest = new CreateUserRequest
                {
                    Email = userInfoResponse.Email!,
                    FirstName = userInfoResponse.Name!,
                    LastName = userInfoResponse.Surname!,
                    Password = password!,
                    ConfirmPassword = password!,
                    UserName = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.Username747! : userInfoResponse.PlayerInfo!.Username747!,
                    PhoneNumber = userInfoResponse.Phone!
                };

                UserDetailsDto? updatedOrcreatedUser = await _userService.CreateSevenFourSevenAsync(createUserRequest, string.Empty, GetOriginFromRequest());

                if (updatedOrcreatedUser is { } && updatedOrcreatedUser.Id != default)
                {
                    AppUser? appUser = await _repoAppUser.FirstOrDefaultAsync(new AppUserByApplicationUserIdSpec(updatedOrcreatedUser.Id.ToString()), cancellationToken);

                    long userId747 = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.UserId747! : userInfoResponse.PlayerInfo!.UserId747!;
                    string userName747 = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.Username747! : userInfoResponse.PlayerInfo!.Username747!;
                    string uniqueCode = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.UniqueCode! : userInfoResponse.PlayerInfo!.UniqueCode!;
                    bool canLinkPlayer = userInfoResponse.CanLinkPlayer;
                    bool canLinkAgent = userInfoResponse.CanLinkAgent;
                    string socialCode = userInfoResponse.SocialCode!;

                    if (appUser is { } && appUser.ApplicationUserId != default)
                    {
                        appUser.Update(raffleUserId747: userId747.ToString(), raffleUsername747: userName747, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkPlayer, socialCode: socialCode);
                        await _repoAppUser.UpdateAsync(appUser);
                    }
                    else
                    {
                        string roleId = string.Empty;
                        string roleName = string.Empty;
                        List<UserRoleDto>? userRoles = await _userService.GetRolesAsync(updatedOrcreatedUser.Id.ToString(), cancellationToken);
                        if (userRoles is { } && userRoles.Any())
                        {
                            UserRoleDto? userRole = userRoles.Where(r => r.RoleName == "Basic").FirstOrDefault();

                            if (userRole is { })
                            {
                                roleId = userRole.RoleId!;
                                roleName = userRole.RoleName!;
                            }
                        }

                        appUser = new AppUser(applicationUserId: updatedOrcreatedUser.Id.ToString(), roleId: roleId, roleName: roleName, raffleUserId747: userId747.ToString(), raffleUsername747: userName747, isAgent: registerUserRequest.IsAgent, uniqueCode: uniqueCode, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkAgent, socialCode: socialCode);

                        appUser.RaffleUsername747 = userName747;
                        appUser.RaffleUserId747 = userId747.ToString();

                        appUser.FacebookUrl = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.FacebookUrl! : userInfoResponse.PlayerInfo!.FacebookUrl!;
                        appUser.InstagramUrl = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.InstagramUrl! : userInfoResponse.PlayerInfo!.InstagramUrl!;
                        appUser.TwitterUrl = registerUserRequest.IsAgent ? userInfoResponse.AgentInfo!.TwitterUrl! : userInfoResponse.PlayerInfo!.TwitterUrl!;

                        await _repoAppUser.AddAsync(appUser);
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

        return new GenericResponse
        {
            ErorrCode = 1,
            Message = "Generic error, unable to include in the reward and dashboard system."
        };
    }

    /// <summary>
    /// Check if the user is a Player in Raffle system
    /// </summary>
    /// <param name="checkUserRequest"></param>
    /// <returns>bool</returns>
    [HttpPost("is-legitimate-user")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("Check if player exists in the raffle system", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<bool> IsLegitimateUserInRaffleSystemAsync([FromBody] CheckUserRequest checkUserRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            checkUserRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            string json = JsonSerializer.Serialize(checkUserRequest);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            string urlToCheckExistence = string.Empty;
            bool checkUserType = checkUserRequest.IsAgent ?? false;

            if (!checkUserType)
                urlToCheckExistence = $"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckPlayerExists").Value!}";
            else
                urlToCheckExistence = $"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckAgentExists").Value!}";

            HttpResponseMessage response = await client.PostAsync(urlToCheckExistence, data);

            if (response.IsSuccessStatusCode)
            {
                GenericExistCountResponse? result = await response.Content.ReadFromJsonAsync<GenericExistCountResponse>();

                if (result is { })
                {
                    if (result.ErorrCode != 0)
                    {
                        return false;
                    }
                    else
                    {
                        if (result.ExistCount > 0)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
        }

        return false;
    }

    private async Task<GetUserInfoResponse> RegisterUserToRaffleSystemAsync(RegisterUserRequest registerUserRequest)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            registerUserRequest.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

            string json = JsonSerializer.Serialize(registerUserRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            string urlToCheckExistence = string.Empty;
            bool checkUserType = registerUserRequest.IsAgent;

            if (!checkUserType)
                urlToCheckExistence = $"{_config.GetSection("SevenFourSevenAPIs:Raffle:RegisterPlayerUrl").Value!}";
            else
                urlToCheckExistence = $"{_config.GetSection("SevenFourSevenAPIs:Raffle:RegisterAgentUrl").Value!}";

            HttpResponseMessage response = await client.PostAsync(urlToCheckExistence, data);

            if (response.IsSuccessStatusCode)
            {
                GetUserInfoResponse? result = await response.Content.ReadFromJsonAsync<GetUserInfoResponse>();

                if (result is { })
                {
                    return result;
                }
            }
        }

        return new GetUserInfoResponse
        {
            ErorrCode = 1,
            Message = "Generic error."
        };
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
