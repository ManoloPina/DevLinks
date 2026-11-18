using Microsoft.AspNetCore.Identity;

namespace DevLinks.Core.Entities;

public sealed class AppUser : IdentityUser<Guid>
{
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private AppUser() { }

    public static AppUser Create(string email, string userName)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
        };
    }

}
