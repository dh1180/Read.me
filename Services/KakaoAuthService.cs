using System.Net.Http.Headers;
using System.Text.Json;
using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public class KakaoAuthService : IKakaoAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KakaoAuthService> _logger;

    public KakaoAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<KakaoAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string redirectUri, string? state = null)
    {
        var clientId = _configuration["Kakao:RestApiKey"]?.Trim() ?? string.Empty;
        var url = $"https://kauth.kakao.com/oauth/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code";
        if (!string.IsNullOrWhiteSpace(state))
        {
            url += $"&state={Uri.EscapeDataString(state)}";
        }
        return url;
    }

    public async Task<KakaoTokenResponse?> GetTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["Kakao:RestApiKey"]?.Trim() ?? string.Empty;
        var clientSecret = _configuration["Kakao:ClientSecret"]?.Trim();

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientId },
            { "redirect_uri", redirectUri },
            { "code", code }
        };

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            parameters.Add("client_secret", clientSecret);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://kauth.kakao.com/oauth/token")
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("카카오 토큰 발급 실패: Status={StatusCode}, Body={Body}", response.StatusCode, content);
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<KakaoTokenResponse>(content);
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카카오 토큰 요청 중 예외 발생");
            return null;
        }
    }

    public async Task<KakaoUserProfile?> GetUserProfileAsync(string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://kapi.kakao.com/v2/user/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("카카오 사용자 정보 조회 실패: Status={StatusCode}, Body={Body}", response.StatusCode, content);
                return null;
            }

            var profile = JsonSerializer.Deserialize<KakaoUserProfile>(content);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카카오 사용자 정보 요청 중 예외 발생");
            return null;
        }
    }
}
