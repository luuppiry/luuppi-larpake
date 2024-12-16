using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Services.Implementations;


/// <summary>
/// Keep track of connection clients using in-memory dictionary.
/// This class is thread safe.
/// </summary>
public class InMemoryClientPool : IClientPool
{
    readonly ILogger<IClientPool> _logger;
    readonly Dictionary<Guid, List<HttpResponse>> _clients = [];
    readonly Lock _lock = new();

    public InMemoryClientPool(ILogger<IClientPool> logger, ClientPoolConfiguration config)
    {
        MaxSize = config.MaxSize;
        _logger = logger;
        
    }


    public bool IsFull
    {
        get
        {
            lock (_lock)
            {
                return Size >= MaxSize;
            }
        }
    }
    public int Size
    {
        get
        {
            lock (_lock)
            {
                return field;
            }
        }
        private set
        {
            lock (_lock)
            {
                field = value;
            }
        }
    }
    public int MaxSize { get; init; } = 1000;


    public PoolInsertStatus Add(Guid userId, HttpResponse client)
    {
        if (IsFull)
        {
            _logger.LogInformation("Pool is full, cannot add client {id}.", userId);
            return PoolInsertStatus.Full;
        }
        if (client is null)
        {
            _logger.LogWarning("Cannot add null HttpResponse (client) to the pool.");
            return PoolInsertStatus.Failed;
        }
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Cannot add client with empty guid to the pool");
            return PoolInsertStatus.Failed;
        }

        // Add client to pool, overwrite if already exists
        lock (_lock)
        {
            if (_clients.TryGetValue(userId, out var clients))
            {
                clients.Add(client);      
            }
            else
            {
                _clients[userId] = [client];
            }
            Size++;
            _logger.LogInformation("Added client for user {clientId}, pool size {size}.",
                    userId, Size);
        }
        return PoolInsertStatus.Success;
    }

    public bool Remove(Guid userId, HttpResponse client)
    {
        // Remove if not overwritten
        lock (_lock)
        {
            // Remove is 
            if (_clients.TryGetValue(userId, out var clients))
            {
                if (clients.Remove(client))
                {
                    Size--;
                    _logger.LogInformation("Removed client for user {id}, pool size {size}.", 
                        userId, Size);
                    return true;
                }
            }
            _logger.LogWarning("Cannot remove client for user {id}, not found.", userId);
            return false;
        }
    }

    public bool TryFind(Guid? userId, [NotNullWhen(true)] out HttpResponse[]? clients)
    {
        clients = null;
        if (userId is null)
        {
            return false;
        }
        lock (_lock)
        {
            bool isFound = _clients.TryGetValue(userId.Value, out var values);
            clients = values?.ToArray();
            return isFound;
        }
    }

    public HttpResponse[] GetAll()
    {
        // Copy for thread safety
        lock (_lock)
        {
            return _clients.Values
                .SelectMany(x => x)
                .ToArray();
        }
    }
}
