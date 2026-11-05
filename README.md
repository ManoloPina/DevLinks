# devlinks (Link-Sharing App) — Full-Stack .NET 9 & React

Aplicação moderna de compartilhamento e agregação de perfis sociais para desenvolvedores (**devlinks**), baseada no desafio de nível _Guru_ do [Frontend Mentor](https://www.frontendmentor.io/challenges/linksharing-app-Fbt7yweGsT).

O projeto conta com um backend robusto em **C#** e **.NET 9**, arquitetura limpa com Minimal APIs, autenticação **ASP.NET Core Identity & JWT**, persistência relacional em **PostgreSQL**, upload e validação de avatar, catálogo com validação estrita de URLs para 14 plataformas, testes automatizados reais com **Testcontainers** e containerização com **Docker Compose**.

---

## 📚 Documentação Técnica Completa

Toda a engenharia de requisitos, design de software, diagramas e especificações está documentada na pasta [docs/](docs/README.md):

- [docs/01-visao-e-escopo.md](docs/01-visao-e-escopo.md) — Visão do produto, objetivos, personas e limites do escopo.
- [docs/02-requisitos-e-regras-de-negocio.md](docs/02-requisitos-e-regras-de-negocio.md) — Requisitos Funcionais, Regras de Negócio (validação regex de 14 plataformas, upload de avatar) e RNF.
- [docs/03-arquitetura-e-design-tecnico.md](docs/03-arquitetura-e-design-tecnico.md) — Arquitetura de software, pipeline JWT, catálogo de plataformas, storage de mídia e RFC 7807.
- [docs/04-modelo-de-dados.md](docs/04-modelo-de-dados.md) — Modelagem relacional PostgreSQL (`Users`, `Profiles`, `PlatformLinks`), Fluent API e migrações.
- [docs/05-especificacao-de-endpoints.md](docs/05-especificacao-de-endpoints.md) — Contratos REST, DTOs (`sealed record`), endpoints de Auth, Links, Perfil, Upload e Preview público.
- [docs/06-qualidade-testes-e-devops.md](docs/06-qualidade-testes-e-devops.md) — Suíte de testes (xUnit + Testcontainers PostgreSQL), Dockerfile e GitHub Actions CI.
- [docs/07-roadmap-e-plano-de-implementacao.md](docs/07-roadmap-e-plano-de-implementacao.md) — Guia de comandos CLI, governança do GitHub e checklist de tarefas por fase.

---

## 🛠️ Stack Tecnológica

- **Linguagem:** C# 13
- **Framework Web:** .NET 9 (ASP.NET Core Minimal APIs)
- **Autenticação & Segurança:** ASP.NET Core Identity + JWT Bearer Tokens
- **Banco de Dados Relacional:** PostgreSQL 16+ com Entity Framework Core 9 (Code-First & Migrations)
- **Documentação de API:** OpenAPI nativo + Scalar (`/scalar/v1`)
- **Testes Automatizados:** xUnit, FluentAssertions, `WebApplicationFactory<Program>` e `Testcontainers.PostgreSql`
- **Containerização & DevOps:** Docker, Docker Compose e GitHub Actions CI
