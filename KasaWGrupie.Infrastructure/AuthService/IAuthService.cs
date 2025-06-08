using Microsoft.AspNetCore.Http;

namespace KasaWGrupie.Infrastructure.AuthService;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateAsync(string idToken, CancellationToken cancellationToken);
    Task<string> GetEmailFromAuthTokenAsync(HttpContext context, CancellationToken cancellationToken);
    Task<int> GetUserIdFromAuthTokenAsync(HttpContext context, CancellationToken cancellationToken);
    Task<int> GetUserIdFromAuthTokenAsync(HttpContext context) => GetUserIdFromAuthTokenAsync(context, context.RequestAborted);
}