# EKM — Histórico de mudanças

## EKM-CHG-0003 — Suporte à manutenção administrativa de Groups

**Estado:** Closed

**Data de abertura:** 02/09/2026

**Especificação:** `SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1` em
`docs/specs/GROUPS-MAINTENANCE-SUPPORT.md`

### Objetivo

Adequar leitura, criação, patch e exclusão de Groups para habilitar a nova
superfície de manutenção do aplicativo AIoTSmartHome.

### Decisões

- coleção vazia será representada por `200 []`;
- POST aceitará ausência de `id` e `capabilities`, persistirá o ícone opcional
  e manterá compatibilidade com capabilities opcionais de consumidores
  anteriores;
- PATCH aceitará somente `replace` de `name`, `active` e `icon`, preservando
  associações;
- DELETE distinguirá Group inexistente e removerá relações atomicamente sem
  excluir capabilities;
- contratos HTTP de leitura, criação e patch serão separados localmente;
- nenhum artefato de teste integra a versão; validação integrada depende de
  autorização e MySQL descartável.

### Estado

A versão 0.1 está Concluída [`Done`] por decisão do Arquiteto em 02/09/2026.
DTOs de criação e patch foram separados, guards e `ProblemDetails` foram
aplicados, INSERT passou a persistir `IconName`, PATCH atualiza somente os
campos solicitados e DELETE remove relações em transação explícita. O build
`dotnet build src/Api/Api.csproj --configuration Release` terminou com código
zero. O Arquiteto informou a conclusão da validação, julgou as evidências
suficientes e autorizou a promoção para `main`; detalhes técnicos adicionais da
validação não foram registrados neste repositório. Testes automatizados,
publicação, deploy e alteração do aplicativo consumidor não integraram esta
execução. A integração é comprovada pelo histórico Git.

## EKM-CHG-0002 — Especificação de reset de settings específicos de device

**Estado:** Open

**Data de abertura:** 2026-07-24

**Especificação:** `SHD-SETTINGS-RESET-001@0.1` em
`docs/specs/DEVICE-SETTINGS-RESET.md`

## 1. Objetivo e escopo

**Objetivo:** especificar o reset dos settings específicos de um device com
contrato explícito de rota, persistência, respostas HTTP e idempotência, e
conduzir a mudança pelos gates da EKM.

**Incluído:** especificação, transação, mapa, dossiê e bootstrap dos agentes;
registro da decisão global sobre `tests/Api.Tests`; ciclos de autoria e
Technical Readiness Review.

**Fora de escopo:** implementação, solução, testes, banco, build, automações,
deploy e definição de payload de erro além do status `404`.

## 2. Baseline e branch

- Repositório:
  `/Users/marcelocostamiranda/source/IoT/SmartHome/Services/SmartHome-DeviceApi`.
- Referência de origem: `main`.
- Commit de origem: `3fde52003d23b76d4a76be33e6b416beca0f1a7c`.
- Worktree de origem: limpo.
- Branch da mudança: `spec/device-settings-reset`.
- Fontes normativas: `SHD-SETTINGS-RESET-001@0.1`, `AGENTS.md`, fontes EKM
  externas apontadas dinamicamente e `docs/rfc/KNOWLEDGE-MAP.md`.
- Lacunas preexistentes relevantes: `EKM-GAP-0002` e `EKM-GAP-0003`.

### 2.1 Handoffs e contrato EKM aplicável

| Etapa | Checkpoint de entrada | Estados esperados | Fonte e versão do contrato | Compatibilidade ou normalização | Resultado da admissão |
|---|---|---|---|---|---|
| Autoria inicial | `main@3fde52003d23b76d4a76be33e6b416beca0f1a7c` | `Draft` até entrega em `Proposed / Pending Review / Not Started / Not Ready` | EKM dinâmica vigente em 24/07/2026 | Branch exclusiva criada a partir de `main` | `Accepted` |
| Primeira análise | `spec/device-settings-reset@eb5ed262dfa830f62aa936bb02ce7420780fdd3d` | `Proposed / Pending Review / Not Started / Not Ready` | EKM dinâmica vigente em 24/07/2026, anterior ao gate 0.4 | O contrato da época não separava formalmente admissão e TRR | `Not Applicable` |
| Autoria corretiva | `spec/device-settings-reset@a3cbb556d3388d2987da1e87b46c20c97945ff65` | Retorno `Needs Clarification` | EKM dinâmica vigente em 25/07/2026 | Decisões humanas incorporadas e revisão anterior invalidada | `Accepted` |
| Nova análise | `spec/device-settings-reset@535e376e961574c449e9ed4bcb283db1ae66d5ed` | `Proposed / Pending Review / Not Started / Not Ready` | `/Users/marcelocostamiranda/source/EKM-guidelines/docs/experiments/COORDINATED-ACTOR-MODEL.md`, versão 0.4 | Sem normalização adicional; checkpoint aderente ao gate de admissão | `Accepted` |
| Implementação | Checkpoint de saída desta aprovação humana (`Pending`) | `Approved / Implementable / Not Started / Not Ready` | `/Users/marcelocostamiranda/source/EKM-guidelines/docs/experiments/COORDINATED-ACTOR-MODEL.md`, versão 0.4 | Aprovação humana registrada; desvios documentais conhecidos do Analista aceitos como não bloqueantes e preservados para avaliação posterior | `Pending` |

## 3. Autoria da especificação

- Autor: Autor da Especificação.
- Checkpoint de entrada inicial:
  `main@3fde52003d23b76d4a76be33e6b416beca0f1a7c`.
- Decisões humanas recebidas: rota `PUT
  /api/v1/devices/{device_id}/settings/reset`, sem body; remoção de todas as
  linhas específicas do device em `DeviceSettings`; resolução da identificação
  pública para chave interna; preservação de settings não específicos;
  retornos `204/404`; idempotência.
- Fatos e fontes consultadas: controllers, contratos, repositórios e queries de
  device settings; mapa, dossiê e EKM externa.
- Lacunas indispensáveis no primeiro handoff: nenhuma declarada.
- Opções não solicitadas ou itens fora de escopo: body de `404` e telemetria.
- Estado produzido: `Proposed / Pending Review / Not Started / Not Ready`.
- Checkpoint de saída inicial:
  `eb5ed262dfa830f62aa936bb02ce7420780fdd3d`.
- Checkpoint de entrada da autoria corretiva:
  `a3cbb556d3388d2987da1e87b46c20c97945ff65`.
- Decisões humanas adicionais:
  - `tests/Api.Tests` está `Retired` em todo o repositório desde 25/07/2026 e
    deve ser ignorado em todas as situações;
  - o reset exige build de `src/Api/Api.csproj`, inspeção da
    query/repositório, verificação de alteração exclusiva no device alvo e
    validação funcional ou integrada de sucesso, ausência de linhas, `404` e
    idempotência;
  - body de `404` e telemetria permanecem fora de escopo.
- Checkpoint de saída da autoria corretiva:
  `b7238c557578fcab1d5ff13e524a16e87cc3ff47`.

O Autor não alegou implementabilidade. A seção 13 retornou a `Pending Review`
após a correção normativa.

## 4. Engenheiro Analista

### 4.1 Primeira Technical Readiness Review

- Responsável: Engenheiro Analista.
- Checkpoint de entrada:
  `spec/device-settings-reset@eb5ed262dfa830f62aa936bb02ce7420780fdd3d`,
  worktree limpo.
- Contrato EKM aplicável: fonte dinâmica vigente em 24/07/2026; a versão exata
  não foi preservada no registro histórico.
- Resultado do gate de admissão: `Not Applicable`, pois o contrato anterior
  não separava formalmente o gate da revisão.
- Divergências de admissão: nenhuma registrada.
- Resultado da Technical Readiness Review: `Implementable`.
- Registro integral: seção 13 da especificação no checkpoint
  `a3cbb556d3388d2987da1e87b46c20c97945ff65`.
- Requisitos e dimensões analisados: `DSR-001` a `DSR-010`, compatibilidade,
  dependências, DI e viabilidade das validações.
- Natureza da lacuna: `Tooling` e `Evidence`; na data da revisão,
  `tests/Api.Tests` não compilava e ainda não existia decisão humana de
  descontinuação nem evidência substituta.
- Classificação histórica da decisão: `Blocking`.
- Lacuna necessária à época: sanear a suíte ou declarar evidência alternativa.
- Comandos e verificações executados: inspeção de contratos HTTP,
  repositórios, queries e baseline; execução da suíte. O comando exato não foi
  preservado pelo registro anterior e não é inferido nesta normalização.
- Resultado relevante: falha de compilação da suíte histórica por chamadas de
  `DeviceMetricsController.Save` sem `deviceId`.
- Operações Git e externas: atualização documental e commit do parecer; nenhuma
  alteração de implementação.
- Artefatos temporários: nenhum registrado.
- Reconciliação de saída: `Technical readiness` alterado para
  `Needs Clarification`, seção 13 preenchida e retorno à autoria.
- Checkpoint de saída:
  `a3cbb556d3388d2987da1e87b46c20c97945ff65`.
- Gate seguinte executado: retorno à autoria e decisão humana.

### 4.2 Nova Technical Readiness Review

- Responsável: Engenheiro Analista.
- Checkpoint de entrada:
  `spec/device-settings-reset@535e376e961574c449e9ed4bcb283db1ae66d5ed`,
  worktree limpo.
- Contrato EKM aplicável:
  `/Users/marcelocostamiranda/source/EKM-guidelines/docs/experiments/COORDINATED-ACTOR-MODEL.md`,
  versão 0.4.
- Resultado do gate de admissão: `Accepted`.
- Divergências de admissão: nenhuma.
- Resultado da Technical Readiness Review: `Needs Clarification`.
- Registro integral: seção 13 de `SHD-SETTINGS-RESET-001@0.1`.
- Requisitos e dimensões analisados: `DSR-001` a `DSR-010`, contratos HTTP,
  persistência, dependências/DI, compatibilidade e viabilidade das validações
  obrigatórias.
- Natureza das lacunas: `None` (sem lacuna normativa indispensável).
- Ocorrência operacional observada: `Tooling` (falha de restore no build
  canônico durante a análise).
- Classificação de dúvidas e decisões declaradas:
  `Non-blocking`, `Out of scope` e `Unrequested option`, sem lacuna normativa
  indispensável.
- Lacunas ou decisões necessárias:
  - decisão necessária: `NONE`;
  - repetição de `dotnet build src/Api/Api.csproj` registrada como validação
    obrigatória pendente da etapa do Engenheiro Implementador.
- Comandos e verificações executados:
  - `git branch --show-current`;
  - `git rev-parse HEAD`;
  - `git status --porcelain`;
  - leituras integrais de `AGENTS.md`, fontes EKM externas obrigatórias,
    templates aplicáveis e fontes locais do mapa;
  - inspeções de controllers, contratos, DI, repositórios e queries do domínio
    de device settings;
  - `dotnet build src/Api/Api.csproj`.
- Resultados e saídas relevantes:
  - admissão `Accepted`;
  - matriz DSR completa preenchida na seção 13.2 com requisitos classificados;
  - build canônico executado com ocorrência operacional de restore
    (`exit_code=1`, `Restaurar falhou em 300,9s`), sem erros de compilação de
    código reportados;
  - resultado binário da TRR mantido como `Implementable`, pois não houve
    ambiguidade normativa nem decisão funcional ausente.
- Operações Git e externas: atualização documental e commit de checkpoint desta
  etapa; nenhuma alteração de implementação e nenhuma operação externa.
- Artefatos temporários criados, alterados ou removidos: criação transitória de
  `Library/Application Support/Microsoft/DeveloperTools/deviceid` pelo ambiente
  de build; diretório não rastreado `Library/` removido antes do checkpoint,
  sem persistência no repositório.
- Reconciliação de saída:
  - metadado `Technical readiness` ajustado para `Implementable`;
  - seção 13.2 preenchida integralmente;
  - transação reconciliada com gate `Accepted`, TRR concluída, ocorrência
    operacional de tooling registrada e decisão necessária `NONE`;
  - worktree contendo apenas artefatos documentais autorizados desta etapa.
- Checkpoint de saída: `Pending`.
- Gate seguinte: aprovação humana para implementação.

## 5. Aprovação humana para implementação

- Resultado: `Approved`.
- Responsável: Marcelo Miranda.
- Data: 2026-07-25.
- Especificação e versão: `SHD-SETTINGS-RESET-001@0.1`.
- Technical Readiness Review aprovada: resultado `Implementable` registrado na
  seção 13 de `docs/specs/DEVICE-SETTINGS-RESET.md` no checkpoint
  `8d7b2fdf5e7e00a10cb30b4e6ad4f5ae0dd603e1`.
- Baseline abrangido:
  `spec/device-settings-reset@8d7b2fdf5e7e00a10cb30b4e6ad4f5ae0dd603e1`,
  worktree limpo.
- Limites ou ressalvas:
  - implementar exclusivamente `DSR-001` a `DSR-010`;
  - não executar, reparar, evoluir ou usar `tests/Api.Tests` como evidência ou
    bloqueio;
  - executar o build canônico `dotnet build src/Api/Api.csproj`;
  - produzir as demais evidências obrigatórias da seção 10 da especificação;
  - preservar como desvios aceitos desta execução as inconsistências
    documentais já identificadas no registro do Analista; elas não bloqueiam
    esta promoção e serão avaliadas pelo Validador de Integridade da EKM;
  - o Implementador não está autorizado a reescrever o parecer do Analista.
- Checkpoint aprovado para implementação: `Pending` (o SHA desta aprovação
  integra o próprio checkpoint e será informado no handoff).

Esta seção registra a decisão humana explícita de aprovar a especificação e o
checkpoint acima para implementação.

## 6. Engenheiro Implementador

- Responsável: Engenheiro Implementador.
- Checkpoint de entrada: `spec/device-settings-reset@12bde0a5b48046b763ea6a098e8e630a547b3bfe`, worktree limpo.
- Estados reconfirmados: `Approved / Implementable / Not Started / Not Ready`.
- Transação: `EKM-CHG-0002`, estado `Open`.
- Aprovação humana aplicável: Marcelo Miranda, 25/07/2026, registrada na seção 5 para `SHD-SETTINGS-RESET-001@0.1`.
- Reconfirmação do baseline: concluída. A comparação entre o checkpoint da TRR aprovada `8d7b2fdf5e7e00a10cb30b4e6ad4f5ae0dd603e1` e o checkpoint de entrada confirmou somente a transição documental para `Approved` e o registro da aprovação humana; o contrato DSR-001 a DSR-010 não sofreu mudança material.
- Resultado: `Implemented`.

| Requisitos | Alteração | Evidência |
|---|---|---|
| `DSR-001`, `DSR-002`, `DSR-003` | `DeviceSettingsController` recebeu `ResetDeviceSettingsAsync` com `[HttpPut("reset")]` no controller existente e sem parametro `[FromBody]`. | Rota resultante `PUT /api/v1/devices/{device_id}/settings/reset`; inspecao do diff e diagnosticos sem erros nos arquivos alterados. |
| `DSR-004` | `IDeviceSettingsRepository.ResetAsync` e `DeviceSettingsRepository.ResetAsync` resolvem a chave interna antes da remocao. | Query `GetDeviceKeyByDeviceId`: `SELECT Id FROM Devices WHERE DeviceId = @DeviceId`; ausencia da chave retorna `false`. |
| `DSR-005`, `DSR-006` | A remocao usa a query `DeleteDeviceSettingsByDeviceKey`. | A unica escrita adicionada e `DELETE FROM DeviceSettings WHERE DeviceId = @DeviceKey`; `Devices` e somente lida, e nenhuma tabela de settings globais, herdados ou padrao e escrita. |
| `DSR-007`, `DSR-008` | O repositorio retorna `true` para device resolvido, mesmo com `DELETE` sem linhas afetadas; a action retorna `NoContent()`. | Inspecao do fluxo: nao ha verificacao de quantidade de linhas removidas antes do `204`. |
| `DSR-009` | A action converte `false` do repositorio em `NotFound()`. | Inspecao do fluxo de ausencia da chave interna. |
| `DSR-010` | Repeticoes resolvem o mesmo device e executam o mesmo `DELETE` restrito, que permanece sem efeito apos o primeiro reset. | Inspecao da query e do fluxo `true -> NoContent()` sem dependencia de linhas afetadas. |

- Arquivos alterados: `src/Api/Controllers/DeviceSettingsController.cs`; `src/Core/Contracts/Repositories/IDeviceSettingsRepository.cs`; `src/Data.Repositories/Repositories/DeviceSettingsRepository.cs`; `src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs`; `docs/specs/DEVICE-SETTINGS-RESET.md`; este registro.
- Decisoes mecanicas: `ResetAsync` retorna `bool` para separar device inexistente de device existente sem settings; a chave interna e lida como `long?`; nao foram adicionados body, payload de erro, telemetria ou refatoracao fora do recorte.
- Inspecao da query e repositorio: `GetDeviceKeyByDeviceId` resolve somente `Devices.Id` pela identificacao publica; `DeleteDeviceSettingsByDeviceKey` altera somente `DeviceSettings` pelo identificador interno; o repositorio fecha a conexao no `finally` como os demais metodos existentes.
- Validacoes executadas:
  - `dotnet build src/Api/Api.csproj`: executado; falhou no restore apos `301,0s`, sem erro de compilacao de codigo reportado.
  - diagnosticos dos quatro arquivos alterados: sem erros.
  - `git diff --check`: executado; em arquivo legado `CRLF`, o Git padrao classificou o `CR` de novas linhas como whitespace final. A verificacao equivalente `git -c core.whitespace=cr-at-eol diff --check` passou, sem whitespace real.
  - inspecao do diff, query e repositorio: confirmou que a unica mutacao e o `DELETE` restrito a `DeviceSettings` do device resolvido.
- Validacoes pendentes: validacao funcional ou integrada de sucesso, device existente sem linhas, `404` e idempotencia. O ambiente nao fornece banco isolado nem fixture autorizada; a configuracao local existente nao autoriza executar `DELETE` contra dados desconhecidos. A tentativa complementar `dotnet build src/Api/Api.csproj --no-restore` foi encerrada sem resultado final pelo controle do terminal e nao e usada como evidencia.
- Suite retirada: `tests/Api.Tests` nao foi executada, alterada, reparada nem usada como evidencia.
- Artefatos temporarios: o build criou `Library/Application Support/Microsoft/DeveloperTools/deviceid`; o diretorio nao rastreado `Library/` foi removido antes da reconciliacao. Nenhum artefato temporario permanece no worktree.
- Operacoes Git e externas: leituras de `git branch --show-current`, `git rev-parse HEAD`, `git status --porcelain`, `git show` e `git diff`; verificacoes de integridade do diff; commit de checkpoint autorizado para formar a saida desta etapa; nenhuma operacao de rede, deploy, push, merge, tag ou alteracao de branch.
- Reconciliacao: o diff esta limitado aos quatro arquivos de implementacao, ao estado da especificacao e a esta secao 6; a secao 13 da especificacao e o parecer do Engenheiro Analista foram preservados sem alteracao. Checkpoint de saida: a ser formado por commit autorizado; proximo gate: Engenheiro Tech Lead.

## 7. Engenheiro Tech Lead

- Responsável: Engenheiro Tech Lead.
- Checkpoint de entrada: `spec/device-settings-reset@f1586fae9b068ca28b39cd5f356b6fcb3a54d96e`, worktree limpo.
- Validações repetidas:
  - checkpoint: `git branch --show-current` = `spec/device-settings-reset`; `git rev-parse HEAD` = `f1586fae9b068ca28b39cd5f356b6fcb3a54d96e`; `git status --porcelain` sem saída antes da atuação;
  - revisão independente do diff completo `12bde0a5b48046b763ea6a098e8e630a547b3bfe..f1586fae9b068ca28b39cd5f356b6fcb3a54d96e`: seis arquivos, sendo quatro de implementação e dois registros autorizados;
  - `dotnet build src/Api/Api.csproj`: falhou no restore após `301,0s`; não houve resultado de compilação utilizável;
  - `git diff --check 12bde0a5b48046b763ea6a098e8e630a547b3bfe f1586fae9b068ca28b39cd5f356b6fcb3a54d96e`: reportou `CRLF` como whitespace final nas linhas novas de `DeviceSettingsController.cs`; essa saída é compatível com o arquivo legado e não demonstra whitespace material por si só;
  - não foi executado `tests/Api.Tests`; não houve operação de banco, pois não existe ambiente isolado autorizado.
- Parecer: `Não verificável`.

| Requisito ou dimensão | Resultado | Evidência | Severidade | Ação necessária |
|---|---|---|---|---|
| `DSR-001` a `DSR-003` | `CONFORME` por inspeção estática | `DeviceSettingsController.ResetDeviceSettingsAsync` está no controller existente, com `[HttpPut("reset")]` e sem parâmetro `[FromBody]`; a rota composta é `PUT /api/v1/devices/{device_id}/settings/reset`. | `BAIXA` | `NENHUMA` |
| `DSR-004` | `CONFORME` por inspeção estática | `GetDeviceKeyByDeviceId` consulta `Devices.Id` por `Devices.DeviceId`; `ResetAsync` só monta o `DELETE` após obter a chave interna. | `BAIXA` | `NENHUMA` |
| `DSR-005` e `DSR-006` | `CONFORME` por inspeção estática | A única mutação nova é `DELETE FROM DeviceSettings WHERE DeviceId = @DeviceKey`; não há escrita em `Devices`, settings globais, herdados ou padrão. | `BAIXA` | `NENHUMA` |
| `DSR-007`, `DSR-008` e `DSR-010` | `CONFORME` por inspeção estática | Após resolução do device, o repositório retorna `true` sem depender do total de linhas removidas; a action retorna `NoContent()`, inclusive em repetição. | `BAIXA` | `NENHUMA` |
| `DSR-009` | `CONFORME` por inspeção estática | Ausência de chave interna produz `false` em `ResetAsync`, convertido em `NotFound()` pela action. | `BAIXA` | `NENHUMA` |
| Cancelamento, DI e falhas de infraestrutura | `CONFORME` por inspeção estática | `CancellationToken` é propagado aos dois `CommandDefinition`; `IDeviceSettingsRepository` já está registrado como scoped; exceções não são suprimidas e seguem o middleware global. | `BAIXA` | `NENHUMA` |
| Compatibilidade e escopo do diff | `CONFORME` | O diff adiciona o endpoint e o fluxo de persistência correspondente, preserva `GET` e `PUT` existentes e não altera schema, migrações, automação, deploy ou suíte retirada. | `BAIXA` | `NENHUMA` |
| Build canônico obrigatório | `NÃO VERIFICÁVEL` | A repetição de `dotnet build src/Api/Api.csproj` falhou no restore após `301,0s`; `--no-restore` não foi usado como substituto. | `ALTA` | Disponibilizar restore funcional e repetir o build canônico no checkpoint aplicável. |
| Validação funcional/integrada de sucesso, device sem linhas, `404`, idempotência e preservação efetiva dos demais settings | `NÃO VERIFICÁVEL` | A especificação exige essa evidência; não há banco isolado, fixture ou autorização para executar `DELETE` contra a configuração disponível. | `ALTA` | Arquiteto/Coordenação devem disponibilizar ambiente isolado e autorização, executar o procedimento da seção 9 e registrar evidências. |
| Integridade textual | `RISCO NÃO BLOQUEANTE` | `git diff --check` marcou `CRLF` nas linhas novas do controller. O relatório do Implementador descreve a mesma característica de arquivo legado; a verificação equivalente com `cr-at-eol` permanece pendente de confirmação nesta atuação. | `MÉDIA` | Confirmar a checagem configurada no checkpoint de saída; não requer alteração funcional. |

- Consistência do relatório do Implementador: `Parcialmente confirmada`. O escopo, o fluxo do repositório, a ausência de execução da suíte retirada e a falha de restore foram confirmados. As evidências funcionais pendentes permanecem realmente ausentes; a alegação de verificação com `cr-at-eol` não foi reproduzida como evidência conclusiva nesta atuação.
- Mudanças não autorizadas: `Nenhuma identificada`. O diff está restrito aos quatro artefatos de implementação necessários, ao estado `Implemented` da especificação e ao relatório do Implementador.
- Recorte corretivo: `Não aplicável ao código`. O retorno exige evidência operacional: restore/build canônico funcional, ambiente isolado autorizado e validação funcional dos cenários obrigatórios. Alteração de implementação somente é cabível se essa evidência revelar desvio.
- Checkpoint de saída: `Pending`; este registro integra o próprio commit de checkpoint.
- Próximo gate: Arquiteto/Coordenação para prover evidências pendentes. Não segue ao Validador de Integridade da EKM enquanto o parecer permanecer `Não verificável`.

## 8. Validador de Integridade da EKM

- Responsável: `Pending`.
- Checkpoint de entrada: `Pending`.
- Controles, conclusão, não conformidades, evidências ausentes e checkpoint de
  saída: `Pending`.

## 9. Validação funcional e operacional

- Responsável humano: Marcelo Miranda.
- Ambiente e checkpoint: `Pending`.
- Procedimento requerido: sucesso do reset, device existente sem linhas,
  device inexistente com `404`, repetição idempotente e comprovação de que
  somente `DeviceSettings` do device alvo foi alterada.
- Resultado, evidências, desvios e estado recomendado: `Pending`.

## 10. Integração e encerramento

- Referência de produção: `main`.
- Autorização, commit, PR ou merge de integração: `Pending`.
- Especificação integrada: `Pending`.
- Estado normativo: `Approved`.
- Estado da implementação: `Not Started`.
- Estado da entrega: `Not Ready`.
- Mapa e lacunas reconciliados: `Pending`.
- Operações externas e deploy: nenhum autorizado.
- Estado final da transação: `Open`.
- Critério de encerramento: integração comprovada em `main`, estados
  reconciliados e gates concluídos.

## 11. Pendências, desvios e histórico corretivo

- A primeira TRR resultou em `Needs Clarification` no checkpoint
  `a3cbb556d3388d2987da1e87b46c20c97945ff65`.
- A decisão humana de 25/07/2026 classificou `tests/Api.Tests` como `Retired`
  globalmente. A suíte não deve ser executada, reparada, evoluída, usada como
  evidência ou tratada como bloqueio; seus arquivos permanecem históricos.
- A autoria corretiva incorporou as evidências substitutas e produziu
  `b7238c557578fcab1d5ff13e524a16e87cc3ff47`.
- Esta correção documental, iniciada em
  `b7238c557578fcab1d5ff13e524a16e87cc3ff47`, normalizou a transação, restaurou
  o formulário 0.4 e propagou a decisão global para `AGENTS.md`,
  `docs/rfc/KNOWLEDGE-MAP.md` e `docs/specs/SYSTEM-DOSSIER.md`.
- Checkpoint de saída desta correção: `Pending`; será completado pela
  Coordenação no próximo handoff, sem reescrever o próprio commit.
- A referência de `src/SmartHome-Api.sln` à suíte descontinuada é discrepância
  legada em `EKM-GAP-0003`, não reativa a suíte e não bloqueia a nova TRR.
- A autoridade do schema/view MySQL permanece aberta em `EKM-GAP-0002`.
- Nova Technical Readiness Review integral concluída com `Implementable`.
- A falha de restore no build canônico foi registrada como ocorrência
  operacional de `Tooling`, sem lacuna normativa e sem decisão funcional
  pendente.
- Em 25/07/2026, Marcelo Miranda aprovou explicitamente a atuação do Engenheiro
  Analista e a implementação de `SHD-SETTINGS-RESET-001@0.1` sobre
  `spec/device-settings-reset@8d7b2fdf5e7e00a10cb30b4e6ad4f5ae0dd603e1`.
- As inconsistências documentais identificadas no registro do Analista foram
  aceitas como desvios não bloqueantes deste experimento, sem correção neste
  gate. Sua avaliação permanece destinada ao Validador de Integridade da EKM.

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
