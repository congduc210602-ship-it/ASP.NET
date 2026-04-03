using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TranCongDuc_21231110517.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceCode { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; } // Bắt buộc phải có khách hàng

        [Required]
        public int StoreId { get; set; } // Đặt tại chi nhánh nào

        [Required]
        [StringLength(20)]
        public string OrderType { get; set; } = "delivery"; // delivery hoặc pickup

        [StringLength(255)]
        public string? DeliveryAddress { get; set; } // Null nếu là pickup

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0; // Tiền giảm giá

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending"; // pending, preparing, delivering, completed, cancelled

        [StringLength(20)]
        public string? PaymentMethod { get; set; } // cod, vnpay

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Các khóa ngoại liên kết ---
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}