# Especificação — Dashboard API v1

**ID:** `SHD-DASHBOARD-API-V1-001`

**Classe da fonte:** Normativa (proposta em rascunho)

**Versão:** 0.1

**Estado do workflow:** Rascunho [`Draft`]

**Implementação:** Não iniciada; fora da autorização desta atuação

**Relação normativa:** Nova [`New`], aditiva aos contratos existentes.

**Origem:** conversa “Design de Dashboard IoT”, de 04/09/2026,
identificador `6a373c18-d4a4-83e9-b423-30e62d60571a`, e ordem explícita
de registro no repositório. “API v1” identifica a API; 0.1 identifica a
revisão documental EKOM.

## Contexto de registro e autoridade

Este documento registra a proposta solicitada, sem aprovar implementação.
As seções 1 a 13 preservam o conteúdo recuperado da proposta; os exemplos
SQL e JSON são desenho proposto, não descrição do banco ou dos endpoints
existentes. As ressalvas de integração e decisões pendentes da seção 16
delimitam sua autoridade. Contradições ainda abertas não devem ser resolvidas
por implementação silenciosa.

A leitura disponibilizada da conversa foi truncada durante a seção 13.4.
O conteúdo recuperado até 13.3 foi preservado; nenhum trecho posterior é
atribuído à conversa sem evidência. As seções 14 a 17 organizam o registro
local conforme EKOM.

O escopo confirmado é dashboard dinâmico por capability, catálogo e
compatibilidade de widgets, CRUD e dados para renderização, com prioridade
para valor atual. Não inclui app Swift, controle/comando de dispositivos,
implementação, migrações executáveis, alteração do banco, testes, deploy
ou mudança transversal de autenticação.

A arquitetura observada permanece controller/modelo HTTP → Core →
repositórios Dapper → MySQL. Os componentes sugeridos nas seções 10 a 12
pertencem ao domínio Dashboard; não autorizam uma camada transversal nova.

As especificações `SHD-CAPABILITY-TYPE-ID-001@0.2`,
`SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1` e
`SHD-SETTINGS-RESET-001@0.1` permanecem preservadas. Este contrato não altera
suas rotas, identificadores ou comportamentos.

## 1. Objetivo

Criar uma API para permitir que o usuário crie dashboards personalizados no IoTSmartSys.

Cada dashboard será composto por widgets. Cada widget representa visualmente uma capability de um dispositivo, usando uma forma de visualização compatível com o tipo de dado daquela capability.

A API deve permitir:

- criar dashboards;
- listar dashboards;
- editar dashboards;
- excluir dashboards;
- adicionar widgets;
- editar widgets;
- remover widgets;
- retornar os dados prontos para renderização no app;
- informar quais widgets são compatíveis com cada tipo de capability.

---

## 2. Conceitos principais

### 2.1 Dashboard

Um dashboard é uma coleção de widgets configuráveis.

Exemplo:

```text
Dashboard: Casa - Visão Geral

Widgets:
- Temperatura da sala como gauge
- Umidade como card de valor
- Porta da frente como ícone de estado
- Luz da garagem como card de status
```

---

### 2.2 Widget

Um widget é uma representação visual de uma capability.

Um widget contém:

```text
- dashboard vinculado
- capability vinculada
- tipo de visualização
- posição no layout
- tamanho no layout
- configurações específicas
```

---

### 2.3 Capability

A capability continua sendo a fonte lógica de dados.

A API de dashboard não deve depender diretamente de nomes como `temperature`, `humidity`, `door`, `relay` etc.

A decisão de quais widgets são permitidos deve ser baseada principalmente no tipo visual de dado da capability.

---

## 3. Enums e contratos compartilhados

### 3.1 DashboardLayoutType

```text
grid
free_grid
list
```

Na v1, usar principalmente:

```text
grid
```

---

### 3.2 CapabilityVisualDataType

```text
numeric
logical
state
text
event
```

Descrição:

| Tipo | Uso |
|---|---|
| `numeric` | Temperatura, umidade, tensão, corrente, potência, nível, luminosidade |
| `logical` | Verdadeiro/falso, presença detectada/não detectada |
| `state` | Estados nomeados: `on/off`, `open/closed`, `online/offline` |
| `text` | Informação textual simples |
| `event` | Último evento, histórico de evento ou timestamp relevante |

---

### 3.3 DashboardWidgetType

Tipos iniciais da v1:

```text
value_card
gauge
line_chart
state_icon
status_card
```

Descrição:

| Widget | Uso |
|---|---|
| `value_card` | Exibe um valor simples |
| `gauge` | Exibe valor numérico em escala |
| `line_chart` | Exibe histórico numérico |
| `state_icon` | Exibe estado como ícone |
| `status_card` | Exibe estado textual/visual |

---

### 3.4 WidgetDataMode

```text
current_value
history
aggregated_history
```

Na v1 inicial, priorizar:

```text
current_value
```

O modo `history` pode ser preparado no contrato, mas implementado depois.

---

## 4. Compatibilidade entre tipo de dado e widget

Regra inicial:

```json
{
  "numeric": [
    "value_card",
    "gauge",
    "line_chart"
  ],
  "logical": [
    "state_icon",
    "status_card"
  ],
  "state": [
    "state_icon",
    "status_card"
  ],
  "text": [
    "value_card",
    "status_card"
  ],
  "event": [
    "status_card"
  ]
}
```

A API deve impedir a criação de widgets incompatíveis.

Exemplo inválido:

```text
capability state/open_closed + widget gauge
```

Exemplo válido:

```text
capability numeric/temperature + widget gauge
```

---

## 5. Modelo de banco de dados

### 5.1 Tabela `dashboards`

```sql
CREATE TABLE dashboards (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,

    user_id BIGINT NULL,

    name VARCHAR(120) NOT NULL,
    description VARCHAR(255) NULL,

    layout_type VARCHAR(50) NOT NULL DEFAULT 'grid',

    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INT NOT NULL DEFAULT 0,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL,

    INDEX idx_dashboards_user_id (user_id),
    INDEX idx_dashboards_display_order (display_order)
);
```

Observações:

- `user_id` fica `NULL` na v1 se ainda não houver multiusuário plenamente resolvido.
- `is_default` permite definir o dashboard principal.
- `display_order` permite ordenação no app.

---

### 5.2 Tabela `dashboard_widgets`

```sql
CREATE TABLE dashboard_widgets (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,

    dashboard_id BIGINT NOT NULL,

    title VARCHAR(120) NULL,

    device_id BIGINT NULL,
    capability_id BIGINT NOT NULL,

    widget_type VARCHAR(80) NOT NULL,
    data_mode VARCHAR(80) NOT NULL DEFAULT 'current_value',

    position_x INT NOT NULL DEFAULT 0,
    position_y INT NOT NULL DEFAULT 0,
    width INT NOT NULL DEFAULT 1,
    height INT NOT NULL DEFAULT 1,

    config_json JSON NULL,

    refresh_interval_seconds INT NULL,

    display_order INT NOT NULL DEFAULT 0,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL,

    CONSTRAINT fk_dashboard_widgets_dashboard
        FOREIGN KEY (dashboard_id)
        REFERENCES dashboards(id)
        ON DELETE CASCADE,

    INDEX idx_dashboard_widgets_dashboard_id (dashboard_id),
    INDEX idx_dashboard_widgets_capability_id (capability_id),
    INDEX idx_dashboard_widgets_device_id (device_id),
    INDEX idx_dashboard_widgets_display_order (display_order)
);
```

Observações:

- `device_id` pode ser redundante se a capability já aponta para o device, mas ajuda performance e simplifica payload.
- `config_json` guarda configurações específicas por tipo de widget.
- `data_mode` indica se o widget usa valor atual, histórico ou agregado.

---

### 5.3 Tabela `dashboard_widget_types`

```sql
CREATE TABLE dashboard_widget_types (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,

    code VARCHAR(80) NOT NULL UNIQUE,
    name VARCHAR(120) NOT NULL,
    description VARCHAR(255) NULL,

    compatible_data_types JSON NOT NULL,
    default_data_mode VARCHAR(80) NOT NULL DEFAULT 'current_value',
    default_config_json JSON NULL,

    enabled BOOLEAN NOT NULL DEFAULT TRUE,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL
);
```

Registros iniciais sugeridos:

```sql
INSERT INTO dashboard_widget_types
(code, name, description, compatible_data_types, default_data_mode, default_config_json)
VALUES
(
    'value_card',
    'Card de valor',
    'Exibe o valor atual de uma capability.',
    JSON_ARRAY('numeric', 'text'),
    'current_value',
    JSON_OBJECT('decimals', 1)
),
(
    'gauge',
    'Gauge',
    'Exibe um valor numérico dentro de uma escala.',
    JSON_ARRAY('numeric'),
    'current_value',
    JSON_OBJECT('min', 0, 'max', 100, 'decimals', 1)
),
(
    'line_chart',
    'Gráfico de linha',
    'Exibe o histórico de uma capability numérica.',
    JSON_ARRAY('numeric'),
    'history',
    JSON_OBJECT('period', '24h', 'aggregation', 'avg', 'decimals', 1)
),
(
    'state_icon',
    'Ícone de estado',
    'Exibe um estado lógico ou textual usando ícone.',
    JSON_ARRAY('logical', 'state'),
    'current_value',
    JSON_OBJECT()
),
(
    'status_card',
    'Card de status',
    'Exibe o estado atual de uma capability.',
    JSON_ARRAY('logical', 'state', 'text', 'event'),
    'current_value',
    JSON_OBJECT()
);
```

---

## 6. Regras de negócio

### 6.1 Criação de dashboard

Ao criar um dashboard:

- `name` é obrigatório;
- `layout_type` deve assumir `grid` se não informado;
- `display_order` pode ser calculado automaticamente;
- `is_default` deve ser `false` por padrão.

Se o dashboard for criado como padrão, a API deve remover o padrão dos demais dashboards do mesmo usuário/contexto.

---

### 6.2 Criação de widget

Ao criar um widget:

- `dashboard_id` deve existir;
- `capability_id` deve existir;
- `widget_type` deve existir e estar habilitado;
- o tipo visual da capability deve ser compatível com o widget;
- `data_mode` pode ser omitido e herdado do widget type;
- `config_json` deve ser mesclado com `default_config_json`.

Exemplo:

```text
default_config_json + config_json informado pelo usuário = config final
```

---

### 6.3 Atualização de widget

Ao atualizar um widget:

- validar novamente compatibilidade se `capability_id` ou `widget_type` forem alterados;
- permitir alteração de título, posição, tamanho, config e intervalo de atualização;
- não permitir que widget fique vinculado a uma capability inexistente.

---

### 6.4 Exclusão de dashboard

Ao excluir dashboard:

- todos os widgets devem ser removidos automaticamente por `ON DELETE CASCADE`.

---

### 6.5 Exclusão de widget

Ao excluir widget:

- apenas o widget deve ser removido;
- a capability e o device não são afetados.

---

## 7. Endpoints da API

Base path:

```http
/api/v1
```

---

# 7.1 Listar dashboards

```http
GET /api/v1/dashboards
```

Resposta:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Casa - Visão Geral",
      "description": "Resumo dos principais dispositivos",
      "layoutType": "grid",
      "isDefault": true,
      "displayOrder": 0,
      "widgetCount": 5,
      "createdAt": "2026-09-04T22:26:00-03:00",
      "updatedAt": null
    }
  ]
}
```

---

# 7.2 Obter dashboard

```http
GET /api/v1/dashboards/{dashboardId}
```

Resposta:

```json
{
  "id": 1,
  "name": "Casa - Visão Geral",
  "description": "Resumo dos principais dispositivos",
  "layoutType": "grid",
  "isDefault": true,
  "displayOrder": 0,
  "widgets": [
    {
      "id": 10,
      "title": "Temperatura da sala",
      "deviceId": 5,
      "capabilityId": 22,
      "capabilityCode": "temperature",
      "widgetType": "gauge",
      "dataType": "numeric",
      "dataMode": "current_value",
      "position": {
        "x": 0,
        "y": 0,
        "width": 1,
        "height": 1
      },
      "config": {
        "unit": "°C",
        "min": 0,
        "max": 50,
        "decimals": 1
      },
      "refreshIntervalSeconds": null,
      "displayOrder": 0
    }
  ]
}
```

---

# 7.3 Criar dashboard

```http
POST /api/v1/dashboards
```

Request:

```json
{
  "name": "Casa - Visão Geral",
  "description": "Resumo dos principais dispositivos",
  "layoutType": "grid",
  "isDefault": true
}
```

Resposta:

```json
{
  "id": 1,
  "name": "Casa - Visão Geral",
  "description": "Resumo dos principais dispositivos",
  "layoutType": "grid",
  "isDefault": true,
  "displayOrder": 0,
  "createdAt": "2026-09-04T22:26:00-03:00"
}
```

---

# 7.4 Atualizar dashboard

```http
PUT /api/v1/dashboards/{dashboardId}
```

Request:

```json
{
  "name": "Casa - Visão Geral",
  "description": "Resumo atualizado",
  "layoutType": "grid",
  "isDefault": true,
  "displayOrder": 0
}
```

Resposta:

```json
{
  "id": 1,
  "name": "Casa - Visão Geral",
  "description": "Resumo atualizado",
  "layoutType": "grid",
  "isDefault": true,
  "displayOrder": 0,
  "updatedAt": "2026-09-04T22:35:00-03:00"
}
```

---

# 7.5 Excluir dashboard

```http
DELETE /api/v1/dashboards/{dashboardId}
```

Resposta:

```http
204 No Content
```

---

# 7.6 Criar widget

```http
POST /api/v1/dashboards/{dashboardId}/widgets
```

Request:

```json
{
  "title": "Temperatura da sala",
  "deviceId": 5,
  "capabilityId": 22,
  "widgetType": "gauge",
  "position": {
    "x": 0,
    "y": 0,
    "width": 1,
    "height": 1
  },
  "config": {
    "unit": "°C",
    "min": 0,
    "max": 50,
    "warningFrom": 32,
    "dangerFrom": 38,
    "decimals": 1
  },
  "refreshIntervalSeconds": null
}
```

Resposta:

```json
{
  "id": 10,
  "dashboardId": 1,
  "title": "Temperatura da sala",
  "deviceId": 5,
  "capabilityId": 22,
  "capabilityCode": "temperature",
  "widgetType": "gauge",
  "dataType": "numeric",
  "dataMode": "current_value",
  "position": {
    "x": 0,
    "y": 0,
    "width": 1,
    "height": 1
  },
  "config": {
    "unit": "°C",
    "min": 0,
    "max": 50,
    "warningFrom": 32,
    "dangerFrom": 38,
    "decimals": 1
  },
  "refreshIntervalSeconds": null,
  "displayOrder": 0
}
```

---

# 7.7 Atualizar widget

```http
PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId}
```

Request:

```json
{
  "title": "Temperatura ambiente",
  "deviceId": 5,
  "capabilityId": 22,
  "widgetType": "gauge",
  "position": {
    "x": 0,
    "y": 0,
    "width": 2,
    "height": 1
  },
  "config": {
    "unit": "°C",
    "min": 0,
    "max": 60,
    "warningFrom": 32,
    "dangerFrom": 40,
    "decimals": 1
  },
  "refreshIntervalSeconds": 30,
  "displayOrder": 0
}
```

Resposta:

```json
{
  "id": 10,
  "dashboardId": 1,
  "title": "Temperatura ambiente",
  "deviceId": 5,
  "capabilityId": 22,
  "capabilityCode": "temperature",
  "widgetType": "gauge",
  "dataType": "numeric",
  "dataMode": "current_value",
  "position": {
    "x": 0,
    "y": 0,
    "width": 2,
    "height": 1
  },
  "config": {
    "unit": "°C",
    "min": 0,
    "max": 60,
    "warningFrom": 32,
    "dangerFrom": 40,
    "decimals": 1
  },
  "refreshIntervalSeconds": 30,
  "displayOrder": 0
}
```

---

# 7.8 Excluir widget

```http
DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId}
```

Resposta:

```http
204 No Content
```

---

# 7.9 Listar tipos de widgets

```http
GET /api/v1/dashboard-widget-types
```

Resposta:

```json
{
  "items": [
    {
      "code": "value_card",
      "name": "Card de valor",
      "description": "Exibe o valor atual de uma capability.",
      "compatibleDataTypes": [
        "numeric",
        "text"
      ],
      "defaultDataMode": "current_value",
      "defaultConfig": {
        "decimals": 1
      },
      "enabled": true
    },
    {
      "code": "gauge",
      "name": "Gauge",
      "description": "Exibe um valor numérico dentro de uma escala.",
      "compatibleDataTypes": [
        "numeric"
      ],
      "defaultDataMode": "current_value",
      "defaultConfig": {
        "min": 0,
        "max": 100,
        "decimals": 1
      },
      "enabled": true
    }
  ]
}
```

---

# 7.10 Listar capabilities disponíveis para dashboard

```http
GET /api/v1/dashboard-capabilities
```

Resposta:

```json
{
  "items": [
    {
      "deviceId": 5,
      "deviceName": "Sensor da sala",
      "capabilityId": 22,
      "capabilityCode": "temperature",
      "capabilityName": "Temperatura",
      "dataType": "numeric",
      "unit": "°C",
      "semanticType": "temperature",
      "currentValue": 28.4,
      "lastUpdatedAt": "2026-09-04T22:25:40-03:00",
      "compatibleWidgets": [
        "value_card",
        "gauge",
        "line_chart"
      ]
    },
    {
      "deviceId": 8,
      "deviceName": "Porta da frente",
      "capabilityId": 31,
      "capabilityCode": "contact",
      "capabilityName": "Contato",
      "dataType": "state",
      "semanticType": "door_contact",
      "currentValue": "closed",
      "lastUpdatedAt": "2026-09-04T22:24:10-03:00",
      "compatibleWidgets": [
        "state_icon",
        "status_card"
      ]
    }
  ]
}
```

Este endpoint é usado pelo editor do app Swift.

---

# 7.11 Obter widgets compatíveis com uma capability

```http
GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets
```

Resposta:

```json
{
  "capabilityId": 22,
  "capabilityCode": "temperature",
  "dataType": "numeric",
  "compatibleWidgets": [
    {
      "code": "value_card",
      "name": "Card de valor",
      "defaultDataMode": "current_value",
      "defaultConfig": {
        "decimals": 1
      }
    },
    {
      "code": "gauge",
      "name": "Gauge",
      "defaultDataMode": "current_value",
      "defaultConfig": {
        "min": 0,
        "max": 100,
        "decimals": 1
      }
    },
    {
      "code": "line_chart",
      "name": "Gráfico de linha",
      "defaultDataMode": "history",
      "defaultConfig": {
        "period": "24h",
        "aggregation": "avg",
        "decimals": 1
      }
    }
  ]
}
```

---

# 7.12 Obter dados de renderização do dashboard

```http
GET /api/v1/dashboards/{dashboardId}/data
```

Resposta:

```json
{
  "dashboardId": 1,
  "name": "Casa - Visão Geral",
  "layoutType": "grid",
  "generatedAt": "2026-09-04T22:26:00-03:00",
  "widgets": [
    {
      "widgetId": 10,
      "title": "Temperatura da sala",
      "deviceId": 5,
      "capabilityId": 22,
      "capabilityCode": "temperature",
      "widgetType": "gauge",
      "dataType": "numeric",
      "dataMode": "current_value",
      "value": 28.4,
      "unit": "°C",
      "label": "28.4 °C",
      "status": "ok",
      "lastUpdatedAt": "2026-09-04T22:25:40-03:00",
      "position": {
        "x": 0,
        "y": 0,
        "width": 1,
        "height": 1
      },
      "config": {
        "min": 0,
        "max": 50,
        "warningFrom": 32,
        "dangerFrom": 38,
        "decimals": 1
      }
    },
    {
      "widgetId": 11,
      "title": "Porta da frente",
      "deviceId": 8,
      "capabilityId": 31,
      "capabilityCode": "contact",
      "widgetType": "state_icon",
      "dataType": "state",
      "dataMode": "current_value",
      "value": "closed",
      "unit": null,
      "label": "Fechada",
      "icon": "door-closed",
      "status": "ok",
      "lastUpdatedAt": "2026-09-04T22:24:10-03:00",
      "position": {
        "x": 1,
        "y": 0,
        "width": 1,
        "height": 1
      },
      "config": {
        "openLabel": "Aberta",
        "closedLabel": "Fechada",
        "openIcon": "door-open",
        "closedIcon": "door-closed"
      }
    }
  ]
}
```

---

## 8. Estrutura padrão de erro

Todas as respostas de erro devem seguir um padrão simples.

Exemplo:

```json
{
  "error": {
    "code": "INVALID_WIDGET_FOR_CAPABILITY",
    "message": "O widget gauge não é compatível com capabilities do tipo state.",
    "details": {
      "widgetType": "gauge",
      "capabilityId": 31,
      "capabilityDataType": "state"
    }
  }
}
```

---

## 9. Códigos de erro previstos

```text
DASHBOARD_NOT_FOUND
WIDGET_NOT_FOUND
CAPABILITY_NOT_FOUND
DEVICE_NOT_FOUND
WIDGET_TYPE_NOT_FOUND
WIDGET_TYPE_DISABLED
INVALID_WIDGET_FOR_CAPABILITY
INVALID_DASHBOARD_NAME
INVALID_WIDGET_POSITION
INVALID_WIDGET_CONFIG
INVALID_DATA_MODE
```

---

## 10. Serviço principal da API

Criar um serviço de domínio para concentrar as regras:

```text
DashboardService
```

Responsabilidades:

```text
- criar dashboard
- atualizar dashboard
- excluir dashboard
- criar widget
- atualizar widget
- excluir widget
- validar compatibilidade
- montar dados para renderização
```

---

## 11. Resolver de dados do dashboard

Criar um componente separado:

```text
DashboardDataResolver
```

Responsabilidade:

```text
Receber um dashboard + widgets e retornar os dados atuais/históricos necessários para renderização.
```

Na v1 inicial, resolver apenas:

```text
current_value
```

Posteriormente:

```text
history
aggregated_history
```

---

## 12. Compatibilidade de widgets

Criar um componente:

```text
DashboardWidgetCompatibilityResolver
```

Responsabilidade:

```text
Validar se uma capability pode ser exibida usando determinado widgetType.
```

Regra:

```text
capability.dataType precisa estar presente em dashboard_widget_types.compatible_data_types
```

---

## 13. Configuração JSON por widget

### 13.1 Gauge

```json
{
  "unit": "°C",
  "min": 0,
  "max": 50,
  "warningFrom": 32,
  "dangerFrom": 38,
  "decimals": 1
}
```

Campos:

| Campo | Descrição |
|---|---|
| `unit` | Unidade exibida |
| `min` | Valor mínimo da escala |
| `max` | Valor máximo da escala |
| `warningFrom` | Valor a partir do qual o app pode exibir alerta visual |
| `dangerFrom` | Valor a partir do qual o app pode exibir perigo visual |
| `decimals` | Casas decimais |

---

### 13.2 Value card

```json
{
  "unit": "°C",
  "decimals": 1,
  "showLastUpdated": true
}
```

---

### 13.3 State icon

```json
{
  "onIcon": "power-on",
  "offIcon": "power-off",
  "onLabel": "Ligado",
  "offLabel": "Desligado",
  "invertState": false
}
```

Para porta:

```json
{
  "openIcon": "door-open",
  "closedIcon": "door-closed",
  "openLabel": "Aberta",
  "closedLabel": "Fechada",
  "invertState": false
}
```

---

## 14. Requisitos rastreáveis

| ID | Contrato proposto | Referência |
|---|---|---|
| DASH-001 | Criar, listar, consultar, atualizar e excluir dashboards; nome obrigatório e defaults declarados. | 5.1, 6.1, 7.1–7.5 |
| DASH-002 | Manter no máximo um dashboard padrão por usuário/contexto definido. | 6.1; pendência P-01 |
| DASH-003 | Criar, atualizar e excluir widgets vinculados a dashboard e capability existentes. | 5.2, 6.2–6.5, 7.6–7.8 |
| DASH-004 | Validar tipo habilitado e compatibilidade pelo tipo visual, sem depender de nomes de sensores. | 3, 4, 6.2, 12 |
| DASH-005 | Persistir título, posição, tamanho, ordem, intervalo e configuração; combinar defaults com configuração informada. | 5, 6, 13 |
| DASH-006 | Expor catálogo, capabilities disponíveis e widgets compatíveis para o editor. | 7.9–7.11 |
| DASH-007 | Resolver dados atuais para renderização com valor, tipo, unidade, label, status e instante da leitura. | 7.12, 11 |
| DASH-008 | Devolver erros de domínio estruturados e distinguíveis. | 8, 9; pendência P-05 |
| DASH-009 | Excluir widgets do dashboard excluído sem excluir capabilities ou devices. | 5.2, 6.4, 6.5 |
| DASH-010 | Distinguir contrato futuro de histórico e recursos efetivamente habilitados na primeira entrega. | 3.4, 11; pendência P-03 |

## 15. Critérios de aceite e evidências futuras

Nenhum artefato de teste integra esta revisão. Esta autoria não executa
build, testes, chamadas HTTP ou operações de banco. A suíte
`tests/Api.Tests` permanece `Retired`. Os cenários abaixo orientam a análise
e a validação futura, cuja execução depende da autorização operacional
correspondente e de ambiente isolado.

| Critério | Cenário e ação | Resultado observável | Meio de validação |
|---|---|---|---|
| AC-01 / DASH-001 | Criar dashboard com defaults, consultar, editar e excluir; repetir com nome inválido. | Campos persistidos e consultáveis; exclusão 204; nome inválido distinguível. | HTTP e leitura do banco isolado. |
| AC-02 / DASH-002 | Marcar dois dashboards como padrão no mesmo contexto. | Somente um padrão; nenhum contexto alheio alterado. Depende de P-01. | HTTP e inspeção transacional no banco. |
| AC-03 / DASH-003, DASH-004 | Criar gauge numérico e tentar gauge de estado, tipo desabilitado e capability ausente. | Válido persistido; inválidos rejeitados sem persistência parcial. | HTTP e banco isolado. |
| AC-04 / DASH-005 | Salvar e alterar configuração e layout; consultar novamente. | Defaults e sobrescritas recuperados conforme contrato reconciliado em P-06. | Comparação de payloads HTTP. |
| AC-05 / DASH-006, DASH-010 | Consultar catálogo e compatibilidade para cada grupo visual. | Matriz respeitada; recursos adiados não apresentados como utilizáveis. Depende de P-03. | Inspeção de respostas HTTP. |
| AC-06 / DASH-007 | Consultar dados numéricos, lógicos e de estado conhecidos. | Valor JSON e metadados correspondem à fonte; configuração preservada. | Fixture conhecida e resposta HTTP; depende de P-02/P-04. |
| AC-07 / DASH-008 | Solicitar dashboard/widget inexistente e enviar configuração inválida. | Erros identificáveis com código e status definidos em P-05/P-06. | HTTP e inspeção das respostas. |
| AC-08 / DASH-009 | Excluir um widget e depois um dashboard com vários widgets. | Apenas os widgets pertinentes são removidos; devices/capabilities preservados. | Comparação de dados antes/depois no banco isolado. |
| AC-09 / DASH-007 | Consultar fonte sem leitura, offline ou removida após criação do widget. | Comportamento definido em P-04, sem confundir ausência com zero/false. | Fixture controlada e resposta HTTP. |

Critério sem execução é evidência ausente, não aprovação. Os critérios
dependentes de decisão permanecem provisórios até reconciliação e análise.

## 16. Fatos observados, propostas e decisões pendentes

Baseline consultada: `main@648b4ce5c935b4343a3cca682b8f526fcf59249b`,
árvore limpa antes da autoria.

| ID | Fato ou ambiguidade | Decisão necessária antes da implementação |
|---|---|---|
| P-01 | A proposta permite `user_id NULL` e menciona usuário/contexto sem contrato de ownership. | Definir escopo global, usuário ou residência, fonte da identidade e autorização de leitura/escrita; unicidade do padrão deve seguir esse escopo. Não presumir acesso público. |
| P-02 | `Core.Entities.Capability` possui `Id int`, `UID string` e `DeviceId string`; os exemplos propõem `deviceId` numérico e `capabilityCode` não existente como propriedade própria. | Confirmar identificador público do device e origem de `capabilityCode`, `unit`, `semanticType` e tipo visual. Recomenda-se preservar o DeviceId público string e usar Capability.Id para capabilityId; os exemplos numéricos não autorizam mudar o contrato vigente. |
| P-03 | `line_chart` aparece na matriz e catálogo, mas o resolver inicial é somente `current_value`; `history` e `aggregated_history` são posteriores. | Confirmar catálogo inicial; recomenda-se desabilitar line_chart e rejeitar modos históricos até entrega própria. Confirmar também se apenas grid é aceito, mantendo free_grid/list reservados. |
| P-04 | O valor vigente da capability é string e DataType é opcional; UpdatedAt não declara offset no modelo. A proposta não define ausência, conversão inválida, obsolescência ou falha parcial. | Definir conversão JSON, timezone, freshness, status e tratamento de capability removida/inativa/offline; não inferir boolean/número por nome. |
| P-05 | Há middleware global de erros; a proposta introduz envelope próprio sem tabela de status HTTP completa. | Confirmar status por código, erros de recurso aninhado e isolamento do envelope às novas rotas, preservando APIs existentes. |
| P-06 | A proposta não define limites de posição/tamanho/refresh, colisões, merge profundo ou raso, nulls, PUT completo/parcial e semântica genérica de estados. | Confirmar validações, precedência de defaults, unidade/label/ícone, thresholds e invertState, concorrência e política de alterações inválidas. Não tornar valores exemplificativos limites obrigatórios. |
| P-07 | `EKM-GAP-0002` registra ausência de schema MySQL completo e autoridade de migração. | Confirmar integração das tabelas propostas, tipos/FKs e procedimento de migração na etapa apropriada; SQL desta proposta não é migração aprovada. |

A existência de endpoint de histórico não demonstra que o contrato de gráficos,
agregações ou períodos já esteja atendido. Mudança transversal necessária deve
receber delimitação e decisão próprias, sem ser absorvida silenciosamente por
Dashboard. Nenhuma pendência foi convertida em débito técnico aceito.

## 17. Fontes locais e encaminhamento

- [Mapa de conhecimento](../rfc/KNOWLEDGE-MAP.md).
- [Histórico e transações](../rfc/EKM-CHANGELOG.md).
- [Dossiê do sistema](SYSTEM-DOSSIER.md).
- [Capability Types por id](CAPABILITY-TYPE-ID.md).
- [Groups](GROUPS-MAINTENANCE-SUPPORT.md).
- [Reset de settings](DEVICE-SETTINGS-RESET.md).
- [Instruções locais](../../AGENTS.md).
- Fontes factuais: `src/Core/Entities/Capability.cs`,
  `src/Api/Models/Capability.cs` e
  `src/Api/Controllers/CapabilityHistoryController.cs`.
- Perfis consultados diretamente na raiz externa
  `/Users/marcelocostamiranda/source/EKM-guidelines`:
  `roles/REGRAS-COMUNS.md` (modelo 4.7) e
  `roles/AUTOR-DA-ESPECIFICACAO.md`.
  O bootstrap local informa modelo 4.6; não foi alterado nesta autoria.

A autorização atual cobre o registro documental da proposta. A análise formal
de implementabilidade permanece não executada. O próximo estágio é reconciliar
as pendências materiais e analisar a versão resultante; não há classificação
`Ready`, aprovação de implementação nem encerramento `Done` nesta entrega.
