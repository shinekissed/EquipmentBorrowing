using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly BorrowEquipmentService _borrowService;

    public ObservableCollection<Equipment> EquipmentList { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTime? _expectedReturnDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public Action? OnBorrowSucceeded { get; set; }

    // Fallback constructor
    public EquipmentViewModel() : this(null!)
    {
    }

    // DI Constructor
    public EquipmentViewModel(BorrowEquipmentService borrowService)
    {
        _borrowService = borrowService;
        Refresh();
    }

    public void Refresh()
    {
        EquipmentList.Clear();
        Students.Clear();

        using var db = new EquipmentBorrowingDbContext();

        // LINQ Query 1 (Part M): Retrieve equipment from SQLite without tracking
        var equipmentFromDb = db.Equipment.AsNoTracking().ToList();
        foreach (var item in equipmentFromDb)
        {
            EquipmentList.Add(item);
        }

        // Query students from SQLite
        var studentsFromDb = db.Students.AsNoTracking().ToList();
        foreach (var s in studentsFromDb)
        {
            Students.Add(s);
        }

        if (EquipmentList.Count > 0 && (SelectedEquipment is null || !EquipmentList.Any(e => e.Id == SelectedEquipment.Id)))
            SelectedEquipment = EquipmentList[0];

        if (Students.Count > 0 && (SelectedStudent is null || !Students.Any(s => s.Id == SelectedStudent.Id)))
            SelectedStudent = Students[0];
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
        if (SelectedEquipment is null)
        {
            ShowFeedback("Please select an equipment item from the list.", false);
            return;
        }

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a borrower student.", false);
            return;
        }

        if (!ExpectedReturnDate.HasValue || ExpectedReturnDate.Value.Date < DateTime.Today)
        {
            ShowFeedback("Expected return date must be today or in the future.", false);
            return;
        }

        var result = await _borrowService.ExecuteAsync(
            studentId: SelectedStudent.Id,
            equipmentId: SelectedEquipment.Id,
            expectedReturnDate: ExpectedReturnDate.Value);

        if (result.Success)
        {
            ShowFeedback($"SUCCESS! Borrowing #{result.BorrowingId} recorded for {SelectedStudent.Name}.", true);
            Refresh();
            OnBorrowSucceeded?.Invoke();
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
