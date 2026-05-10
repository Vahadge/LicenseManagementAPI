namespace LicenseManagementAPI.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> Success(T? data, string message = "Operation completed successfully.")
        => new() { IsSuccess = true, Message = message, Data = data };

    public static ApiResponse<T> Failure(string message, IEnumerable<string>? errors = null)
        => new() { IsSuccess = false, Message = message, Errors = errors };
}
