namespace Data.Repositories;

internal static class DashboardQuery
{
    public const string GetAllDashboards = @"
    SELECT Id, Name, Description, LayoutType, IsDefault, DisplayOrder, CreatedAt, UpdatedAt FROM Dashboards ORDER BY DisplayOrder, Id;
    ";

    public const string GetById = @"
    SELECT Id, Name, Description, LayoutType, IsDefault, DisplayOrder, CreatedAt, UpdatedAt FROM Dashboards WHERE Id = @id;
    ";

    public const string GetWriteLock = @"
    SELECT Id FROM DashboardWriteLocks WHERE Id = 1 FOR UPDATE;
    ";

    public const string Insert = @"
    INSERT INTO Dashboards (Name, Description, LayoutType, IsDefault, DisplayOrder, CreatedAt, UpdatedAt)
    VALUES (@Name, @Description, @LayoutType, @IsDefault, @DisplayOrder, @CreatedAt, @UpdatedAt);
    SELECT LAST_INSERT_ID();
    ";

    public const string Update = @"
    UPDATE Dashboards
    SET Name = @Name, Description = @Description, LayoutType = @LayoutType, IsDefault = @IsDefault,
        DisplayOrder = @DisplayOrder, UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
    ";

    public const string RemoveDefault = @"
    UPDATE Dashboards SET IsDefault = FALSE, UpdatedAt = @updatedAt WHERE IsDefault = TRUE AND Id <> @id;
    ";

    public const string Delete = @"
    DELETE FROM Dashboards WHERE Id = @id;
    ";

}
