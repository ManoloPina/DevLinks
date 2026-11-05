# 06. Qualidade, Testes e DevOps

Este documento define a estratégia de testes automatizados, as configurações de containerização com Docker, a orquestração com Docker Compose e o pipeline de Integração Contínua (CI) com GitHub Actions para o projeto **devlinks (Link-Sharing App)**.

---

## 1. Estratégia de Testes Automatizados

A garantia de qualidade e conformidade com os requisitos do Frontend Mentor apoia-se em duas camadas estruturadas:

```
          ▲
         / \
        /   \
       / E2E \          <-- Validação de fluxos visuais e drag-and-drop no Frontend
      /───────\
     / Integração\      <-- WebApplicationFactory + Testcontainers.PostgreSql (Auth, Links, Profile)
    /─────────────\
   /   Unitários   \    <-- xUnit + FluentAssertions (Validadores de Plataformas, Reordenação, Entidades)
  /─────────────────\
```

### 1.1 Testes Unitários (`UrlShortener.UnitTests`)

Focam nas regras de negócio isoladas, sem dependência de banco de dados ou rede:

1. **Validador de Plataformas (`PlatformValidatorTests`):**
   - Testa as 14 plataformas (GitHub, YouTube, LinkedIn, Frontend Mentor, Twitter/X, Dev.to, Codewars, etc.).
   - Assegura que URLs corretas passem na validação.
   - Rejeita URLs de outros domínios, sem protocolo ou com formatos malformados.
2. **Entidade de Perfil (`UserProfileTests`):**
   - Valida que `FirstName` e `LastName` são obrigatórios e não aceitam apenas espaços.
   - Verifica a atualização de e-mail de exibição e avatar.
3. **Lógica de Reordenação e Sincronização de Links:**
   - Garante que a ordenação em lote normaliza os índices de `DisplayOrder` de $0$ a $N-1$.
   - Identifica corretamente links a serem excluídos e inseridos.

#### Exemplo de Teste Unitário com `[Theory]` para Plataformas:

```csharp
using FluentAssertions;
using UrlShortener.Core.Services;
using Xunit;

namespace UrlShortener.UnitTests;

public sealed class PlatformValidatorTests
{
    private readonly PlatformValidator _validator = new();

    [Theory]
    [InlineData("github", "https://github.com/benwright")]
    [InlineData("github", "https://www.github.com/benwright/")]
    [InlineData("frontendmentor", "https://www.frontendmentor.io/profile/benwright")]
    [InlineData("twitter", "https://twitter.com/benwright")]
    [InlineData("twitter", "https://x.com/benwright")]
    [InlineData("linkedin", "https://www.linkedin.com/in/benwright")]
    [InlineData("youtube", "https://www.youtube.com/@benwright")]
    [InlineData("devto", "https://dev.to/benwright")]
    public void ValidateUrl_WithValidPlatformUrl_ShouldReturnSuccess(string platform, string url)
    {
        var result = _validator.Validate(platform, url);
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("github", "https://google.com/benwright")]
    [InlineData("frontendmentor", "https://github.com/benwright")]
    [InlineData("linkedin", "https://linkedin.com/in/")] // sem username
    [InlineData("twitter", "not-a-url")]
    public void ValidateUrl_WithInvalidPlatformUrl_ShouldReturnError(string platform, string url)
    {
        var result = _validator.Validate(platform, url);
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
```

---

### 1.2 Testes de Integração (`UrlShortener.IntegrationTests`)

Executam o fluxo completo da API contra uma instância real e efêmera de **PostgreSQL** provisionada em container temporário via **Testcontainers.PostgreSql**:

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using UrlShortener.Api.Common.DTOs;
using Xunit;

namespace UrlShortener.IntegrationTests;

public sealed class LinkSharingFlowTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", _postgres.GetConnectionString());
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserFlow_Register_UpdateProfile_BatchSaveLinks_AndGetPublicProfile()
    {
        // 1. Registro de usuário
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "ben@example.com", "Password123!", "Password123!"));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authData.Should().NotBeNull();
        var token = authData!.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Atualizar perfil (First name e Last name)
        var profileResponse = await _client.PutAsJsonAsync("/api/profile", new UpdateProfileRequest(
            "Ben", "Wright", "ben.public@example.com"));
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Salvar links em lote (com drag-and-drop order)
        var batchRequest = new BatchSaveLinksRequest([
            new LinkItemDto(null, "github", "https://github.com/benwright", 0),
            new LinkItemDto(null, "youtube", "https://www.youtube.com/@benwright", 1)
        ]);

        var saveLinksResponse = await _client.PutAsJsonAsync("/api/links", batchRequest);
        saveLinksResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Consultar o perfil público (sem autenticação)
        using var anonClient = _factory.CreateClient();
        var publicResponse = await anonClient.GetAsync($"/api/public/{authData.UserName}");
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicData = await publicResponse.Content.ReadFromJsonAsync<PublicProfileResponse>();
        publicData.Should().NotBeNull();
        publicData!.FirstName.Should().Be("Ben");
        publicData.LastName.Should().Be("Wright");
        publicData.IsOwner.Should().BeFalse();
        publicData.Links.Should().HaveCount(2);
        publicData.Links[0].Platform.Should().Be("github");
        publicData.Links[0].DisplayOrder.Should().Be(0);
        publicData.Links[1].Platform.Should().Be("youtube");
        publicData.Links[1].DisplayOrder.Should().Be(1);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```

---

## 2. Containerização com Docker

A API utiliza um `Dockerfile` multi-stage baseado em imagens leves Alpine Linux.

### Arquivo: `src/UrlShortener.Api/Dockerfile`

```dockerfile
# Estágio 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

# Copiar arquivos de projeto para otimizar o cache de camadas
COPY ["src/UrlShortener.Api/UrlShortener.Api.csproj", "src/UrlShortener.Api/"]
COPY ["src/UrlShortener.Core/UrlShortener.Core.csproj", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "src/UrlShortener.Infrastructure/"]
RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"

# Copiar todo o código-fonte restante e publicar
COPY src/ src/
WORKDIR "/app/src/UrlShortener.Api"
RUN dotnet publish "UrlShortener.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime leve
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Criar pasta para uploads e atribuir permissões para usuário não-root
RUN mkdir -p /app/uploads/avatars && chown -R $APP_UID /app/uploads

USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
```

---

## 3. Orquestração Local com Docker Compose

O arquivo `docker-compose.yml` sobe a API e o PostgreSQL já vinculados:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/UrlShortener.Api/Dockerfile
    container_name: devlinks-api
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Port=5432;Database=devlinks_db;Username=postgres;Password=postgres_password
      - JwtSettings__Secret=<defina-uma-chave-com-pelo-menos-256-bits>
      - JwtSettings__Issuer=devlinks-api
      - JwtSettings__Audience=devlinks-app
    volumes:
      - avatar_uploads:/app/uploads/avatars
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped

  postgres:
    image: postgres:16-alpine
    container_name: devlinks-postgres
    ports:
      - "5432:5432"
    environment:
      POSTGRES_DB: devlinks_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d devlinks_db"]
      interval: 5s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
  avatar_uploads:
```

---

## 4. Pipeline de CI com GitHub Actions

O arquivo `.github/workflows/ci.yml` garante a qualidade do código a cada push ou pull request:

```yaml
name: Continuous Integration

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout do Repositório
        uses: actions/checkout@v4

      - name: Configurar SDK do .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"

      - name: Restaurar Dependências
        run: dotnet restore

      - name: Validar Formatação de Código (dotnet format)
        run: dotnet format --verify-no-changes --verbosity diagnostic

      - name: Compilar Solution
        run: dotnet build --no-restore --configuration Release

      - name: Executar Testes Automatizados (Unitários e Integração com Testcontainers)
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
```

---

## 5. Observabilidade e Health Checks

- **Configuração no `Program.cs`:**

  ```csharp
  builder.Services.AddHealthChecks()
      .AddNpgSql(builder.Configuration.GetConnectionString("Database")!, name: "postgresql");

  app.MapHealthChecks("/healthz");
  ```

- **Logging Estruturado:** Logs semânticos para registro de logins, reordenação de links e uploads de avatares com `UserId` e `TraceId`.
