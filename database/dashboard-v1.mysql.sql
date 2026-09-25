-- Dashboard API v1, specification SHD-DASHBOARD-API-V1-001 revision 0.3.
-- Additive MySQL DDL; apply only through an authorized schema operation.
-- Requires InnoDB and JSON/generated-column support. Existing source tables are not altered.
-- No automatic startup migration; existing source schema remains EKM-GAP-0002.
-- Fresh schema only: this replaces the rejected lowercase/snake_case DDL,
-- but does not migrate installations on which that earlier DDL was applied.

CREATE TABLE IF NOT EXISTS DashboardWriteLocks (
    Id TINYINT UNSIGNED NOT NULL PRIMARY KEY
) ENGINE=InnoDB;
INSERT IGNORE INTO DashboardWriteLocks(Id) VALUES (1);

CREATE TABLE IF NOT EXISTS Dashboards (
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(120) NOT NULL,
    Description VARCHAR(255) NULL,
    LayoutType VARCHAR(50) NOT NULL DEFAULT 'grid',
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
    DefaultSlot TINYINT GENERATED ALWAYS AS (CASE WHEN IsDefault THEN 1 ELSE NULL END) STORED,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NULL,
    UNIQUE KEY UX_Dashboards_Default (DefaultSlot),
    INDEX IX_Dashboards_DisplayOrder (DisplayOrder,Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS DashboardWidgetTypes (
    Code VARCHAR(80) COLLATE utf8mb4_bin NOT NULL PRIMARY KEY,
    Name VARCHAR(120) NOT NULL,
    Description VARCHAR(255) NULL,
    CompatibleDataTypes JSON NOT NULL,
    DefaultDataMode VARCHAR(80) NOT NULL,
    DefaultConfigJson JSON NOT NULL,
    Enabled BOOLEAN NOT NULL,
    Lifecycle VARCHAR(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS DashboardWidgets (
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    DashboardId BIGINT NOT NULL,
    CapabilityId INT NOT NULL,
    Title VARCHAR(120) NULL,
    WidgetType VARCHAR(80) COLLATE utf8mb4_bin NOT NULL,
    DataMode VARCHAR(80) NOT NULL DEFAULT 'current_value',
    PositionX INT NOT NULL DEFAULT 0,
    PositionY INT NOT NULL DEFAULT 0,
    Width INT NOT NULL DEFAULT 1,
    Height INT NOT NULL DEFAULT 1,
    ConfigJson JSON NOT NULL,
    RefreshIntervalSeconds INT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT FK_DashboardWidgets_Dashboards FOREIGN KEY (DashboardId)
        REFERENCES Dashboards(Id) ON DELETE CASCADE,
    INDEX IX_DashboardWidgets_DisplayOrder (DashboardId,DisplayOrder,Id),
    INDEX IX_DashboardWidgets_CapabilityId (CapabilityId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Intentionally no FK to Capabilities/Devices: deleted sources must leave a visible widget.
-- Seeds are insert-only; reapplication must not silently reset an existing catalogue's state.

INSERT INTO DashboardWidgetTypes
(Code,Name,Description,CompatibleDataTypes,DefaultDataMode,DefaultConfigJson,Enabled,Lifecycle)
SELECT 'value_card','Card de valor','Exibe o valor atual de uma capability.','["numeric","text"]','current_value','{"unit":null,"decimals":1,"showLastUpdated":true}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM DashboardWidgetTypes WHERE Code='value_card');

INSERT INTO DashboardWidgetTypes
(Code,Name,Description,CompatibleDataTypes,DefaultDataMode,DefaultConfigJson,Enabled,Lifecycle)
SELECT 'gauge','Gauge','Exibe valor numérico em escala.','["numeric"]','current_value','{"unit":null,"min":0,"max":100,"warningFrom":null,"dangerFrom":null,"decimals":1}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM DashboardWidgetTypes WHERE Code='gauge');

INSERT INTO DashboardWidgetTypes
(Code,Name,Description,CompatibleDataTypes,DefaultDataMode,DefaultConfigJson,Enabled,Lifecycle)
SELECT 'state_icon','Ícone de estado','Exibe estado como ícone.','["logical","state"]','current_value','{"unit":null,"invertState":false,"onLabel":"Ligado","offLabel":"Desligado","onIcon":"power-on","offIcon":"power-off","openLabel":"Aberta","closedLabel":"Fechada","openIcon":"door-open","closedIcon":"door-closed","pressedLabel":"Pressionado","releasedLabel":"Liberado","pressedIcon":null,"releasedIcon":null}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM DashboardWidgetTypes WHERE Code='state_icon');

INSERT INTO DashboardWidgetTypes
(Code,Name,Description,CompatibleDataTypes,DefaultDataMode,DefaultConfigJson,Enabled,Lifecycle)
SELECT 'status_card','Card de status','Exibe estado textual ou visual.','["logical","state","text","event"]','current_value','{"unit":null,"invertState":false,"onLabel":"Ligado","offLabel":"Desligado","onIcon":"power-on","offIcon":"power-off","openLabel":"Aberta","closedLabel":"Fechada","openIcon":"door-open","closedIcon":"door-closed","pressedLabel":"Pressionado","releasedLabel":"Liberado","pressedIcon":null,"releasedIcon":null}',TRUE,'available'
WHERE NOT EXISTS (SELECT 1 FROM DashboardWidgetTypes WHERE Code='status_card');

INSERT INTO DashboardWidgetTypes
(Code,Name,Description,CompatibleDataTypes,DefaultDataMode,DefaultConfigJson,Enabled,Lifecycle)
SELECT 'line_chart','Gráfico de linha','Planejado; indisponível na v1.','["numeric"]','history','{}',FALSE,'planned'
WHERE NOT EXISTS (SELECT 1 FROM DashboardWidgetTypes WHERE Code='line_chart');
