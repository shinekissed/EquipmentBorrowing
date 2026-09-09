using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        DateTime expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return BorrowResult.Fail($"Student {studentId} does not exist.");

        if (!student.IsAllowedToBorrow)
            return BorrowResult.Fail($"Student {studentId} is not currently allowed to borrow equipment.");

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null)
            return BorrowResult.Fail($"Equipment {equipmentId} does not exist.");

        if (!equipment.IsAvailable)
            return BorrowResult.Fail($"Equipment {equipmentId} is not currently available.");

        var activeCount = await _borrowingRepository.CountActiveByStudentIdAsync(studentId, cancellationToken);
        if (activeCount >= student.MaxActiveBorrowings)
            return BorrowResult.Fail(
                $"Student {studentId} has reached the maximum of {student.MaxActiveBorrowings} active borrowings.");

        equipment.MarkAsBorrowed();
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        var borrowing = new Borrowing(
            id: GenerateBorrowingId(),
            studentId: studentId,
            equipmentId: equipmentId,
            dateBorrowed: DateTime.UtcNow,
            expectedReturnDate: expectedReturnDate);

        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowResult.Ok(borrowing.Id);
    }

    private static int GenerateBorrowingId() => Random.Shared.Next(1000, 999999);
}