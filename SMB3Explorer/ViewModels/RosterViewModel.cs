using Serilog;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Services.ApplicationContext;
using SMB3Explorer.Services.DataService;
using SMB3Explorer.Services.DataService.SMBEI;
using SMB3Explorer.Services.NavigationService;
using SMB3Explorer.Services.SystemIoWrapper;
using SMB3Explorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using OneOf;
using SMB3Explorer.Enums;
using SMB3Explorer.Services.DataService.RosterDataService;
using SMB3Explorer.Services.DataService.RosterDataService.SMB2;

namespace SMB3Explorer.ViewModels;

public partial class RosterViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IRosterDataService _dataService;
    private readonly INavigationService _navigationService;
    private readonly ISystemIoWrapper _systemIoWrapper;

    private ObservableCollection<TeamSelection> _teams = new();
    private ObservableCollection<TeamConfigurationSelection> _teamConfigurations = new();
    private bool _interactionEnabled;
    private TeamSelection? _selectedTeam;
    private TeamConfigurationSelection? _selectedTeamConfiguration;
    private Roster? _roster = new();

    public RosterViewModel(INavigationService navigationService, IRosterDataServiceFactory dataServiceFactory,
        IApplicationContext applicationContext, ISystemIoWrapper systemIoWrapper)
    {
        _navigationService = navigationService;
        _applicationContext = applicationContext;
        _dataService = dataServiceFactory.create(_applicationContext.SelectedGame);
        _systemIoWrapper = systemIoWrapper;

        Log.Information("Initializing RosterViewModel");

        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;

        GetTeams();
    }

    private void ApplicationContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    public TeamSelection? SelectedTeam
    {
        get => _selectedTeam;
        set
        {
            SetField(ref _selectedTeam, value);
            _applicationContext.SelectedTeam = value;

            if (value != null)
            {
                GetTeamConfigurationsForTeam(value.id);
                if (!_applicationContext.SelectedGame.Equals(SelectedGame.SmbEI))
                {
                    GetRosterForTeamConfiguration(value.id, null);
                }
            }


            OnPropertyChanged(nameof(TeamSelected));
        }
    }

    public TeamConfigurationSelection? SelectedTeamConfiguration
    {
        get => _selectedTeamConfiguration;
        set
        {
            SetField(ref _selectedTeamConfiguration, value);
            _applicationContext.SelectedTeamConfiguration = value;

            if (value != null)
            {
                GetRosterForTeamConfiguration(_applicationContext.SelectedTeam!.id, value.id);
            }

            OnPropertyChanged(nameof(TeamConfigurationSelected));
        }
    }

    public Roster? CurrentRoster
    {
        get => _roster;
        set
        {
            SetField(ref _roster, value);
            _applicationContext.CurrentRoster = value;


            OnPropertyChanged(nameof(RosterSelected));
            OnPropertyChanged(nameof(CurrentRosterPitchers));
        }
    }

    public List<Player>? CurrentRosterPitchers
    {
        get => _roster!.Players.FindAll(player => player.PrimaryPosition.Equals("P"));
        set => _roster!.UpdatePitchers(value);
    }

    public ObservableCollection<TeamSelection> Teams
    {
        get => _teams;
        set => SetField(ref _teams, value);
    }

    public ObservableCollection<TeamConfigurationSelection> TeamConfigurations
    {
        get => _teamConfigurations;
        set => SetField(ref _teamConfigurations, value);
    }

    private bool TeamSelected => SelectedTeam is not null;
    private bool TeamConfigurationSelected => SelectedTeamConfiguration is not null;

    private bool RosterSelected => CurrentRoster is not null;

    public bool InteractionEnabled
    {
        get => _interactionEnabled;
        set => SetField(ref _interactionEnabled, value);
    }

    [RelayCommand(CanExecute = nameof(RosterSelected))]
    public void SaveRoster()
    {
        HandleSaveRoster();
    }

    private void GetTeams()
    {
        _dataService.GetTeams()
            .ContinueWith(async task =>
            {
                if (task.Exception != null)
                {
                    DefaultExceptionHandler.HandleException(_systemIoWrapper, "Failed to get teams.",
                        task.Exception);
                    return;
                }

                if (task.Result.Any())
                {
                    Log.Debug("{Count} Teams found", task.Result.Count);
                    Teams = new ObservableCollection<TeamSelection>(task.Result);
                    InteractionEnabled = true;
                }
                else
                {
                    Log.Debug("No teams found, navigating to landing page");
                    MessageBox.Show("No teams found. Please select a different save file.");
                    await _dataService.Disconnect();
                    _navigationService.NavigateTo<LandingViewModel>();
                }
            });
    }

    private void GetTeamConfigurationsForTeam(string teamId)
    {
        var testId = _applicationContext.SelectedGame.Equals(SelectedGame.SmbEI) ? int.Parse(teamId) : 0;

        _dataService.GetTeamConfigurations(testId)
            .ContinueWith( task =>
            {
                if (task.Exception != null)
                {
                    DefaultExceptionHandler.HandleException(_systemIoWrapper, "Failed to get teams.",
                        task.Exception);
                    return;
                }

                if (task.Result.Any())
                {
                    Log.Debug("{Count} team configurations found for the {Team}", task.Result.Count, teamId);
                    TeamConfigurations = new ObservableCollection<TeamConfigurationSelection>(task.Result);
                    InteractionEnabled = true;
                }
                else
                {
                    Log.Debug("No team configurations found, navigating to landing page");
                    // MessageBox.Show("No team configurations found. Please select a different save file.");
                    // await _dataService.Disconnect();
                    // _navigationService.NavigateTo<LandingViewModel>();
                }
            });
    }

    private void GetRosterForTeamConfiguration(string teamId, int? teamConfigurationId)
    {
        OneOf<int, Guid> parsedTeamId = _applicationContext.SelectedGame switch
        {
            SelectedGame.SmbEI => int.Parse(teamId),
            SelectedGame.Smb2 => Guid.Parse(teamId),
            _ => throw new NotImplementedException()
        };

        _dataService.GetRosterForTeam(parsedTeamId, teamConfigurationId)
            .ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    DefaultExceptionHandler.HandleException(_systemIoWrapper, "Failed to get roster for team",
                        task.Exception);
                    return;
                }

                Log.Debug("Roster found with {Count} players", task.Result.Players.Count);
                CurrentRoster = task.Result;
            });
    }


    private void HandleSaveRoster()
    {
        Log.Debug("Saving");

        if (SelectedTeam == null || SelectedTeamConfiguration == null || CurrentRoster == null)
        {
            Log.Debug("Could not save roster, something is null");
            return;
        }

        OneOf<int, Guid> parsedTeamId = _applicationContext.SelectedGame switch
        {
            SelectedGame.SmbEI => int.Parse(SelectedTeam.id),
            SelectedGame.Smb2 => Guid.Parse(SelectedTeam.id),
            _ => throw new NotImplementedException()
        };

        _dataService.SaveRosterForTeam(parsedTeamId, SelectedTeamConfiguration.id, CurrentRoster)
            .ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    DefaultExceptionHandler.HandleException(_systemIoWrapper, "Failed to get roster for team",
                        task.Exception);
                    return;
                }

                if (task.Result.TryPickT1(out var error, out var success))
                {
                    DefaultExceptionHandler.HandleException(_systemIoWrapper,
                        $"Failed to get roster for team {error.Value}",
                        new Exception(error.Value));
                    return;
                }

                Log.Debug("Saved Roster Successfully");
            });
    }
}
