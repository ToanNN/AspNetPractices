using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UsingWindowsAuthenticationWithSignalR.CustomAuthorizationHandlers;

public class DomainRestrictedRequirement : AuthorizationHandler<DomainRestrictedRequirement, HubInvocationContext>,
    IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DomainRestrictedRequirement requirement,
        HubInvocationContext resource)
    {
        if (!string.IsNullOrEmpty(context.User.Identity?.Name) &&
            IsUserAllowedToPerformAction(resource.HubMethodName, context.User.Identity?.Name))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool IsUserAllowedToPerformAction(string hubMethodName, string? userName)
    {
        if (hubMethodName.Equals("ViewUserHistory") &&
            string.Compare(userName, "admin@qr.com.au", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return true;
        }

        return false;
    }
}