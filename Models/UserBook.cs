using System.ComponentModel.DataAnnotations;

namespace ReadMeApp.Models;

public class UserBook
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }

    public ReadingStatus Status { get; set; } = ReadingStatus.Wishlist;

    public int CurrentPage { get; set; } = 0;

    public DateTime? StartDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    [MaxLength(1000)]
    public string? Summary { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReadingNote> Notes { get; set; } = new List<ReadingNote>();

    public int ProgressPercentage
    {
        get
        {
            if (Book == null || Book.TotalPages <= 0) return 0;
            if (Status == ReadingStatus.Completed) return 100;
            var pct = (int)((double)CurrentPage / Book.TotalPages * 100);
            return Math.Clamp(pct, 0, 100);
        }
    }
}
