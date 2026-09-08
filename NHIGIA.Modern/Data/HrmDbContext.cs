using Microsoft.EntityFrameworkCore;

namespace NHIGIA.Modern.Data;

public sealed class HrmDbContext : DbContext
{
    public HrmDbContext(DbContextOptions<HrmDbContext> options)
        : base(options)
    {
    }
}
