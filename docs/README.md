# Documentação Técnica — devlinks (Link-Sharing App)

Bem-vindo à documentação oficial de requisitos, arquitetura e engenharia do projeto **devlinks (Link-Sharing App)**, concebido com base no desafio avançado (_Guru_) do [Frontend Mentor](https://www.frontendmentor.io/challenges/linksharing-app-Fbt7yweGsT).

Este projeto é desenvolvido como uma aplicação **Full-Stack de alto nível**, contendo um backend robusto em **C# / .NET 9 (Minimal APIs)**, persistência com **PostgreSQL** e **EF Core**, autenticação completa via **ASP.NET Core Identity & JWT**, upload e processamento de imagem de avatar, catálogo de validação de plataformas de desenvolvedores e suíte de testes com **Testcontainers**.

---

## 🗺️ Mapa da Documentação

A documentação está estruturada em artefatos modulares que cobrem todas as fases do projeto:

| Documento                                                                                  | Descrição                                                                                                                                             |
| :----------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------- |
| [docs/01-visao-e-escopo.md](docs/01-visao-e-escopo.md)                                     | Visão do produto, alinhamento com o desafio Frontend Mentor, objetivos técnicos, personas e limites do escopo.                                        |
| [docs/02-requisitos-e-regras-de-negocio.md](docs/02-requisitos-e-regras-de-negocio.md)     | Requisitos Funcionais (RF), Regras de Negócio (RN), validação de URLs por plataforma, regras de avatar e Requisitos Não Funcionais (RNF).             |
| [docs/03-arquitetura-e-design-tecnico.md](docs/03-arquitetura-e-design-tecnico.md)         | Arquitetura de software (.NET Minimal APIs), pipeline de autenticação JWT, estratégia de upload de imagens, catálogo de plataformas e resiliência.    |
| [docs/04-modelo-de-dados.md](docs/04-modelo-de-dados.md)                                   | Modelagem relacional PostgreSQL (`Users`, `Profiles`, `PlatformLinks`), mapeamento fluente com EF Core, índices e regras de integridade.              |
| [docs/05-especificacao-de-endpoints.md](docs/05-especificacao-de-endpoints.md)             | Contratos REST/OpenAPI, DTOs em C# (`sealed record`), endpoints de Auth, Links (batch/reorder), Perfil, Upload de Avatar e Perfil Público (RFC 7807). |
| [docs/06-qualidade-testes-e-devops.md](docs/06-qualidade-testes-e-devops.md)               | Estratégia de testes automatizados (Unitários xUnit + FluentAssertions, Integração com Testcontainers PostgreSQL), Dockerfile e GitHub Actions CI.    |
| [docs/07-roadmap-e-plano-de-implementacao.md](docs/07-roadmap-e-plano-de-implementacao.md) | Roteiro prático passo a passo de desenvolvimento, comandos da .NET CLI, governança do GitHub e checklist de tarefas por fase.                         |

---

## 🚀 Resumo Executivo do Projeto

- **Domínio da Aplicação:** Agregação e compartilhamento de links sociais de desenvolvedores (_devlinks_), personalização de perfil, upload de imagem de avatar, ordenação flexível (_drag-and-drop_) e página pública de perfil.
- **Linguagem & Framework:** C# 13 no .NET 9 (ASP.NET Core Minimal APIs).
- **Autenticação & Segurança:** ASP.NET Core Identity com geração e validação de tokens JWT (Bearer), armazenamento seguro de senhas com hashing criptográfico e proteção de endpoints privados.
- **Banco de Dados Relacional:** PostgreSQL 16+ gerenciado via Entity Framework Core 9 (Code-First com Migrations).
- **Validação de Plataformas:** Verificação estrita de formato de URL para 14+ plataformas (GitHub, YouTube, LinkedIn, Frontend Mentor, Twitter/X, Dev.to, Codewars, etc.).
- **Upload de Mídia:** Serviço para upload e validação de avatar (formato JPG/PNG, limite de resolução e tamanho).
- **Reordenação de Links:** Ordenação atômica em lote no salvamento, permitindo reordenação fluida por _drag-and-drop_.
- **Qualidade & DevOps:** Testes automatizados reais com `WebApplicationFactory` e `Testcontainers.PostgreSql`, containerização com Docker multi-stage e pipeline de CI no GitHub Actions.
