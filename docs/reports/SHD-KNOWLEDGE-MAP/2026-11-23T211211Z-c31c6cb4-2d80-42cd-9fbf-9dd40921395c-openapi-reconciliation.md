# Reconciliação integral OpenAPI — SHD-KNOWLEDGE-MAP-001@0.1

**Data/hora:** 2026-11-23 21:12:11 UTC
**Run ID:** `c31c6cb4-2d80-42cd-9fbf-9dd40921395c`
**Branch/baseline:** `task/shd-knowledge-map-001` / `1caf4909cb3f`.
**Escopo:** reconciliação documental estática integral; sem executar API, build, testes ou serviços.

## Procedência e integridade

O arquivo original foi fornecido pelo Arquiteto como `Downloads/swagger.json` e copiado byte a byte para [`docs/contracts/openapi.json`](../../contracts/openapi.json). OpenAPI `3.0.1`; versão declarada da API `1.0`; título `Api`. A data de geração, ferramenta geradora e revisão do código que o gerou não são informadas no arquivo; não foram inferidas. Revisão confrontada: `1caf4909cb3ff96ce1084544d2abe8362285bd58`. SHA-256: `130d8be6bed4edca19d06a1ea2ca6383ac4092dccd940f667823666c0811050b`.

JSON válido; 52 paths, 76 operações HTTP e 53 schemas. As 680 ocorrências de `$ref` (53 alvos únicos) são resolvíveis; 0 referências quebradas. A busca de padrões de credencial/segredo/chave definida nesta inspeção retornou 0 correspondências. O arquivo não foi alterado manualmente.

## Inventário quantitativo

| Medida | Contagem verificável | Método |
|---|---:|---|
| Operações HTTP no OpenAPI | 76 | pares método + path em `paths` |
| Operações de controller | 73 | atributos Http* expandidos sobre todos os `[Route]` de cada controller; parâmetros de template normalizados |
| Operações OpenAPI reconciliadas a controllers | 73 | interseção método + path normalizado |
| Operações adicionais em `Program.cs` | 3 | Minimal API `MapGet` encontrada estaticamente |
| OpenAPI operações sem implementação confirmada no código local | 0 | diferença após controllers + `Program.cs` |
| Operações de controller ausentes do OpenAPI | 0 | diferença após normalização de parâmetro |
| Operações sem referência funcional no conjunto de `docs/knowledge/` | 0 | cada operação está relacionada ao domínio e documento abaixo; inventário completo nesta matriz |
| Referências `$ref` locais quebradas | 0 | resolução de JSON Pointer no próprio artefato |
| Divergências estáticas listadas abaixo | 16 grupos | achados identificados; alguns afetam múltiplas operações |

Os 3 métodos OpenAPI que não são de controllers correspondem a `/api/v1/timezone`, `/api/v1/timezone/datetime` e `/api/v1/health`, definidos por `app.MapGet` em `src/Api/Program.cs`; portanto, operações OpenAPI sem confirmação em código são zero. A contagem de controller usa operações método-rota depois de expandir os dois prefixes de `CapabilityController`; não é contagem de métodos C# únicos.

## Cobertura por domínio

| Tag do OpenAPI | Operações | Referência funcional | Resultado |
|---|---:|---|---|
| `Api` — Endpoints utilitários | 3 | [utility-endpoints.md](../../knowledge/utility-endpoints.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Capability` — Capabilities | 20 | [capabilities.md](../../knowledge/capabilities.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `CapabilityHistory` — Capability History | 2 | [capabilities.md](../../knowledge/capabilities.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `CapabilityType` — Capability Types | 5 | [capability-types.md](../../knowledge/capability-types.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Dashboard` — Dashboards e widgets | 6 | [dashboards-and-widgets.md](../../knowledge/dashboards-and-widgets.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `DashboardCapability` — Dashboards e widgets | 2 | [dashboards-and-widgets.md](../../knowledge/dashboards-and-widgets.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `DashboardWidget` — Dashboards e widgets | 3 | [dashboards-and-widgets.md](../../knowledge/dashboards-and-widgets.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `DashboardWidgetType` — Dashboards e widgets | 1 | [dashboards-and-widgets.md](../../knowledge/dashboards-and-widgets.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Device` — Devices | 5 | [devices.md](../../knowledge/devices.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `DeviceMetrics` — Device Metrics | 3 | [device-metrics.md](../../knowledge/device-metrics.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `DeviceSettings` — Settings de device | 3 | [settings.md](../../knowledge/settings.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Group` — Groups | 7 | [groups.md](../../knowledge/groups.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `MonitoredPlace` — Monitored Places | 1 | [platforms-and-places.md](../../knowledge/platforms-and-places.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `OAuth` — Controle de acesso legado | 3 | [legacy-access-control.md](../../knowledge/legacy-access-control.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Platform` — Platforms | 1 | [platforms-and-places.md](../../knowledge/platforms-and-places.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Properties` — Properties | 6 | [properties.md](../../knowledge/properties.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `Settings` — Settings globais | 3 | [settings.md](../../knowledge/settings.md) | Reconciliadas com rota/método no código e entradas do inventário |
| `SmartHomeCapability` — Smart Home Capabilities | 2 | [smart-home-capabilities.md](../../knowledge/smart-home-capabilities.md) | Reconciliadas com rota/método no código e entradas do inventário |

O inventário integral abaixo registra, por operação, parâmetros (local, tipo e obrigatoriedade), corpo (media types, schema e campos `required` declarados), e respostas (códigos, schema/media type quando expostos). Schemas completos estão preservados no JSON derivado, evitando criar uma segunda autoridade documental. Os campos não listados em `required` são opcionais no JSON Schema; `nullable` permanece uma propriedade distinta.

## Divergências e limitações encontradas

Os itens descrevem diferenças entre metadados gerados, assinatura/metadata de controller e/ou caminho observado no código. São registros informativos, não correções de comportamento. Contagem: 16 grupos; há sobreposição entre operações afetadas.

### D-01 — Parâmetro de rota sem segmento correspondente

4 operações: `POST /api/v1/capabilities`, `PATCH /api/v1/capabilities`, `PATCH /api/v1/capabilities/value` e `GET /api/v1/capabilities/{capability_name}`. As ações exigem `[FromRoute] device_id`, mas o prefixo global não contém esse segmento; os dois PATCH também têm a rota aninhada que contém o parâmetro. OpenAPI lista `device_id` como path parameter mesmo quando ausente do template. Evidência: `CapabilityController` class Routes + assinaturas. Também `GET by name` na rota global recebe parâmetro não presente.

### D-02 — Resposta de capability singular declarada como coleção

6 operações nas rotas expandidas por `name`, `platforms/{reference_id}` e `uid/{uid}` (global e aninhada): a ação retorna um único DTO, mas `ProducesResponseType`/OpenAPI declara array de `Capability`. Evidência: ações de lookup em `CapabilityController` e schema de cada resposta 200.

### D-03 — Resposta Tiny dinâmica não completamente modelada

2 operações `GET .../capabilities/tiny`: `format=mcu` retorna arquivo `text/plain` de linhas delimitadas por `;`, enquanto OpenAPI declara array de `CapabilityTiny` para `text/plain`, JSON e text/json. A representação de arquivo e a representação JSON variam pelo parâmetro.

### D-04 — Status de Capability History

1 operação `GET /api/v1/capabilities/{capability_id}/history`: metadata declara 404, mas ação retorna `Ok(collection)` independentemente de coleção vazia; body de erro/404 não é produzido nesse caminho específico.

### D-05 — Campo `Device.settings` ausente do schema derivado

O DTO possui campo público `settings`; schema `Api.Models.Device` no OpenAPI não o enumera. Schema é usado por `GET /api/v1/devices`, `GET /api/v1/devices/{device_id}` e `POST /api/v1/devices`. As conversões Device↔Core também não demonstram persistência/retorno do conteúdo, conforme `devices.md`.

### D-06 — Defaults do código não representados no OpenAPI

OpenAPI declara `page=1` e `pageSize=100` nos parâmetros de `GET /api/v1/devices/{deviceId}/metrics/history`, em concordância com os valores do controller. O schema/body de `POST /api/v1/capabilities-types` não declara o default `id=0` do parâmetro opcional no record `CapabilityType`. Defaults `null` de outros parâmetros de DTO permanecem opcionais/nullables, sem palavra-chave `default`.

### D-07 — `device_id` de métricas ignorado no mapeamento

1 operação `POST /api/v1/devices/{deviceId}/metrics`: request schema declara `device_id` obrigatório; `DeviceMetricsRequest.ToEntity()` não o copia, e a ação usa o parâmetro da rota para persistência.

### D-08 — Forma de resposta de Device Settings incompleta

1 operação `GET /api/v1/devices/{device_id}/settings`: controller declara schema singular `Settings`; retorna coleção quando `use_key_value=true|yes` e dicionário no modo padrão. A forma variante não está representada no OpenAPI.

### D-09 — Schemas de respostas utilitárias omitidos

3 operações em `Program.cs`: `GET /api/v1/timezone` retorna `TimezoneResponse`, datetime retorna string e health retorna objeto `{status}`; cada response OpenAPI 200 não possui `content`/schema.

### D-10 — Contratos de acesso legado não descritos no OpenAPI

3 operações `/oauth/authorize`, `/oauth/token`, `/oauth/me`: inputs de query, form fields e/ou esquema Bearer não constam como parâmetros, requestBody ou security scheme no artefato. O controller lê esses valores manualmente de `HttpContext`. A classificação “Controle de acesso proprietário legado — Deprecated” permanece inalterada; não se infere conformidade OAuth 2.0.

### D-11 — Filtro `smart_home_id` compara com nome

2 operações de collection que aceitam `smart_home_id`: `GET /api/v1/capabilities` (query) e `GET /api/v1/smart-home/{smart_home_id}/capabilities` (route/query). O query builder compara com `sh.Name`; o significado “ID” não é sustentado pela comparação observada.

### D-12 — Busca Smart Home por UID não restringe pelo pai

1 operação `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}`: controller recebe o smart_home_id mas repository é chamado apenas com UID.

### D-13 — Query `zone` não realiza conversão de timezone

1 operação `GET /api/v1/timezone`: o valor retornado é ecoado/rotulado e dados derivam do timezone/cultura do processo, sem resolução de timezone informado; código descreve essa limitação em `utility-endpoints.md`.

### D-14 — Obrigatoriedade do request body omitida

28 operações têm `requestBody` sem a propriedade `required`; pelo OpenAPI, a ausência equivale a `false` (corpo opcional). As ações correspondentes usam `[FromBody]` e/ou tratam o body como entrada obrigatória. Esse contraste afeta schemas de body dos vários domínios e não é corrigido editando o snapshot. Há sobreposição com os três pares já listados em D-01.

### D-15 — Regras de campo/cardinalidade exigidas no código não expressas no schema

Quatro operações: `POST /api/v1/groups` exige `name` e, quando `icon` é fornecido, `icon.name`; `POST /api/v1/capabilities/{capability_id}/history` rejeita `value` nulo/vazio; `POST /api/v1/dashboards` valida `name`; `PUT /api/v1/devices/{device_id}/settings` rejeita array vazio. Os schemas correspondentes deixam esses campos opcionais e/ou não declaram `minItems`. `DeviceMetricsRequest` e `DeviceNetworkMetricsRequest` são exceções: suas 12 propriedades estão marcadas `required`.


### D-16 — Tipo ou shape de campos não declarado no schema

O schema não informa tipo/shape para quatro propriedades: `DashboardCapability.currentValue`, `DashboardWidgetData.value`, `Microsoft.AspNetCore.JsonPatch.Operations.Operation.value` e `Microsoft.AspNetCore.Mvc.ValidationProblemDetails.errors`. No código, as duas primeiras são `object?`; o valor JSON Patch é polimórfico; `errors` não tem forma de mapa definida no OpenAPI. O artefato não registra `oneOf`, `type` ou forma alternativa. Em contraste, `Program.cs` contém um `MapType` específico para JSON Patch de Group que declara `replace`, paths `/name`, `/active`, `/icon` e alternativas de tipo para `value`; o snapshot continua mostrando `Operation.value` sem tipo. Afeta cinco operações PATCH com JSON Patch, respostas de `GET /api/v1/dashboard-capabilities` e `GET /api/v1/dashboards/{dashboardId}/data`, e a resposta de erro de `POST /api/v1/groups`.

## Inventário de operações OpenAPI

Os nomes de schemas referem-se às entradas de `components.schemas`. Em `required`, campos marcados `nullable` continuam podendo aceitar null conforme a declaração no artefato.

### Api — Endpoints utilitários

**Fonte C#:** [`Program.cs`](../../../src/Api/Program.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/timezone` | `zone` query:string req | — | 200 [sem schema] |
| `GET /api/v1/timezone/datetime` | — | — | 200 [sem schema] |
| `GET /api/v1/health` | — | — | 200 [sem schema] |

### Capability — Capabilities

**Fonte C#:** [`CapabilityController.cs`](../../../src/Api/Controllers/CapabilityController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/devices/{device_id}/capabilities` | `name` query:string opt; `type` query:string opt; `owner` query:string opt; `value` query:string opt; `active` query:boolean opt; `reference_id` query:string opt; `smart_home_id` query:string opt; `uid` query:string opt; `group_name` query:string opt; `device_id` query:string opt; `device_id` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `POST /api/v1/devices/{device_id}/capabilities` | `device_id` path:string req | opt: application/json-patch+json → array<Api.Models.Capability>; application/json → array<Api.Models.Capability>; text/json → array<Api.Models.Capability>; application/*+json → array<Api.Models.Capability> | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/devices/{device_id}/capabilities` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.CapabilityUpdate; application/json → Api.Models.CapabilityUpdate; text/json → Api.Models.CapabilityUpdate; application/*+json → Api.Models.CapabilityUpdate | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities` | `name` query:string opt; `type` query:string opt; `owner` query:string opt; `value` query:string opt; `active` query:boolean opt; `reference_id` query:string opt; `smart_home_id` query:string opt; `uid` query:string opt; `group_name` query:string opt; `device_id` query:string opt | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `POST /api/v1/capabilities` | `device_id` path:string req | opt: application/json-patch+json → array<Api.Models.Capability>; application/json → array<Api.Models.Capability>; text/json → array<Api.Models.Capability>; application/*+json → array<Api.Models.Capability> | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/capabilities` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.CapabilityUpdate; application/json → Api.Models.CapabilityUpdate; text/json → Api.Models.CapabilityUpdate; application/*+json → Api.Models.CapabilityUpdate | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}/capabilities/tiny` | `name` query:string opt; `type` query:string opt; `owner` query:string opt; `value` query:string opt; `active` query:boolean opt; `reference_id` query:string opt; `smart_home_id` query:string opt; `uid` query:string opt; `group_name` query:string opt; `device_id` query:string opt; `format` query:string opt; `device_id` path:string req | — | 200 [text/plain:array<Api.Models.CapabilityTiny>, application/json:array<Api.Models.CapabilityTiny>, text/json:array<Api.Models.CapabilityTiny>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities/tiny` | `name` query:string opt; `type` query:string opt; `owner` query:string opt; `value` query:string opt; `active` query:boolean opt; `reference_id` query:string opt; `smart_home_id` query:string opt; `uid` query:string opt; `group_name` query:string opt; `device_id` query:string opt; `format` query:string opt | — | 200 [text/plain:array<Api.Models.CapabilityTiny>, application/json:array<Api.Models.CapabilityTiny>, text/json:array<Api.Models.CapabilityTiny>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}/capabilities/platforms/{reference_id}` | `reference_id` path:string req; `device_id` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities/platforms/{reference_id}` | `reference_id` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}/capabilities/{capability_name}` | `device_id` path:string req; `capability_name` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities/{capability_name}` | `device_id` path:string req; `capability_name` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}/capabilities/uid/{uid}` | `uid` path:string req; `device_id` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities/uid/{uid}` | `uid` path:string req | — | 200 [text/plain:array<Api.Models.Capability>, application/json:array<Api.Models.Capability>, text/json:array<Api.Models.Capability>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/devices/{device_id}/capabilities/value` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.CapabilityUpdate; application/json → Api.Models.CapabilityUpdate; text/json → Api.Models.CapabilityUpdate; application/*+json → Api.Models.CapabilityUpdate | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/capabilities/value` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.CapabilityUpdate; application/json → Api.Models.CapabilityUpdate; text/json → Api.Models.CapabilityUpdate; application/*+json → Api.Models.CapabilityUpdate | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/devices/{device_id}/capabilities/{id}/patches` | `id` path:integer/int32 req; `device_id` path:string req | opt: application/json-patch+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; text/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/*+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation> | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/capabilities/{id}/patches` | `id` path:integer/int32 req | opt: application/json-patch+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; text/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/*+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation> | 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `DELETE /api/v1/devices/{device_id}/capabilities/{id}` | `id` path:integer/int32 req; `device_id` path:string req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `DELETE /api/v1/capabilities/{id}` | `id` path:integer/int32 req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

### CapabilityHistory — Capability History

**Fonte C#:** [`CapabilityHistoryController.cs`](../../../src/Api/Controllers/CapabilityHistoryController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/capabilities/{capability_id}/history` | `capability_id` path:integer/int32 req; `last_hours` query:integer/int32 opt; `date_start` query:string opt; `date_end` query:string opt | — | 200 [text/plain:array<Core.Contracts.Repositories.CapabilityHistory>, application/json:array<Core.Contracts.Repositories.CapabilityHistory>, text/json:array<Core.Contracts.Repositories.CapabilityHistory>]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `POST /api/v1/capabilities/{capability_id}/history` | `capability_id` path:integer/int32 req | opt: application/json-patch+json → Api.Models.CapabilityHistoryCreate; application/json → Api.Models.CapabilityHistoryCreate; text/json → Api.Models.CapabilityHistoryCreate; application/*+json → Api.Models.CapabilityHistoryCreate | 201 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

### CapabilityType — Capability Types

**Fonte C#:** [`CapabilityTypeController.cs`](../../../src/Api/Controllers/CapabilityTypeController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/capabilities-types` | `name` query:string opt | — | 200 [text/plain:array<Api.Models.CapabilityType>, application/json:array<Api.Models.CapabilityType>, text/json:array<Api.Models.CapabilityType>]; 204 [sem schema]; 500 [sem schema] |
| `POST /api/v1/capabilities-types` | — | opt: application/json-patch+json → Api.Models.CapabilityType; application/json → Api.Models.CapabilityType; text/json → Api.Models.CapabilityType; application/*+json → Api.Models.CapabilityType | 201 [text/plain:Api.Models.CapabilityType, application/json:Api.Models.CapabilityType, text/json:Api.Models.CapabilityType]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/capabilities-types/{id}` | `id` path:integer/int32 req | — | 200 [text/plain:Api.Models.CapabilityType, application/json:Api.Models.CapabilityType, text/json:Api.Models.CapabilityType]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/capabilities-types/{id}` | `id` path:integer/int32 req | opt: application/json-patch+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; text/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/*+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation> | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `DELETE /api/v1/capabilities-types/{id}` | `id` path:integer/int32 req | — | 204 [sem schema]; 500 [sem schema] |

### Dashboard — Dashboards e widgets

**Fonte C#:** [`DashboardController.cs`](../../../src/Api/Controllers/DashboardController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/dashboards` | — | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardsResponse, application/json:Api.Models.DashboardsResponse, text/json:Api.Models.DashboardsResponse] |
| `POST /api/v1/dashboards` | — | opt: application/json-patch+json → Api.Models.DashboardCreateRequest; application/json → Api.Models.DashboardCreateRequest; text/json → Api.Models.DashboardCreateRequest; application/*+json → Api.Models.DashboardCreateRequest | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 201 [text/plain:Api.Models.Dashboard, application/json:Api.Models.Dashboard, text/json:Api.Models.Dashboard]; 415 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse] |
| `GET /api/v1/dashboards/{dashboardId}` | `dashboardId` path:integer/int64 req | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.Dashboard, application/json:Api.Models.Dashboard, text/json:Api.Models.Dashboard] |
| `PUT /api/v1/dashboards/{dashboardId}` | `dashboardId` path:integer/int64 req | opt: application/json-patch+json → Api.Models.DashboardUpdateRequest; application/json → Api.Models.DashboardUpdateRequest; text/json → Api.Models.DashboardUpdateRequest; application/*+json → Api.Models.DashboardUpdateRequest | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.Dashboard, application/json:Api.Models.Dashboard, text/json:Api.Models.Dashboard]; 415 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse] |
| `DELETE /api/v1/dashboards/{dashboardId}` | `dashboardId` path:integer/int64 req | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 204 [sem schema] |
| `GET /api/v1/dashboards/{dashboardId}/data` | `dashboardId` path:integer/int64 req | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardData, application/json:Api.Models.DashboardData, text/json:Api.Models.DashboardData] |

### DashboardCapability — Dashboards e widgets

**Fonte C#:** [`DashboardCapabilityController.cs`](../../../src/Api/Controllers/DashboardCapabilityController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/dashboard-capabilities` | — | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardCapabilitiesResponse, application/json:Api.Models.DashboardCapabilitiesResponse, text/json:Api.Models.DashboardCapabilitiesResponse] |
| `GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets` | `capabilityId` path:integer/int32 req | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardCompatibleWidgetsResponse, application/json:Api.Models.DashboardCompatibleWidgetsResponse, text/json:Api.Models.DashboardCompatibleWidgetsResponse] |

### DashboardWidget — Dashboards e widgets

**Fonte C#:** [`DashboardWidgetController.cs`](../../../src/Api/Controllers/DashboardWidgetController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `POST /api/v1/dashboards/{dashboardId}/widgets` | `dashboardId` path:integer/int64 req | opt: application/json-patch+json → Api.Models.DashboardWidgetCreateRequest; application/json → Api.Models.DashboardWidgetCreateRequest; text/json → Api.Models.DashboardWidgetCreateRequest; application/*+json → Api.Models.DashboardWidgetCreateRequest | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 201 [text/plain:Api.Models.DashboardWidget, application/json:Api.Models.DashboardWidget, text/json:Api.Models.DashboardWidget]; 415 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse] |
| `PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | `dashboardId` path:integer/int64 req; `widgetId` path:integer/int64 req | opt: application/json-patch+json → Api.Models.DashboardWidgetUpdateRequest; application/json → Api.Models.DashboardWidgetUpdateRequest; text/json → Api.Models.DashboardWidgetUpdateRequest; application/*+json → Api.Models.DashboardWidgetUpdateRequest | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardWidget, application/json:Api.Models.DashboardWidget, text/json:Api.Models.DashboardWidget]; 415 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse] |
| `DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | `dashboardId` path:integer/int64 req; `widgetId` path:integer/int64 req | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 204 [sem schema] |

### DashboardWidgetType — Dashboards e widgets

**Fonte C#:** [`DashboardWidgetTypeController.cs`](../../../src/Api/Controllers/DashboardWidgetTypeController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/dashboard-widget-types` | — | — | 400 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 404 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 422 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 500 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 503 [text/plain:Api.Models.DashboardErrorResponse, application/json:Api.Models.DashboardErrorResponse, text/json:Api.Models.DashboardErrorResponse]; 200 [text/plain:Api.Models.DashboardWidgetTypesResponse, application/json:Api.Models.DashboardWidgetTypesResponse, text/json:Api.Models.DashboardWidgetTypesResponse] |

### Device — Devices

**Fonte C#:** [`DeviceController.cs`](../../../src/Api/Controllers/DeviceController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/devices` | `DeviceId` query:string opt; `Name` query:string opt; `Description` query:string opt; `Platform` query:string opt; `SettingsKey` query:string opt; `SettingsValue` query:string opt; `IsActive` query:boolean opt | — | 200 [text/plain:array<Api.Models.Device>, application/json:array<Api.Models.Device>, text/json:array<Api.Models.Device>]; 500 [sem schema] |
| `POST /api/v1/devices` | — | opt: application/json-patch+json → Api.Models.Device; application/json → Api.Models.Device; text/json → Api.Models.Device; application/*+json → Api.Models.Device | 201 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}` | `device_id` path:string req | — | 200 [text/plain:Api.Models.Device, application/json:Api.Models.Device, text/json:Api.Models.Device]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/devices/{device_id}` | `device_id` path:string req | opt: application/json-patch+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; text/json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation>; application/*+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation> | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `DELETE /api/v1/devices/{device_id}` | `device_id` path:string req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |

### DeviceMetrics — Device Metrics

**Fonte C#:** [`DeviceMetricsController.cs`](../../../src/Api/Controllers/DeviceMetricsController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `POST /api/v1/devices/{deviceId}/metrics` | `deviceId` path:string req | opt: application/json-patch+json → Api.Models.DeviceMetricsRequest; campos req: cpu_cores, cpu_percent, device_id, frequency_mhz, memory_percent, network, temperature_c, uptime_ms; application/json → Api.Models.DeviceMetricsRequest; campos req: cpu_cores, cpu_percent, device_id, frequency_mhz, memory_percent, network, temperature_c, uptime_ms; text/json → Api.Models.DeviceMetricsRequest; campos req: cpu_cores, cpu_percent, device_id, frequency_mhz, memory_percent, network, temperature_c, uptime_ms; application/*+json → Api.Models.DeviceMetricsRequest; campos req: cpu_cores, cpu_percent, device_id, frequency_mhz, memory_percent, network, temperature_c, uptime_ms | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `GET /api/v1/devices/{deviceId}/metrics/current` | `deviceId` path:string req | — | 200 [text/plain:Api.Models.DeviceMetricsCurrentResponse, application/json:Api.Models.DeviceMetricsCurrentResponse, text/json:Api.Models.DeviceMetricsCurrentResponse]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `GET /api/v1/devices/{deviceId}/metrics/history` | `deviceId` path:string req; `from` query:string/date-time opt; `to` query:string/date-time opt; `page` query:integer/int32 opt default=1; `pageSize` query:integer/int32 opt default=100 | — | 200 [text/plain:Api.Models.DeviceMetricsHistoryResponse, application/json:Api.Models.DeviceMetricsHistoryResponse, text/json:Api.Models.DeviceMetricsHistoryResponse]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |

### DeviceSettings — Settings de device

**Fonte C#:** [`DeviceSettingsController.cs`](../../../src/Api/Controllers/DeviceSettingsController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/devices/{device_id}/settings` | `device_id` path:string req; `prefix_auto_format_properties_json` query:string opt; `use_key_value` query:string opt | — | 200 [text/plain:Api.Models.Settings, application/json:Api.Models.Settings, text/json:Api.Models.Settings]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PUT /api/v1/devices/{device_id}/settings` | `device_id` path:string req | opt: application/json-patch+json → array<Api.Models.Settings>; application/json → array<Api.Models.Settings>; text/json → array<Api.Models.Settings>; application/*+json → array<Api.Models.Settings> | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `PUT /api/v1/devices/{device_id}/settings/reset` | `device_id` path:string req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |

### Group — Groups

**Fonte C#:** [`GroupController.cs`](../../../src/Api/Controllers/GroupController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `POST /api/v1/groups` | — | opt: application/json-patch+json → Api.Models.GroupCreateRequest; application/json → Api.Models.GroupCreateRequest; text/json → Api.Models.GroupCreateRequest; application/*+json → Api.Models.GroupCreateRequest | 200 [text/plain:Api.Models.Group, application/json:Api.Models.Group, text/json:Api.Models.Group]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ValidationProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ValidationProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ValidationProblemDetails]; 409 [text/plain:string, application/json:string, text/json:string]; 500 [text/plain:string, application/json:string, text/json:string]; 503 [text/plain:string, application/json:string, text/json:string] |
| `GET /api/v1/groups` | — | — | 200 [text/plain:array<Api.Models.Group>, application/json:array<Api.Models.Group>, text/json:array<Api.Models.Group>]; 500 [text/plain:string, application/json:string, text/json:string]; 503 [text/plain:string, application/json:string, text/json:string] |
| `PUT /api/v1/groups/{id}` | `id` path:integer/int32 req | opt: application/json-patch+json → Api.Models.Group; application/json → Api.Models.Group; text/json → Api.Models.Group; application/*+json → Api.Models.Group | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/groups/{id}` | `id` path:integer/int32 req | opt: application/json-patch+json → array<Microsoft.AspNetCore.JsonPatch.Operations.Operation> | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 409 [text/plain:string, application/json:string, text/json:string]; 500 [text/plain:string, application/json:string, text/json:string]; 503 [text/plain:string, application/json:string, text/json:string] |
| `DELETE /api/v1/groups/{id}` | `id` path:integer/int32 req | — | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [text/plain:string, application/json:string, text/json:string]; 503 [text/plain:string, application/json:string, text/json:string] |
| `PUT /api/v1/groups/{groupId}/capabilities` | `groupId` path:integer/int32 req | opt: application/json-patch+json → Api.Models.CapabilityGroup; application/json → Api.Models.CapabilityGroup; text/json → Api.Models.CapabilityGroup; application/*+json → Api.Models.CapabilityGroup | 204 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `DELETE /api/v1/groups/{groupId}/capabilities/{capabilityId}` | `groupId` path:integer/int32 req; `capabilityId` path:integer/int32 req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

### MonitoredPlace — Monitored Places

**Fonte C#:** [`MonitoredPlaceController.cs`](../../../src/Api/Controllers/MonitoredPlaceController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/places` | — | — | 200 [text/plain:array<Api.Models.MonitoredPlace>, application/json:array<Api.Models.MonitoredPlace>, text/json:array<Api.Models.MonitoredPlace>]; 500 [sem schema] |

### OAuth — Controle de acesso legado

**Fonte C#:** [`OAuthController.cs`](../../../src/Api/Controllers/OAuth/OAuthController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /oauth/authorize` | — | — | 302 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `POST /oauth/token` | — | — | 200 [sem schema]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 401 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |
| `GET /oauth/me` | — | — | 200 [sem schema]; 401 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails] |

### Platform — Platforms

**Fonte C#:** [`PlatformController.cs`](../../../src/Api/Controllers/PlatformController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/platforms` | — | — | 200 [text/plain:array<Api.Models.Platform>, application/json:array<Api.Models.Platform>, text/json:array<Api.Models.Platform>]; 500 [sem schema] |

### Properties — Properties

**Fonte C#:** [`PropertiesController.cs`](../../../src/Api/Controllers/PropertiesController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/devices/{device_id}/properties/{id}` | `device_id` path:string req; `id` path:integer/int32 req | — | 200 [text/plain:Api.Models.Property, application/json:Api.Models.Property, text/json:Api.Models.Property]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `DELETE /api/v1/devices/{device_id}/properties/{id}` | `device_id` path:string req; `id` path:integer/int32 req | — | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/devices/{device_id}/properties` | `device_id` path:string req; `name` query:string opt; `value` query:string opt; `description` query:string opt | — | 200 [text/plain:array<Api.Models.Property>, application/json:array<Api.Models.Property>, text/json:array<Api.Models.Property>]; 204 [sem schema]; 500 [sem schema] |
| `POST /api/v1/devices/{device_id}/properties` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.Property; application/json → Api.Models.Property; text/json → Api.Models.Property; application/*+json → Api.Models.Property | 201 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PUT /api/v1/devices/{device_id}/properties` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.Property; application/json → Api.Models.Property; text/json → Api.Models.Property; application/*+json → Api.Models.Property | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PATCH /api/v1/devices/{device_id}/properties` | `device_id` path:string req | opt: application/json-patch+json → Api.Models.Property; application/json → Api.Models.Property; text/json → Api.Models.Property; application/*+json → Api.Models.Property | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 400 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

### Settings — Settings globais

**Fonte C#:** [`SettingsController.cs`](../../../src/Api/Controllers/SettingsController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/settings` | — | — | 200 [text/plain:array<Api.Models.Settings>, application/json:array<Api.Models.Settings>, text/json:array<Api.Models.Settings>]; 204 [sem schema]; 500 [sem schema] |
| `PUT /api/v1/settings` | — | opt: application/json-patch+json → array<Api.Models.Settings>; application/json → array<Api.Models.Settings>; text/json → array<Api.Models.Settings>; application/*+json → array<Api.Models.Settings> | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `PUT /api/v1/settings/{name}` | `name` path:string req | opt: application/json-patch+json → Api.Models.ValueSettings; application/json → Api.Models.ValueSettings; text/json → Api.Models.ValueSettings; application/*+json → Api.Models.ValueSettings | 204 [sem schema]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

### SmartHomeCapability — Smart Home Capabilities

**Fonte C#:** [`SmartHomeCapabilityController.cs`](../../../src/Api/Controllers/SmartHomeCapabilityController.cs)

| Método e path | Parâmetros | Request body | Responses |
|---|---|---|---|
| `GET /api/v1/smart-home/{smart_home_id}/capabilities` | `smart_home_id` path:string req; `name` query:string opt; `type` query:string opt; `owner` query:string opt; `value` query:string opt; `active` query:boolean opt; `reference_id` query:string opt; `smart_home_id` query:string opt; `uid` query:string opt; `group_name` query:string opt; `device_id` query:string opt | — | 200 [text/plain:Api.Models.SmartHomeCapability, application/json:Api.Models.SmartHomeCapability, text/json:Api.Models.SmartHomeCapability]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |
| `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}` | `smart_home_id` path:string req; `uid` path:string req | — | 200 [text/plain:Api.Models.SmartHomeCapability, application/json:Api.Models.SmartHomeCapability, text/json:Api.Models.SmartHomeCapability]; 404 [text/plain:Microsoft.AspNetCore.Mvc.ProblemDetails, application/json:Microsoft.AspNetCore.Mvc.ProblemDetails, text/json:Microsoft.AspNetCore.Mvc.ProblemDetails]; 500 [sem schema] |

## Inventário de schemas

A tabela registra todos os 53 schemas: campos obrigatórios, campos opcionais (tipo e `nullable`) e defaults presentes. Nenhum schema declara `default`; os dois defaults encontrados estão nos parâmetros `page`/`pageSize` do inventário de operações. Campos e formatos são reproduzidos do JSON como evidência do snapshot; não se tornam regra normativa.

| Schema | Campos `required` | Campos opcionais (`type`, nullable quando aplicável) | Defaults |
|---|---|---|---|
| `Api.Models.Capability` | — | `capability_name`:string nullable, `uid`:string nullable, `description`:string nullable, `owner`:string nullable, `device_id`:string nullable, `type`:string nullable, `mode`:string nullable, `value`:string nullable, `platforms`:array<Api.Models.Capability.Platform> nullable, `value_type`:string nullable, `updated_at`:string nullable, `active`:boolean, `icon`:Api.Models.Capability.Icon, `groups`:array<Api.Models.Capability.Group> nullable, `id`:integer/int32 | nenhum |
| `Api.Models.Capability.Group` | — | `name`:string nullable, `icon_name`:string nullable, `icon_color`:string nullable | nenhum |
| `Api.Models.Capability.Icon` | — | `name`:string nullable, `active_color`:string nullable, `inactive_color`:string nullable | nenhum |
| `Api.Models.Capability.Platform` | — | `platform`:string nullable, `referenceId`:string nullable | nenhum |
| `Api.Models.CapabilityGroup` | — | `id`:integer/int32, `capability_name`:string nullable, `description`:string nullable | nenhum |
| `Api.Models.CapabilityHistoryCreate` | — | `value`:string nullable | nenhum |
| `Api.Models.CapabilityIcon` | — | `name`:string nullable, `primary_color`:string nullable, `secondary_color`:string nullable | nenhum |
| `Api.Models.CapabilityTiny` | — | `capability_name`:string nullable, `owner`:string nullable, `value`:string nullable | nenhum |
| `Api.Models.CapabilityType` | — | `name`:string nullable, `actuator_mode`:string nullable, `data_type`:string nullable, `computed_value`:boolean, `value_symbol`:string nullable, `active_value`:string nullable, `icons`:array<Api.Models.CapabilityIcon> nullable, `id`:integer/int32 | nenhum |
| `Api.Models.CapabilityUpdate` | — | `capability_name`:string nullable, `value`:string nullable | nenhum |
| `Api.Models.Dashboard` | — | `id`:integer/int64, `name`:string nullable, `description`:string nullable, `layoutType`:string nullable, `isDefault`:boolean, `displayOrder`:integer/int32, `createdAt`:string nullable, `updatedAt`:string nullable, `widgets`:array<Api.Models.DashboardWidget> nullable | nenhum |
| `Api.Models.DashboardCapabilitiesResponse` | — | `items`:array<Api.Models.DashboardCapability> nullable | nenhum |
| `Api.Models.DashboardCapability` | — | `deviceId`:string nullable, `deviceName`:string nullable, `capabilityId`:integer/int32, `capabilityCode`:string nullable, `capabilityName`:string nullable, `dataType`:string nullable, `unit`:string nullable, `semanticType`:string nullable, `currentValue`:sem tipo declarado nullable, `lastUpdatedAt`:string nullable, `status`:string nullable, `compatibleWidgets`:array<string> nullable | nenhum |
| `Api.Models.DashboardCompatibleWidgetsResponse` | — | `capabilityId`:integer/int32, `capabilityCode`:string nullable, `dataType`:string nullable, `compatibleWidgets`:array<Api.Models.DashboardWidgetType> nullable | nenhum |
| `Api.Models.DashboardCreateRequest` | — | `name`:string nullable, `description`:string nullable, `layoutType`:string nullable, `isDefault`:boolean nullable, `displayOrder`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardData` | — | `dashboardId`:integer/int64, `name`:string nullable, `layoutType`:string nullable, `generatedAt`:string nullable, `widgets`:array<Api.Models.DashboardWidgetData> nullable | nenhum |
| `Api.Models.DashboardError` | — | `code`:string nullable, `message`:string nullable, `details`:Api.Models.DashboardErrorDetails | nenhum |
| `Api.Models.DashboardErrorDetails` | — | `field`:string nullable | nenhum |
| `Api.Models.DashboardErrorResponse` | — | `error`:Api.Models.DashboardError | nenhum |
| `Api.Models.DashboardPosition` | — | `x`:integer/int32, `y`:integer/int32, `width`:integer/int32, `height`:integer/int32 | nenhum |
| `Api.Models.DashboardPositionRequest` | — | `x`:integer/int32 nullable, `y`:integer/int32 nullable, `width`:integer/int32 nullable, `height`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardSummary` | — | `id`:integer/int64, `name`:string nullable, `description`:string nullable, `layoutType`:string nullable, `isDefault`:boolean, `displayOrder`:integer/int32, `createdAt`:string nullable, `updatedAt`:string nullable, `widgetCount`:integer/int32 | nenhum |
| `Api.Models.DashboardUpdateRequest` | — | `name`:string nullable, `description`:string nullable, `layoutType`:string nullable, `isDefault`:boolean nullable, `displayOrder`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardWidget` | — | `id`:integer/int64, `dashboardId`:integer/int64, `title`:string nullable, `capabilityId`:integer/int32, `deviceId`:string nullable, `capabilityCode`:string nullable, `dataType`:string nullable, `widgetType`:string nullable, `dataMode`:string nullable, `position`:Api.Models.DashboardPosition, `config`:Api.Models.DashboardWidgetConfig, `refreshIntervalSeconds`:integer/int32 nullable, `displayOrder`:integer/int32, `createdAt`:string nullable, `updatedAt`:string nullable | nenhum |
| `Api.Models.DashboardWidgetConfig` | — | `unit`:string nullable, `min`:number/double nullable, `max`:number/double nullable, `warningFrom`:number/double nullable, `dangerFrom`:number/double nullable, `decimals`:integer/int32 nullable, `showLastUpdated`:boolean nullable, `invertState`:boolean nullable, `onLabel`:string nullable, `onIcon`:string nullable, `offLabel`:string nullable, `offIcon`:string nullable, `openLabel`:string nullable, `openIcon`:string nullable, `closedLabel`:string nullable, `closedIcon`:string nullable, `pressedLabel`:string nullable, `pressedIcon`:string nullable, `releasedLabel`:string nullable, `releasedIcon`:string nullable | nenhum |
| `Api.Models.DashboardWidgetCreateRequest` | — | `title`:string nullable, `deviceId`:string nullable, `capabilityId`:integer/int32 nullable, `widgetType`:string nullable, `dataMode`:string nullable, `position`:Api.Models.DashboardPositionRequest, `config`:Api.Models.DashboardWidgetConfig, `refreshIntervalSeconds`:integer/int32 nullable, `displayOrder`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardWidgetData` | — | `widgetId`:integer/int64, `title`:string nullable, `deviceId`:string nullable, `capabilityId`:integer/int32, `capabilityCode`:string nullable, `widgetType`:string nullable, `dataType`:string nullable, `dataMode`:string nullable, `value`:sem tipo declarado nullable, `unit`:string nullable, `label`:string nullable, `icon`:string nullable, `status`:string nullable, `lastUpdatedAt`:string nullable, `position`:Api.Models.DashboardPosition, `config`:Api.Models.DashboardWidgetConfig, `displayOrder`:integer/int32, `refreshIntervalSeconds`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardWidgetType` | — | `code`:string nullable, `name`:string nullable, `description`:string nullable, `compatibleDataTypes`:array<string> nullable, `defaultDataMode`:string nullable, `defaultConfig`:Api.Models.DashboardWidgetConfig, `enabled`:boolean, `lifecycle`:string nullable | nenhum |
| `Api.Models.DashboardWidgetTypesResponse` | — | `items`:array<Api.Models.DashboardWidgetType> nullable | nenhum |
| `Api.Models.DashboardWidgetUpdateRequest` | — | `title`:string nullable, `deviceId`:string nullable, `capabilityId`:integer/int32 nullable, `widgetType`:string nullable, `dataMode`:string nullable, `position`:Api.Models.DashboardPositionRequest, `config`:Api.Models.DashboardWidgetConfig, `refreshIntervalSeconds`:integer/int32 nullable, `displayOrder`:integer/int32 nullable | nenhum |
| `Api.Models.DashboardsResponse` | — | `items`:array<Api.Models.DashboardSummary> nullable | nenhum |
| `Api.Models.Device` | — | `device_id`:string nullable, `device_name`:string nullable, `description`:string nullable, `state`:string nullable, `mac_address`:string nullable, `ip_address`:string nullable, `protocol`:string nullable, `platform`:string nullable, `last_active`:string nullable, `power_on`:string nullable, `is_active`:boolean, `capabilities`:array<Api.Models.Capability> nullable, `properties`:array<Api.Models.Property> nullable, `id`:integer/int32 nullable | nenhum |
| `Api.Models.DeviceMetricsCurrentResponse` | — | `device_id`:string nullable, `uptime_ms`:integer/int64, `cpu_cores`:integer/int32, `cpu_percent`:number/double, `memory_percent`:number/double, `temperature_c`:number/double, `frequency_mhz`:integer/int32, `network`:Api.Models.DeviceNetworkMetricsResponse, `received_at`:string/date-time | nenhum |
| `Api.Models.DeviceMetricsHistoryItemResponse` | — | `uptime_ms`:integer/int64, `cpu_percent`:number/double, `memory_percent`:number/double, `temperature_c`:number/double, `frequency_mhz`:integer/int32, `rssi`:integer/int32, `received_at`:string/date-time | nenhum |
| `Api.Models.DeviceMetricsHistoryResponse` | — | `page`:integer/int32, `page_size`:integer/int32, `total_items`:integer/int64, `total_pages`:integer/int32, `items`:array<Api.Models.DeviceMetricsHistoryItemResponse> nullable | nenhum |
| `Api.Models.DeviceMetricsRequest` | `device_id`:string, `uptime_ms`:integer/int64, `cpu_cores`:integer/int32, `cpu_percent`:number/double, `memory_percent`:number/double, `temperature_c`:number/double, `frequency_mhz`:integer/int32, `network`:Api.Models.DeviceNetworkMetricsRequest | — | nenhum |
| `Api.Models.DeviceNetworkMetricsRequest` | `rssi`:integer/int32, `last_disconnection_uptime_ms`:integer/int64, `disconnection_reason`:integer/int32, `connection_count`:integer/int64 | — | nenhum |
| `Api.Models.DeviceNetworkMetricsResponse` | — | `rssi`:integer/int32, `last_disconnection_uptime_ms`:integer/int64, `disconnection_reason`:integer/int32, `connection_count`:integer/int64 | nenhum |
| `Api.Models.Group` | — | `id`:integer/int32, `name`:string nullable, `active`:boolean, `icon`:Api.Models.IconGroup, `capabilities`:array<Api.Models.Capability> nullable | nenhum |
| `Api.Models.GroupCreateRequest` | — | `name`:string nullable, `active`:boolean, `icon`:Api.Models.GroupIconRequest, `capabilities`:array<Api.Models.Capability> nullable | nenhum |
| `Api.Models.GroupIconRequest` | — | `name`:string nullable | nenhum |
| `Api.Models.IconGroup` | — | `name`:string nullable | nenhum |
| `Api.Models.MonitoredPlace` | — | `id`:integer/int32, `identifier`:string nullable, `name`:string nullable, `radius`:integer/int32, `latitude`:number/float, `longitude`:number/float | nenhum |
| `Api.Models.Platform` | — | `name`:string nullable, `description`:string nullable | nenhum |
| `Api.Models.Property` | — | `name`:string nullable, `value`:string nullable, `description`:string nullable | nenhum |
| `Api.Models.Settings` | — | `name`:string nullable, `value`:string nullable, `description`:string nullable | nenhum |
| `Api.Models.SmartHomeCapability` | — | `capability_name`:string nullable, `uid`:string nullable, `description`:string nullable, `type`:string nullable, `value_type`:string nullable, `value`:string nullable, `active`:boolean | nenhum |
| `Api.Models.ValueSettings` | — | `value`:string nullable | nenhum |
| `Core.Contracts.Repositories.CapabilityHistory` | — | `updatedAt`:string/date-time, `value`:string nullable | nenhum |
| `Microsoft.AspNetCore.JsonPatch.Operations.Operation` | — | `operationType`:Microsoft.AspNetCore.JsonPatch.Operations.OperationType, `path`:string nullable, `op`:string nullable, `from`:string nullable, `value`:sem tipo declarado nullable | nenhum |
| `Microsoft.AspNetCore.JsonPatch.Operations.OperationType` | — | — | nenhum |
| `Microsoft.AspNetCore.Mvc.ProblemDetails` | — | `type`:string nullable, `title`:string nullable, `status`:integer/int32 nullable, `detail`:string nullable, `instance`:string nullable | nenhum |
| `Microsoft.AspNetCore.Mvc.ValidationProblemDetails` | — | `type`:string nullable, `title`:string nullable, `status`:integer/int32 nullable, `detail`:string nullable, `instance`:string nullable, `errors`:sem tipo declarado nullable | nenhum |

## Critério de cobertura

- As 73 operações expandidas de controllers têm par método/path no OpenAPI; os três endpoints mínimos também estão mapeados em `Program.cs`. Nenhuma operação ficou ausente de qualquer lado após normalização de placeholders e constraints (`{id:int}` → `{}`).
- As referências funcionais por domínio estão em `docs/knowledge/`; o índice acima vincula cada tag/operation set a uma referência. A presente matriz é o índice exaustivo de rotas, schemas e respostas do recorte.
- Operações OAuth estão inventariadas somente para descrever a API local; mantêm-se as limitações DA-03 e os limites de não investigar serviços externos.
- Não foram realizadas chamadas HTTP, inicialização da API, acesso a banco, build, testes ou validações operacionais. O confronto é estático contra os arquivos locais na revisão indicada.
- A ausência de uma especificação funcional normativa geral continua em `EKM-GAP-0001`; OpenAPI é derivado, não normativa. As diferenças acima permanecem lacunas documentais/implementacionais explícitas e não são resolvidas por esta reconciliação.
