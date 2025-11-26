using Microsoft.AspNetCore.Identity;
using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<User> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<User> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/Login", "Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
