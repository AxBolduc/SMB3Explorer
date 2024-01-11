using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OneOf.Types;
using OneOf;
using Serilog;
using SMB3Explorer.Models.Internal;
using SMB3Explorer.Services.SystemIoWrapper;
using SMB3Explorer.Utils;

namespace SMB3Explorer.Services.DataService.RosterDataService.SMB2;

public partial class Smb2RosterDataService
{
    public async Task<OneOf<string, Error<string>>> DecompressSaveGame(string filePath,
        ISystemIoWrapper systemIoWrapper)
    {
        Log.Information("Decompressing save game at {FilePath}", filePath);
        await using var compressedStream = systemIoWrapper.FileOpenRead(filePath);
        if (compressedStream is null)
        {
            Log.Error("Failed to open file, the file stream was null");
            return new Error<string>("Could not open file.");
        }

        await using var zlibStream = systemIoWrapper.GetZlibDecompressionStream(compressedStream);
        using var decompressedStream = new MemoryStream();

        var buffer = new byte[4096];
        int count;

        Log.Debug("Writing decompressed data to memory stream");
        while ((count = zlibStream.Read(buffer, 0, buffer.Length)) != 0)
        {
            decompressedStream.Write(buffer, 0, count);
        }

        decompressedStream.Position = 0;

        var decompressedFileName = $"smb2_save_{DateTime.Now:yyyyMMddHHmmssfff}.sqlite";
        var decompressedFilePath = Path.Combine(Path.GetTempPath(), decompressedFileName);

        CurrentFilePath = decompressedFilePath;

        Log.Debug("Writing decompressed data to file at {FilePath}", decompressedFilePath);
        await using var fileStream = systemIoWrapper.FileCreateStream(decompressedFilePath);
        await decompressedStream.CopyToAsync(fileStream);

        Log.Information("Finished decompressing save game");
        return decompressedFilePath;
    }

    public async Task<OneOf<List<Smb4LeagueSelection>, Error<string>>> EstablishDbConnection(string filePath)
    {
        Log.Information("Establishing database connection to {FilePath}", filePath);
        var decompressedFilePath = filePath;

        var decompressResult = await DecompressSaveGame(filePath, _systemIoWrapper);
        if (decompressResult.TryPickT1(out var error, out decompressedFilePath))
        {
            return new Error<string>(error.Value);
        }

        CurrentFilePath = decompressedFilePath;

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = CurrentFilePath,
            Mode = SqliteOpenMode.ReadWrite,
            Cache = SqliteCacheMode.Shared
        };

        Log.Debug("Opening connection to database at {FilePath}", CurrentFilePath);

        _applicationContext.Connection = new SqliteConnection(connectionStringBuilder.ToString());
        _applicationContext.Connection.Open();

        Log.Debug("Testing connection to database");
        var command = _applicationContext.Connection.CreateCommand();
        var commandText = SqlRunner.GetSqlCommand(SqlFile.DatabaseTables);
        command.CommandText = commandText;
        var reader = await command.ExecuteReaderAsync();

        List<string> tableNames = new();
        while (reader.Read())
        {
            var tableName = reader.GetString(0);
            tableNames.Add(tableName);
        }

        if (!tableNames.Contains("t_baseball_players") || !tableNames.Contains("t_teams"))
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
        if (_applicationContext.Connection is not null)
        {
            try
            {
                Log.Debug("Committing all transactions");
                var command = _applicationContext.Connection.CreateCommand();
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
                var conn = _applicationContext.Connection;
                _applicationContext.Connection = null;

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
