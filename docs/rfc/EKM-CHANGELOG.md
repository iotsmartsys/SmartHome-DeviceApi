# EKM — Histórico de mudanças

## EKM-CHG-0002 — Especificação de reset de settings específicos de device

**Estado:** Open

**Data:** 24/07/2026

### Objetivo

Criar especificação EKM para o reset de settings específicos de device com
contrato explícito de rota, sem body, mapeamento de `device_id` público para
chave interna persistida, preservação de settings não específicos, retorno
`204/404` e idempotência.

### Baseline

- Repositório:
  `/Users/marcelocostamiranda/source/IoT/SmartHome/Services/SmartHome-DeviceApi`.
- Referência de origem: `main`.
- Commit de origem da branch da mudança:
  `3fde52003d23b76d4a76be33e6b416beca0f1a7c`.
- Worktree de origem: limpo.
- Branch da mudança: `spec/device-settings-reset`, derivada do commit de
  origem.
- Esta atuação é exclusivamente documental e não altera implementação.

### Escopo

- criar especificação `docs/specs/DEVICE-SETTINGS-RESET.md`;
- registrar a transação `EKM-CHG-0002` neste changelog;
- atualizar `docs/rfc/KNOWLEDGE-MAP.md` para refletir a nova fonte normativa
  no domínio de settings;
- registrar dúvidas e decisões ausentes sem inferência;
- não alterar código, testes, banco, build, automações ou deploy.

### Ativos afetados

- `docs/specs/DEVICE-SETTINGS-RESET.md`;
- `docs/rfc/EKM-CHANGELOG.md`;
- `docs/rfc/KNOWLEDGE-MAP.md`.

### Decisões e requisitos

- endpoint no controller existente de settings do device;
- rota `PUT /api/v1/devices/{device_id}/settings/reset`;
- requisição sem body;
- remoção de todas as linhas específicas do device em `DeviceSettings`;
- resolução de `device_id` público para chave interna persistida;
- preservação de settings globais, herdados ou padrão;
- retorno `204 No Content` quando o device existe sem linhas específicas;
- retorno `204 No Content` após reset concluído;
- retorno `404 Not Found` para `device_id` inexistente;
- operação idempotente.

### Technical Readiness Review

- Resultado: `Pending Review`.
- Baseline e fontes analisadas:
  `AGENTS.md`, `docs/rfc/KNOWLEDGE-MAP.md`, `docs/specs/SYSTEM-DOSSIER.md`,
  `docs/rfc/EKM-CHANGELOG.md`, fontes externas EKM e templates aplicáveis,
  além dos artefatos técnicos de settings/device do baseline.
- Matriz integral de requisitos e dimensões:
  `docs/specs/DEVICE-SETTINGS-RESET.md`.
- Lacunas ou decisões ausentes:
  `EKM-GAP-0002`, `EKM-DECISION-PENDING-DSR-001`,
  `EKM-DECISION-PENDING-DSR-002`.
- Evidência de que a revisão encerrou sem alteração de implementação:
  esta atuação realizou apenas mudanças em artefatos documentais.
- Aprovação humana para implementar: pendente.
- Reconfirmação do baseline antes da primeira alteração: não aplicável nesta
  etapa.

### Estado da entrega

- Referência de produção: `main`.
- Estado: `Not Ready`.
- Relações normativas criadas: `SHD-SETTINGS-RESET-001@0.1`.
- Evidência de integração, quando `Done`: pendente.

### Validações

- leitura obrigatória das fontes EKM externas e locais na ordem definida em
  `AGENTS.md`;
- inspeção dos contratos atuais de settings/device e persistência;
- verificação de branch exclusiva derivada de `main` com worktree limpo;
- validação de escopo documental sem alteração de implementação.

### Pendências e desvios

- Technical Readiness Review permanece pendente por separação de papéis e gate;
- decisões ausentes registradas na especificação:
  `EKM-DECISION-PENDING-DSR-001` e `EKM-DECISION-PENDING-DSR-002`;
- lacuna estrutural de autoridade de schema/view MySQL permanece em
  `EKM-GAP-0002`.

### Encerramento

A transação permanece `Open` até aprovação humana da especificação, execução da
Technical Readiness Review por papel apropriado e avanço autorizado do ciclo
EKM.

## EKM-CHG-0001 — Fundação EKM por referência externa

**Estado:** Open

**Data:** 24/07/2026

### Objetivo

Adotar a fundação EKM no SmartHome-DeviceApi sem alterar comportamento e sem
copiar as definições compartilhadas da EKM para este repositório.

### Baseline

- Repositório:
  `/Users/marcelocostamiranda/source/IoT/SmartHome/Services/SmartHome-DeviceApi`.
- Referência de origem: `main`.
- Commit de origem: `e396b300c48ddd52190c6bba742bde6e6b6cf96d`.
- Worktree de origem: limpo.
- Branch da mudança: `chore/adopt-ekm`, derivada do commit de origem.
- Código, build, testes e automações preexistentes não fazem parte da mudança.

### Escopo

- criar o bootstrap `AGENTS.md`;
- apontar dinamicamente para a EKM em
  `/Users/marcelocostamiranda/source/EKM-guidelines`;
- criar o mapa das fontes de verdade;
- criar este changelog e a transação da fundação;
- criar um dossiê factual inicial;
- registrar lacunas observadas;
- não criar ainda a especificação de reset de settings;
- não alterar código, testes, dependências, build, CI/CD ou deploy.

### Decisões confirmadas

- a EKM compartilhada será lida diretamente da pasta local indicada;
- não haverá cópia local das diretrizes, método, governança ou protocolo;
- nesta fase, não será validado nem fixado o commit da EKM;
- cada ator deve reler a fonte EKM no início de sua atuação;
- `main` é a referência de produção;
- especificações funcionais nascem em branch exclusiva derivada de `main`;
- `qa` e `homolog` são previstas, mas ainda não obrigatórias.

### Fontes externas consultadas

- `docs/EKM-CONCEPT.md`;
- `docs/EKM-METHOD.md`;
- `docs/GOVERNANCE.md`;
- `docs/experiments/COORDINATED-ACTOR-MODEL.md`;
- `templates/EKM-LEGACY-ADOPTION-INSTRUCTIONS.md`;
- `templates/docs/specs/SYSTEM-DOSSIER.md`.

Todas pertencem ao repositório externo informado no baseline. A consulta é
dinâmica e não preserva integridade por commit nesta fase.

### Critério de prontidão da mudança documental

Esta é uma mudança de governança exclusivamente documental; não autoriza nem
altera implementação funcional.

O critério próprio de prontidão exige:

- ativos mínimos localizáveis;
- afirmações relevantes classificadas como fatos, decisões ou lacunas;
- links e caminhos válidos;
- ausência de alteração funcional;
- integridade textual;
- validações executadas ou explicitamente pendentes;
- autorização humana para criar a branch e o commit.

### Aprovação humana

O arquiteto Marcelo Miranda aprovou em 24/07/2026:

- a adoção documental separada;
- o apontamento local e dinâmico para a EKM;
- a ausência inicial de validação da EKM por commit;
- a criação da branch da fundação;
- a criação do commit documental.

### Estado da entrega

- Referência de produção: `main`.
- Estado: `Not Ready`.
- Motivo: a fundação ainda não foi integrada em `main`.
- Relações normativas criadas: `AGENTS.md` e
  `docs/rfc/KNOWLEDGE-MAP.md`.

### Validações

- inspeção da estrutura, manifests, API, persistência, testes e operação;
- verificação de caminhos e links: aprovada;
- integridade textual: aprovada por `git diff --check`;
- build de `src/Api/Api.csproj`: aprovado, sem erros ou avisos;
- build da solução em execução paralela: interrompido após permanecer sem saída;
- repetição serial do build da API: aprovada;
- testes de `tests/Api.Tests/Api.Tests.csproj`: reprovados na compilação por
  duas ocorrências preexistentes de `CS7036` em
  `tests/Api.Tests/DeviceMetricsTests.cs`, que não fornecem o parâmetro
  `deviceId` exigido pela assinatura atual de `DeviceMetricsController.Save`;
- revisão interna da fundação: concluída;
- auditoria independente: pendente para o gate posterior à implementação do
  experimento.

### Pendências e desvios

- `EKM-GAP-0001` a `EKM-GAP-0005`, registrados no mapa;
- o conjunto mínimo do modelo 1.6 prevê uma cópia local de
  `EKM-GUIDELINES.md`; por decisão experimental, esta adoção usa referência
  externa e registra a diferença explicitamente;
- a especificação de reset de settings será criada somente após esta fundação
  alcançar `main`.

### Operações Git e externas

- branch `chore/adopt-ekm` criada a partir de
  `main@e396b300c48ddd52190c6bba742bde6e6b6cf96d`;
- commit documental autorizado; o SHA resultante será registrado no próximo
  checkpoint, pois este arquivo integra o próprio commit;
- nenhum push, pull request, merge, tag, deploy ou publicação autorizado.

### Encerramento

A transação permanece `Open`. Poderá ser fechada após auditoria e integração da
fundação em `main`, sem alteração funcional.
