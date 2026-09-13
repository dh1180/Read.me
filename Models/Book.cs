using System.ComponentModel.DataAnnotations;

namespace ReadMeApp.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Isbn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Publisher { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CoverImageUrl { get; set; } = string.Empty;

    public int TotalPages { get; set; } = 300;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? PublishedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
}
