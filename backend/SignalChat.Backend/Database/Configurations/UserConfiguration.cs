using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(u => u.Code)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.HasIndex(u => u.Code)
            .IsUnique();

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(36);

        builder.Property(u => u.RefreshTokenExpiry);

    }
}
