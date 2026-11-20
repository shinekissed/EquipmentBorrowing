using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly ReturnEquipmentService _returnService;
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

    public Action? OnReturnSucceeded { get; set; }

    // Fallback constructor
    public BorrowingsViewModel() : this(null!, null!)
    {
    }

    // DI Constructor
    public BorrowingsViewModel(
        ReturnEquipmentService returnService,
        IBorrowingRepository borrowingRepository)
    {
        _returnService = returnService;
        _borrowingRepository = borrowingRepository;
        Refresh();
    }

    public void Refresh()
    {
        ActiveBorrowings.Clear();

        if (_borrowingRepository is null) return;

        var activeDtos = _borrowingRepository.GetActiveBorrowingsAsync().GetAwaiter().GetResult();
        foreach (var b in activeDtos)
        {
            ActiveBorrowings.Add(new BorrowingItemDisplay(
                b.Id,
                b.StudentName,
                b.StudentId,
                b.EquipmentName,
                b.EquipmentId,
                b.DateBorrowed,
                b.ExpectedReturnDate
            ));
        }

        if (ActiveBorrowings.Count > 0)
            SelectedBorrowing = ActiveBorrowings[0];
        else
            SelectedBorrowing = null;
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync()
    {
        if (SelectedBorrowing is null)
        {
            ShowFeedback("Please select an active borrowing record from the list.", false);
            return;
        }

        var result = await _returnService.ExecuteAsync(
            studentId: SelectedBorrowing.StudentId,
            equipmentId: SelectedBorrowing.EquipmentId);

        if (result.Success)
        {
            ShowFeedback($"SUCCESS! Equipment '{SelectedBorrowing.EquipmentName}' returned successfully.", true);
            Refresh();
            OnReturnSucceeded?.Invoke();
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

// Display model for the UI
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
