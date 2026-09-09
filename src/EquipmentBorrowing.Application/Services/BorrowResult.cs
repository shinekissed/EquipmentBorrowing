namespace EquipmentBorrowing.Application.Services;

public class BorrowResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public int? BorrowingId { get; }

    private BorrowResult(bool success, string? errorMessage, int? borrowingId)
    {
        Success = success;
        ErrorMessage = errorMessage;
        BorrowingId = borrowingId;
    }

    public static BorrowResult Ok(int borrowingId) => new(true, null, borrowingId);
    public static BorrowResult Fail(string errorMessage) => new(false, errorMessage, null);
}