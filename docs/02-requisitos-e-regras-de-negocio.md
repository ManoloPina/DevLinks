# 02. Requisitos e Regras de Negócio

Este documento detalha os Requisitos Funcionais (RF), as Regras de Negócio (RN) e os Requisitos Não Funcionais (RNF) que governam o comportamento e a arquitetura do projeto **devlinks (Link-Sharing App)**, em conformidade com o desafio de nível _Guru_ do Frontend Mentor.

---

## 1. Requisitos Funcionais (RF)

### RF-001: Autenticação de Usuário (Registro e Login)

- **Descrição:** O sistema deve permitir que novos usuários criem uma conta com e-mail e senha, façam login para obter um token JWT e consultem o usuário autenticado atual.
- **Entrada:**
  - Registro: `email`, `password`, `confirmPassword`.
  - Login: `email`, `password`.
- **Saída:** Token JWT (_Bearer_), data de expiração e dados básicos da conta.
- **Critérios de Aceitação:**
  - **Cenário 1 (Registro com sucesso):**
    - _Dado_ que o usuário submete um e-mail válido ainda não cadastrado e uma senha com no mínimo 8 caracteres.
    - _Quando_ a requisição `POST /api/auth/register` for executada.
    - _Então_ o status retornado deve ser `201 Created` e um registro correspondente em `Users` e `Profiles` deve ser criado.
  - **Cenário 2 (Tentativa de registro com e-mail duplicado):**
    - _Dado_ que o e-mail informado já existe na base.
    - _Quando_ o endpoint de registro for acionado.
    - _Então_ deve responder com `409 Conflict` ou `400 Bad Request` padronizado em RFC 7807 indicando duplicidade.
  - **Cenário 3 (Login com credenciais corretas):**
    - _Dado_ que o usuário informa credenciais válidas.
    - _Quando_ `POST /api/auth/login` for acionado.
    - _Então_ retorna `200 OK` com o token JWT e tempo de vida em segundos.
  - **Cenário 4 (Login com credenciais inválidas):**
    - _Dado_ e-mail inexistente ou senha incorreta.
    - _Quando_ `POST /api/auth/login` for executado.
    - _Então_ retorna `401 Unauthorized` sem vazar se o e-mail existe.

### RF-002: Gestão de Links do Usuário (CRUD e Reordenação em Lote)

- **Descrição:** O usuário autenticado deve ser capaz de listar seus links atuais e salvar alterações em lote (adicionar novos links, atualizar plataformas/URLs existentes, reordenar a sequência de exibição e remover links).
- **Entrada:** Lista de links com `id` (opcional se for novo), `platform` (enum ou identificador da plataforma), `url` (string) e `displayOrder` (inteiro baseado em 0).
- **Saída:** Coleção atualizada e ordenada dos links persistidos.
- **Critérios de Aceitação:**
  - **Cenário 1 (Salvamento em lote com reordenação):**
    - _Dado_ um usuário autenticado que reordenou 3 links via _drag-and-drop_ e alterou uma URL.
    - _Quando_ submeter `PUT /api/links`.
    - _Então_ o sistema persiste todas as alterações de forma transacional e retorna `200 OK` com a lista atualizada e ordenada por `displayOrder`.
  - **Cenário 2 (Exclusão individual de link):**
    - _Dado_ um link pertencente ao usuário autenticado.
    - _Quando_ for enviado `DELETE /api/links/{id}`.
    - _Então_ retorna `204 No Content` e o link é removido do banco de dados.
  - **Cenário 3 (Tentativa de alterar/excluir link de outro usuário):**
    - _Dado_ que o usuário tenta excluir um link associado a outro `UserId`.
    - _Quando_ a requisição for processada.
    - _Então_ retorna `404 Not Found` ou `403 Forbidden`.

### RF-003: Validação Estrita de URLs por Plataforma

- **Descrição:** Ao salvar links, o sistema deve validar se a URL fornecida é válida e atende ao padrão de expressão regular (_regex_) específico da plataforma escolhida (ex: perfil do GitHub, canal/vídeo do YouTube, perfil do LinkedIn, etc.).
- **Critérios de Aceitação:**
  - **Cenário 1 (URL válida para a plataforma):**
    - _Dado_ a plataforma `GitHub` com a URL `https://github.com/usuario-exemplo`.
    - _Quando_ for submetida.
    - _Então_ a validação é aprovada.
  - **Cenário 2 (URL com padrão incorreto para a plataforma):**
    - _Dado_ a plataforma `Frontend Mentor` com a URL informada `https://facebook.com/teste`.
    - _Quando_ for submetida.
    - _Então_ o sistema retorna `400 Bad Request` (RFC 7807) detalhando que a URL informada não corresponde ao padrão exigido pela plataforma Frontend Mentor.

### RF-004: Gestão de Detalhes de Perfil

- **Descrição:** O usuário autenticado pode consultar e atualizar seus dados pessoais de perfil: Primeiro Nome (`firstName`), Sobrenome (`lastName`) e E-mail Público de exibição (`displayEmail`).
- **Entrada:** `firstName` (obrigatório), `lastName` (obrigatório), `displayEmail` (opcional, formato de e-mail).
- **Saída:** Objeto contendo os dados do perfil atualizados.
- **Critérios de Aceitação:**
  - **Cenário 1 (Campos obrigatórios preenchidos):**
    - _Dado_ que `firstName = "Ben"` e `lastName = "Wright"`.
    - _Quando_ `PUT /api/profile` for submetido.
    - _Então_ o perfil é atualizado e o endpoint retorna `200 OK`.
  - **Cenário 2 (Campo obrigatório ausente):**
    - _Dado_ que o usuário submete `firstName` vazio.
    - _Quando_ `PUT /api/profile` for acionado.
    - _Então_ retorna `400 Bad Request` indicando que o primeiro nome não pode ficar vazio.

### RF-005: Upload e Armazenamento de Avatar de Perfil

- **Descrição:** O usuário autenticado pode enviar uma foto de avatar para ilustrar seu perfil no cabeçalho e na pré-visualização.
- **Entrada:** Arquivo binário multipart/form-data (`file`).
- **Saída:** URL pública acessível da imagem salva (`avatarUrl`).
- **Critérios de Aceitação:**
  - **Cenário 1 (Upload válido):**
    - _Dado_ uma imagem PNG de 300x300 pixels com 400KB.
    - _Quando_ enviada via `POST /api/profile/avatar`.
    - _Então_ o sistema armazena a imagem, atualiza o campo `AvatarUrl` do perfil e responde com `200 OK` contendo a URL gerada.
  - **Cenário 2 (Arquivo com tamanho excessivo ou tipo proibido):**
    - _Dado_ um arquivo de 5MB ou um arquivo `.pdf` renomeado.
    - _Quando_ enviado para o endpoint de avatar.
    - _Então_ o sistema rejeita com `400 Bad Request` informando as restrições permitidas (JPG/PNG até 2MB).

### RF-006: Visualização de Perfil Público e Compartilhamento (Preview)

- **Descrição:** O sistema deve disponibilizar um endpoint público para exibição da página de links do usuário (`/api/public/{username}`), consumida pela tela de pré-visualização e pelos visitantes externos.
- **Entrada:** `username` via path da requisição.
- **Saída:** Dados consolidados do perfil (`firstName`, `lastName`, `displayEmail`, `avatarUrl`) e lista ordenada de links (`platform`, `url`, `displayOrder`).
- **Critérios de Aceitação:**
  - **Cenário 1 (Usuário existente):**
    - _Dado_ um username cadastrado com links ativos.
    - _Quando_ `GET /api/public/{username}` for consultado sem cabeçalho de autenticação.
    - _Então_ retorna `200 OK` com os dados do perfil e a lista de links ordenada por `displayOrder`.
  - **Cenário 2 (Usuário inexistente):**
    - _Dado_ um username inexistente na base.
    - _Quando_ consultado.
    - _Então_ retorna `404 Not Found`.

### RF-007: Health Check do Sistema

- **Descrição:** Endpoint `/healthz` expondo o status operacional da API e a conectividade com o banco PostgreSQL.

---

## 2. Regras de Negócio (RN)

| Identificador | Título                                       | Descrição Normativa                                                                                                                                                                                             |
| :------------ | :------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **RN-001**    | **Exclusividade de Conta e E-mail**          | Cada conta deve possuir um e-mail único no sistema. O registro de um novo usuário com e-mail já existente é expressamente rejeitado.                                                                            |
| **RN-002**    | **Obrigatoriedade nos Dados de Perfil**      | Para salvar as informações de perfil, os campos `firstName` e `lastName` são de preenchimento obrigatório. Se ausentes ou compostos apenas por espaços em branco, a operação deve falhar com erro de validação. |
| **RN-003**    | **Catálogo e Padrões de URL por Plataforma** | Toda URL vinculada a uma plataforma deve respeitar o domínio e o padrão oficial da respectiva rede. A tabela normativa abaixo define as 14 plataformas oficiais suportadas:                                     |

### Tabela Normativa de Plataformas e Padrões de URL (RN-003)

| Plataforma ID    | Nome Exibido    | Cor Padrão (Hex)   | Expressão Regular de Validação (Regex)                                            | Exemplo Aceito                                    |
| :--------------- | :-------------- | :----------------- | :-------------------------------------------------------------------------------- | :------------------------------------------------ | ----------------------------- | ------------------------------------ |
| `github`         | GitHub          | `#1A1A1A`          | `^https?:\/\/(www\.)?github\.com\/[a-zA-Z0-9_\-]+(\/)?$`                          | `https://github.com/benwright`                    |
| `frontendmentor` | Frontend Mentor | `#FFFFFF` (border) | `^https?:\/\/(www\.)?frontendmentor\.io\/profile\/[a-zA-Z0-9_\-]+(\/)?$`          | `https://www.frontendmentor.io/profile/benwright` |
| `twitter`        | Twitter / X     | `#43B7E9`          | `^https?:\/\/(www\.)?(twitter                                                     | x)\.com\/[a-zA-Z0-9_]{1,15}(\/)?$`                | `https://x.com/benwright`     |
| `linkedin`       | LinkedIn        | `#2D68FF`          | `^https?:\/\/(www\.)?linkedin\.com\/in\/[a-zA-Z0-9_\-]+(\/)?$`                    | `https://www.linkedin.com/in/benwright`           |
| `youtube`        | YouTube         | `#EE3939`          | `^https?:\/\/(www\.)?youtube\.com\/(@[a-zA-Z0-9_\-]+                              | channel\/[a-zA-Z0-9_\-]+                          | user\/[a-zA-Z0-9_\-]+)(\/)?$` | `https://www.youtube.com/@benwright` |
| `facebook`       | Facebook        | `#2442AC`          | `^https?:\/\/(www\.)?facebook\.com\/[a-zA-Z0-9\.]+(\/)?$`                         | `https://www.facebook.com/benwright`              |
| `twitch`         | Twitch          | `#EE3FC8`          | `^https?:\/\/(www\.)?twitch\.tv\/[a-zA-Z0-9_]+(\/)?$`                             | `https://www.twitch.tv/benwright`                 |
| `devto`          | Dev.to          | `#333333`          | `^https?:\/\/(www\.)?dev\.to\/[a-zA-Z0-9_\-]+(\/)?$`                              | `https://dev.to/benwright`                        |
| `codewars`       | Codewars        | `#8A1A50`          | `^https?:\/\/(www\.)?codewars\.com\/users\/[a-zA-Z0-9_\-]+(\/)?$`                 | `https://www.codewars.com/users/benwright`        |
| `codepen`        | Codepen         | `#302267`          | `^https?:\/\/(www\.)?codepen\.io\/[a-zA-Z0-9_\-]+(\/)?$`                          | `https://codepen.io/benwright`                    |
| `freecodecamp`   | freeCodeCamp    | `#302267`          | `^https?:\/\/(www\.)?freecodecamp\.org\/[a-zA-Z0-9_\-]+(\/)?$`                    | `https://www.freecodecamp.org/benwright`          |
| `gitlab`         | GitLab          | `#EB4925`          | `^https?:\/\/(www\.)?gitlab\.com\/[a-zA-Z0-9_\-]+(\/)?$`                          | `https://gitlab.com/benwright`                    |
| `hashnode`       | Hashnode        | `#0330D1`          | `^https?:\/\/(www\.)?hashnode\.com\/@[a-zA-Z0-9_\-]+(\/)?$`                       | `https://hashnode.com/@benwright`                 |
| `stackoverflow`  | Stack Overflow  | `#EC7100`          | `^https?:\/\/(www\.)?stackoverflow\.com\/users\/[0-9]+(\/[a-zA-Z0-9_\-]+)?(\/)?$` | `https://stackoverflow.com/users/12345/ben`       |

---

| Identificador | Título                                                | Descrição Normativa                                                                                                                                                                                                                                                                           |
| :------------ | :---------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **RN-004**    | **Restrições de Formato e Tamanho de Avatar**         | O avatar submetido deve pertencer estritamente aos tipos MIME `image/jpeg` ou `image/png`. O tamanho do arquivo não pode exceder **2 MB**. Dimensões recomendadas no design: até 1024x1024 pixels. Caso o usuário não tenha avatar, a visualização utiliza como fallback as iniciais do nome. |
| **RN-005**    | **Atomicidade na Reordenação de Links**               | Ao salvar a coleção de links (`PUT /api/links`), os valores de `displayOrder` devem ser normalizados sequencialmente a partir do índice `0` até `N-1`. Toda a operação de inserção, atualização e remoção de links da lista deve ser executada dentro de uma transação no banco de dados.     |
| **RN-006**    | **Isolamento e Controle de Acesso (IDOR Prevention)** | Qualquer operação de leitura, alteração ou exclusão de links e perfis deve extrair o `UserId` exclusivamente das _claims_ do token JWT autenticado, tornando impossível manipular recursos de outros usuários.                                                                                |
| **RN-007**    | **Exibição Contextual da Tela de Preview**            | A rota pública exibe os dados para qualquer visitante. No entanto, os botões "Back to Editor" e controles de navegação administrativa só devem ser visíveis para o usuário se ele for o proprietário autenticado daquele perfil.                                                              |
| **RN-008**    | **Padronização RFC 7807 (ProblemDetails)**            | Todas as respostas de erro de cliente (4xx) e servidor (5xx) devem retornar `application/problem+json` com campos `type`, `title`, `status`, `detail`, `instance` e dicionário `errors` para validações.                                                                                      |

---

## 3. Requisitos Não Funcionais (RNF)

### RNF-001: Performance e Tempo de Resposta

- O endpoint público `GET /api/public/{username}` deve responder em menos de **30 milissegundos** no percentil 95 ($P_{95}$).
- O salvamento em lote de até 15 links deve ser concluído em menos de **100 milissegundos**.

### RNF-002: Segurança e Criptografia

- Senhas nunca devem ser armazenadas em texto simples; devem utilizar o algoritmo de derivação padrão do ASP.NET Core Identity (PBKDF2 com HMAC-SHA256 e sal aleatório).
- Os tokens JWT emitidos devem expirar em prazo controlado (ex: 24 horas) e ser validados criptograficamente a cada requisição protegida.

### RNF-003: Armazenamento e Serviço de Imagens

- A camada de storage de avatares deve ser implementada através de uma interface desacoplada (`IAvatarStorageService`), permitindo alternar entre armazenamento em disco local (desenvolvimento) e provedor em nuvem (produção) via configuração (`appsettings.json`).

### RNF-004: Escalabilidade e Arquitetura Stateless

- A API deve ser 100% _stateless_, permitindo execução em múltiplos containers sem afinidade de sessão.

### RNF-005: Padrões de Código e Manutenibilidade

- C# 13 com recursos modernos: `sealed record` para DTOs, _primary constructors_, _pattern matching_ e _nullable reference types_ ativados.
- APIs estruturadas através de grupos organizados (`RouteGroupBuilder`).

### RNF-006: Testabilidade

- Testes unitários para validação de expressões regulares de todas as plataformas, reordenação de listas e regras de perfil.
- Testes de integração ponta a ponta com `Testcontainers.PostgreSql` validando fluxos de autenticação, persistência e endpoints públicos.

### RNF-007: Containerização e Portabilidade

- O backend deve ser executável com um comando único `docker compose up --build`, subindo a API e o PostgreSQL devidamente configurados e integrados.
