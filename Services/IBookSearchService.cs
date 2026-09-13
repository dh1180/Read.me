using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public interface IBookSearchService
{
    Task<List<BookSearchResultDto>> SearchBooksAsync(string query);
}
