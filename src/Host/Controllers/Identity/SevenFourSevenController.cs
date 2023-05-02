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

        // craete the non player/affiliated user
        if (!isUserInRaffleSystem)
        {
            registerUserRequest.SocialProfiles.FacebookUrl = string.Empty;
            registerUserRequest.SocialProfiles.InstagramUrl = string.Empty;
            registerUserRequest.SocialProfiles.TwitterUrl = string.Empty;
            _ = await RegisterUserToRaffleSystemAsync(registerUserRequest);
        }

        // recheck if player in the raffle system, after operation
        isUserInRaffleSystem = await IsLegitimateUserInRaffleSystemAsync(new CheckUserRequest
        {
            AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
            Email = registerUserRequest.Email,
            Phone = registerUserRequest.Phone,
            UserId747 = registerUserRequest.Info747.UserId747,
            UserName747 = registerUserRequest.Info747.Username747,
            IsAgent = registerUserRequest.IsAgent
        });

        if (isUserInRaffleSystem)
        {
            // TODO:// To be implemented.
            // make it more secure 
            // hash generated that will check the user from the server
            // against external user
            // This additional secret: {_config.GetSection("SevenFourSevenAPIs:PasswordSecretHash").Value!}
            string? password = $"{getValueMiddleOfCustomString(registerUserRequest.OwnUserAuthCode)}";
            string? confirmPassword = $"{getValueMiddleOfCustomString(registerUserRequest.OwnUserAuthCode)}";

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
                AppUser? appUser = await _repoAppUser.FirstOrDefaultAsync(new AppUserByApplicationUserIdSpec(updatedOrcreatedUser.Id.ToString()), cancellationToken);

                if (appUser is { } && appUser.ApplicationUserId != default)
                {
                    appUser.Update(raffleUserId747: registerUserRequest.Info747.UserId747, raffleUsername747: registerUserRequest.Info747.Username747, canLinkPlayer: registerUserRequest.CanLinkPlayer, canLinkAgent: registerUserRequest.CanLinkAgent, socialCode: registerUserRequest.SocialCode);
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

                    appUser = new AppUser(applicationUserId: updatedOrcreatedUser.Id.ToString(), roleId: roleId, roleName: roleName, raffleUserId747: registerUserRequest.Info747.UserId747, raffleUsername747: registerUserRequest.Info747.Username747, isAgent: registerUserRequest.IsAgent, uniqueCode: registerUserRequest.Info747.UniqueCode, canLinkPlayer: registerUserRequest.CanLinkPlayer, canLinkAgent: registerUserRequest.CanLinkAgent, socialCode: registerUserRequest.SocialCode);

                    if (registerUserRequest.Info747 is { })
                    {
                        appUser.RaffleUsername747 = registerUserRequest.Info747.Username747;
                        appUser.RaffleUserId747 = registerUserRequest.Info747.UserId747;
                    }

                    if (registerUserRequest.SocialProfiles is { })
                    {
                        appUser.FacebookUrl = registerUserRequest.SocialProfiles.FacebookUrl;
                        appUser.InstagramUrl = registerUserRequest.SocialProfiles.InstagramUrl;
                        appUser.TwitterUrl = registerUserRequest.SocialProfiles.TwitterUrl;
                    }

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
    [HttpPost("is-player")]
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

    // register the user, after past arengu's validation
    // check if the user's system affiliation, either agent or player
    private async Task<bool> RegisterUserToRaffleSystemAsync(RegisterUserRequest registerUserRequest)
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
