# Settings globais e de device

**Estado descrito:** implementação no commit `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional até a revisão `48806770fdd486229981ae4973b70632710271e6`. Referência informativa. O reset de settings específicas é normatizado por [SHD-SETTINGS-RESET-001@0.1](../specs/DEVICE-SETTINGS-RESET.md); os demais caminhos não têm contrato normativo geral identificado.

**Fontes:** [controllers](../../src/Api/Controllers/SettingsController.cs), [DeviceSettingsController](../../src/Api/Controllers/DeviceSettingsController.cs), [DTOs/helpers](../../src/Api/Models/Settings.cs), [repositories](../../src/Data.Repositories/Repositories/SettingsRepository.cs) e [DeviceSettingsRepository](../../src/Data.Repositories/Repositories/DeviceSettingsRepository.cs), [queries](../../src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs). O [OpenAPI derivado](../contracts/openapi.json) não é normativo.

## Modelo e projeções

Settings globais são lidas/escritas na tabela `Settings`; overrides de device são escritos em `DeviceSettings`. A consulta por device lê `v_DeviceEffectiveSettings`, que pode combinar fontes segundo uma definição externa não preservada. A precedência global/específica e a definição da view não podem ser deduzidas somente desta API. `EKM-GAP-0002` registra a falta do DDL MySQL completo e da autoridade de views.

O DTO `Settings` contém `name`, `value`, `description`, todos anuláveis, e possui `Validate()` para exigir name/value não vazios; os handlers não chamam explicitamente esse método. `ValueSettings` contém somente `value`. As operações não tratam a lista enviada como substituição integral: upsert não remove chaves omitidas.

## Operações HTTP

| Operação | Entrada, regra e projeção | Resultado observado/implementado |
|---|---|---|
| `GET /api/v1/settings` | Sem parâmetros ou body. Executa `SELECT Id, Name, Value, Description FROM Settings`; não declara ordenação. | `200` com coleção quando não vazia; `204` sem body quando vazia. Erro de banco segue middleware. · [controller](../../src/Api/Controllers/SettingsController.cs) · [repository](../../src/Data.Repositories/Repositories/SettingsRepository.cs) |
| `PUT /api/v1/settings/{name}` | Path `name`; body `ValueSettings { value }`. Atualiza apenas `Settings.Value` para linhas com o nome; description não muda. | `204` se `ExecuteAsync` reporta linhas afetadas. Zero linhas lança `KeyNotFoundException`, não traduzida pelo middleware geral para 404; resultado observado é 500. A interpretação de zero pode depender do driver quando valor não muda. Snapshot declara 404, divergindo do caminho atual. · [controller](../../src/Api/Controllers/SettingsController.cs) · [repository](../../src/Data.Repositories/Repositories/SettingsRepository.cs) · AUD-DEV-007 |
| `PUT /api/v1/settings` | Body é sequência de Settings; para cada item atualiza somente Value por Name. Não chama `Validate()`. Não há guarda de coleção vazia. | Uma transação envolve o lote. `204` após commit, inclusive para coleção vazia. Se qualquer UPDATE reportar zero linhas, o repository lança `KeyNotFoundException`, tenta rollback e relança; middleware resulta em 500, não o 404 anunciado. · [controller](../../src/Api/Controllers/SettingsController.cs) · [repository](../../src/Data.Repositories/Repositories/SettingsRepository.cs) · AUD-DEV-007 |
| `GET /api/v1/devices/{device_id}/settings` | Query opcional: `use_key_value` e `prefix_auto_format_properties_json`. Repository consulta a view efetiva por `DeviceKey=@DeviceId`, seleciona Name/Description/Value/Type e ordena por Name. `use_key_value=true` ou `yes` (comparação sensível a maiúsculas/minúsculas) retorna coleção de Settings; qualquer outro valor usa `DataParserHelper` e retorna objeto. Prefixos podem vir da query ou da configuração encontrada entre as settings. | `404` com texto “No settings found…” quando view não retorna linhas. Com dados, `200`. O objeto agrupa chaves hierárquicas por prefixo (padrão `mqtt`) e converte valores reconhecidos em IP/string, int, bool, double ou DateTime conforme parsers da plataforma; os demais permanecem string. Essa conversão depende da cultura/configuração do processo e da view efetiva. O snapshot representa somente um formato fixo. · [controller](../../src/Api/Controllers/DeviceSettingsController.cs) · [parser](../../src/Api/Models/DataParserHelper.cs) · [query](../../src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs) · AUD-DEV-006/007 |
| `PUT /api/v1/devices/{device_id}/settings` | Body é coleção não nula e não vazia; a ação retorna `400` textual se null/vazia. Para cada item resolve a chave interna do Device e faz upsert por `(DeviceId, Name)` conforme as constraints instaladas. | `204` depois do commit da transação. Em colisão, Value sempre recebe o novo valor; Description só substitui a anterior se a nova não for null nem whitespace. Chaves omitidas permanecem. Falha intermediária tenta rollback. O DTO tem Validate(), mas a ação não o invoca. · [controller](../../src/Api/Controllers/DeviceSettingsController.cs) · [repository/query](../../src/Data.Repositories/Repositories/DeviceSettingsRepository.cs) · AUD-DEV-001/007 |
| `PUT /api/v1/devices/{device_id}/settings/reset` | Sem body. Repository busca `Devices.Id` pelo `device_id` público; se encontrar, executa `DELETE FROM DeviceSettings WHERE DeviceId=@DeviceKey`. | `404` se Device não existe; `204` se existe, mesmo sem overrides. O lookup e DELETE são comandos separados e não compartilham transação. O reset não remove Settings globais nem prova como a view efetiva ficará depois; comportamento normativo e limites constam em [DEVICE-SETTINGS-RESET](../specs/DEVICE-SETTINGS-RESET.md). · [controller](../../src/Api/Controllers/DeviceSettingsController.cs) · [repository/query](../../src/Data.Repositories/Repositories/DeviceSettingsRepository.cs) |

### Exemplo de leitura

`GET /api/v1/devices/demo-device/settings?use_key_value=true` retorna uma coleção de settings efetivas quando há linhas na view. `GET /api/v1/devices/demo-device/settings` retorna o objeto hierárquico parseado. Esses exemplos descrevem o ramo do controller; não afirmam que todas as instalações possuam overrides ou uma precedência específica.

Exemplos estruturais das escritas. Os nomes são demonstrativos e a existência das linhas globais/constraints depende do banco instalado:

```http
PUT /api/v1/settings/mqtt.host
Content-Type: application/json
```

```json
{"value":"broker.example.invalid"}
```

```http
PUT /api/v1/settings
Content-Type: application/json
```

```json
[{"name":"mqtt.host","value":"broker.example.invalid","description":"Endpoint"}]
```

```http
PUT /api/v1/devices/demo-device/settings
Content-Type: application/json
```

```json
[{"name":"mqtt.host","value":"broker.example.invalid","description":"Endpoint"}]
```

O reset específico não recebe body; exemplo: `PUT /api/v1/devices/demo-device/settings/reset`.

## Regras transversais, erros e limites

GETs elegíveis com resultado `200` podem ser servidos pelo output cache base por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. Após um PUT/reset, a próxima leitura pode reutilizar um valor anterior até esse TTL; respostas `204`, `404` e erros não são armazenadas por essa política. A elegibilidade no gateway e os limites da chave estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

Não existe upsert de settings globais: os PUTs globais executam UPDATE e não criam chaves ausentes. O upsert específico de device não substitui a coleção inteira. O reset remove apenas linhas físicas de `DeviceSettings` para a chave interna resolvida. A leitura de settings específicas usa view efetiva, portanto lista de leitura e conjunto removido não são necessariamente iguais.

O middleware geral não trata `KeyNotFoundException` como 404, e suas falhas podem usar formato diferente de erros de model binding. Os metadados do OpenAPI divergem em 404 nos PUTs globais e não representam a forma variante do GET de device. Restrições de nome, unicidade, triggers e conteúdo da view dependem do DDL/configuração MySQL não versionados. Não há evidência de cache, comunicação MQTT ou publicação externa acionada por essas operações; nomes de settings relacionados a MQTT não comprovam tal integração.

Consulte [Devices](devices.md), [falhas e segurança no Dossiê](../specs/SYSTEM-DOSSIER.md#8-falhas-segurança-e-recuperação), [EKM-GAP-0002](../rfc/KNOWLEDGE-MAP.md#5-lacunas) e a [matriz de cobertura consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
