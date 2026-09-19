using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;
using ReadMeApp.Services;

namespace ReadMeApp.Controllers;

public class HomeController : Controller
{
    private const int PageSize = 12;
    private readonly ReadmeDbContext _context;
    private readonly IReadmeExportService _exportService;

    public HomeController(ReadmeDbContext context, IReadmeExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(string sort = "latest", string? query = null, int page = 1)
    {
        page = Math.Max(page, 1);

        var bookReviewsQuery = _context.UserBooks
            .Include(ub => ub.Book)
            .Where(ub => ub.Status == ReadingStatus.Completed && ub.Content != null && ub.Content != "")
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

        var filteredCount = await bookReviewsQuery.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(filteredCount / (double)PageSize));
        page = Math.Min(page, totalPages);

        bookReviewsQuery = sort switch
        {
            "popular" => bookReviewsQuery.OrderByDescending(ub => ub.LikesCount).ThenByDescending(ub => ub.CreatedAt),
            "rating" => bookReviewsQuery.OrderByDescending(ub => ub.Rating).ThenByDescending(ub => ub.CreatedAt),
            _ => bookReviewsQuery.OrderByDescending(ub => ub.CreatedAt)
        };

        var reviews = await bookReviewsQuery
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            Reviews = reviews,
            CurrentSort = sort,
            SearchQuery = query,
            CurrentPage = page,
            PageSize = PageSize,
            TotalPages = totalPages,
            FilteredReviewsCount = filteredCount
        };

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ExportReadme()
    {
        var reviewerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(reviewerName)) return Unauthorized();

        var markdown = await _exportService.GenerateReadmeMarkdownAsync(reviewerName);
        return Json(ApiResponse<string>.Ok(markdown));
    }

    [HttpGet]
    [Route("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reviews = await _context.UserBooks
            .Where(ub => ub.Status == ReadingStatus.Completed && ub.Content != null && ub.Content != "")
            .OrderByDescending(ub => ub.UpdatedAt)
            .Take(1000)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine($"  <url><loc>{baseUrl}/</loc><lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod><changefreq>daily</changefreq><priority>1.0</priority></url>");

        foreach (var review in reviews)
        {
            var lastMod = (review.UpdatedAt != default ? review.UpdatedAt : review.CreatedAt).ToString("yyyy-MM-dd");
            sb.AppendLine($"  <url><loc>{baseUrl}/Books/Details/{review.Id}</loc><lastmod>{lastMod}</lastmod><changefreq>weekly</changefreq><priority>0.8</priority></url>");
        }

        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpGet]
    [Route("robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /Auth/");
        sb.AppendLine("Disallow: /Search/AddToLibrary");
        sb.AppendLine("Disallow: /Books/Create");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
