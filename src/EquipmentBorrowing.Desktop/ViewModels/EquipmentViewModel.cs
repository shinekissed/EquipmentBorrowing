using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly BorrowEquipmentService _borrowService;
    private readonly List<Equipment> _sharedEquipment;

    public ObservableCollection<Equipment> EquipmentList { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTimeOffset? _expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public Action? OnBorrowSucceeded { get; set; }

    // Fallback constructor
    public EquipmentViewModel()
    {
        _borrowService = null!;
        _sharedEquipment = new();
    }

    // Constructor Injection (Part H)
    public EquipmentViewModel(
        BorrowEquipmentService borrowService,
        List<Equipment> sharedEquipment,
        List<Student> sharedStudents)
    {
        _borrowService = borrowService;
        _sharedEquipment = sharedEquipment;

        foreach (var s in sharedStudents)
            Students.Add(s);

        if (Students.Count > 0)
            SelectedStudent = Students[0];

        Refresh();
    }

    public void Refresh()
    {
        EquipmentList.Clear();
        foreach (var item in _sharedEquipment)
        {
            EquipmentList.Add(item);
        }

        if (EquipmentList.Count > 0 && SelectedEquipment is null)
            SelectedEquipment = EquipmentList[0];
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
            expectedReturnDate: ExpectedReturnDate.Value.DateTime);

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