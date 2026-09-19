using System.ComponentModel.DataAnnotations;
using Markdig;

namespace ReadMeApp.Models;

public class ReadingNote
{
    public int Id { get; set; }
    public int UserBookId { get; set; }
    public UserBook? UserBook { get; set; }
    public int PageNumber { get; set; }

    [MaxLength(2000)]
    public string Quote { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Thought { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string ThoughtHtml
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Thought)) return string.Empty;
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .DisableHtml()
                .Build();
            return Markdown.ToHtml(Thought, pipeline);
        }
    }
}
