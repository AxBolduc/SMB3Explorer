using System.Collections.Generic;
using System.Threading.Tasks;
using OneOf.Types;
using OneOf;
using SMB3Explorer.Models.Internal;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMB2;

public partial class Smb2RosterDataService
{
    public Task<List<TeamSelection>> GetTeams()
    {
        throw new System.NotImplementedException();
    }

    public Task<Roster> GetRosterForTeam(int teamId, int teamConfigurationId)
    {
        throw new System.NotImplementedException();
    }

    public Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId)
    {
        throw new System.NotImplementedException();
    }

    public Task<OneOf<Success, Error<string>>> SaveRosterForTeam(int teamId, int configurationId, Roster roster)
    {
        throw new System.NotImplementedException();
    }
}
