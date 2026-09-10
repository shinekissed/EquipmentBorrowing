using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly ReturnEquipmentService _returnService;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public ObservableCollection<BorrowingItemDisplay> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private BorrowingItemDisplay? _selectedBorrowing;

    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public BorrowingsViewModel()
    {
        // 1. Initialize Repositories
        _borrowingRepository = new InMemoryBorrowingRepository();

        // Seed active borrowings in repository
        _borrowingRepository.AddAsync(new Borrowing(
            id: 1,
            studentId: 1,
            equipmentId: 101,
            dateBorrowed: DateTime.UtcNow.AddDays(-2),
            expectedReturnDate: DateTime.UtcNow.AddDays(5)));

        _borrowingRepository.AddAsync(new Borrowing(
            id: 2,
            studentId: 1,
            equipmentId: 104,
            dateBorrowed: DateTime.UtcNow.AddDays(-4),
            expectedReturnDate: DateTime.UtcNow.AddDays(3)));

        // Seed equipment in repository
        _equipmentRepository = new InMemoryEquipmentRepository(new List<Equipment>
        {
            new(101, "Oscilloscope", isAvailable: false),
            new(104, "Function Generator", isAvailable: false)
        });

        // 2. Initialize Return Application Service
        _returnService = new ReturnEquipmentService(_equipmentRepository, _borrowingRepository);

        // 3. Load UI display items
        LoadData();
    }

    private void LoadData()
    {
        ActiveBorrowings.Add(new BorrowingItemDisplay(
            id: 1,
            studentName: "Juan Dela Cruz",
            studentId: 1,
            equipmentName: "Oscilloscope",
            equipmentId: 101,
            dateBorrowed: DateTime.Now.AddDays(-2),
            dueDate: DateTime.Now.AddDays(5)
        ));

        ActiveBorrowings.Add(new BorrowingItemDisplay(
            id: 2,
            studentName: "Juan Dela Cruz",
            studentId: 1,
            equipmentName: "Function Generator",
            equipmentId: 104,
            dateBorrowed: DateTime.Now.AddDays(-4),
            dueDate: DateTime.Now.AddDays(3)
        ));

        if (ActiveBorrowings.Count > 0)
        {
            SelectedBorrowing = ActiveBorrowings[0];
        }
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync()
    {
        // --- 1. Presentation Validation (Handled by ViewModel per Part I) ---
        if (SelectedBorrowing is null)
        {
            ShowFeedback("Please select an active borrowing record from the list.", false);
            return;
        }

        // --- 2. Business Operation (Handled by Application Service per Part F) ---
        var result = await _returnService.ExecuteAsync(
            studentId: SelectedBorrowing.StudentId,
            equipmentId: SelectedBorrowing.EquipmentId);

        if (result.Success)
        {
            ShowFeedback($"SUCCESS! Equipment '{SelectedBorrowing.EquipmentName}' returned successfully.", true);

            // Remove returned item from the active borrowings list
            var returnedItem = SelectedBorrowing;
            ActiveBorrowings.Remove(returnedItem);
            SelectedBorrowing = ActiveBorrowings.Count > 0 ? ActiveBorrowings[0] : null;
        }
        else
        {
            ShowFeedback($"FAILED: {result.ErrorMessage}", false);
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        FeedbackMessage = message;
        IsFeedbackSuccess = isSuccess;
        HasFeedback = true;
    }
}

// Helper display model for the UI
public class BorrowingItemDisplay
{
    public int Id { get; }
    public string StudentName { get; }
    public int StudentId { get; }
    public string EquipmentName { get; }
    public int EquipmentId { get; }
    public DateTime DateBorrowed { get; }
    public DateTime DueDate { get; }
    public string Status { get; }

    public BorrowingItemDisplay(int id, string studentName, int studentId, string equipmentName, int equipmentId, DateTime dateBorrowed, DateTime dueDate)
    {
        Id = id;
        StudentName = studentName;
        StudentId = studentId;
        EquipmentName = equipmentName;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        DueDate = dueDate;
        Status = "Active";
    }
}