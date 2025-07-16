namespace Core.Entities;

public class MonitoredPlace
{
    public int Id { get; set; }
    public string Identifier { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Radius { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }

    public MonitoredPlace(string identifier, string name, int radius, float latitude, float longitude)
    {
        Identifier = identifier;
        Name = name;
        Radius = radius;
        Latitude = latitude;
        Longitude = longitude;
    }

    public MonitoredPlace() { }
}