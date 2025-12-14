using AutoNexus.Domain.Interfaces;

namespace AutoNexus.Domain.Common
{
    public abstract class BaseEntity : ISoftDeletable
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
    }
}