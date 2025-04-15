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
                    relation = new()
                    {
                        PinId = pinProgress.Key,
                        Progress = pinProgress.Value,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    };
                    this.PinProgressRelations.Add(relation);
                    Console.WriteLine($"UpdatePinsForUser: NEW Progress relation: ID: {relation.PinId}, prog: {relation.Progress}, PubName: {relation.Publisher.Username}, first: {relation.FirstPublished}, last: {relation.LastUpdated}, beta: {relation.IsBeta}");
                }
                else
                {
                    // Update relation
                    relation.Progress = pinProgress.Value;
                    relation.LastUpdated = now;
                    Console.WriteLine($"UpdatePinsForUser: UPDATED Progress relation: ID: {relation.PinId}, prog: {relation.Progress}, PubName: {relation.Publisher.Username}, first: {relation.FirstPublished}, last: {relation.LastUpdated}, beta: {relation.IsBeta}");
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
                if (relation == null || relation.PinId != progressType)
                {
                    // Check if the user even has any progress on this pin to be able to show it off
                    if (exProgresses.Any(p => p.PinId == progressType))
                    {
                        // Remove old relation
                        if (relation != null) 
                        {
                            this.ProfilePinRelations.Remove(relation);
                            Console.WriteLine($"UpdatePinsForUser: Removed profile pin: pin ID {relation.PinId}, pubName: {relation.Publisher.Username}, at: {relation.Index}, game: {relation.Game}, time: {relation.Timestamp}");
                        }

                        // Replace with new relation
                        relation = new()
                        {
                            PinId = progressType,
                            Publisher = user,
                            Index = i,
                            Game = game,
                            Timestamp = now,
                        };
                        this.ProfilePinRelations.Add(relation);
                        Console.WriteLine($"UpdatePinsForUser: Added profile pin: pin ID {relation.PinId}, pubName: {relation.Publisher.Username}, at: {relation.Index}, game: {relation.Game}, time: {relation.Timestamp}");
                    }
                    else
                    {
                        Console.WriteLine($"UpdatePinsForUser: Skipped profile pin ID {progressType} at {i} update due to no progress relation");
                    }
                }
                else
                {
                    Console.WriteLine($"UpdatePinsForUser: Skipped profile pin ID {progressType} at {i} update due to same");
                }
            }
        });
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations.Where(p => p.Publisher == user && p.IsBeta == isBeta);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild), skip, count);

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game)
        => this.ProfilePinRelations.Where(p => p.Publisher == user && p._Game == (int)game);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game), skip, count);
}