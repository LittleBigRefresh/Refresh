using Microsoft.EntityFrameworkCore.Storage;
using Refresh.Common;
using Refresh.Database.Models.Workers;
using Refresh.Workers.State;

namespace Refresh.Workers;

public abstract class MigrationJob<TEntity> : WorkerJob, IJobStoresState where TEntity : class
{
    public virtual string JobId => this.GetType().Name;
    public object JobState { get; set; } = null!;
    public Type JobStateType => typeof(MigrationJobState);
    public WorkerClass JobClass => WorkerClass.Refresh;

    public MigrationJobState? MigrationJobState => JobState as MigrationJobState;

    protected virtual int BatchCount => 1_000;

    public override bool CanExecute()
    {
        return this.MigrationJobState == null || !this.MigrationJobState.StateInitialized || !this.MigrationJobState.Complete;
    }

    public override void ExecuteJob(WorkContext context)
    {
        IQueryable<TEntity> query = context.Database.Set<TEntity>();
        query = this.SortAndFilter(query);

        MigrationJobState state = this.MigrationJobState!;

        if (!state.StateInitialized)
        {
            state.Total = query.Count();
            state.StateInitialized = true;
        }

        query = query.Skip(state.Processed).Take(this.BatchCount);

        using IDbContextTransaction transaction = context.Database.Database.BeginTransaction();

        TEntity[] batch = query.ToArray();
        
        int entitiesLeftCount = Migrate(context, batch);
        context.Database.SaveChanges();
        transaction.Commit();

        state.Processed += entitiesLeftCount;
        state.Total = state.Total - batch.Length + entitiesLeftCount;
        context.Logger.LogInfo(RefreshContext.Database, $"{this.JobId} migrated {batch.Length} objects ({state.Processed}/{state.Total}, complete: {state.Complete})");
    }
    
    protected abstract IQueryable<TEntity> SortAndFilter(IQueryable<TEntity> query);

    /// <returns>
    /// The number of entities in the batch, minus the number of entities removed from DB during this migration.
    /// E.g. if 1000 items are in the given batch, and 5 got removed from DB during migration, 
    /// the returned number will be 1000 - 5 = 995
    /// </returns>
    protected abstract int Migrate(WorkContext context, TEntity[] batch);
}