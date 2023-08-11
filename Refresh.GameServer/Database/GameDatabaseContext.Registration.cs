using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Registration
{
    public GameUser CreateUser(string username, string emailAddress)
    {
        if (this.IsUsernameTaken(username))
            throw new InvalidOperationException("Cannot create a user with an existing username");
        
        GameUser user = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            JoinDate = this._time.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(user);
        });
        return user;
    }

    public GameUser CreateUserFromQueuedRegistration(QueuedRegistration registration, TokenPlatform? platform = null)
    {
        QueuedRegistration cloned = (QueuedRegistration)registration.Clone();

        this._realm.Write(() =>
        {
            this._realm.Remove(registration);
        });

        GameUser user = this.CreateUser(cloned.Username, cloned.EmailAddress);
        this.SetUserPassword(user, cloned.PasswordBcrypt);

        if (platform != null)
        {
            this._realm.Write(() =>
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (platform)
                {
                    case TokenPlatform.PS3:
                    case TokenPlatform.Vita:
                        user.PsnAuthenticationAllowed = true;
                        break;
                    case TokenPlatform.RPCS3:
                        user.RpcnAuthenticationAllowed = true;
                        break;
                }
            });
        }

        return user;
    }

    public bool IsUsernameTaken(string username)
    {
        return this._realm.All<GameUser>().Any(u => u.Username == username) ||
               this._realm.All<QueuedRegistration>().Any(r => r.Username == username);
    }

    public void AddRegistrationToQueue(string username, string emailAddress, string passwordBcrypt)
    {
        if (this.IsUsernameTaken(username))
            throw new InvalidOperationException("Cannot create a registration with an existing username");
        
        QueuedRegistration registration = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            PasswordBcrypt = passwordBcrypt,
            ExpiryDate = this._time.Now + TimeSpan.FromDays(1), // This registration expires in 1 day
        };

        this._realm.Write(() =>
        {
            this._realm.Add(registration);
        });
    }

    public void RemoveRegistrationFromQueue(QueuedRegistration registration)
    {
        this._realm.Write(() =>
        {
            this._realm.Remove(registration);
        });
    }
    
    public bool IsRegistrationExpired(QueuedRegistration registration) => registration.ExpiryDate >= this._time.Now;

    public QueuedRegistration? GetQueuedRegistration(string username) 
        => this._realm.All<QueuedRegistration>().FirstOrDefault(q => q.Username == username);
    

    public DatabaseList<QueuedRegistration> GetAllQueuedRegistrations()
        => new(this._realm.All<QueuedRegistration>());
}