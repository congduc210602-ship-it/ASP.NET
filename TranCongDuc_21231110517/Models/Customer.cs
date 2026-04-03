using System.ComponentModel.DataAnnotations;

namespace TranCongDuc_21231110517.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Points { get; set; } = 0;

        // Bổ sung cho Web
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Password { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
    }
}