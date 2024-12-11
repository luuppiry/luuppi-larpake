using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IAttendanceDatabase
{
    Task<Attendance[]> Get(AttendanceQueryOptions options);
    Task<Result<int>> InsertUncompleted(Attendance attendance);
    Task<Result<AttendedCreated>> Complete(AttendanceCompletionMetadata completion);
    Task<Result<int>> UnComplete(Guid userId, long eventId);
}