using AutoNexus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNexus.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
            builder.Property(c => c.CPF).IsRequired().HasMaxLength(14); // 111.222.333-44
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.ZipCode).HasMaxLength(10);
        }
    }
}