namespace ReadMeApp.Models.ViewModels;

public class DashboardViewModel
{
    public int ReadingCount { get; set; }
    public int CompletedCount { get; set; }
    public int WishlistCount { get; set; }
    public int TotalNotesCount { get; set; }

    public int YearlyGoal { get; set; } = 20;

    public List<UserBook> CurrentlyReading { get; set; } = new();
    public List<UserBook> RecentlyCompleted { get; set; } = new();
    public List<ReadingNote> RecentNotes { get; set; } = new();

    public int GoalProgressPercentage => YearlyGoal > 0 
        ? Math.Min(100, (int)((double)CompletedCount / YearlyGoal * 100)) 
        : 0;
}
