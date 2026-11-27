using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using DevLinks.Core.Entities;
using DevLinks.Core.Interfaces;
using DevLinks.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtAuthenticationFixture : IDisposable
{
    public const string Key = "chave-de-teste-com-mais-de-32-bytes";
    public const string Issuer = "devlinks-test";
    public const string Audience = "devlinks-client-test";
    public const string ConnectionString = "Host=localhost;Database=devlinks_test;Username=test;Password=test";

    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    public JwtAuthenticationFixture()
    {
        var configuration = new Dictionary<string, string?>
        {
            ["ConnectionStrings__Database"] = ConnectionString,
            ["Jwt__Key"] = Key,
            ["Jwt__Issuer"] = Issuer,
            ["Jwt__Audience"] = Audience,
            ["Jwt__ExpirationMinutes"] = "60"
        };
        var previousEnvironment = configuration.Keys.ToDictionary(
          key => key,
          Environment.GetEnvironmentVariable);

        try
        {
            foreach (var setting in configuration)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(
              builder =>
              {
                  builder.UseEnvironment("Testing");
                  builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = ConnectionString,
                    ["Jwt:Key"] = Key,
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:ExpirationMinutes"] = "60"
                });
            });
                  builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<AuthorizationOptions>(options =>
          {
              options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
          });
            });
              }
            );
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
        }
        finally
        {
            foreach (var setting in previousEnvironment)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }
        }
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

}


public class JwtAuthenticationTests : IClassFixture<JwtAuthenticationFixture>
{
    private readonly JwtAuthenticationFixture _fixture;

    public JwtAuthenticationTests(JwtAuthenticationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Services_ShouldUsePostgresIdentityAndBearer()
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var authentication = services.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
        Assert.Equal(JwtAuthenticationFixture.ConnectionString, context.Database.GetConnectionString());
        Assert.NotNull(services.GetRequiredService<UserManager<AppUser>>());
        Assert.NotNull(services.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        Assert.NotNull(services.GetRequiredService<IUserStore<AppUser>>());
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authentication.DefaultAuthenticateScheme);
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authentication.DefaultChallengeScheme);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldAcceptTokenFromGenerator()
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var token = generator.GenerateToken(AppUser.Create("dev@example.com", "devuser"));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/_test/auth");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldRejectMissingToken()
    {
        using var response = await _fixture.Client.GetAsync("/_test/auth");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(response.Headers.WwwAuthenticate, header => header.Scheme == "Bearer");
    }

    [Theory]
    [InlineData("invalid-token")]
    [InlineData("")]
    public async Task ProtectedEndpoint_ShouldRejectInvalidToken(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/_test/auth");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        using var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldRejectExpiredToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtAuthenticationFixture.Key));
        var token = new JwtSecurityToken(
            issuer: JwtAuthenticationFixture.Issuer,
            audience: JwtAuthenticationFixture.Audience,
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/_test/auth");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serializedToken);
        using var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
