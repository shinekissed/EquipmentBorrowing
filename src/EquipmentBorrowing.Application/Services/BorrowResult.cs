namespace EquipmentBorrowing.Application.Services;

public class BorrowResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public int? BorrowingId { get; }
    public IReadOnlyList<int> BorrowingIds { get; }

    private BorrowResult(bool success, string? errorMessage, int? borrowingId, IReadOnlyList<int>? borrowingIds = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        BorrowingId = borrowingId;
        BorrowingIds = borrowingIds ?? (borrowingId.HasValue ? new[] { borrowingId.Value } : Array.Empty<int>());
    }

    public static BorrowResult Ok(int borrowingId) => new(true, null, borrowingId);
    public static BorrowResult OkMulti(IReadOnlyList<int> borrowingIds) =>
        new(true, null, borrowingIds.FirstOrDefault(), borrowingIds);
    public static BorrowResult Fail(string errorMessage) => new(false, errorMessage, null);
}
