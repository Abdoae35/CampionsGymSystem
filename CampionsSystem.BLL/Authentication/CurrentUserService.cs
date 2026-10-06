namespace CampionsSystem.BLL.Authentication;

public sealed class CurrentUserService(ICurrentUserIdentity identity, UserContextService contexts) : ICurrentUserService
{
    public bool IsAuthenticated => identity.IsAuthenticated;
    public Guid? UserId => identity.UserId;

    public Task<CurrentUserContext?> GetAsync(CancellationToken cancellationToken = default) =>
        IsAuthenticated && UserId is Guid userId
            ? contexts.ResolveAsync(userId, cancellationToken)
            : Task.FromResult<CurrentUserContext?>(null);
}
