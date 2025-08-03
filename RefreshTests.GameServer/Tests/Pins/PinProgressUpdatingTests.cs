using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Pins;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Levels;

public class PinProgressUpdatingTests : GameServerTest
{
    private static List<long> ToList(Dictionary<long, int> pins)
    {
        List<long> pinList = [];
        foreach(KeyValuePair<long, int> pin in pins)
        {
            pinList.Add(pin.Key);
            pinList.Add(pin.Value);
        }
        return pinList;
    }

    [Test]
    public async Task UploadListOfPinProgressesTest()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        // These pins' progress works a little differently (better the smaller, not the greater),
        // so we also test them specifically and differently
        long specialPin1Id = (long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores;
        long specialPin2Id = (long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores;

        // Upload some pins to sync
        Dictionary<long, int> pinsToUpload1 = new() 
        {
            {1, 1},
            {2, 2},
            {3, 1},
            {specialPin1Id, 10},
            {specialPin2Id, 10},
        };
        SerializedPins request1 = new()
        {
            ProgressPins = ToList(pinsToUpload1),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/update_my_pins", new StringContent(request1.AsJson())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Deserialize
        SerializedPins? response1 = JsonConvert.DeserializeObject<SerializedPins>(Encoding.UTF8.GetString(await message.Content.ReadAsByteArrayAsync()));
        Assert.That(response1, Is.Not.Null);
        Assert.That(response1!.ProgressPins, Is.Not.Empty);

        Dictionary<long, int> responsePins1 = SerializedPins.ToDictionary(response1!.ProgressPins);

        // Check the response to have the same pins as the request
        foreach (KeyValuePair<long, int> requestPin in pinsToUpload1)
        {
            KeyValuePair<long, int>? responsePin = responsePins1.FirstOrDefault(p => p.Key == requestPin.Key);
            Assert.That(responsePin, Is.Not.Null);
            Assert.That(responsePin.Value.Value, Is.EqualTo(requestPin.Value));
        }

        // Now upload another request to try to update the pins and add new ones
        Dictionary<long, int> pinsToUpload2 = new() 
        {
            {4, 1},
            {5, 1},
            {2, 1}, // Have this one be intentionally worse to test whether the server only keeps better progresses
            {3, 4},
            {specialPin1Id, 5},
            {specialPin2Id, 100}, // Same with this one
        };
        SerializedPins request2 = new()
        {
            ProgressPins = ToList(pinsToUpload2),
        };

        message = client.PostAsync($"/lbp/update_my_pins", new StringContent(request2.AsJson())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Deserialize
        SerializedPins? response2 = JsonConvert.DeserializeObject<SerializedPins>(Encoding.UTF8.GetString(await message.Content.ReadAsByteArrayAsync()));
        Assert.That(response2, Is.Not.Null);
        Assert.That(response2!.ProgressPins, Is.Not.Empty);

        Dictionary<long, int> responsePins2 = SerializedPins.ToDictionary(response2!.ProgressPins);

        // This is how the pin progress in the new response should look like
        Dictionary<long, int> syncedPinsSolution = new() 
        {
            {1, 1}, // Ignored
            {2, 2}, // Not updated
            {3, 4}, // Updated
            {4, 1}, // New
            {5, 1}, // New
            {specialPin1Id, 5}, // Updated
            {specialPin2Id, 10}, // Not Updated
        };
        
        // Check the new response with the solution
        foreach (KeyValuePair<long, int> solutionPin in syncedPinsSolution)
        {
            KeyValuePair<long, int>? responsePin = responsePins2.FirstOrDefault(p => p.Key == solutionPin.Key);
            Assert.That(responsePin, Is.Not.Null);
            Assert.That(responsePin.Value.Value, Is.EqualTo(solutionPin.Value));
        }
    }
}