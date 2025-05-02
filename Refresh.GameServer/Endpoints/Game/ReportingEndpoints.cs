using System.Drawing;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Database.Models.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Report;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;

namespace Refresh.GameServer.Endpoints.Game; 

public class ReportingEndpoints : EndpointGroup 
{
    [GameEndpoint("grief", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UploadReport(RequestContext context, GameDatabaseContext database, GameReport body, GameUser user,
        IDateTimeProvider time, Token token, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameLevel? level = database.GetLevelById(body.LevelId);

        Size imageSize;
        switch (token.TokenGame)
        {
            case TokenGame.LittleBigPlanet1:
            case TokenGame.LittleBigPlanet2:
            case TokenGame.LittleBigPlanet3:
                imageSize = new Size(640, 360);
                break;
            case TokenGame.LittleBigPlanetVita:
                imageSize = new Size(512, 290);
                break;
            case TokenGame.LittleBigPlanetPSP:
                imageSize = new Size(480, 272);
                break;
            case TokenGame.Website:
            default:
                context.Logger.LogWarning(BunkumCategory.Game, $"User {user} tried to upload grief report with token type {token.TokenGame}!");
                return BadRequest;
        }

        List<SerializedPhotoSubject> subjects = new();
        if (body.Players != null)
            subjects.AddRange(body.Players.Select(player => new SerializedPhotoSubject
            {
                Username = player.Username,
                DisplayName = player.Username,
                // ReSharper disable PossibleLossOfFraction YES I KNOW THESE ARE INTEGERS
                BoundsList = player.Rectangle == null 
                    ? null : $"{(float)(player.Rectangle.Left - imageSize.Width / 2) / (imageSize.Width / 2)}," +
                             $"{(float)(player.Rectangle.Top - imageSize.Height / 2) / (imageSize.Height / 2)}," +
                             $"{(float)(player.Rectangle.Right - imageSize.Width / 2) / (imageSize.Width / 2)}," +
                             $"{(float)(player.Rectangle.Bottom - imageSize.Height / 2) / (imageSize.Height / 2)}",
            }));

        string hash = body.JpegHash;
        
        //If the level is specified but its invalid, set it to 0, to indicate the level is unknown
        //This case is hit when someone makes a grief report from a non-existent level, which we allow
        if (body.LevelId != 0 && level == null)
            body.LevelId = 0;
        
        //Basic validation
        if (body.Players is { Length: > 4 } || body.ScreenElements is { Player.Length: > 4 })
            //Return OK on PSP, since if we dont, it will error when trying to access the community moon and soft-lock the save file
            return context.IsPSP() ? OK : BadRequest;
            
        database.UploadPhoto(new SerializedPhoto
        {
            Timestamp = time.TimestampSeconds,
            AuthorName = user.Username,
            SmallHash = hash,
            MediumHash = hash,
            LargeHash = hash,
            PlanHash = "0",
            //If the level id is 0 or we couldn't find the level null, dont fill out the `Level` field
            Level = body.LevelId == 0 || level == null ? null : new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = level.SlotType switch {
                    GameSlotType.User => "user",
                    GameSlotType.Story => "developer",
                    _ => throw new ArgumentOutOfRangeException(),
                },
            },
            PhotoSubjects = subjects,
        }, user);
            
        return OK;
    }
}