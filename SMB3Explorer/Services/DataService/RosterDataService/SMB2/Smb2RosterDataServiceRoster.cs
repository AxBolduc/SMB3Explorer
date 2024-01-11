using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OneOf.Types;
using OneOf;
using Serilog;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Utils;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMB2;

public partial class Smb2RosterDataService
{
    public async Task<List<TeamSelection>> GetTeams()
    {
        if (!_applicationContext.IsConnected)
        {
            var errMsg = "Could not get roster for team/configuration, database not connected";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        var command = _applicationContext.Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(Smb2SqlFile.GetTeams);
        command.CommandText = commandText;

        var reader = await command.ExecuteReaderAsync();

       List<TeamSelection> teams = new();
        while (reader.Read())
        {
            var teamSelection = new TeamSelection()
            {
                id = reader.GetGuid(0).ToString(),
                teamName = reader.GetString(1)
            };
            teams.Add(teamSelection);
        }

        return teams;
    }

    public async Task<Roster> GetRosterForTeam(OneOf<int, Guid> teamId, int? teamConfigurationId)
    {
        if (teamId.TryPickT0(out int parsedTeamId, out Guid teamGuid))
        {
            var errMsg = "Invalid team id type for SMB2 got int expected Guid";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        if (!_applicationContext.IsConnected)
        {
            var errMsg = "Could not get roster for team/configuration, database not connected";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        var command = _applicationContext.Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(Smb2SqlFile.GetRosterForTeam);
        command.CommandText = commandText;

        command.Parameters.Add(new SqliteParameter("@TeamGUID", SqliteType.Blob)
        {
            Value = teamGuid
        });

        var reader = await command.ExecuteReaderAsync();

        Roster roster = new Roster()
        {
            TeamName = _applicationContext.SelectedTeam!.teamName
        };

        while (reader.Read())
        {
            var player = new Player()
            {
                id = reader.GetGuid(0).ToString()!,
                FirstName = reader["firstName"].ToString()!,
                LastName = reader["lastName"].ToString()!,
                PrimaryPosition = reader["primaryPosition"].ToString()!,
                ContactRating = float.Parse(reader["contactRating"].ToString()!),
                PowerRating = float.Parse(reader["powerRating"].ToString()!),
                SpeedRating = float.Parse(reader["speedRating"].ToString()!),
                ArmRating = float.Parse(reader["armRating"].ToString()!),
                FieldingRating = float.Parse(reader["fieldingRating"].ToString()!),
                VelocityRating = float.Parse(reader["velocityRating"].ToString()!),
                JunkRating = float.Parse(reader["junkRating"].ToString()!),
                AccuracyRating = float.Parse(reader["accuracyRating"].ToString()!),
            };

            roster.Players.Add(player);
        }

        return roster;
    }

    public Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId)
    {
        List<TeamConfigurationSelection> emptyList = new List<TeamConfigurationSelection>();

        return Task.FromResult(emptyList);
    }

    public Task<OneOf<Success, Error<string>>> SaveRosterForTeam(OneOf<int, Guid> teamId, int? configurationId,
        Roster roster)
    {
        throw new NotImplementedException();
    }
}
