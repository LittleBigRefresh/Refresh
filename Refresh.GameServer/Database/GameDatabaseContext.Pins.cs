using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Pins;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdatePinsForUser(Dictionary<long, int> pinProgresses, List<long> profilePins, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        bool isBeta = game == TokenGame.BetaBuild;

        IEnumerable<PinProgressRelation> exProgresses = this.GetPinProgressesByUser(user, isBeta);
        IEnumerable<ProfilePinRelation> exProfilePins = this.GetProfilePinsByUser(user, game);
        int failedProfilePinUpdates = 0;
       
        this.Write(() => 
        {
            // Update pin progress
            foreach (KeyValuePair<long, int> pinProgress in pinProgresses)
            {
                PinProgressRelation? exRelation = exProgresses.FirstOrDefault(p => p.PinId == pinProgress.Key);

                if (exRelation == null)
                {
                    // Add new relation
                    PinProgressRelation newRelation = new()
                    {
                        PinId = pinProgress.Key,
                        Progress = pinProgress.Value,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    };
                    this.PinProgressRelations.Add(newRelation);
                }
                // Only update if the new progress is actually better
                else if (pinProgress.Value > exRelation.Progress)
                {
                    // Update relation
                    exRelation.Progress = pinProgress.Value;
                    exRelation.LastUpdated = now;
                }
            }

            // Update to also have the new pins
            exProgresses = this.GetPinProgressesByUser(user, isBeta);
            
            // Update profile pins
            for (int i = 0; i < profilePins.Count; i++)
            {
                // Get the pin at this index, aswell as the progress type of the new profile pin
                ProfilePinRelation? exRelation = exProfilePins.FirstOrDefault(p => p.Index == i);
                long progressType = profilePins[i];

                // If there is no profile pin at that index or the existing profile pin, which is there,
                // is not referencing the same pin (by the progressType) as the new profile pin
                if (exRelation == null || exRelation.PinId != progressType)
                {
                    // Check if the user even has any progress on this pin before adding it as a profile pin
                    if (exProgresses.Any(p => p.PinId == progressType))
                    {
                        // Replace the profile pin by removing the old relation (if it exists) and adding a new one
                        if (exRelation != null) 
                        {
                            Console.WriteLine($"UpdatePinsForUser: Removed profile pin: pin ID {exRelation.PinId}, pubName: {exRelation.Publisher.Username}, at: {exRelation.Index}, game: {exRelation.Game}, time: {exRelation.Timestamp}");
                            this.ProfilePinRelations.Remove(exRelation);
                        }

                        // Replace with new relation
                        ProfilePinRelation newRelation = new()
                        {
                            PinId = progressType,
                            Publisher = user,
                            Index = i,
                            Game = game,
                            Timestamp = now,
                        };
                        this.ProfilePinRelations.Add(newRelation);
                        Console.WriteLine($"UpdatePinsForUser: Added profile pin: pin ID {newRelation.PinId}, pubName: {newRelation.Publisher.Username}, at: {newRelation.Index}, game: {newRelation.Game}, time: {newRelation.Timestamp}");
                    }
                    else
                    {
                        failedProfilePinUpdates++;
                    }
                }
            }
        });

        if (failedProfilePinUpdates > 0)
        {
            this.AddErrorNotification
            (
                "Profile pin update failed", 
                $"Failed to update {failedProfilePinUpdates} out of {profilePins.Count} profile pins "+
                $"for game {game} due to you not having unlocked them",
                user
            );
        }
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