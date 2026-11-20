using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.DTOs;

public record BorrowingRecordDto(
    int Id,
    int StudentId,
    string StudentNumber,
    string StudentName,
    int EquipmentId,
    string EquipmentIdentifier,
    string EquipmentName,
    DateTime DateBorrowed,
    DateTime ExpectedReturnDate,
    DateTime? ActualReturnDate,
    BorrowingStatus Status
);

public enum BorrowingStatusFilter
{
    All,
    Active,
    Returned
}

public enum BorrowingSortField
{
    CheckoutDate,
    ExpectedReturnDate
}
