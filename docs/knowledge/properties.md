# Properties

**Estado descrito:** implementação da revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional até a revisão de trabalho `48806770fdd486229981ae4973b70632710271e6`. Referência informativa, sem especificação normativa geral identificada.

**Fontes:** [controller](../../src/Api/Controllers/PropertiesController.cs), [DTO e validação explícita](../../src/Api/Models/Property.cs), [entidade Core](../../src/Core/Entities/Property.cs), [repository](../../src/Data.Repositories/Repositories/PropertyRepository.cs), [query builder](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) e [SQL](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs). O [OpenAPI derivado](../contracts/openapi.json) é informativo; veja também a [reconciliação](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md).

## Modelo e identidade

Property tem `name`, `value` e `description`, todos string anuláveis no DTO. `id` é a chave interna usada pelas rotas, mas não integra a representação `Property` devolvida; `device_id` é a chave pública textual na URL e é convertida para `Devices.Id` nos comandos de escrita que a resolvem.

`Property.Validate()` rejeita `name` e `value` nulos, vazios ou em branco, mas nenhum dos cinco handlers chama esse método. A validação uniforme desses campos não está demonstrada. `[ApiController]` e o model binding ainda rejeitam body ausente/nulo nas operações com DTO não anulável e podem rejeitar null de membro dependendo do tipo; não interpretar isso como validação de conteúdo não vazio.

## Operações HTTP

| Operação | Entrada e caminho | Resposta, persistência e limitações |
|---|---|---|
| `GET /api/v1/devices/{device_id}/properties/{id}` | `device_id` textual e `id` inteiro. Repository instancia o builder com `device_id` e chama `WithId(id)`. | A ação prevê `200` com Property ou `404` se o retorno for null. Porém `WithId` substitui o SQL inteiro por `dp.id = @id`, sem SELECT/FROM e sem filtro do device. P2 confirmou o `CommandText` gerado; não enviou o comando ao banco. Não afirmar que a leitura funciona nem prever o status de execução. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [builder](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · AUD-DEV-013 |
| `GET /api/v1/devices/{device_id}/properties` | Query opcional com `name`, `value` e `description`. Sem critérios, o SELECT junta `DeviceProperties` a `Devices` e filtra pelo `device_id` público. `name` e `value` usam igualdade; `description` usa `LIKE` com curingas. | `200` com coleção se houver itens; `204` sem itens. Quando qualquer filtro é acrescentado, o builder concatena predicados sem operador `AND`; o `CommandText` fica incompatível. P2 confirmou a construção, sem enviar SQL. O resultado HTTP da falha não foi observado. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [builder e SQL](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · AUD-DEV-013 |
| `POST /api/v1/devices/{device_id}/properties` | Body `Property`. Controller converte o DTO, consulta Device por ID público e retorna `404` se o Device não existir. `PropertyRepository.AddAsync` também resolve a chave numérica, abre transação e insere name, description e value. A SQL contém `SELECT LAST_INSERT_ID()`, mas o repository executa `ExecuteAsync`; não atribui o ID à entidade. | Após commit, o controller retorna `CreatedAtAction(GetAllAsync, {device_id,id}, property)`. Em host MVC isolado, a inclusão concluiu no double e a geração de URL falhou com 500 “No route matches the supplied values”, sem `Location`. Em uso do repository MySQL, o commit acontece antes da formatação HTTP; portanto erro 500 não prova ausência de escrita e uma repetição pode inserir de novo, sem que duplicidade tenha sido observada. Sucesso 201 anunciado no OpenAPI não está confirmado no caminho atual. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [repository/SQL](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · H2/AUD-DEV-008 |
| `PUT /api/v1/devices/{device_id}/properties` | Body `Property`. Não é substituição de recurso nem JSON Patch: chama o mesmo upsert que PATCH. Não faz lookup prévio de Device nem chama `Validate()`. | `204` se o comando/commit concluir. INSERT usa a chave interna resolvida pela subquery com `device_id`, grava name/description/value e `UpdatedAt=NOW()`. Em conflito de chave, atualiza name, value e timestamp, preservando a description existente. A identidade da colisão depende das constraints MySQL; o DDL não está completo. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [upsert](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs) |
| `PATCH /api/v1/devices/{device_id}/properties` | Mesmo DTO e mesmo caminho de `PUT`; não usa `JsonPatchDocument`. | Mesma semântica upsert de `PUT`, inclusive ausência de chamada a `Validate()` e falta de prova sobre a constraint que aciona a colisão. `204` após commit; exceção de banco é relançada. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [repository/SQL](../../src/Data.Repositories/Repositories/PropertyRepository.cs) |
| `DELETE /api/v1/devices/{device_id}/properties/{id}` | Controller consulta primeiro o Device por `device_id`; Device ausente dá `404`. Depois passa ID interno da property ao repository. | Repository resolve o ID numérico do Device e executa DELETE filtrando por ambas as chaves, dentro de transação. Não examina linhas afetadas: property ausente ou fora desse Device ainda resulta em `204` se a execução SQL concluir. Erro de persistência é relançado. A confirmação prévia do Device e a exclusão não compartilham uma única transação. · [controller](../../src/Api/Controllers/PropertiesController.cs) · [repository/SQL](../../src/Data.Repositories/Repositories/PropertyRepository.cs) |

Exemplos de leitura:

```http
GET /api/v1/devices/demo-device/properties
GET /api/v1/devices/demo-device/properties?name=mode
GET /api/v1/devices/demo-device/properties/17
```

O primeiro exemplo usa a consulta base. Os dois seguintes mostram seletores/filtros reconhecidos pela rota, mas a SQL construída é incompatível (AUD-DEV-013); não são exemplos de sucesso comprovado.

Para POST/PUT/PATCH, um objeto JSON com os três campos é a forma estrutural do DTO. Nenhum exemplo garante sucesso de escrita no schema instalado; para POST, também não há resposta bem-sucedida sustentada pela evidência H2.

Exemplos sintéticos de body para as três ações de escrita:

```json
{"name":"mode","value":"auto","description":"Operating mode"}
```

Envie esse objeto para `POST`, `PUT` ou `PATCH /api/v1/devices/demo-device/properties` conforme o comportamento de cada linha acima. POST ainda pode confirmar a transação e falhar ao gerar `Location`; o upsert pode depender da constraint de colisão instalada. O exemplo demonstra apenas a forma do DTO, não sucesso integrado.

## Persistência, erros e semântica operacional

Respostas GET elegíveis `200` podem ser servidas pelo output cache base por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. A consulta filtrada/individual descrita em AUD-DEV-013 falha antes de produzir uma resposta normal; erros não são armazenados. As condições e limites constam no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

`GET` de coleção é o único modo do builder que conserva a consulta SELECT base sem filtros. `GET` individual e consultas filtradas são afetados por AUD-DEV-013. `POST`, `PUT`, `PATCH` e `DELETE` têm transações locais; exceções disparam tentativa de rollback e são relançadas. O ensaio P2 não executou SQL, e H2 usou repository substituto; não há prova de persistência integrada ou de rollback físico para estas rotas.

O pipeline pode gerar `400` por binding/validação automática, `404` pelos casos descritos acima, `204` em alterações sem body e erro `500` para exceções genéricas. Status/headers exatos para erro SQL dependem do middleware. Em particular, o sucesso de persistência do POST pode anteceder a falha observada ao construir a resposta.

O bootstrap não estabelece autorização local por recurso nessas ações. O limite do gateway e a admissão por ambiente não são inferíveis do controller. Unicidade, defaults, triggers, cascade e demais constraints dependem do schema MySQL efetivo, pendente em `EKM-GAP-0002`.

Consulte também [Devices](devices.md), [erros e segurança no Dossiê](../specs/SYSTEM-DOSSIER.md#8-falhas-segurança-e-recuperação) e a [matriz de cobertura consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
