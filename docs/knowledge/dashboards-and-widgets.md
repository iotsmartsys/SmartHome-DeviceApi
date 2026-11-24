# Dashboards, widgets e catálogo

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa; o contrato normativo é [SHD-DASHBOARD-API-V1-001@0.3](../specs/DASHBOARD-API-V1.md). Fontes de implementação: [DashboardController](../../src/Api/Controllers/DashboardController.cs), [DashboardWidgetController](../../src/Api/Controllers/DashboardWidgetController.cs), [catálogo e capabilities](../../src/Api/Controllers/DashboardWidgetTypeController.cs), [serviço Core](../../src/Core/Services/DashboardService.cs), [resolver de dados](../../src/Core/Services/DashboardDataResolver.cs), [compatibilidade](../../src/Core/Services/DashboardWidgetCompatibilityResolver.cs) e [repository](../../src/Data.Repositories/Repositories/DashboardRepository.cs).

## Modelo e relações

Dashboard possui ID Int64, nome, descrição, layout, default/order e widgets. Widget pertence a Dashboard, referencia capability por ID Int32 e inclui tipo, modo, posição, configuração e ordem. Device associado é derivado da capability; o contrato proíbe tratar `deviceId` como identidade do vínculo do widget. Tipos habilitados declaram data types compatíveis e configuração default.

## Operações HTTP

Requisitos e regras normativas completas estão em [DASHBOARD-API-V1-001@0.3 §§2–13](../specs/DASHBOARD-API-V1.md). As rotas pertencem ao contexto global de cliente admitido descrito nessa fonte; isso não confirma política de admissão do gateway. Controllers usam `DashboardRequestFilter` e respostas de erro do Dashboard.

| Operação | Entrada e fluxo observado | Resultado |
|---|---|---|
| `GET /api/v1/dashboards` | Sem body/query. Consulta e mapeia summaries; service/repository aplicam ordenação/default conforme contrato. | `200` com envelope `{ items: [...] }`, inclusive coleção vazia. Não retorna widgets na lista; summary inclui `widgetCount`. · [controller](../../src/Api/Controllers/DashboardController.cs) · DASH-001 |
| `GET /api/v1/dashboards/{dashboardId}` | Path ID; carrega dashboard/widgets e catálogo de capabilities para campos derivados. | `200` com dashboard e widgets; ID inválido `400`, inexistente `404`. A consulta de capability adicional pode afetar resposta depois de carregar o dashboard. · [controller/service](../../src/Api/Controllers/DashboardController.cs) |
| `POST /api/v1/dashboards` | Body `DashboardCreateRequest`; service aplica nome, layout, defaults, ordem e regra de dashboard padrão global. | `201` com dashboard criado e `Location` para GET individual, após sucesso do service. Entrada inválida `400`; conflito `409` se regra de unicidade efetiva o produzir; media type não suportado `415`; indisponibilidade/falha geral `503`/`500`. · [controller](../../src/Api/Controllers/DashboardController.cs) · DASH-001/002/005/008 |
| `PUT /api/v1/dashboards/{dashboardId}` | Body `DashboardUpdateRequest`; é update parcial: omitido preserva, null pode limpar os campos anuláveis conforme spec; no-op não cria versão artificial. | `200` com dashboard atualizado; `400/404/409/415/500/503` conforme erro tipado, sem retorno de sucesso para falha. · [controller](../../src/Api/Controllers/DashboardController.cs) · DASH-001/002/005/008 |
| `DELETE /api/v1/dashboards/{dashboardId}` | Path ID; repository exclui Dashboard e widgets pertencentes conforme transação local; não exclui capability fonte. | `204` sem corpo. ID inválido `400`; ausente `404`; falha de persistência segue envelope `500/503`. Repetição após primeira exclusão retorna `404` conforme contrato. · [controller](../../src/Api/Controllers/DashboardController.cs) · DASH-005/009 |
| `GET /api/v1/dashboards/{dashboardId}/data` | Path ID; carrega dashboard, capabilities e widget types; resolve cada widget no mesmo instante UTC `now`. | `200` com `dashboardId`, `name`, `layoutType`, `generatedAt` e todos widgets, inclusive com status problemático. Dashboard inválido/ausente produz erro HTTP; estado por widget não é erro HTTP. · [controller/resolver](../../src/Api/Controllers/DashboardController.cs) · DASH-007 |
| `POST /api/v1/dashboards/{dashboardId}/widgets` | Body `DashboardWidgetCreateRequest`; vincula capability por `capabilityId`, valida pai, tipo, compatibilidade, configuração e posição; `deviceId` recebido não define identidade da capability. | `201` com widget e `Location` apontando para dashboard proprietário. Erros de referência `404`, payload/configuração `400`, incompatibilidade/disabled `422`, media type `415`, falha `500/503`. · [controller/service](../../src/Api/Controllers/DashboardWidgetController.cs) · DASH-003/004/005/008 |
| `PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | Body `DashboardWidgetUpdateRequest`; primeiro valida Dashboard e vínculo do widget, depois campos fornecidos; omitido preserva, null redefine campo anulável, config faz merge conforme spec. | `200` com widget atualizado; `404` quando pai/widget ausente ou widget não pertence ao pai; inválido `400`, incompatibilidade `422`, `415/500/503` quando aplicável. Falha não persiste atualização parcial conforme contrato. · [controller/service](../../src/Api/Controllers/DashboardWidgetController.cs) · DASH-003/005/008 |
| `DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | Path IDs; confere propriedade do widget no dashboard e remove apenas o widget. Capability de origem é preservada. | `204` sem body; ID inválido `400`, pai/widget ausente ou de outro pai `404`; erro de persistência `500/503`. · [controller/service](../../src/Api/Controllers/DashboardWidgetController.cs) · DASH-003/009 |
| `GET /api/v1/dashboard-widget-types` | Sem body/query. Lê catálogo estático/configurado de tipos e os apresenta em envelope `items`; textos não determinam compatibilidade. | `200`, inclusive catálogo vazio conforme retorno do service. Erros usam envelope Dashboard. · [controller/service](../../src/Api/Controllers/DashboardWidgetTypeController.cs) · DASH-004/006 |
| `GET /api/v1/dashboard-capabilities` | Sem body/query. Carrega capabilities e tipos; resolve valor/status atual e associa lista de widget types compatíveis. Não remove silenciosamente capabilities inativas ou tipos sem suporte. | `200` com envelope `items`; cada item inclui IDs, metadados, valor/status e compatibilidade. Status segue precedência da seção 11 da spec; não suportado tem dataType null e compatibleWidgets vazio. · [controller/resolvers](../../src/Api/Controllers/DashboardCapabilityController.cs) · DASH-006/007 |
| `GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets` | Path ID inteiro; consulta uma capability e aplica resolver de compatibilidade sobre catálogo habilitado. | `200` com capabilityId, capabilityCode, dataType e array `compatibleWidgets`; capability ausente resulta `404`. · [controller/resolver](../../src/Api/Controllers/DashboardCapabilityController.cs) · DASH-004/006 |

IDs numéricos são validados por filtro para distinguir valores inválidos (`400`) de recurso inexistente (`404`); a rota não prova autenticação. O contrato detalha campos por modelo, respostas e códigos por cenário. A documentação de exemplos de criação de widget e erro representativo está em [DASHBOARD-API-V1 §§7–9](../specs/DASHBOARD-API-V1.md).

Exemplos de escrita complementares (IDs e capability são ilustrativos; o recurso precisa existir e estar compatível para sucesso):

```http
POST /api/v1/dashboards
Content-Type: application/json
```

```json
{"name":"Casa","layoutType":"grid","isDefault":false,"displayOrder":0}
```

```http
PUT /api/v1/dashboards/10
Content-Type: application/json
```

```json
{"description":"Visão principal","displayOrder":1}
```

O body de criação de widget está na [especificação §7.2](../specs/DASHBOARD-API-V1.md#72-formatos-de-configuração). O mesmo formato é aceito para os membros fornecidos no PUT de widget, que preserva os campos omitidos e aplica as regras distintas para null/config descritas acima.

## Regras implementadas e contrato

O service valida IDs positivos e converte ausências em erros de domínio. O Core limita layout a `grid`, modo de dados a `current_value`, valida tipo habilitado, tipo de dados compatível, posições e configurações. PUT é atualização parcial; semântica detalhada de omitido/null, defaults, concorrência, ordenação, envelope de erro e persistência está na especificação e deve prevalecer. O resolver classifica status como `ok`, `stale`, `offline`, `no_data`, `invalid_value`, `error` e `capability_missing`; fonte textual inválida não é coercida livremente. `refreshIntervalSeconds` é metadado/sugestão, não agendamento de polling pela API.

## Estado e lacunas

GETs Dashboard elegíveis com resposta `200` podem ser reutilizados pelo output cache base por até dois segundos. A chave usa método, esquema, host/porta, path e todos os parâmetros da query; não varia por identidade. Após escrita, o cliente pode observar brevemente o estado anterior. A elegibilidade e os limites de identidade estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

A especificação 0.3 e o mapa registram reimplementação/build para revisão, schema não aplicado e aceite operacional pendente. Portanto, esta referência descreve contrato e implementação encontrada; não declara implantação ou validação operacional. Acesso ao banco e execução de suíte não fazem parte desta documentação. O schema completo continua em `EKM-GAP-0002`.
