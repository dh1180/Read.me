namespace ReadMeApp.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalReviewsCount { get; set; }
    public int TotalBooksCount { get; set; }
    public List<UserBook> Reviews { get; set; } = new();
    public List<Book> PopularBooks { get; set; } = new();
    public string CurrentSort { get; set; } = "latest";
    public string? SearchQuery { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages { get; set; } = 1;
    public int FilteredReviewsCount { get; set; }
}
