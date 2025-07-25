using System.Drawing;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Report;

namespace Refresh.Interfaces.Game.Endpoints; 

public class ReportingEndpoints : EndpointGroup 
{
    [GameEndpoint("grief", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UploadReport(RequestContext context, GameDatabaseContext database, GameReport body, GameUser user,
        IDateTimeProvider time, Token token, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameLevel? level = database.GetLevelByIdAndType(body.LevelType, body.LevelId);
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
                context.Logger.LogWarning(BunkumCategory.Game, $"User {user} tried to upload grief report with invalid token type {token.TokenGame}!");
                return BadRequest;
        }
        
        string jpegHash = body.JpegHash;
        
        //If the level is specified but its invalid, set it to 0, to indicate the level is unknown
        //This case is hit when someone makes a grief report from a non-existent level, which we allow
        if (body.LevelId != 0 && level == null)
            body.LevelId = 0;
        
        //Basic validation
        if (body.Players is { Length: > 4 } || body.ScreenElements is { Player.Length: > 4 })
            // Return OK on PSP, since if we dont, it will error when trying to access the community moon and soft-lock the save file
            return context.IsPSP() ? OK : BadRequest;

        
        if (user.RedirectGriefReportsToPhotos)
        {
            List<SerializedPhotoSubject> subjects = new();
            if (body.Players != null)
                subjects.AddRange(body.Players.Select(player => new SerializedPhotoSubject
                {
                    Username = player.Username,
                    DisplayName = player.Username,
                    BoundsList = player.Rectangle == null ? null : NormalizeRectangle(player.Rectangle, imageSize),
                }));

            database.UploadPhoto(new SerializedPhoto
            {
                Timestamp = time.TimestampSeconds,
                AuthorName = user.Username,
                SmallHash = jpegHash,
                MediumHash = jpegHash,
                LargeHash = jpegHash,
                PlanHash = "0",
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
        
            return OK; // just upload photo
        }
        
        // create report //

        string marqeeRect = body.Marqee == null ? string.Empty : NormalizeRectangle(body.Marqee.Rect, imageSize);

        List<ReportPlayerRelation> players = new();
        if (body.Players != null)
        {
            players.AddRange(body.Players.Select(player => 
            {
                GameUser? playerUser = database.GetUserByUsername(player.Username); // what if user doesnt exist
                return new ReportPlayerRelation
                {
                    User = playerUser, 
                    IsReporter = player.Reporter,
                    IsInGameNow = player.IngameNow, 
                    PlayerNumber = player.PlayerNumber,
                    PlayerRect = player.Rectangle == null ? string.Empty : NormalizeRectangle(player.Rectangle, imageSize)
                };
            }));
        }

        Report report = new()
        {
            Reporter = user,
            Level = level,
            LevelType = body.LevelType,
            InitialStateHash = body.InitialStateHash ?? "0",
            GriefStateHash = body.GriefStateHash ?? "0", 
            PhotoAssetHash = jpegHash ?? "0",
            MarkerRect = marqeeRect,
            Type = body.Type,
            Description = body.Description ?? string.Empty,
            Players = players
        };

        database.CreateReport(report);
            
        return OK;
    }
    
    private static string NormalizeRectangle(Rect rect, Size imageSize)
    {
        float halfWidth = imageSize.Width / 2f;
        float halfHeight = imageSize.Height / 2f;
    
        return $"{(rect.Left - halfWidth) / halfWidth}," +
               $"{(rect.Top - halfHeight) / halfHeight}," +
               $"{(rect.Right - halfWidth) / halfWidth}," +
               $"{(rect.Bottom - halfHeight) / halfHeight}"; // "l,t,r,b"
    }
}