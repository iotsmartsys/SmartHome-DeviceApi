# Dashboards, widgets e catálogo

**Natureza:** referência informativa; o contrato normativo é [SHD-DASHBOARD-API-V1-001@0.3](../specs/DASHBOARD-API-V1.md). Fontes de implementação: [DashboardController](../../src/Api/Controllers/DashboardController.cs), [DashboardWidgetController](../../src/Api/Controllers/DashboardWidgetController.cs), [catálogo e capabilities](../../src/Api/Controllers/DashboardWidgetTypeController.cs), [serviço Core](../../src/Core/Services/DashboardService.cs), [resolver de dados](../../src/Core/Services/DashboardDataResolver.cs), [compatibilidade](../../src/Core/Services/DashboardWidgetCompatibilityResolver.cs) e [repository](../../src/Data.Repositories/Repositories/DashboardRepository.cs).

## Modelo e relações

Dashboard possui ID Int64, nome, descrição, layout, default/order e widgets. Widget pertence a Dashboard, referencia capability por ID Int32 e inclui tipo, modo, posição, configuração e ordem. Device associado é derivado da capability; o contrato proíbe tratar `deviceId` como identidade do vínculo do widget. Tipos habilitados declaram data types compatíveis e configuração default.

## Operações

| Operação | Resultado |
|---|---|
| `GET /api/v1/dashboards` | Lista summaries em `200`. |
| `GET /api/v1/dashboards/{dashboardId}` | Busca dashboard e widgets em `200`; ausência/ID inválido traduz-se pelo service e Dashboard middleware. |
| `POST /api/v1/dashboards` | Cria dashboard, `201` e Location. |
| `PUT /api/v1/dashboards/{dashboardId}` | Atualiza conforme semântica omitido/null definida na especificação; responde `200`. |
| `DELETE /api/v1/dashboards/{dashboardId}` | Exclui, `204`; ausência gera erro de domínio. |
| `GET /api/v1/dashboards/{dashboardId}/data` | Lê fonte e catálogo, resolve valores atuais/status/apresentação, retorna `200`. |
| `POST /api/v1/dashboards/{dashboardId}/widgets` | Cria widget validando dashboard, capability, tipo, configuração e compatibilidade; `201`. |
| `PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | Atualiza parcialmente segundo regras normativas; `200`. |
| `DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | Exclui widget; `204`. |
| `GET /api/v1/dashboard-capabilities` | Lista capabilities com valores resolvidos e tipos de widget compatíveis; `200`. |
| `GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets` | Consulta uma capability e seu catálogo compatível; `200`. |
| `GET /api/v1/dashboard-widget-types` | Retorna catálogo e configurações; `200`. |

## Regras implementadas e contrato

O service valida IDs positivos e converte ausências em erros de domínio. O Core limita layout a `grid`, modo de dados a `current_value`, valida tipo habilitado, tipo de dados compatível, posições e configurações. PUT é atualização parcial; semântica detalhada de omitido/null, defaults, concorrência, ordenação, envelope de erro e persistência está na especificação e deve prevalecer. O resolver classifica status como `ok`, `stale`, `offline`, `no_data`, `invalid_value`, `error` e `capability_missing`; fonte textual inválida não é coercida livremente. `refreshIntervalSeconds` é metadado/sugestão, não agendamento de polling pela API.

## Estado e lacunas

A especificação 0.3 e o mapa registram reimplementação/build para revisão, schema não aplicado e aceite operacional pendente. Portanto, esta referência descreve contrato e implementação encontrada; não declara implantação ou validação operacional. Acesso ao banco e execução de suíte não fazem parte desta documentação. O schema completo continua em `EKM-GAP-0002`.
