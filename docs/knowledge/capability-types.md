# Capability Types

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa, subordinada ao contrato normativo [SHD-CAPABILITY-TYPE-ID-001@0.2](../specs/CAPABILITY-TYPE-ID.md). Implementação: [controller](../../src/Api/Controllers/CapabilityTypeController.cs), [DTO](../../src/Api/Models/CapabilityType.cs), [Core entity](../../src/Core/Entities/CapabilityType.cs), [repository](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs) e queries correspondentes.

## Modelo e relação

Capability Type cataloga `name`, `actuator_mode`, `data_type`, `computed_value`, `value_symbol`, `active_value` e uma coleção de ícones. Sua chave `id` é numérica e persistida. Capabilities guardam o tipo por associação/nome e usam seu data type para interpretar value; a regra de estado do ar é específica e normatizada separadamente.

## Operações

| Operação | Contrato/implementação |
|---|---|
| `GET /api/v1/capabilities-types?name=` | Lista; filtro por nome é aplicado como busca contendo (`%name%`). `200` ou `204` vazio. |
| `GET /api/v1/capabilities-types/{id:int}` | `200` com tipo e ícones ou `404`. Segmento numérico é chave, não nome. |
| `POST /api/v1/capabilities-types` | Cria tipo e ícones em transação; `201` com recurso e Location por ID; `400` para corpo nulo. O record `CapabilityType` usa `id=0` como default no código; o schema derivado não declara esse valor. |
| `PATCH /api/v1/capabilities-types/{id:int}` | JSON Patch; consulta por ID, preserva ID e persiste; `204` ou `404`. |
| `DELETE /api/v1/capabilities-types/{id:int}` | Exclusão por ID; `204`, inclusive se ausente na implementação do repository; restrições de banco podem falhar. |

## Regras e limites

Ícones são lidos separadamente e agrupados pelo ID do tipo. Criação e atualização do conjunto de ícones ocorrem em transação. No PATCH, controller não declara whitelist de paths; modelo, conversão e repository delimitam persistência real. Não inferir regra de unicidade de nome a partir do código.

A especificação 0.2 substitui rotas legadas por ID e define respostas, invariantes de identidade, critérios e validações; usar essa fonte para contrato e a implementação como evidência atual. Detalhes de constraint/cascade e schema completo não estão preservados localmente.
