using OneOf;
using OneOf.Types;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Services.SystemIoWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMB3Explorer.Services.DataService.SMBEI
{
    public interface ISmbEiDataService
    {
        Task<OneOf<List<Smb4LeagueSelection>, Error<string>>> EstablishDbConnection(string filePath);

        Task Disconnect();

        Task<List<TeamSelection>> GetTeams();

        Task<Roster> GetRosterForTeam(int teamId, int teamConfigurationId);

        Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId);

        Task<OneOf<Success, Error<string>>> SaveRosterForTeam(int teamId, int configurationId, Roster roster);
    }
}
