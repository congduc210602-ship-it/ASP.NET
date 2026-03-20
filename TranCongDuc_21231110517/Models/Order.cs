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
        public int UserId { get; set; }

        public int? TableId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [StringLength(20)]
        public string? PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Các khóa ngoại liên kết ---
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("TableId")]
        public Table? Table { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
