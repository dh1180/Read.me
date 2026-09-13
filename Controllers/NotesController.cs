using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Models;
using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Controllers;

public class NotesController : Controller
{
    private readonly ReadmeDbContext _context;

    public NotesController(ReadmeDbContext context)
    {
        _context = context;
    }

    // POST: /Notes/Create (Ajax)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Thought))
        {
            return Json(ApiResponse<object>.Fail("메모 또는 감상 내용을 입력해 주세요."));
        }

        var userBook = await _context.UserBooks.FindAsync(request.UserBookId);
        if (userBook == null)
        {
            return Json(ApiResponse<object>.Fail("도서를 찾을 수 없습니다."));
        }

        var note = new ReadingNote
        {
            UserBookId = request.UserBookId,
            PageNumber = request.PageNumber,
            Quote = request.Quote?.Trim() ?? string.Empty,
            Thought = request.Thought.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ReadingNotes.Add(note);
        await _context.SaveChangesAsync();

        return Json(ApiResponse<object>.Ok(new
        {
            note.Id,
            note.PageNumber,
            note.Quote,
            note.Thought,
            ThoughtHtml = note.ThoughtHtml,
            CreatedAt = note.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        }, "독서 메모가 저장되었습니다."));
    }

    // POST: /Notes/Delete/5 (Ajax)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _context.ReadingNotes.FindAsync(id);
        if (note == null)
        {
            return Json(ApiResponse<object>.Fail("메모를 찾을 수 없습니다."));
        }

        _context.ReadingNotes.Remove(note);
        await _context.SaveChangesAsync();

        return Json(ApiResponse<object>.Ok(new { id }, "메모가 삭제되었습니다."));
    }
}
