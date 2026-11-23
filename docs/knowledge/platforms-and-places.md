# Platforms e Monitored Places

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa; não foram encontradas especificações normativas próprias para estes recursos no índice. Evidências: [controllers](../../src/Api/Controllers/PlatformController.cs) e [MonitoredPlaceController](../../src/Api/Controllers/MonitoredPlaceController.cs), [DTOs](../../src/Api/Models/Platform.cs) e [MonitoredPlace](../../src/Api/Models/MonitoredPlace.cs), [repositories](../../src/Data.Repositories/Repositories/PlatformRepository.cs).

## Recursos

Platform é representada por `name` e `description` na API. MonitoredPlace apresenta `id`, `identifier`, `name`, `radius`, latitude e longitude. O catálogo de platforms pode ser referenciado por capability; o controller de capabilities expõe busca por `reference_id`. O código consultado não confirma relação direta entre Platform e MonitoredPlace.

## Operações

- `GET /api/v1/platforms`: lista todos; controller retorna `200` se houver registros e `204` se vazio.
- `GET /api/v1/places`: lista monitored places; controller retorna `200` se houver registros e `204` se vazio.

## Limitações

Os controllers não expõem criação, atualização ou exclusão. Não se confirmam regras de validação de coordenadas/raio, unicidade de identificadores, ordenação, paginação ou associação de places com devices. Queries/repositórios consultam as tabelas locais; schema MySQL integral e suas constraints permanecem lacuna (`EKM-GAP-0002`).
