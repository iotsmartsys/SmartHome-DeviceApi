-- Dashboard API v1, specification SHD-DASHBOARD-API-V1-001 revision 0.3.
-- Additive MySQL DDL; apply only through an authorized schema operation.
-- Requires InnoDB and JSON/generated-column support. Existing source tables are not altered.
-- No automatic startup migration; existing source schema remains EKM-GAP-0002.

CREATE TABLE IF NOT EXISTS dashboard_write_lock (
    id TINYINT UNSIGNED NOT NULL PRIMARY KEY
) ENGINE=InnoDB;
INSERT IGNORE INTO dashboard_write_lock(id) VALUES (1);

CREATE TABLE IF NOT EXISTS dashboards (
    id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    description VARCHAR(255) NULL,
    layout_type VARCHAR(50) NOT NULL DEFAULT 'grid',
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    default_slot TINYINT GENERATED ALWAYS AS (CASE WHEN is_default THEN 1 ELSE NULL END) STORED,
    display_order INT NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NULL,
    UNIQUE KEY ux_dashboards_default (default_slot),
    INDEX idx_dashboards_order (display_order,id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS dashboard_widget_types (
    code VARCHAR(80) COLLATE utf8mb4_bin NOT NULL PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    description VARCHAR(255) NULL,
    compatible_data_types JSON NOT NULL,
    default_data_mode VARCHAR(80) NOT NULL,
    default_config_json JSON NOT NULL,
    enabled BOOLEAN NOT NULL,
    lifecycle VARCHAR(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS dashboard_widgets (
    id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    dashboard_id BIGINT NOT NULL,
    capability_id INT NOT NULL,
    title VARCHAR(120) NULL,
    widget_type VARCHAR(80) COLLATE utf8mb4_bin NOT NULL,
    data_mode VARCHAR(80) NOT NULL DEFAULT 'current_value',
    position_x INT NOT NULL DEFAULT 0,
    position_y INT NOT NULL DEFAULT 0,
    width INT NOT NULL DEFAULT 1,
    height INT NOT NULL DEFAULT 1,
    config_json JSON NOT NULL,
    refresh_interval_seconds INT NULL,
    display_order INT NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NULL,
    CONSTRAINT fk_dashboard_widgets_dashboard FOREIGN KEY (dashboard_id)
        REFERENCES dashboards(id) ON DELETE CASCADE,
    INDEX idx_dashboard_widgets_order (dashboard_id,display_order,id),
    INDEX idx_dashboard_widgets_capability (capability_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Intentionally no FK to Capabilities/Devices: deleted sources must leave a visible widget.
-- Seeds are insert-only; reapplication must not silently reset an existing catalogue's state.

INSERT INTO dashboard_widget_types
(code,name,description,compatible_data_types,default_data_mode,default_config_json,enabled,lifecycle)
SELECT 'value_card','Card de valor','Exibe o valor atual de uma capability.','["numeric","text"]','current_value','{"unit":null,"decimals":1,"showLastUpdated":true}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM dashboard_widget_types WHERE code='value_card');

INSERT INTO dashboard_widget_types
(code,name,description,compatible_data_types,default_data_mode,default_config_json,enabled,lifecycle)
SELECT 'gauge','Gauge','Exibe valor numérico em escala.','["numeric"]','current_value','{"unit":null,"min":0,"max":100,"warningFrom":null,"dangerFrom":null,"decimals":1}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM dashboard_widget_types WHERE code='gauge');

INSERT INTO dashboard_widget_types
(code,name,description,compatible_data_types,default_data_mode,default_config_json,enabled,lifecycle)
SELECT 'state_icon','Ícone de estado','Exibe estado como ícone.','["logical","state"]','current_value','{"unit":null,"invertState":false,"onLabel":"Ligado","offLabel":"Desligado","onIcon":"power-on","offIcon":"power-off","openLabel":"Aberta","closedLabel":"Fechada","openIcon":"door-open","closedIcon":"door-closed","pressedLabel":"Pressionado","releasedLabel":"Liberado","pressedIcon":null,"releasedIcon":null}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM dashboard_widget_types WHERE code='state_icon');

INSERT INTO dashboard_widget_types
(code,name,description,compatible_data_types,default_data_mode,default_config_json,enabled,lifecycle)
SELECT 'status_card','Card de status','Exibe estado textual ou visual.','["logical","state","text","event"]','current_value','{"unit":null,"invertState":false,"onLabel":"Ligado","offLabel":"Desligado","onIcon":"power-on","offIcon":"power-off","openLabel":"Aberta","closedLabel":"Fechada","openIcon":"door-open","closedIcon":"door-closed","pressedLabel":"Pressionado","releasedLabel":"Liberado","pressedIcon":null,"releasedIcon":null}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM dashboard_widget_types WHERE code='status_card');

INSERT INTO dashboard_widget_types
(code,name,description,compatible_data_types,default_data_mode,default_config_json,enabled,lifecycle)
SELECT 'line_chart','Gráfico de linha','Planejado; indisponível na v1.','["numeric"]','history','{}',FALSE,'planned'
WHERE NOT EXISTS (SELECT 1 FROM dashboard_widget_types WHERE code='line_chart');
