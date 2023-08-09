using Microsoft.Extensions.Configuration;
using UNIFIEDDASHBOARD.WebApi.Application.Common.Persistence;
using UNIFIEDDASHBOARD.WebApi.Application.Identity.Users;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Bridge;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.Raffle;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.SendgridTwilio;
using UNIFIEDDASHBOARD.WebApi.Domain.Catalog;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UNIFIEDDASHBOARD.WebApi.Infrastructure.SendGrid
{
    public class SendGridHelper
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IRepositoryWithEvents<AppUser> _repoAppUser;
        private readonly ISendGridClient _sendGridClient;

        public SendGridHelper(IUserService userService, IConfiguration config, IRepositoryWithEvents<AppUser> repoAppUser, ISendGridClient sendGridClient)
        {
            _config = config;
            _userService = userService;
            _repoAppUser = repoAppUser;
            _sendGridClient = sendGridClient;
        }
        public Task<GenericResponse> SendgridResetMailAsync(string AuthCode, string Email, string Name)
        {
            return SendgridMailAsync(Email, Name,
                $"Hi, authorization reset requested. This is your 747Live Reward System authorization path: {_config.GetSection("MainRewardSystem:BaseUrl").Value!}?AuthCode={AuthCode}. " +
                $"Copy and paste this path in your browser's address. Congratulations and welcome back to 747 live, enjoy and good luck." +
                $"If you have not made this request, please kindly ignore and/or contact support. Thank you very much. Yours, 747Live Reward Systems.",

                $"Hi,<br /><br />" +
                $"Authorization reset requested." +
                $"<br /><br />" +
                $"This is your 747Live Reward System authorization <strong><a href='{_config.GetSection("MainRewardSystem:BaseUrl").Value!}/?AuthCode={AuthCode}'>{AuthCode}</a></strong>" +
                $"<br /><br />" +
                $"If you have not made this request, please kindly ignore and/or contact support." +
                $"<br /><br />" +
                $"Thank you very much." +
                $"<br /><br />" +
                $"Yours," +
                $"<br /><br />" +
                $"747Live Reward Systems");
        }
        public Task<GenericResponse> SendgridLoginMailAsync(string AuthCode, string Email, string Name)
        {
            return SendgridMailAsync(Email, Name,
                $"Hi, authorization link requested. This is your 747Live Reward System authorization path: {_config.GetSection("MainRewardSystem:BaseUrl").Value!}?AuthCode={AuthCode}. " +
                $"Copy and paste this path in your browser's address. Congratulations and welcome back to 747 live, enjoy and good luck." +
                $"If you have not made this request, please kindly ignore and/or contact support. Thank you very much. Yours, 747Live Reward Systems.",

                $"Hi,<br /><br />" +
                $"Authorization link  requested." +
                $"<br /><br />" +
                $"This is your 747Live Reward System authorization <strong><a href='{_config.GetSection("MainRewardSystem:BaseUrl").Value!}/?AuthCode={AuthCode}'>{AuthCode}</a></strong>" +
                $"<br /><br />" +
                $"If you have not made this request, please kindly ignore and/or contact support." +
                $"<br /><br />" +
                $"Thank you very much." +
                $"<br /><br />" +
                $"Yours," +
                $"<br /><br />" +
                $"747Live Reward Systems");
        }

        public async Task<GenericResponse> SendgridMailAsync(string Email, string Name, string plainTextContent, string htmlContent)
        {
            SendgridMailRequest sendgridMailRequest = new SendgridMailRequest()
            {
                Email = Email,
                Name = Name
            };
            // var client = new SendGridClient(_config.GetSection("SevenFourSevenAPIs:Sendgrid:ApiKey").Value!);
            EmailAddress from = new EmailAddress(_config.GetSection("SevenFourSevenAPIs:Sendgrid:Email").Value!, _config.GetSection("SevenFourSevenAPIs:Sendgrid:Name").Value!);
            string subject = _config.GetSection("SevenFourSevenAPIs:Sendgrid:Subject").Value!;
            EmailAddress to = new EmailAddress(sendgridMailRequest.Email, sendgridMailRequest.Name);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response is not null && response.IsSuccessStatusCode)
            {
                return new GenericResponse
                {
                    ErorrCode = 0,
                    Message = "Email successful."
                };
            }

            return new GenericResponse
            {
                ErorrCode = 1,
                Message = "Unsuccessful request. Generic error."
            };
        }

        public Task<GenericResponse> SendSMSResetBridgeAsync(string AuthCode, string UserName, bool IsAgent)
        {
            return SendSMSBridgeAsync(new InternalMessageCodeRequest()
            {
                AuthToken = _config.GetSection("SevenFourSevenAPIs:Bridge:AuthToken").Value!,
                Message = $"Hi,<br /><br />" +
                $"Authorization reset requested." +
                $"<br /><br />" +
                $"This is your 747Live Reward System authorization <strong><a href='{_config.GetSection("MainRewardSystem:BaseUrl").Value!}/?AuthCode={AuthCode}'>{AuthCode}</a></strong>" +
                $"<br /><br />" +
                $"If you have not made this request, please kindly ignore and/or contact support." +
                $"<br /><br />" +
                $"Thank you very much." +
                $"<br /><br />" +
                $"Yours," +
                $"<br /><br />" +
                $"747Live Reward Systems",
                Platform = IsAgent ? 1 : 2,
                Subject = _config.GetSection("SevenFourSevenAPIs:Sendgrid:Subject").Value!,
                Username = UserName
            });
        }

        public Task<GenericResponse> SendSMSLoginBridgeAsync(string AuthCode, string UserName, bool IsAgent)
        {
            return SendSMSBridgeAsync(new InternalMessageCodeRequest()
            {
                AuthToken = _config.GetSection("SevenFourSevenAPIs:Bridge:AuthToken").Value!,
                Message = $"Hi,<br /><br />" +
                $"Authorization link requested." +
                $"<br /><br />" +
                $"This is your 747Live Reward System authorization <strong><a href='{_config.GetSection("MainRewardSystem:BaseUrl").Value!}/?AuthCode={AuthCode}'>{AuthCode}</a></strong>" +
                $"<br /><br />" +
                $"If you have not made this request, please kindly ignore and/or contact support." +
                $"<br /><br />" +
                $"Thank you very much." +
                $"<br /><br />" +
                $"Yours," +
                $"<br /><br />" +
                $"747Live Reward Systems",
                Platform = IsAgent ? 1 : 2,
                Subject = _config.GetSection("SevenFourSevenAPIs:Sendgrid:Subject").Value!,
                Username = UserName
            });
        }

        public async Task<GenericResponse> SendSMSBridgeAsync(InternalMessageCodeRequest internalMessageCodeRequest)
        {
            using (HttpClient? client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:Bridge:BaseUrl").Value!);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
                            return new GenericResponse
                            {
                                ErorrCode = result.Status,
                                Message = result.Message ?? string.Empty
                            };
                        }

                        return new GenericResponse
                        {
                            ErorrCode = 0,
                            Message = result.Message ?? string.Empty
                        };
                    }
                }
            }

            return new GenericResponse
            {
                ErorrCode = 1,
                Message = "Generic error. Unknow."
            };
        }
    }
}