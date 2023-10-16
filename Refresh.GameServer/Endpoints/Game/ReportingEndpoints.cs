using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Report;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game; 

public class ReportingEndpoints : EndpointGroup 
{
    [GameEndpoint("grief", HttpMethods.Post, ContentType.Xml)]
    public Response UploadReport(RequestContext context, GameDatabaseContext database, GameReport body, GameUser user, IDateTimeProvider time, Token token)
    {
        GameLevel? level = database.GetLevelById(body.LevelId);

        Size imageSize = token.TokenGame switch {
            TokenGame.LittleBigPlanet1 => new Size(640, 360),
            TokenGame.LittleBigPlanet2 => new Size(640, 360),
            TokenGame.LittleBigPlanet3 => new Size(640, 360),
            TokenGame.LittleBigPlanetVita => new Size(512, 290),
            TokenGame.LittleBigPlanetPSP => new Size(480, 272),
            _ => throw new ArgumentOutOfRangeException(nameof(token), $"Token game {token.TokenGame} is not allowed for grief upload!"),
        };
        
        //If the level is specified but its invalid, return BadRequest
        if (body.LevelId != 0 && level == null)
            return BadRequest;

        if (user.RedirectGriefReportsToPhotos)
        {
            List<SerializedPhotoSubject> subjects = new();
            if (body.Players != null)
                subjects.AddRange(body.Players.Select(player => new SerializedPhotoSubject
                {
                    Username = player.Username,
                    DisplayName = player.Username,
                    // ReSharper disable PossibleLossOfFraction YES I KNOW THESE ARE INTEGERS
                    BoundsList = $"{(float)(player.Rectangle.Left - imageSize.Width / 2) / (imageSize.Width / 2)}," +
                                 $"{(float)(player.Rectangle.Top - imageSize.Height / 2) / (imageSize.Height / 2)}," +
                                 $"{(float)(player.Rectangle.Right - imageSize.Width / 2) / (imageSize.Width / 2)}," +
                                 $"{(float)(player.Rectangle.Bottom - imageSize.Height / 2) / (imageSize.Height / 2)}",
                }));

            string hash = context.IsPSP() ? "psp/" + body.JpegHash : body.JpegHash;
            
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
                    Type = level.Source switch {
                        GameLevelSource.User => "user",
                        GameLevelSource.Story => "developer",
                        _ => throw new ArgumentOutOfRangeException(),
                    },
                },
                PhotoSubjects = subjects,
            }, user);
            
            return OK;
        }
        
        //Basic validation
        if (body.Players is { Length: > 4 } || body.ScreenElements is { Player.Length: > 4 })
            return BadRequest;

        database.AddGriefReport(body);
        
        return OK;
    }
}