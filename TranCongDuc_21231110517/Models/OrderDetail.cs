using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TranCongDuc_21231110517.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtTime { get; set; }

        // Lưu mảng Topping dưới dạng chuỗi JSON
        public string? Toppings { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
