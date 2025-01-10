using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface ILarpakeDatabase
{
    Task<int> DeleteLarpake(long larpakeId);
    Task<int> DeleteSection(long sectionId);
    Task<Larpake?> GetLarpake(long larpakeId);
    Task<LarpakeSection[]> GetLarpakeSections(long larpakeId);
    Task<Larpake[]> GetLarpakkeet(QueryOptions options);
    Task<LarpakeSection?> GetSection(long sectionId);
    Task<LarpakeSection[]> GetSections(QueryOptions options);
    Task<Result<long>> InsertLarpake(Larpake record);
    Task<Result<long>> InsertSection(LarpakeSection section);
    Task<Result<int>> UpdateLarpake(Larpake record);
    Task<Result<int>> UpdateSection(LarpakeSection record);
}