# Dossiê do Sistema — SmartHome-DeviceApi

**Tipo:** Informativo com decisões confirmadas identificadas

**Status:** Draft

**Última auditoria:** 25/07/2026

## 1. Resumo executivo

**Fato observado:** o SmartHome-DeviceApi é uma API HTTP em ASP.NET Core para
gerenciamento de devices e recursos associados do ambiente IoT SmartHome. A
solução também expõe capabilities, properties, settings, grupos, métricas,
plataformas, locais monitorados e endpoints OAuth.

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

`Readme.md` documenta parcialmente a API de devices. Swagger/OpenAPI é
habilitado em ambiente de desenvolvimento. Não foi observada uma fonte
normativa completa para compatibilidade da API; os contratos funcionais devem
ser especificados incrementalmente.

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

| Domínio | Fonte atual | Cobertura | Observação |
|---|---|---|---|
| Devices | Código e `Readme.md` | Inventoried | Sem especificação normativa EKM |
| Capabilities | Código | Inventoried | Inclui tipos, histórico e relações |
| Capability Types por id | `SHD-CAPABILITY-TYPE-ID-001@0.1` | Mapped | Implementação e build concluídos; encaminhada para Revisão, validação HTTP/banco não executada |
| Settings | Código e queries | Mapped | Abrange settings globais, específicos e efetivos |
| Properties | Código | Inventoried | Escopo por device |
| Groups | `SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1` e código | Mapped | Contrato administrativo concluído e validado por decisão do Arquiteto; mantém relações com capabilities |
| Métricas | Código e schema parcial | Mapped | A suíte histórica `tests/Api.Tests` está `Retired` e não constitui evidência |
| OAuth | Código | Inventoried | Contratos e política ainda não especificados |
| Operação | Docker, workflow e scripts | Mapped | Existem caminhos de publicação distintos |

As fontes navegáveis e lacunas correspondentes estão em
`docs/rfc/KNOWLEDGE-MAP.md`.

## 11. Riscos, legado e preparação futura

- ausência de especificações normativas para contratos funcionais existentes;
- schema MySQL completo e processo de migração não localizados;
- cobertura automatizada limitada entre os domínios;
- política de segurança e segredos não consolidada;
- documentação de API parcial;
- caminhos distintos de publicação, cuja vigência relativa não está confirmada;
- apontamento absoluto para a EKM externa, adequado ao experimento local, mas
  ainda não portátil.

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
