using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public interface IKakaoAuthService
{
    string GetAuthorizationUrl(string redirectUri, string? state = null);
    Task<KakaoTokenResponse?> GetTokenAsync(string code, string redirectUri);
    Task<KakaoUserProfile?> GetUserProfileAsync(string accessToken);
}
