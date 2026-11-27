---
description: "Inicia a issue informada: lê issue e roadmap, cria a branch feature/#<fase>.<item> e gera a anotação passo a passo"
agent: "git.start"
argument-hint: "Número da issue, URL da issue ou URL/ID do card no GitHub Project"
---

Inicie a issue informada na mensagem seguindo o agente `git.start`. Atualize a issue e, quando necessário, o roadmap antes de gerar a anotação. Produza um roteiro completo com o conteúdo proposto de cada arquivo, testes e comandos de validação, sem implementar a funcionalidade. Quando precisar de informação, decisão ou autorização, use `vscode/askQuestions` se estiver disponível, aguarde a resposta e retome o fluxo sem exigir outro prompt. Em caso de bloqueio recuperável, ofereça opções para nova verificação após correção ou suspensão, respeitando as constraints. Aguarde o resultado das chamadas do GitHub e não presuma que uma operação foi concluída. Se nenhum identificador tiver sido informado, solicite-o antes de prosseguir.
