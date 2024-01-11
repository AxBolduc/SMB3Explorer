using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using SMB3Explorer.Enums;
using SMB3Explorer.Models.Internal;

namespace SMB3Explorer.Services.ApplicationContext;

public interface IApplicationContext
{
    FranchiseSelection? SelectedFranchise { get; set; }
    TeamSelection? SelectedTeam { get; set; }
    TeamConfigurationSelection? SelectedTeamConfiguration { get; set; }
    Roster? CurrentRoster { get; set; }
    bool IsFranchiseSelected { get; }
    bool IsTeamSelected { get; }
    ConcurrentBag<FranchiseSeason> FranchiseSeasons { get; }
    FranchiseSeason? MostRecentFranchiseSeason { get; set; }
    bool FranchiseSeasonsLoading { get; set; }
    SelectedGame SelectedGame { get; set; }
    event PropertyChangedEventHandler? PropertyChanged;
    SqliteConnection? Connection { get; set; }
    bool IsConnected { get; }
}
