using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OneOf;
using OneOf.Types;
using Serilog;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Utils;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMBEI;

public partial class SmbEiDataService
{
    public async Task<List<TeamSelection>> GetTeams()
    {
        if (!_applicationContext.IsConnected)
        {
            var errMsg = "Could not get teams, database not connected";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        var command = _applicationContext.Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SmbEiSqlFile.GetTeamsForSmbEi);
        command.CommandText = commandText;
        var reader = await command.ExecuteReaderAsync();

        List<TeamSelection> teams = new();
        while (reader.Read())
        {
            var teamSelection = new TeamSelection()
            {
                id = reader["id"].ToString()!,
                teamName = reader["teamName"].ToString()!
            };
            teams.Add(teamSelection);
        }

        return teams;
    }

    public async Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId)
    {
        if (!_applicationContext.IsConnected)
        {
            var errMsg = "Could not get team configurations, database not connected";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        var command = _applicationContext.Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SmbEiSqlFile.GetTeamConfigurationsForTeamSmbEi);
        command.CommandText = commandText;

        command.Parameters.Add(new SqliteParameter("@TeamId", SqliteType.Integer)
        {
            Value = teamId
        });

        var reader = await command.ExecuteReaderAsync();

        List<TeamConfigurationSelection> teamConfigurations = new();
        while (reader.Read())
        {
            var teamConfigurationSelection = new TeamConfigurationSelection()
            {
                id = int.Parse(reader["id"].ToString()!),
                name = reader["name"].ToString()!
            };
            teamConfigurations.Add(teamConfigurationSelection);
        }

        return teamConfigurations;
    }

    public async Task<Roster> GetRosterForTeam(OneOf<int, Guid> teamId, int? teamConfigurationId)
    {
        if (teamId.TryPickT1(out Guid teamGuid, out int parsedTeamId))
        {
            var errMsg = "Invalid team id type for SMB: EI got Guid expected int";
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
        var commandText = SqlRunner.GetSqlCommand(SmbEiSqlFile.GetRosterForTeamSmbEi);
        command.CommandText = commandText;

        command.Parameters.Add(new SqliteParameter("@TeamId", SqliteType.Integer)
        {
            Value = parsedTeamId
        });

        command.Parameters.Add(new SqliteParameter("@TeamConfigurationId", SqliteType.Integer)
        {
            Value = teamConfigurationId
        });

        var reader = await command.ExecuteReaderAsync();

        Roster roster = new();
        while (reader.Read())
        {
            if (roster.TeamName.Equals(string.Empty))
            {
                roster.TeamName = reader["teamName"].ToString()!;
            }

            var player = new Player()
            {
                id = reader["playerId"].ToString()!,
                FirstName = reader["firstName"].ToString()!,
                LastName = reader["lastName"].ToString()!,
                PrimaryPosition = reader["primaryPosition"].ToString()!,
                SecondaryPosition = reader["secondaryPosition"].ToString()!,
                ContactRating = float.Parse(reader["contactRating"].ToString()!),
                PowerRating = float.Parse(reader["powerRating"].ToString()!),
                MojoRating = float.Parse(reader["mojoRating"].ToString()!),
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

    public async Task<OneOf<Success, Error<string>>> SaveRosterForTeam(OneOf<int, Guid> teamId,
        int? teamConfigurationId,
        Roster roster)
    {
        if (teamId.TryPickT1(out Guid teamGuid, out int parsedTeamId))
        {
            var errMsg = "Invalid team id type for SMB: EI got Guid expected int";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        if (teamConfigurationId == null)
        {
            var errMsg = "Team Configuration Id cannot be null when saving SMB: EI file";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        foreach (Player player in roster.Players)
        {
            var playerSaveResult = await SavePlayerInRoster(parsedTeamId, teamConfigurationId.Value, player);
            if (playerSaveResult.TryPickT1(out var error, out var success))
            {
                return new Error<string>(error.Value);
            }
        }

        return new Success();
    }

    private async Task<OneOf<Success, Error<string>>> SavePlayerInRoster(int teamId, int teamConfigurationId,
        Player player)
    {
        if (!_applicationContext.IsConnected)
        {
            var errMsg = "Could not save player in roster, database not connected";
            Log.Debug(errMsg);
            throw new Exception(errMsg);
        }

        var command = _applicationContext.Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SmbEiSqlFile.UpdatePlayerInRosterSmbEi);
        command.CommandText = commandText;

        var teamIdParam = new SqliteParameter("@TeamId", SqliteType.Integer) { Value = teamId };
        var teamConfigurationIdParam = new SqliteParameter("@TeamConfigurationId", SqliteType.Integer)
            { Value = teamConfigurationId };
        var playerIdParam = new SqliteParameter("@PlayerId", SqliteType.Integer) { Value = player.id };
        var contactParam = new SqliteParameter("@Contact", SqliteType.Real) { Value = player.ContactRating };
        var powerParam = new SqliteParameter("@Power", SqliteType.Real) { Value = player.PowerRating };
        var speedParam = new SqliteParameter("@Speed", SqliteType.Real) { Value = player.SpeedRating };
        var armParam = new SqliteParameter("@Arm", SqliteType.Real) { Value = player.ArmRating };
        var fieldingParam = new SqliteParameter("@Fielding", SqliteType.Real) { Value = player.FieldingRating };
        var mojoParam = new SqliteParameter("@Mojo", SqliteType.Real) { Value = player.MojoRating };
        var accuracyParam = new SqliteParameter("@Accuracy", SqliteType.Real) { Value = player.AccuracyRating };
        var junkParam = new SqliteParameter("@Junk", SqliteType.Real) { Value = player.JunkRating };
        var velocityParam = new SqliteParameter("@Velocity", SqliteType.Real) { Value = player.VelocityRating };


        var firstNameParam = new SqliteParameter("@FirstName", SqliteType.Text) { Value = player.FirstName };
        var lastNameParam = new SqliteParameter("@LastName", SqliteType.Text) { Value = player.LastName };

        command.Parameters.Add(teamIdParam);
        command.Parameters.Add(teamConfigurationIdParam);
        command.Parameters.Add(playerIdParam);
        command.Parameters.Add(contactParam);
        command.Parameters.Add(powerParam);
        command.Parameters.Add(speedParam);
        command.Parameters.Add(armParam);
        command.Parameters.Add(fieldingParam);
        command.Parameters.Add(mojoParam);
        command.Parameters.Add(accuracyParam);
        command.Parameters.Add(junkParam);
        command.Parameters.Add(velocityParam);


        command.Parameters.Add(firstNameParam);
        command.Parameters.Add(lastNameParam);

        try
        {
            await command.ExecuteReaderAsync();
            return new Success();
        }
        catch (SqliteException err)
        {
            return new Error<string>($"Failed to update player with id {player.id.ToString()} in roster {err}");
        }
    }
}
