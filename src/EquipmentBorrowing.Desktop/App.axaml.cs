using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure Dependency Injection (Composition Root per Part H)
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 1. Initial Seed Data
        var seedStudents = new List<Student>
        {
            new(1, "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 2),
            new(2, "Maria Santos (Suspended)", isAllowedToBorrow: false, maxActiveBorrowings: 2),
            new(3, "Pedro Penduko", isAllowedToBorrow: true, maxActiveBorrowings: 2)
        };

        var seedEquipment = new List<Equipment>
        {
            new(100, "Digital Multimeter", isAvailable: true),
            new(101, "Digital Oscilloscope", isAvailable: false),
            new(102, "Soldering Station Kit", isAvailable: true),
            new(103, "DC Power Supply (30V/5A)", isAvailable: true),
            new(104, "Function Generator", isAvailable: false)
        };

        var seedBorrowings = new List<Borrowing>
        {
            new(1, studentId: 1, equipmentId: 101, dateBorrowed: DateTime.UtcNow.AddDays(-2), expectedReturnDate: DateTime.UtcNow.AddDays(5)),
            new(2, studentId: 1, equipmentId: 104, dateBorrowed: DateTime.UtcNow.AddDays(-4), expectedReturnDate: DateTime.UtcNow.AddDays(3))
        };

        var borrowingRepo = new InMemoryBorrowingRepository();
        foreach (var b in seedBorrowings)
        {
            borrowingRepo.AddAsync(b);
        }

        // 2. Register Repositories as Singletons (Part H - preserves state across views)
        services.AddSingleton<IStudentRepository>(new InMemoryStudentRepository(seedStudents));
        services.AddSingleton<IEquipmentRepository>(new InMemoryEquipmentRepository(seedEquipment));
        services.AddSingleton<IBorrowingRepository>(borrowingRepo);

        // Also register shared data collections for easy UI binding
        services.AddSingleton(seedStudents);
        services.AddSingleton(seedEquipment);

        // 3. Register Application Services as Transient (Part H)
        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        // 4. Register ViewModels
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddSingleton<MainViewModel>();
    }
}