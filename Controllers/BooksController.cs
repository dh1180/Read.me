using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Controllers;

public class BooksController : Controller
{
    private readonly ReadmeDbContext _context;

    public BooksController(ReadmeDbContext context)
    {
        _context = context;
    }

    // GET: /Books?status=Reading
    public async Task<IActionResult> Index(ReadingStatus? status)
    {
        var query = _context.UserBooks
            .Include(ub => ub.Book)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(ub => ub.Status == status.Value);
        }

        var list = await query.OrderByDescending(ub => ub.UpdatedAt).ToListAsync();
        ViewBag.CurrentStatus = status;
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

    // POST: /Books/UpdateProgress (Ajax)
    [HttpPost]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        var userBook = await _context.UserBooks
            .Include(ub => ub.Book)
            .FirstOrDefaultAsync(ub => ub.Id == request.UserBookId);

        if (userBook == null)
        {
            return Json(ApiResponse<object>.Fail("도서를 찾을 수 없습니다."));
        }

        var totalPages = userBook.Book?.TotalPages ?? 300;
        userBook.CurrentPage = Math.Clamp(request.CurrentPage, 0, totalPages);
        userBook.UpdatedAt = DateTime.UtcNow;

        if (request.Status.HasValue)
        {
            userBook.Status = request.Status.Value;
        }
        else if (userBook.CurrentPage >= totalPages)
        {
            userBook.Status = ReadingStatus.Completed;
            userBook.CompletedDate ??= DateTime.UtcNow;
        }
        else if (userBook.CurrentPage > 0 && userBook.Status == ReadingStatus.Wishlist)
        {
            userBook.Status = ReadingStatus.Reading;
            userBook.StartDate ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Json(ApiResponse<object>.Ok(new
        {
            userBook.Id,
            userBook.CurrentPage,
            userBook.ProgressPercentage,
            Status = userBook.Status.ToString()
        }, "독서 진행률이 성공적으로 업데이트되었습니다."));
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
