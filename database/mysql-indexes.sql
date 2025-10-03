-- Recommended indexes for MySQL to optimize Capabilities queries
-- Run these once on the SmartHome-Devices database.

-- Helper procedure to create index if missing
DELIMITER //
CREATE PROCEDURE create_index_if_missing(IN tbl VARCHAR(64), IN idx VARCHAR(64), IN ddl TEXT)
BEGIN
	IF NOT EXISTS (
		SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS 
		WHERE table_schema = DATABASE() AND table_name = tbl AND index_name = idx
	) THEN
		SET @s = ddl;
		PREPARE stmt FROM @s;
		EXECUTE stmt;
		DEALLOCATE PREPARE stmt;
	END IF;
END //
DELIMITER ;

-- Capabilities base table lookups and filters
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_Name', 'CREATE INDEX IX_Capabilities_Name ON Capabilities (Name)');
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_DeviceOwner', 'CREATE INDEX IX_Capabilities_DeviceOwner ON Capabilities (DeviceOwner)');
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_Active', 'CREATE INDEX IX_Capabilities_Active ON Capabilities (Active)');
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_UpdatedAt', 'CREATE INDEX IX_Capabilities_UpdatedAt ON Capabilities (UpdatedAt)');
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_CapabilityTypeId', 'CREATE INDEX IX_Capabilities_CapabilityTypeId ON Capabilities (CapabilityTypeId)');
-- Composite for common filters and sorts
CALL create_index_if_missing('Capabilities', 'IX_Capabilities_Active_UpdatedAt', 'CREATE INDEX IX_Capabilities_Active_UpdatedAt ON Capabilities (Active, UpdatedAt)');

-- CapabilityTypes lookup by Name
CALL create_index_if_missing('CapabilityTypes', 'IX_CapabilityTypes_Name', 'CREATE INDEX IX_CapabilityTypes_Name ON CapabilityTypes (Name)');

-- Relationship: Group_RelationShipCapabilities (for JOIN by CapabilityId then GroupId)
CALL create_index_if_missing('Group_RelationShipCapabilities', 'IX_GRC_CapabilityId', 'CREATE INDEX IX_GRC_CapabilityId ON Group_RelationShipCapabilities (CapabilityId)');
CALL create_index_if_missing('Group_RelationShipCapabilities', 'IX_GRC_GroupId', 'CREATE INDEX IX_GRC_GroupId ON Group_RelationShipCapabilities (GroupId)');

-- Groups lookup by Name when needed
CALL create_index_if_missing('Groups', 'IX_Groups_Name', 'CREATE INDEX IX_Groups_Name ON `Groups` (Name)');

-- Relationship: Capabilities_RelationShip_Platforms (for JOIN by CapabilityId then PlatformId, and filter by ReferenceId)
CALL create_index_if_missing('Capabilities_RelationShip_Platforms', 'IX_CRSP_CapabilityId', 'CREATE INDEX IX_CRSP_CapabilityId ON Capabilities_RelationShip_Platforms (CapabilityId)');
CALL create_index_if_missing('Capabilities_RelationShip_Platforms', 'IX_CRSP_PlatformId', 'CREATE INDEX IX_CRSP_PlatformId ON Capabilities_RelationShip_Platforms (PlatformId)');
CALL create_index_if_missing('Capabilities_RelationShip_Platforms', 'IX_CRSP_ReferenceId', 'CREATE INDEX IX_CRSP_ReferenceId ON Capabilities_RelationShip_Platforms (ReferenceId)');

-- Platforms lookup by Name (used in subselects elsewhere)
CALL create_index_if_missing('Platforms', 'IX_Platforms_Name', 'CREATE INDEX IX_Platforms_Name ON Platforms (Name)');

-- Cleanup helper
DROP PROCEDURE create_index_if_missing;
