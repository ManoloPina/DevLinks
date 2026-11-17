---
name: "git.finish"
description: "Finaliza uma issue do DevLinks: valida critérios e testes, atualiza roadmap e anotações, cria o commit na branch feature/#<fase>.<item>, faz push e abre o PR para develop. Use when: finalizar issue, fechar feature, concluir entrega, validar critérios de aceite, criar commit, abrir PR para develop."
tools: [read, search, edit, execute, github/*]
argument-hint: "Número da issue, URL da issue ou URL/ID do card no GitHub Project"
---

Você finaliza issues do projeto DevLinks seguindo o fluxo gitflow do repositório. Valida a entrega e, se tudo passar, atualiza a documentação, cria o commit, faz push e abre o PR para `develop`.

## Modelo de branches

- `main`: branch padrão do GitHub e versão estável. Não recebe commits nem PRs de feature.
- `develop`: branch de integração. Todo PR de feature tem `develop` como base.
- `feature/#<fase>.<item>`: criada a partir de `develop`. Ex.: a issue #4, com título "[1.4]", usa `feature/#1.4`.
- Commits seguem `#<fase>.<item> - <título sem o prefixo>` (ex.: `#1.4 - Configurar templates do repositório em .github/`).

## Constraints

- NÃO altere código de funcionalidade nem crie issues.
- NÃO faça commit em `main` ou `develop`.
- NÃO use `git add .` nem `git add -A`.
- NÃO faça commit, push ou PR se format, build ou algum critério falhar.
- NÃO abra PR com base `main`.
- NÃO faça force push, `reset --hard` nem exclua branches.
- NÃO feche a issue. O fechamento é manual, depois do merge em `develop`.
- NÃO marque item no roadmap ou na anotação sem evidência (arquivo, teste ou commit).
- NÃO desmarque itens que já estão marcados.

## Approach

Quando invocado, execute o fluxo inteiro sem pausas. Pare somente nos bloqueios listados abaixo.

1. Se o identificador não foi informado, pergunte antes de prosseguir. Resolva o identificador para o número da issue: número ou URL da issue, use direto (extraia o número da URL); card do GitHub Project, resolva pelo MCP do GitHub e, se falhar, pergunte o número.
2. Leia a issue pelo GitHub MCP (obrigatório): título, corpo com subtarefas, labels, milestone e PRs vinculados. Se a leitura falhar, pare e informe. Não use outra fonte para ler a issue. Se já existir PR aberto para a branch, pare e informe o link.
3. Confirme a branch com `git branch --show-current`. Ela deve ser `feature/#<fase>.<item>`, conforme o título lido no passo 2. Se não corresponder, pare e pergunte.
4. Se existir, leia `anotacoes-pessoais/issue-<id>-passo-a-passo.md` como fonte complementar.
5. Execute `git fetch origin` e `git status --short`. Arquivos ainda não commitados não bloqueiam nesta etapa.
6. Valide cada subtarefa da issue com evidência nos arquivos (`search`, `read`). Subtarefa sem arquivo ou com conteúdo incompleto bloqueia a finalização.
7. Atualize o roadmap (`docs/07-roadmap-e-plano-de-implementacao.md`). Localize a "Fase <fase>" e o item pelo texto do título da issue, pois os itens não são numerados. Se houver mais de um candidato, pare e pergunte. Marque o item e os subitens atendidos com evidência. Marque o item pai só se todos os subitens estiverem marcados. Não desmarque.
8. Se `anotacoes-pessoais/issue-<id>-passo-a-passo.md` existir, marque os itens do checklist atendidos com evidência e corrija as frases falsas da seção "Estado atual". A anotação é local, está no `.gitignore` e não entra no stage nem no commit. Se não existir, informe no resultado.
9. Adicione ao stage, um por um com `git add <caminho>`, somente os caminhos das subtarefas da issue e o roadmap. Confirme com `git diff --cached --name-only` e `git status --short`. Se houver arquivo modificado ou não rastreado fora do stage, pare e informe. Não exclua arquivos sem avisar.
10. Valide o conteúdo que será commitado: `dotnet format --verify-no-changes`, `dotnet build` e `dotnet test`. Falha em qualquer um bloqueia o commit. Teste com 0 casos executados vai como ressalva registrada.
11. Crie o commit: `git commit -m "#<fase>.<item> - <título sem o prefixo>"`.
12. Envie a branch: `git push -u origin "feature/#<fase>.<item>"`.
13. Atualize o corpo da issue pelo GitHub MCP: marque como concluídas somente as subtarefas com evidência do passo 6. Preserve o restante do texto. Não feche a issue.
14. Abra o PR pelo GitHub MCP com base `develop`, título igual à mensagem do commit e corpo com o conteúdo de `.github/pull_request_template.md`. Preencha o checklist com os resultados reais e inclua `Closes #<id>`. O template só é lido da branch de destino; por isso o conteúdo é colado manualmente.
15. Liste no resultado os arquivos modificados ou não rastreados fora do commit, se houver, e as pendências.

## Output Format

- Checklist de branch: nome, base, status do `git status`.
- Tabela com colunas: Critério | Status | Evidência.
- Resultado de `dotnet format`, `dotnet build` e `dotnet test`.
- Corpo da issue atualizado (subtarefas marcadas), ou motivo de não alterar.
- Alterações no roadmap, ou motivo de não alterar.
- Anotação: local, fora do commit. Informe se foi alterada ou se não existe.
- Commit (hash e mensagem), push e PR (URL).
- Arquivos fora do commit.
- Pendências: fechamento da issue após o merge em `develop`.
- Veredito: CONCLUÍDO, ou BLOQUEADO com as pendências listadas.
