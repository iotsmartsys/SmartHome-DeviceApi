# Groups

**Estado descrito:** código da revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional identificado até `48806770fdd486229981ae4973b70632710271e6`. Referência informativa subordinada a [SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1](../specs/GROUPS-MAINTENANCE-SUPPORT.md), estado `Done`. Divergências documentais abaixo não reabrem o contrato nem alteram a decisão humana.

**Contrato derivado:** [OpenAPI](../contracts/openapi.json), informativo, e [reconciliação](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). Implementação: [controller](../../src/Api/Controllers/GroupController.cs), [DTOs](../../src/Api/Models/Group.cs), [Core entity](../../src/Core/Entities/Group.cs), [repository](../../src/Data.Repositories/Repositories/GroupRepository.cs), [queries](../../src/Data.Repositories/Repositories/Queries/GroupQuery.cs).

## Modelo e relações

Group tem `id` numérico, `name`, `active`, `icon` opcional e `capabilities`. A resposta inclui associações agregadas por join. `GroupCreateRequest` recebe `name`, `active`, `icon` e `capabilities` opcionais; criação não usa ID enviado. `GroupPatchRequest` limita o contrato administrativo a name, active e icon. `CapabilityGroup` inclui `id`, `capability_name` e description opcional; o `id` numérico é o valor usado pela escrita da associação, ainda que o controller valide o nome.

## Operações HTTP

| Operação | Entrada e comportamento | Resposta e estado da persistência |
|---|---|---|
| `GET /api/v1/groups` | Sem parâmetros/body. Consulta Groups com joins a ícone, capability, device e type, e agrega as relações. | `200` sempre com array, inclusive `[]`. O JSON de cada Group traz id/name/active/icon e capabilities; icon nulo pode ser omitido pela serialização configurada. · GRP-API-001–003 · [controller/query](../../src/Api/Controllers/GroupController.cs) |
| `POST /api/v1/groups` | Body exige `name` não branco; `icon.name` deve ser preenchido se icon vier. Handler aplica Trim em name/icon.name. `active` é booleano; capabilities é opcional e ausente equivale a vazio. Insert grava Name, Activated, IconName e timestamps; associações legadas fornecidas são inseridas na mesma transação. | `200` com Group (inclui ID gerado) depois do commit; `400 ValidationProblemDetails` para body/name/icon inválidos. Conflitos e falhas seguem handlers/middleware. A operação não retorna Location. · GRP-API-004–009 · [controller](../../src/Api/Controllers/GroupController.cs) · [repository/query](../../src/Data.Repositories/Repositories/GroupRepository.cs) |
| `PUT /api/v1/groups/{id}` | Body DTO integral `Group { id, name, active, icon, capabilities }`. Se `id` do corpo diverge do path, retorna `404`. Caso corresponda, converte para entidade e faz update completo; repository substitui as relações: exclui as atuais e insere as que vieram no DTO, na mesma transação. | `204` depois do repository concluir. O repository não confirma se a linha principal existia nem verifica linhas afetadas; assim, igualdade dos IDs não comprova que houve update de Group. Relações fornecidas podem ser regravadas mesmo quando UPDATE não encontra Group, conforme constraints. Este é o caminho legado distinto do PATCH administrativo. · [controller/repository/query](../../src/Api/Controllers/GroupController.cs) |
| `PATCH /api/v1/groups/{id}` | Requer `application/json-patch+json`. Body array JSON Patch; somente `replace` em `/name`, `/active`, `/icon`. ID deve ser positivo e documento não vazio. Valida tipo de value e guards do name/icon, aplica sobre estado lido, normaliza strings e envia somente os campos tocados ao `UpdateAdministrativeAsync`. Relações não são alteradas. · GRP-API-010–015 | `204` após transação. ID não positivo, operação/path/value ou patch vazio: `400` com ProblemDetails/ValidationProblemDetails; positivo inexistente: `404` com ProblemDetails. `/icon` com null remove o ícone. A configuração do snapshot OpenAPI não representa fielmente o schema específico do JSON Patch e pode expor Operation genérica; GRP-API-019/AC-007 permanece divergência de metadados, não corrigida aqui. · [controller](../../src/Api/Controllers/GroupController.cs) · [repository](../../src/Data.Repositories/Repositories/GroupRepository.cs) · [Program.cs](../../src/Api/Program.cs) · AUD-DEV-006 |
| `DELETE /api/v1/groups/{id}` | Sem body. ID deve ser positivo. Repository apaga relações e Group sob a mesma transação; se o DELETE principal afeta zero linhas, faz rollback e retorna false. | `204` quando Group foi removido; `400` para ID não positivo; `404` para ID positivo sem Group; falha de banco não é convertida em sucesso. Capabilities não são excluídas pelo comando. Atomicidade física depende do engine/schema instalados e não foi verificada nesta consolidação. · GRP-API-016–018 · [controller/repository/query](../../src/Api/Controllers/GroupController.cs) |
| `PUT /api/v1/groups/{groupId}/capabilities/` | Body `CapabilityGroup`; IDs devem ser positivos e Group deve existir. Handler rejeita body null ou `capability_name` vazio, mas repository escreve `capability.Id` numérico, não resolve capability pelo nome validado. | `204` depois do INSERT; `400` para ID/name inválidos; `404` para Group ausente. Capability ID inexistente/duplicado depende de FK/constraints; não se verifica linha já associada. O snapshot/guia que sugere que somente o nome determina a associação é divergente. · [controller/repository/query](../../src/Api/Controllers/GroupController.cs) · AUD-DEV-003 |
| `DELETE /api/v1/groups/{groupId}/capabilities/{capabilityId}` | Sem body. Ambos IDs devem ser positivos e Group precisa existir. DELETE filtra por `GroupId` e `CapabilityId`. | `204` após execução mesmo quando nenhuma relação foi removida; `400` para IDs inválidos; `404` se Group ausente. Não confirma existência da capability nem linhas afetadas. · [controller/repository/query](../../src/Api/Controllers/GroupController.cs) |

Exemplos informativos de criação e patch válidos estão na [especificação §§5–6](../specs/GROUPS-MAINTENANCE-SUPPORT.md). Eles representam o contrato aprovado, não prova de execução integrada nesta tarefa.

Exemplos das operações legadas fora do contrato normativo principal:

```http
PUT /api/v1/groups/7
Content-Type: application/json
```

```json
{
  "id": 7,
  "name": "Sala",
  "active": true,
  "icon": {"name":"sofa.fill"},
  "capabilities": [{"id":22,"capability_name":"temperature","device_id":"demo-device","type":"Sensor","description":"Temperatura"}]
}
```

```http
PUT /api/v1/groups/7/capabilities
Content-Type: application/json
```

```json
{"id":22,"capability_name":"temperature"}
```

No segundo exemplo, `id` numérico é a chave efetivamente escrita; o nome validado não substitui nem resolve essa chave. Os IDs são ilustrativos e não afirmam que existam.

## Contrato normativo, testes e limites

As respostas GET elegíveis `200` podem ser reutilizadas por até dois segundos pelo output cache base; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. A coleção pode não refletir uma escrita recente nesse intervalo. As condições e limites estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

A especificação de Groups trata de listagem, criação, patch administrativo e exclusão. O PUT integral e os endpoints de associação foram preservados fora do escopo normativo principal e são descritos acima pelo caminho de código. O snapshot OpenAPI foi mantido inalterado; a configuração atual do gerador contém mapeamento específico de patch, mas o artefato versionado reconcilia-se em Operation genérica, fato registrado em AUD-DEV-006.

Os cenários da suíte `tests/Api.Tests` são proibidos como evidência por decisão humana e não foram usados. Os relatórios de implementação preservam suas evidências históricas e não equivalem a execução desta documentação. A auditoria identificou cobertura automatizada vigente concentrada em Dashboard; não identificou cenário de Groups nas suites ativas pesquisadas.

Não há autenticação/autorização por Group definida no controller; alcance pela rede depende do gateway/ambiente. Unicidade de name, cascades, FKs e engine do banco dependem do schema MySQL ausente (`EKM-GAP-0002`). Veja [Capabilities](capabilities.md), [mapa](../rfc/KNOWLEDGE-MAP.md) e [matriz consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
