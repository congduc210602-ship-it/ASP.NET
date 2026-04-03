using System.ComponentModel.DataAnnotations;

namespace TranCongDuc_21231110517.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}