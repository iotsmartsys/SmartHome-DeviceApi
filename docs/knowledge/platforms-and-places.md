# Platforms e Monitored Places

**Estado descrito:** código em `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional até `48806770fdd486229981ae4973b70632710271e6`. Referência informativa; não foram encontradas especificações normativas próprias.

**Contrato derivado:** [OpenAPI](../contracts/openapi.json), informativo; [reconciliação](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). Evidências: [PlatformController](../../src/Api/Controllers/PlatformController.cs), [MonitoredPlaceController](../../src/Api/Controllers/MonitoredPlaceController.cs), [DTOs](../../src/Api/Models/Platform.cs), [MonitoredPlace DTO](../../src/Api/Models/MonitoredPlace.cs) e [repositories](../../src/Data.Repositories/Repositories/PlatformRepository.cs).

## Recursos e relações

Platform no DTO tem `name` string e `description` nullable. MonitoredPlace tem `id` inteiro, `identifier`, `name`, `radius` inteiro, `latitude` e `longitude` float. O catálogo de platforms é consultado por outras rotas, incluindo referências de platforms em Capabilities. O código investigado não estabelece relação direta entre Platform e MonitoredPlace.

## Operações HTTP

| Operação | Consulta e representação | Resposta e limites |
|---|---|---|
| `GET /api/v1/platforms` | Sem parâmetros ou body. Executa `SELECT Id, Name, Description FROM Platforms` e converte cada linha para `{ name, description }`; não há filtro, ordenação ou paginação na query. | `200` com array quando há registros; `204 No Content` quando vazio. O snapshot omite 204. Falhas de query passam ao middleware geral. · [controller/repository](../../src/Api/Controllers/PlatformController.cs) · [SQL](../../src/Data.Repositories/Repositories/PlatformRepository.cs) · AUD-DEV-006 |
| `GET /api/v1/places` | Sem parâmetros ou body. Repository executa `MonitoredPlaceQuery.GetAll` e converte todas as linhas para `{ id, identifier, name, radius, latitude, longitude }`; sem filtro/paginação explícitos no controller. | `200` com array; `204` vazio, também omitido no snapshot; falhas de query seguem middleware. · [controller](../../src/Api/Controllers/MonitoredPlaceController.cs) · [query](../../src/Data.Repositories/Repositories/Queries/MonitoredPlaceQuery.cs) · [repository](../../src/Data.Repositories/Repositories/PlatformRepository.cs) · AUD-DEV-006 |

Exemplo sintético da forma de retorno de Platform:

```json
[{"name":"demo","description":"Synthetic platform"}]
```

Forma de retorno de Place é um array dos campos do DTO acima. Nenhuma das duas rotas tem body de sucesso quando retorna 204.

## Limites

Respostas GET elegíveis `200` podem permanecer no output cache base por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. Alterações externas no catálogo podem não aparecer imediatamente. Ver [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida) sobre condições de elegibilidade e limites da chave.

Os controllers só expõem leitura. O comportamento do endpoint de busca por `reference_id` para capability está descrito em [Capabilities](capabilities.md). Constraints, unicidade, defaults, ordenação implícita do banco e cardinalidade real dependem do schema MySQL não integralmente versionado (`EKM-GAP-0002`). Não se inferem validações de raio/coordenadas nem associação de place com Device. A aplicação não declara autorização local nestes GETs; acesso pelo gateway/ambiente não está confirmado.

Veja a [matriz consolidada de cobertura](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
