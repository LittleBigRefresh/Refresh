using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Workers;

public interface IWorker
{
    /// <summary>
    /// How often to perform work, in milliseconds
    /// </summary>
    public int WorkInterval { get; }
    
    /// <summary>
    /// Instructs the worker to do work.
    /// </summary>
    /// <param name="logger">A Refresh logger, able to be operated by the worker.</param>
    /// <param name="dataStore">The server's data store, for workers to use.</param>
    /// <param name="database">A database context, for workers to use.</param>
    /// <returns>True if the worker did work, false if it did not.</returns>
    public bool DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database);
}