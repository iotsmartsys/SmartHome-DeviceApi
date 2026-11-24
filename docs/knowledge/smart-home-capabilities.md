# Smart Home Capabilities

**Estado descrito:** implementação da revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional local até `48806770fdd486229981ae4973b70632710271e6`. Referência informativa. A relação com estado do ar está limitada à especificação [AIR-CONDITIONER-STATE](../specs/AIR-CONDITIONER-STATE.md).

**Fontes:** [controller](../../src/Api/Controllers/SmartHomeCapabilityController.cs), [DTOs](../../src/Api/Models/SmartHomeCapability.cs) e [Capability](../../src/Api/Models/Capability.cs), [entidade](../../src/Core/Entities/Capability.cs), [repository](../../src/Data.Repositories/Repositories/CapabilityRepository.cs), [query builder](../../src/Data.Repositories/Repositories/Builders/FindCapabilityQueryBuilder.cs) e [OpenAPI](../contracts/openapi.json), derivado/informativo.

## Modelo e consultas

Capabilities podem possuir associação `CapabilityTypeSmartHome` e a conversão HTTP agrupa essas relações na propriedade `smart_home` do DTO. O path usa `smart_home_id`, mas a consulta da coleção injeta esse valor como filtro de `CapabilityFind`; o query builder o compara a `sh.Name`. Assim, o nome do parâmetro e a coluna comparada não demonstram semântica de ID técnico.

### `GET /api/v1/smart-home/{smart_home_id}/capabilities`

Aceita `smart_home_id` no path e os query filters opcionais `name`, `type`, `owner`, `value`, `active`, `reference_id`, `uid`, `group_name` e `device_id`. O controller substitui qualquer `smart_home_id` de query pelo valor de rota, consulta repository e converte as capabilities para o DTO de Smart Home. `200` com array quando existem resultados; `204` sem body quando vazio. O snapshot apresenta forma/status divergente (objeto singular/404).

Não é um endpoint de manutenção: não cria nem altera associações. O filtro compara `sh.Name` com o segmento chamado `smart_home_id`, conforme query builder; não afirmar que valida a existência/identidade de um Smart Home antes da consulta. Falhas de persistência/leitura seguem middleware geral. Evidências: [controller](../../src/Api/Controllers/SmartHomeCapabilityController.cs), [builder](../../src/Data.Repositories/Repositories/Builders/FindCapabilityQueryBuilder.cs), AUD-DEV-005/006.

### `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}`

Recebe `smart_home_id` e `uid` no path, mas o controller consulta apenas pelo UID. `200` com objeto `SmartHomeCapability` se a capability existe; `404` se não existe. O valor do segmento `smart_home_id` não restringe a consulta. Não há evidência de unicidade global do UID no schema, então correspondência ambígua depende das constraints/dados instalados.

Exemplos sintéticos de leitura:

```http
GET /api/v1/smart-home/Demo/capabilities?device_id=demo-device&type=Sensor
GET /api/v1/smart-home/OutroNome/capabilities/demo-uid
```

O segundo exemplo evidencia que `smart_home_id` não participa do lookup singular; os nomes não afirmam identidade existente.

## Limites e relações

Respostas GET elegíveis `200` podem ser servidas pelo output cache base por até dois segundos. A chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. A leitura por UID e a lista podem não refletir imediatamente uma escrita/alteração externa. A elegibilidade e os limites de identidade estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

Escopo de seleção não é autorização. O bootstrap não apresenta policy por Smart Home; política de admissão/isolamento no gateway permanece fora das fontes locais. A persistência completa das relações e constraints depende do schema de banco em `EKM-GAP-0002`. Consulte [Capabilities](capabilities.md) para filtros compartilhados e a [matriz consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
