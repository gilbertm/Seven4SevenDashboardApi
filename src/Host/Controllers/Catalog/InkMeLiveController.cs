using Mapster;
using RAFFLE.WebApi.Application.Common.Persistence;
using RAFFLE.WebApi.Application.Identity.Tokens;
using RAFFLE.WebApi.Application.Identity.Users;
using RAFFLE.WebApi.Application.SevenFourSeven.Bridge;
using RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;
using RAFFLE.WebApi.Domain.Catalog;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Image = System.Drawing.Image;

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

    [HttpPost("get-player/{userName}")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.Raffles)]
    [OpenApiOperation("Get the player details.", "")]
    public async Task<PlayersModel> GetPlayerAsync(string userName)
    {
        InkMeLiveTokenResponse token = await GetTokenAsync() ?? default!;

        using (HttpClient? client = new HttpClient())
        {
            client.BaseAddress = new Uri(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (token is not null && !string.IsNullOrWhiteSpace(token.AuthToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AuthToken);

            HttpResponseMessage response = await client.GetAsync($"{_config.GetSection("SevenFourSevenAPIs:InkMeLive:GetDetailsByUserName").Value!}?playerUsername={userName}");

            if (response.IsSuccessStatusCode)
            {
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
                            string path = Path.Combine(_config.GetSection("SevenFourSevenAPIs:InkMeLive:BaseUrl").Value!, player.DigitalSignaturePath!);

                            using HttpClient httClient = new HttpClient();
                            byte[] imageBytes = await httClient.GetByteArrayAsync(path);
                            player.DigitalSignature = Encoding.UTF8.GetString(imageBytes);
                        }

                        return player ?? default!;
                    }
                }

            }
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

            if (inkMeLivePlayerDetailsRequest.DigitalSignature is not null)
            {
                var byteToImage = byteArrayToImage(inkMeLivePlayerDetailsRequest.DigitalSignature);

                inkMeLivePlayerDetailsRequest.DigitalSignature = imageToByteArray(byteToImage);
            }

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

    private byte[] imageToByteArray(Image img)
    {
        MemoryStream ms = new MemoryStream();
        img.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private Image byteArrayToImage(byte[] byteArrayIn)
    {
        string base64 = Encoding.UTF8.GetString(byteArrayIn);

        var base64Data = Regex.Match(base64, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
        var binData = Convert.FromBase64String(base64Data);


        MemoryStream ms = new MemoryStream(binData, 0, binData.Length);
        ms.Write(binData, 0, binData.Length);
        Image image = Image.FromStream(ms, true);
        return image;
    }
}