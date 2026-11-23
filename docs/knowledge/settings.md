# Settings globais e de device

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa. Sources: [SettingsController](../../src/Api/Controllers/SettingsController.cs), [DeviceSettingsController](../../src/Api/Controllers/DeviceSettingsController.cs), [modelos](../../src/Api/Models/Settings.cs), [repositories](../../src/Data.Repositories/Repositories/SettingsRepository.cs) e [DeviceSettingsRepository](../../src/Data.Repositories/Repositories/DeviceSettingsRepository.cs), [queries](../../src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs).

## Responsabilidade e modelo

Settings globais e settings específicos de device são persistidos separadamente. Cada entrada tem name, value e description. A leitura de settings efetivos usada por outras partes do modelo consulta `v_DeviceEffectiveSettings`; não se confirmou aqui a definição completa da view nem sua precedência.

## Operações

| Operação | Comportamento observado |
|---|---|
| `GET /api/v1/settings` | Lista settings globais; `200` ou `204` quando vazia. |
| `PUT /api/v1/settings/{name}` | Atualiza somente value pelo nome; controller retorna `204`, inclusive sem distinguir linha ausente antes do retorno. O repository lança `KeyNotFoundException` quando não afeta linha. |
| `PUT /api/v1/settings` | Atualiza valores de várias settings; repository usa transação e rollback se qualquer atualização não afetar linha; controller retorna `204` após sucesso. |
| `GET /api/v1/devices/{device_id}/settings?use_key_value=&prefix_auto_format_properties_json=` | Lê settings específicas. `use_key_value=true|yes` retorna coleção; padrão transforma em objeto por `DataParserHelper`. Prefixos para autoformatação podem vir da query ou da setting correspondente. Resposta `404` quando coleção vazia. |
| `PUT /api/v1/devices/{device_id}/settings` | Body de coleção não vazio; upsert de cada linha em transação; vazio `400`, sucesso `204`. |
| `PUT /api/v1/devices/{device_id}/settings/reset` | Sem body; remove somente settings do device mapeando ID público para chave interna. `404` se device inexistente; `204` se encontrado, ainda que nada houvesse para remover. Especificado em [DEVICE-SETTINGS-RESET](../specs/DEVICE-SETTINGS-RESET.md). |

## Regras, relações e limites

O modelo de device agrega settings, mas o DTO de settings do endpoint de device pode sair como coleção ou dicionário. `Settings.Validate` e `Property.Validate` exigem name/value não vazios quando invocados; os controllers de settings não chamam explicitamente esse método, portanto não afirmar que toda entrada passa por essa validação.

Chaves relacionadas a MQTT aparecem no Core (`SettingsTypes`) e são usadas pelo método `Device.HasSettingsMqtt`; isso não comprova cliente MQTT ou integração externa nesta API. O schema/view e a regra integral de herança permanecem lacunas. Semântica operacional de secrets/settings não deve ser inferida.
