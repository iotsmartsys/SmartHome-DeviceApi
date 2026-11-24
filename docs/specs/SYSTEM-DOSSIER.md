# Dossiê do Sistema — SmartHome-DeviceApi

**Tipo:** Informativo com decisões confirmadas identificadas

**Status:** Draft

**Última auditoria:** 24/11/2026

## 1. Resumo executivo

**Fato observado:** o SmartHome-DeviceApi é uma API HTTP em ASP.NET Core para
gerenciamento de devices e recursos associados do ambiente IoT SmartHome. A
solução também expõe capabilities, properties, settings, grupos, métricas,
plataformas, locais monitorados, dashboards e endpoints utilitários. O
componente de controle de acesso próprio tem rotas técnicas `oauth` e é
classificado como controle de acesso proprietário legado — `Deprecated`,
conforme decisão humana.

**Decisão confirmada:** a branch `main` é a referência de produção.

## 2. Escopo e suporte observado

| Item | Situação observada | Natureza | Fonte |
|---|---|---|---|
| Runtime | .NET 9 / ASP.NET Core | Fato | `src/Api/Api.csproj` |
| Persistência selecionada no bootstrap | MySQL por `MySqlConnector` e Dapper | Fato | `src/Api/Program.cs`, `src/Data.Repositories/DI/MySqlDependencyInjection.cs` |
| API | Controllers e rotas HTTP, predominantemente sob `/api/v1` | Fato | `src/Api/Controllers/`, `src/Api/Program.cs` |
| Execução em container | Imagem Linux baseada nos runtimes oficiais .NET 9 | Fato | `src/Api/Dockerfile` |
| Orquestração observada | Docker Swarm com Traefik e Portainer no fluxo de publicação | Fato | `docker-compose.swarm.yaml`, `.github/workflows/docker-api.yml` |
| Referência de produção | `main` | Decisão confirmada | Arquiteto, 24/07/2026 |

Plataformas de cliente, versões mínimas suportadas e política de
retrocompatibilidade ainda não possuem fonte normativa local.

## 3. Arquitetura

**Fatos observados:**

- `src/Api` contém bootstrap, controllers, modelos HTTP, middleware e serviço
  hospedado;
- `src/Core` contém entidades, contratos de repositório, exceções de domínio e
  serviço de inclusão de capabilities;
- `src/Data.Repositories` implementa persistência com Dapper e
  `IDbConnection`;
- `tests/Api.Tests` contém um projeto xUnit histórico, classificado por decisão
  humana como `Retired` em todo o repositório;
- o bootstrap registra os repositórios MySQL por injeção de dependência;
- existe composição alternativa para SQL Server, mas ela não é selecionada por
  `src/Api/Program.cs`.

As referências funcionais internas por domínio estão em [`docs/knowledge/`](../knowledge/). São informativas: descrevem código e evidências disponíveis, mas não substituem especificações normativas nem confirmam intenção não registrada.

O fluxo predominante observado é:

```text
requisição HTTP
    ↓
controller/modelo da API
    ↓
contrato ou serviço do Core
    ↓
repositório Dapper
    ↓
MySQL
```

## 4. Entradas e ciclo de vida

`src/Api/Program.cs`:

- obtém a connection string `Devices`;
- registra Core, repositórios MySQL, cache, controllers, OpenAPI e Swagger;
- habilita CORS para qualquer origem, header e método;
- configura política base de output cache com expiração de dois segundos e a
  registra no pipeline depois do CORS;
- inicia `DatabaseWatchdogService`;
- mapeia controllers, timezone e health;
- usa `ExceptionHandler`;
- cancela a aplicação em encerramento do processo.

O watchdog consulta o banco a cada dois minutos e encerra o processo após três
falhas consecutivas, permitindo reinício pelo orquestrador.

No ASP.NET Core 9, a política base inclui a política padrão: só tenta armazenar
GET/HEAD sem header `Authorization` e sem identidade autenticada; só persiste
resposta `200` que não envie `Set-Cookie`. Sua expiração configurada é de dois
segundos. A chave varia por método, esquema, host/porta, path e todos os parâmetros da
query; não varia por identidade, cookie de requisição ou outros headers, pois
nenhuma dessas dimensões foi configurada.
Logo, um GET `200` elegível pode refletir estado anterior por até dois segundos
após uma escrita. `204`, redirecionamentos, erros e demais respostas não-200 não
são gravados, mas uma resposta `200` já armazenada permanece até expirar.

O pipeline local não configura autenticação/autorização ASP.NET. Se o APISIX
preservar `Authorization`, esse header exclui a requisição do output cache; se a
identidade externa depender de cookie ou header próprio, o código local não a
usa na chave nem confirma isolamento entre identidades. A configuração do
gateway e o efeito implantado não foram observados nesta consolidação. O
comportamento decorre da [configuração de política base documentada pelo
ASP.NET Core 9](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-9.0)
e da [política padrão](https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Middleware/OutputCaching/src/Policies/DefaultPolicy.cs),
[regra padrão de variação por host](https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Middleware/OutputCaching/src/CacheVaryByRules.cs)
e [composição da chave](https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Middleware/OutputCaching/src/OutputCacheKeyProvider.cs)
do framework. Não houve observação de runtime nesta consolidação.
`CapabilityController.GetAllCapabilitiesTiny` também mantém um `IMemoryCache`
próprio com TTL de dois segundos e chave que omite filtros; ele pode interagir
com a camada de output cache e tem seu limite descrito na referência do domínio.

## 5. API pública e consumidores

**Superfície observada:**

- devices;
- capabilities e histórico;
- dashboards globais, widgets, catálogo e dados atuais (implementação 0.3; validação operacional pendente);
- settings globais;
- settings específicos por device;
- properties por device;
- métricas atuais e históricas por device;
- grupos e relações com capabilities;
- tipos de capability;
- plataformas e locais monitorados;
- capabilities por smart home;
- OAuth;
- timezone e health.

Índice do conhecimento funcional: [KNOWLEDGE-MAP](../rfc/KNOWLEDGE-MAP.md). As referências detalhadas por domínio estão em [docs/knowledge](../knowledge/).

`Readme.md` documenta parcialmente a API de devices. Swagger/OpenAPI é
habilitado em ambiente de desenvolvimento. Um snapshot derivado fornecido pelo
Arquiteto está versionado em [`docs/contracts/openapi.json`](../contracts/openapi.json);
ele declara OpenAPI 3.0.1 e API 1.0, mas não informa data, ferramenta nem revisão
de geração. Sua procedência e confronto estático integral estão registrados no
[relatório de reconciliação](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md).
O artefato é derivado e informativo, não uma fonte normativa completa para
compatibilidade; especificações funcionais continuam sendo elaboradas
incrementalmente.

A auditoria documental da revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`
inventariou 76 pares método/rota no código e no snapshot; 22 têm tratamento
normativo direto. A revisão atual `48806770fdd486229981ae4973b70632710271e6`
não contém alterações funcionais posteriores. A cobertura descritiva por
operação, suas limitações e a reconciliação dos treze achados estão nos
[relatórios de consolidação](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
O inventário inclui aliases, endpoints utilitários e três rotas OAuth legadas;
presença na aplicação não comprova disponibilidade pública.

## 6. Dados e persistência

**Fatos observados:**

- os repositórios executam SQL com Dapper;
- o runtime principal usa MySQL;
- a chave pública de device é textual, enquanto várias relações persistidas
  usam a chave interna numérica de `Devices`;
- settings globais e específicos por device são persistidos separadamente;
- a leitura de settings efetivos por device usa `v_DeviceEffectiveSettings`;
- métricas usam tabelas corrente e histórica com relacionamento para `Devices`;
- várias operações de escrita usam transações explícitas.

O repositório não preserva o schema MySQL completo. `database/` contém scripts
parciais, e `database/init.sql` usa sintaxe compatível com outro SGBD. A
autoridade do schema e das views está registrada em `EKM-GAP-0002`.

## 7. Integrações e protocolos

| Integração | Evidência observada | Estado documental |
|---|---|---|
| MySQL | bootstrap, repositórios, SQL Dapper e connection string de runtime | Código e comandos descritos por domínio; DDL autoritativo, views, constraints, defaults e cascades ausentes (`EKM-GAP-0002`) |
| GitHub Actions e GHCR | workflow de build e publicação de imagem | Mapped |
| Portainer | webhook posterior à publicação | Mapped parcialmente |
| Docker Swarm e Traefik | compose de deploy e labels de roteamento | Mapped parcialmente |
| Azure Container Registry | script local de build e deploy | Legado ou uso atual não confirmado |

Timeouts, autenticação das integrações e ownership operacional ainda não possuem
fonte normativa consolidada.

Os fluxos HTTP investigados não demonstram publicação MQTT ou chamada a broker
como efeito das operações de settings. O nome de uma configuração, isolado, não
prova uma integração ativa. Comunicação de dispositivos e comportamento de
consumidores externos não são deduzidos da API.

## 8. Falhas, segurança e recuperação

**Fatos observados:**

- exceções de argumento, ausência e falhas MySQL percorrem tratamentos que
  dependem do middleware e da rota; model binding, estado do ar, Dashboard e o
  handler geral podem produzir envelopes distintos;
- timeouts e perdas críticas de banco podem solicitar encerramento do processo;
- o ramo geral inclui `X-Trace-Id`; isso não é garantia uniforme para cada
  resposta de erro, pois há caminhos próprios e respostas de model binding;
- o watchdog também encerra o processo diante de falhas consecutivas;
- o CORS está configurado para permitir qualquer origem, header e método;
- endpoints OAuth existem, mas não foi observada configuração global de
  autorização no bootstrap;
- há configuração sensível em artefato versionado de deploy.

A política pretendida de autenticação, autorização, exposição CORS, segredos e
rotação não pode ser inferida apenas do código. O risco de configuração sensível
está registrado em `EKM-GAP-0004`.

O bootstrap local não aplica uma policy global de autenticação/autorização aos
controllers de negócio. Isso descreve somente a aplicação; admissão, identidade,
permissões, TLS e transformação de erros no APISIX dependem da configuração por
ambiente, não versionada neste repositório. CORS permissivo não comprova acesso
anônimo ou exposição externa. Ver [reconciliação AUD-DEV-009](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-AUDIT-KNOWLEDGE-RECONCILIATION.md).

### Tratamento HTTP de exceções

O `ExceptionHandler` geral é registrado antes do handler específico de
Dashboard. Respostas explícitas de controllers, erros de model binding e
handlers OAuth próprios não passam necessariamente por esse mapeamento.

| Caminho local | Resposta implementada |
|---|---|
| `ArgumentDomainException` | `400`; mensagem textual no corpo, media type configurado como `application/json; charset=utf-8`, header `X-Trace-Id`. O corpo não é um objeto JSON. |
| `NotFoundExceptionDomain` | `404`; mesmo formato textual e header de rastreamento do ramo geral. |
| `MySqlException` 1062 | `409` com mensagem genérica de duplicidade; inclui `X-Trace-Id`. |
| `MySqlException` 1451/1452 | `400`; mensagem sobre relação inválida; inclui `X-Trace-Id`. |
| `MySqlException` 1205 | `503`; mensagem de timeout; solicita parada da aplicação. |
| `MySqlException` 1213, 1040 | `503`; mensagens de deadlock/conexões; não solicitam parada pelo ramo descrito. |
| `MySqlException` 2006/2013 | `503`; mensagem de perda de conexão; solicita parada da aplicação. |
| `TimeoutException` | `503`; solicita parada da aplicação. |
| Exceção não mapeada no handler geral | `500`; mensagem genérica textual, media type JSON, `X-Trace-Id`. |
| `AirConditionerStateConflictException` | `409` com JSON `{"error":"invalid_air_conditioner_stored_state"}`; caminho curto, sem o `X-Trace-Id` do ramo geral. |
| Rotas Dashboard | Handler dedicado produz `{ "error": { "code", "message", "details" } }`; validação, ausência, incompatibilidade, datasource e erro interno usam códigos/status específicos da especificação e `DashboardErrorResponse`. Erro remove `Location`. |
| Model binding / `[ApiController]` | Resultado de validação automática é `400` Problem Details/Validation Problem Details e não recebe necessariamente o header do middleware geral. Resposta pode variar conforme model/converter. |

O handler geral gera um novo identificador e o escreve em `X-Trace-Id` apenas
quando a resposta ainda não começou. Assim, nem todos os erros tratados,
incluindo respostas explícitas e os caminhos próprios acima, oferecem esse
header. Falhas anteriores ao serviço, como APISIX/TLS, têm formato externo.
Erros e respostas não-200 ficam fora do armazenamento padrão do output cache;
um sucesso anterior ainda pode ser servido até o TTL terminar.

## 9. Build, testes e operação

- Solução: `src/SmartHome-Api.sln`.
- Build canônico da API: `dotnet build src/Api/Api.csproj`.
- Validações adicionais: definidas pela especificação aplicável.
- Suíte HTTP Dashboard: `tests/Api.IntegrationTests`; run/cleanup independentes,
  criada e compilada, sem execução nesta entrega.
- Contrato da suíte: [DASHBOARD-API-INTEGRATION-TESTS](DASHBOARD-API-INTEGRATION-TESTS.md).
- Suite histórica: `tests/Api.Tests`, classificada como `Retired`.
- Execução local: `Makefile` carrega `.env` e inicia a API.
- Imagem: `src/Api/Dockerfile`.
- Publicação automatizada: workflow para pull requests, `main` e tags.
- Publicação local adicional: `build.sh`.

Por decisão humana de 25/07/2026, `tests/Api.Tests` deve ser ignorada em todas
as situações: não deve ser executada, reparada, evoluída, usada como evidência
ou tratada como bloqueio. Seus arquivos permanecem apenas como registro
histórico. A referência ainda presente em `src/SmartHome-Api.sln` é uma
discrepância legada e não reativa a suíte. Sua remoção futura e a suficiência
das evidências por domínio permanecem registradas em `EKM-GAP-0003`.

## 10. Domínios e fontes de verdade

O [`KNOWLEDGE-MAP.md`](../rfc/KNOWLEDGE-MAP.md) é o índice de autoridades,
evidências e lacunas. As referências abaixo são informativas; contratos
normativos existentes mantêm autoridade para o comportamento que especificam.

| Domínio | Referência informativa | Estado/limite documental |
|---|---|---|
| Devices | [devices.md](../knowledge/devices.md) | Cinco operações, campos requeridos, projeções e fluxo de criação/escrita descritos; DDL/gateway externos. |
| Capabilities e histórico | [capabilities.md](../knowledge/capabilities.md) | 22 pares método/rota, filtros, associações, cache, history e divergências descritos. |
| Capability Types | [capability-types.md](../knowledge/capability-types.md) | [Contrato 0.2](CAPABILITY-TYPE-ID.md) Done; modelo e cinco operações descritos, estado preservado. |
| Device Metrics | [device-metrics.md](../knowledge/device-metrics.md) | Três operações; DTO, faixas, projeções e paginação descritos; DDL externo. |
| Device Settings e Settings globais | [settings.md](../knowledge/settings.md) | Seis operações; view efetiva, update/upsert/reset e erros descritos; DDL externo. |
| Properties | [properties.md](../knowledge/properties.md) | Seis operações; queries incompatíveis e falha pós-commit descritas; sem contrato EKM geral. |
| Groups | [groups.md](../knowledge/groups.md) | [Contrato 0.1](GROUPS-MAINTENANCE-SUPPORT.md) concluído; CRUD normativo e associações legadas descritos. |
| Platforms e Monitored Places | [platforms-and-places.md](../knowledge/platforms-and-places.md) | Dois endpoints somente leitura e suas projeções descritos; relações/constraints dependem do DDL. |
| Smart Home Capabilities | [smart-home-capabilities.md](../knowledge/smart-home-capabilities.md) | Duas consultas; comparação por nome e ausência de escopo por UID descritas. |
| Dashboards e widgets | [dashboards-and-widgets.md](../knowledge/dashboards-and-widgets.md) | [Contrato 0.3](DASHBOARD-API-V1.md); 12 operações e regras descritas, aceite operacional continua pendente. |
| Endpoints utilitários | [utility-endpoints.md](../knowledge/utility-endpoints.md) | Três endpoints; health sem verificação de DB e watchdog distinto descritos. |
| Controle de acesso legado | [legacy-access-control.md](../knowledge/legacy-access-control.md) | `Deprecated` por decisão humana; três rotas proprietárias e limites descritos, uso atual permanece contexto informado. |
| Operação | Docker, workflow e scripts | Caminhos de publicação distintos; vigência e ownership não confirmados. |

Nenhum desses mapas declara completude funcional, execução de testes ou aceite
operacional além do que decisão e fonte normativa citadas sustentam.

A [matriz consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md)
acompanha as 76 operações e as dimensões de documentação; a reconciliação,
lacunas externas e relatório da intervenção estão no mesmo diretório. Essa
cobertura descreve o código local e não altera o aceite operacional dos
Dashboards nem as autoridades normativas.

## 11. Riscos, legado e preparação futura

- ausência de especificações normativas para contratos funcionais existentes;
- schema MySQL completo e processo de migração não localizados;
- cobertura automatizada limitada entre os domínios;
- política de segurança e segredos não consolidada;
- OpenAPI e Swagger usam caminhos de geração distintos em Development; existe
  snapshot OpenAPI 3.0.1/API 1.0 versionado, fornecido pelo Arquiteto, cuja
  revisão e ferramenta de geração originais não estão identificadas. O snapshot
  e os geradores não representam integralmente os comportamentos observados;
  divergências não foram corrigidas nesta consolidação;
- há 76 pares método/rota no inventário auditado; o snapshot OpenAPI não inclui
  exemplos, e a auditoria identificou checks automatizados em 14/76 operações.
  As referências funcionais agora incluem exemplos sintéticos por família de
  entrada; a [matriz consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md)
  distingue comportamento descrito de dependências externas e não afirma
  execução ou persistência real;
- caminhos distintos de publicação, cuja vigência relativa não está confirmada;
- apontamento absoluto para a EKM externa, adequado ao experimento local, mas
  ainda não portátil.

## 11.1 Divergências observadas na auditoria documental

- `CapabilityController` declara rota de leitura por nome com e sem
  `device_id`, mas a ação requer esse parâmetro; o template global não o fornece.
- GET de Capability History declara `404` em atributo, mas devolve `200` com a
  coleção, inclusive quando vazia.
- Filtro Smart Home compara `smart_home_id` com `sh.Name`; consulta individual
  por UID não aplica esse escopo.
- DTO de Device contém `settings`, mas conversões e criação inspecionadas não
  demonstram leitura/gravação desse campo nesse endpoint.
- Métricas exigem `device_id` no body, mas o mapper não o usa e a chave efetiva
  vem da rota.
- `/api/v1/timezone` devolve a etiqueta `zone` e tempos locais/UTC, sem converter
  o instante ao fuso indicado; `timezone/datetime` não recebe fuso apesar da
  descrição.

As divergências são registros de implementação, não foram corrigidas e não
constituem contratos novos.

A lista anterior resume apenas discrepâncias representativas. O registro
completo dos achados AUD-DEV-001 a AUD-DEV-013 e a respectiva localização
descritiva estão na [matriz de reconciliação](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-AUDIT-KNOWLEDGE-RECONCILIATION.md).

## 12. Questões abertas

| ID | Questão | Impacto | Destino |
|---|---|---|---|
| `EKM-GAP-0001` | Quais contratos existentes devem ser especificados primeiro? | Limita reconstruibilidade e revisão de compatibilidade. | Specification on touch e priorização humana. |
| `EKM-GAP-0002` | Qual é a fonte autoritativa do schema MySQL e das views? | Impede reconstrução completa da persistência. | Inventário e decisão sobre migrações. |
| `EKM-GAP-0003` | Qual evidência automatizada será obrigatória por domínio? | Pode permitir regressões não detectadas. | Critérios de aceite das especificações. |
| `EKM-GAP-0004` | Qual é a política vigente para segredos e configuração operacional? | Risco de exposição e operação insegura. | Decisão e correção operacional autorizadas. |
| `EKM-GAP-0005` | Quando o apontamento da EKM precisará ser portátil? | Limita execução fora desta máquina. | Evolução posterior do experimento. |

## Regra de manutenção

Este dossiê oferece navegação e visão geral. Não substitui especificações
normativas nem transforma o estado atual do código em intenção confirmada.

## Preparação da qualificação de engenharia

[Contrato 0.1](../rfc/REPOSITORY-ENGINEERING-CONTRACT.md) aprovado para capabilities e dependências do ar; [qualificação](../rfc/REPOSITORY-READINESS.md) Ready no recorte após decisão humana de 23/09/2026. Nenhum domínio externo ao alcance foi habilitado. AGENTS.md reconciliado para adoção EKOM 5.0 delimitada, preservando o histórico 4.x.

A habilitação documental de `SHD-KNOWLEDGE-MAP-001@0.1` é um escopo separado
registrado em [Repository Readiness](../rfc/REPOSITORY-READINESS.md); não
qualifica mudanças funcionais nos domínios listados.
