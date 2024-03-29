using Microsoft.Identity.Client;
using UNIFIEDDASHBOARD.WebApi.Application.Identity.Users;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Bridge;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Twilio.Rest.Verify.V2.Service;
using Twilio;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.SendgridTwilio;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using UNIFIEDDASHBOARD.WebApi.Application.Catalog.AppUsers;
using UNIFIEDDASHBOARD.WebApi.Domain.Catalog;
using UNIFIEDDASHBOARD.WebApi.Application.Common.Persistence;
using UNIFIEDDASHBOARD.WebApi.Infrastructure.SendGrid;

namespace UNIFIEDDASHBOARD.WebApi.Host.Controllers.Identity;

public class SendgridController : VersionNeutralApiController
{

    private readonly IConfiguration _config;
    private readonly IUserService _userService;
    private readonly IRepositoryWithEvents<AppUser> _repoAppUser;
    private readonly ISendGridClient _sendGridClient;

    public SendgridController(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser, ISendGridClient sendGridClient)
    {
        _config = config;
        _userService = userService;
        _repoAppUser = repoAppUser;
        _sendGridClient = sendGridClient;
    }

    /// <summary>
    /// Send an email verification code from Twilio (SendGrid).
    /// Recipient is the target user
    /// Channel is the mode of sending. "sms", "email"
    /// </summary>
    /// <param name="VerificationRequest"></param>
    /// <returns></returns>
    [HttpPost("send-code")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("Send an email verification code", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public VerificationResource SendCode([FromBody] TwilioVerificationRequest VerificationRequest)
    {
        try
        {
            TwilioClient.Init(_config.GetSection("SevenFourSevenAPIs:Twilio:AccountSID").Value!, _config.GetSection("SevenFourSevenAPIs:Twilio:AuthToken").Value!);

            return VerificationResource.Create(
                        to: VerificationRequest.Recipient,
                        channel: VerificationRequest.Channel,
                        pathServiceSid: $"{_config.GetSection("SevenFourSevenAPIs:Twilio:ServiceId").Value!}");
        }
        catch
        {
            return default!;
        }

    }

    /// <summary>
    /// Verifies the email sent. Twilio (SendGrid).
    /// </summary>
    /// <param name="twilioCodeRequest"></param>
    /// <returns>VerificationCheckResource.</returns>
    [HttpPost("verify-code")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("Verify code", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public VerificationCheckResource VerifyCodeAsync([FromBody] TwilioCodeRequest twilioCodeRequest)
    {
        TwilioClient.Init(_config.GetSection("SevenFourSevenAPIs:Twilio:AccountSID").Value!, _config.GetSection("SevenFourSevenAPIs:Twilio:AuthToken").Value!);

        return VerificationCheckResource.Create(to: twilioCodeRequest.Recipient, code: twilioCodeRequest.Code, pathServiceSid: $"{_config.GetSection("SevenFourSevenAPIs:Twilio:ServiceId").Value!}");

    }

    /// <summary>
    /// Reset the token from
    /// a. Raffle system
    /// b. Update Unified UI (password) using the new code
    /// c. Send an Email
    /// d. Send a Message to 747 live
    /// </summary>
    /// <param name="userInfoRaffleWithUsername"></param>
    /// <returns>GenericResponse.</returns>
    [HttpPost("reset-token")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("Reset token and send email / message notifications", "")]
    [ApiConventionMethod(typeof(RAFFLEApiConventions), nameof(RAFFLEApiConventions.Register))]
    public async Task<GenericResponse> ResetTokenAsync([FromBody] UserInfoRaffleWithUsernameRequest userInfoRaffleWithUsername)
    {
        SendGridHelper sendGridHelper = new(_userService, _config, _repoAppUser, _sendGridClient);

        // reset token
        // admin's auth code
        userInfoRaffleWithUsername.AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!;

        GetUserInfoResponse getUserInfo = await RaffleUserInfoUsingOwnAuthCodeAsync(userInfoRaffleWithUsername);

        if (getUserInfo is not null && getUserInfo.ErorrCode == 0)
        {
            // this is the user's old auth code
            // before reset
            string oldAuthCode = getUserInfo.AuthCode!;

            AuthCodeResponse raffleUserAuthCode = await ResetRaffleUserAuthCodeAsync(getUserInfo.Email!);

            if (raffleUserAuthCode is not null && raffleUserAuthCode.ErorrCode != 0)
            {
                return new GenericResponse()
                {
                    ErorrCode = 1,
                    Message = $"Raffle reset token error. {raffleUserAuthCode!.Message}"
                };
            }

            // send an email
            // user's auth code
            GenericResponse sendGridMail = await sendGridHelper.SendgridResetMailAsync(raffleUserAuthCode.AuthCode!, getUserInfo.Email!, userInfoRaffleWithUsername.UserName747!);

            if (sendGridMail is not null && sendGridMail.ErorrCode != 0)
            {
                return new GenericResponse()
                {
                    ErorrCode = 1,
                    Message = $"Sendgrid send mail error. {sendGridMail!.Message}"
                };
            }

            // send an bridge message
            // user's auth code
            GenericResponse sendSMSBridgeAsync = await sendGridHelper.SendSMSResetBridgeAsync(raffleUserAuthCode.AuthCode!, userInfoRaffleWithUsername.UserName747!, userInfoRaffleWithUsername.IsAgent);

            if (sendSMSBridgeAsync is not null && sendSMSBridgeAsync.ErorrCode != 0)
            {
                return new GenericResponse()
                {
                    ErorrCode = 1,
                    Message = $"Bridge message send error. {sendSMSBridgeAsync!.Message}"
                };
            }

            // change the password
            // align it with the raffle system
            // user's auth cod
            await ChangePasswordAsync(oldAuthCode, raffleUserAuthCode!.AuthCode, userInfoRaffleWithUsername.UserName747, getUserInfo, userInfoRaffleWithUsername.IsAgent);

            return new GenericResponse()
            {
                ErorrCode = 0,
                Message = $"Reset tasks successful."
            };
        }

        return new GenericResponse()
        {
            ErorrCode = 1,
            Message = $"User info retrieval error. {getUserInfo!.Message}"
        };

    }

    private async Task ChangePasswordAsync(string oldAuthCode, string newAuthCode, string userName, GetUserInfoResponse userInfo, bool IsAgent)
    {
        CancellationToken cancellationToken = default(CancellationToken);

        UserDetailsDto userDetails = await _userService.GetByUsernameAsync(userName, cancellationToken);

        Application.Identity.Users.Password.ChangePasswordRequest request = new Application.Identity.Users.Password.ChangePasswordRequest()
        {
            Password = oldAuthCode,
            NewPassword = newAuthCode,
            ConfirmNewPassword = newAuthCode
        };

        if (userDetails != null)
        {
            // got entry already in the system
            await _userService.ChangePasswordAsync(request, userDetails.Id.ToString());
        }
        else
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
                Password = newAuthCode!,
                ConfirmPassword = newAuthCode!,

                // username can be used on both
                // player or agent
                UserName = IsAgent ? $"{userInfo.AgentInfo!.Username747!}.agent" : $"{userInfo.PlayerInfo!.Username747!}.player",
                PhoneNumber = userInfo.Phone!
            };

            UserDetailsDto? updatedOrCreatedUser = await _userService.CreateSevenFourSevenAsync(createUserRequest, string.Empty, GetOriginFromRequest());

            if (updatedOrCreatedUser is { } && updatedOrCreatedUser.Id != default)
            {
                AppUser? appUser = await _repoAppUser.FirstOrDefaultAsync(new AppUserByApplicationUserIdSpec(updatedOrCreatedUser.Id.ToString()), cancellationToken);

                long? userId747 = IsAgent ? userInfo.AgentInfo!.UserId747 : userInfo.PlayerInfo!.UserId747;
                string? userName747 = IsAgent ? userInfo.AgentInfo!.Username747 : userInfo.PlayerInfo!.Username747;
                string? uniqueCode = IsAgent ? userInfo.AgentInfo!.UniqueCode : userInfo.PlayerInfo!.UniqueCode;
                bool canLinkPlayer = userInfo.CanLinkPlayer;
                bool canLinkAgent = userInfo.CanLinkAgent;
                string? socialCode = userInfo.SocialCode;

                if (appUser is { } && appUser.ApplicationUserId != default)
                {
                    appUser.Update(raffleUserId747: userId747 is null ? string.Empty : userId747.ToString()!, raffleUsername747: userName747 is null ? string.Empty : userName747, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkPlayer, socialCode: socialCode is null ? string.Empty : socialCode);
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

                    appUser = new AppUser(applicationUserId: updatedOrCreatedUser.Id.ToString(), roleId: roleId, roleName: roleName, raffleUserId747: userId747 is null ? string.Empty : userId747.ToString()!, raffleUsername747: userName747 is null ? string.Empty : userName747, isAgent: IsAgent, uniqueCode: uniqueCode is null ? string.Empty : uniqueCode, canLinkPlayer: canLinkPlayer, canLinkAgent: canLinkAgent, socialCode: socialCode is null ? string.Empty : socialCode);

                    appUser.RaffleUsername747 = userName747 is null ? string.Empty : userName747;
                    appUser.RaffleUserId747 = userId747 is null ? string.Empty : userId747.ToString()!;

                    appUser.FacebookUrl = IsAgent ? userInfo.AgentInfo!.FacebookUrl : userInfo.PlayerInfo!.FacebookUrl;
                    appUser.InstagramUrl = IsAgent ? userInfo.AgentInfo!.InstagramUrl : userInfo.PlayerInfo!.InstagramUrl;
                    appUser.TwitterUrl = IsAgent ? userInfo.AgentInfo!.TwitterUrl : userInfo.PlayerInfo!.TwitterUrl;

                    await _repoAppUser.AddAsync(appUser);
                }
            }
        }
    }

    private async Task<AuthCodeResponse> ResetRaffleUserAuthCodeAsync(string Email)
    {
        ResetAuthCodeUserToken resetAuthCodeUserToken = new()
        {
            AuthCode = _config.GetSection("SevenFourSevenAPIs:Raffle:AuthCode").Value!,
            Email = Email
        };
        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Raffle:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonSerializer.Serialize(resetAuthCodeUserToken);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:Raffle:ResetUserToken").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                AuthCodeResponse? result = await response.Content.ReadFromJsonAsync<AuthCodeResponse>();

                if (result is { })
                {
                    if (result.ErorrCode != 0)
                    {
                        return new AuthCodeResponse
                        {
                            AuthCode = result.AuthCode!,
                            ErorrCode = result.ErorrCode,
                            Message = result.Message
                        };
                    }

                    return result;
                }
            }
        }

        return new AuthCodeResponse
        {
            AuthCode = string.Empty,
            ErorrCode = 1,
            Message = "Unsuccessful request. Generic error."
        };
    }

    private async Task<GetUserInfoResponse> RaffleUserInfoUsingOwnAuthCodeAsync(UserInfoRaffleWithUsernameRequest userInfoRaffleWithUsernameRequest)
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

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}