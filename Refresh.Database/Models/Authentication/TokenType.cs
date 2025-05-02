namespace Refresh.Database.Models.Authentication;

public enum TokenType
{
    Game = 0,
    Api = 1,
    PasswordReset = 2,
    ApiRefresh = 3,
}