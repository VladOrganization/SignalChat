using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations;

public class DialogParcipantConfiguration:IEntityTypeConfiguration<DialogParticipant>
{
    public void Configure(EntityTypeBuilder<DialogParticipant> builder)
    {
        builder.HasKey(m =>new { m.DialogId, m.UserId });
        builder.Property(m => m.JoinedAt).IsRequired();
        builder.Property(m => m.LastReadAt);

        builder.HasOne(m => m.Dialog)
            .WithMany(x=>x.Participants)
            .HasForeignKey(m => m.DialogId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(m => m.UserId);
    }
}