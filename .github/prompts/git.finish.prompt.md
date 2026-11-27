---
description: "Finaliza a issue informada: valida, atualiza roadmap e anotações, commita, faz push e abre PR para develop"
agent: "git.finish"
argument-hint: "Número da issue, URL da issue ou URL/ID do card no GitHub Project"
---

Finalize a issue informada na mensagem seguindo o agente `git.finish`. Priorize os critérios da issue e inclua alterações adicionais cuja inclusão o usuário tenha autorizado, descrevendo-as no PR. Execute as validações antes do commit e respeite os hooks de commit e push, sem ignorá-los. Quando precisar de informação, decisão ou autorização, use `vscode/askQuestions` se estiver disponível, aguarde a resposta e retome o fluxo sem exigir outro prompt. Se houver bloqueio ou falha de validação, apresente opções para corrigir e verificar novamente ou suspender, sem avançar enquanto os critérios falharem. Em falha de hook, pare e informe. Aguarde o resultado das chamadas do GitHub antes de continuar e não presuma que uma operação foi concluída. Se nenhum identificador tiver sido informado, solicite-o antes de prosseguir.
