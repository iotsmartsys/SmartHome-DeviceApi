namespace Core.Entities;

public class OAuthAuthorizationCode
{
    public long Id { get; set; }
    public string Code { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Scope { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
