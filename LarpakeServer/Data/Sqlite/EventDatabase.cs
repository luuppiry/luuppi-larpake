﻿using LarpakeServer.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using System.Text;

namespace LarpakeServer.Data.Sqlite;

public class EventDatabase(SqliteConnectionString connectionString)
    : SqliteDbBase(connectionString), IEventDatabase
{
    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteScalarAsync($"""
            CREATE TABLE IF NOT EXISTS Events (
                {nameof(Event.Id)} INTEGER,
                {nameof(Event.Title)} TEXT,
                {nameof(Event.Body)} TEXT,
                {nameof(Event.StartTimeUtc)} DATETIME NOT NULL,
                {nameof(Event.EndTimeUtc)} DATETIME DEFAULT NULL,
                {nameof(Event.Location)} TEXT NOT NULL,
                {nameof(Event.ImageUrl)} TEXT NOT NULL,
                {nameof(Event.LuuppiRefId)} INTEGER,
                {nameof(Event.WebsiteUrl)} TEXT NOT NULL,
                {nameof(Event.CreatedBy)} TEXT,
                {nameof(Event.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Event.LastModifiedBy)} TEXT,
                {nameof(Event.LastModifiedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Event.TimeDeletedUtc)} DATETIME,
                PRIMARY KEY({nameof(Event.Id)})
            )
            """);
    }

    public async Task<Event[]> Get(EventQueryOptions options)
    {
        StringBuilder query = new();

        query.AppendLine($"""
            SELECT * FROM Events
            WHERE {nameof(Event.TimeDeletedUtc)} IS NULL
            """);

        // Add filters
        if (options.AfterUtc is not null)
        {
            // Start time is after given time
            query.AppendLine($"""
                AND {nameof(Event.StartTimeUtc)} >= @{nameof(options.AfterUtc)} 
                """);
        }
        if (options.BeforeUtc is not null)
        {
            // Start time is before given time
            query.AppendLine($"""
                AND {nameof(Event.StartTimeUtc)} <= @{nameof(options.BeforeUtc)} 
                """);
        }
        if (options.Title is not null)
        {
            // Title like given title (can include any characters around)
            query.AppendLine($"""
                AND {nameof(Event.Title)} LIKE %@{nameof(options.Title)}% 
                """);
        }

        query.Append($"""
            ORDER BY {nameof(Event.StartTimeUtc)} ASC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = await GetConnection();
        var queryStr = query.ToString();

        var events = await connection.QueryAsync<Event>(query.ToString(), options);
        return events.ToArray();
    }

    public async Task<Event?> Get(long id)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Event>($"""
            SELECT * FROM Events WHERE {nameof(Event.Id)} = @id LIMIT 1;
            """, new { id });
    }

    public async Task<Result<long>> Insert(Event record)
    {
        try
        {
            using var connection = await GetConnection();
            return await connection.ExecuteScalarAsync<long>($"""
                INSERT INTO Events (
                    {nameof(Event.Title)}, 
                    {nameof(Event.Body)}, 
                    {nameof(Event.StartTimeUtc)},
                    {nameof(Event.EndTimeUtc)}, 
                    {nameof(Event.Location)}, 
                    {nameof(Event.LuuppiRefId)},
                    {nameof(Event.WebsiteUrl)}, 
                    {nameof(Event.ImageUrl)}, 
                    {nameof(Event.CreatedBy)},
                    {nameof(Event.LastModifiedBy)},
                    {nameof(Event.TimeDeletedUtc)}
                ) 
                Values (
                    @{nameof(Event.Title)},
                    @{nameof(Event.Body)},
                    @{nameof(Event.StartTimeUtc)},
                    @{nameof(Event.EndTimeUtc)},
                    @{nameof(Event.Location)},
                    @{nameof(Event.LuuppiRefId)},
                    @{nameof(Event.WebsiteUrl)},
                    @{nameof(Event.ImageUrl)},
                    @{nameof(Event.CreatedBy)},
                    @{nameof(Event.LastModifiedBy)},
                    NULL
                );
                last_insert_rowid();
                """, record);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 19)
        {
            return new Error(500, "Cannot create new primary key.");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 1)
        {
            return new Error(500, "Server failed to process request.");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 1)
        {
            return new Error(404, "User id not found.");
        }
        catch (Exception)
        {
            throw;
        }

    }
}
