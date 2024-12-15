using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Services.Implementations;


/// <summary>
/// Keep track of connection clients using in-memory dictionary.
/// [Note] 
/// Logs from this class might not be in right order.
/// If many clients are added/removed at once correct order is 
/// not guaranteed, but the content should be still correct.
/// </summary>
public class InMemoryClientPool : IClientPool
{
    private readonly ILogger<IClientPool> _logger;

    public InMemoryClientPool(ILogger<IClientPool> logger, ClientPoolConfiguration config)
    {
        MaxSize = config.MaxSize;
        _logger = logger;
        
    }


    public bool IsFull => Size >= MaxSize;

    public int Size => Clients.Count;

    public int MaxSize { get; init; } = 1000;

    private OverwritingConcurrentDictionary<Guid, HttpResponse> Clients { get; } = [];

    public PoolInsertStatus Add(Guid clientId, HttpResponse client)
    {
        if (IsFull)
        {
            _logger.LogInformation("Pool is full, cannot add client {id}.", clientId);
            return PoolInsertStatus.Full;
        }
        if (client is null)
        {
            _logger.LogWarning("Cannot add null HttpResponse (client) to the pool.");
            return PoolInsertStatus.Failed;
        }
        if (clientId == Guid.Empty)
        {
            _logger.LogWarning("Cannot add client with empty guid to the pool");
            return PoolInsertStatus.Failed;
        }

        // Add client to pool, overwrite if already exists
        bool wasNewValue = Clients.AddOrOverwrite(clientId, client, out int size);
        if (wasNewValue)
        {
            _logger.LogInformation("Added client {clientId} with pool size of {size}.",
              clientId, size);
        }
        else
        {
            _logger.LogInformation("Overwrote client {id} with new one.", clientId);
        }
        return PoolInsertStatus.Success;
    }


    public bool Remove(Guid clientId, HttpResponse client)
    {
        // Remove if not overwritten
        if (Clients.TryRemove(new(clientId, client), out int size))
        {
            _logger.LogInformation("Remove client {id}, pool size {size}.", clientId, size);
            return true;
        }
        _logger.LogInformation("Client {clientId} already overwritten.", clientId);
        return true;
    }

    public bool TryFind(Guid? clientId, [NotNullWhen(true)] out HttpResponse? client)
    {
        if (clientId is null)
        {
            client = null;
            return false;
        }
        return Clients.TryGetValue(clientId.Value, out client);
    }

    public IEnumerable<HttpResponse> GetAll()
    {
        // Copy for thread safety
        return [.. Clients.Values];
    }



    class OverwritingConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        readonly Lock _lock = new();

        public new int Count
        {
            get
            {
                lock (_lock)
                {
                    return base.Count;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns>True if added new value, false if overwrote existing value.</returns>
        public bool AddOrOverwrite(TKey key, TValue value, out int size)
        {
            lock (_lock)
            {
                // Overwrite if key exists
                bool keyExists = ContainsKey(key);
                this[key] = value;
                size = base.Count;
                return keyExists is false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="size"></param>
        /// <returns>True if actually removed value, false if value had been overwritten.</returns>
        public bool TryRemove(KeyValuePair<TKey, TValue> pair, out int size)
        {
            lock (_lock)
            {
                // Remove and decrement size if not overwritten
                size = base.Count;
                if (TryRemove(pair))
                {
                    size--;
                    return true;
                }
                return false;
            }
        }

    }
}
