using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public class BorrowingHistoryService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public BorrowingHistoryService(
        IBorrowingRepository borrowingRepository,
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<List<BorrowingRecordDto>> GetStudentBorrowingHistoryAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            throw new InvalidOperationException($"Student with ID {studentId} was not found (BR-18).");

        return await _borrowingRepository.GetHistoryByStudentIdAsync(studentId, cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> GetEquipmentBorrowingHistoryAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null)
            throw new InvalidOperationException($"Equipment with ID {equipmentId} was not found (BR-18).");

        return await _borrowingRepository.GetHistoryByEquipmentIdAsync(equipmentId, cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> GetActiveBorrowingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _borrowingRepository.GetActiveBorrowingsAsync(cancellationToken);
    }

    public async Task<List<BorrowingRecordDto>> SearchBorrowingsAsync(
        string? searchText,
        BorrowingStatusFilter statusFilter,
        BorrowingSortField sortField,
        bool sortAscending,
        CancellationToken cancellationToken = default)
    {
        return await _borrowingRepository.QueryBorrowingsAsync(
            searchText,
            statusFilter,
            sortField,
            sortAscending,
            cancellationToken);
    }
}
