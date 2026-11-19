using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public EquipmentViewModel EquipmentVm { get; }
    public BorrowingsViewModel BorrowingsVm { get; }
    public StudentManagementViewModel StudentVm { get; }
    public MultiItemBorrowViewModel MultiBorrowVm { get; }

    public MainViewModel() : this(
        new EquipmentViewModel(),
        new BorrowingsViewModel(),
        new StudentManagementViewModel(),
        new MultiItemBorrowViewModel())
    {
    }

    // Constructor Injection (Part H)
    public MainViewModel(
        EquipmentViewModel equipmentVm,
        BorrowingsViewModel borrowingsVm,
        StudentManagementViewModel studentVm,
        MultiItemBorrowViewModel multiBorrowVm)
    {
        EquipmentVm = equipmentVm;
        BorrowingsVm = borrowingsVm;
        StudentVm = studentVm;
        MultiBorrowVm = multiBorrowVm;

        // Auto-refresh cross-view state when an action happens
        EquipmentVm.OnBorrowSucceeded = () => BorrowingsVm.Refresh();
        BorrowingsVm.OnReturnSucceeded = () => EquipmentVm.Refresh();
        MultiBorrowVm.OnMultiBorrowSucceeded = () =>
        {
            EquipmentVm.Refresh();
            BorrowingsVm.Refresh();
        };

        _currentView = EquipmentVm;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        EquipmentVm.Refresh();
        CurrentView = EquipmentVm;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        BorrowingsVm.Refresh();
        CurrentView = BorrowingsVm;
    }

    [RelayCommand]
    private void ShowStudents()
    {
        StudentVm.LoadStudentsCommand.Execute(null);
        CurrentView = StudentVm;
    }

    [RelayCommand]
    private void ShowMultiBorrow()
    {
        MultiBorrowVm.LoadDataCommand.Execute(null);
        CurrentView = MultiBorrowVm;
    }
}
