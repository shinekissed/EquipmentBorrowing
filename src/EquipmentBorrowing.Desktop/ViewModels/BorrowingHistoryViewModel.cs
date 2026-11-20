using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.DTOs;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingHistoryViewModel : ViewModelBase
{
    private readonly BorrowingHistoryService? _historyService;
    private readonly StudentService? _studentService;
    private readonly EquipmentService? _equipmentService;

    // UC-07: Student History
    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<BorrowingRecordDto> StudentHistory { get; } = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private bool _isStudentHistoryLoading;

    [ObservableProperty]
    private bool _hasNoStudentHistory;

    // UC-08: Equipment History
    public ObservableCollection<Equipment> EquipmentList { get; } = new();
    public ObservableCollection<BorrowingRecordDto> EquipmentHistory { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private bool _isEquipmentHistoryLoading;

    [ObservableProperty]
    private bool _hasNoEquipmentHistory;

    // Feedback
    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public BorrowingHistoryViewModel() : this(null!, null!, null!)
    {
    }

    public BorrowingHistoryViewModel(
        BorrowingHistoryService? historyService,
        StudentService? studentService,
        EquipmentService? equipmentService)
    {
        _historyService = historyService;
        _studentService = studentService;
        _equipmentService = equipmentService;

        _ = RefreshAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (_studentService is null || _equipmentService is null) return;

        try
        {
            var students = await _studentService.GetAllStudentsAsync();
            Students.Clear();
            foreach (var s in students.OrderBy(x => x.Name))
            {
                Students.Add(s);
            }

            var equipment = await _equipmentService.GetAllEquipmentAsync();
            EquipmentList.Clear();
            foreach (var e in equipment.OrderBy(x => x.Name))
            {
                EquipmentList.Add(e);
            }

            if (Students.Count > 0 && SelectedStudent is null)
                SelectedStudent = Students[0];

            if (EquipmentList.Count > 0 && SelectedEquipment is null)
                SelectedEquipment = EquipmentList[0];
        }
        catch (Exception ex)
        {
            ShowFeedback($"Failed to load master records: {ex.Message}", false);
        }
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is not null)
        {
            _ = LoadStudentHistoryAsync(value.Id);
        }
        else
        {
            StudentHistory.Clear();
            HasNoStudentHistory = true;
        }
    }

    private async Task LoadStudentHistoryAsync(int studentId)
    {
        if (_historyService is null) return;

        IsStudentHistoryLoading = true;
        try
        {
            var list = await _historyService.GetStudentBorrowingHistoryAsync(studentId);
            StudentHistory.Clear();
            foreach (var record in list)
            {
                StudentHistory.Add(record);
            }
            HasNoStudentHistory = StudentHistory.Count == 0;
        }
        catch (Exception ex)
        {
            ShowFeedback($"Error loading student history: {ex.Message}", false);
            StudentHistory.Clear();
            HasNoStudentHistory = true;
        }
        finally
        {
            IsStudentHistoryLoading = false;
        }
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is not null)
        {
            _ = LoadEquipmentHistoryAsync(value.Id);
        }
        else
        {
            EquipmentHistory.Clear();
            HasNoEquipmentHistory = true;
        }
    }

    private async Task LoadEquipmentHistoryAsync(int equipmentId)
    {
        if (_historyService is null) return;

        IsEquipmentHistoryLoading = true;
        try
        {
            var list = await _historyService.GetEquipmentBorrowingHistoryAsync(equipmentId);
            EquipmentHistory.Clear();
            foreach (var record in list)
            {
                EquipmentHistory.Add(record);
            }
            HasNoEquipmentHistory = EquipmentHistory.Count == 0;
        }
        catch (Exception ex)
        {
            ShowFeedback($"Error loading equipment history: {ex.Message}", false);
            EquipmentHistory.Clear();
            HasNoEquipmentHistory = true;
        }
        finally
        {
            IsEquipmentHistoryLoading = false;
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        FeedbackMessage = message;
        IsFeedbackSuccess = isSuccess;
        HasFeedback = true;
    }
}
