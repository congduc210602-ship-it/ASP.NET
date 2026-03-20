using System.ComponentModel.DataAnnotations;

namespace TranCongDuc_21231110517.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "empty";
    }
}
