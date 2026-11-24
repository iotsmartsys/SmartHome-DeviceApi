# Cobertura do conhecimento funcional e técnico por operação

**População:** 76 pares método + template do inventário auditado na revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`. A revisão local `48806770fdd486229981ae4973b70632710271e6` não alterou código, testes, SQL, OpenAPI nem especificações; por isso a população e as conclusões de comportamento continuam aplicáveis. O inventário inclui aliases e três rotas OAuth proprietárias legadas. Não representa exposição pelo gateway.

**Avaliação atual:** leitura operação por operação das referências expandidas em `docs/knowledge/`, conferidas contra handlers, DTOs, Core, repositórios/SQL, tratamento de exceções e fontes normativas aplicáveis. As classificações abaixo refletem a documentação atual; a matriz histórica foi preservada e não foi usada como resultado automático. Cada linha aponta a referência descritiva, o handler e os achados AUD associados. A auditoria original mantém schemas, links para consultas e metodologia: [matriz histórica](../DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-COVERAGE-MATRIX.md).

## Estados

| Código | Estado permitido | Critério |
|---|---|---|
| D | Documentado | Comportamento, regra, campo, resposta ou ausência de garantia descritos para o fluxo e ligados a fonte. Divergências antigas são D quando o estado efetivo e sua discrepância foram descritos. |
| DE | Documentado com dependência externa explicitada | A parte determinada pela aplicação está documentada; completar a garantia depende de fonte externa identificada (gateway/configuração ou DDL/runtime do banco). |
| NA | Não aplicável | A dimensão não se aplica ao fluxo descrito, por exemplo persistência em endpoint de relógio/health ou efeitos de uma consulta read-only sem cache. |
| P | Parcialmente documentado | A referência ainda omite detalhe interno necessário. Não usado nesta versão após a verificação dos domínios; os itens que não podem ser determinados estão delimitados como DE. |
| PI | Pendente de investigação | Não usado: não ficou questão interna identificada sem investigação nesta matriz. |
| B | Bloqueado por informação externa identificada | Não usado quando a aplicação já pode ser descrita e a garantia externa está explicitada em DE. |

As células usam os códigos acima. Uma operação conta como integralmente documentada quando todas as dimensões aplicáveis estão em D, DE ou NA. Para `Entrada` e `Obrigatoriedade`, rotas sem parâmetro/corpo usam NA; demais contratos, inclusive alias cuja action não consegue obter o parâmetro exigido, estão descritos. `Validações` NA significa que não há guarda específica identificada para o caminho e que a referência descreve binding/ausência. `Exemplos` cobre bodies representativos por DTO/handler nas referências ou especificações normativas; em operações sem body, a rota e a forma/status de resposta descritos são o exemplo aplicável. Não se exige amostra com dados reais. A dimensão de segurança delimita a fronteira externa de admissão e não afirma acesso anônimo. Testes não são uma das dimensões pedidas; nenhum foi executado nesta intervenção.

## Matriz

| Operação | Identificação | Entrada | Obrigatoriedade | Validações | Regras | Transformações | Persistência | Saída | Erros | Segurança | Efeitos | Semântica | Exemplos | Rastreabilidade | Conhecimento e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/devices/{device_id}/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L13); AUD: AUD-DEV-005 |
| `GET /api/v1/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L13); AUD: AUD-DEV-005 |
| `GET /api/v1/devices/{device_id}/capabilities/tiny` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L26); AUD: AUD-DEV-005, AUD-DEV-006 |
| `GET /api/v1/capabilities/tiny` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L26); AUD: AUD-DEV-005, AUD-DEV-006 |
| `GET /api/v1/devices/{device_id}/capabilities/platforms/{reference_id}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L56); AUD: AUD-DEV-006, AUD-DEV-005 |
| `GET /api/v1/capabilities/platforms/{reference_id}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L56); AUD: AUD-DEV-006 |
| `GET /api/v1/devices/{device_id}/capabilities/{capability_name}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L76); AUD: AUD-DEV-006 |
| `GET /api/v1/capabilities/{capability_name}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L76); AUD: AUD-DEV-006, AUD-DEV-005 |
| `GET /api/v1/devices/{device_id}/capabilities/uid/{uid}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L95); AUD: AUD-DEV-006, AUD-DEV-005 |
| `GET /api/v1/capabilities/uid/{uid}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L95); AUD: AUD-DEV-006 |
| `POST /api/v1/devices/{device_id}/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L114); AUD: AUD-DEV-001 |
| `POST /api/v1/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L114); AUD: AUD-DEV-001, AUD-DEV-005 |
| `PATCH /api/v1/devices/{device_id}/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L125); AUD: AUD-DEV-001 |
| `PATCH /api/v1/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L125); AUD: AUD-DEV-001, AUD-DEV-005 |
| `PATCH /api/v1/devices/{device_id}/capabilities/value` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L126); AUD: AUD-DEV-001 |
| `PATCH /api/v1/capabilities/value` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L126); AUD: AUD-DEV-001, AUD-DEV-005 |
| `PATCH /api/v1/devices/{device_id}/capabilities/{id}/patches` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L140); AUD: AUD-DEV-001, AUD-DEV-005 |
| `PATCH /api/v1/capabilities/{id}/patches` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L140); AUD: AUD-DEV-001 |
| `DELETE /api/v1/devices/{device_id}/capabilities/{id}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L171); AUD: AUD-DEV-005 |
| `DELETE /api/v1/capabilities/{id}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Capability](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityController.cs#L171); AUD: — |
| `GET /api/v1/capabilities/{capability_id}/history` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityHistory](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityHistoryController.cs#L8); AUD: AUD-DEV-006 |
| `POST /api/v1/capabilities/{capability_id}/history` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityHistory](../../knowledge/capabilities.md); [handler](../../../src/Api/Controllers/CapabilityHistoryController.cs#L19); AUD: AUD-DEV-001, AUD-DEV-003 |
| `GET /api/v1/capabilities-types` | D | NA | NA | NA | D | D | DE | D | D | DE | D | D | D | D | [CapabilityType](../../knowledge/capability-types.md); [handler](../../../src/Api/Controllers/CapabilityTypeController.cs#L10); AUD: — |
| `GET /api/v1/capabilities-types/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityType](../../knowledge/capability-types.md); [handler](../../../src/Api/Controllers/CapabilityTypeController.cs#L23); AUD: — |
| `POST /api/v1/capabilities-types` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityType](../../knowledge/capability-types.md); [handler](../../../src/Api/Controllers/CapabilityTypeController.cs#L36); AUD: AUD-DEV-001 |
| `PATCH /api/v1/capabilities-types/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityType](../../knowledge/capability-types.md); [handler](../../../src/Api/Controllers/CapabilityTypeController.cs#L50); AUD: AUD-DEV-001 |
| `DELETE /api/v1/capabilities-types/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [CapabilityType](../../knowledge/capability-types.md); [handler](../../../src/Api/Controllers/CapabilityTypeController.cs#L69); AUD: — |
| `GET /api/v1/dashboard-capabilities` | D | NA | NA | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardCapability](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardCapabilityController.cs#L15); AUD: — |
| `GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardCapability](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardCapabilityController.cs#L28); AUD: — |
| `GET /api/v1/dashboards` | D | NA | NA | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L15); AUD: — |
| `GET /api/v1/dashboards/{dashboardId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L23); AUD: — |
| `POST /api/v1/dashboards` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L33); AUD: AUD-DEV-001 |
| `PUT /api/v1/dashboards/{dashboardId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L45); AUD: AUD-DEV-001 |
| `DELETE /api/v1/dashboards/{dashboardId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L56); AUD: — |
| `GET /api/v1/dashboards/{dashboardId}/data` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Dashboard](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardController.cs#L65); AUD: — |
| `POST /api/v1/dashboards/{dashboardId}/widgets` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardWidget](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardWidgetController.cs#L15); AUD: AUD-DEV-001 |
| `PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardWidget](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardWidgetController.cs#L27); AUD: AUD-DEV-001 |
| `DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardWidget](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardWidgetController.cs#L38); AUD: — |
| `GET /api/v1/dashboard-widget-types` | D | NA | NA | D | D | D | DE | D | D | DE | D | D | D | D | [DashboardWidgetType](../../knowledge/dashboards-and-widgets.md); [handler](../../../src/Api/Controllers/DashboardWidgetTypeController.cs#L15); AUD: — |
| `GET /api/v1/devices` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Device](../../knowledge/devices.md); [handler](../../../src/Api/Controllers/DeviceController.cs#L13); AUD: AUD-DEV-004, AUD-DEV-010 |
| `GET /api/v1/devices/{device_id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Device](../../knowledge/devices.md); [handler](../../../src/Api/Controllers/DeviceController.cs#L27); AUD: AUD-DEV-004 |
| `POST /api/v1/devices` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Device](../../knowledge/devices.md); [handler](../../../src/Api/Controllers/DeviceController.cs#L40); AUD: AUD-DEV-001, AUD-DEV-002, AUD-DEV-003, AUD-DEV-009, AUD-DEV-012 |
| `PATCH /api/v1/devices/{device_id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Device](../../knowledge/devices.md); [handler](../../../src/Api/Controllers/DeviceController.cs#L51); AUD: AUD-DEV-001, AUD-DEV-003 |
| `DELETE /api/v1/devices/{device_id}` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [Device](../../knowledge/devices.md); [handler](../../../src/Api/Controllers/DeviceController.cs#L75); AUD: AUD-DEV-006, AUD-DEV-010 |
| `POST /api/v1/devices/{deviceId}/metrics` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DeviceMetrics](../../knowledge/device-metrics.md); [handler](../../../src/Api/Controllers/DeviceMetricsController.cs#L9); AUD: AUD-DEV-001 |
| `GET /api/v1/devices/{deviceId}/metrics/current` | D | D | D | NA | D | D | DE | D | D | DE | D | D | D | D | [DeviceMetrics](../../knowledge/device-metrics.md); [handler](../../../src/Api/Controllers/DeviceMetricsController.cs#L25); AUD: — |
| `GET /api/v1/devices/{deviceId}/metrics/history` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DeviceMetrics](../../knowledge/device-metrics.md); [handler](../../../src/Api/Controllers/DeviceMetricsController.cs#L43); AUD: — |
| `GET /api/v1/devices/{device_id}/settings` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DeviceSettings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/DeviceSettingsController.cs#L11); AUD: AUD-DEV-006, AUD-DEV-007 |
| `PUT /api/v1/devices/{device_id}/settings` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DeviceSettings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/DeviceSettingsController.cs#L35); AUD: AUD-DEV-001, AUD-DEV-007 |
| `PUT /api/v1/devices/{device_id}/settings/reset` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [DeviceSettings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/DeviceSettingsController.cs#L50); AUD: — |
| `POST /api/v1/groups` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L12); AUD: AUD-DEV-001 |
| `GET /api/v1/groups` | D | NA | NA | NA | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L52); AUD: — |
| `PUT /api/v1/groups/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L63); AUD: AUD-DEV-001, AUD-DEV-003 |
| `PATCH /api/v1/groups/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L83); AUD: AUD-DEV-001, AUD-DEV-006 |
| `DELETE /api/v1/groups/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L174); AUD: — |
| `PUT /api/v1/groups/{groupId}/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L193); AUD: AUD-DEV-001, AUD-DEV-003 |
| `DELETE /api/v1/groups/{groupId}/capabilities/{capabilityId}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Group](../../knowledge/groups.md); [handler](../../../src/Api/Controllers/GroupController.cs#L225); AUD: — |
| `GET /api/v1/places` | D | NA | NA | NA | D | D | DE | D | D | DE | D | D | D | D | [MonitoredPlace](../../knowledge/platforms-and-places.md); [handler](../../../src/Api/Controllers/MonitoredPlaceController.cs#L9); AUD: AUD-DEV-006 |
| `GET /oauth/authorize` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [OAuth](../../knowledge/legacy-access-control.md); [handler](../../../src/Api/Controllers/OAuth/OAuthController.cs#L16); AUD: AUD-DEV-006, AUD-DEV-009 |
| `POST /oauth/token` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [OAuth](../../knowledge/legacy-access-control.md); [handler](../../../src/Api/Controllers/OAuth/OAuthController.cs#L60); AUD: AUD-DEV-006, AUD-DEV-009 |
| `GET /oauth/me` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [OAuth](../../knowledge/legacy-access-control.md); [handler](../../../src/Api/Controllers/OAuth/OAuthController.cs#L182); AUD: AUD-DEV-006, AUD-DEV-009 |
| `GET /api/v1/platforms` | D | NA | NA | NA | D | D | DE | D | D | DE | D | D | D | D | [Platform](../../knowledge/platforms-and-places.md); [handler](../../../src/Api/Controllers/PlatformController.cs#L9); AUD: AUD-DEV-006 |
| `GET /api/v1/devices/{device_id}/properties/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L9); AUD: AUD-DEV-013 |
| `GET /api/v1/devices/{device_id}/properties` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L26); AUD: AUD-DEV-013 |
| `POST /api/v1/devices/{device_id}/properties` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L43); AUD: AUD-DEV-001, AUD-DEV-008 |
| `PUT /api/v1/devices/{device_id}/properties` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L68); AUD: AUD-DEV-001, AUD-DEV-003 |
| `PATCH /api/v1/devices/{device_id}/properties` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L69); AUD: AUD-DEV-001, AUD-DEV-003 |
| `DELETE /api/v1/devices/{device_id}/properties/{id}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Properties](../../knowledge/properties.md); [handler](../../../src/Api/Controllers/PropertiesController.cs#L85); AUD: — |
| `GET /api/v1/settings` | D | NA | NA | NA | D | D | DE | D | D | DE | D | D | D | D | [Settings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/SettingsController.cs#L9); AUD: — |
| `PUT /api/v1/settings/{name}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Settings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/SettingsController.cs#L23); AUD: AUD-DEV-001, AUD-DEV-007 |
| `PUT /api/v1/settings` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [Settings](../../knowledge/settings.md); [handler](../../../src/Api/Controllers/SettingsController.cs#L35); AUD: AUD-DEV-001, AUD-DEV-007 |
| `GET /api/v1/smart-home/{smart_home_id}/capabilities` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [SmartHomeCapability](../../knowledge/smart-home-capabilities.md); [handler](../../../src/Api/Controllers/SmartHomeCapabilityController.cs#L9); AUD: AUD-DEV-005, AUD-DEV-006 |
| `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}` | D | D | D | D | D | D | DE | D | D | DE | D | D | D | D | [SmartHomeCapability](../../knowledge/smart-home-capabilities.md); [handler](../../../src/Api/Controllers/SmartHomeCapabilityController.cs#L26); AUD: AUD-DEV-005, AUD-DEV-006 |
| `GET /api/v1/timezone` | D | D | D | D | D | D | NA | D | D | DE | D | D | D | D | [Api](../../knowledge/utility-endpoints.md); [handler](../../../src/Api/Program.cs#L115); AUD: AUD-DEV-006 |
| `GET /api/v1/timezone/datetime` | D | NA | NA | NA | D | D | NA | D | D | DE | D | D | D | D | [Api](../../knowledge/utility-endpoints.md); [handler](../../../src/Api/Program.cs#L123); AUD: AUD-DEV-006 |
| `GET /api/v1/health` | D | NA | NA | NA | D | D | NA | D | D | DE | D | D | D | D | [Api](../../knowledge/utility-endpoints.md); [handler](../../../src/Api/Program.cs#L129); AUD: AUD-DEV-006 |

## Síntese quantitativa

- **Operações integralmente documentadas nas dimensões aplicáveis:** 76/76.
- **Operações parcialmente documentadas:** 0/76.
- **Operações bloqueadas por informação externa:** 0/76.
- **Operações pendentes de investigação interna:** 0/76.
- **Operações com dependência externa explicitada em ao menos uma dimensão:** 76/76; todas dependem de configuração de admissão/segurança do gateway que não está versionada. 73 operações também consultam ou alteram MySQL e dependem do schema/configuração instalado.
- **Operações sem persistência de banco:** 3/76 (`/timezone`, `/timezone/datetime`, `/health`).

Contagem das 14 dimensões classificadas por operação: Documentado = 876; Documentado com dependência externa explicitada = 149; Não aplicável = 39; Parcialmente documentado = 0; Pendente de investigação = 0; Bloqueado por informação externa identificada = 0 (1.064 células no total).

Esta contagem não mede qualidade nem execução. Ela informa que as 76 operações estão individualmente descritas e rastreadas. Dependências externas não impedem documentar o fluxo interno conhecido. A auditoria identificou testes/checks automatizados em 14/76 operações; eles não foram executados nesta consolidação e sua existência não altera a cobertura documental. `tests/Api.Tests` permanece `Retired`.

## Fontes e continuidade

- [Auditoria, inventário e método](../DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-AUDIT.md).
- [Matriz histórica por operação](../DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-COVERAGE-MATRIX.md).
- [Tratamento documental dos achados](DEVICE-API-AUDIT-KNOWLEDGE-RECONCILIATION.md).
- [Lacunas externas legítimas](DEVICE-API-RESIDUAL-KNOWLEDGE-GAPS.md).
- [Relatório desta intervenção](DEVICE-API-KNOWLEDGE-CONSOLIDATION-IMPLEMENTATION.md).

As referências são descritivas. As especificações citadas por domínio permanecem as autoridades normativas no respectivo escopo; esta matriz não cria requisito nem declara implantação.
