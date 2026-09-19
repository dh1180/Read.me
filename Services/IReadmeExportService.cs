namespace ReadMeApp.Services;

public interface IReadmeExportService
{
    Task<string> GenerateReadmeMarkdownAsync(string reviewerName);
}
