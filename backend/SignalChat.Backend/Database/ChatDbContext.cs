using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database;

public class ChatDbContext(DbContextOptions<ChatDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Reaction> Reactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ChatDbContext).Assembly
        );
    }
}