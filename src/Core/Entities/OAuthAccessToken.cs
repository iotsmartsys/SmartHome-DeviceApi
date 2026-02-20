namespace Core.Entities;

public class OAuthAccessToken
{
    public long Id { get; set; }
    public string AccessToken { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string Scope { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
