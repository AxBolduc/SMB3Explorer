﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SMB3Explorer.ApplicationConfig;
using SMB3Explorer.Enums;
using SMB3Explorer.Services.ApplicationContext;
using SMB3Explorer.Services.CsvWriterWrapper;
using SMB3Explorer.Services.DataService;
using SMB3Explorer.Services.DataService.RosterDataService;
using SMB3Explorer.Services.DataService.RosterDataService.SMB2;
using SMB3Explorer.Services.DataService.SMBEI;
using SMB3Explorer.Services.HttpService;
using SMB3Explorer.Services.NavigationService;
using SMB3Explorer.Services.SystemIoWrapper;
using SMB3Explorer.Utils;
using SMB3Explorer.ViewModels;
using SMB3Explorer.Views;
using SmbEiDataService = SMB3Explorer.Services.DataService.RosterDataService.SMBEI.SmbEiDataService;

namespace SMB3Explorer;

public partial class App
{
    private IServiceProvider ServiceProvider { get; set; } = null!;
    private IServiceCollection Services { get; set; } = null!;

    public App()
    {
#if RELEASE
        DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.InitializeLogger();
        Services = new ServiceCollection();
        await ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        await ((MainWindowViewModel) mainWindow.DataContext).Initialize();
    }

    private static Task ConfigureServices(IServiceCollection services)
    {
        Log.Information("Configuring services...");
        services.AddHttpClient();
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IApplicationContext, ApplicationContext>();
        services.AddSingleton<ISystemIoWrapper, SystemIoWrapper>();
        services.AddSingleton<IApplicationConfig, ApplicationConfig.ApplicationConfig>();

        services.AddSingleton<MainWindow>(serviceProvider => new MainWindow
        {
            DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
        });

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<LandingViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<RosterViewModel>();
        services.AddTransient<ICsvWriterWrapper, CsvWriterWrapper>();

        services.AddTransient<IRosterDataService, SmbEiDataService>();
        services.AddTransient<IRosterDataService, Smb2RosterDataService>();
        services.AddSingleton<Func<IEnumerable<IRosterDataService>>>(x => () => x.GetService<IEnumerable<IRosterDataService>>()!);
        services.AddSingleton<IRosterDataServiceFactory, RosterDataServiceFactory>();

                // NavigationService calls this Func to get the ViewModel instance
        services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider =>
            viewModelType => (ViewModelBase) serviceProvider.GetRequiredService(viewModelType));

        Log.Information("Finished configuring services");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Global exception handler. In debug mode, this will not be called and exceptions will be thrown.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled exception");

        var systemIoWrapper = ServiceProvider.GetRequiredService<ISystemIoWrapper>();
        DefaultExceptionHandler.HandleException(systemIoWrapper,
            "An unexpected error occurred that caused the termination of the program.", e.Exception);
        e.Handled = true;

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
