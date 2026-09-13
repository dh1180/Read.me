using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Controllers;

public class BooksController : Controller
{
    private readonly ReadmeDbContext _context;
    private readonly ILogger<BooksController> _logger;

    public BooksController(ReadmeDbContext context, ILogger<BooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Books
    public async Task<IActionResult> Index(string? sort, string? query)
    {
        var bookReviewsQuery = _context.UserBooks
            .Include(ub => ub.Book)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            bookReviewsQuery = bookReviewsQuery.Where(ub =>
                (ub.Book != null && (ub.Book.Title.ToLower().Contains(q) || ub.Book.Author.ToLower().Contains(q))) ||
                (ub.Summary != null && ub.Summary.ToLower().Contains(q)) ||
                (ub.Quote != null && ub.Quote.ToLower().Contains(q)) ||
                (ub.Content != null && ub.Content.ToLower().Contains(q)) ||
                ub.ReviewerName.ToLower().Contains(q));
        }

        bookReviewsQuery = sort switch
        {
            "popular" => bookReviewsQuery.OrderByDescending(ub => ub.LikesCount).ThenByDescending(ub => ub.CreatedAt),
            "rating" => bookReviewsQuery.OrderByDescending(ub => ub.Rating).ThenByDescending(ub => ub.CreatedAt),
            _ => bookReviewsQuery.OrderByDescending(ub => ub.CreatedAt)
        };

        var list = await bookReviewsQuery.ToListAsync();
        ViewBag.CurrentSort = sort ?? "latest";
        ViewBag.SearchQuery = query;
        return View(list);
    }

    // GET: /Books/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userBook = await _context.UserBooks
            .Include(ub => ub.Book)
            .Include(ub => ub.Notes.OrderByDescending(n => n.CreatedAt))
            .FirstOrDefaultAsync(ub => ub.Id == id);

        if (userBook == null)
        {
            return NotFound();
        }

        return View(userBook);
    }

    // POST: /Books/CreateReview (Ajax)
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            return Json(ApiResponse<object>.Fail("도서 정보가 올바르지 않습니다."));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return Json(ApiResponse<object>.Fail("독서록 본문(감상평)을 작성해 주세요."));
        }

        try
        {
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

            // 1. Find or create Book entity
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
                    TotalPages = 300,
                    Description = desc
                };
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }

            // 2. Create UserBook (Review)
            var reviewer = string.IsNullOrWhiteSpace(request.ReviewerName)
                ? "익명의 독서가"
                : request.ReviewerName.Trim();
            if (reviewer.Length > 50) reviewer = reviewer[..50];

            var summary = request.Summary?.Trim();
            if (summary != null && summary.Length > 200) summary = summary[..200];

            var quote = request.Quote?.Trim();
            if (quote != null && quote.Length > 500) quote = quote[..500];

            var content = request.Content.Trim();
            if (content.Length > 4000) content = content[..4000];

            var rating = Math.Clamp(request.Rating, 1, 5);

            var userBook = new UserBook
            {
                BookId = book.Id,
                ReviewerName = reviewer,
                Rating = rating,
                Summary = summary,
                Quote = quote,
                Content = content,
                ReadDate = request.ReadDate ?? DateTime.UtcNow,
                LikesCount = 0,
                Status = ReadingStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            return Json(ApiResponse<object>.Ok(new { id = userBook.Id }, $"'{book.Title}' 독서록이 등록되었습니다!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "독서록 등록 중 오류 발생");
            return Json(ApiResponse<object>.Fail($"독서록 등록 중 오류가 발생했습니다: {ex.Message}"));
        }
    }

    // POST: /Books/Like/5 (Ajax)
    [HttpPost]
    public async Task<IActionResult> Like(int id)
    {
        var userBook = await _context.UserBooks.FindAsync(id);
        if (userBook == null)
        {
            return Json(ApiResponse<object>.Fail("독서록을 찾을 수 없습니다."));
        }

        userBook.LikesCount++;
        await _context.SaveChangesAsync();

        return Json(ApiResponse<object>.Ok(new { likes = userBook.LikesCount }, "좋아요를 남겼습니다!"));
    }

    // POST: /Books/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userBook = await _context.UserBooks.FindAsync(id);
        if (userBook != null)
        {
            _context.UserBooks.Remove(userBook);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
