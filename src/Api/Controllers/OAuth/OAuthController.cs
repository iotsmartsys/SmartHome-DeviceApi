using System.Security.Cryptography;
using Core.Contracts.Repositories;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SmartHome_Api.Controllers;

[Route("oauth")]
[ApiController]
public class OAuthController(ILogger<OAuthController> logger) : ControllerBase
{

    /// <summary>
    /// OAuth2 authorize endpoint (MVP).
    /// </summary>
    [HttpGet("authorize")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authorize([FromServices] IOAuthAuthorizationCodeRepository authCodeRepo)
    {
        var q = HttpContext.Request.Query;

        var responseType = (string?)q["response_type"];
        var clientId = (string?)q["client_id"];
        var redirectUri = (string?)q["redirect_uri"];
        var state = (string?)q["state"];
        var scope = (string?)q["scope"];

        if (!string.Equals(responseType, "code", StringComparison.Ordinal))
            return BadRequest("invalid_request: response_type must be code");

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(state))
            return BadRequest("invalid_request: missing client_id/redirect_uri/state");

        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var redirect) || !string.Equals(redirect.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            return BadRequest("invalid_request: redirect_uri must be https");

        var userId = "user-1";

        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var expiresAt = DateTime.UtcNow.AddMinutes(2);

        await authCodeRepo.InsertAsync(new Core.Entities.OAuthAuthorizationCode
        {
            Code = code,
            ClientId = clientId,
            RedirectUri = redirectUri,
            UserId = userId,
            Scope = scope ?? "",
            ExpiresAt = expiresAt
        }, HttpContext.RequestAborted);

        var sep = redirectUri.Contains('?') ? "&" : "?";
        var location = $"{redirectUri}{sep}code={Uri.EscapeDataString(code)}&state={Uri.EscapeDataString(state)}";

        Response.Headers.TryAdd("code", code);
        return Redirect(location);
    }

    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Token([FromServices] IOAuthClientRepository oauthClientRepo
    , [FromServices] IOAuthAccessTokenRepository accessTokenRepo
    , [FromServices] IOAuthAuthorizationCodeRepository authCodeRepo
    , CancellationToken cancellationToken)
    {
        // Read form
        var form = await HttpContext.Request.ReadFormAsync();

        var grantType = form["grant_type"].ToString();
        var code = form["code"].ToString();
        var redirectUri = form["redirect_uri"].ToString();

        // Client credentials: Basic Auth preferred
        string? clientId = null;
        string? clientSecret = null;

        var auth = HttpContext.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Using Basic Auth for client credentials");
            var b64 = auth.Substring("Basic ".Length).Trim();
            var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            var parts = raw.Split(':', 2);
            if (parts.Length == 2)
            {
                clientId = parts[0];
                clientSecret = parts[1];
            }
        }
        else
        {
            clientId = form["client_id"].ToString();
            clientSecret = form["client_secret"].ToString();
        }

        if (!string.Equals(grantType, "authorization_code", StringComparison.Ordinal))
        {
            logger.LogWarning("Unsupported grant type: {GrantType}", grantType);
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(redirectUri))
        {
            logger.LogWarning("Missing code or redirect_uri");
            return BadRequest(new { error = "invalid_request", error_description = "missing code or redirect_uri" });
        }

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            logger.LogWarning("Missing client credentials");
            return Unauthorized();
        }

        // 1) Validate client in DB (pseudo)
        var client = await oauthClientRepo.GetByClientIdAsync(clientId, cancellationToken);
        if (client == null || !client.IsActive || client.ClientSecret != clientSecret)
        {
            logger.LogWarning("Invalid client credentials for client_id: {ClientId}", clientId);
            return Unauthorized();
        }

        // 2) Load code from DB (pseudo)
        var authCode = await authCodeRepo.GetByCodeAsync(code, cancellationToken);

        if (authCode == null || authCode.UsedAt != null || authCode.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Invalid authorization code: {Code}", code);
            logger.LogInformation("UsedAt: {UsedAt}, ExpiresAt: {ExpiresAt}, Now: {Now}", authCode?.UsedAt, authCode?.ExpiresAt, DateTime.UtcNow);
            return BadRequest(new { error = "invalid_grant" });
        }

        // Example validation placeholders:
        if (authCode.ClientId != clientId)
        {
            logger.LogWarning("Authorization code client_id mismatch: {Code}", code);
            return BadRequest(new { error = "invalid_grant" });
        }

        if (authCode.RedirectUri != redirectUri)
        {
            logger.LogWarning("Authorization code redirect_uri mismatch: {Code}", code);
            return BadRequest(new { error = "invalid_grant" });
        }

        // 3) Mark code used (single-use)
        await authCodeRepo.MarkUsedAsync(code, DateTime.UtcNow, cancellationToken);

        // 4) Issue access token (MVP: random opaque token)
        var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var expiresIn = 3600;

        // Store token -> user mapping (you will need this!)
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

        // authCode.UserId / authCode.ClientId / authCode.Scope vêm do registro do code validado
        await accessTokenRepo.InsertAsync(new OAuthAccessToken
        {
            AccessToken = accessToken,
            UserId = authCode.UserId,
            ClientId = clientId,
            Scope = authCode.Scope ?? "",
            ExpiresAt = expiresAt
        }, cancellationToken);

        return Ok(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = expiresIn
        });
    }

    /// <summary>
    /// OAuth2 userinfo (MVP) - resolves Bearer token to your UserId
    /// </summary>
    /// <remarks>
    /// This is a protected endpoint that requires a valid Bearer token issued by our /oauth/token endpoint. It demonstrates how to authenticate API requests using the access tokens we issued.
    /// </remarks>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me([FromServices] IOAuthAccessTokenRepository accessTokenRepo, CancellationToken cancellationToken)
    {
        var auth = HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Missing or invalid Authorization header");
            return Unauthorized();
        }

        var token = auth.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Empty Bearer token");
            return Unauthorized();
        }

        var row = await accessTokenRepo.GetByAccessTokenAsync(token, cancellationToken);
        if (row == null || row.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Invalid or expired access token: {Token}", token);
            return Unauthorized();
        }

        return Ok(new
        {
            user_id = row.UserId,
            client_id = row.ClientId,
            scope = row.Scope
        });
    }
}
