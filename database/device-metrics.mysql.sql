-- MySQL 8 schema for ESP device metrics. Devices must already exist.
CREATE TABLE IF NOT EXISTS DeviceMetricsCurrent (
    DeviceId INT NOT NULL,
    UptimeMs BIGINT NOT NULL,
    CpuCores INT NOT NULL,
    CpuPercent DECIMAL(5,2) NOT NULL,
    MemoryPercent DECIMAL(5,2) NOT NULL,
    TemperatureC DECIMAL(6,2) NOT NULL,
    FrequencyMhz INT NOT NULL,
    Rssi SMALLINT NOT NULL,
    LastDisconnectionUptimeMs BIGINT NOT NULL,
    LastDisconnectionReason INT NOT NULL,
    ConnectionCount BIGINT NOT NULL,
    ReceivedAt DATETIME(3) NOT NULL,
    UpdatedAt DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (DeviceId),
    CONSTRAINT FK_DeviceMetricsCurrent_Devices
        FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE CASCADE,
    CONSTRAINT CK_DeviceMetricsCurrent_CpuPercent CHECK (CpuPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_DeviceMetricsCurrent_MemoryPercent CHECK (MemoryPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_DeviceMetricsCurrent_Rssi CHECK (Rssi BETWEEN -150 AND 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DeviceMetricsHistory (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    DeviceId INT NOT NULL,
    UptimeMs BIGINT NOT NULL,
    CpuPercent DECIMAL(5,2) NOT NULL,
    MemoryPercent DECIMAL(5,2) NOT NULL,
    TemperatureC DECIMAL(6,2) NOT NULL,
    FrequencyMhz INT NOT NULL,
    Rssi SMALLINT NOT NULL,
    ReceivedAt DATETIME(3) NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_DeviceMetricsHistory_Devices
        FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE CASCADE,
    INDEX IX_DeviceMetricsHistory_DeviceId_ReceivedAt (DeviceId, ReceivedAt DESC),
    CONSTRAINT CK_DeviceMetricsHistory_CpuPercent CHECK (CpuPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_DeviceMetricsHistory_MemoryPercent CHECK (MemoryPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_DeviceMetricsHistory_Rssi CHECK (Rssi BETWEEN -150 AND 0)
) ENGINE=InnoDB;
