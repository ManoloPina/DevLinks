# 01. Visão e Escopo do Projeto

## 1. Contexto e Motivação

O projeto **devlinks** é uma aplicação completa de compartilhamento e agregação de links (_Link-sharing app_), inspirada no desafio de nível **Guru** do [Frontend Mentor](https://www.frontendmentor.io/challenges/linksharing-app-Fbt7yweGsT).

Diferente de um encurtador de links simples, o **devlinks** funciona como uma plataforma personalizada (no estilo _Linktree_ para desenvolvedores), na qual cada profissional pode:

- Centralizar sua presença online (GitHub, LinkedIn, YouTube, Frontend Mentor, Dev.to, Twitter/X, etc.).
- Personalizar seu perfil com foto de avatar, nome, sobrenome e e-mail de contato público.
- Reordenar dinamicamente seus links via interface intuitiva (_drag-and-drop_).
- Visualizar um _mockup_ de smartphone atualizado em tempo real enquanto edita seus dados.
- Gerar e compartilhar uma página pública de perfil única para divulgar em redes sociais e currículos.

Para elevar o projeto ao patamar de solução _Full-Stack_ pronta para produção e portfólio de engenharia de software, o backend foi concebido em **C# / .NET 9**, implementando persistência relacional com **PostgreSQL**, autenticação completa com **ASP.NET Core Identity & JWT**, upload e validação de imagens, testes automatizados ponta a ponta e esteira de CI/CD via Docker e GitHub Actions.

---

## 2. Objetivos do Projeto

### 2.1 Objetivos Técnicos

1. **Desenvolver uma API RESTful Moderna com ASP.NET Core Minimal APIs:** Criar endpoints concisos, fortemente tipados (`TypedResults`), estruturados por recursos e documentados via OpenAPI / Scalar.
2. **Implementar Autenticação & Autorização Segura:** Utilizar ASP.NET Core Identity com autenticação baseada em tokens JWT (_Bearer tokens_), senhas protegidas com hashing criptográfico e isolamento rigoroso de recursos por usuário autenticado.
3. **Persistência Relacional Avançada com EF Core & PostgreSQL:** Modelar entidades e relacionamentos (`User`, `Profile`, `PlatformLink`), mapeamentos via Fluent API, índices de consulta rápida e migrações versionadas.
4. **Validação Especializada por Plataforma:** Criar um mecanismo robusto no backend para validar formatos de URLs específicas de acordo com a plataforma selecionada (ex: `https://github.com/:username`, `https://www.frontendmentor.io/profile/:username`).
5. **Gerenciamento de Mídia e Upload de Imagens:** Suportar upload, validação de tipos MIME, dimensões e tamanho de foto de perfil (avatar).
6. **Testes Automatizados com Testcontainers:** Garantir a confiabilidade da aplicação através de testes unitários (regras de validação e ordenação) e testes de integração contra instâncias reais do PostgreSQL efêmeras.
7. **Containerização e DevOps:** Empacotar a aplicação via Dockerfile multi-stage, Docker Compose para desenvolvimento local e workflow de CI no GitHub Actions.

### 2.2 Objetivos de Negócio / Produto

- Proporcionar uma experiência impecável de agregação de links voltada a desenvolvedores e criadores de conteúdo.
- Garantir que apenas o dono do perfil possa editar ou excluir links e alterar suas fotos.
- Disponibilizar uma rota pública rápida e responsiva para visitantes acessarem os links do desenvolvedor.
- Permitir cópia rápida do link público com um clique para a área de transferência (_Share Link_).

---

## 3. Escopo da Aplicação (Full-Stack & Backend)

```
┌────────────────────────────────────────────────────────────────────────┐
│ CAMADA VISUAL (Frontend: React / Next.js / TypeScript)                  │
│ • Tabs: "Links" & "Profile Details"                                    │
│ • Mockup de celular interativo em tempo real                           │
│ • Drag-and-drop de links (= Link #1, #2...)                            │
│ • Dropdown de plataformas com ícones e cores temáticas                 │
│ • Upload interativo de avatar com pré-visualização instantânea         │
│ • Tela de Preview público com botão "Copy Link" / Toast                │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ HTTP REST / JSON / Multipart
┌───────────────────────────────────▼────────────────────────────────────┐
│ CAMADA DE BACKEND (ASP.NET Core Minimal APIs .NET 9)                   │
│ • Autenticação & Gestão de Contas (Register, Login, Me)                │
│ • CRUD e Reordenação em Lote de Links (/api/links)                     │
│ • Perfil & Upload de Avatar (/api/profile, /api/profile/avatar)        │
│ • Endpoint Público de Compartilhamento (/api/public/{username})        │
│ • RFC 7807 ProblemDetails para tratamento padronizado de erros         │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ PERSISTÊNCIA & STORAGE                                                 │
│ • PostgreSQL 16+ (Users, Profiles, PlatformLinks)                      │
│ • Storage para Imagens (Local File Storage / Cloudinary / S3)          │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Matriz de Escopo (Dentro vs. Fora do Escopo)

### 4.1 O que ESTÁ Dentro do Escopo (In-Scope)

- **Autenticação & Contas:**
  - Cadastro de novo usuário (`POST /api/auth/register`) com e-mail e senha segura.
  - Autenticação e emissão de JWT (`POST /api/auth/login`).
  - Obtenção dos dados do usuário logado (`GET /api/auth/me`).
- **Gestão de Links:**
  - Consulta de links do usuário autenticado (`GET /api/links`).
  - Salvamento e reordenação em lote dos links (`PUT /api/links`), atualizando de forma atômica posições (`DisplayOrder`), plataformas e URLs.
  - Exclusão individual de links (`DELETE /api/links/{id}`).
  - Validação de URL específica por plataforma (GitHub, LinkedIn, YouTube, Frontend Mentor, Twitter/X, Dev.to, Codewars, etc.).
- **Gestão de Perfil:**
  - Consulta de dados de perfil (`GET /api/profile`).
  - Atualização de nome, sobrenome e e-mail público de exibição (`PUT /api/profile`).
  - Upload de foto de perfil (`POST /api/profile/avatar`) com validação de formato (JPG/PNG) e tamanho máximo.
- **Visualização Pública & Compartilhamento:**
  - Consulta pública de perfil e links (`GET /api/public/{username}`).
  - Se visualizado pelo proprietário logado: exibe ações administrativas ("Back to Editor" e "Share Link").
  - Se visualizado por visitantes anônimos: exibe apenas os dados públicos e botão de copiar link, sem controles de edição.
- **Qualidade, Infraestrutura & Segurança:**
  - Testes unitários para regras de domínio e validadores de plataformas.
  - Testes de integração automatizados com PostgreSQL via Testcontainers.
  - Tratamento global de erros em conformidade com **RFC 7807 (Problem Details)**.
  - Orquestração de ambiente com **Docker Compose**.
  - Esteira de CI via **GitHub Actions**.

### 4.2 O que NÃO ESTÁ no Escopo Inicial (Out-of-Scope)

- Redirecionamento por códigos hash curtos tipo Bitly (este projeto foca na agregação e exibição visual de links em página de perfil).
- Integração com OAuth2 de terceiros (Google/GitHub Login) na primeira versão (pode ser adicionado como evolução futura).
- Sistema de métricas avançadas em tempo real com mapa de calor de cliques geolocalizados.
- Monetização ou planos pagos (_Devlinks Pro_).

---

## 5. Perfis de Usuário (Personas / Atores)

| Perfil                                          | Descrição                                                                          | Ações Típicas                                                                                                                                    |
| :---------------------------------------------- | :--------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
| **Desenvolvedor / Dono da Conta (Autenticado)** | Usuário que criou conta na plataforma para gerenciar seus links e perfil.          | Cadastra-se, faz login, cadastra/edita/reordena links, faz upload do avatar, ajusta nome e copia seu link público para divulgação.               |
| **Visitante Público / Recrutador**              | Qualquer pessoa que acessa o link público compartilhado pelo desenvolvedor.        | Acessa a página pública (`/preview/{username}`), visualiza o avatar, nome e clica nos botões estilizados para visitar os links do desenvolvedor. |
| **Administrador / Sistema DevOps**              | Responsável por manter a aplicação rodando com alta disponibilidade e integridade. | Monitora logs estruturados, verifica o endpoint de health check (`/healthz`) e gerencia a esteira de CI/CD.                                      |

---

## 6. Premissas e Restrições Técnicas

### 6.1 Premissas

- Toda data/hora registrada pelo backend adota estritamente o fuso horário **UTC** (`DateTimeOffset`).
- O identificador público para a URL de compartilhamento é o `username` único ou identificador do usuário.
- O usuário deve poder registrar múltiplos links, mas cada link deve respeitar a validação de formato exigida para a plataforma escolhida.
- O campo `DisplayOrder` deve manter os links organizados exatamente na ordem definida pelo usuário no _drag-and-drop_.

### 6.2 Restrições Técnicas

- **Linguagem & Framework:** C# 13 e ASP.NET Core 9 Minimal APIs.
- **Segurança de Autenticação:** JWT com chave simétrica HMAC-SHA256 de no mínimo 256 bits, tempo de expiração configurável e hashing de senhas via ASP.NET Core Identity.
- **Integridade de Dados:** O banco de dados PostgreSQL deve impor restrição de chave única no e-mail e no nome de usuário (`username`), além de cascade delete dos links ao remover uma conta.
- **Validação de Uploads:** Somente arquivos de imagem (`image/jpeg`, `image/png`) com tamanho máximo de 2MB e resolução recomendada de até 1024x1024 pixels.
- **Padrão de Erros:** Todas as respostas de erro de cliente (4xx) e servidor (5xx) devem seguir a RFC 7807 (`application/problem+json`).
