using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Services;

public interface IClientPool
{
    /// <summary>
    /// Check if pool is currently full.
    /// </summary>
    bool IsFull { get; }

    /// <summary>
    /// Maximum allowed number of clients in the pool.
    /// </summary>
    int MaxSize { get; init; }


    /// <summary>
    /// Adds a client to the pool. 
    /// </summary>
    /// <param name="userId">User id which client subscribes.</param>
    /// <param name="client"><see cref="HttpResponse"/> representing request client.</param>
    /// <returns><see cref="PoolInsertStatus"/> representing add status.</returns>
    PoolInsertStatus Add(Guid userId, HttpResponse client);

    /// <summary>
    /// Removes a client from the pool.
    /// </summary>
    /// <param name="userId">Same as used in <see cref="Add(Guid, HttpResponse)"/></param>
    /// <param name="client">Same as used in <see cref="Add(Guid, HttpResponse)"/></param>
    /// <returns><see langword="true"/> if successfully removed, otherwise <see langword="false"/>.</returns>
    bool Remove(Guid userId, HttpResponse client);

    /// <summary>
    /// Tries to find a client in the pool.
    /// </summary>
    /// <param name="userId">User id which is wanted to retrieve.</param>
    /// <returns>Client with given id if success, otherwise <see langword="null"/>.</returns>
     bool TryFind(Guid? userId, [NotNullWhen(true)] out HttpResponse[]? client);

    /// <summary>
    /// Gets all clients in the pool.
    /// </summary>
    /// <returns>All <see cref="HttpResponse"/>s in the pool.</returns>
    HttpResponse[] GetAll();



}
