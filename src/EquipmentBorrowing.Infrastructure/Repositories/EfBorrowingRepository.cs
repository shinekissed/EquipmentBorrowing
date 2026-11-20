using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.Borrowings.AddAsync(borrowing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // LINQ Query: Count active borrowings for a specific student (No-tracking for read-only speed)
    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    // LINQ Query: Find active borrowing by student and equipment (Tracking required because it will be returned/modified)
    public async Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .FirstOrDefaultAsync(b => b.StudentId == studentId && b.EquipmentId == equipmentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CheckoutBatchAsync(
        IEnumerable<Borrowing> borrowings,
        IEnumerable<Equipment> updatedEquipment,
        bool simulateTransactionFailure = false,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var equipment in updatedEquipment)
                {
                    _context.Equipment.Update(equipment);
                }

                await _context.SaveChangesAsync(cancellationToken);

                if (simulateTransactionFailure)
                {
                    throw new InvalidOperationException("Simulated transaction failure after write for A06 demonstration.");
                }

                await _context.Borrowings.AddRangeAsync(borrowings, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<List<BorrowingRecordDto>> GetHistoryByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await ProjectToDto(_context.Borrowings.AsNoTracking().Where(b => b.StudentId == studentId))
            .OrderByDescending(b => b.DateBorrowed)
            .ThenByDescending(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> GetHistoryByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        return await ProjectToDto(_context.Borrowings.AsNoTracking().Where(b => b.EquipmentId == equipmentId))
            .OrderByDescending(b => b.DateBorrowed)
            .ThenByDescending(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        return await ProjectToDto(_context.Borrowings.AsNoTracking().Where(b => b.Status == BorrowingStatus.Active))
            .OrderBy(b => b.ExpectedReturnDate)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> QueryBorrowingsAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortField,
        bool sortAscending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Borrowings.AsNoTracking();

        // 1. Status Filter
        if (statusFilter == BorrowingStatusFilter.Active)
            query = query.Where(b => b.Status == BorrowingStatus.Active);
        else if (statusFilter == BorrowingStatusFilter.Returned)
            query = query.Where(b => b.Status == BorrowingStatus.Returned);

        // 2. Search text (BR-17: case-insensitive partial match on name, student number, equipment name/id)
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var trimmed = searchText.Trim().ToLower();
            query = query.Where(b =>
                b.Student!.Name.ToLower().Contains(trimmed) ||
                b.Student!.StudentNumber.ToLower().Contains(trimmed) ||
                b.Equipment!.Name.ToLower().Contains(trimmed) ||
                b.Equipment!.Identifier.ToLower().Contains(trimmed));
        }

        var dtoQuery = ProjectToDto(query);

        // 3. Sorting with stable tie-breaker (BR-17)
        dtoQuery = (sortField, sortAscending) switch
        {
            (BorrowingSortField.CheckoutDate, true) => dtoQuery.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id),
            (BorrowingSortField.CheckoutDate, false) => dtoQuery.OrderByDescending(b => b.DateBorrowed).ThenByDescending(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, true) => dtoQuery.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            (BorrowingSortField.ExpectedReturnDate, false) => dtoQuery.OrderByDescending(b => b.ExpectedReturnDate).ThenByDescending(b => b.Id),
            _ => dtoQuery.OrderByDescending(b => b.DateBorrowed).ThenByDescending(b => b.Id)
        };

        return await dtoQuery.ToListAsync(cancellationToken);
    }

    private static IQueryable<BorrowingRecordDto> ProjectToDto(IQueryable<Borrowing> query)
    {
        return query.Select(b => new BorrowingRecordDto(
            b.Id,
            b.StudentId,
            b.Student != null ? b.Student.StudentNumber : string.Empty,
            b.Student != null ? b.Student.Name : string.Empty,
            b.EquipmentId,
            b.Equipment != null ? b.Equipment.Identifier : string.Empty,
            b.Equipment != null ? b.Equipment.Name : string.Empty,
            b.DateBorrowed,
            b.ExpectedReturnDate,
            b.ActualReturnDate,
            b.Status
        ));
    }
}

