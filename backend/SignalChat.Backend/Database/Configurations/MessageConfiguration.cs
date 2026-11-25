using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Text)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(m => m.Time)
            .IsRequired();

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<Dialog>()
            .WithMany(m => m.Messages)
            .HasForeignKey(m => m.DialogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.DialogId, m.Time });
        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => m.Time);
    }
}
