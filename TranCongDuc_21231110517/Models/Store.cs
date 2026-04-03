using System.ComponentModel.DataAnnotations;

namespace TranCongDuc_21231110517.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}