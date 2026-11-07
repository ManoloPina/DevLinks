# 07. Roadmap e Plano de Implementação

Este documento estabelece o roteiro de execução prático e detalhado para construir o backend e a integração da aplicação **devlinks (Link-Sharing App)**, baseada no desafio de nível _Guru_ do [Frontend Mentor](https://www.frontendmentor.io/challenges/linksharing-app-Fbt7yweGsT), cobrindo desde a fundação técnica até a publicação no GitHub.

---

## 1. Visão Geral das Fases

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ Fase 1   │───►│ Fase 2   │───►│ Fase 3   │───►│ Fase 4   │───►│ Fase 5   │───►│ Fase 6+  │
│ Setup &  │    │ Auth &   │    │ Perfil & │    │ Links &  │    │ Preview  │    │ Testes,  │
│ Tooling  │    │ Identity │    │ Avatar   │    │ Validação│    │ Público  │    │ CI & Pub │
└──────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘
```

---

## 2. Guia de Scaffold com a .NET CLI

Execute os comandos abaixo a partir da raiz do repositório para estruturar a solution completa:

### 2.1 Criar Solution, Gitignore e Manifesto de Ferramentas

```bash
# 1. Criar solution no formato .slnx e gitignore
dotnet new sln --format slnx -n DevLinks
dotnet new gitignore

# 2. Configurar manifesto de ferramentas locais e instalar a CLI do EF Core
dotnet new tool-manifest
dotnet tool install dotnet-ef
```

### 2.2 Criar os Projetos

```bash
# 1. API Principal (Minimal APIs)
dotnet new webapi -n DevLinks.Api -o src/DevLinks.Api --use-program-main=false

# 2. Camada de Domínio e Aplicação (Class Library)
dotnet new classlib -n DevLinks.Core -o src/DevLinks.Core

# 3. Camada de Infraestrutura e Banco (Class Library)
dotnet new classlib -n DevLinks.Infrastructure -o src/DevLinks.Infrastructure

# 4. Projetos de Teste (xUnit)
dotnet new xunit -n DevLinks.UnitTests -o tests/DevLinks.UnitTests
dotnet new xunit -n DevLinks.IntegrationTests -o tests/DevLinks.IntegrationTests
```

Após os scaffolds, remover os arquivos `Class1.cs` (Core e Infrastructure) e `UnitTest1.cs` (testes) gerados pelos templates.

### 2.3 Vincular os Projetos à Solution

```bash
dotnet sln add src/DevLinks.Api
dotnet sln add src/DevLinks.Core
dotnet sln add src/DevLinks.Infrastructure
dotnet sln add tests/DevLinks.UnitTests
dotnet sln add tests/DevLinks.IntegrationTests
```

### 2.4 Configurar as Referências entre Projetos

```bash
# Api depende de Core e Infrastructure
dotnet add src/DevLinks.Api reference src/DevLinks.Core
dotnet add src/DevLinks.Api reference src/DevLinks.Infrastructure

# Infrastructure depende de Core
dotnet add src/DevLinks.Infrastructure reference src/DevLinks.Core

# Testes Unitários dependem de Core
dotnet add tests/DevLinks.UnitTests reference src/DevLinks.Core

# Testes de Integração dependem da Api e Infrastructure
dotnet add tests/DevLinks.IntegrationTests reference src/DevLinks.Api
dotnet add tests/DevLinks.IntegrationTests reference src/DevLinks.Infrastructure
```

### 2.5 Instalar os Pacotes NuGet Necessários

```bash
# Na API:
dotnet add src/DevLinks.Api package Scalar.AspNetCore
dotnet add src/DevLinks.Api package Microsoft.AspNetCore.OpenApi
dotnet add src/DevLinks.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/DevLinks.Api package AspNetCore.HealthChecks.NpgSql
dotnet add src/DevLinks.Api package Microsoft.EntityFrameworkCore.Design

# Na Infraestrutura:
dotnet add src/DevLinks.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/DevLinks.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/DevLinks.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/DevLinks.Infrastructure package System.IdentityModel.Tokens.Jwt

# Nos Testes Unitários:
dotnet add tests/DevLinks.UnitTests package FluentAssertions
dotnet add tests/DevLinks.UnitTests package NSubstitute
dotnet add tests/DevLinks.UnitTests package coverlet.collector

# Nos Testes de Integração:
dotnet add tests/DevLinks.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/DevLinks.IntegrationTests package Testcontainers.PostgreSql
dotnet add tests/DevLinks.IntegrationTests package FluentAssertions
dotnet add tests/DevLinks.IntegrationTests package coverlet.collector
```

### 2.6 Verificar a Solution

```bash
dotnet build DevLinks.slnx
dotnet sln list
dotnet ef --version
```

---

## 3. Checklist de Execução Fase a Fase

### Fase 1: Setup da Solution e Governança do Repositório

- [ ] Executar comandos de scaffold e vincular dependências da solution.
- [ ] Criar arquivo `.editorconfig` com padrões de formatação C# e espaçamento.
- [ ] Criar arquivo `LICENSE` (MIT).
- [ ] Configurar templates do repositório em `.github/`:
  - [ ] `.github/pull_request_template.md`
  - [ ] `.github/ISSUE_TEMPLATE/bug_report.md`
  - [ ] `.github/ISSUE_TEMPLATE/feature_request.md`
  - [ ] `.github/dependabot.yml` para monitoramento de pacotes NuGet e Actions.
- [ ] Fazer o commit inicial no Git (`chore: initial solution scaffold and governance`).

### Fase 2: Autenticação com ASP.NET Core Identity e JWT

- [ ] Subir container do PostgreSQL via Docker Compose.
- [ ] Implementar entidade `AppUser` herdando de `IdentityUser<Guid>`.
- [ ] Configurar `AppDbContext` herdando de `IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>`.
- [ ] Implementar `JwtTokenGenerator` com emissão de token contendo claims (`sub`, `email`, `name`).
- [ ] Configurar autenticação JWT no pipeline (`builder.Services.AddAuthentication()`).
- [ ] Criar endpoints de autenticação em `AuthEndpoints`:
  - [ ] `POST /api/auth/register` (com criação automática de perfil em branco).
  - [ ] `POST /api/auth/login`.
  - [ ] `GET /api/auth/me`.
- [ ] Adicionar suporte ao `IExceptionHandler` para respostas padronizadas em RFC 7807 (`application/problem+json`).

### Fase 3: Gestão de Perfil e Upload de Avatar

- [ ] Criar entidade `UserProfile` (`FirstName`, `LastName`, `DisplayEmail`, `AvatarUrl`) e mapeamento fluente.
- [ ] Criar endpoints em `ProfileEndpoints`:
  - [ ] `GET /api/profile` (buscar dados do usuário logado).
  - [ ] `PUT /api/profile` (atualizar nome, sobrenome e e-mail com validação obrigatória).
- [ ] Implementar `IAvatarStorageService` e serviço local `LocalAvatarStorageService`.
- [ ] Implementar `POST /api/profile/avatar` (upload multipart com validação de JPG/PNG e limite de 2MB).
- [ ] Configurar middleware de arquivos estáticos (`UseStaticFiles`) para servir avatares em `/uploads/avatars/`.

### Fase 4: Catálogo de Plataformas, Validação e Reordenação de Links

- [ ] Criar `PlatformRegistry` no Core contendo as 14 plataformas oficiais com nome, cor e regex.
- [ ] Implementar `PlatformValidator` validando URLs conforme a plataforma selecionada.
- [ ] Criar entidade `UserLink` e configuração fluente com índice composto em `(UserId, DisplayOrder)`.
- [ ] Criar endpoints em `LinkEndpoints`:
  - [ ] `GET /api/links` (listar links do usuário logado ordenados por `DisplayOrder`).
  - [ ] `PUT /api/links` (sincronização atômica em lote com suporte a _drag-and-drop_).
  - [ ] `DELETE /api/links/{id}` (excluir link individual do usuário autenticado).
- [ ] Criar e aplicar migrações do EF Core (`dotnet ef migrations add InitialCreate`).

### Fase 5: Visualização Pública e Compartilhamento (Preview)

- [ ] Criar endpoint `GET /api/public/{username}` em `PublicEndpoints`.
- [ ] Projetar perfil público consolidado com lista de links ordenados.
- [ ] Implementar verificação contextual: marcar `isOwner = true` se o chamador for o dono autenticado.
- [ ] Otimizar consulta com `AsNoTracking()` para máxima velocidade.

### Fase 6: Testes Automatizados

- [ ] **Testes Unitários:**
  - [ ] Validação com `[Theory]` para todas as 14 plataformas no `PlatformValidatorTests`.
  - [ ] Testes da entidade `UserProfile` (obrigatoriedade de nomes, sanitização).
  - [ ] Testes de algoritmo de reordenação e normalização de `DisplayOrder`.
- [ ] **Testes de Integração:**
  - [ ] Configurar suíte com `WebApplicationFactory<Program>` e `Testcontainers.PostgreSql`.
  - [ ] Testar fluxo completo: registro -> login -> atualizar perfil -> salvar links em lote -> consultar perfil público.
- [ ] Garantir 100% de sucesso e cobertura executando `dotnet test`.

### Fase 7: Containerização e CI/CD

- [ ] Criar `Dockerfile` multi-stage otimizado na pasta `src/DevLinks.Api`.
- [ ] Configurar `docker-compose.yml` integrando API + PostgreSQL com volume de uploads e Health Checks.
- [ ] Testar execução completa do ambiente local via `docker compose up --build`.
- [ ] Criar workflow do GitHub Actions em `.github/workflows/ci.yml` (restore, format check, build, testcontainers test).
- [ ] Configurar documentação interativa com Scalar (`/scalar/v1`).

### Fase 8: Integração com o Frontend do Frontend Mentor e Publicação

- [ ] Conectar o frontend do desafio (React / Next.js) aos endpoints da API .NET.
- [ ] Validar a sincronização em tempo real do _mockup_ de smartphone e reordenação _drag-and-drop_.
- [ ] Validar o fluxo de cópia do link com toast na tela de Preview.
- [ ] Atualizar o `README.md` principal na raiz com capturas de tela, diagrama de arquitetura, instruções de execução via Docker e documentação dos endpoints.
- [ ] Publicar no GitHub e submeter a solução na plataforma [Frontend Mentor](https://www.frontendmentor.io/challenges/linksharing-app-Fbt7yweGsT).
