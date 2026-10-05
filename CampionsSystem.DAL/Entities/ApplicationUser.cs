using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser() => Id = Guid.NewGuid();

    public bool IsActive { get; set; } = true;
}
