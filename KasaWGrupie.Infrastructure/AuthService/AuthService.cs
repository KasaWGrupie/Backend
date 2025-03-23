using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;

namespace KasaWGrupie.Infrastructure.AuthService;

public class AuthService : IAuthService
{
    
    public async Task<AuthenticationResult> AuthenticateAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var decodedToken = await (FirebaseAuth.DefaultInstance ?? throw new Exception("Firebase app instance not initialized"))
                .VerifyIdTokenAsync(idToken, cancellationToken);
            return AuthenticationResult.Success(decodedToken);
        }
        catch (FirebaseException e)
        {
            return AuthenticationResult.Failure(e);
        }
        
    }
    
    public async Task<string> GetEmailFromAuthTokenAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var idToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var authResult = await AuthenticateAsync(idToken, cancellationToken);

        if (authResult.IsAuthenticated)
        {
            var decodedToken = authResult.FirebaseToken;
            var email = decodedToken.Claims["email"]?.ToString();
            if (email is null)
                throw new UnauthorizedAccessException("Email address is required.");
            
            return email;
        }
        else
        {
            // zakładamy, że ta metoda jest wykorzystywana tylko po uprzedniej autoryzacji, więc błąd pojawi się tylko w wyjątkowej sytuacji
            throw authResult.Exception;
        }
    }
}