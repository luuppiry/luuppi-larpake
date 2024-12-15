using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Services;

public interface IClientPool
{
    /// <summary>
    /// Adds a client to the pool. 
    /// </summary>
    /// <param name="clientId">Client identifier in pool.</param>
    /// <param name="client"><see cref="HttpResponse"/> representing request client.</param>
    /// <returns><see cref="PoolInsertStatus"/> representing add status.</returns>
    PoolInsertStatus Add(Guid clientId, HttpResponse client);


    /// <summary>
    /// Removes a client from the pool.
    /// </summary>
    /// <param name="clientId">Guid used when calling <see cref="Add(HttpResponse, Guid?)"/>.</param>
    /// <returns><see langword="true"/> if successfully removed, otherwise <see langword="false"/>.</returns>
    bool Remove(Guid clientId, HttpResponse client);

    /// <summary>
    /// Tries to find a client in the pool.
    /// </summary>
    /// <param name="clientId">Guid used when calling <see cref="Add(HttpResponse, Guid?)"/></param>
    /// <returns>Client with given id if success, otherwise <see langword="null"/>.</returns>
     bool TryFind(Guid? clientId, [NotNullWhen(true)] out HttpResponse? client);

    /// <summary>
    /// Gets all clients in the pool.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="HttpResponse"/></returns>
    IEnumerable<HttpResponse> GetAll();

    /// <summary>
    /// Check if pool is currently full.
    /// </summary>
    bool IsFull { get; }

    /// <summary>
    /// Maximum allowed number of clients in the pool.
    /// Existing clients are not kicked during resize.
    /// </summary>
    int MaxSize { get; init; }

}
