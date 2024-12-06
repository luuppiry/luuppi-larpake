namespace LarpakeServer.Data;

public interface ISqliteDependencyTable<T>
{
    Task CreateTables();
}
