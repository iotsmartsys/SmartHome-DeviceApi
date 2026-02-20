namespace Core.Entities;

public class OAuthClient
{
    public long Id { get; set; }
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string Name { get; set; } = "alexa";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
