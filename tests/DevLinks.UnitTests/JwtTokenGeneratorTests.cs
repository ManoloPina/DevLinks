using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevLinks.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


public class JwtTokenGeneratorTests
{
    private const string Key = "chave-de-teste-com-mais-de-32-bytes";
    private const string Issuer = "devlinks-test";
    private const string Audience = "devlinks-client-test";

    private static JwtTokenGenerator CreateGenerator()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = Key,
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:ExpirationMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(settings)
        .Build();
        return new JwtTokenGenerator(configuration);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeUserClaimsAndUniqueJti()
    {
        var generator = CreateGenerator();
        var user = AppUser.Create("dev@example.com", "devuser");

        var firstToken = generator.GenerateToken(user);
        var secondToken = generator.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var firstJwt = handler.ReadJwtToken(firstToken);
        var secondJwt = handler.ReadJwtToken(secondToken);

        Assert.Equal(user.Id.ToString(), firstJwt.Subject);
        Assert.Equal(user.Email, firstJwt.Claims.Single(claim => claim.Type == "email").Value);
        Assert.Equal(user.UserName, firstJwt.Claims.Single(claim => claim.Type == "name").Value);

        var firstJti = firstJwt.Claims.Single(claim => claim.Type == "jti").Value;
        var secondJti = secondJwt.Claims.Single(claim => claim.Type == "jti").Value;

        Assert.True(Guid.TryParse(firstJti, out _));
        Assert.NotEqual(firstJti, secondJti);

    }

    [Fact]
    public void GenerateToken_ShouldCreateSignedTokenWithIssuerAudienceAndExpiration()
    {
        var generator = CreateGenerator();
        var user = AppUser.Create("dev@example.com", "devuser");
        var beforeGeneration = DateTime.UtcNow;

        var token = generator.GenerateToken(user);

        var afterGeneration = DateTime.UtcNow;
        var handler = new JwtSecurityTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        handler.ValidateToken(token, parameters, out var validatedToken);

        Assert.InRange(
          validatedToken.ValidTo,
          beforeGeneration.AddMinutes(60).AddSeconds(-1),
          afterGeneration.AddMinutes(60)
        );
    }

}
