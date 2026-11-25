using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Database.Configurations;

public class DialogConfiguratio:IEntityTypeConfiguration<Dialog>
{
   public void Configure(EntityTypeBuilder<Dialog> builder)
   {
      builder.HasKey(m => m.Id);
      builder.Property(x => x.Type)
         .IsRequired();

      builder.Property(x => x.DirectKey)
         .HasMaxLength(80);

      builder.Property(x => x.CreatedAt)
         .IsRequired();
      
      builder.HasIndex(x => x.DirectKey)
         .IsUnique()
         .HasFilter("\"DirectKey\" IS NOT NULL");
      

   }
}