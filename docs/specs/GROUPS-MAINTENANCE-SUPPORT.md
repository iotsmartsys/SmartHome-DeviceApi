# Especificação — Suporte da Device API à manutenção de Groups

**ID:** `SHD-GROUPS-MAINTENANCE-SUPPORT-001`

**Classe da fonte:** Normativa

**Versão:** 0.1

**Estado do workflow:** `Draft`

**Análise de implementabilidade:** Pendente

**Bloqueio arquitetural:** Nenhum

**Relações normativas e de dependência:**

- Nova [`New`] — primeiro contrato normativo local da manutenção administrativa
  de Groups na Device API;
- Habilita [`Enables`]
  `AIOTSMARTHOME-GROUPS-MAINTENANCE-001@0.1`, após implementação e validação
  deste contrato e reconciliação da especificação consumidora.

## 1. Objetivo e contexto

Adequar o contrato e a persistência de Groups da Device API para que o
aplicativo AIoTSmartHome possa listar, criar, editar e excluir Groups sem
administrar suas associações com capabilities.

A baseline possui as rotas necessárias, mas diverge do contrato consumidor em
três pontos materiais: retorna `204` para coleção vazia, exige
`capabilities` no modelo recebido pelo POST e não persiste `IconName` no INSERT.
O PATCH também expõe propriedades que não pertencem à manutenção administrativa
e o DELETE não distingue recurso ausente.

## 2. Escopo

- estabilizar `GET /api/v1/groups` para retornar uma coleção, inclusive vazia;
- aceitar criação sem `id` e sem `capabilities`;
- preservar compatibilidade de criação com capabilities fornecidas por
  consumidores anteriores, sem torná-las obrigatórias;
- persistir o ícone opcional durante a criação;
- restringir o PATCH administrativo a `name`, `active` e `icon`;
- permitir remover o ícone por JSON Patch com valor `null`;
- preservar associações com capabilities durante PATCH administrativo;
- distinguir exclusão bem-sucedida de Group inexistente;
- remover atomicamente as associações ao excluir o Group, sem excluir as
  capabilities relacionadas;
- alinhar OpenAPI, respostas HTTP e `ProblemDetails` ao comportamento definido.

## 3. Fora de escopo

- Alterar as rotas de associação de capabilities a Groups.
- Alterar o comportamento funcional de capabilities, devices ou Capability
  Types.
- Remover o `PUT /api/v1/groups/{id}` ou redefinir seu contrato vigente.
- Criar uma nova versão de rota, mecanismo geral de versionamento ou política
  transversal de compatibilidade da API.
- Alterar autenticação, autorização, CORS, middleware geral de exceções ou
  observabilidade da API.
- Definir ou migrar o schema MySQL completo.
- Alterar o aplicativo AIoTSmartHome neste repositório.
- Alterar ou reutilizar `tests/Api.Tests` nesta versão.

### 3.1 Arquitetura e organização

**Precedente aplicável:** fluxo vigente
controller/modelo HTTP → entidade/contrato do Core → repositório Dapper →
MySQL. `CapabilityTypeController` e seus modelos oferecem precedente para
respostas terminais e conversões, sem transferir seus contratos de domínio.

**Elementos preservados:** `GroupController`, `IGroupRepository`, entidade
`Core.Entities.Group`, `GroupRepository`, rotas `/api/v1/groups`, resposta de
leitura contendo capabilities e endpoints separados de associação.

**Desvio arquitetural explícito:** separar representações HTTP de leitura,
criação e patch no domínio de Groups. Hoje `Api.Models.Group` acumula as três
responsabilidades e torna `id` e `capabilities` parte das mutações. A separação
fica limitada ao controller e aos modelos HTTP de Groups; não introduz nova
camada nem padrão transversal.

### 3.2 Limite de escopo funcional

**Capacidades arquiteturais pressupostas:** ASP.NET Core com
`JsonPatchDocument`, Dapper, transações locais do repositório e MySQL capaz de
preservar a atomicidade da exclusão do Group e de suas relações.

**Preparação arquitetural separada:** Não aplicável. A ausência do schema MySQL
autoritativo permanece em `EKM-GAP-0002`; a implementação deve comprovar o
comportamento de exclusão sem tentar resolver a governança geral de migrações.

## 4. Requisitos

### 4.1 Leitura

- **`GRP-API-001`:** `GET /api/v1/groups` deve retornar `200` com um array JSON
  em toda leitura bem-sucedida; quando não houver Groups, o corpo deve ser
  exatamente uma coleção vazia.
- **`GRP-API-002`:** Cada Group retornado deve preservar `id`, `name`, `active`,
  `icon` opcional e `capabilities`, mantendo compatibilidade com consumidores
  que ainda usam as associações na leitura.
- **`GRP-API-003`:** `icon` nulo pode ser omitido pela serialização vigente;
  isso não deve criar ícone substituto nem alterar a persistência.

### 4.2 Criação

- **`GRP-API-004`:** `POST /api/v1/groups` deve aceitar `name`, `active` e
  `icon` opcional sem exigir `id` nem `capabilities`.
- **`GRP-API-005`:** A ausência de `capabilities` no POST deve ser equivalente
  a uma coleção vazia e não deve criar associações.
- **`GRP-API-006`:** Para compatibilidade, se `capabilities` forem fornecidas
  por consumidor anterior, a API deve preservar o comportamento de criação das
  associações na mesma transação. Essa propriedade permanece opcional e não
  integra o payload do novo aplicativo.
- **`GRP-API-007`:** Quando `icon.name` for informado, o INSERT deve persistir
  o valor em `Groups.IconName`. Quando `icon` estiver ausente ou nulo, deve
  persistir ausência de ícone.
- **`GRP-API-008`:** Criação válida deve retornar `200` somente depois do
  commit, com representação contendo o `id` atribuído e os valores persistidos.
- **`GRP-API-009`:** `name` deve ser rejeitado com `400` quando for nulo, vazio
  ou composto apenas por espaços. A API deve remover espaços das extremidades
  antes de persistir. Um objeto `icon` com `name` vazio ou composto apenas por
  espaços também deve produzir `400`.

### 4.3 Edição

- **`GRP-API-010`:** `PATCH /api/v1/groups/{id}` deve aceitar somente operações
  `replace` para `/name`, `/active` e `/icon`.
- **`GRP-API-011`:** Operação, caminho ou valor inválido deve retornar `400`
  com `ProblemDetails` ou `ValidationProblemDetails`, sem executar UPDATE.
- **`GRP-API-012`:** PATCH com `id` não positivo deve retornar `400`; para `id`
  positivo de Group inexistente, deve retornar `404`. Nenhum desses casos pode
  executar UPDATE.
- **`GRP-API-013`:** `replace /icon` com objeto válido deve persistir o nome;
  `replace /icon` com `null` deve persistir `IconName = null`.
- **`GRP-API-014`:** PATCH válido deve atualizar somente os campos
  administrativos alterados, preservar `id` e todas as associações com
  capabilities e retornar `204` após a persistência.
- **`GRP-API-015`:** Após aplicar o patch, `name` e `icon.name` devem obedecer
  aos mesmos guards da criação; falha deve retornar `400` sem persistir estado
  parcial.

### 4.4 Exclusão e falhas

- **`GRP-API-016`:** `DELETE /api/v1/groups/{id}` deve retornar `400` para `id`
  não positivo, `404` para Group inexistente e `204` somente quando o Group
  existente for excluído.
- **`GRP-API-017`:** A exclusão deve remover atomicamente as relações do Group
  em `Group_RelationShipCapabilities`, por cascade comprovado ou transação
  explícita, sem excluir registros de `Capabilities`.
- **`GRP-API-018`:** Falha de persistência não deve produzir resposta de
  sucesso; deve preservar a atomicidade e seguir o tratamento de erro vigente.
- **`GRP-API-019`:** O OpenAPI deve declarar os schemas efetivos de leitura,
  criação e patch e os status `200`, `204`, `400`, `404`, `409` e `500` somente
  nas operações em que possam ocorrer.
- **`GRP-API-020`:** Falhas esperadas de validação e ausência devem oferecer
  `ProblemDetails` ou `ValidationProblemDetails` consumível. Conflitos de
  persistência e falhas inesperadas permanecem sob o middleware vigente.

## 5. Fluxos, estados e contratos

```text
GET /groups
  └── consulta concluída → 200 [GroupResponse] ou 200 []

POST /groups
  ├── payload inválido → 400
  ├── conflito de persistência → 409
  └── commit concluído → 200 GroupResponse

PATCH /groups/{id}
  ├── operação/payload inválido → 400
  ├── Group ausente → 404
  └── UPDATE concluído → 204

DELETE /groups/{id}
  ├── Group ausente → 404
  └── Group e relações removidos atomicamente → 204
```

Payload mínimo de criação:

```json
{
  "name": "Sala",
  "active": true,
  "icon": {
    "name": "sofa.fill"
  }
}
```

`icon` pode ser omitido ou enviado como `null`. `capabilities` também pode ser
omitida; quando presente por compatibilidade, usa o formato já aceito pela
baseline. `id` recebido no POST não participa da criação.

Patch de remoção do ícone:

```json
[
  {
    "op": "replace",
    "path": "/icon",
    "value": null
  }
]
```

Invariantes de persistência:

- criação sem capabilities produz zero relações;
- PATCH administrativo nunca inclui nem regrava relações;
- exclusão remove relações, mas preserva as capabilities;
- nenhuma resposta terminal de sucesso precede o commit correspondente.

## 6. Falhas e condições de borda

- Coleção vazia é `200 []`, nunca `204` ou `404`.
- `id` de rota não positivo é requisição inválida e retorna `400`.
- `name` inválido é rejeitado antes da persistência.
- Ícone ausente permanece ausente; a API não conhece nem atribui o fallback
  visual `house.fill` do aplicativo.
- PATCH vazio, com operação diferente de `replace`, caminho fora da allowlist
  ou tentativa de alterar `id` ou `capabilities` retorna `400`.
- PATCH com múltiplas operações é aplicado somente quando o documento inteiro
  for válido; nenhuma operação parcial pode ser persistida.
- DELETE repetido retorna `204` na primeira exclusão e `404` nas seguintes.
- Falha de foreign key, ausência de cascade ou falha transacional não pode ser
  convertida em exclusão bem-sucedida.
- Conflito de unicidade imposto pela persistência retorna `409`; esta versão
  não cria nova regra de unicidade para `name`.

## 7. Critérios de aceite e validações

### `GRP-API-AC-001` — Leitura preenchida e vazia

**Cobre:** `GRP-API-001` a `003`

- **Dado que** existam zero ou mais Groups, incluindo registros com e sem
  ícone e capabilities;
- **Quando** `GET /api/v1/groups` for chamado;
- **Então** a API retorna `200`, com coleção vazia ou representações completas
  compatíveis;
- **Evidência:** inspeção do contrato e validação integrada autorizada contra
  MySQL descartável.

### `GRP-API-AC-002` — Criação sem associações

**Cobre:** `GRP-API-004`, `005`, `007` a `009`

- **Dado que** o payload contenha nome, active e ícone opcional, sem id nem
  capabilities;
- **Quando** o POST for processado;
- **Então** a API retorna `200` após persistir exatamente um Group, seu ícone
  opcional e nenhuma relação;
- **Evidência:** inspeção de DTO, fluxo transacional e query, seguida de
  validação integrada autorizada.

### `GRP-API-AC-003` — Compatibilidade de criação anterior

**Cobre:** `GRP-API-006`

- **Dado que** um consumidor envie capabilities válidas no POST;
- **Quando** a criação concluir;
- **Então** o Group e suas relações são preservados na mesma transação;
- **Evidência:** inspeção do fluxo e validação integrada autorizada.

### `GRP-API-AC-004` — Patch administrativo e remoção do ícone

**Cobre:** `GRP-API-010` a `015`

- **Dado que** exista um Group com capabilities associadas;
- **Quando** nome, active ou ícone forem substituídos, inclusive ícone por
  `null`;
- **Então** a API retorna `204`, persiste somente os campos administrativos e
  preserva todas as relações;
- **Evidência:** inspeção da allowlist, modelo de patch e UPDATE, seguida de
  validação integrada autorizada.

### `GRP-API-AC-005` — Rejeição de patch inválido

**Cobre:** `GRP-API-010` a `012`, `015`, `020`

- **Dado que** o patch use `id`, operação, caminho ou valor inválido;
- **Quando** a requisição for processada;
- **Então** a API retorna `400` com detalhes consumíveis e não altera o Group;
- **Evidência:** inspeção e validação integrada autorizada, comparando o estado
  antes e depois.

### `GRP-API-AC-006` — Exclusão existente e inexistente

**Cobre:** `GRP-API-016` a `018`

- **Dado que** um Group possua relações com capabilities;
- **Quando** ele for excluído e a exclusão for repetida;
- **Então** a primeira chamada retorna `204`, remove Group e relações sem
  remover capabilities, e a repetição retorna `404`;
- **Evidência:** validação integrada autorizada com MySQL descartável e inspeção
  das linhas relacionadas.

### `GRP-API-AC-007` — OpenAPI coerente

**Cobre:** `GRP-API-019`, `020`

- **Dado que** a API seja construída com os contratos desta versão;
- **Quando** o documento OpenAPI for gerado;
- **Então** schemas, nulabilidade, payloads e status correspondem ao contrato;
- **Evidência:** inspeção do OpenAPI gerado, sem publicação.

### 7.1 Evidências planejadas

- **Artefatos de teste no recorte:** Nenhum. Esta versão não autoriza criar,
  reparar ou alterar projeto de testes.
- Inspeção do delta, dos DTOs, guards, queries, transações e OpenAPI gerado.
- Validação integrada em MySQL descartável dos sete critérios, mediante
  autorização operacional própria; sem execução autorizada, o resultado deve
  permanecer `Not Executed`.
- A implementação deve preservar registros comparativos antes e depois para os
  cenários de PATCH inválido e DELETE com relações.

## 8. Conhecimento afetado

- `src/Api/Controllers/GroupController.cs`;
- modelos HTTP de Groups em `src/Api/Models/`;
- `src/Core/Contracts/Repositories/IGroupRepository.cs` e
  `src/Core/Entities/Group.cs`, somente se necessários ao contrato local;
- `src/Data.Repositories/Repositories/GroupRepository.cs`;
- `src/Data.Repositories/Repositories/Queries/GroupQuery.cs`;
- OpenAPI/Swagger gerado pela API;
- `docs/rfc/KNOWLEDGE-MAP.md`, `docs/specs/SYSTEM-DOSSIER.md` e
  `docs/rfc/EKM-CHANGELOG.md`.

## 9. Relações, decisões, lacunas e débitos

**Fatos observados:** `GET` retorna `204` quando a consulta não encontra
Groups; o modelo HTTP torna `capabilities` obrigatório e sua conversão o
percorre sem guard; o INSERT ignora `IconName`; PATCH usa o modelo completo e
DELETE sempre responde `204`. O README afirma uso de cascade, mas o schema
MySQL autoritativo não está versionado.

**Intenção e decisões confirmadas:** criar nesta API o suporte requerido pela
manutenção de Groups do AIoTSmartHome, preservando leitura de capabilities e
deixando a administração de associações fora do novo aplicativo.

**Solução proposta:** separar os contratos HTTP de leitura, criação e patch;
manter a resposta completa para compatibilidade; aceitar capabilities como
propriedade opcional legada no POST; aplicar allowlist no PATCH; e tornar a
exclusão observável e atômica.

**Decisões pendentes:** Nenhuma para esta versão. Não é criada regra de
unicidade; eventual restrição já existente na persistência permanece
autoridade operacional e seu conflito é exposto como `409`.

**Relações:** `EKM-CHG-0003`, `EKM-GAP-0001`, `EKM-GAP-0002` e
`AIOTSMARTHOME-GROUPS-MAINTENANCE-001@0.1` no repositório consumidor.

**ADRs relacionadas:** Nenhuma. A separação de DTOs é local ao contrato de
Groups; política geral de compatibilidade ou versionamento continua fora do
recorte.

**Autoridades confrontadas:** `AGENTS.md`, `docs/rfc/KNOWLEDGE-MAP.md`,
`docs/specs/SYSTEM-DOSSIER.md` e, no aplicativo consumidor,
`AIOTSMARTHOME-GROUPS-MAINTENANCE-001@0.1` e seu relatório de
implementabilidade.

**Relatórios esperados:** análise, implementação e revisão, se acionada;
validação integrada, quando autorizada.

## 10. Encaminhamento

Esta versão permanece em `Draft` e deve seguir para Análise de
Implementabilidade. Ela não autoriza implementação, build, testes, banco ou
alteração do aplicativo consumidor.
