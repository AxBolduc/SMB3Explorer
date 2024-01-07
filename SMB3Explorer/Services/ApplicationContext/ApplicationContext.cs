using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SMB3Explorer.Enums;
using SMB3Explorer.Models.Internal;

namespace SMB3Explorer.Services.ApplicationContext;

public sealed class ApplicationContext : IApplicationContext, INotifyPropertyChanged
{
    private FranchiseSelection? _selectedFranchise;
    private TeamSelection? _selectedTeam;
    private TeamConfigurationSelection? _selectedTeamConfiguration;
    private Roster? _currentRoster;
    private bool _franchiseSeasonsLoading;
    private FranchiseSeason? _mostRecentFranchiseSeason;

    public FranchiseSelection? SelectedFranchise
    {
        get => _selectedFranchise;
        set
        {
            SetField(ref _selectedFranchise, value);
            OnPropertyChanged(nameof(IsFranchiseSelected));
        }
    }

    public TeamSelection? SelectedTeam
    {
        get => _selectedTeam;
        set
        {
            SetField(ref _selectedTeam, value);
            OnPropertyChanged(nameof(IsTeamSelected));
        }
    }

    public TeamConfigurationSelection? SelectedTeamConfiguration
    {
        get => _selectedTeamConfiguration;
        set
        {
            SetField(ref _selectedTeamConfiguration, value);
            OnPropertyChanged(nameof(IsTeamConfigurationSelected));
        }
    }

    public Roster? CurrentRoster
    {
        get => _currentRoster;
        set
        {
            SetField(ref _currentRoster, value);
            OnPropertyChanged(nameof(IsRosterSelected));
        }
    }

    public bool IsFranchiseSelected => SelectedFranchise is not null;
    public bool IsTeamSelected => SelectedTeam is not null;
    public bool IsTeamConfigurationSelected => SelectedTeamConfiguration is not null;
    public bool IsRosterSelected => CurrentRoster is not null;

    public ConcurrentBag<FranchiseSeason> FranchiseSeasons { get; } = new();

    public FranchiseSeason? MostRecentFranchiseSeason
    {
        get => _mostRecentFranchiseSeason;
        set => SetField(ref _mostRecentFranchiseSeason, value);
    }

    public bool FranchiseSeasonsLoading
    {
        get => _franchiseSeasonsLoading;
        set => SetField(ref _franchiseSeasonsLoading, value);
    }

    public SelectedGame SelectedGame { get; set; }

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
