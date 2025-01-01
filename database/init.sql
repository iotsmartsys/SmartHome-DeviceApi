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
INSERT INTO Devices([Name], [DeviceId], [Description], [Status], [LastActive], [IpAddress], [MacAddress], [CommunicationTypeId], [Platform]) VALUES('Temperature Sensor 1', 'TEMP-001', 'Temperature Sensor 1', 'Active', GETDATE(), '192.168.0.12', '00:11:22:33:44:55', 1, 'ESP32-WROOM-32');

/*Device Capabilities*/
INSERT INTO DeviceCapabilities([DeviceId], [CapabilityId], [Name], [Value]) VALUES(1, 13, 'LuzIndicadora', 'Luz Indicadora', 'ON');

/*SCRIPT TO DEPLOY ON MYSQL*/
CREATE table Capabilities(
    Id int PRIMARY KEY AUTO_INCREMENT,
    `Name` nvarchar(50) NOT NULL,
    `ActuatorMode` nvarchar(50) NULL
);

CREATE TABLE CommunicationTypes(
    Id int PRIMARY KEY AUTO_INCREMENT,
    `Name` nvarchar(50) NOT NULL
);

CREATE table Devices(
    Id int PRIMARY KEY AUTO_INCREMENT,
    `Name` nvarchar(50) NOT NULL,
    `DeviceId` nvarchar(50) NOT NULL,
    `Description` nvarchar(100) NOT NULL,
    `Status` nvarchar(50) NOT NULL,
    `LastActive` datetime NOT NULL,
    `IpAddress` nvarchar(50) NOT NULL,
    `MacAddress` nvarchar(50) NOT NULL,
    `CommunicationTypeId` int NOT NULL,
    `Platform` nvarchar(50) NOT NULL
);

CREATE table DeviceCapabilities(
    Id int PRIMARY KEY AUTO_INCREMENT,
    `DeviceId` int NOT NULL,
    `CapabilityId` int NOT NULL,
    `Name` nvarchar(50) NOT NULL,
    `Description` nvarchar(100) NULL,
    `Value` nvarchar(50) NOT NULL
);

/*Data Seed*/
/*Device Types*/
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Temperature Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Humidity Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Pressure Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Light Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Motion Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Smoke Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Gas Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Water Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Door Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Window Sensor', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Door Lock', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Window Lock', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Light Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Fan Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Heater Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Cooler Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Sprinkler Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Alarm Actuator', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Camera', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Speaker', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Microphone', 'Read');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Display', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Motor', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Pump', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Valve', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Switch', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Relay', 'Write');
INSERT INTO Capabilities(`Name`, `ActuatorMode`) VALUES('Actuator', 'Write');

/*Communication Types*/
INSERT INTO CommunicationTypes(`Name`) VALUES('WebServer');
INSERT INTO CommunicationTypes(`Name`) VALUES('MQTT');
INSERT INTO CommunicationTypes(`Name`) VALUES('HTTP');
INSERT INTO CommunicationTypes(`Name`) VALUES('TCP');
INSERT INTO CommunicationTypes(`Name`) VALUES('UDP');
INSERT INTO CommunicationTypes(`Name`) VALUES('Bluetooth');

/*Devices*/
INSERT INTO Devices(`Name`, `DeviceId`, `Description`, `Status`, `LastActive`, `IpAddress`, `MacAddress`, `CommunicationTypeId`, `Platform`) VALUES('Temperature Sensor 1', 'TEMP-001', 'Temperature Sensor 1', 'Active', NOW(), '192.168.0.12', '00:11:22:33:44:55', 1, 'ESP32-WROOM-32');

/*Device Capabilities*/
INSERT INTO DeviceCapabilities(`DeviceId`, `CapabilityId`, `Name`, `Value`) VALUES(1, 13, 'LuzIndicadora', 'Luz Indicadora', 'ON');