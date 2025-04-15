using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Pins;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdatePinsForUser(Dictionary<long, int> pinProgresses, List<long> profilePins, GameUser user, TokenGame game)
    {
        bool isBeta = game == TokenGame.BetaBuild;
        IEnumerable<PinProgressRelation> exProgresses = this.GetPinProgressesByUser(user, isBeta);
        DateTimeOffset now = this._time.Now;

        this.Write(() => 
        {
            // Update pin progress
            foreach (KeyValuePair<long, int> pinProgress in pinProgresses)
            {
                PinProgressRelation? relation = exProgresses.FirstOrDefault(p => p.PinId == pinProgress.Key);

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

            // Update to also have the new pins
            exProgresses = this.GetPinProgressesByUser(user, isBeta);
            IEnumerable<ProfilePinRelation> exProfilePins = this.GetProfilePinsByUser(user, game);

            // Update profile pins
            for (int i = 0; i < profilePins.Count; i++)
            {
                // Get the pin at this index
                ProfilePinRelation? relation = exProfilePins.FirstOrDefault(p => p.Index == i);
                long progressType = profilePins[i];

                // If that pin is the exact previously as in the update (same pin id/progressType), don't touch it
                // (especially its timestamp)
                if (relation != null && relation.Pin.PinId != progressType)
                {
                    // Check if the user even has any progress on this pin to be able to show it off
                    PinProgressRelation? progressRelation = exProgresses.FirstOrDefault(p => p.PinId == progressType);
                    if (progressRelation != null)
                    {
                        // Remove old relation
                        this.ProfilePinRelations.Remove(relation);

                        // Replace with new relation
                        this.ProfilePinRelations.Add(new()
                        {
                            Pin = progressRelation,
                            Index = i,
                            Game = game,
                            Timestamp = now,
                        });
                    }
                }
            }
        });
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations.Where(p => p.Publisher == user && p.IsBeta == isBeta);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta, int skip, int count)
        => new(this.GetPinProgressesByUser(user, isBeta), skip, count);

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game)
        => this.ProfilePinRelations.Where(p => p.Pin.Publisher == user && p.Game == game);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game), skip, count);
}