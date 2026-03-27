using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations
{
    public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
    {
        public void Configure(EntityTypeBuilder<Reaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageId)
                .IsRequired();

            builder.Property(x => x.Emoji)
                .IsRequired();


            builder.HasOne(x => x.Message)
                .WithMany(x => x.Reaction)
                .HasForeignKey(x=>x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Reaction)
                .HasForeignKey(x=>x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasIndex(x=>x.MessageId);
        }
    }
}
