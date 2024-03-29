using Mapster;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UNIFIEDDASHBOARD.WebApi.Host.Controllers.Identity;

// TODO: FluentValidation, HttpClientFactory with token handler
public class InkMeLiveController : VersionNeutralApiController
{
    private readonly IConfiguration _config;

    public InkMeLiveController(IConfiguration config)
    {
        _config = config;
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

    [HttpPost("get-player/{userName}")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Get the player details.", "")]
    public async Task<PlayersModel> GetPlayerAsync(string userName)
    {
        InkMeLiveTokenResponse token = await GetTokenAsync() ?? default!;

        try
        {
            using (HttpClient? client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (token is not null && !string.IsNullOrWhiteSpace(token.AuthToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

                HttpResponseMessage response = await client.GetAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:GetDetailsByUserName").Value!}?playerUsername={userName}");

                InkMeLiveApiWithPlayerModelResponse result = await response.Content.ReadFromJsonAsync<InkMeLiveApiWithPlayerModelResponse>() ?? default!;

                if (result is not null && result.IsSuccess)
                {
                    if (result.Data is not null)
                    {
                        PlayersModel player = result.Data.Adapt<PlayersModel>();

                        if (player != null && !string.IsNullOrWhiteSpace(player.IDProofBackSide))
                            player.IDProofBackSide = Path.Combine(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!, player.IDProofBackSidePath!);

                        if (player != null && !string.IsNullOrWhiteSpace(player.IDProofFrontSide))
                            player.IDProofFrontSide = Path.Combine(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!, player.IDProofFrontSidePath!);

                        if (player != null && !string.IsNullOrWhiteSpace(player.DigitalSignature))
                        {
                            var path = Path.Combine(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!, player.DigitalSignaturePath!);

                            using var httClient = new HttpClient();
                            var imageBytes = await httClient.GetByteArrayAsync(path);
                            var base64 = Convert.ToBase64String(imageBytes);
                            player.DigitalSignature = $"data:image/png;base64,{base64}";
                        }

                        return player ?? default!;
                    }
                }
            }
        }
        catch
        {
            return default!;
        }

        return default!;
    }

    [HttpPost("delete-player/{userName}")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Delete an ink me live player data.", "")]
    public async Task<InkMeLiveApiResponse> DeletePlayer(string userName)
    {
        InkMeLiveTokenResponse token = await GetTokenAsync() ?? default!;

        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (token is not null && !string.IsNullOrWhiteSpace(token.AuthToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

            HttpResponseMessage response = await client.GetAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:DeletePlayer").Value!}?playerUsername={userName}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InkMeLiveApiResponse>() ?? default!;
            }
        }

        return default!;
    }

    [HttpPost("create-applicant-player")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Create an ink me live player applicant.", "")]
    public async Task<InkMeLiveApiResponse> CreateApplicantPlayerAsync([FromBody] InkMeLivePlayerDetailsRequest inkMeLivePlayerDetailsRequest)
    {
        InkMeLiveTokenResponse token = await GetTokenAsync() ?? default!;

        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (token is not null && !string.IsNullOrWhiteSpace(token.AuthToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

            string json = JsonSerializer.Serialize(inkMeLivePlayerDetailsRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:SignUp").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InkMeLiveApiResponse>() ?? default!;
            }
        }

        return default!;
    }

    [HttpPost("update-applicant-player")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Update an ink me live player applicant.", "")]
    public async Task<InkMeLiveApiResponse> UpdateApplicantPlayerAsync([FromBody] InkMeLivePlayerDetailsRequest inkMeLivePlayerDetailsRequest)
    {
        InkMeLiveTokenResponse token = await GetTokenAsync() ?? default!;

        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (token is not null && !string.IsNullOrWhiteSpace(token.AuthToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

            string json = JsonSerializer.Serialize(inkMeLivePlayerDetailsRequest);

            StringContent? data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:UpdatePlayer").Value!}", data);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InkMeLiveApiResponse>() ?? default!;
            }
        }

        return default!;
    }

    [HttpPost("applicant-player-attachments")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [RequestSizeLimit(300_000_000)] // 300 mb
    [OpenApiOperation("Update an ink me live player applicant attachments.", "")]
    public async Task<InkMeLiveApiResponse> ApplicantPlayerAttachmentsAsync([FromForm] InkMeLivePlayerAttachmentsRequest request)
    {
        if (request.Attachments.Count is 0 || request.Attachments.All(x => x.Length is 0))
            return default!;

        var token = await GetTokenAsync();
        using var client = new HttpClient
        {
            BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!)
        };

        if (!string.IsNullOrWhiteSpace(token.AuthToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

        using var formContent = new MultipartFormDataContent("NKdKd9Yk");

        formContent.Add(new StringContent(request.PlayerUsername, Encoding.UTF8), "PlayerUsername");

        foreach (var attachment in request.Attachments)
        {
            var streamContent = new StreamContent(attachment.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            formContent.Add(streamContent, "Attachments", attachment.FileName);
        }

        var playerAttachmentsPath = _config.GetRequiredSection("SevenFourSevenAPIs:InkMeLive:PlayerAttachments").Value;
        var response = await client.PostAsync(playerAttachmentsPath, formContent);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<InkMeLiveApiResponse>() ?? default!
            : default!;
    }

    [HttpPost("applicant-player-agreement")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Update an ink me live player applicant agreement.", "")]
    public async Task<InkMeLiveApiResponse> ApplicantPlayerAgreementAsync([FromBody] InkMeLivePlayerAgreementRequest playerAgreementRequest)
    {
        var token = await GetTokenAsync();
        using var client = new HttpClient
        {
            BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!)
        };

        if (!string.IsNullOrWhiteSpace(token.AuthToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

        using var formContent = new MultipartFormDataContent("NKdKd9Yk");
        var fileName = $"{playerAgreementRequest.PlayerUserName}.Agreement.{DateTime.UtcNow:yyyyMMdd_hhmmss}{playerAgreementRequest.AgreementFileExtension}";
        formContent.Add(new ByteArrayContent(playerAgreementRequest.Agreement), "agreement", fileName);

        var playerAgreementPath = _config.GetRequiredSection("SevenFourSevenAPIs:InkMeLive:PlayerAgreement").Value;
        var playerAgreementPathWithQueryParameters = $"{playerAgreementPath}?playerUserName={playerAgreementRequest.PlayerUserName}";

        var response = await client.PostAsync(playerAgreementPathWithQueryParameters, formContent);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<InkMeLiveApiResponse>() ?? default!
            : default!;
    }
}