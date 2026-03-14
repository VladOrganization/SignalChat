using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MessageId)
                .IsRequired();

            builder.Property(m => m.ImageUrl)
                .IsRequired();

            builder.HasOne(m => m.Message)
                .WithMany(m=>m.Images)
                .HasForeignKey(m => m.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasIndex(m => m.MessageId);
        }
    }
}

