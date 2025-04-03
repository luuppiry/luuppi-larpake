using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface ILarpakeDatabase
{
    Task<int> DeleteLarpake(long larpakeId);
    Task<int> DeleteSection(long sectionId);
    Task<Larpake[]> GetLarpakkeet(LarpakeQueryOptions options);
    Task<Larpake?> GetLarpake(long larpakeId);
    Task<LarpakeSection?> GetSection(long sectionId);
    Task<LarpakeSection?> GetSectionsByIdAndUser(long sectionId, Guid userId);

    Task<LarpakeSection[]> GetSections(long larpakeId, QueryOptions options);
    Task<Result<long>> InsertLarpake(Larpake record);
    Task<Result<long>> InsertSection(LarpakeSection section);
    Task<Result<int>> UpdateLarpake(Larpake record);
    Task<Result<int>> UpdateSection(LarpakeSection record);
}