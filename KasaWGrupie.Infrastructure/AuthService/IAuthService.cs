namespace KasaWGrupie.Infrastructure.AuthService;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateAsync(string idToken, CancellationToken cancellationToken);
}