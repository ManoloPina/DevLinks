using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevLinks.Core.Entities;
using DevLinks.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key não foi configurada.");


        if (Encoding.UTF8.GetByteCount(_key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 bytes.");
        }

        _issuer = configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Jwt:Issuer não foi configurado.");

        _audience = configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("Jwt:Audience não foi configurada.");

        if (!int.TryParse(configuration["Jwt:ExpirationMinutes"], out _expirationMinutes)
        || _expirationMinutes <= 0)
        {
            _expirationMinutes = 60;
        }

    }

    public string GenerateToken(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var email = user.Email
        ?? throw new ArgumentException("O usuário precisa ter um email.", nameof(user));

        var userName = user.UserName ?? throw new ArgumentException("O usuário precisa ter um nome de usuário.", nameof(user));

        var claims = new[]
        {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Email, email),
      new Claim(JwtRegisteredClaimNames.Name, userName),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(
          signingKey,
          SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
          issuer: _issuer,
          audience: _audience,
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
          signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
