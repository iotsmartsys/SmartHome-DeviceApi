# Instruções para agentes

Este repositório adota a Engineering Knowledge Management (EKM) por referência
externa. As definições compartilhadas da EKM não devem ser copiadas para este
repositório.

## Fonte EKM

A fonte atual deve ser lida diretamente em:

`/Users/marcelocostamiranda/source/EKM-guidelines`

Antes de iniciar cada etapa ou operação, leia na fonte externa:

1. `docs/EKM-CONCEPT.md`;
2. `docs/EKM-METHOD.md`;
3. `docs/GOVERNANCE.md`;
4. `docs/experiments/COORDINATED-ACTOR-MODEL.md`;
5. os templates aplicáveis à tarefa.

O apontamento é dinâmico nesta fase experimental. Não fixe nem valide o commit da
EKM. Se o caminho não estiver acessível, interrompa a atuação; não use uma cópia
ou cache como fallback.

## Fontes específicas deste projeto

Depois da leitura externa, leia nesta ordem:

1. `docs/rfc/KNOWLEDGE-MAP.md`;
2. `docs/specs/SYSTEM-DOSSIER.md`;
3. as especificações relacionadas em `docs/specs/`;
4. `docs/rfc/EKM-CHANGELOG.md` e a transação aplicável.

As especificações, o mapa, o dossiê, as transações, as lacunas, os relatórios e
as evidências deste sistema permanecem neste repositório.

## Decisões de processo confirmadas

- `main` é a referência de produção.
- Toda especificação funcional deve nascer em branch exclusiva derivada de
  `main`.
- Especificação e desenvolvimento percorrem a mesma branch da mudança.
- Cada atuação de agente começa em commit explícito, com worktree limpo e
  estados da especificação registrados.
- As branches `qa` e `homolog` são previstas para adoção futura, mas ainda não
  são obrigatórias.

## Limites

- Não invente intenção, contrato, compatibilidade ou comportamento ausente.
- Preserve alterações preexistentes e fora do escopo.
- Não trate relatório como fonte de novos requisitos.
- Não execute operação Git ou externa sem autorização aplicável.
- Mudanças funcionais exigem especificação, Technical Readiness Review integral,
  aprovação humana e reconfirmação do checkpoint.
