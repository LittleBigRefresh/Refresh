using Refresh.Core.Types.Data;

namespace Refresh.Interfaces.Workers;

public interface IWorker
{
    /// <summary>
    /// How often to perform work, in milliseconds
    /// </summary>
    public int WorkInterval { get; }

    /// <summary>
    /// Instructs the worker to do work.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="dataStore">The server's data store, for workers to use.</param>
    /// <param name="database">A database context, for workers to use.</param>
    /// <returns>True if the worker did work, false if it did not.</returns>
    public void DoWork(DataContext context);
}