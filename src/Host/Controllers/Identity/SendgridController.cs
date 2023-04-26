using Microsoft.Identity.Client;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.Bridge;
using RAFFLE.WebApi.Application.SevenFourSeven.Raffle;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Twilio.Rest.Verify.V2.Service;
using Twilio;
using RAFFLE.WebApi.Application.SevenFourSeven.SendgridTwilio;

namespace RAFFLE.WebApi.Host.Controllers.Identity;

public class SendgridController : VersionNeutralApiController
{

    private readonly IConfiguration _config;

    public SendgridController(IConfiguration config)
    {
        _config = config;
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
        TwilioClient.Init(_config.GetSection("SevenFourSevenAPIs:Twilio:AccountSID").Value!, _config.GetSection("SevenFourSevenAPIs:Twilio:AuthToken").Value!);

        return VerificationResource.Create(
                    to: VerificationRequest.Recipient,
                    channel: VerificationRequest.Channel,
                    pathServiceSid: $"{_config.GetSection("SevenFourSevenAPIs:Twilio:ServiceId").Value!}");

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
}
