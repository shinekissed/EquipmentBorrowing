using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;


var students = new List<Student>
    {
    new(id: 1, name: "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2),
    new(id: 2, name: "Maria Santos", isAllowedToBorrow: false, maxActiveBorrowings: 2),
    };

var equipment = new List<Equipment>
{
    new(id: 100, name: "Digital Multimeter", isAvailable: true),
    new(id: 101, name: "Oscilloscope", isAvailable: false),
};

IStudentRepository studentRepository = new InMemoryStudentRepository(students);
IEquipmentRepository equipmentRepository = new InMemoryEquipmentRepository(equipment);
IBorrowingRepository borrowingRepository = new InMemoryBorrowingRepository();

var borrowService = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
var returnService = new ReturnEquipmentService(equipmentRepository, borrowingRepository);

Console.WriteLine("=== Equipment Borrowing System — Demonstration ===\n");

Console.WriteLine("Case 1: Student 1 borrows equipment 100 (should succeed)");
var result1 = await borrowService.ExecuteAsync(
    studentId: 1,
    equipmentId: 100,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result1);

Console.WriteLine("\nCase 2: Student 1 tries to borrow equipment 101, already unavailable (should fail)");
var result2 = await borrowService.ExecuteAsync(
    studentId: 1,
    equipmentId: 101,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result2);

Console.WriteLine("\nCase 3: Student 2 (not allowed) tries to borrow equipment 100 (should fail)");
var result3 = await borrowService.ExecuteAsync(
    studentId: 2,
    equipmentId: 100,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result3);

Console.WriteLine("\nCase 4: Student 1 returns equipment 100 (should succeed)");
var returnResult1 = await returnService.ExecuteAsync(
    studentId: 1,
    equipmentId: 100);

PrintReturnResult(returnResult1);

Console.WriteLine("\nCase 5: Student 1 tries to return equipment 100 again (should fail)");
var returnResult2 = await returnService.ExecuteAsync(
    studentId: 1,
    equipmentId: 100);

PrintReturnResult(returnResult2);

static void PrintBorrowResult(BorrowResult result)
{
    Console.WriteLine(result.Success
        ? $"  SUCCESS - Borrowing #{result.BorrowingId} created."
        : $"  FAILED - {result.ErrorMessage}");
}

static void PrintReturnResult(ReturnResult result)
{
    Console.WriteLine(result.Success
        ? "  SUCCESS - Equipment returned successfully."
        : $"  FAILED - {result.ErrorMessage}");
}