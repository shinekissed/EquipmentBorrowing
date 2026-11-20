using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly Dictionary<int, Borrowing> _borrowings = new();
    private readonly IEnumerable<Student> _students;
    private readonly IEnumerable<Equipment> _equipment;

    public InMemoryBorrowingRepository(
        IEnumerable<Borrowing>? seed = null,
        IEnumerable<Student>? students = null,
        IEnumerable<Equipment>? equipment = null)
    {
        if (seed != null)
        {
            foreach (var b in seed)
            {
                _borrowings[b.Id] = b;
            }
        }
        _students = students ?? Enumerable.Empty<Student>();
        _equipment = equipment ?? Enumerable.Empty<Equipment>();
    }

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Values
            .Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.Values.FirstOrDefault(b =>
            b.StudentId == studentId &&
            b.EquipmentId == equipmentId &&
            b.Status == BorrowingStatus.Active);
        return Task.FromResult(borrowing);
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task CheckoutBatchAsync(
        IEnumerable<Borrowing> borrowings,
        IEnumerable<Equipment> updatedEquipment,
        bool simulateTransactionFailure = false,
        CancellationToken cancellationToken = default)
    {
        if (simulateTransactionFailure)
        {
            foreach (var eq in updatedEquipment)
            {
                eq.MarkAsReturned();
            }
            throw new InvalidOperationException("Simulated transaction failure after write for A06 demonstration.");
        }

        foreach (var b in borrowings)
        {
            _borrowings[b.Id] = b;
        }
        return Task.CompletedTask;
    }

    public Task<List<BorrowingRecordDto>> GetHistoryByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var dtos = _borrowings.Values
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenByDescending(b => b.Id)
            .Select(ToDto)
            .ToList();

        return Task.FromResult(dtos);
    }

    public Task<List<BorrowingRecordDto>> GetHistoryByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        var dtos = _borrowings.Values
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ThenByDescending(b => b.Id)
            .Select(ToDto)
            .ToList();

        return Task.FromResult(dtos);
    }

    public Task<List<BorrowingRecordDto>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        var dtos = _borrowings.Values
            .Where(b => b.Status == BorrowingStatus.Active)
            .OrderBy(b => b.ExpectedReturnDate)
            .ThenBy(b => b.Id)
            .Select(ToDto)
            .ToList();

        return Task.FromResult(dtos);
    }

    public Task<List<BorrowingRecordDto>> QueryBorrowingsAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortField,
        bool sortAscending,
        CancellationToken cancellationToken = default)
    {
        var query = _borrowings.Values.AsEnumerable();

        if (statusFilter == BorrowingStatusFilter.Active)
            query = query.Where(b => b.Status == BorrowingStatus.Active);
        else if (statusFilter == BorrowingStatusFilter.Returned)
            query = query.Where(b => b.Status == BorrowingStatus.Returned);

        var dtos = query.Select(ToDto);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var trimmed = searchText.Trim().ToLower();
            dtos = dtos.Where(d =>
                d.StudentName.ToLower().Contains(trimmed) ||
                d.StudentNumber.ToLower().Contains(trimmed) ||
                d.EquipmentName.ToLower().Contains(trimmed) ||
                d.EquipmentIdentifier.ToLower().Contains(trimmed));
        }

        dtos = (sortField, sortAscending) switch
        {
            (BorrowingSortField.CheckoutDate, true) => dtos.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.CheckoutDate, false) => dtos.OrderByDescending(b => b.DateBorrowed).ThenByDescending(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, true) => dtos.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, false) => dtos.OrderByDescending(b => b.ExpectedReturnDate).ThenByDescending(b => b.Id),
            _ => dtos.OrderByDescending(b => b.DateBorrowed).ThenByDescending(b => b.Id)
        };

        return Task.FromResult(dtos.ToList());
    }

    private BorrowingRecordDto ToDto(Borrowing b)
    {
        var student = _students.FirstOrDefault(s => s.Id == b.StudentId) ?? b.Student;
        var equipment = _equipment.FirstOrDefault(e => e.Id == b.EquipmentId) ?? b.Equipment;

        return new BorrowingRecordDto(
            b.Id,
            b.StudentId,
            student?.StudentNumber ?? $"S-{b.StudentId}",
            student?.Name ?? $"Student {b.StudentId}",
            b.EquipmentId,
            equipment?.Identifier ?? $"EQ-{b.EquipmentId}",
            equipment?.Name ?? $"Equipment {b.EquipmentId}",
            b.DateBorrowed,
            b.ExpectedReturnDate,
            b.ActualReturnDate,
            b.Status
        );
    }
}
