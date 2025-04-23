using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class SignatureDatabase(NpgsqlConnectionString connectionString, ILogger<SignatureDatabase> logger) 
    : PostgresDb(connectionString, logger), ISignatureDatabase
{
    public async Task<Signature[]> Get(SignatureQueryOptions options)
    {
        SelectQuery query = new();
        query.AppendLine($"""
            SELECT 
                id,
                user_id,
                path_data_json,
                height,
                width,
                line_width,
                stroke_style,
                line_cap,
                created_at
            FROM signatures
            """);

        query.IfNotNull(options.UserId).AppendConditionLine($"""
            user_id = @{nameof(options.UserId)}
            """);

        query.IfNotNull(options.SignatureIds).AppendConditionLine($"""
            id = ANY(@{nameof(options.SignatureIds)})
            """);

        query.AppendLine($"""
            ORDER BY user_id ASC, created_at DESC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)}
            """);

        string parsed = query.ToString();
        using var connection = GetConnection();
        var records = await connection.QueryAsync<Signature>(parsed, options);
        return records.ToArray();
    }

    public async Task<Signature?> Get(Guid id)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Signature>($"""
            SELECT 
                id,
                user_id,
                path_data_json,
                height,
                width,
                line_width,
                stroke_style,
                line_cap,
                created_at
            FROM signatures
            WHERE id = @{nameof(id)}
            LIMIT 1;
            """, new { id });
    }

    public async Task<Result<Guid>> Insert(Signature record)
    {
        record.Id = Guid.CreateVersion7();
        try
        {
            using var connection = GetConnection();
            await connection.ExecuteAsync($"""
            INSERT INTO signatures (
                id,
                user_id,
                path_data_json,
                height,
                width,
                line_width,
                stroke_style,
                line_cap
            ) 
            VALUES (
                @{nameof(record.Id)},
                @{nameof(record.UserId)},
                @{nameof(record.PathDataJson)},
                @{nameof(record.Height)},
                @{nameof(record.Width)},
                @{nameof(record.LineWidth)},
                @{nameof(record.StrokeStyle)},
                @{nameof(record.LineCap)}
            );
            """, record);

            return record.Id;
        }
        catch (NpgsqlException ex)
        {
            // TODO: Handle exception
            logger.LogError("Failed to insert signature, {msg}", ex.Message);
            throw;
        }
    }

    public async Task<Result<int>> Delete(Guid id)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM signatures WHERE id = @{nameof(id)};
            """, new { id });
    }

   
}
