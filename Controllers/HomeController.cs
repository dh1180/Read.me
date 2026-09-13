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

    public async Task<IActionResult> Index()
    {
        var readingCount = await _context.UserBooks.CountAsync(ub => ub.Status == ReadingStatus.Reading);
        var completedCount = await _context.UserBooks.CountAsync(ub => ub.Status == ReadingStatus.Completed);
        var wishlistCount = await _context.UserBooks.CountAsync(ub => ub.Status == ReadingStatus.Wishlist);
        var totalNotesCount = await _context.ReadingNotes.CountAsync();

        var currentlyReading = await _context.UserBooks
            .Include(ub => ub.Book)
            .Where(ub => ub.Status == ReadingStatus.Reading)
            .OrderByDescending(ub => ub.UpdatedAt)
            .ToListAsync();

        var recentlyCompleted = await _context.UserBooks
            .Include(ub => ub.Book)
            .Where(ub => ub.Status == ReadingStatus.Completed)
            .OrderByDescending(ub => ub.CompletedDate)
            .Take(4)
            .ToListAsync();

        var recentNotes = await _context.ReadingNotes
            .Include(n => n.UserBook)
            .ThenInclude(ub => ub!.Book)
            .OrderByDescending(n => n.CreatedAt)
            .Take(4)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            ReadingCount = readingCount,
            CompletedCount = completedCount,
            WishlistCount = wishlistCount,
            TotalNotesCount = totalNotesCount,
            YearlyGoal = 20,
            CurrentlyReading = currentlyReading,
            RecentlyCompleted = recentlyCompleted,
            RecentNotes = recentNotes
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
