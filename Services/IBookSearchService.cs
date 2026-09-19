using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public interface IBookSearchService
{
    Task<BookSearchPageDto> SearchBooksAsync(string query, int page = 1, int size = 20);
}
