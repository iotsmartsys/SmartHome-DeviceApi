# Dossiê do Sistema — SmartHome-DeviceApi

**Tipo:** Informativo com decisões confirmadas identificadas

**Status:** Draft

**Última auditoria:** 23/11/2026

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
- configura output cache com base de dois segundos;
- inicia `DatabaseWatchdogService`;
- mapeia controllers, timezone e health;
- usa `ExceptionHandler`;
- cancela a aplicação em encerramento do processo.

O watchdog consulta o banco a cada dois minutos e encerra o processo após três
falhas consecutivas, permitindo reinício pelo orquestrador.

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
| MySQL | bootstrap, repositórios e connection string de runtime | Mapped parcialmente |
| GitHub Actions e GHCR | workflow de build e publicação de imagem | Mapped |
| Portainer | webhook posterior à publicação | Mapped parcialmente |
| Docker Swarm e Traefik | compose de deploy e labels de roteamento | Mapped parcialmente |
| Azure Container Registry | script local de build e deploy | Legado ou uso atual não confirmado |

Timeouts, autenticação das integrações e ownership operacional ainda não possuem
fonte normativa consolidada.

## 8. Falhas, segurança e recuperação

**Fatos observados:**

- exceções de argumento, ausência e falhas MySQL são convertidas em respostas
  HTTP pelo middleware;
- timeouts e perdas críticas de banco podem solicitar encerramento do processo;
- cada erro tratado recebe um identificador de rastreamento na resposta;
- o watchdog também encerra o processo diante de falhas consecutivas;
- o CORS está configurado para permitir qualquer origem, header e método;
- endpoints OAuth existem, mas não foi observada configuração global de
  autorização no bootstrap;
- há configuração sensível em artefato versionado de deploy.

A política pretendida de autenticação, autorização, exposição CORS, segredos e
rotação não pode ser inferida apenas do código. O risco de configuração sensível
está registrado em `EKM-GAP-0004`.

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
| Devices | [devices.md](../knowledge/devices.md) | Código mapeado; sem especificação normativa geral; diferenças de DTO/settings registradas. |
| Capabilities e histórico | [capabilities.md](../knowledge/capabilities.md) | Inclui relações, filtros e histórico; rota por nome e resposta vazia têm divergências registradas. |
| Capability Types | [capability-types.md](../knowledge/capability-types.md) | [Contrato 0.2](CAPABILITY-TYPE-ID.md) Done; estado preservado. |
| Device Metrics | [device-metrics.md](../knowledge/device-metrics.md) | Validações e queries mapeadas; schema geral continua parcial. |
| Device Settings e Settings globais | [settings.md](../knowledge/settings.md) | [Reset 0.1](DEVICE-SETTINGS-RESET.md) cobre reset específico; view/precedência não integralmente confirmadas. |
| Properties | [properties.md](../knowledge/properties.md) | Escopo por device mapeado; sem contrato EKM geral localizado. |
| Groups | [groups.md](../knowledge/groups.md) | [Contrato 0.1](GROUPS-MAINTENANCE-SUPPORT.md) concluído; associações documentadas separadamente. |
| Platforms e Monitored Places | [platforms-and-places.md](../knowledge/platforms-and-places.md) | Endpoints de leitura mapeados; relações/constraints incompletas. |
| Smart Home Capabilities | [smart-home-capabilities.md](../knowledge/smart-home-capabilities.md) | Filtros e escopo de consulta individual têm limitações registradas. |
| Dashboards e widgets | [dashboards-and-widgets.md](../knowledge/dashboards-and-widgets.md) | [Contrato 0.3](DASHBOARD-API-V1.md); aceite operacional pendente no registro anterior. |
| Endpoints utilitários | [utility-endpoints.md](../knowledge/utility-endpoints.md) | Sem contrato funcional próprio; health não verifica banco. |
| Controle de acesso legado | [legacy-access-control.md](../knowledge/legacy-access-control.md) | `Deprecated` por decisão humana; operação atual é contexto informado, não uso demonstrado no repo. |
| Operação | Docker, workflow e scripts | Caminhos de publicação distintos; vigência e ownership não confirmados. |

Nenhum desses mapas declara completude funcional, execução de testes ou aceite
operacional além do que decisão e fonte normativa citadas sustentam.

## 11. Riscos, legado e preparação futura

- ausência de especificações normativas para contratos funcionais existentes;
- schema MySQL completo e processo de migração não localizados;
- cobertura automatizada limitada entre os domínios;
- política de segurança e segredos não consolidada;
- OpenAPI/Swagger é configurado no bootstrap e habilitado em Development; não há
  documento OpenAPI versionado integral nesta baseline;
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
