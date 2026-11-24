---
description: "Finaliza a issue informada: valida, atualiza roadmap e anotações, commita, faz push e abre PR para develop"
agent: "git.finish"
argument-hint: "Número da issue, URL da issue ou URL/ID do card no GitHub Project"
---

Finalize a issue informada na mensagem seguindo o agente `git.finish`. Priorize os critérios da issue e inclua alterações adicionais cuja inclusão o usuário tenha autorizado, descrevendo-as no PR. Execute as validações antes do commit e respeite os hooks de commit e push, sem ignorá-los. Se houver alterações que bloqueiem o fluxo, pergunte como o usuário deseja tratá-las antes de parar. Se nenhum identificador tiver sido informado, pergunte antes de prosseguir.
