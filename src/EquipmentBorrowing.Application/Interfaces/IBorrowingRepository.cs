using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task CheckoutBatchAsync(
        IEnumerable<Borrowing> borrowings,
        IEnumerable<Equipment> updatedEquipment,
        bool simulateTransactionFailure = false,
        CancellationToken cancellationToken = default);

    Task<List<BorrowingRecordDto>> GetHistoryByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<List<BorrowingRecordDto>> GetHistoryByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task<List<BorrowingRecordDto>> QueryBorrowingsAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortField,
        bool sortAscending,
        CancellationToken cancellationToken = default);
    Task<List<BorrowingRecordDto>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default);
}