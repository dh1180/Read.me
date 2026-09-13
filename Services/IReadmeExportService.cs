using ReadMeApp.Models.ViewModels;

namespace ReadMeApp.Services;

public interface IReadmeExportService
{
    Task<string> GenerateReadmeMarkdownAsync();
}
