using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<ReturnResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository.GetActiveByStudentAndEquipmentAsync(
            studentId, equipmentId, cancellationToken);

        if (borrowing is null)
            return ReturnResult.Fail(
                $"No active borrowing found for student {studentId} and equipment {equipmentId}.");

        borrowing.MarkAsReturned();
        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        equipment!.MarkAsReturned();
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ReturnResult.Ok();
    }
}