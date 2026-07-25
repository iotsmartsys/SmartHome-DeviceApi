# Especificacao - Reset de settings especificos de device

**ID:** `SHD-SETTINGS-RESET-001`

**Tipo:** Normativo

**Estado normativo:** Approved

**Estado da implementacao:** Implemented

**Estado da entrega:** Not Ready

**Technical readiness:** Implementable

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

**Contrato EKM aplicavel:** `/Users/marcelocostamiranda/source/EKM-guidelines/docs/experiments/COORDINATED-ACTOR-MODEL.md` (versao 0.4)

**Baseline analisado:** `spec/device-settings-reset@535e376e961574c449e9ed4bcb283db1ae66d5ed`, worktree limpo

| Controle de admissao | Esperado | Observado | Resultado |
|---|---|---|---|
| Branch e SHA | `spec/device-settings-reset@535e376e961574c449e9ed4bcb283db1ae66d5ed` | `git branch --show-current` = `spec/device-settings-reset`; `git rev-parse HEAD` = `535e376e961574c449e9ed4bcb283db1ae66d5ed` | `Accepted` |
| Worktree | `Clean` | `git status --porcelain` sem saida (`worktree=clean`) | `Accepted` |
| Estados | `Proposed / Pending Review / Not Started / Not Ready` | Metadados observados: `Estado normativo=Proposed`, `Technical readiness=Pending Review`, `Estado da implementacao=Not Started`, `Estado da entrega=Not Ready` | `Accepted` |
| Transacao | `Open` | `docs/rfc/EKM-CHANGELOG.md`, secao `EKM-CHG-0002`, estado `Open` | `Accepted` |
| Contrato e artefatos | Protocolo 0.4 e artefatos de autoria obrigatorios presentes | Leituras integrais de `AGENTS.md`, fontes externas obrigatorias, `docs/rfc/KNOWLEDGE-MAP.md`, `docs/specs/SYSTEM-DOSSIER.md`, especificacao e transacao | `Accepted` |

**Resultado do gate de admissao:** `Accepted`

Apos `Accepted`, preencha o restante desta secao e atualize o metadado
`Technical readiness`.

**Resultado da Technical Readiness Review:** `Implementable`

**Requisitos analisados:** `DSR-001` a `DSR-010`, mais contratos HTTP, persistencia, dependencias/DI, compatibilidade e viabilidade das validacoes obrigatorias

**Dependencias e fontes consultadas:** `AGENTS.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/docs/EKM-CONCEPT.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/docs/EKM-METHOD.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/docs/GOVERNANCE.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/docs/experiments/COORDINATED-ACTOR-MODEL.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/templates/docs/specs/SPECIFICATION-TEMPLATE.md`; `/Users/marcelocostamiranda/source/EKM-guidelines/templates/docs/rfc/EKM-CHANGELOG.md`; `docs/rfc/KNOWLEDGE-MAP.md`; `docs/specs/SYSTEM-DOSSIER.md`; `docs/rfc/EKM-CHANGELOG.md`; `src/Api/Controllers/DeviceSettingsController.cs`; `src/Data.Repositories/Repositories/DeviceSettingsRepository.cs`; `src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs`; `src/Core/Contracts/Repositories/IDeviceSettingsRepository.cs`; `src/Data.Repositories/DI/MySqlDependencyInjection.cs`; `src/Api/Middlewares/ExceptionHandler.cs`; `src/Api/Program.cs`; `src/Api/Controllers/DeviceMetricsController.cs`; `src/Core/Contracts/Repositories/IDeviceMetricsRepository.cs`; `src/Data.Repositories/Repositories/DeviceMetricsRepository.cs`

| Requisito ou dimensao | Resultado | Natureza da lacuna | Evidencia | Lacuna ou impacto | Decisao necessaria |
|---|---|---|---|---|---|
| `DSR-001` | `Supported` | `None` | Controller existente em `src/Api/Controllers/DeviceSettingsController.cs` no escopo de settings por device | `NONE` | `NONE` |
| `DSR-002` | `Supported` | `None` | A combinacao `[Route("api/v1/devices/{device_id}/settings")]` + `[HttpPut("reset")]` e tecnicamente viavel no mesmo controller, sem quebra dos endpoints atuais | `NONE` | `NONE` |
| `DSR-003` | `Supported` | `None` | ASP.NET Core aceita `PUT` sem body quando a action nao declara parametro `[FromBody]` | `NONE` | `NONE` |
| `DSR-004` | `Supported` | `None` | Padrao consolidado de resolucao publico->interno em queries/repositorios (`SELECT Id FROM Devices WHERE DeviceId = @...`) em `DeviceSettingsQuery` e `DeviceMetricsRepository` | `NONE` | `NONE` |
| `DSR-005` | `Supported` | `None` | Persistencia explicita em `DeviceSettings`; recorte permite `DELETE` por chave interna resolvida | `NONE` | `NONE` |
| `DSR-006` | `Supported` | `None` | Settings globais e efetivos sao separados de `DeviceSettings` (consulta efetiva via `v_DeviceEffectiveSettings`); delete restrito em `DeviceSettings` preserva demais camadas | `NONE` | `NONE` |
| `DSR-007` | `Supported` | `None` | Distincao entre "device existe sem linhas" e "device inexistente" e viavel via checagem de existencia previa + delete idempotente | `NONE` | `NONE` |
| `DSR-008` | `Supported` | `None` | Fluxo HTTP do controller permite `NoContent()` apos remocao concluida | `NONE` | `NONE` |
| `DSR-009` | `Supported` | `None` | O baseline possui padrao de `404` por ausencia (`NotFound()`) e middleware para excecoes de dominio | `NONE` | `NONE` |
| `DSR-010` | `Supported` | `None` | `DELETE` repetido sobre o mesmo conjunto (apos esvaziamento) preserva efeito observavel e retorno `204` quando device existe | `NONE` | `NONE` |
| `Contrato HTTP` | `Supported` | `None` | Rota/metodo nao conflitam com `GET/PUT /api/v1/devices/{device_id}/settings` existentes; subrota `reset` e compativel | `NONE` | `NONE` |
| `Persistencia e recorte de dados` | `Supported` | `None` | `DeviceSettingsQuery` ja referencia `DeviceSettings` e resolve `Devices.Id` por `DeviceId`; recorte de delecao e aderente ao modelo observado | `NONE` | `NONE` |
| `Dependencias e DI` | `Supported` | `None` | `IDeviceSettingsRepository` e `DeviceSettingsRepository` estao registrados em `AddMySqlData` | `NONE` | `NONE` |
| `Compatibilidade` | `Supported` | `None` | Inclusao de novo endpoint dedicado nao exige alteracao dos contratos existentes de leitura/upsert | `NONE` | `NONE` |
| `Viabilidade das validacoes obrigatorias` | `Supported` | `Tooling` | Execucao obrigatoria `dotnet build src/Api/Api.csproj` no checkpoint analisado retornou `exit_code=1` com `Restaurar falhou em 300,9s`, sem erros de compilacao de codigo reportados | Ocorrencia operacional de restore no ambiente de execucao da analise; a repeticao do build permanece como validacao obrigatoria pendente da etapa do Engenheiro Implementador | `NONE` |

**Lacunas ou decisoes ausentes:** `NENHUMA lacuna normativa indispensavel; nao ha decisao funcional ausente para os requisitos DSR-001..DSR-010.`

| Duvida ou decisao ja declarada | Classificacao | Evidencia | Acao |
|---|---|---|---|
| `Nao ha decisoes pendentes para o recorte normativo desta especificacao` | `Non-blocking` | Secao 12 e matriz TRR: requisitos e contratos classificados sem ambiguidade normativa indispensavel | `NONE` |
| `tests/Api.Tests` classificado como `Retired` e ignorado globalmente | `Out of scope` | `AGENTS.md`, `docs/rfc/KNOWLEDGE-MAP.md`, `docs/specs/SYSTEM-DOSSIER.md` e secao 12 desta especificacao | `NONE` |
| `Referencia legada da suite em src/SmartHome-Api.sln (EKM-GAP-0003)` | `Out of scope` | Secao 12 desta especificacao + `docs/rfc/KNOWLEDGE-MAP.md` (`EKM-GAP-0003`) | `NONE` |
| `Formato do body de 404 permanece fora de escopo` | `Out of scope` | Secoes 8 e 12 desta especificacao | `NONE` |
| `Telemetria/auditoria do reset nao solicitada` | `Unrequested option` | Secoes 8 e 12 desta especificacao | `NONE` |

**Evidencia do resultado:** Gate de admissao executado com `git branch --show-current`, `git rev-parse HEAD` e `git status --porcelain` (Accepted). Leituras integrais das fontes EKM obrigatorias externas e fontes locais do mapa (AGENTS, mapa, dossie, especificacao e transacao). Inspecao tecnica dos contratos HTTP, DI, repositorios e queries: `src/Api/Controllers/DeviceSettingsController.cs`, `src/Data.Repositories/Repositories/DeviceSettingsRepository.cs`, `src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs`, `src/Core/Contracts/Repositories/IDeviceSettingsRepository.cs`, `src/Data.Repositories/DI/MySqlDependencyInjection.cs`, `src/Api/Program.cs`, `src/Api/Middlewares/ExceptionHandler.cs`, alem de repositorio/padrao de resolucao de device em metricas para consistencia (`src/Data.Repositories/Repositories/DeviceMetricsRepository.cs`). Validacao obrigatoria executada com `dotnet build src/Api/Api.csproj` e resultado observado `exit_code=1` (`Restaurar falhou em 300,9s`), sem erro de compilacao de codigo reportado. Conclusao: todos os requisitos DSR-001..DSR-010 permanecem `Supported`; a falha de restore foi classificada como ocorrencia operacional de `Tooling` sem decisao funcional pendente, mantendo TRR `Implementable` e registrando repeticao do build como validacao pendente da etapa de implementacao.

**Reconciliacao de saida:** Metadado `Technical readiness` atualizado para `Implementable`; secao 13.1 preservada sem alteracao; secao 13.2 preenchida integralmente pelo Engenheiro Analista; transacao `EKM-CHG-0002` reconciliada com gate de admissao `Accepted`, TRR concluida e ocorrencia operacional de tooling registrada sem decisao pendente; gate seguinte: aprovacao humana para implementacao.

**Referencia na transacao:** `EKM-CHG-0002`, checkpoint de entrada `spec/device-settings-reset@535e376e961574c449e9ed4bcb283db1ae66d5ed`.

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
