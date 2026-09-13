namespace ReadMeApp.Models.ViewModels;

public class UpdateProgressRequest
{
    public int UserBookId { get; set; }
    public int CurrentPage { get; set; }
    public ReadingStatus? Status { get; set; }
}

public class CreateNoteRequest
{
    public int UserBookId { get; set; }
    public int PageNumber { get; set; }
    public string Quote { get; set; } = string.Empty;
    public string Thought { get; set; } = string.Empty;
}

public class AddBookRequest
{
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalPages { get; set; } = 300;
    public string Description { get; set; } = string.Empty;
    public ReadingStatus Status { get; set; } = ReadingStatus.Wishlist;
}

public class BookSearchResultDto
{
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalPages { get; set; } = 300;
    public string Description { get; set; } = string.Empty;
    public string PublishedDate { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "성공적으로 처리되었습니다.")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }
}
