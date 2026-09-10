using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly BorrowEquipmentService _borrowService;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

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

    public EquipmentViewModel()
    {
        // 1. Seed equipment and students
        EquipmentList.Add(new Equipment(100, "Digital Multimeter", isAvailable: true));
        EquipmentList.Add(new Equipment(101, "Digital Oscilloscope", isAvailable: false));
        EquipmentList.Add(new Equipment(102, "Soldering Station Kit", isAvailable: true));
        EquipmentList.Add(new Equipment(103, "DC Power Supply (30V/5A)", isAvailable: true));
        EquipmentList.Add(new Equipment(104, "Function Generator", isAvailable: false));

        Students.Add(new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2));
        Students.Add(new Student(2, "Maria Santos (Suspended)", isAllowedToBorrow: false, maxActiveBorrowings: 2));
        Students.Add(new Student(3, "Pedro Penduko", isAllowedToBorrow: true, maxActiveBorrowings: 2));

        SelectedEquipment = EquipmentList[0];
        SelectedStudent = Students[0];

        // 2. Initialize repositories with the seed lists
        _studentRepository = new InMemoryStudentRepository(Students);
        _equipmentRepository = new InMemoryEquipmentRepository(EquipmentList);
        _borrowingRepository = new InMemoryBorrowingRepository();

        // 3. Initialize application service
        _borrowService = new BorrowEquipmentService(_studentRepository, _equipmentRepository, _borrowingRepository);
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
        // --- Presentation Validation (Handled by ViewModel per Part I) ---
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

        // --- Business Validation & Operation (Handled by Application Service) ---
        var result = await _borrowService.ExecuteAsync(
            studentId: SelectedStudent.Id,
            equipmentId: SelectedEquipment.Id,
            expectedReturnDate: ExpectedReturnDate.Value.DateTime);

        if (result.Success)
        {
            ShowFeedback($"SUCCESS! Borrowing #{result.BorrowingId} recorded for {SelectedStudent.Name}.", true);

            // Refresh UI list so availability badge updates
            var index = EquipmentList.IndexOf(SelectedEquipment);
            if (index >= 0)
            {
                EquipmentList[index] = SelectedEquipment;
            }
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