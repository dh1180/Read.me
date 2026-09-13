using System.Text.Json;
using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public class KakaoBookSearchService : IBookSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KakaoBookSearchService> _logger;

    public KakaoBookSearchService(HttpClient httpClient, IConfiguration configuration, ILogger<KakaoBookSearchService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<BookSearchResultDto>> SearchBooksAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<BookSearchResultDto>();
        }

        var apiKey = _configuration["Kakao:RestApiKey"];
        
        // If API key is configured, call Kakao Book Search Open API
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://dapi.kakao.com/v3/search/book?query={Uri.EscapeDataString(query)}&size=15");
                request.Headers.Add("Authorization", $"KakaoAK {apiKey}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var documents = doc.RootElement.GetProperty("documents");

                    var list = new List<BookSearchResultDto>();
                    foreach (var item in documents.EnumerateArray())
                    {
                        var isbnFull = item.GetProperty("isbn").GetString() ?? "";
                        var isbn = isbnFull.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? Guid.NewGuid().ToString();
                        var title = item.GetProperty("title").GetString() ?? "제목 없음";
                        var publisher = item.GetProperty("publisher").GetString() ?? "";
                        var thumbnail = item.GetProperty("thumbnail").GetString() ?? "";
                        var contents = item.GetProperty("contents").GetString() ?? "";
                        var datetime = item.GetProperty("datetime").GetString() ?? "";

                        var authorsList = new List<string>();
                        if (item.TryGetProperty("authors", out var authorsElem))
                        {
                            foreach (var a in authorsElem.EnumerateArray())
                            {
                                authorsList.Add(a.GetString() ?? "");
                            }
                        }

                        list.Add(new BookSearchResultDto
                        {
                            Isbn = isbn,
                            Title = title,
                            Author = string.Join(", ", authorsList),
                            Publisher = publisher,
                            CoverImageUrl = thumbnail,
                            TotalPages = 320, // 카카오 API는 기본 페이지 수를 제공하지 않아 기본값 지정
                            Description = contents,
                            PublishedDate = datetime.Length >= 10 ? datetime[..10] : datetime
                        });
                    }

                    if (list.Count > 0) return list;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kakao Book Search API 호출 실패. Mock 데이터로 폴백합니다.");
            }
        }

        // Fallback / Sample Mock search for instant out-of-the-box demonstration
        return GetMockSearchResults(query);
    }

    private List<BookSearchResultDto> GetMockSearchResults(string query)
    {
        var sampleLibrary = new List<BookSearchResultDto>
        {
            new() {
                Isbn = "9788966263301",
                Title = "클린 코드 (Clean Code)",
                Author = "로버트 C. 마틴",
                Publisher = "인사이트",
                CoverImageUrl = "https://image.aladin.co.kr/product/3084/64/cover500/8966260956_1.jpg",
                TotalPages = 584,
                Description = "애자일 소프트웨어 장인 정신과 읽기 쉬운 깨끗한 코드 작성법을 다룬 바이블.",
                PublishedDate = "2013-12-24"
            },
            new() {
                Isbn = "9788966262472",
                Title = "프로그래머의 길, 멘토에게 묻다",
                Author = "데이브 후버, 아디트야 바르가바",
                Publisher = "인사이트",
                CoverImageUrl = "https://image.aladin.co.kr/product/642/41/cover500/8966260271_1.jpg",
                TotalPages = 340,
                Description = "소프트웨어 장인이 되기 위해 스스로를 훈련하고 성장하는 구체적 실천법.",
                PublishedDate = "2011-04-18"
            },
            new() {
                Isbn = "9788968482954",
                Title = "도메인 주도 설계 핵심",
                Author = "반 버논",
                Publisher = "에이콘출판",
                CoverImageUrl = "https://image.aladin.co.kr/product/10452/78/cover500/8968482950_1.jpg",
                TotalPages = 288,
                Description = "복잡한 비즈니스 로직을 소프트웨어 모델로 풀어내는 DDD의 정수.",
                PublishedDate = "2017-03-20"
            },
            new() {
                Isbn = "9788966261024",
                Title = "리팩터링 2판",
                Author = "마틴 파울러",
                Publisher = "한빛미디어",
                CoverImageUrl = "https://image.aladin.co.kr/product/23617/38/cover500/k282638848_1.jpg",
                TotalPages = 552,
                Description = "코드 구조를 체계적으로 개선하여 소프트웨어의 가치를 극대화하는 리팩터링 기법.",
                PublishedDate = "2020-04-01"
            },
            new() {
                Isbn = "9788960773417",
                Title = "객체지향의 사실과 오해",
                Author = "조영호",
                Publisher = "위키북스",
                CoverImageUrl = "https://image.aladin.co.kr/product/6108/42/cover500/8998139768_1.jpg",
                TotalPages = 264,
                Description = "역할, 책임, 협력의 관점에서 객체지향의 본질을 명쾌하게 파헤친 입문서.",
                PublishedDate = "2015-06-17"
            },
            new() {
                Isbn = "9788966260959",
                Title = "실용주의 프로그래머 20주년 기념판",
                Author = "데이비드 토머스, 앤드루 헌트",
                Publisher = "인사이트",
                CoverImageUrl = "https://image.aladin.co.kr/product/28801/95/cover500/8966263548_1.jpg",
                TotalPages = 496,
                Description = "소프트웨어 개발자의 커리어와 실무 접근 방식을 혁신하는 고전 중의 고전.",
                PublishedDate = "2022-03-25"
            },
            new() {
                Isbn = "9788968484828",
                Title = "C# 12와 .NET 8 교과서",
                Author = "박용준",
                Publisher = "길벗",
                CoverImageUrl = "https://image.aladin.co.kr/product/33420/22/cover500/k782938834_1.jpg",
                TotalPages = 860,
                Description = "C#의 기초 문법부터 최신 ASP.NET Core 웹 개발까지 단계별로 마스터하는 가이드.",
                PublishedDate = "2024-02-28"
            }
        };

        return sampleLibrary
            .Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        b.Publisher.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
