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

        // ====== THÊM 2 TRƯỜNG NÀY ======
        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định dạng tiền tệ
        public decimal Price { get; set; } = 0;

        [Required]
        public int Availability { get; set; } = 0;
        // ===============================

        public bool IsActive { get; set; } = true;

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}