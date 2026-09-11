using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public EquipmentViewModel EquipmentVm { get; }
    public BorrowingsViewModel BorrowingsVm { get; }

    public MainViewModel() : this(new EquipmentViewModel(), new BorrowingsViewModel())
    {
    }

    // Constructor Injection (Part H)
    public MainViewModel(EquipmentViewModel equipmentVm, BorrowingsViewModel borrowingsVm)
    {
        EquipmentVm = equipmentVm;
        BorrowingsVm = borrowingsVm;

        // Auto-refresh cross-view state when an action happens
        EquipmentVm.OnBorrowSucceeded = () => BorrowingsVm.Refresh();
        BorrowingsVm.OnReturnSucceeded = () => EquipmentVm.Refresh();

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
}