using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using Serilog;
using SMB3Explorer.Enums;
using SMB3Explorer.Services.ApplicationContext;
using SMB3Explorer.Services.DataService.SMBEI;
using SMB3Explorer.Services.SystemIoWrapper;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMBEI;
public sealed partial class SmbEiDataService: INotifyPropertyChanged, ISmbEiDataService
{
    public SelectedGame GameType => SelectedGame.SmbEI;

    private readonly IApplicationContext _applicationContext;
    private readonly ISystemIoWrapper _systemIoWrapper;

    private string _currentFilePath = string.Empty;

    public SmbEiDataService(IApplicationContext applicationContext, ISystemIoWrapper systemIoWrapper)
    {
        _applicationContext = applicationContext;
        _systemIoWrapper = systemIoWrapper;

        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;
    }

    private void ApplicationContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IApplicationContext.SelectedFranchise):
            {
                if (_applicationContext.SelectedFranchise is null)
                {
                    Log.Information("Clearing cached franchise seasons");
                    _applicationContext.FranchiseSeasons.Clear();
                    _applicationContext.MostRecentFranchiseSeason = null;
                    break;
                }

                Mouse.OverrideCursor = Cursors.Wait;
                _applicationContext.FranchiseSeasonsLoading = true;
                Log.Information("Setting cached franchise seasons");
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);

                break;
            }
        }
    }

    public string CurrentFilePath
    {
        get => _currentFilePath;
        private set => SetField(ref _currentFilePath, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}
