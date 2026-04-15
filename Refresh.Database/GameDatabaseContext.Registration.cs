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

    public bool IsUsernameTaken(string username, GameUser? userToName = null)
    {
        if (this.GameUsers.Any(u => u.Username == username)) return true;
        if (this.QueuedRegistrations.Any(r => r.Username == username)) return true;
        if (this.IsUserDisallowed(username)) return true;
        
        PreviousUsername? previous = this.PreviousUsernames.FirstOrDefault(p => p.Username == username);
        // no one has ever had this name before
        if (previous == null) return false;
        // this is not the initial owner of the name (only previous owners may be renamed back)
        if (userToName == null || userToName.UserId != previous.UserId) return true;

        return false;
    }

    public bool IsEmailTaken(string emailAddress)
    {
        return this.GameUsers.Any(u => u.EmailAddress == emailAddress) ||
               this.QueuedRegistrations.Any(r => r.EmailAddress == emailAddress) ||
               this.IsEmailAddressDisallowed(emailAddress);
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
            UsernameLower = username.ToLower(),
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

    public QueuedRegistration? GetQueuedRegistrationByUsername(string username, bool setToCorrectUsernameCasing = true)
    {
#pragma warning disable CA1862
        QueuedRegistration? registration = this.QueuedRegistrations.FirstOrDefault(q => q.UsernameLower == username.ToLower());
#pragma warning restore CA1862

        // Correct the username's casing so we don't end up using this registration to create an account with a potentially
        // wrongly cased username, leading to it not exactly matching to the ticket's username and preventing future logins.
        if (registration != null && setToCorrectUsernameCasing)
        {
            registration.Username = username;
        }

        return registration;
    }

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

    public bool IsUserDisallowed(string username)
        => this.DisallowedUsers.Any(u => u.Username == username);
    
    public DisallowedUser? GetDisallowedUserInfo(string username)
        => this.DisallowedUsers.FirstOrDefault(d => d.Username == username);
    
    public DatabaseList<DisallowedUser> GetDisallowedUsers(int skip, int count)
        => new(this.DisallowedUsers, skip, count);
    
    public (DisallowedUser, bool) DisallowUser(string username, string reason)
    {
        DisallowedUser? existing = this.GetDisallowedUserInfo(username);
        if (existing != null) return (existing, false);
        
        DisallowedUser disallowed = new()
        {
            Username = username,
            Reason = reason,
            DisallowedAt = this._time.Now,
        };
        this.DisallowedUsers.Add(disallowed);
        this.SaveChanges();
        
        return (disallowed, true);
    }
    
    public bool ReallowUser(string username)
    {
        DisallowedUser? disallowedUser = this.GetDisallowedUserInfo(username);
        if (disallowedUser == null) 
            return false;
        
        this.DisallowedUsers.Remove(disallowedUser);
        this.SaveChanges();
        
        return true;
    }

    public bool IsEmailAddressDisallowed(string emailAddress)
        => this.DisallowedEmailAddresses.Any(u => u.Address == emailAddress);

    public DisallowedEmailAddress? GetDisallowedEmailAddressInfo(string emailAddress)
        => this.DisallowedEmailAddresses.FirstOrDefault(d => d.Address == emailAddress);

    public DatabaseList<DisallowedEmailAddress> GetDisallowedEmailAddresses(int skip, int count)
        => new(this.DisallowedEmailAddresses, skip, count);

    public (DisallowedEmailAddress, bool) DisallowEmailAddress(string emailAddress, string reason)
    {
        DisallowedEmailAddress? existing = this.GetDisallowedEmailAddressInfo(emailAddress);
        if (existing != null) return (existing, false);
        
        DisallowedEmailAddress disallowed = new()
        {
            Address = emailAddress,
            Reason = reason,
            DisallowedAt = this._time.Now,
        };
        this.DisallowedEmailAddresses.Add(disallowed);
        this.SaveChanges();
        
        return (disallowed, true);
    }
    
    public bool ReallowEmailAddress(string emailAddress)
    {
        DisallowedEmailAddress? disallowed = this.GetDisallowedEmailAddressInfo(emailAddress);
        if (disallowed == null) 
            return false;
        
        this.DisallowedEmailAddresses.Remove(disallowed);
        this.SaveChanges();
        
        return true;
    }

    private string GetEmailDomainFromAddress(string emailAddress)
        => emailAddress.Split('@').Last();
    
    public bool IsEmailDomainDisallowed(string emailAddress)
    {
        string emailDomain = this.GetEmailDomainFromAddress(emailAddress);
        return this.DisallowedEmailDomains.Any(u => u.Domain == emailDomain);
    }

    public DisallowedEmailDomain? GetDisallowedEmailDomainInfo(string emailAddress)
    {
        string emailDomain = this.GetEmailDomainFromAddress(emailAddress);
        return this.DisallowedEmailDomains.FirstOrDefault(d => d.Domain == emailDomain);
    }

    public DatabaseList<DisallowedEmailDomain> GetDisallowedEmailDomains(int skip, int count)
        => new(this.DisallowedEmailDomains, skip, count);

    public (DisallowedEmailDomain, bool) DisallowEmailDomain(string emailAddress, string reason)
    {
        string emailDomain = this.GetEmailDomainFromAddress(emailAddress);
        DisallowedEmailDomain? existing = this.GetDisallowedEmailDomainInfo(emailDomain);
        if (existing != null) return (existing, false);
        
        DisallowedEmailDomain disallowed = new()
        {
            Domain = emailDomain,
            Reason = reason,
            DisallowedAt = this._time.Now,
        };
        this.DisallowedEmailDomains.Add(disallowed);
        this.SaveChanges();
        
        return (disallowed, true);
    }

    public bool ReallowEmailDomain(string emailAddress)
    {
        string emailDomain = this.GetEmailDomainFromAddress(emailAddress);
        DisallowedEmailDomain? disallowedDomain = this.GetDisallowedEmailDomainInfo(emailDomain);
        if (disallowedDomain == null) 
            return false;
        
        this.DisallowedEmailDomains.Remove(disallowedDomain);
        this.SaveChanges();
        
        return true;
    }
}