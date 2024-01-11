using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;
using SMB3Explorer.Enums;
using SMB3Explorer.Models.Internal;

namespace SMB3Explorer.Services.DataService.RosterDataService;

public interface IRosterDataService
{
    SelectedGame GameType { get; }

    Task<OneOf<List<Smb4LeagueSelection>, Error<string>>> EstablishDbConnection(string filePath);

    Task Disconnect();

    Task<List<TeamSelection>> GetTeams();

    Task<Roster> GetRosterForTeam(OneOf<int, Guid> teamId, int? teamConfigurationId);

    Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId);

    Task<OneOf<Success, Error<string>>> SaveRosterForTeam(OneOf<int, Guid> teamId, int? configurationId, Roster roster);
}
