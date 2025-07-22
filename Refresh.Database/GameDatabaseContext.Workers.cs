using Refresh.Database.Models.Workers;

namespace Refresh.Database;

public partial class GameDatabaseContext // Workers
{
    public int CreateWorker()
    {
        DateTimeOffset now = this._time.Now;
        
        // remove workers with this class so there's only one of us
        this.Workers.RemoveRange(w => w.Class == WorkerClass.Refresh);

        WorkerInfo worker = new()
        {
            Class = WorkerClass.Refresh,
            CreatedAt = now,
            LastContact = now,
        };
        
        this.Workers.Add(worker);
        this.SaveChanges();

        return worker.WorkerId;
    }

    /// <summary>
    /// Mark a worker as contacted.
    /// </summary>
    /// <param name="id">Our worker ID.</param>
    /// <returns>False if the worker doesn't exist, and the worker should shut down.</returns>
    public bool MarkWorkerContacted(int id)
    {
        WorkerInfo? worker = this.Workers.FirstOrDefault(w => w.WorkerId == id);
        if (worker == null)
            return false;

        worker.LastContact = this._time.Now;
        this.SaveChanges();
        
        return true;
    }

    public object? GetJobState(string jobId, Type type)
    {
        PersistentJobState? state = this.JobStates.FirstOrDefault(s => s.JobId == jobId);
        if (state == null)
            return null;

        return JsonConvert.DeserializeObject(state.State, type);
    }

    public void UpdateOrCreateJobState(string jobId, object state)
    {
        PersistentJobState jobState = new()
        {
            JobId = jobId,
            State = JsonConvert.SerializeObject(state, Formatting.None),
        };

        this.JobStates.Update(jobState);
        this.SaveChanges();
    }
}