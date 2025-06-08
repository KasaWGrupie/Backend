using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using UnauthorizedResult = Microsoft.AspNetCore.Mvc.UnauthorizedResult;

namespace KasaWGrupie.Infrastructure.AuthService;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FirebaseAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authService = context.HttpContext.RequestServices.GetService<IAuthService>()!;
        var idToken = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var cancellationToken = context.HttpContext.RequestAborted;
        
        if (string.IsNullOrEmpty(idToken))
        {
            context.Result = new UnauthorizedObjectResult("Authorization header is missing.");
            return;
        }
        
        var authResult = await authService.AuthenticateAsync(idToken, cancellationToken);
        
        if (!authResult.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(authResult.Exception.Message);
        }
    }
}
