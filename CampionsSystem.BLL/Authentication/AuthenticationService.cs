using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Authentication;

public sealed class AuthenticationService(UserManager<ApplicationUser> users, ApplicationDbContext database,
    UserContextService contexts) : IAuthenticationService
{
    public async Task<CurrentUserContext?> ValidateCredentialsAsync(LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrEmpty(request.Password)
            || request.UserNameOrEmail.Length > 256 || request.Password.Length > 256) return null;

        var identifier = request.UserNameOrEmail.Trim();
        var user = await users.FindByNameAsync(identifier);
        if (user is null)
        {
            // Refuse ambiguous legacy email mappings rather than choosing an account.
            var normalizedEmail = users.NormalizeEmail(identifier);
            var matches = await database.Users.Where(x => x.NormalizedEmail == normalizedEmail)
                .Take(2).ToListAsync(cancellationToken);
            if (matches.Count != 1) return null;
            user = matches[0];
        }
        if (!user.IsActive || user.TwoFactorEnabled || await users.IsLockedOutAsync(user)) return null;
        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            if (users.SupportsUserLockout) await users.AccessFailedAsync(user);
            return null;
        }
        if (users.SupportsUserLockout && user.AccessFailedCount > 0
            && !(await users.ResetAccessFailedCountAsync(user)).Succeeded) return null;
        cancellationToken.ThrowIfCancellationRequested();
        return await contexts.ResolveAsync(user.Id, cancellationToken);
    }
}
