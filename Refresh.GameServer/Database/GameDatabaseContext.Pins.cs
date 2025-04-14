using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Pins;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdatePinsForUser(Dictionary<long, int> pinProgresses, IEnumerable<long> profilePins, GameUser user, TokenGame game)
    {
        bool isBeta = game == TokenGame.BetaBuild;
        IEnumerable<PinProgressRelation> existingRelations = this.GetPinProgressesByUser(user, isBeta);
        DateTimeOffset now = this._time.Now;

        this.Write(() => 
        {
            // Update pin progress
            foreach (KeyValuePair<long, int> pinProgress in pinProgresses)
            {
                PinProgressRelation? relation = existingRelations.FirstOrDefault(p => p.PinId == pinProgress.Key);

                if (relation == null)
                {
                    // Add new relation
                    this.PinProgressRelations.Add(new()
                    {
                        PinId = pinProgress.Key,
                        Progress = pinProgress.Value,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    });
                }
                else
                {
                    // Update relation
                    relation.Progress = pinProgress.Value;
                    relation.LastUpdated = now;
                }
            }

            // Update profile pins
            foreach (long progressType in profilePins)
            {

            }
        });
    }

    public PinProgressRelation? GetPinProgressByUser(long progressType, GameUser user, bool isBeta)
        => this.PinProgressRelations.FirstOrDefault(p => p.PinId == progressType && p.Publisher == user && p.IsBeta == isBeta);

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations.Where(p => p.Publisher == user && p.IsBeta == isBeta);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta, int skip, int count)
        => new(this.GetPinProgressesByUser(user, isBeta), skip, count);
}