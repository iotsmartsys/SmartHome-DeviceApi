# Especificacao - Reset de settings especificos de device

**ID:** `SHD-SETTINGS-RESET-001`

**Tipo:** Normativo

**Estado normativo:** Proposed

**Estado da implementacao:** Not Started

**Estado da entrega:** Not Ready

**Technical readiness:** Pending Review

**Versao:** 0.1

**Responsavel:** Autor da Especificacao

**Relacao normativa:** New

## 1. Objetivo

Definir o contrato normativo para resetar os settings especificos de um device por
endpoint dedicado, sem remover settings globais, herdados ou padrao.

## 2. Contexto e problema

O baseline atual expoe consulta e upsert de settings por device no mesmo
controller, mas nao possui endpoint para remover somente as linhas especificas de
`DeviceSettings` associadas a um device identificado publicamente por
`device_id`.

Sem esse contrato, o comportamento de reset pode ser implementado de forma
inconsistente entre persistencia, retorno HTTP e idempotencia.

## 3. Escopo

Abrange exclusivamente o contrato do endpoint de reset de settings especificos de
device:

- criar endpoint no controller existente de settings de device;
- rota `PUT /api/v1/devices/{device_id}/settings/reset`;
- requisicao sem body;
- remocao das linhas especificas do device na tabela `DeviceSettings`;
- mapeamento de `device_id` publico para chave interna persistida;
- preservacao de settings globais, herdados ou padrao;
- respostas HTTP de sucesso e ausencia do device;
- idempotencia da operacao.

## 4. Requisitos

- **`DSR-001`:** deve existir endpoint no controller de settings de device.
- **`DSR-002`:** o endpoint deve responder em `PUT /api/v1/devices/{device_id}/settings/reset`.
- **`DSR-003`:** a requisicao nao possui body.
- **`DSR-004`:** o `device_id` da rota representa identificacao publica e deve ser resolvido para a chave interna persistida antes da remocao.
- **`DSR-005`:** o reset deve remover todas as linhas especificas do device na tabela `DeviceSettings` associadas a chave interna resolvida.
- **`DSR-006`:** settings globais, herdados ou padrao nao devem ser removidos.
- **`DSR-007`:** se o device existir e nao houver linhas especificas para remover, a resposta deve ser `204 No Content`.
- **`DSR-008`:** se a remocao for concluida, a resposta deve ser `204 No Content`.
- **`DSR-009`:** se `device_id` nao existir, a resposta deve ser `404 Not Found`.
- **`DSR-010`:** a operacao deve ser idempotente, mantendo o mesmo efeito observavel em chamadas repetidas.

## 5. Fluxos e estados

### 5.1 Fluxo principal

1. Receber `PUT /api/v1/devices/{device_id}/settings/reset` sem body.
2. Resolver `device_id` publico para chave interna persistida.
3. Remover em `DeviceSettings` todas as linhas especificas vinculadas a chave interna.
4. Retornar `204 No Content`.

### 5.2 Device existente sem linhas especificas

1. Resolver `device_id` publico para chave interna persistida.
2. Detectar ausencia de linhas especificas em `DeviceSettings` para o device.
3. Retornar `204 No Content`.

### 5.3 Device inexistente

1. Tentar resolver `device_id` publico.
2. Quando nao houver correspondencia persistida, retornar `404 Not Found`.

### 5.4 Idempotencia

1. Uma chamada bem-sucedida seguida de novas chamadas para o mesmo `device_id`.
2. O efeito persistido deve permanecer equivalente a ausencia de linhas
   especificas na `DeviceSettings` para o device.
3. O retorno deve permanecer `204 No Content` enquanto o device existir.

## 6. Contratos e invariantes

### 6.1 Contrato HTTP

- Metodo: `PUT`.
- Rota: `/api/v1/devices/{device_id}/settings/reset`.
- Request body: inexistente.
- Success response: `204 No Content`.
- Device inexistente: `404 Not Found`.

### 6.2 Invariantes de persistencia

- A remocao atua somente sobre linhas especificas do device em `DeviceSettings`.
- A operacao usa a chave interna persistida do device, obtida a partir do
  `device_id` publico.
- Dados globais, herdados ou padrao nao podem ser removidos por esse endpoint.

### 6.3 Compatibilidade

- A adicao do endpoint nao altera o contrato existente de leitura e escrita de
  settings no mesmo controller.

## 7. Falhas e condicoes de borda

- `device_id` inexistente: retornar `404 Not Found`.
- Device existente sem linhas especificas: retornar `204 No Content`.
- Repeticao da operacao no mesmo device: manter idempotencia e retornar `204 No Content`.
- Falhas de infraestrutura (ex.: indisponibilidade de banco): seguem o
  tratamento global de excecoes ja vigente no sistema.

## 8. Fora de escopo

- Alteracao de estrutura de banco, view ou migracoes.
- Definicao de novo modelo de heranca de settings.
- Alteracao do contrato dos endpoints ja existentes em
  `PUT /api/v1/devices/{device_id}/settings` e `GET /api/v1/devices/{device_id}/settings`.
- Definicao de payload de erro alem do status requerido para `404`.

## 9. Criterios de aceite

- `DSR-001` e `DSR-002`: endpoint existe no controller de settings de device e
  atende exatamente a rota e metodo definidos.
- `DSR-003`: chamada sem body e processada sem dependencia de payload.
- `DSR-004`: evidencia de resolucao de `device_id` publico para chave interna
  antes de remover.
- `DSR-005`: evidencia de remocao de todas as linhas especificas do device na
  tabela `DeviceSettings`.
- `DSR-006`: evidencia de preservacao de dados globais, herdados ou padrao.
- `DSR-007`: evidencia de retorno `204` para device existente sem linhas especificas.
- `DSR-008`: evidencia de retorno `204` para reset concluido.
- `DSR-009`: evidencia de retorno `404` para `device_id` inexistente.
- `DSR-010`: evidencia de idempotencia em chamadas repetidas.

## 10. Validacoes obrigatorias

As validacoes abaixo ficam obrigatorias para a fase de implementacao,
fora do escopo desta atuacao de autoria de especificacao:

- build exato de `src/Api/Api.csproj`;
- inspecao da query e do repositorio envolvidos no reset;
- verificacao de que apenas `DeviceSettings` do device alvo foi alterada;
- validacao funcional ou de integracao cobrindo sucesso, ausencia de linhas,
  retorno `404` para device inexistente e idempotencia.

## 11. Ativos de conhecimento afetados

- `docs/specs/DEVICE-SETTINGS-RESET.md` (esta especificacao).
- `AGENTS.md` (politica global da suite de testes descontinuada).
- `docs/rfc/KNOWLEDGE-MAP.md` (fonte normativa do dominio settings).
- `docs/specs/SYSTEM-DOSSIER.md` (visao factual de build e validacao).
- `docs/rfc/EKM-CHANGELOG.md` (transacao EKM da mudanca).

## 12. Relacoes, desvios e lacunas

- Relacao com lacuna `EKM-GAP-0001`: adiciona especificacao normativa para
  parte do dominio de settings.
- Relacao com lacuna `EKM-GAP-0002`: a autoridade completa do schema/view de
  persistencia permanece aberta; nao foi resolvida nesta especificacao.
- Desvio conhecido do baseline atual: inexistencia do endpoint de reset
  especificado.

### Duvidas e decisoes ausentes registradas

- Nao ha decisoes pendentes para o recorte normativo desta especificacao.
- Por decisao humana de 25/07/2026, `tests/Api.Tests` esta classificado como
  `Retired` em todo o repositorio e deve ser ignorado em todas as situacoes:
  nao deve ser executado, reparado, usado como evidencia ou tratado como
  bloqueio. Seus arquivos permanecem apenas como registro historico.
- A referencia ainda existente em `src/SmartHome-Api.sln` e uma discrepancia
  legada registrada em `EKM-GAP-0003`; ela nao reativa a suite e sua
  reconciliacao nao integra esta correcao documental.
- O formato do body de `404` permanece fora de escopo por decisao humana.
- Telemetria/auditoria do evento de reset nao foi solicitada e permanece fora
  de escopo.

## 13. Registro da Technical Readiness Review

### 13.1 Estado entregue pelo Autor da Especificacao

No checkpoint de saida da autoria, esta secao deve permanecer exatamente como:

**Resultado:** `Pending Review`

**Revisao executada:** Nao.

### 13.2 Registro exclusivo do Engenheiro Analista

Antes de modificar esta especificacao, o Engenheiro Analista executa o gate de
admissao. Se o resultado for `Checkpoint Blocked`, nao altera este documento e
entrega um relatorio read-only a Coordenacao, que o registra na transacao.

Somente apos `Accepted`, o Analista preserva a secao 13.1 como evidencia do
handoff e preenche o registro abaixo.

**Contrato EKM aplicavel:** `<FONTE E VERSAO DO PROTOCOLO>`

**Baseline analisado:** `<BRANCH, COMMIT E WORKTREE>`

| Controle de admissao | Esperado | Observado | Resultado |
|---|---|---|---|
| Branch e SHA | `<CHECKPOINT>` | `<EVIDENCIA>` | `Accepted` |
| Worktree | `Clean` | `<EVIDENCIA>` | `Accepted` |
| Estados | `Proposed / Pending Review / Not Started / Not Ready` | `<ESTADOS>` | `Accepted` |
| Transacao | `Open` | `<ESTADO>` | `Accepted` |
| Contrato e artefatos | `<VERSAO E ARTEFATOS>` | `<EVIDENCIA>` | `Accepted` |

**Resultado do gate de admissao:** `Accepted`

Apos `Accepted`, preencha o restante desta secao e atualize o metadado
`Technical readiness`.

**Resultado da Technical Readiness Review:** `Implementable | Needs Clarification`

**Requisitos analisados:** `<LISTA OU INTERVALO>`

**Dependencias e fontes consultadas:** `<LISTA>`

| Requisito ou dimensao | Resultado | Natureza da lacuna | Evidencia | Lacuna ou impacto | Decisao necessaria |
|---|---|---|---|---|---|
| `<ID OU ASPECTO>` | `Supported`, `Gap`, `Conflict` ou `Not Applicable` | `Normative`, `Baseline`, `Tooling`, `Evidence` ou `None` | `<EVIDENCIA>` | `<IMPACTO OU NONE>` | `<DECISAO OU NONE>` |

**Lacunas ou decisoes ausentes:** `<NENHUMA OU ITENS RASTREAVEIS>`

| Duvida ou decisao ja declarada | Classificacao | Evidencia | Acao |
|---|---|---|---|
| `<ITEM OU NONE>` | `Blocking`, `Non-blocking`, `Out of scope` ou `Unrequested option` | `<EVIDENCIA>` | `<RETORNO A AUTORIA OU NONE>` |

**Evidencia do resultado:** `<COMANDOS, INSPECOES E CONCLUSAO>`

**Reconciliacao de saida:** `<METADADOS, SECAO 13, TRANSACAO E GATE SEGUINTE>`

**Referencia na transacao:** `<EKM-CHG-NNNN E CHECKPOINT>`

A revisao deve continuar apos o primeiro bloqueio ate classificar todos os
itens. Sua execucao encerra sem alterar implementacao, inclusive com
`Implementable`.

Distinga decisao indispensavel, comportamento fora de escopo e opcao nao
solicitada. Somente decisao indispensavel ausente produz `Gap`. A natureza da
lacuna explica sua origem, mas nao cria um terceiro resultado da revisao.

Uma especificacao `Needs Clarification` nao autoriza implementacao parcial nem
alteracao de artefatos de implementacao. Somente registros EKM e a correcao
normativa aprovada podem mudar. Apos correcao normativa, a analise deve ser
repetida integralmente.

`Implementable` e recomendacao tecnica. Aprovacao humana e reconfirmacao do
baseline sao registradas na transacao antes da implementacao.
