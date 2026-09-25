using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly ReturnEquipmentService _returnService;
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly List<Equipment> _sharedEquipment;
    private readonly List<Student> _sharedStudents;

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
    public BorrowingsViewModel()
    {
        _returnService = null!;
        _borrowingRepository = null!;
        _sharedEquipment = new();
        _sharedStudents = new();
    }

    // Constructor Injection (Part H)
    public BorrowingsViewModel(
        ReturnEquipmentService returnService,
        IBorrowingRepository borrowingRepository,
        List<Equipment> sharedEquipment,
        List<Student> sharedStudents)
    {
        _returnService = returnService;
        _borrowingRepository = borrowingRepository;
        _sharedEquipment = sharedEquipment;
        _sharedStudents = sharedStudents;

        Refresh();
    }

    public void Refresh()
    {
        ActiveBorrowings.Clear();

        using var db = new EquipmentBorrowingDbContext();

        // LINQ Query 2 (Part M): Join active borrowings with Student and Equipment from SQLite
        var activeList = (from b in db.Borrowings.AsNoTracking()
                          join s in db.Students.AsNoTracking() on b.StudentId equals s.Id
                          join e in db.Equipment.AsNoTracking() on b.EquipmentId equals e.Id
                          where b.Status == BorrowingStatus.Active
                          select new BorrowingItemDisplay(
                              b.Id,
                              s.Name,
                              s.Id,
                              e.Name,
                              e.Id,
                              b.DateBorrowed,
                              b.ExpectedReturnDate
                          )).ToList();

        foreach (var item in activeList)
        {
            ActiveBorrowings.Add(item);
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