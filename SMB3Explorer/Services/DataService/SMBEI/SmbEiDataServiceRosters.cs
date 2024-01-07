using Microsoft.Data.Sqlite;
using OneOf;
using OneOf.Types;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Serilog;

namespace SMB3Explorer.Services.DataService.SMBEI;

public partial class SmbEiDataService
{
    public async Task<List<TeamSelection>> GetTeams()
    {
        var command = Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.GetTeamsForSmbEi);
        command.CommandText = commandText;
        var reader = await command.ExecuteReaderAsync();

        List<TeamSelection> teams = new();
        while (reader.Read())
        {
            var teamSelection = new TeamSelection()
            {
                id = int.Parse(reader["id"].ToString()!),
                teamName = reader["teamName"].ToString()!
            };
            teams.Add(teamSelection);
        }

        return teams;
    }

    public async Task<List<TeamConfigurationSelection>> GetTeamConfigurations(int teamId)
    {
        var command = Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.GetTeamConfigurationsForTeamSmbEi);
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

    public async Task<Roster> GetRosterForTeam(int teamId, int teamConfigurationId)
    {
        var command = Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.GetRosterForTeamSmbEi);
        command.CommandText = commandText;

        command.Parameters.Add(new SqliteParameter("@TeamId", SqliteType.Integer)
        {
            Value = teamId
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
                id = int.Parse(reader["playerId"].ToString()!),
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
            };

            roster.Players.Add(player);
        }

        return roster;
    }

    public async Task<OneOf<Success, Error<string>>> SaveRosterForTeam(int teamId, int teamConfigurationId,
        Roster roster)
    {
        foreach (Player player in roster.Players)
        {
            var playerSaveResult = await SavePlayerInRoster(teamId, teamConfigurationId, player);
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
        var command = Connection!.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.UpdatePlayerInRosterSmbEi);
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
        command.Parameters.Add(firstNameParam);
        command.Parameters.Add(lastNameParam);

        try
        {
            await command.ExecuteReaderAsync();
            Log.Debug("Saved");
            return new Success();
        }
        catch (SqliteException err)
        {
            return new Error<string>($"Failed to update player with id {player.id.ToString()} in roster {err}");
        }
    }
}
