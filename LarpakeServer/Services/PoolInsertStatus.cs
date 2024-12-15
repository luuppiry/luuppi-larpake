namespace LarpakeServer.Services;

public enum PoolInsertStatus
{
    /// <summary>
    /// Successfully added client to pool.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Failed because invalid data was provided.
    /// </summary>
    Failed,

    /// <summary>
    /// Pool is full, client was not added. Try again later.
    /// </summary>
    Full
}
