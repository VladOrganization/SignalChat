using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database;

public class ChatDbContext(DbContextOptions<ChatDbContext> dbContextOptions) : IdentityDbContext<User>(dbContextOptions)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Reaction> Reactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }
}