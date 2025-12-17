using AutoNexus.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace AutoNexus.Domain.Entities
{
    public class Client : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(14)] // 000.000.000-00
        public string CPF { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public virtual ICollection<Sale> Purchases { get; set; } = new List<Sale>();
    }
}