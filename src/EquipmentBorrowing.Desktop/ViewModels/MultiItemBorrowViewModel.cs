using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class SelectableEquipmentItem : ObservableObject
{
    public Equipment Item { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableEquipmentItem(Equipment item)
    {
        Item = item;
    }
}

public partial class MultiItemBorrowViewModel : ViewModelBase
{
    private readonly BorrowEquipmentService? _borrowService;
    private readonly StudentService? _studentService;
    private readonly EquipmentService? _equipmentService;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<SelectableEquipmentItem> AvailableEquipment { get; } = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private DateTime? _expectedReturnDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private bool _simulateTransactionFailure;

    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _isFeedbackSuccess;

    [ObservableProperty]
    private bool _hasFeedback;

    public Action? OnMultiBorrowSucceeded { get; set; }

    // Fallback constructor
    public MultiItemBorrowViewModel() : this(null!, null!, null!)
    {
    }

    // DI Constructor
    public MultiItemBorrowViewModel(
        BorrowEquipmentService? borrowService,
        StudentService? studentService,
        EquipmentService? equipmentService)
    {
        _borrowService = borrowService;
        _studentService = studentService;
        _equipmentService = equipmentService;

        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (_studentService is null || _equipmentService is null) return;

        try
        {
            // Load students
            var studentsList = await _studentService.GetAllStudentsAsync();
            Students.Clear();
            foreach (var student in studentsList.Where(s => s.IsEligible).OrderBy(s => s.Name))
            {
                Students.Add(student);
            }

            if (Students.Count > 0 && (SelectedStudent is null || !Students.Any(s => s.Id == SelectedStudent.Id)))
            {
                SelectedStudent = Students[0];
            }

            // Load available equipment
            var equipmentList = await _equipmentService.GetAllEquipmentAsync();
            AvailableEquipment.Clear();
            foreach (var eq in equipmentList.Where(e => e.IsAvailable).OrderBy(e => e.Name))
            {
                AvailableEquipment.Add(new SelectableEquipmentItem(eq));
            }
        }
        catch (Exception ex)
        {
            ShowFeedback($"Failed to load data: {ex.Message}", false);
        }
    }

    [RelayCommand]
    public async Task CheckoutMultiAsync()
    {
        if (_borrowService is null) return;

        if (SelectedStudent is null)
        {
            ShowFeedback("Please select a borrower student.", false);
            return;
        }

        var selectedItems = AvailableEquipment.Where(x => x.IsSelected).ToList();
        if (selectedItems.Count < 2)
        {
            ShowFeedback("Please select at least two equipment items to check out (BR-12).", false);
            return;
        }

        if (!ExpectedReturnDate.HasValue || ExpectedReturnDate.Value.Date <= DateTime.Today)
        {
            ShowFeedback("Expected return date must be strictly later than today (BR-11).", false);
            return;
        }

        var selectedIds = selectedItems.Select(x => x.Item.Id).ToList();

        var result = await _borrowService.ExecuteMultiAsync(
            SelectedStudent.Id,
            selectedIds,
            ExpectedReturnDate.Value,
            SimulateTransactionFailure);

        if (result.Success)
        {
            var idString = string.Join(", #", result.BorrowingIds);
            ShowFeedback($"SUCCESS! Atomically checked out {selectedIds.Count} items for {SelectedStudent.Name}. Borrowing IDs: #{idString}", true);
            await LoadDataAsync();
            OnMultiBorrowSucceeded?.Invoke();
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
