using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

// ---- Seed data (this is the composition root: the one place allowed to know about every layer) ----

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

Console.WriteLine("=== Equipment Borrowing System — Demonstration ===\n");

// ---- Case 1: Successful borrow ----
Console.WriteLine("Case 1: Student 1 borrows equipment 100 (should succeed)");
var result1 = await borrowService.ExecuteAsync(
    studentId: 1,
    equipmentId: 100,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result1);

// ---- Case 2: Failure — equipment already unavailable ----
Console.WriteLine("\nCase 2: Student 1 tries to borrow equipment 101, already unavailable (should fail)");
var result2 = await borrowService.ExecuteAsync(
    studentId: 1,
    equipmentId: 101,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result2);

// ---- Case 3: Failure — student not allowed to borrow ----
Console.WriteLine("\nCase 3: Student 2 (not allowed) tries to borrow equipment 100 (should fail)");
var result3 = await borrowService.ExecuteAsync(
    studentId: 2,
    equipmentId: 100,
    expectedReturnDate: DateTime.UtcNow.AddDays(7));

PrintBorrowResult(result3);

static void PrintBorrowResult(BorrowResult result)
{
    Console.WriteLine(result.Success
        ? $"  SUCCESS - Borrowing #{result.BorrowingId} created."
        : $"  FAILED - {result.ErrorMessage}");
}