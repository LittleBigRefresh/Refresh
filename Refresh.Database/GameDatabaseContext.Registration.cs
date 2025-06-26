using MongoDB.Bson;
using Refresh.Common.Verification;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Statistics;
using Refresh.Database.Models.Users;

namespace Refresh.Database;

public partial class GameDatabaseContext // Registration
{
    public GameUser CreateUser(string username, string emailAddress, bool skipChecks = false)
    {
        if (!skipChecks)
        {
            if (!this.IsUsernameValid(username))
                throw new FormatException(
                    "Username must be valid (3 to 16 alphanumeric characters, plus hyphens and underscores)"); 
            
            if (this.IsUsernameTaken(username))
                throw new InvalidOperationException("Cannot create a user with an existing username");
        
            if (this.IsEmailTaken(emailAddress))
                throw new InvalidOperationException("Cannot create a user with an existing email address");
        }

        emailAddress = emailAddress.ToLowerInvariant();
        
        GameUser user = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            EmailAddressVerified = false,
            JoinDate = this._time.Now,
        };

        this.Write(() =>
        {
            this.GameUsers.Add(user);
        });
        
        this.Write(() =>
        {
            user.Statistics = new GameUserStatistics
            {
                UserId = user.UserId,
            };
            this.GameUserStatistics.Add(user.Statistics);
        });
        return user;
    }

    public GameUser CreateUserFromQueuedRegistration(QueuedRegistration registration, TokenPlatform? platform = null)
    {
        this.Write(() =>
        {
            this.QueuedRegistrations.Remove(registration);
        });

        GameUser user = this.CreateUser(registration.Username, registration.EmailAddress);
        this.SetUserPassword(user, registration.PasswordBcrypt);

        if (platform != null)
        {
            this.Write(() =>
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (platform)
                {
                    case TokenPlatform.PS3:
                    case TokenPlatform.Vita:
                    case TokenPlatform.PSP:
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
    
    public bool IsUsernameValid(string username)
    {
        return CommonPatterns.UsernameRegex().IsMatch(username);
    }

    public bool IsUsernameQueued(string username)
    {
        return this.QueuedRegistrations.Any(r => r.Username == username);
    }
    
    public bool IsEmailQueued(string emailAddress)
    {
        return this.QueuedRegistrations.Any(r => r.EmailAddress == emailAddress);
    }

    public bool IsUsernameTaken(string username)
    {
        return this.GameUsers.Any(u => u.Username == username) ||
               this.QueuedRegistrations.Any(r => r.Username == username);
    }
    
    public bool IsEmailTaken(string emailAddress)
    {
        return this.GameUsers.Any(u => u.EmailAddress == emailAddress) ||
               this.QueuedRegistrations.Any(r => r.EmailAddress == emailAddress);
    }

    public void AddRegistrationToQueue(string username, string emailAddress, string passwordBcrypt)
    {
        if (this.IsUsernameTaken(username))
            throw new InvalidOperationException("Cannot create a registration with an existing username");
        
        if (this.IsEmailTaken(emailAddress))
            throw new InvalidOperationException("Cannot create a user with an existing email address");
        
        QueuedRegistration registration = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            PasswordBcrypt = passwordBcrypt,
            ExpiryDate = this._time.Now + TimeSpan.FromHours(1),
        };

        this.Write(() =>
        {
            this.QueuedRegistrations.Add(registration);
        });
    }

    public void RemoveRegistrationFromQueue(QueuedRegistration registration)
    {
        this.Write(() =>
        {
            this.QueuedRegistrations.Remove(registration);
        });
    }
    
    public void RemoveAllRegistrationsFromQueue() => this.Write(this.RemoveAll<QueuedRegistration>);

    public bool IsRegistrationExpired(QueuedRegistration registration) => registration.ExpiryDate < this._time.Now;

    public QueuedRegistration? GetQueuedRegistrationByUsername(string username) 
        => this.QueuedRegistrations.FirstOrDefault(q => q.Username == username);
    
    public QueuedRegistration? GetQueuedRegistrationByObjectId(ObjectId id) 
        => this.QueuedRegistrations.FirstOrDefault(q => q.RegistrationId == id);
    

    public DatabaseList<QueuedRegistration> GetAllQueuedRegistrations()
        => new(this.QueuedRegistrations);
    
    public DatabaseList<EmailVerificationCode> GetAllVerificationCodes()
        => new(this.EmailVerificationCodes);
    
    public void VerifyUserEmail(GameUser user)
    {
        this.Write(() =>
        {
            user.EmailAddressVerified = true;
            this.EmailVerificationCodes.RemoveRange(c => c.User == user);
        });
    }

    public bool VerificationCodeMatches(GameUser user, string code) => 
        this.EmailVerificationCodes.Any(c => c.User == user && c.Code == code);
    
    public bool IsVerificationCodeExpired(EmailVerificationCode code) => code.ExpiryDate < this._time.Now;

    public EmailVerificationCode CreateEmailVerificationCode(GameUser user)
    {
        EmailVerificationCode verificationCode = new()
        {
            User = user,
            Code = CodeHelper.GenerateDigitCode(),
            ExpiryDate = this._time.Now + TimeSpan.FromDays(1),
        };

        this.Write(() =>
        {
            this.EmailVerificationCodes.Add(verificationCode);
        });

        return verificationCode;
    }

    public void RemoveEmailVerificationCode(EmailVerificationCode code)
    {
        this.Write(() =>
        {
            this.EmailVerificationCodes.Remove(code);
        });
    }
    
    public bool DisallowUser(string username)
    {
        if (this.DisallowedUsers.FirstOrDefault(u => u.Username == username) != null) 
            return false;
        
        this.Write(() =>
        {
            this.DisallowedUsers.Add(new DisallowedUser
            {
                Username = username,
            });
        });
        
        return true;
    }
    
    public bool ReallowUser(string username)
    {
        DisallowedUser? disallowedUser = this.DisallowedUsers.FirstOrDefault(u => u.Username == username);
        if (disallowedUser == null) 
            return false;
        
        this.Write(() =>
        {
            this.DisallowedUsers.Remove(disallowedUser);
        });
        
        return true;
    }
    
    public bool IsUserDisallowed(string username)
    {
        return this.DisallowedUsers.FirstOrDefault(u => u.Username == username) != null;
    }
}