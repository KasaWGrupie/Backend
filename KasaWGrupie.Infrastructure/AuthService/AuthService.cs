using FirebaseAdmin;
using FirebaseAdmin.Auth;

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
}