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
    private readonly BorrowEquipmentService? _borrowService;
    private readonly EquipmentService? _equipmentService;
    private readonly StudentService? _studentService;

    public ObservableCollection<Equipment> EquipmentList { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTime? _expectedReturnDate = DateTime.Today.AddDays(7);

    // Register Form Fields (UC-15)
    [ObservableProperty]
    private string _newIdentifier = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    // Edit Form Fields (UC-16)
    [ObservableProperty]
    private string _editIdentifier = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    // Feedback Banner
    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public Action? OnBorrowSucceeded { get; set; }

    // Fallback constructor
    public EquipmentViewModel() : this(null!, null!, null!)
    {
    }

    // DI Constructor
    public EquipmentViewModel(
        BorrowEquipmentService? borrowService,
        EquipmentService? equipmentService = null,
        StudentService? studentService = null)
    {
        _borrowService = borrowService;
        _equipmentService = equipmentService;
        _studentService = studentService;
        Refresh();
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is not null)
        {
            EditIdentifier = value.Identifier;
            EditName = value.Name;
            EditDescription = value.Description ?? string.Empty;
        }
        else
        {
            EditIdentifier = string.Empty;
            EditName = string.Empty;
            EditDescription = string.Empty;
        }
    }

    public void Refresh()
    {
        EquipmentList.Clear();
        Students.Clear();

        if (_equipmentService != null)
        {
            var equipmentFromDb = _equipmentService.GetAllEquipmentAsync().GetAwaiter().GetResult();
            foreach (var item in equipmentFromDb.OrderBy(e => e.Name))
            {
                EquipmentList.Add(item);
            }
        }

        if (_studentService != null)
        {
            var studentsFromDb = _studentService.GetAllStudentsAsync().GetAwaiter().GetResult();
            foreach (var s in studentsFromDb.OrderBy(s => s.Name))
            {
                Students.Add(s);
            }
        }

        if (EquipmentList.Count > 0 && (SelectedEquipment is null || !EquipmentList.Any(e => e.Id == SelectedEquipment.Id)))
            SelectedEquipment = EquipmentList[0];

        if (Students.Count > 0 && (SelectedStudent is null || !Students.Any(s => s.Id == SelectedStudent.Id)))
            SelectedStudent = Students[0];
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
        if (_borrowService is null) return;

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

    [RelayCommand]
    public async Task RegisterEquipmentAsync()
    {
        if (_equipmentService is null) return;

        var result = await _equipmentService.RegisterEquipmentAsync(
            NewIdentifier,
            NewName,
            string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            NewIdentifier = string.Empty;
            NewName = string.Empty;
            NewDescription = string.Empty;
            Refresh();
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    [RelayCommand]
    public async Task UpdateEquipmentAsync()
    {
        if (_equipmentService is null) return;

        if (SelectedEquipment is null)
        {
            ShowFeedback("Please select an equipment item from the list first.", false);
            return;
        }

        var result = await _equipmentService.UpdateEquipmentAsync(
            SelectedEquipment.Id,
            EditIdentifier,
            EditName,
            string.IsNullOrWhiteSpace(EditDescription) ? null : EditDescription);

        if (result.Success)
        {
            ShowFeedback(result.Message, true);
            var currentId = SelectedEquipment.Id;
            Refresh();
            SelectedEquipment = EquipmentList.FirstOrDefault(e => e.Id == currentId);
        }
        else
        {
            ShowFeedback(result.Message, false);
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        FeedbackMessage = message;
        IsFeedbackSuccess = isSuccess;
        HasFeedback = true;
    }
}
