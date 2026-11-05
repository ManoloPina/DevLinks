# 05. Especificação de Endpoints da API

Este documento define os contratos de interface REST, formatos de dados (DTOs), códigos de status HTTP e o modelo de tratamento de erros da API **devlinks (Link-Sharing App)**.

---

## 1. Convenções Globais e Padrões de API

- **Formato de Dados:** Todos os payloads utilizam `Content-Type: application/json; charset=utf-8` (exceto uploads multipart em `POST /api/profile/avatar`).
- **Autenticação:** Endpoints protegidos exigem o cabeçalho HTTP:
  ```http
  Authorization: Bearer <seu-jwt-token>
  ```
- **Formato de Erro:** Em conformidade estrita com o padrão **RFC 7807 (Problem Details)** com `Content-Type: application/problem+json`.
- **Tratamento de Datas:** Todas as propriedades temporais utilizam o padrão **ISO 8601** em UTC com sufixo `Z` (`yyyy-MM-ddTHH:mm:ss.fffZ`), representadas no C# por `DateTimeOffset`.
- **Definição de DTOs:** Todos os DTOs são modelados como `sealed record` em C# 13, garantindo imutabilidade e alta performance.

---

## 2. Modelos de Entrada e Saída (DTOs C#)

```csharp
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Api.Common.DTOs;

// --- DTOs de Autenticação ---
public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, Compare(nameof(Password))] string ConfirmPassword);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string UserName,
    string Token,
    int ExpiresInSeconds);

public sealed record UserSummaryResponse(
    Guid UserId,
    string Email,
    string UserName);

// --- DTOs de Links ---
public sealed record LinkItemDto(
    Guid? Id,
    [Required] string Platform,
    [Required, Url] string Url,
    [Range(0, 100)] int DisplayOrder);

public sealed record BatchSaveLinksRequest(
    [Required] List<LinkItemDto> Links);

public sealed record UserLinkResponse(
    Guid Id,
    string Platform,
    string Url,
    int DisplayOrder,
    DateTimeOffset CreatedAtUtc);

// --- DTOs de Perfil ---
public sealed record UpdateProfileRequest(
    [Required, MinLength(1), MaxLength(100)] string FirstName,
    [Required, MinLength(1), MaxLength(100)] string LastName,
    [EmailAddress, MaxLength(256)] string? DisplayEmail);

public sealed record ProfileResponse(
    string FirstName,
    string LastName,
    string? DisplayEmail,
    string? AvatarUrl,
    DateTimeOffset UpdatedAtUtc);

public sealed record AvatarUploadResponse(
    string AvatarUrl);

// --- DTOs de Perfil Público (Preview / Sharing) ---
public sealed record PublicProfileResponse(
    string UserName,
    string FirstName,
    string LastName,
    string? DisplayEmail,
    string? AvatarUrl,
    bool IsOwner,
    IReadOnlyList<UserLinkResponse> Links);
```

---

## 3. Catálogo de Endpoints

### 3.1 Módulo de Autenticação (`/api/auth`)

#### `POST /api/auth/register` — Criar Conta

- **Descrição:** Registra um novo usuário no sistema e cria automaticamente seu registro de perfil em branco.
- **Autenticação:** Pública.
- **Requisição:**
  ```json
  {
    "email": "ben@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }
  ```
- **Respostas:**
  - `201 Created`:
    ```json
    {
      "userId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "email": "ben@example.com",
      "userName": "ben",
      "token": "eyJhbGciOiJIUzI1NiIs...",
      "expiresInSeconds": 86400
    }
    ```
  - `400 Bad Request`: Validação inválida ou senhas não conferem.
  - `409 Conflict`: O e-mail informado já está em uso.

#### `POST /api/auth/login` — Autenticar Usuário

- **Descrição:** Valida as credenciais e retorna o token JWT.
- **Autenticação:** Pública.
- **Requisição:**
  ```json
  {
    "email": "ben@example.com",
    "password": "Password123!"
  }
  ```
- **Respostas:**
  - `200 OK`: Payload idêntico ao `AuthResponse`.
  - `401 Unauthorized`: Credenciais inválidas.

#### `GET /api/auth/me` — Obter Dados do Usuário Autenticado

- **Descrição:** Retorna os dados básicos da conta do token atual.
- **Autenticação:** Requer Bearer JWT.
- **Respostas:**
  - `200 OK`: `{ "userId": "...", "email": "ben@example.com", "userName": "ben" }`
  - `401 Unauthorized`: Token ausente ou inválido.

---

### 3.2 Módulo de Links (`/api/links`)

#### `GET /api/links` — Listar Links do Usuário

- **Descrição:** Retorna todos os links do usuário logado ordenados por `displayOrder` ascendente.
- **Autenticação:** Requer Bearer JWT.
- **Respostas:**
  - `200 OK`:
    ```json
    [
      {
        "id": "e81d11cb-469b-4394-a957-c03561ec9d62",
        "platform": "github",
        "url": "https://github.com/benwright",
        "displayOrder": 0,
        "createdAtUtc": "2026-11-04T10:00:00Z"
      },
      {
        "id": "f92e22dc-570c-5405-b068-d14672fd0e73",
        "platform": "youtube",
        "url": "https://www.youtube.com/@benwright",
        "displayOrder": 1,
        "createdAtUtc": "2026-11-04T10:05:00Z"
      }
    ]
    ```

#### `PUT /api/links` — Salvar e Reordenar Links em Lote

- **Descrição:** Sincroniza a coleção completa de links (adiciona novos, atualiza existentes, reordena posições e remove ausentes) em uma única transação atômica.
- **Autenticação:** Requer Bearer JWT.
- **Requisição:**
  ```json
  {
    "links": [
      {
        "id": "e81d11cb-469b-4394-a957-c03561ec9d62",
        "platform": "github",
        "url": "https://github.com/benwright",
        "displayOrder": 0
      },
      {
        "id": null,
        "platform": "linkedin",
        "url": "https://www.linkedin.com/in/benwright",
        "displayOrder": 1
      }
    ]
  }
  ```
- **Respostas:**
  - `200 OK`: Retorna a lista completa atualizada e ordenada.
  - `400 Bad Request`: Se alguma URL não obedecer ao padrão regex da sua plataforma (RFC 7807).

#### `DELETE /api/links/{id}` — Excluir Link Individual

- **Descrição:** Remove um link específico pertencente ao usuário autenticado.
- **Autenticação:** Requer Bearer JWT.
- **Respostas:**
  - `204 No Content`: Excluído com sucesso.
  - `404 Not Found`: Link não existe ou pertence a outro usuário.

---

### 3.3 Módulo de Perfil (`/api/profile`)

#### `GET /api/profile` — Consultar Detalhes do Perfil

- **Descrição:** Retorna os dados do perfil do usuário logado.
- **Autenticação:** Requer Bearer JWT.
- **Respostas:**
  - `200 OK`:
    ```json
    {
      "firstName": "Ben",
      "lastName": "Wright",
      "displayEmail": "ben@example.com",
      "avatarUrl": "/uploads/avatars/avatar-ben.png",
      "updatedAtUtc": "2026-11-04T11:00:00Z"
    }
    ```

#### `PUT /api/profile` — Atualizar Detalhes do Perfil

- **Descrição:** Atualiza primeiro nome, sobrenome e e-mail de contato público.
- **Autenticação:** Requer Bearer JWT.
- **Requisição:**
  ```json
  {
    "firstName": "Ben",
    "lastName": "Wright",
    "displayEmail": "ben.public@example.com"
  }
  ```
- **Respostas:**
  - `200 OK`: Perfil atualizado.
  - `400 Bad Request`: `firstName` ou `lastName` vazios.

#### `POST /api/profile/avatar` — Upload de Imagem de Avatar

- **Descrição:** Processa e salva a imagem do avatar (JPG ou PNG até 2MB).
- **Autenticação:** Requer Bearer JWT.
- **Content-Type:** `multipart/form-data`
- **Campo do Formulário:** `file` (arquivo binário).
- **Respostas:**
  - `200 OK`:
    ```json
    {
      "avatarUrl": "/uploads/avatars/4f17c2f1-5e92-4f9e-bdf1-e8d7a12c3b4e.png"
    }
    ```
  - `400 Bad Request`: Arquivo maior que 2MB ou formato não suportado.

---

### 3.4 Módulo de Visualização Pública (`/api/public`)

#### `GET /api/public/{username}` — Obter Perfil Público para Compartilhamento

- **Descrição:** Endpoint público utilizado pela tela de _Preview_ e por visitantes anônimos externos. Se o visitante enviar o cabeçalho `Authorization` com o token do dono do perfil, a propriedade `isOwner` retorna `true`, instruindo o frontend a exibir os botões "Back to Editor" e "Share Link".
- **Autenticação:** Opcional (Pública).
- **Parâmetros:** `username` (string via path).
- **Respostas:**
  - `200 OK`:
    ```json
    {
      "userName": "ben",
      "firstName": "Ben",
      "lastName": "Wright",
      "displayEmail": "ben.public@example.com",
      "avatarUrl": "/uploads/avatars/4f17c2f1-5e92-4f9e-bdf1-e8d7a12c3b4e.png",
      "isOwner": false,
      "links": [
        {
          "id": "e81d11cb-469b-4394-a957-c03561ec9d62",
          "platform": "github",
          "url": "https://github.com/benwright",
          "displayOrder": 0,
          "createdAtUtc": "2026-11-04T10:00:00Z"
        },
        {
          "id": "f92e22dc-570c-5405-b068-d14672fd0e73",
          "platform": "youtube",
          "url": "https://www.youtube.com/@benwright",
          "displayOrder": 1,
          "createdAtUtc": "2026-11-04T10:05:00Z"
        }
      ]
    }
    ```
  - `404 Not Found`: Usuário não encontrado.

---

### 3.5 Monitoramento e Saúde (`/healthz`)

- **Método:** `GET /healthz`
- **Respostas:** `200 OK` (`{ "status": "Healthy" }`) ou `503 Service Unavailable`.

---

## 4. Padronização de Erros (RFC 7807 ProblemDetails)

Todas as respostas de erro obedecem ao padrão `application/problem+json`:

### Exemplo 1: URL Inválida para a Plataforma (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Falha na validação dos links submetidos.",
  "instance": "/api/links",
  "errors": {
    "Links[0].Url": [
      "A URL informada não corresponde ao padrão exigido para a plataforma 'github'. Exemplo esperado: https://github.com/username"
    ]
  }
}
```

### Exemplo 2: Primeiro Nome Ausente (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Dados de perfil incompletos.",
  "instance": "/api/profile",
  "errors": {
    "FirstName": ["O primeiro nome é obrigatório para salvar o perfil."]
  }
}
```

### Exemplo 3: Conflito de E-mail no Cadastro (409 Conflict)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Já existe uma conta cadastrada com o e-mail informado.",
  "instance": "/api/auth/register"
}
```

---

## 5. Arquivo de Requisições de Teste (`tests/requests.http`)

Para testar o fluxo completo via extensão REST Client do VS Code:

```http
@baseUrl = http://localhost:5000

### 1. Registrar Novo Usuário
# @name register
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
  "email": "dev@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}

### 2. Efetuar Login
# @name login
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "dev@example.com",
  "password": "Password123!"
}

@authToken = {{login.response.body.token}}

### 3. Obter Usuário Atual
GET {{baseUrl}}/api/auth/me
Authorization: Bearer {{authToken}}

### 4. Atualizar Dados do Perfil
PUT {{baseUrl}}/api/profile
Authorization: Bearer {{authToken}}
Content-Type: application/json

{
  "firstName": "Ben",
  "lastName": "Wright",
  "displayEmail": "ben.wright@example.com"
}

### 5. Consultar Dados do Perfil
GET {{baseUrl}}/api/profile
Authorization: Bearer {{authToken}}

### 6. Salvar Links em Lote (com Reordenação)
PUT {{baseUrl}}/api/links
Authorization: Bearer {{authToken}}
Content-Type: application/json

{
  "links": [
    {
      "platform": "github",
      "url": "https://github.com/benwright",
      "displayOrder": 0
    },
    {
      "platform": "youtube",
      "url": "https://www.youtube.com/@benwright",
      "displayOrder": 1
    },
    {
      "platform": "linkedin",
      "url": "https://www.linkedin.com/in/benwright",
      "displayOrder": 2
    }
  ]
}

### 7. Listar Links do Usuário
GET {{baseUrl}}/api/links
Authorization: Bearer {{authToken}}

### 8. Consultar Perfil Público (Preview Compartilhável)
GET {{baseUrl}}/api/public/dev

### 9. Health Check
GET {{baseUrl}}/healthz
```
