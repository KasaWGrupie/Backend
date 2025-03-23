using System.Diagnostics.CodeAnalysis;
using FirebaseAdmin.Auth;

namespace KasaWGrupie.Infrastructure.AuthService;

public class AuthenticationResult
{
    [MemberNotNullWhen(true, nameof(FirebaseToken))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsAuthenticated { get; private init; }
    public FirebaseToken? FirebaseToken { get; private init; }
    public Exception? Exception { get; private init; }

    private AuthenticationResult() {}

    public static AuthenticationResult Success(FirebaseToken token) =>
        new()
        {
            IsAuthenticated = true,
            FirebaseToken = token,
        };

    public static AuthenticationResult Failure(Exception exception) =>
        new()
        {
            IsAuthenticated = false,
            Exception = exception,
        };
}