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

    public async Task<BorrowResult> ExecuteMultiAsync(
        int studentId,
        IEnumerable<int> equipmentIds,
        DateTime expectedReturnDate,
        bool simulateTransactionFailure = false,
        CancellationToken cancellationToken = default)
    {
        var idList = equipmentIds?.ToList() ?? new List<int>();

        // BR-12: Must contain at least two items
        if (idList.Count < 2)
            return BorrowResult.Fail("Multi-item checkout requires at least two distinct equipment items (BR-12).");

        // BR-12: Must be distinct (reject duplicates)
        if (idList.Distinct().Count() != idList.Count)
            return BorrowResult.Fail("Duplicate equipment selections detected. Multi-item checkout requires distinct items (BR-12).");

        // BR-11: Expected return date must be strictly after checkout date
        if (expectedReturnDate.Date <= DateTime.Today)
            return BorrowResult.Fail("Expected return date must be later than the checkout date (BR-11).");

        // BR-08: Student must exist
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return BorrowResult.Fail($"Student {studentId} does not exist (BR-08).");

        // BR-04, BR-08: Student must be eligible to borrow
        if (!student.IsAllowedToBorrow)
            return BorrowResult.Fail($"Student '{student.Name}' is not currently allowed to borrow equipment (BR-04, BR-08).");

        // BR-09: Active limit of 3 items
        var activeCount = await _borrowingRepository.CountActiveByStudentIdAsync(studentId, cancellationToken);
        if (activeCount + idList.Count > student.MaxActiveBorrowings)
            return BorrowResult.Fail(
                $"Student '{student.Name}' cannot borrow {idList.Count} items. Active borrowings: {activeCount}, limit: {student.MaxActiveBorrowings} (BR-09).");

        // BR-08, BR-10: Equipment validation
        var equipmentItems = new List<Equipment>();
        foreach (var eqId in idList)
        {
            var eq = await _equipmentRepository.GetByIdAsync(eqId, cancellationToken);
            if (eq is null)
                return BorrowResult.Fail($"Equipment {eqId} does not exist (BR-08).");

            if (!eq.IsAvailable)
                return BorrowResult.Fail($"Equipment '{eq.Name}' ({eq.Identifier}) is not currently available (BR-10).");

            equipmentItems.Add(eq);
        }

        // Prepare domain entities
        var borrowings = new List<Borrowing>();
        var now = DateTime.UtcNow;
        foreach (var eq in equipmentItems)
        {
            eq.MarkAsBorrowed();
            borrowings.Add(new Borrowing(
                id: GenerateBorrowingId(),
                studentId: studentId,
                equipmentId: eq.Id,
                dateBorrowed: now,
                expectedReturnDate: expectedReturnDate));
        }

        // Commit atomically via repository transaction (BR-15, BR-16)
        try
        {
            await _borrowingRepository.CheckoutBatchAsync(
                borrowings,
                equipmentItems,
                simulateTransactionFailure,
                cancellationToken);

            return BorrowResult.OkMulti(borrowings.Select(b => b.Id).ToList());
        }
        catch (Exception ex)
        {
            // Revert in-memory equipment availability changes in case of failure
            foreach (var eq in equipmentItems)
            {
                eq.MarkAsReturned();
            }

            return BorrowResult.Fail($"Transaction failed and was safely rolled back: {ex.Message} (BR-15, BR-18).");
        }
    }

    private static int GenerateBorrowingId() => Random.Shared.Next(1000, 999999);
}
