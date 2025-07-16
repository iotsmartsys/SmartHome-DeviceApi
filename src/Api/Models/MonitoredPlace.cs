
namespace Api.Models;

public record class MonitoredPlace(int id, string identifier, string name, int radius, float latitude, float longitude)
{
    public static implicit operator MonitoredPlace(Core.Entities.MonitoredPlace place) =>
        new(place.Id, place.Identifier, place.Name, place.Radius, place.Latitude, place.Longitude);
}