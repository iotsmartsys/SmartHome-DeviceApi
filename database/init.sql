/*SCRIPT TO DEPLOY ON SQL SERVER*/
CREATE table Capabilities(
    Id int PRIMARY KEY IDENTITY(1,1),
    [Name] nvarchar(50) NOT NULL,
    [ActuatorMode] nvarchar(50) NULL,
);

CREATE TABLE CommunicationTypes(
    Id int PRIMARY KEY IDENTITY(1,1),
    [Name] nvarchar(50) NOT NULL
);

CREATE table Devices(
    Id int PRIMARY KEY IDENTITY(1,1),
    [Name] nvarchar(50) NOT NULL,
    [DeviceId] nvarchar(50) NOT NULL,
    [Description] nvarchar(100) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [LastActive] datetime NOT NULL,
    [IpAddress] nvarchar(50) NOT NULL,
    [MacAddress] nvarchar(50) NOT NULL,
    [CommunicationTypeId] int NOT NULL FOREIGN KEY REFERENCES CommunicationTypes(Id),
    [Platform] nvarchar(50) NOT NULL,
);

CREATE table DeviceCapabilities(
    Id int PRIMARY KEY IDENTITY(1,1),
    [DeviceId] int NOT NULL FOREIGN KEY REFERENCES Devices(Id),
    [CapabilityId] int NOT NULL FOREIGN KEY REFERENCES Capabilities(Id),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(100) NULL,
    [Value] nvarchar(50) NOT NULL
);

CREATE table DeviceProperties(
    Id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [DeviceId] int NOT NULL FOREIGN KEY REFERENCES Devices(Id),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(100) NULL,
    [Value] nvarchar(50) NOT NULL
);

CREATE TABLE Platforms(
    Id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(100) NULL
);

CREATE TABLE DeviceCapabilities_RelationShip_Platforms(
    Id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [DeviceCapabilityId] int NOT NULL FOREIGN KEY REFERENCES DeviceCapabilities(Id),
    [PlatformId] int NOT NULL FOREIGN KEY REFERENCES Platforms(Id),
);

/*Data Seed*/
/*Device Types*/
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Temperature Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Humidity Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Pressure Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Light Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Motion Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Smoke Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Gas Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Water Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Door Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Window Sensor', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Door Lock', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Window Lock', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Light Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Fan Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Heater Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Cooler Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Sprinkler Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Alarm Actuator', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Camera', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Speaker', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Microphone', 'Read');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Display', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Motor', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Pump', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Valve', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Switch', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Relay', 'Write');
INSERT INTO Capabilities([Name], [ActuatorMode]) VALUES('Actuator', 'Write');

/*Communication Types*/
INSERT INTO CommunicationTypes([Name]) VALUES('WebServer');
INSERT INTO CommunicationTypes([Name]) VALUES('MQTT');
INSERT INTO CommunicationTypes([Name]) VALUES('HTTP');
INSERT INTO CommunicationTypes([Name]) VALUES('TCP');
INSERT INTO CommunicationTypes([Name]) VALUES('UDP');
INSERT INTO CommunicationTypes([Name]) VALUES('Bluetooth');
INSERT INTO CommunicationTypes([Name]) VALUES('Zigbee');
INSERT INTO CommunicationTypes([Name]) VALUES('Zwave');
INSERT INTO CommunicationTypes([Name]) VALUES('LoRa');
INSERT INTO CommunicationTypes([Name]) VALUES('Sigfox');
INSERT INTO CommunicationTypes([Name]) VALUES('NB-IoT');
INSERT INTO CommunicationTypes([Name]) VALUES('LTE-M');
INSERT INTO CommunicationTypes([Name]) VALUES('5G');
INSERT INTO CommunicationTypes([Name]) VALUES('WiFi');

/*Devices*/
INSERT INTO Devices([Name], [DeviceId], [Description], [Status], [LastActive], [IpAddress], [MacAddress], [CommunicationTypeId], [Platform]) VALUES('Central Home Office', 'esp32s3-6AC178', 'Dispositivo responsável por contrlar os dispositivos agregados do Escritório', 'Active', GETDATE(), 'ST:192.168.1.119;AP:192.168.4.1', '84:FC:E6:6A:C1:78', 1, 'ESP32-S3-WROOM-1');

/*Device Capabilities*/
INSERT INTO DeviceCapabilities([DeviceId], [CapabilityId], [Name], [Value]) VALUES(1, 13, 'LuzIndicadora', 'Luz Indicadora', 'ON');

/*Device Configurations*/
INSERT INTO DeviceProperties([DeviceId], [Name], [Value]) VALUES(1, 'SubscribedTopics', 'devices.status');

/*Platforms*/
INSERT INTO Platforms([Name], [Description]) VALUES('Arduino IoT Cloud', 'Plataforma de IoT da Arduino');
INSERT INTO Platforms([Name], [Description]) VALUES('Sinric Pro', 'Plataforma de IoT da Sinric Pro');
INSERT INTO Platforms([Name], [Description]) VALUES('ESP IoT Cloud', 'Plataforma de IoT da Espressif');
INSERT INTO Platforms([Name], [Description]) VALUES('Google Cloud IoT', 'Plataforma de IoT da Google');
INSERT INTO Platforms([Name], [Description]) VALUES('AWS IoT', 'Plataforma de IoT da Amazon');

/*Device Capabilities RelationShip Platforms*/
INSERT INTO DeviceCapabilities_RelationShip_Platforms([DeviceCapabilityId], [PlatformId]) 
VALUES((SELECT Id FROM DeviceCapabilities WHERE Name = @name), (SELECT Id FROM Platforms WHERE Name = 'Sinric Pro'));

/*SqlServer Operators*/
CREATE TABLE Operators(
    Id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Symbol] nvarchar(100) NULL
);

INSERT INTO Operators([Name], [Symbol]) VALUES('EQUAL', '=');
INSERT INTO Operators([Name], [Symbol]) VALUES('GREATER', '>');
INSERT INTO Operators([Name], [Symbol]) VALUES('LESS', '<');
INSERT INTO Operators([Name], [Symbol]) VALUES('GREATER_OR_EQUAL', '>=');
INSERT INTO Operators([Name], [Symbol]) VALUES('LESS_OR_EQUAL', '<=');
INSERT INTO Operators([Name], [Symbol]) VALUES('DIFFERENT', '!=');
INSERT INTO Operators([Name], [Symbol]) VALUES('LIKE', 'LIKE');
INSERT INTO Operators([Name], [Symbol]) VALUES('IN', 'IN');
INSERT INTO Operators([Name], [Symbol]) VALUES('NOT_IN', 'NOT IN');
INSERT INTO Operators([Name], [Symbol]) VALUES('BETWEEN', 'BETWEEN');
INSERT INTO Operators([Name], [Symbol]) VALUES('NOT_BETWEEN', 'NOT BETWEEN');
INSERT INTO Operators([Name], [Symbol]) VALUES('IS_NULL', 'IS NULL');
INSERT INTO Operators([Name], [Symbol]) VALUES('IS_NOT_NULL', 'IS NOT NULL');
INSERT INTO Operators([Name], [Symbol]) VALUES('PERIOD', 'PERIOD');

/*MySql Operators*/
CREATE TABLE Operators(
    Id int PRIMARY KEY AUTO_INCREMENT NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Symbol] nvarchar(100) NULL
);

INSERT INTO Operators(`Name`, `Symbol`) VALUES('EQUAL', '=');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('GREATER', '>');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('LESS', '<');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('GREATER_OR_EQUAL', '>=');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('LESS_OR_EQUAL', '<=');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('DIFFERENT', '!=');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('LIKE', 'LIKE');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('IN', 'IN');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('NOT_IN', 'NOT IN');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('BETWEEN', 'BETWEEN');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('NOT_BETWEEN', 'NOT BETWEEN');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('IS_NULL', 'IS NULL');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('IS_NOT_NULL', 'IS NOT NULL');
INSERT INTO Operators(`Name`, `Symbol`) VALUES('PERIOD', 'PERIOD');
