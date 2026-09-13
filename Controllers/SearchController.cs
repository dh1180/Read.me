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

    public SearchController(IBookSearchService searchService, ReadmeDbContext context)
    {
        _searchService = searchService;
        _context = context;
    }

    // GET: /Search/Query?q=... (Ajax)
    [HttpGet]
    public async Task<IActionResult> Query(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(ApiResponse<List<BookSearchResultDto>>.Ok(new List<BookSearchResultDto>()));
        }

        var results = await _searchService.SearchBooksAsync(q);
        return Json(ApiResponse<List<BookSearchResultDto>>.Ok(results));
    }

    // POST: /Search/AddToLibrary (Ajax)
    [HttpPost]
    public async Task<IActionResult> AddToLibrary([FromBody] AddBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Json(ApiResponse<object>.Fail("책 제목은 필수입니다."));
        }

        // Find or create Book entity
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Isbn == request.Isbn);
        if (book == null)
        {
            book = new Book
            {
                Isbn = string.IsNullOrWhiteSpace(request.Isbn) ? Guid.NewGuid().ToString() : request.Isbn,
                Title = request.Title,
                Author = request.Author,
                Publisher = request.Publisher,
                CoverImageUrl = request.CoverImageUrl,
                TotalPages = request.TotalPages > 0 ? request.TotalPages : 300,
                Description = request.Description
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
            Status = request.Status,
            CurrentPage = 0,
            StartDate = request.Status == ReadingStatus.Reading ? DateTime.UtcNow : null
        };

        _context.UserBooks.Add(userBook);
        await _context.SaveChangesAsync();

        return Json(ApiResponse<object>.Ok(new { userBookId = userBook.Id }, $"'{book.Title}'이(가) 서재에 추가되었습니다."));
    }
}
