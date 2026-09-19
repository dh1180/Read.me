using Microsoft.AspNetCore.Authorization;
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

    [HttpGet]
    public async Task<IActionResult> Query(string? q, string? query, int page = 1, int size = 20)
    {
        var searchTerm = !string.IsNullOrWhiteSpace(q) ? q : query;
        page = Math.Clamp(page, 1, 50);
        size = Math.Clamp(size, 1, 50);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Json(ApiResponse<BookSearchPageDto>.Ok(new BookSearchPageDto
            {
                Page = page,
                PageSize = size,
                IsEnd = true
            }));
        }

        try
        {
            var results = await _searchService.SearchBooksAsync(searchTerm, page, size);
            return Json(ApiResponse<BookSearchPageDto>.Ok(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "도서 검색 중 오류 발생");
            return Json(ApiResponse<BookSearchPageDto>.Fail("도서 검색 중 오류가 발생했습니다."));
        }
    }

    [HttpGet]
    public Task<IActionResult> SearchBooks(string? query, string? q, int page = 1, int size = 20)
        => Query(q, query, page, size);

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToLibrary([FromBody] AddBookRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            return Json(ApiResponse<object>.Fail("책 정보가 올바르지 않습니다."));
        }

        var reviewerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(reviewerName)) return Unauthorized();

        try
        {
            var isbn = string.IsNullOrWhiteSpace(request.Isbn) ? Guid.NewGuid().ToString("N")[..13] : request.Isbn.Trim();
            isbn = isbn.Length > 50 ? isbn[..50] : isbn;

            var title = request.Title.Trim();
            title = title.Length > 200 ? title[..200] : title;

            var author = string.IsNullOrWhiteSpace(request.Author) ? "저자 미상" : request.Author.Trim();
            author = author.Length > 100 ? author[..100] : author;

            var publisher = request.Publisher?.Trim() ?? string.Empty;
            publisher = publisher.Length > 100 ? publisher[..100] : publisher;

            var cover = request.CoverImageUrl?.Trim() ?? string.Empty;
            cover = cover.Length > 500 ? cover[..500] : cover;

            var description = request.Description?.Trim() ?? string.Empty;
            description = description.Length > 2000 ? description[..2000] : description;

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
                    Description = description
                };
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }

            var existingUserBook = await _context.UserBooks.FirstOrDefaultAsync(
                ub => ub.BookId == book.Id && ub.ReviewerName == reviewerName);

            if (existingUserBook != null)
            {
                return Json(ApiResponse<object>.Fail("이미 내 기록에 등록된 책입니다."));
            }

            var userBook = new UserBook
            {
                BookId = book.Id,
                ReviewerName = reviewerName.Length > 50 ? reviewerName[..50] : reviewerName,
                Rating = 5,
                Status = ReadingStatus.Wishlist,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            return Json(ApiResponse<object>.Ok(
                new { userBookId = userBook.Id },
                $"'{book.Title}'을(를) 읽고 싶은 책에 담았습니다."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "서재 등록 중 오류 발생");
            return Json(ApiResponse<object>.Fail("책을 내 기록에 추가하는 중 오류가 발생했습니다."));
        }
    }
}
