using Microsoft.EntityFrameworkCore;
using AudioProcessor.Domain.Entities;

namespace AudioProcessor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AudioFile> AudioFiles => Set<AudioFile>();
}