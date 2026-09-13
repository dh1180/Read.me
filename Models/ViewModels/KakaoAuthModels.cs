using System.Text.Json.Serialization;

namespace ReadMeApp.Models.ViewModels;

public class KakaoTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class KakaoUserProfile
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("connected_at")]
    public string? ConnectedAt { get; set; }

    [JsonPropertyName("properties")]
    public KakaoProperties? Properties { get; set; }

    [JsonPropertyName("kakao_account")]
    public KakaoAccount? KakaoAccount { get; set; }

    public string Nickname => KakaoAccount?.Profile?.Nickname 
        ?? Properties?.Nickname 
        ?? "카카오 사용자";

    public string? ProfileImageUrl => KakaoAccount?.Profile?.ProfileImageUrl 
        ?? Properties?.ProfileImage;

    public string? ThumbnailImageUrl => KakaoAccount?.Profile?.ThumbnailImageUrl 
        ?? Properties?.ThumbnailImage;

    public string? Email => KakaoAccount?.Email;
}

public class KakaoProperties
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("profile_image")]
    public string? ProfileImage { get; set; }

    [JsonPropertyName("thumbnail_image")]
    public string? ThumbnailImage { get; set; }
}

public class KakaoAccount
{
    [JsonPropertyName("profile")]
    public KakaoProfile? Profile { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("is_email_verified")]
    public bool? IsEmailVerified { get; set; }

    [JsonPropertyName("is_email_valid")]
    public bool? IsEmailValid { get; set; }
}

public class KakaoProfile
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("thumbnail_image_url")]
    public string? ThumbnailImageUrl { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }
}
