# Capabilities e histórico

**Estado descrito:** implementação em `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem mudança funcional identificada em `48806770fdd486229981ae4973b70632710271e6`. Referência informativa. A regra de value de estado do ar está normatizada em [AIR-CONDITIONER-STATE](../specs/AIR-CONDITIONER-STATE.md); ela não especifica todo CRUD de Capability. Operações de Group e Dashboard mantêm suas próprias fontes normativas.

**Contrato HTTP derivado:** [OpenAPI](../contracts/openapi.json), informativo; [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). Evidências: [CapabilityController](../../src/Api/Controllers/CapabilityController.cs), [HistoryController](../../src/Api/Controllers/CapabilityHistoryController.cs), [modelos HTTP](../../src/Api/Models/Capability.cs), [entidades Core](../../src/Core/Entities/Capability.cs), [serviço de inclusão](../../src/Core/Services/AddCapabilityService.cs), [repositório](../../src/Data.Repositories/Repositories/CapabilityRepository.cs), [queries](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs) e [builder de filtros](../../src/Data.Repositories/Repositories/Builders/FindCapabilityQueryBuilder.cs).

## Modelo, identidade e filtros

Capability associa nome e value a um Device e Capability Type. A resposta pode incluir `id` numérico interno, `uid`, `device_id`, `type`, `mode`, `value_type`, `active`, `updated_at`, icon, platforms, groups e dados agregados de smart home. Algumas propriedades de saída não entram nas conversões de escrita; não tratar DTO como espelho de persistência. `value` é texto. A interpretação de tipos e a normalização de estado de ar dependem do data type persistido.

O corpo `Capability` possui os campos `capability_name`, `uid`, `description`, `owner`, `device_id`, `type`, `mode`, `value`, `platforms`, `value_type`, `updated_at`, `active`, `icon` e `groups`; `smart_home` é um dicionário iniciado vazio e `Id` não tem setter público. `capability_name`, `device_id` e `type` são referências não anuláveis no record; os demais strings/objetos anotados anuláveis são opcionais. `active` ausente assume `false`. Um `CapabilityUpdate` contém os strings não anuláveis `capability_name` e `value`. Body null/ausente pode ser rejeitado pelo `[ApiController]`; validação de conteúdo adicional depende de cada fluxo.

`CapabilityFind` aceita filtros independentes opcionais `name`, `type`, `owner`, `value`, `active`, `reference_id`, `smart_home_id`, `uid`, `group_name` e `device_id`. O builder combina os predicados presentes. `device_id` é filtro somente quando recebido no query model ou definido pelo método de repository; prefixo de rota, sozinho, não garante escopo.

## Operações HTTP

`CapabilityController` declara dois prefixes: `/api/v1/devices/{device_id}/capabilities` e `/api/v1/capabilities`. A tabela distingue os aliases que compartilham a mesma ação. A existência da rota aninhada não implica vínculo ou autorização pelo Device pai.

| Operação | Comportamento e resposta |
|---|---|
| `GET /api/v1/devices/{device_id}/capabilities` | Ação de lista usa somente `CapabilityFind` da query; não injeta o `device_id` do prefixo. `200` com array se há resultados; `404` se vazio. Para filtrar por Device, enviar `device_id` na query. · [GetAllCapabilities](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005 |
| `GET /api/v1/capabilities` | Mesmo handler e respostas; pode usar os dez filtros opcionais descritos acima. · [controller/builder](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/devices/{device_id}/capabilities/tiny` | Handler de Tiny, sem escopo automático pelo prefixo. Exclui tipos `Message`, `Alexa`, `TIME_OF_DAY`, `Scene`. `format=mcu` (case-insensitive) gera `text/plain` com linhas `name;value;updated_at` e nome de arquivo `capabilities.csv`; demais valores retornam array `CapabilityTiny`. Usa `IMemoryCache` interno por 2 s; a chave omite `smart_home_id`, `uid`, `group_name` e `device_id`, logo filtros distintos nesses campos podem compartilhar entrada. Além disso, o output cache HTTP base pode armazenar resposta 200 elegível por 2 s; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. Se query inicial está vazia retorna 404; se há resultados mas todos são excluídos, retorna 200 vazio. · [GetAllCapabilitiesTiny](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005/006 |
| `GET /api/v1/capabilities/tiny` | Mesmo handler de Tiny e limitações de cache/formato; sem parâmetro de rota de Device. · [controller](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/devices/{device_id}/capabilities/platforms/{reference_id}` | Busca a primeira capability associada à referência de plataforma; `device_id` do prefixo não entra na consulta. `200` com objeto, `404` se ausente. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/capabilities/platforms/{reference_id}` | Mesmo lookup por `reference_id`, sem filtro de Device; `200` objeto ou `404`. · [controller/builder](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/devices/{device_id}/capabilities/{capability_name}` | Busca por combinação de Device público e nome. `200` objeto ou `404`. Este caminho usa o `device_id` da rota e é diferente da lista aninhada. · [GetCapabilityByName](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/capabilities/{capability_name}` | A mesma ação está registrada sob o prefixo global, mas exige também `device_id` como parâmetro de rota, que esse template não fornece. Não documentar como caminho funcional confirmado. · [controller](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005/006 |
| `GET /api/v1/devices/{device_id}/capabilities/uid/{uid}` | Busca exclusivamente por UID; a ação não recebe nem usa `device_id`. `200` objeto ou `404`; prefixo não restringe a busca ao Device. · [GetCapabilityByUid](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005 |
| `GET /api/v1/capabilities/uid/{uid}` | Mesmo lookup global por UID, `200` objeto ou `404`. · [controller](../../src/Api/Controllers/CapabilityController.cs) |
| `POST /api/v1/devices/{device_id}/capabilities` | Body é sequência de Capability. Serviço envia a chave do Device da rota ao repository, que consulta `Devices.Id`, insere cada Capability pela chave interna e type/name/value/owner/description dentro de transação. Se o tipo persistido for estado de ar, normaliza/inicializa value conforme a especificação de ar. Resposta `204`; Device ausente gera exceção de domínio e `404`; erros de banco/validação seguem middleware. Campos de associações enviados no DTO não são inseridos por esse fluxo de inclusão do repository. · [controller/service/repository](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-001 |
| `POST /api/v1/capabilities` | Mesmo handler, mas o template global não fornece `device_id`, embora a ação exija esse parâmetro de rota. Não há confirmação de execução funcional para esse alias. · [controller](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005 |
| `PATCH /api/v1/devices/{device_id}/capabilities` | Body `CapabilityUpdate { capability_name, value }`. Consulta por device/nome e altera value. Para data type de ar, aplica comando ao estado atual sob transação/lock; para outros tipos grava value recebido. `204` se atualizado, `404` se não localizado, `409` para conflito tipado do estado de ar, `400` para entrada inválida. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) · [AIR-CONDITIONER-STATE](../specs/AIR-CONDITIONER-STATE.md) |
| `PATCH /api/v1/capabilities` | Mesmo handler sem `device_id` no prefixo, embora requerido na assinatura. Não confirmado como funcional. · [controller](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005 |
| `PATCH /api/v1/devices/{device_id}/capabilities/value` | Alias da ação PATCH de value, mesmos campos, regras, respostas e limite de estado. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) |
| `PATCH /api/v1/capabilities/value` | Alias global sem o segmento exigido `device_id`; não confirmado como funcional. · [controller](../../src/Api/Controllers/CapabilityController.cs) |
| `PATCH /api/v1/devices/{device_id}/capabilities/{id}/patches` | Body JSON Patch de Capability, seletor `id` inteiro. Busca por ID interno, aplica operações ao modelo e preserva a chave. Só considera value alterado se operação não `test` tocar `/value` ou mover a partir de `/value`; PATCH de metadados não reenvia snapshot antigo de value como comando. Repository atualiza associações de platforms/groups e campos suportados. `204`, `404`, `400`, `409` do domínio do ar e erros de persistência. `device_id` do prefixo não é parâmetro nem filtro da ação. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) · [especificação do ar](../specs/AIR-CONDITIONER-STATE.md) |
| `PATCH /api/v1/capabilities/{id}/patches` | Mesmo PATCH por ID interno, sem filtro de Device. `204` ou `404` conforme lookup; respostas de erro adicionais conforme patch, domínio e middleware. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) |
| `DELETE /api/v1/devices/{device_id}/capabilities/{id}` | Exclui por ID interno, em transação. Handler não recebe nem aplica `device_id`; não compara linhas afetadas, então ausência não vira 404 pelo resultado do DELETE. `204` se comando conclui. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) · AUD-DEV-005 |
| `DELETE /api/v1/capabilities/{id}` | Mesmo DELETE por ID, sem escopo de Device; `204` se execução conclui. · [controller/repository](../../src/Api/Controllers/CapabilityController.cs) |
| `GET /api/v1/capabilities/{capability_id}/history` | Filtros opcionais `last_hours`, `date_start`, `date_end`; repository ordena `UpdatedAt` e ID em ordem decrescente e combina janela/data conforme parâmetros. A ação sempre retorna `200` com coleção, inclusive `[]`; o `404` anunciado é divergente. · [HistoryController](../../src/Api/Controllers/CapabilityHistoryController.cs) · [repository](../../src/Data.Repositories/Repositories/CapabilityHistoryRepository.cs) · AUD-DEV-006 |
| `POST /api/v1/capabilities/{capability_id}/history` | Body `{ "value": "…" }`; value nulo/vazio/em branco retorna `400`. Consulta capability pelo ID interno, `404` se ausente; insere histórico usando novamente o nome da capability e retorna `201` com `Location` apontando à coleção de histórico e body vazio. Repetição pode inserir novas linhas; sem idempotency key. · [controller/repository](../../src/Api/Controllers/CapabilityHistoryController.cs) · AUD-DEV-001/003 |

### Formatos de resposta

As consultas singulares acima retornam objetos, embora alguns responses do snapshot estejam tipados como arrays. `GET .../tiny?format=mcu` é arquivo de texto; Tiny JSON usa `CapabilityTiny`, que expõe `capability_name`, `owner` (mapeado como null no conversor) e `value`. O GET de histórico devolve array vazio com 200. Esses casos são divergências descritivas, não alteração do OpenAPI nesta tarefa.

### Exemplos de requisição

Os exemplos usam nomes/IDs fictícios. `type` deve corresponder a um tipo existente para chegar à gravação; exemplos não afirmam existência dos dados nem sucesso integrado.

Consulta filtrada e formato MCU:

```http
GET /api/v1/capabilities?device_id=demo-device&type=Sensor
GET /api/v1/capabilities/tiny?format=mcu&device_id=demo-device
```

Inclusão sem relações aninhadas (evita o caminho de `platforms` que falha no binding Dapper):

```http
POST /api/v1/devices/demo-device/capabilities
Content-Type: application/json
```

```json
[
  {
    "capability_name": "temperature",
    "device_id": "demo-device",
    "type": "Sensor",
    "value": "21.5",
    "owner": "sensor",
    "platforms": [],
    "groups": []
  }
]
```

Atualização simples de value e PATCH genérico de metadados:

```http
PATCH /api/v1/devices/demo-device/capabilities/value
Content-Type: application/json
```

```json
{"capability_name":"temperature","value":"22"}
```

```http
PATCH /api/v1/capabilities/17/patches
Content-Type: application/json-patch+json
```

```json
[{"op":"replace","path":"/description","value":"Temperature sensor"}]
```

Registro de histórico:

```http
POST /api/v1/capabilities/17/history
Content-Type: application/json
```

```json
{"value":"22"}
```

Consulta de histórico com limites temporais:

```http
GET /api/v1/capabilities/17/history?last_hours=24&date_start=2026-11-23T00%3A00%3A00Z&date_end=2026-11-24T00%3A00%3A00Z
```

O alias global de listagem/criação/value PATCH não fornece `device_id` exigido por algumas actions; não é usado como exemplo de sucesso. Os exemplos representam a forma do request e o binding conhecido, não o estado do banco.

Exemplos dos aliases globais cujo template não fornece `device_id` à action:

```http
GET /api/v1/capabilities/temperature
POST /api/v1/capabilities
PATCH /api/v1/capabilities
PATCH /api/v1/capabilities/value
```

Essas chamadas ilustram as rotas registradas, não requests de sucesso: a assinatura do handler exige `device_id` de rota e o binding/executabilidade desses aliases não foi demonstrado. Enviar esse campo no body ou query não altera o segmento da rota.

## Regras de leitura/escrita e evidências

As leituras agregam joins e reconstroem platforms, groups e associações smart-home. Para data types do estado do ar, a leitura normaliza o valor. A inclusão por rota aninhada resolve nome de type persistido antes de decidir qual regra de value usar. PATCH genérico por ID bloqueia/consulta o estado atual em transação; o caminho de value usa seleção `FOR UPDATE` e combina comandos de ar sob a mesma transação. Não existe prova aqui de atuação física do equipamento.

Outros GETs elegíveis que produzem `200` estão sujeitos à política base de output cache por dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. O GET Tiny também tem o cache interno acima. A política e seus limites de autenticação externa estão descritos no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

O body aceito e a projeção gravada variam por operação: inclusão comum grava nome, descrição, type, value e owner; as relações enviadas na inclusão não são equivalentes às relações atualizadas via PATCH. Não inferir persistência de `uid`, `active`, icon, `value_type`, smart_home ou associations sem observar a query aplicável. Exclusões não testam contagem de linhas. Filtros de rota não são controles de autorização.

`tests/loads` contém checks k6 de status para GET global e PATCH global `/value`; o alias PATCH não recebe o `device_id` requerido pela ação, então o script registra expectativa, não prova de sucesso. Não foi identificada outra asserção automatizada vigente para essas operações. A suíte `tests/Api.Tests` é aposentada. A auditoria local não executou testes adicionais nesta consolidação.

**Limites externos:** schema/constraints/uniqueness, conteúdo das views e políticas de admissão dependem de fontes não disponíveis. Consulte [Capability Types](capability-types.md), [History via cobertura auditada](../reports/DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-COVERAGE-MATRIX.md), [Smart Home](smart-home-capabilities.md), [Groups](groups.md), [Dashboard](dashboards-and-widgets.md) e a [matriz de cobertura consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
