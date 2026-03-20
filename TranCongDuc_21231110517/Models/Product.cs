using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TranCongDuc_21231110517.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Avatar { get; set; } // Đường dẫn ảnh

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Khóa ngoại liên kết tới bảng Category (Navigation Property)
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}
