using SendGrid;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;
using UNIFIEDDASHBOARD.WebApi.Application.Common.Persistence;
using UNIFIEDDASHBOARD.WebApi.Application.Identity.Users;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Bridge;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;
using UNIFIEDDASHBOARD.WebApi.Domain.Catalog;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.SendGrid;

namespace UNIFIEDDASHBOARD.WebApi.Host.Controllers.Identity;

public class SevenFourSevenController : VersionNeutralApiController
{
    private readonly IUserService _userService;
    private readonly IRepositoryWithEvents<AppUser> _repoAppUser;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ISendGridClient _sendGridClient;

    public SevenFourSevenController(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser, IHttpClientFactory clientFactory, ISendGridClient sendGridClient)
    {
        _userService = userService;
        _config = config;
        _repoAppUser = repoAppUser;
        _clientFactory = clientFactory;
        _sendGridClient = sendGridClient;
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
        using (HttpClient? client = _clientFactory.CreateClient("Raffle.ByPass.API"))
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
        UserInfoRaffleWithUsernameRequest userInfoRaffleWithUsername;

        SendGridHelper sendGridHelper = new(_userService, _config, _repoAppUser, _sendGridClient);

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
            GetUserInfoResponse userInfoResponse = await RegisterUserToRaffleSystemAsync(registerUserRequest);

            if (userInfoResponse.ErorrCode == 0)
            {
                string? password = userInfoResponse.AuthCode ??= null;

                if (password is not null)
                {
                    userInfoRaffleWithUsername = new UserInfoRaffleWithUsernameRequest()
                    {
                        AuthCode = password,
                        IsAgent = registerUserRequest.IsAgent,
                        UserName747 = registerUserRequest.Info747.Username747!

                    };
                }
                else
                {
                    return new GenericResponse
                    {
                        ErorrCode = 1,
                        Message = "Generic error. Authcode error. Unknown"
                    };
                }
            }
            else
            {
                return new GenericResponse
                {
                    ErorrCode = userInfoResponse.ErorrCode,
                    Message = userInfoResponse.Message
                };
            }
        }
        else
        {
            // reset
            // use main
            userInfoRaffleWithUsername = new UserInfoRaffleWithUsernameRequest()
            {
                AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
                IsAgent = registerUserRequest.IsAgent,
                UserName747 = registerUserRequest.Info747.Username747!

            };
        }

        // the user is in the raffle now
        // must be
        // reset token
        // admin's auth code
        GetUserInfoResponse getUserInfo = await RaffleUserInfoUsingCodeAsync(userInfoRaffleWithUsername);

        if (getUserInfo is not null && getUserInfo.ErorrCode == 0)
        {
            await SelfRegisterAsync(getUserInfo, registerUserRequest.IsAgent);

            // only for the newly created
            if (!isUserInRaffleSystem)
            {
                // send an email
                // user's auth code
                GenericResponse sendGridMail = await sendGridHelper.SendgridResetMailAsync(getUserInfo.AuthCode!, getUserInfo.Email!, userInfoRaffleWithUsername.UserName747!);

                if (sendGridMail is not null && sendGridMail.ErorrCode != 0)
                {
                    return new GenericResponse()
                    {
                        ErorrCode = 1,
                        Message = $"SendGrid send mail error. {sendGridMail!.Message}"
                    };
                }

                // send an bridge message
                // user's auth code
                GenericResponse sendSMSBridgeAsync = await sendGridHelper.SendSMSResetBridgeAsync(getUserInfo.AuthCode!, userInfoRaffleWithUsername.UserName747!, userInfoRaffleWithUsername.IsAgent);

                if (sendSMSBridgeAsync is not null && sendSMSBridgeAsync.ErorrCode != 0)
                {
                    return new GenericResponse()
                    {
                        ErorrCode = 1,
                        Message = $"Bridge message send error. {sendSMSBridgeAsync!.Message}"
                    };
                }
            }

            return new GenericResponse
            {
                ErorrCode = getUserInfo.ErorrCode,

                // auth code
                Message = getUserInfo.AuthCode ?? string.Empty
            };
        }
        else
        {
            return new GenericResponse
            {
                ErorrCode = 1,
                Message = "Error getting user info."
            };
        }
    }

    private async Task<GetUserInfoResponse> RaffleUserInfoUsingCodeAsync(UserInfoRaffleWithUsernameRequest userInfoRaffleWithUsernameRequest)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonSerializer.Serialize(userInfoRaffleWithUsernameRequest);

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

    private async Task SelfRegisterAsync(GetUserInfoResponse userInfo, bool isAgent)
    {
        CancellationToken cancellationToken = default(CancellationToken);

        string userName = isAgent ? $"{userInfo.AgentInfo!.Username747!}.agent" : $"{userInfo.PlayerInfo!.Username747!}.player";

        UserDetailsDto userDetails = await _userService.GetByUsernameAsync(userName, cancellationToken);

        if (userDetails is null)
        {
            // self register
            // for the universal dashboard

            // the user is successfully registered on our raffle system
            // it is safe to give him access to the dashboard now
            CreateUserRequest createUserRequest = new CreateUserRequest
            {
                Email = userInfo.Email!,
                FirstName = userInfo.Name!,
                LastName = userInfo.Surname!,
                Password = userInfo.AuthCode!,
                ConfirmPassword = userInfo.AuthCode!,

                // username can be used on both
                // player or agent
                UserName = userName,
                PhoneNumber = userInfo.Phone!
            };

            UserDetailsDto? updatedOrCreatedUser = await _userService.CreateSevenFourSevenAsync(createUserRequest, string.Empty, GetOriginFromRequest());

            if (updatedOrCreatedUser is { } && updatedOrCreatedUser.Id != default)
            {
                AppUser? appUser = await _repoAppUser.FirstOrDefaultAsync(new AppUserByApplicationUserIdSpec(updatedOrCreatedUser.Id.ToString()), cancellationToken);

                long userId747 = isAgent ? (userInfo.AgentInfo!.UserId747 ?? 0) : (userInfo.PlayerInfo!.UserId747 ?? 0);
                string userName747 = isAgent ? (userInfo.AgentInfo!.Username747 ?? string.Empty) : (userInfo.PlayerInfo!.Username747 ?? string.Empty);
                string uniqueCode = isAgent ? (userInfo.AgentInfo!.UniqueCode ?? string.Empty) : (userInfo.PlayerInfo!.UniqueCode ?? string.Empty);
                bool canLinkPlayer = userInfo.CanLinkPlayer;
                bool canLinkAgent = userInfo.CanLinkAgent;
                string socialCode = userInfo.SocialCode!;

                if (appUser is { } && appUser.ApplicationUserId != default)
                {
                    appUser.Update(raffleUserId747: userId747.ToString(), raffleUsername747: userName747, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkPlayer, socialCode: socialCode);
                    await _repoAppUser.UpdateAsync(appUser);
                }
                else
                {
                    string roleId = string.Empty;
                    string roleName = string.Empty;
                    List<UserRoleDto>? userRoles = await _userService.GetRolesAsync(updatedOrCreatedUser.Id.ToString(), cancellationToken);
                    if (userRoles is { } && userRoles.Any())
                    {
                        UserRoleDto? userRole = userRoles.Where(r => r.RoleName == "Basic").FirstOrDefault();

                        if (userRole is { })
                        {
                            roleId = userRole.RoleId!;
                            roleName = userRole.RoleName!;
                        }
                    }

                    appUser = new AppUser(applicationUserId: updatedOrCreatedUser.Id.ToString(), roleId: roleId, roleName: roleName, raffleUserId747: userId747.ToString(), raffleUsername747: userName747, isAgent: isAgent, uniqueCode: uniqueCode, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkAgent, socialCode: socialCode);

                    appUser.RaffleUsername747 = userName747;
                    appUser.RaffleUserId747 = userId747.ToString();

                    appUser.FacebookUrl = isAgent ? (userInfo.AgentInfo!.FacebookUrl ?? null) : (userInfo.PlayerInfo!.FacebookUrl ?? null);
                    appUser.InstagramUrl = isAgent ? (userInfo.AgentInfo!.InstagramUrl ?? null) : (userInfo.PlayerInfo!.InstagramUrl ?? null);
                    appUser.TwitterUrl = isAgent ? (userInfo.AgentInfo!.TwitterUrl ?? null) : (userInfo.PlayerInfo!.TwitterUrl ?? null);

                    await _repoAppUser.AddAsync(appUser);
                }
            }
        }
    }

    /// <summary>
    /// Check if the user is a Player in Raffle system
    /// </summary>
    /// <param name="checkUserRequest"></param>
    /// <returns>bool.</returns>
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
        using (HttpClient client = new())
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


    /// <summary>
    /// Send the email to user via it can login.
    /// </summary>
    /// <param name="userName747"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("send-login-email")]
    [TenantIdHeader]
    [AllowAnonymous]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Users)]
    [OpenApiOperation("Send the user an email with a link through which he can login", "")]
    public async Task<GenericResponse> SendLoginEmail(string userName747)
    {
        var isUserAgant = await IsUserAgent(userName747);

        var user = await RaffleUserInfoUsingCodeAsync(new UserInfoRaffleWithUsernameRequest
        {
            AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
            IsAgent = isUserAgant,
            UserName747 = userName747
        });

        if (user == null)
        {
            return new GenericResponse
            {
                ErorrCode = 1,
                Message = "Error getting user info."
            };
        }

        if (user.ErorrCode == 0)
        {
            SendGridHelper sendGridHelper = new(_userService, _config, _repoAppUser, _sendGridClient);
            var sendSMSBridgeAsync = await sendGridHelper.SendSMSLoginBridgeAsync(user.AuthCode!, userName747!, isUserAgant);
            var sendEmailBridgeAsync = await sendGridHelper.SendgridLoginMailAsync(user.AuthCode!, user.Email!, userName747!);
            return new GenericResponse
            {
                ErorrCode = sendEmailBridgeAsync.ErorrCode == 0 || sendSMSBridgeAsync.ErorrCode == 0 ?
                    0 : sendSMSBridgeAsync.ErorrCode + sendSMSBridgeAsync.ErorrCode,
                Message = sendEmailBridgeAsync.Message + sendSMSBridgeAsync.Message

            };
        }
        else
        {
            return new GenericResponse { ErorrCode = user.ErorrCode, Message = user.Message };
        }

    }

    private async Task<bool> IsUserAgent(string userName747)
    {
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var checkUserRequest = new CheckUserRequest
            {
                AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
                Email = string.Empty,
                Phone = string.Empty,
                // number string Zero
                UserId747 = "0",
                UserName747 = userName747
            };

            string json = JsonSerializer.Serialize(checkUserRequest);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage responseAgent = await client
                .PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:CheckAgentExists").Value!}", data);
            if (responseAgent.IsSuccessStatusCode)
            {
                GenericExistCountResponse? resultAgentExist = await responseAgent.Content.ReadFromJsonAsync<GenericExistCountResponse>();

                if (resultAgentExist == null || resultAgentExist.ErorrCode != 0 || resultAgentExist.ExistCount <= 0)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
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
