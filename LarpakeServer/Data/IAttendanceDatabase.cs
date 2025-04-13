using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;

namespace LarpakeServer.Data;

public interface IAttendanceDatabase
{
    Task<Attendance[]> Get(AttendanceQueryOptions options);
    Task<Result<AttendanceKey>> GetAttendanceKey(Attendance attendance);
    Task<Result<AttendedCreated>> CompletedKeyed(KeyedCompletionMetadata completion);
    Task<Result<AttendedCreated>> Complete(CompletionMetadata completion);
    Task<Result<int>> Uncomplete(Guid userId, long eventId);
    Task<int> Clean();
    Task<Attendance?> GetByKey(string key);
}