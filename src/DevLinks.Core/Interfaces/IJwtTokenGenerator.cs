using DevLinks.Core.Entities;

namespace DevLinks.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(AppUser user);
}
