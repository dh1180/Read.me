using System.ComponentModel.DataAnnotations;

namespace ReadMeApp.Models;

public class UserBook
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }

    [MaxLength(50)]
    public string ReviewerName { get; set; } = "익명의 독서가";

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [MaxLength(200)]
    public string? Summary { get; set; } // 한 줄 요약 / 제목

    [MaxLength(500)]
    public string? Quote { get; set; } // 인상 깊은 한 문장

    [MaxLength(4000)]
    public string? Content { get; set; } // 독서록 본문

    public DateTime ReadDate { get; set; } = DateTime.UtcNow;

    public int LikesCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Optional status for bookshelf categorization
    public ReadingStatus Status { get; set; } = ReadingStatus.Completed;

    public ICollection<ReadingNote> Notes { get; set; } = new List<ReadingNote>();
}
