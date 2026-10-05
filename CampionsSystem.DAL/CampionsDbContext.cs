using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL;

public sealed class CampionsDbContext(DbContextOptions<CampionsDbContext> options)
    : DbContext(options)
{
    // Add domain entities after the gym schema is agreed.
}
