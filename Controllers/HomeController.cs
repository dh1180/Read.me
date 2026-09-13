using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;
using ReadMeApp.Services;

namespace ReadMeApp.Controllers;

public class HomeController : Controller
{
    private readonly ReadmeDbContext _context;
    private readonly IReadmeExportService _exportService;

    public HomeController(ReadmeDbContext context, IReadmeExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string sort = "latest", string? query = null)
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

        var reviews = await bookReviewsQuery.ToListAsync();
        var totalReviewsCount = await _context.UserBooks.CountAsync();
        var totalBooksCount = await _context.Books.CountAsync();
        var popularBooks = await _context.Books.OrderByDescending(b => b.UserBooks.Count).Take(6).ToListAsync();

        var model = new DashboardViewModel
        {
            TotalReviewsCount = totalReviewsCount,
            TotalBooksCount = totalBooksCount,
            Reviews = reviews,
            PopularBooks = popularBooks,
            CurrentSort = sort,
            SearchQuery = query
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportReadme()
    {
        var markdown = await _exportService.GenerateReadmeMarkdownAsync();
        return Json(ApiResponse<string>.Ok(markdown));
    }
}
