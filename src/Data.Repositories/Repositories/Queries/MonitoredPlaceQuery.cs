namespace Data.Repositories;

internal static class MonitoredPlaceQuery
{
    public const string GetAll = @"
        SELECT 
            Id, 
            Identifier, 
            Name, 
            Radius, 
            Latitude, 
            Longitude 
        FROM MonitoredPlaces ";
}