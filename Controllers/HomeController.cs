using System.Diagnostics;
using System.Text;
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

    // GET: /sitemap.xml (SEO Dynamic Sitemap for Google/Naver)
    [HttpGet]
    [Route("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reviews = await _context.UserBooks
            .OrderByDescending(ub => ub.UpdatedAt)
            .Take(1000)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Main Home Page
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        // Books Index Page
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/Books</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>0.9</priority>");
        sb.AppendLine("  </url>");

        // Review Create Page
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/Books/Create</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>weekly</changefreq>");
        sb.AppendLine("    <priority>0.7</priority>");
        sb.AppendLine("  </url>");

        // Dynamic Review Detail Pages
        foreach (var review in reviews)
        {
            var lastMod = (review.UpdatedAt != default ? review.UpdatedAt : review.CreatedAt).ToString("yyyy-MM-dd");
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/Books/Details/{review.Id}</loc>");
            sb.AppendLine($"    <lastmod>{lastMod}</lastmod>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    // GET: /robots.txt (SEO Crawler Directives)
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
