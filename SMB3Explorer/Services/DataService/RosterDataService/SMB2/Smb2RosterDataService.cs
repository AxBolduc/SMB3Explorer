using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using OneOf;
using OneOf.Types;
using Serilog;
using SMB3Explorer.Enums;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Services.ApplicationContext;
using SMB3Explorer.Services.SystemIoWrapper;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMB2;

public partial class Smb2RosterDataService : ISmb2RosterDataService
{
    public SelectedGame GameType => SelectedGame.Smb2;

    private IApplicationContext _applicationContext;
    private ISystemIoWrapper _systemIoWrapper;

    private SqliteConnection? _connection;
    private string _currentFilePath = string.Empty;

    public Smb2RosterDataService(IApplicationContext applicationContext, ISystemIoWrapper systemIoWrapper)
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

    private SqliteConnection? Connection
    {
        get => _connection;
        set
        {
            SetField(ref _connection, value);

            var isConnectionNull = value is null;
            Log.Debug("Connection changed, is null: {IsConnectionNull}", isConnectionNull);
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsConnected => Connection is not null;

    public event EventHandler<EventArgs>? ConnectionChanged;

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

    public Task<OneOf<List<Smb4LeagueSelection>, Error<string>>> EstablishDbConnection(string filePath)
    {
        throw new System.NotImplementedException();
    }

    public Task Disconnect()
    {
        throw new System.NotImplementedException();
    }


}
