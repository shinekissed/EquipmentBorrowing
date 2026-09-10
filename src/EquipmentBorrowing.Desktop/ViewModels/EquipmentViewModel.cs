using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    public ObservableCollection<Equipment> EquipmentList { get; } = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    public EquipmentViewModel()
    {
        LoadSeedData();
    }

    private void LoadSeedData()
    {
        EquipmentList.Add(new Equipment(100, "Digital Multimeter", isAvailable: true));
        EquipmentList.Add(new Equipment(101, "Digital Oscilloscope", isAvailable: false));
        EquipmentList.Add(new Equipment(102, "Soldering Station Kit", isAvailable: true));
        EquipmentList.Add(new Equipment(103, "DC Power Supply (30V/5A)", isAvailable: true));
        EquipmentList.Add(new Equipment(104, "Function Generator", isAvailable: false));

        // Select first item by default
        SelectedEquipment = EquipmentList[0];
    }
}