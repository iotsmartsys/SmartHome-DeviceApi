# Properties

**Natureza:** referência informativa. Fontes: [controller](../../src/Api/Controllers/PropertiesController.cs), [DTO e validação](../../src/Api/Models/Property.cs), [Core entity](../../src/Core/Entities/Property.cs), [repository](../../src/Data.Repositories/Repositories/PropertyRepository.cs) e [query builder](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs).

## Conceitos

Property é um par textual name/value com description opcional, associado a um device. A resposta da API não expõe a chave interna da property no DTO, embora as rotas individuais a usem como `id`; device usa ID público textual.

## Operações HTTP

| Operação | Comportamento encontrado |
|---|---|
| `GET /api/v1/devices/{device_id}/properties/{id}` | Busca por ID interno no escopo do device; `200` ou `404`. |
| `GET /api/v1/devices/{device_id}/properties` | Filtros de query são `name`, `value` e `description`; `200` com coleção ou `204` vazia. |
| `POST /api/v1/devices/{device_id}/properties` | Confirma device, insere property em transação e retorna `201`; device ausente `404`. |
| `PUT` ou `PATCH /api/v1/devices/{device_id}/properties` | Ambas as rotas chamam o mesmo upsert por device/name e retornam `204`. Não é JSON Patch. |
| `DELETE /api/v1/devices/{device_id}/properties/{id}` | Confirma device, remove ID no escopo do device e retorna `204`; device ausente `404`. |

## Regras, falhas e limites

O DTO/Core possui validação que exige name e value não vazios, mas esses controllers não chamam diretamente `Validate`; a aplicação uniforme dessa regra não está confirmada. `POST` e `DELETE` fazem lookup prévio do device; o upsert executa query parametrizada com `device_id`. Escritas usam transação no repository. Não há contrato normativo local identificado para unicidade, formato de value ou semântica de upsert. O schema completo permanece lacuna.
