using AutoNexus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNexus.Infrastructure.Persistence.Configurations
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.Property(s => s.ProtocolNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(s => s.ProtocolNumber)
                .IsUnique();

            builder.Property(s => s.SalePrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(s => s.Client)
                .WithMany(c => c.Purchases)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(s => s.Vehicle)
                .WithMany()
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}