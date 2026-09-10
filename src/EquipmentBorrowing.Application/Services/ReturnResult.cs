namespace EquipmentBorrowing.Application.Services;

public class ReturnResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private ReturnResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static ReturnResult Ok() => new(true, null);
    public static ReturnResult Fail(string errorMessage) => new(false, errorMessage);
}