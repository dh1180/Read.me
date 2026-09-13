using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;
using ReadMeApp.Services;

namespace ReadMeApp.Controllers;

public class SearchController : Controller
{
    private readonly IBookSearchService _searchService;
    private readonly ReadmeDbContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IBookSearchService searchService, ReadmeDbContext context, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _context = context;
        _logger = logger;
    }

    // GET: /Search/Query?q=... (Ajax)
    [HttpGet]
    public async Task<IActionResult> Query(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(ApiResponse<List<BookSearchResultDto>>.Ok(new List<BookSearchResultDto>()));
        }

        try
        {
            var results = await _searchService.SearchBooksAsync(q);
            return Json(ApiResponse<List<BookSearchResultDto>>.Ok(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "도서 검색 중 오류 발생");
            return Json(ApiResponse<List<BookSearchResultDto>>.Fail("도서 검색 중 오류가 발생했습니다."));
        }
    }

    // POST: /Search/AddToLibrary (Ajax)
    [HttpPost]
    public async Task<IActionResult> AddToLibrary([FromBody] AddBookRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            return Json(ApiResponse<object>.Fail("책 정보가 올바르지 않습니다."));
        }

        try
        {
            // Truncate and sanitize inputs to prevent DB constraint errors
            var isbn = string.IsNullOrWhiteSpace(request.Isbn) 
                ? Guid.NewGuid().ToString("N")[..13] 
                : request.Isbn.Trim();
            if (isbn.Length > 50) isbn = isbn[..50];

            var title = request.Title.Trim();
            if (title.Length > 200) title = title[..200];

            var author = request.Author?.Trim() ?? "저자 미상";
            if (author.Length > 100) author = author[..100];

            var publisher = request.Publisher?.Trim() ?? "";
            if (publisher.Length > 100) publisher = publisher[..100];

            var cover = request.CoverImageUrl?.Trim() ?? "";
            if (cover.Length > 500) cover = cover[..500];

            var desc = request.Description?.Trim() ?? "";
            if (desc.Length > 2000) desc = desc[..2000];

            // Find or create Book entity
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
            if (book == null)
            {
                book = new Book
                {
                    Isbn = isbn,
                    Title = title,
                    Author = author,
                    Publisher = publisher,
                    CoverImageUrl = cover,
                    TotalPages = request.TotalPages > 0 ? request.TotalPages : 300,
                    Description = desc
                };
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }

            // Check if already in user's library
            var existingUserBook = await _context.UserBooks.FirstOrDefaultAsync(ub => ub.BookId == book.Id);
            if (existingUserBook != null)
            {
                return Json(ApiResponse<object>.Fail("이미 내 서재에 등록된 도서입니다."));
            }

            var userBook = new UserBook
            {
                BookId = book.Id,
                Status = request.Status == 0 ? ReadingStatus.Reading : request.Status,
                CurrentPage = 0,
                StartDate = DateTime.UtcNow
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            return Json(ApiResponse<object>.Ok(new { userBookId = userBook.Id }, $"'{book.Title}'이(가) 서재에 추가되었습니다."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "서재 등록 중 오류 발생");
            return Json(ApiResponse<object>.Fail($"서재 추가 중 오류가 발생했습니다: {ex.Message}"));
        }
    }
}
