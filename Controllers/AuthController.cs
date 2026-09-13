using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ReadMeApp.Models.ViewModels;
using ReadMeApp.Services;

namespace ReadMeApp.Controllers;

public class AuthController : Controller
{
    private readonly IKakaoAuthService _kakaoAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IKakaoAuthService kakaoAuthService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _kakaoAuthService = kakaoAuthService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        var redirectUri = GetKakaoRedirectUri();
        var authorizationUrl = _kakaoAuthService.GetAuthorizationUrl(redirectUri, returnUrl);

        return Redirect(authorizationUrl);
    }

    // GET: /auth/kakao/callback
    [HttpGet]
    [Route("auth/kakao/callback")]
    public async Task<IActionResult> KakaoCallback(
        [FromQuery] string? code, 
        [FromQuery] string? error, 
        [FromQuery(Name = "error_description")] string? errorDescription, 
        [FromQuery] string? state)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("카카오 로그인 실패 또는 사용자 취소: {Error} - {Description}", error, errorDescription);
            TempData["AuthError"] = "카카오 로그인이 취소되었거나 실패했습니다.";
            return RedirectToLocal(state);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["AuthError"] = "인가 코드가 전달되지 않았습니다.";
            return RedirectToLocal(state);
        }

        var redirectUri = GetKakaoRedirectUri();
        var token = await _kakaoAuthService.GetTokenAsync(code, redirectUri);
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            TempData["AuthError"] = "카카오 인증 토큰 발급에 실패했습니다.";
            return RedirectToLocal(state);
        }

        var profile = await _kakaoAuthService.GetUserProfileAsync(token.AccessToken);
        if (profile == null)
        {
            TempData["AuthError"] = "카카오 사용자 정보를 가져오지 못했습니다.";
            return RedirectToLocal(state);
        }

        // Build User Claims for Cookie Authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new Claim(ClaimTypes.Name, profile.Nickname),
            new Claim("Provider", "Kakao")
        };

        if (!string.IsNullOrWhiteSpace(profile.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, profile.Email));
        }

        if (!string.IsNullOrWhiteSpace(profile.ThumbnailImageUrl))
        {
            claims.Add(new Claim("ThumbnailImage", profile.ThumbnailImageUrl));
        }

        if (!string.IsNullOrWhiteSpace(profile.ProfileImageUrl))
        {
            claims.Add(new Claim("ProfileImage", profile.ProfileImageUrl));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        _logger.LogInformation("카카오 로그인 성공: {Nickname} (ID: {Id})", profile.Nickname, profile.Id);
        TempData["AuthSuccess"] = $"{profile.Nickname}님, 환영합니다!";

        return RedirectToLocal(state);
    }

    // GET or POST: /Auth/Logout
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["AuthSuccess"] = "성공적으로 로그아웃되었습니다.";
        return RedirectToLocal(returnUrl);
    }

    // GET: /Auth/Status (Ajax)
    [HttpGet]
    public IActionResult Status()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var name = User.Identity?.Name ?? string.Empty;
        var avatar = User.FindFirst("ThumbnailImage")?.Value ?? User.FindFirst("ProfileImage")?.Value ?? string.Empty;

        return Json(ApiResponse<object>.Ok(new
        {
            isAuthenticated,
            name,
            profileImage = avatar
        }));
    }

    private string GetKakaoRedirectUri()
    {
        var configuredUri = _configuration["Kakao:RedirectUri"]?.Trim();
        if (!string.IsNullOrWhiteSpace(configuredUri))
        {
            return configuredUri;
        }

        return $"{Request.Scheme}://{Request.Host}/auth/kakao/callback";
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
