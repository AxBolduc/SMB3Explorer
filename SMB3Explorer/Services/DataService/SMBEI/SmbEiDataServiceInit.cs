using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OneOf;
using OneOf.Types;
using Serilog;
using SMB3Explorer.Enums;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Services.SystemIoWrapper;
using SMB3Explorer.Utils;

namespace SMB3Explorer.Services.DataService.SMBEI;

public partial class SmbEiDataService
{
    public async Task<OneOf<List<Smb4LeagueSelection>, Error<string>>> EstablishDbConnection(string filePath)
    {
        Log.Information( "Establishing database connection to {FilePath}", filePath);
        var CurrentFilePath = filePath;

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = CurrentFilePath,
            Mode = SqliteOpenMode.ReadWrite,
            Cache = SqliteCacheMode.Shared
        };

        Log.Debug("Opening connection to database at {FilePath}", CurrentFilePath);

        Connection = new SqliteConnection(connectionStringBuilder.ToString());
        Connection.Open();

        Log.Debug("Testing connection to database");
        var command = Connection.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.DatabaseTables);
        command.CommandText = commandText;
        var reader = await command.ExecuteReaderAsync();

        List<string> tableNames = new();
        while (reader.Read())
        {
            var tableName = reader.GetString(0);
            tableNames.Add(tableName);
        }

        if (!tableNames.Contains("baseball_players") || !tableNames.Contains("teams"))
        {
            Log.Error("Invalid save file, missing expected tables");
            return new Error<string>("Invalid save file, missing expected tables");
        }

        Log.Information("Successfully established database connection");
        return new List<Smb4LeagueSelection>();
    }

    public async Task Disconnect()
    {
        Log.Information("Disconnecting from database");
        if (Connection is not null)
        {
            try
            {
                Log.Debug("Committing all transactions");
                var command = Connection.CreateCommand();
                command.CommandText = "COMMIT";
                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException)
            {
                Log.Debug("No transactions to commit");
                // no-op, may throw if there are no transactions to commit
            }
            finally
            {
                Log.Debug("Closing and disposing of connection");
                // Remove reference to connection object
                var conn = Connection;
                Connection = null;

                // Close and dispose of the connection object
                conn.Close();
                conn.Dispose();

                // Clear all connections from the connection pool
                SqliteConnection.ClearAllPools();
            }
        }

        CurrentFilePath = string.Empty;
        Log.Debug("Connection to database closed");
    }
}
