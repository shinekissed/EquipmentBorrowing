using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowingHistoryServiceTests
{
    private static (BorrowingHistoryService service, InMemoryBorrowingRepository repo) CreateServiceWithData()
    {
        var student1 = new Student(1, "2026-0001", "Alice Reyes");
        var student2 = new Student(2, "2026-0002", "Bob Cruz");
        var student3 = new Student(3, "2026-0003", "Charlie Tan"); // No history

        var eq1 = new Equipment(10, "EQ-010", "Digital Multimeter");
        var eq2 = new Equipment(20, "EQ-020", "Oscilloscope");

        var b1 = new Borrowing(101, 1, 10, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-3));
        b1.MarkAsReturned(DateTime.UtcNow.AddDays(-4));

        var b2 = new Borrowing(102, 1, 20, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(5)); // Active

        var b3 = new Borrowing(103, 2, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6)); // Active

        var studentRepo = new InMemoryStudentRepository(new[] { student1, student2, student3 });
        var eqRepo = new InMemoryEquipmentRepository(new[] { eq1, eq2 });
        var borrowingRepo = new InMemoryBorrowingRepository(
            new[] { b1, b2, b3 },
            new[] { student1, student2, student3 },
            new[] { eq1, eq2 });

        var service = new BorrowingHistoryService(borrowingRepo, studentRepo, eqRepo);
        return (service, borrowingRepo);
    }

    [Fact]
    public async Task GetStudentBorrowingHistoryAsync_ReturnsOnlySelectedStudentHistory()
    {
        var (service, _) = CreateServiceWithData();

        var historyAlice = await service.GetStudentBorrowingHistoryAsync(1);
        var historyBob = await service.GetStudentBorrowingHistoryAsync(2);
        var historyCharlie = await service.GetStudentBorrowingHistoryAsync(3);

        // Alice has 2 borrowings (1 returned, 1 active)
        Assert.Equal(2, historyAlice.Count);
        Assert.All(historyAlice, r => Assert.Equal("Alice Reyes", r.StudentName));

        // Bob has 1 borrowing
        Assert.Single(historyBob);
        Assert.Equal("Bob Cruz", historyBob[0].StudentName);

        // Charlie has 0 borrowings (empty state)
        Assert.Empty(historyCharlie);
    }

    [Fact]
    public async Task GetEquipmentBorrowingHistoryAsync_ReturnsOnlySelectedEquipmentHistory()
    {
        var (service, _) = CreateServiceWithData();

        var historyEq10 = await service.GetEquipmentBorrowingHistoryAsync(10);
        var historyEq20 = await service.GetEquipmentBorrowingHistoryAsync(20);

        // Eq10 was borrowed by Alice (returned) and Bob (active)
        Assert.Equal(2, historyEq10.Count);
        Assert.All(historyEq10, r => Assert.Equal("EQ-010", r.EquipmentIdentifier));

        // Eq20 was borrowed only once
        Assert.Single(historyEq20);
        Assert.Equal("EQ-020", historyEq20[0].EquipmentIdentifier);
    }

    [Fact]
    public async Task SearchBorrowingsAsync_FilterByStatus_WorksCorrectly()
    {
        var (service, _) = CreateServiceWithData();

        var activeOnly = await service.SearchBorrowingsAsync(null, BorrowingStatusFilter.Active, BorrowingSortField.CheckoutDate, false);
        var returnedOnly = await service.SearchBorrowingsAsync(null, BorrowingStatusFilter.Returned, BorrowingSortField.CheckoutDate, false);

        Assert.Equal(2, activeOnly.Count);
        Assert.All(activeOnly, r => Assert.Equal(BorrowingStatus.Active, r.Status));

        Assert.Single(returnedOnly);
        Assert.Equal(BorrowingStatus.Returned, returnedOnly[0].Status);
    }

    [Fact]
    public async Task SearchBorrowingsAsync_SearchText_MatchesPartialCaseInsensitive()
    {
        var (service, _) = CreateServiceWithData();

        // Search by student name
        var searchName = await service.SearchBorrowingsAsync("alice", BorrowingStatusFilter.All, BorrowingSortField.CheckoutDate, false);
        Assert.Equal(2, searchName.Count);

        // Search by student number
        var searchNumber = await service.SearchBorrowingsAsync("0002", BorrowingStatusFilter.All, BorrowingSortField.CheckoutDate, false);
        Assert.Single(searchNumber);
        Assert.Equal("Bob Cruz", searchNumber[0].StudentName);

        // Search by equipment name
        var searchEq = await service.SearchBorrowingsAsync("oscillo", BorrowingStatusFilter.All, BorrowingSortField.CheckoutDate, false);
        Assert.Single(searchEq);
        Assert.Equal("Oscilloscope", searchEq[0].EquipmentName);
    }

    [Fact]
    public async Task SearchBorrowingsAsync_SortsCorrectly()
    {
        var (service, _) = CreateServiceWithData();

        var asc = await service.SearchBorrowingsAsync(null, BorrowingStatusFilter.All, BorrowingSortField.CheckoutDate, true);
        var desc = await service.SearchBorrowingsAsync(null, BorrowingStatusFilter.All, BorrowingSortField.CheckoutDate, false);

        Assert.True(asc[0].DateBorrowed <= asc[^1].DateBorrowed);
        Assert.True(desc[0].DateBorrowed >= desc[^1].DateBorrowed);
    }
}
