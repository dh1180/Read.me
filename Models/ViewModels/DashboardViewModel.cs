namespace ReadMeApp.Models.ViewModels;

public class DashboardViewModel
{
    public List<UserBook> Reviews { get; set; } = new();
    public string CurrentSort { get; set; } = "latest";
    public string? SearchQuery { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages { get; set; } = 1;
    public int FilteredReviewsCount { get; set; }
}
