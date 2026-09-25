using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // Automatically ensure SQLite database & tables exist on startup
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();
            db.Database.Migrate();
        }

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
        // 1. Configure SQLite Database Connection
        services.AddDbContext<EquipmentBorrowingDbContext>(options =>
            options.UseSqlite("Data Source=equipment_borrowing.db"), ServiceLifetime.Transient);

        // 2. Register Database-Backed Repositories (Part J)
        services.AddTransient<IStudentRepository, EfStudentRepository>();
        services.AddTransient<IEquipmentRepository, EfEquipmentRepository>();
        services.AddTransient<IBorrowingRepository, EfBorrowingRepository>();

        // 3. Register Application Services
        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        // 4. Register ViewModels
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddSingleton<MainViewModel>();
    }
}