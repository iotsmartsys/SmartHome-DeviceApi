# Capabilities e histórico

**Natureza:** referência informativa. Fontes centrais: [CapabilityController](../../src/Api/Controllers/CapabilityController.cs), [HistoryController](../../src/Api/Controllers/CapabilityHistoryController.cs), [Capability DTO](../../src/Api/Models/Capability.cs), [Core Capability](../../src/Core/Entities/Capability.cs), [serviço de inclusão](../../src/Core/Services/AddCapabilityService.cs), [repositórios](../../src/Data.Repositories/Repositories/CapabilityRepository.cs) e [builder de filtros](../../src/Data.Repositories/Repositories/Builders/FindCapabilityQueryBuilder.cs).

## Conceitos e relações

Capability expõe `id` numérico interno, `uid`, nome, `device_id`, tipo, owner, value, estado ativo, metadados e associações. O tipo associado informa data type e metadados visuais. Uma capability pode relacionar-se com platforms (`platform` e `referenceId`), groups e entradas de smart-home. O valor é texto no DTO; conversões e semântica variam por data type.

O controller declara dois prefixes de classe: `/api/v1/capabilities` e
`/api/v1/devices/{device_id}/capabilities`. Cada template relativo pode,
portanto, aparecer sob ambos. Ações que precisam de `device_id` falham em
recebê-lo no prefixo global; ações que não usam esse parâmetro não se tornam
escopadas por device apenas porque a rota aninhada o contém.

## Operações HTTP

| Operação | Parâmetros/comportamento encontrado |
|---|---|
| `GET /api/v1/capabilities` | Filtros: `name`, `type`, `owner`, `value`, `active`, `reference_id`, `smart_home_id`, `uid`, `group_name`, `device_id`. `200` quando há resultados, `404` quando vazia. |
| `GET /api/v1/devices/{device_id}/capabilities` | Mesmo filtro, escopado pelo device. |
| `GET .../capabilities/tiny` | Filtros acima e `format`; exclui tipos `Message`, `Alexa`, `TIME_OF_DAY`, `Scene`. `format=mcu` retorna texto delimitado por `;`; caso contrário DTO compacto. Cache de memória de dois segundos. |
| `GET /api/v1/capabilities/platforms/{reference_id}` | Consulta por referência externa; `200` ou `404`. |
| `GET /api/v1/devices/{device_id}/capabilities/{capability_name}` | Consulta por device + nome. O controller também declara a rota base sem `device_id`; nessa rota o parâmetro exigido não é fornecido — divergência entre template e assinatura. |
| `GET /api/v1/capabilities/uid/{uid}` | Consulta por UID; `200` ou `404`. |
| `POST /api/v1/devices/{device_id}/capabilities` | Recebe sequência de capabilities e delega ao serviço Core; retorna `204`. O template global sem device também existe, mas serviço recebe `device_id`; seu uso sem segmento não está confirmado. |
| `PATCH /api/v1/devices/{device_id}/capabilities` e `/value` | Corpo `{ capability_name, value }`; altera value, `204` ou `404`. |
| `PATCH /api/v1/capabilities/{id}/patches` | JSON Patch de DTO por ID; envia flag informando se operações tocaram `/value`, evitando replay de value obsoleto em patch de metadados. `204` ou `404`. |
| `DELETE /api/v1/capabilities/{id}` | Remove por ID; retorna `204`. |
| `GET /api/v1/capabilities/{capability_id}/history` | Query `last_hours`, `date_start`, `date_end`; retorna `200` com coleção (incluindo vazia, segundo controller). |
| `POST /api/v1/capabilities/{capability_id}/history` | Corpo `{ value }`, rejeita vazio com `400`; capability ausente `404`; cria entrada e responde `201` com Location para leitura. |

## Regras observadas

- Filtros são combinados no query builder. O mapper agrega linhas para reconstituir uma capability com platforms, groups e associações smart-home.
- Criação via serviço resolve o device público para a chave interna e grava em transação. O `DeviceRepository` tem caminho separado para capabilities embutidas na criação do device.
- Leituras normalizam data type de estado de ar-condicionado. Escritas desse tipo combinam estado no Core e persistem em transação; não generalizar essa regra às demais capabilities.
- PATCH genérico só considera value alterado se operation/path realmente tocar value; dados de associação também são atualizados pela implementação do repository.
- History POST converte ID de capability para nome antes de inserir; entidade histórica expõe value e timestamp. O GET declara `404` em atributo, mas a ação retorna `200` com a coleção recebida do repository, inclusive quando vazia; a divergência está sem decisão normativa. O contrato completo dos filtros históricos e retenção depende do repositório/schema.

## Limitações e fontes normativas

Estado do ar: [especificação](../specs/AIR-CONDITIONER-STATE.md). Groups e suas associações: [especificação](../specs/GROUPS-MAINTENANCE-SUPPORT.md). Filtros e respostas observados acima são código, não contrato normativo geral. A rota global por nome requer confirmação; o schema MySQL histórico não é completo. Ver também [Capability Types](capability-types.md), [Smart Home Capabilities](smart-home-capabilities.md) e [Groups](groups.md).
