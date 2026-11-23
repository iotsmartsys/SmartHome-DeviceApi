# Groups

**Natureza:** referência informativa, subordinada a [SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1](../specs/GROUPS-MAINTENANCE-SUPPORT.md). Evidência adicional: [GroupController](../../src/Api/Controllers/GroupController.cs), [DTOs](../../src/Api/Models/Group.cs), [Core entity](../../src/Core/Entities/Group.cs), [repository](../../src/Data.Repositories/Repositories/GroupRepository.cs) e queries de Groups.

## Relações e identidade

Group tem ID numérico, nome, ativo, ícone opcional e relações com capabilities. O DTO de leitura expõe as capabilities; inclusão/remoção de relação possui rotas próprias. Excluir uma associação não exclui capability.

## Rotas e comportamento

| Operação | Comportamento |
|---|---|
| `GET /api/v1/groups` | `200` com array, inclusive vazio. |
| `POST /api/v1/groups` | Recebe `GroupCreateRequest`, valida name e icon.name quando presente, aparando espaços; cria e retorna `200`. `capabilities` é opcional por contrato para compatibilidade. |
| `PUT /api/v1/groups/{id}` | Atualização legada por DTO integral; ID da rota diferente do corpo retorna `404`; sucesso `204`. Preservar como operação observada, distinta do PATCH administrativo. |
| `PATCH /api/v1/groups/{id}` | `application/json-patch+json`; somente replace em `/name`, `/active`, `/icon`. ID não positivo, documento vazio, path/op/value não permitido produz `400`; ausente `404`; sucesso `204`. `/icon=null` remove ícone. |
| `DELETE /api/v1/groups/{id}` | ID não positivo `400`; inexistente `404`; existente excluído `204`. Repository realiza operação transacional sobre group e relações. |
| `PUT /api/v1/groups/{groupId}/capabilities/` | Body `CapabilityGroup` com `capability_name`; valida ID e Group, adiciona associação, retorna `204`. |
| `DELETE /api/v1/groups/{groupId}/capabilities/{capabilityId}` | Valida IDs e Group; remove associação, retorna `204`; Group ausente `404`. |

## Fluxo e limites

Fluxo administrativo: consultar Groups, criar/editar metadados e gerenciar relações em endpoints separados. A especificação declara atomicidade para exclusão de relações sem excluir capabilities. Não inferir cascade global nem relacionamentos adicionais ao que contrato, repository e query sustentam. Erros de banco usam middleware vigente; responses/ProblemDetails e limites estão detalhados na especificação normativa.
