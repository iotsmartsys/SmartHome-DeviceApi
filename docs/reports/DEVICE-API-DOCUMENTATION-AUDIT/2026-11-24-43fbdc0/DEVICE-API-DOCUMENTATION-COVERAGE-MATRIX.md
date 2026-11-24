# Matriz por operação e comportamento

Revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f` · 24/11/2026 · [Relatório](DEVICE-API-DOCUMENTATION-AUDIT.md) · [Catálogo](DEVICE-API-DOCUMENTATION-AUDIT-FINDINGS.md)

A unidade é o par método + template na aplicação. A matriz conserva aliases, remove apenas a barra terminal e expõe o seletor inteiro na evidência C#. Uma rota presente não garante que a ação receba todos os parâmetros nem que seja acessível pelo ingresso. As três rotas legadas integram a população. Interfaces de documentação de Development e trabalho hospedado aparecem separadamente.

**Leitura das colunas.** F finalidade, E entrada, V validações, R regra funcional, P persistência/leitura, S segurança ponta a ponta, O saída de sucesso, X erros, L efeitos colaterais, M repetição/concorrência, Ex exemplos, T testes existentes, Tr rastreabilidade. C = completo e consistente no recorte da coluna, P = parcial, A = ausente no conjunto pesquisado, K = contraditório, D = divergente da implementação, B = obsoleto, V = não verificável, N = não aplicável. As avaliações medem os documentos de origem anteriores à auditoria. Um exemplo de resposta, cenário textual ou payload parcial conta como P. Um exemplo antigo incompatível recebe B. T refere-se a cenários automatizados identificados no repositório ativo, incluindo cleanup e checks k6. Provas novas, aceitações humanas e cenários apenas planejados estão discriminados no relatório.

**Como verificar cada célula.** A operação e seu método selecionam exatamente uma entrada em [paths](../../../contracts/openapi.json). O link da ação, a referência funcional e o repositório no título do domínio sustentam finalidade, parâmetros, validação, transformação, resposta, efeitos e SQL. A matriz histórica detalha parâmetros, media types e schemas de cada par em [reconciliação §§ Inventário de operações e schemas](../../../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O link HTTP em cada linha leva exatamente à operação no snapshot, com parâmetros de path/query, tipos, required, mídia de entrada, respostas e referências de schema. A coluna Norma indica autoridade específica. Ar* significa somente estado do ar e a divergência de workflow registrada no catálogo. Uma referência normativa parcial não transforma a rota inteira em contrato. Sem norma específica, Tr=P quando há mapa → referência → código.

S usa [dossiê §8](../../../specs/SYSTEM-DOSSIER.md), [bootstrap](../../../../src/Api/Program.cs#L84) e AUD-DEV-009. Para X, comparar [middleware geral](../../../../src/Api/Middlewares/ExceptionHandler.cs#L29) e, para Dashboard, [filtro](../../../../src/Api/Middlewares/DashboardRequestFilter.cs) e [middleware próprio](../../../../src/Api/Middlewares/DashboardExceptionHandler.cs). Para P/M, as consultas documentam comandos, com [limite do DDL](../../../rfc/KNOWLEDGE-MAP.md#L125). Para T/Ex, o universo é o inventário de testes e exemplos do relatório. C em O refere-se ao caminho de sucesso, inclusive respostas vazias, e não certifica o erro X nem produção. N em V/M/L indica que não se identificou validação específica, garantia operacional material ou efeito adicional a documentar naquele recorte. Os números na última coluna são os sufixos AUD-DEV.

## Capability

Fonte funcional [capabilities, linhas 11–44](../../../knowledge/capabilities.md). Persistência em [CapabilityRepository](../../../../src/Data.Repositories/Repositories/CapabilityRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/devices/{device_id}/capabilities` | C | P | P | D | P | P | P | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetAllCapabilities:13](../../../../src/Api/Controllers/CapabilityController.cs#L13) · [HTTP](../../../contracts/openapi.json#L55) · Consulta por filtros do DTO. 200 array ou 404. Prefixo de device não vincula a ação ao pai. · 005 |
| `GET /api/v1/capabilities` | C | P | P | P | P | P | P | P | N | N | A | P | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetAllCapabilities:13](../../../../src/Api/Controllers/CapabilityController.cs#L13) · [HTTP](../../../contracts/openapi.json#L402) · Consulta por filtros do DTO. 200 array ou 404. k6 verifica só status. · 005 |
| `GET /api/v1/devices/{device_id}/capabilities/tiny` | C | P | P | P | P | P | D | P | P | P | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetAllCapabilitiesTiny:26](../../../../src/Api/Controllers/CapabilityController.cs#L26) · [HTTP](../../../contracts/openapi.json#L741) · Quatro tipos excluídos. MCU entrega arquivo texto, JSON entrega coleção. Chave do cache de 2 s cobre apenas parte dos filtros. Prefixo de device não vincula a ação ao pai. · 005,006 |
| `GET /api/v1/capabilities/tiny` | C | P | P | P | P | P | D | P | P | P | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetAllCapabilitiesTiny:26](../../../../src/Api/Controllers/CapabilityController.cs#L26) · [HTTP](../../../contracts/openapi.json#L889) · Quatro tipos excluídos. MCU entrega arquivo texto, JSON entrega coleção. Chave do cache de 2 s cobre apenas parte dos filtros. · 005,006 |
| `GET /api/v1/devices/{device_id}/capabilities/platforms/{reference_id}` | C | C | N | P | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByReferenceId:56](../../../../src/Api/Controllers/CapabilityController.cs#L56) · [HTTP](../../../contracts/openapi.json#L1029) · Uma capability pela referência, 404 se ausente. Prefixo de device não vincula a ação ao pai. · 006,005 |
| `GET /api/v1/capabilities/platforms/{reference_id}` | C | C | N | C | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByReferenceId:56](../../../../src/Api/Controllers/CapabilityController.cs#L56) · [HTTP](../../../contracts/openapi.json#L1108) · Uma capability pela referência, 404 se ausente. · 006 |
| `GET /api/v1/devices/{device_id}/capabilities/{capability_name}` | C | C | P | C | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByName:76](../../../../src/Api/Controllers/CapabilityController.cs#L76) · [HTTP](../../../contracts/openapi.json#L1179) · Uma capability por device/nome. · 006 |
| `GET /api/v1/capabilities/{capability_name}` | C | D | P | C | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByName:76](../../../../src/Api/Controllers/CapabilityController.cs#L76) · [HTTP](../../../contracts/openapi.json#L1258) · Uma capability por device/nome. Falta segmento para o parâmetro de rota exigido. · 006,005 |
| `GET /api/v1/devices/{device_id}/capabilities/uid/{uid}` | C | C | N | P | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByUid:95](../../../../src/Api/Controllers/CapabilityController.cs#L95) · [HTTP](../../../contracts/openapi.json#L1337) · Uma capability por UID, 404 se ausente. Prefixo de device não vincula a ação ao pai. · 006,005 |
| `GET /api/v1/capabilities/uid/{uid}` | C | C | N | C | P | P | D | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetCapabilityByUid:95](../../../../src/Api/Controllers/CapabilityController.cs#L95) · [HTTP](../../../contracts/openapi.json#L1416) · Uma capability por UID, 404 se ausente. · 006 |
| `POST /api/v1/devices/{device_id}/capabilities` | C | D | P | P | P | P | C | P | P | P | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [AddCapabilitiesAsync:114](../../../../src/Api/Controllers/CapabilityController.cs#L114) · [HTTP](../../../contracts/openapi.json#L194) · Coleção vinculada ao device. Serviço delega ao repositório, normalização do ar por tipo persistido. · 001 |
| `POST /api/v1/capabilities` | C | D | P | P | P | P | C | P | P | P | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [AddCapabilitiesAsync:114](../../../../src/Api/Controllers/CapabilityController.cs#L114) · [HTTP](../../../contracts/openapi.json#L533) · Coleção vinculada ao device. Serviço delega ao repositório, normalização do ar por tipo persistido. Falta segmento para o parâmetro de rota exigido. · 001,005 |
| `PATCH /api/v1/devices/{device_id}/capabilities` | C | D | P | P | P | P | C | P | P | P | P | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [UpdateValueCapability:125](../../../../src/Api/Controllers/CapabilityController.cs#L125) · [HTTP](../../../contracts/openapi.json#L293) · Localiza por device/nome e combina estado do ar sob lock, devolve 204 ou 404. · 001 |
| `PATCH /api/v1/capabilities` | C | D | P | P | P | P | C | P | P | P | P | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [UpdateValueCapability:125](../../../../src/Api/Controllers/CapabilityController.cs#L125) · [HTTP](../../../contracts/openapi.json#L632) · Localiza por device/nome e combina estado do ar sob lock, devolve 204 ou 404. Falta segmento para o parâmetro de rota exigido. · 001,005 |
| `PATCH /api/v1/devices/{device_id}/capabilities/value` | C | D | P | P | P | P | C | P | P | P | P | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [UpdateValueCapability:126](../../../../src/Api/Controllers/CapabilityController.cs#L126) · [HTTP](../../../contracts/openapi.json#L1487) · Localiza por device/nome e combina estado do ar sob lock, devolve 204 ou 404. · 001 |
| `PATCH /api/v1/capabilities/value` | C | D | P | P | P | P | C | P | P | P | P | P | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [UpdateValueCapability:126](../../../../src/Api/Controllers/CapabilityController.cs#L126) · [HTTP](../../../contracts/openapi.json#L1596) · Localiza por device/nome e combina estado do ar sob lock, devolve 204 ou 404. Falta segmento para o parâmetro de rota exigido. k6 verifica só status. · 001,005 |
| `PATCH /api/v1/devices/{device_id}/capabilities/{id}/patches` | C | D | P | P | P | P | C | P | P | P | P | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [PatchAsync:140](../../../../src/Api/Controllers/CapabilityController.cs#L140) · [HTTP](../../../contracts/openapi.json#L1705) · Por id interno. Metadados sem escrita de value evitam reaplicar estado antigo. Prefixo de device não vincula a ação ao pai. · 001,005 |
| `PATCH /api/v1/capabilities/{id}/patches` | C | D | P | P | P | P | C | P | P | P | P | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [PatchAsync:140](../../../../src/Api/Controllers/CapabilityController.cs#L140) · [HTTP](../../../contracts/openapi.json#L1835) · Por id interno. Metadados sem escrita de value evitam reaplicar estado antigo. · 001 |
| `DELETE /api/v1/devices/{device_id}/capabilities/{id}` | C | C | N | P | P | P | C | P | P | P | A | A | P | — | [DeleteCapabilities:171](../../../../src/Api/Controllers/CapabilityController.cs#L171) · [HTTP](../../../contracts/openapi.json#L1957) · Excluir por id com transação. Ação não decide 404 pelo número de linhas. Prefixo de device não vincula a ação ao pai. · 005 |
| `DELETE /api/v1/capabilities/{id}` | C | C | N | P | P | P | C | P | P | P | A | A | P | — | [DeleteCapabilities:171](../../../../src/Api/Controllers/CapabilityController.cs#L171) · [HTTP](../../../contracts/openapi.json#L2011) · Excluir por id com transação. Ação não decide 404 pelo número de linhas. |

## CapabilityHistory

Fonte funcional [capabilities, linhas 30–44](../../../knowledge/capabilities.md). Persistência em [CapabilityHistoryRepository](../../../../src/Data.Repositories/Repositories/CapabilityHistoryRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/capabilities/{capability_id}/history` | C | P | P | P | C | P | D | P | N | N | A | A | P | — | [GetAllCapabilities:8](../../../../src/Api/Controllers/CapabilityHistoryController.cs#L8) · [HTTP](../../../contracts/openapi.json#L2057) · Zero resultados dá 200 []. Janela e limites de data combináveis, ordenação por timestamp/id DESC. · 006 |
| `POST /api/v1/capabilities/{capability_id}/history` | C | D | P | P | P | P | P | P | P | P | A | A | P | — | [CreateCapabilityHistory:19](../../../../src/Api/Controllers/CapabilityHistoryController.cs#L19) · [HTTP](../../../contracts/openapi.json#L2150) · Valida value, consulta capability por id, insere histórico resolvendo nome, Location aponta à coleção. Repetição pode acrescentar entrada. · 001,003 |

## CapabilityType

Fonte funcional [capability-types, linhas 11–25](../../../knowledge/capability-types.md). Persistência em [CapabilityTypeRepository](../../../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/capabilities-types` | C | C | N | C | C | P | C | P | N | N | P | A | C | [CT](../../../specs/CAPABILITY-TYPE-ID.md) | [GetCapabilitiesTypes:10](../../../../src/Api/Controllers/CapabilityTypeController.cs#L10) · [HTTP](../../../contracts/openapi.json#L2240) · Nome parcial, ícones agrupados por id, vazio 204. |
| `GET /api/v1/capabilities-types/{id}` | C | C | C | C | C | P | C | P | N | N | P | A | C | [CT](../../../specs/CAPABILITY-TYPE-ID.md) | [GetCapabilityTypeById:23](../../../../src/Api/Controllers/CapabilityTypeController.cs#L23) · [HTTP](../../../contracts/openapi.json#L2367) · Seletor int, consulta com ícones, ausência 404. |
| `POST /api/v1/capabilities-types` | C | D | P | C | C | P | P | P | C | P | P | A | C | [CT](../../../specs/CAPABILITY-TYPE-ID.md) | [CreateCapabilityType:36](../../../../src/Api/Controllers/CapabilityTypeController.cs#L36) · [HTTP](../../../contracts/openapi.json#L2291) · Id vem do INSERT, ícones na transação, 201 com id no corpo e Location. · 001 |
| `PATCH /api/v1/capabilities-types/{id}` | C | D | P | C | C | P | C | P | C | P | P | A | C | [CT](../../../specs/CAPABILITY-TYPE-ID.md) | [UpdateCapabilityType:50](../../../../src/Api/Controllers/CapabilityTypeController.cs#L50) · [HTTP](../../../contracts/openapi.json#L2429) · Chave original preservada no patch. Substituição de ícones e tipo em transação. · 001 |
| `DELETE /api/v1/capabilities-types/{id}` | C | C | C | C | P | P | C | P | P | C | P | A | C | [CT](../../../specs/CAPABILITY-TYPE-ID.md) | [DeleteCapabilityType:69](../../../../src/Api/Controllers/CapabilityTypeController.cs#L69) · [HTTP](../../../contracts/openapi.json#L2506) · 204 também na repetição. Restrições relacionais fora da garantia da especificação. |

## DashboardCapability

Fonte funcional [dashboards-and-widgets, linhas 11–34](../../../knowledge/dashboards-and-widgets.md). Persistência em [DashboardRepository](../../../../src/Data.Repositories/Repositories/DashboardRepository.cs).
Core em [DashboardService](../../../../src/Core/Services/DashboardService.cs), resolvers de dados/compatibilidade e [suíte IT-01 a CLEAN](../../../../tests/Api.IntegrationTests/DashboardIntegrationSuite.cs#L20).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/dashboard-capabilities` | C | C | C | C | C | P | P | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetAllDashboardCapabilitiesAsync:15](../../../../src/Api/Controllers/DashboardCapabilityController.cs#L15) · [HTTP](../../../contracts/openapi.json#L3416) · Valores atuais e catálogo de widgets compatíveis. IT-01 |
| `GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets` | C | C | C | C | C | P | C | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetCompatibleDashboardWidgetsAsync:28](../../../../src/Api/Controllers/DashboardCapabilityController.cs#L28) · [HTTP](../../../contracts/openapi.json#L3545) · Capability individual e tipos habilitados e compatíveis. IT-01 |

## Dashboard

Fonte funcional [dashboards-and-widgets, linhas 11–34](../../../knowledge/dashboards-and-widgets.md). Persistência em [DashboardRepository](../../../../src/Data.Repositories/Repositories/DashboardRepository.cs).
Core em [DashboardService](../../../../src/Core/Services/DashboardService.cs), resolvers de dados/compatibilidade e [suíte IT-01 a CLEAN](../../../../tests/Api.IntegrationTests/DashboardIntegrationSuite.cs#L20).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/dashboards` | C | C | C | C | C | P | C | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetAllDashboardsAsync:15](../../../../src/Api/Controllers/DashboardController.cs#L15) · [HTTP](../../../contracts/openapi.json#L2532) · Lista summaries, ordenação e contagem de widgets. IT-04 |
| `GET /api/v1/dashboards/{dashboardId}` | C | C | C | C | C | P | C | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetDashboardByIdAsync:23](../../../../src/Api/Controllers/DashboardController.cs#L23) · [HTTP](../../../contracts/openapi.json#L2832) · Lê dashboard e capabilities para composição, ausência 404. IT-04 |
| `POST /api/v1/dashboards` | C | D | C | C | C | P | P | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [AddDashboardAsync:33](../../../../src/Api/Controllers/DashboardController.cs#L33) · [HTTP](../../../contracts/openapi.json#L2659) · Criação com defaults e regra de padrão global, 201/Location. IT-02 · 001 |
| `PUT /api/v1/dashboards/{dashboardId}` | C | D | C | C | C | P | C | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [UpdateDashboardAsync:45](../../../../src/Api/Controllers/DashboardController.cs#L45) · [HTTP](../../../contracts/openapi.json#L2971) · PUT parcial com omitido/null e no-op. IT-05 · 001 |
| `DELETE /api/v1/dashboards/{dashboardId}` | C | C | C | C | C | P | C | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [DeleteDashboardAsync:56](../../../../src/Api/Controllers/DashboardController.cs#L56) · [HTTP](../../../contracts/openapi.json#L3153) · Apaga dashboard com widgets associados, repetição 404. CLEAN |
| `GET /api/v1/dashboards/{dashboardId}/data` | C | C | C | C | C | P | P | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetDashboardDataAsync:65](../../../../src/Api/Controllers/DashboardController.cs#L65) · [HTTP](../../../contracts/openapi.json#L3276) · Resolve estado, timestamp e apresentação por fonte, agora UTC. IT-04 |

## DashboardWidget

Fonte funcional [dashboards-and-widgets, linhas 11–34](../../../knowledge/dashboards-and-widgets.md). Persistência em [DashboardRepository](../../../../src/Data.Repositories/Repositories/DashboardRepository.cs).
Core em [DashboardService](../../../../src/Core/Services/DashboardService.cs), resolvers de dados/compatibilidade e [suíte IT-01 a CLEAN](../../../../tests/Api.IntegrationTests/DashboardIntegrationSuite.cs#L20).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `POST /api/v1/dashboards/{dashboardId}/widgets` | C | D | C | C | C | P | P | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [AddDashboardWidgetAsync:15](../../../../src/Api/Controllers/DashboardWidgetController.cs#L15) · [HTTP](../../../contracts/openapi.json#L3685) · Vincula capability por id. Location identifica o dashboard pai. IT-03 · 001 |
| `PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | C | D | C | C | C | P | C | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [UpdateDashboardWidgetAsync:27](../../../../src/Api/Controllers/DashboardWidgetController.cs#L27) · [HTTP](../../../contracts/openapi.json#L3869) · PUT preserva campos omitidos, redefine nulos e combina config. IT-06–09 · 001 |
| `DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}` | C | C | C | C | C | P | C | C | C | C | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [DeleteDashboardWidgetAsync:38](../../../../src/Api/Controllers/DashboardWidgetController.cs#L38) · [HTTP](../../../contracts/openapi.json#L4060) · Remove widget no pai, confere ausência no cleanup. CLEAN |

## DashboardWidgetType

Fonte funcional [dashboards-and-widgets, linhas 11–34](../../../knowledge/dashboards-and-widgets.md). Persistência em [DashboardRepository](../../../../src/Data.Repositories/Repositories/DashboardRepository.cs).
Core em [DashboardService](../../../../src/Core/Services/DashboardService.cs), resolvers de dados/compatibilidade e [suíte IT-01 a CLEAN](../../../../tests/Api.IntegrationTests/DashboardIntegrationSuite.cs#L20).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/dashboard-widget-types` | C | C | C | C | C | P | C | C | N | N | P | P | C | [Dash](../../../specs/DASHBOARD-API-V1.md) | [GetAllDashboardWidgetTypesAsync:15](../../../../src/Api/Controllers/DashboardWidgetTypeController.cs#L15) · [HTTP](../../../contracts/openapi.json#L4192) · Catálogo, ordenação, configuração e compatibilidade. IT-01 |

## Device

Fonte funcional [devices, linhas 15–33](../../../knowledge/devices.md). Persistência em [DeviceRepository](../../../../src/Data.Repositories/Repositories/DeviceRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/devices` | C | C | N | D | P | P | D | P | N | N | B | A | P | — | [GetDevices:13](../../../../src/Api/Controllers/DeviceController.cs#L13) · [HTTP](../../../contracts/openapi.json#L4321) · [], filtros combináveis e plataforma CSV. Só carrega properties/settings na consulta base. O README mostra capabilities preenchidas. · 004,010 |
| `GET /api/v1/devices/{device_id}` | C | C | P | D | P | P | D | P | N | N | A | A | P | — | [GetDevice:27](../../../../src/Api/Controllers/DeviceController.cs#L27) · [HTTP](../../../contracts/openapi.json#L4470) · Busca por chave pública, 404 ou DTO com propriedades. Capabilities inicializadas vazias neste caminho. · 004 |
| `POST /api/v1/devices` | C | D | D | D | D | P | P | D | D | P | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [CreateDevice:40](../../../../src/Api/Controllers/DeviceController.cs#L40) · [HTTP](../../../contracts/openapi.json#L4411) · 201 com Location e zero bytes após sucesso do repositório. Ver 25 respostas no relatório. · 001,002,003,009,012 |
| `PATCH /api/v1/devices/{device_id}` | C | D | P | P | P | P | C | P | P | P | A | A | P | — | [UpdateDevice:51](../../../../src/Api/Controllers/DeviceController.cs#L51) · [HTTP](../../../contracts/openapi.json#L4530) · JSON Patch seguido de UPDATE de escalares. Chave interna e identificador da rota têm papéis diferentes. · 001,003 |
| `DELETE /api/v1/devices/{device_id}` | C | C | N | P | V | P | C | D | P | P | A | A | P | — | [DeleteDevice:75](../../../../src/Api/Controllers/DeviceController.cs#L75) · [HTTP](../../../contracts/openapi.json#L4606) · 204 após DELETE por chave pública, sem conferir linhas. Cascade geral alegado no README depende do DDL. · 006,010 |

## DeviceMetrics

Fonte funcional [device-metrics, linhas 11–25](../../../knowledge/device-metrics.md). Persistência em [DeviceMetricsRepository](../../../../src/Data.Repositories/Repositories/DeviceMetricsRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `POST /api/v1/devices/{deviceId}/metrics` | C | D | P | C | C | P | C | P | C | C | A | A | P | — | [Save:9](../../../../src/Api/Controllers/DeviceMetricsController.cs#L9) · [HTTP](../../../contracts/openapi.json#L4648) · device_id do corpo exigido, chave usada vem da rota. Upsert atual + insert histórico, uma transação. · 001 |
| `GET /api/v1/devices/{deviceId}/metrics/current` | C | C | N | C | C | P | C | P | N | N | A | A | P | — | [GetCurrent:25](../../../../src/Api/Controllers/DeviceMetricsController.cs#L25) · [HTTP](../../../contracts/openapi.json#L4734) · Snapshot do device ou 404. |
| `GET /api/v1/devices/{deviceId}/metrics/history` | C | C | C | C | C | P | C | P | N | P | A | A | P | — | [GetHistory:43](../../../../src/Api/Controllers/DeviceMetricsController.cs#L43) · [HTTP](../../../contracts/openapi.json#L4793) · Intervalo inclusivo, page≥1, pageSize 1–500, total, ordem por recepção decrescente. |

## DeviceSettings

Fonte funcional [settings, linhas 11–26](../../../knowledge/settings.md). Persistência em [DeviceSettingsRepository](../../../../src/Data.Repositories/Repositories/DeviceSettingsRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/devices/{device_id}/settings` | C | C | P | D | D | P | D | P | N | N | A | A | P | — | [GetDeviceSettingsAsync:11](../../../../src/Api/Controllers/DeviceSettingsController.cs#L11) · [HTTP](../../../contracts/openapi.json#L4906) · Consulta view efetiva ordenada por nome. true/yes dá coleção, padrão dicionário, vazio 404 com mensagem. · 006,007 |
| `PUT /api/v1/devices/{device_id}/settings` | C | D | P | P | P | P | C | D | P | P | A | A | P | — | [UpdateDeviceSettingsAsync:35](../../../../src/Api/Controllers/DeviceSettingsController.cs#L35) · [HTTP](../../../contracts/openapi.json#L4980) · Array vazio 400. Upsert em bloco mantém descrição anterior se nova descrição vazia/nula em colisão. · 001,007 |
| `PUT /api/v1/devices/{device_id}/settings/reset` | C | C | C | C | C | P | C | P | C | C | P | A | C | [Reset](../../../specs/DEVICE-SETTINGS-RESET.md) | [ResetDeviceSettingsAsync:50](../../../../src/Api/Controllers/DeviceSettingsController.cs#L50) · [HTTP](../../../contracts/openapi.json#L5058) · Sem body. Só DELETE de overrides. Existente sem linhas 204, inexistente 404. Lookup e DELETE separados. |

## Group

Fonte funcional [groups, linhas 11–25](../../../knowledge/groups.md). Persistência em [GroupRepository](../../../../src/Data.Repositories/Repositories/GroupRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `POST /api/v1/groups` | C | D | P | C | C | P | C | P | C | P | P | A | C | [Grp](../../../specs/GROUPS-MAINTENANCE-SUPPORT.md) | [AddGroupAsync:12](../../../../src/Api/Controllers/GroupController.cs#L12) · [HTTP](../../../contracts/openapi.json#L5100) · Trim de name e icon.name. Capabilities legadas opcionais, grupo/associações transacionais. Sucesso 200. · 001 |
| `GET /api/v1/groups` | C | C | N | C | C | P | C | P | N | N | P | A | C | [Grp](../../../specs/GROUPS-MAINTENANCE-SUPPORT.md) | [GetAllGroupsAsync:52](../../../../src/Api/Controllers/GroupController.cs#L52) · [HTTP](../../../contracts/openapi.json#L5231) · 200 com array inclusive vazio, leitura mantém associações. |
| `PUT /api/v1/groups/{id}` | C | D | P | P | P | P | C | P | P | P | A | A | P | — | [UpdateGroupAsync:63](../../../../src/Api/Controllers/GroupController.cs#L63) · [HTTP](../../../contracts/openapi.json#L5309) · PUT integral, ids divergentes 404. O repositório reconstrói associações no update. · 001,003 |
| `PATCH /api/v1/groups/{id}` | C | D | C | C | C | P | C | C | C | C | P | A | C | [Grp](../../../specs/GROUPS-MAINTENANCE-SUPPORT.md) | [UpdatePatchGroupAsync:83](../../../../src/Api/Controllers/GroupController.cs#L83) · [HTTP](../../../contracts/openapi.json#L5397) · Somente replace nos três paths autorizados, valida antes do UPDATE, preserva relações. · 001,006 |
| `DELETE /api/v1/groups/{id}` | C | C | C | C | C | P | C | C | C | C | P | A | C | [Grp](../../../specs/GROUPS-MAINTENANCE-SUPPORT.md) | [DeleteGroupAsync:174](../../../../src/Api/Controllers/GroupController.cs#L174) · [HTTP](../../../contracts/openapi.json#L5530) · Id inválido 400, ausência 404, existente 204 após transação que remove relações e grupo. |
| `PUT /api/v1/groups/{groupId}/capabilities` | C | D | P | P | P | P | C | P | P | P | A | A | P | — | [AddCapabilityToGroupAsync:193](../../../../src/Api/Controllers/GroupController.cs#L193) · [HTTP](../../../contracts/openapi.json#L5633) · Nome validado, vínculo escrito com id numérico do DTO. Duplicidade depende da constraint. · 001,003 |
| `DELETE /api/v1/groups/{groupId}/capabilities/{capabilityId}` | C | C | C | C | P | P | C | P | C | P | A | A | P | — | [DeleteCapabilityFromGroupAsync:225](../../../../src/Api/Controllers/GroupController.cs#L225) · [HTTP](../../../contracts/openapi.json#L5723) · IDs positivos e existência do grupo. Remove vínculo, sem verificação local de linhas da associação. |

## MonitoredPlace

Fonte funcional [platforms-and-places, linhas 7–18](../../../knowledge/platforms-and-places.md). Persistência em [PlatformRepository](../../../../src/Data.Repositories/Repositories/PlatformRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/places` | C | C | N | C | C | P | D | P | N | N | A | A | P | — | [GetAllAsync:9](../../../../src/Api/Controllers/MonitoredPlaceController.cs#L9) · [HTTP](../../../contracts/openapi.json#L5778) · Consulta catálogo local. 200 array ou 204 vazio, omitido nos metadados. · 006 |

## OAuth

Fonte funcional [legacy-access-control, linhas 12–28](../../../knowledge/legacy-access-control.md). Persistência em [OAuthAccessTokenRepository](../../../../src/Data.Repositories/Repositories/OAuthAccessTokenRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /oauth/authorize` | C | P | P | C | P | P | P | P | P | P | A | A | P | — | [Authorize:16](../../../../src/Api/Controllers/OAuth/OAuthController.cs#L16) · [HTTP](../../../contracts/openapi.json#L5819) · Controle legado. Query manual, grava code temporário e responde com redirecionamento. · 006,009 |
| `POST /oauth/token` | C | P | P | C | P | P | P | P | P | P | A | A | P | — | [Token:60](../../../../src/Api/Controllers/OAuth/OAuthController.cs#L60) · [HTTP](../../../contracts/openapi.json#L5851) · Form/credenciais do cliente, valida grant, atualiza uso e cria token em operações distintas. · 006,009 |
| `GET /oauth/me` | C | P | P | C | P | P | P | P | N | N | A | A | P | — | [Me:182](../../../../src/Api/Controllers/OAuth/OAuthController.cs#L182) · [HTTP](../../../contracts/openapi.json#L5903) · Lê Bearer, devolve identidade e scope, ou 401. · 006,009 |

## Platform

Fonte funcional [platforms-and-places, linhas 7–18](../../../knowledge/platforms-and-places.md). Persistência em [PlatformRepository](../../../../src/Data.Repositories/Repositories/PlatformRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/platforms` | C | C | N | C | C | P | D | P | N | N | A | A | P | — | [GetAllAsync:9](../../../../src/Api/Controllers/PlatformController.cs#L9) · [HTTP](../../../contracts/openapi.json#L5935) · Consulta catálogo local. 200 array ou 204 vazio, omitido nos metadados. · 006 |

## Properties

Fonte funcional [properties, linhas 11–23](../../../knowledge/properties.md). Persistência em [PropertyRepository](../../../../src/Data.Repositories/Repositories/PropertyRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/devices/{device_id}/properties/{id}` | C | C | P | D | D | P | D | D | N | N | A | A | P | — | [GetByIdAsync:9](../../../../src/Api/Controllers/PropertiesController.cs#L9) · [HTTP](../../../contracts/openapi.json#L5976) · Lookup anunciado por id e device, mas o builder perde o SELECT. · 013 |
| `GET /api/v1/devices/{device_id}/properties` | C | C | P | D | D | P | D | D | N | N | A | A | P | — | [GetAllAsync:26](../../../../src/Api/Controllers/PropertiesController.cs#L26) · [HTTP](../../../contracts/openapi.json#L6099) · Sem filtro, SELECT por device e vazio 204. Filtros acrescentam predicado sem conjunção. · 013 |
| `POST /api/v1/devices/{device_id}/properties` | C | D | P | P | P | P | D | D | P | P | A | A | P | — | [AddAsync:43](../../../../src/Api/Controllers/PropertiesController.cs#L43) · [HTTP](../../../contracts/openapi.json#L6172) · Repositório termina antes de formatar CreatedAtAction. Falha na geração de Location em host de controle. · 001,008 |
| `PUT /api/v1/devices/{device_id}/properties` | C | D | P | P | P | P | C | P | P | P | A | A | P | — | [CreateOrUpdateAsync:68](../../../../src/Api/Controllers/PropertiesController.cs#L68) · [HTTP](../../../contracts/openapi.json#L6259) · Recebe objeto de property. No ramo de colisão, SQL muda nome/value/timestamp e preserva description. · 001,003 |
| `PATCH /api/v1/devices/{device_id}/properties` | C | D | P | P | P | P | C | P | P | P | A | A | P | — | [CreateOrUpdateAsync:69](../../../../src/Api/Controllers/PropertiesController.cs#L69) · [HTTP](../../../contracts/openapi.json#L6346) · Recebe objeto de property. No ramo de colisão, SQL muda nome/value/timestamp e preserva description. · 001,003 |
| `DELETE /api/v1/devices/{device_id}/properties/{id}` | C | C | P | C | P | P | C | P | P | P | A | A | P | — | [RemoveAsync:85](../../../../src/Api/Controllers/PropertiesController.cs#L85) · [HTTP](../../../contracts/openapi.json#L6045) · Verifica device, exclui por device e id. Não distingue property ausente. |

## Settings

Fonte funcional [settings, linhas 11–26](../../../knowledge/settings.md). Persistência em [SettingsRepository](../../../../src/Data.Repositories/Repositories/SettingsRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/settings` | C | C | N | C | C | P | C | P | N | N | A | A | P | — | [GetAllSettingsAsync:9](../../../../src/Api/Controllers/SettingsController.cs#L9) · [HTTP](../../../contracts/openapi.json#L6435) · Lê Settings, vazio 204. |
| `PUT /api/v1/settings/{name}` | C | D | P | P | C | P | C | D | P | P | A | A | P | — | [UpdateValueSettingAsync:23](../../../../src/Api/Controllers/SettingsController.cs#L23) · [HTTP](../../../contracts/openapi.json#L6548) · Só altera value. Zero linhas gera KeyNotFoundException. · 001,007 |
| `PUT /api/v1/settings` | C | D | P | P | C | P | C | D | C | P | A | A | P | — | [UpdateValuesSettingAsync:35](../../../../src/Api/Controllers/SettingsController.cs#L35) · [HTTP](../../../contracts/openapi.json#L6477) · Um nome sem linha interrompe o lote e causa rollback. · 001,007 |

## SmartHomeCapability

Fonte funcional [smart-home-capabilities, linhas 7–18](../../../knowledge/smart-home-capabilities.md). Persistência em [CapabilityRepository](../../../../src/Data.Repositories/Repositories/CapabilityRepository.cs).

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/smart-home/{smart_home_id}/capabilities` | C | P | P | P | P | P | D | D | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetAllCapabilities:9](../../../../src/Api/Controllers/SmartHomeCapabilityController.cs#L9) · [HTTP](../../../contracts/openapi.json#L6617) · Filtro de smart home compara nome, retorna array ou 204. Snapshot descreve objeto e 404. · 005,006 |
| `GET /api/v1/smart-home/{smart_home_id}/capabilities/{uid}` | C | P | P | P | P | P | P | P | N | N | A | A | P | [Ar*](../../../specs/AIR-CONDITIONER-STATE.md) | [GetByUidCapabilities:26](../../../../src/Api/Controllers/SmartHomeCapabilityController.cs#L26) · [HTTP](../../../contracts/openapi.json#L6749) · UID é consultado independentemente do parâmetro de smart home. · 005,006 |

## Api

Fonte funcional [utility-endpoints, linhas 7–23](../../../knowledge/utility-endpoints.md). Trabalho hospedado em DatabaseWatchdogService.

| Operação | F | E | V | R | P | S | O | X | L | M | Ex | T | Tr | Norma | Evidência, resultado e achados |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/timezone` | C | C | P | C | N | P | P | P | N | N | A | A | P | — | [MapGet:115](../../../../src/Api/Program.cs#L115) · [HTTP](../../../contracts/openapi.json#L9) · Ecoa zone, calendário do processo. Ausência em Development resultou em 500. · 006 |
| `GET /api/v1/timezone/datetime` | C | C | N | C | N | P | P | P | N | N | A | A | P | — | [MapGet:123](../../../../src/Api/Program.cs#L123) · [HTTP](../../../contracts/openapi.json#L31) · String de hora local com precisão de segundos. · 006 |
| `GET /api/v1/health` | C | C | N | C | N | P | P | P | N | N | P | A | P | — | [MapGet:129](../../../../src/Api/Program.cs#L129) · [HTTP](../../../contracts/openapi.json#L43) · Resposta fixa mesmo com DB indisponível. · 006 |

## Além dos contratos de negócio

| Superfície | Classificação e juízo | Evidência |
|---|---|---|
| Swagger UI, Swagger JSON e MapOpenApi | Interface de desenvolvimento, geradores distintos. Regra de ativação completa, procedência histórica parcial. | Program.cs:13–66,107–113, experimento O1 no relatório. |
| DatabaseWatchdogService | Interno, assíncrono, consulta SELECT 1 a cada dois minutos e encerra após três falhas consecutivas. Health HTTP tem contrato independente. Cobertura funcional suficiente, execução prolongada não verificada. | utility-endpoints.md:13–23, DatabaseWatchdogService.cs:8–11,27–62. |
| Persistência e falha | MySQL por DI, Dapper e transações locais, tratamento geral e específico de Dashboard. Schema completo, triggers e comportamento real em timeout permanecem não verificáveis. | MySqlDependencyInjection.cs:9–36, ExceptionHandler.cs:47–128, EKM-GAP-0002. |
| Administração e acesso | Groups, settings e manutenção de catálogos têm uso administrativo funcional. Identidade/privilégio do operador no ingresso não verificáveis. Controle proprietário Deprecated ainda possui três métodos registrados. | groups.md:11–25, settings.md:11–26, legacy-access-control.md:8–28. |
| Integrações, descoberta e agendamento | Registro explícito via POST e descoberta por consulta. Referências a protocolos e settings MQTT, sem publisher, consumidor, agendador de automação ou provisionamento físico identificado no bootstrap e nas implementações registradas. Operação experimental HTTP adicional não identificada. | Program.cs:68–82, Core/Entities/Device.cs:106, SYSTEM-DOSSIER §§4,7, Dashboard API §6. |
