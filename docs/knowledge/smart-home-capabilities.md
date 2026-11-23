# Smart Home Capabilities

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa; código observado em [SmartHomeCapabilityController](../../src/Api/Controllers/SmartHomeCapabilityController.cs), [CapabilityController](../../src/Api/Controllers/CapabilityController.cs), [Capability DTO](../../src/Api/Models/Capability.cs), [Core entities](../../src/Core/Entities/Capability.cs) e query builder de capabilities.

## Modelo e consulta

O caminho da API usa `smart_home_id`, enquanto associações carregadas na capability são agrupadas em propriedades do DTO `smart_home`. A entidade Core modela valores `CapabilityTypeSmartHome` associados por SmartHomeId e outros atributos (`Parent`, `Group`, `Name`, `Value`). O response agrupa tipos pelo smart home e nomes.

| Operação | Comportamento observado |
|---|---|
| `GET /api/v1/smart-home/{smart_home_id}/capabilities` | Acrescenta o parâmetro ao filtro e lista capabilities; aceita `name`, `type`, `owner`, `value`, `active`, `reference_id`, `uid`, `group_name` e `device_id`; `200` com resultados, `204` vazio. |
| `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}` | Consulta por UID; `200` ou `404`. |

## Divergência e limites

No filtro de collection, o query builder compara `smart_home_id` com `sh.Name`, embora o parâmetro seja nomeado ID. Na consulta individual, controller recebe `smart_home_id`, mas chama repository apenas por `uid`; o escopo da smart home não é aplicado pelo método observado. Registrar essas diferenças como divergências de implementação, não como garantias do contrato. Não se confirma exclusividade do UID entre smart homes nem regras de associação/manutenção pelo serviço. Consulte [Capabilities](capabilities.md) para filtros e relações agregadas.
