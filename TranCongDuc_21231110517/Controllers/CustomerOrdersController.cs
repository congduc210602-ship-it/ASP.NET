using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;

namespace TranCongDuc_21231110517.Controllers
{
    // Đổi Route chuẩn theo tài liệu Đặc tả Nhóm 2
    [Route("api/customer/orders")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerOrdersController(AppDbContext context)
        {
            _context = context;
        }

        // --- TẠO DTO (Data Transfer Object) ---
        // Giúp Frontend chỉ cần gửi lên những thông tin cần thiết, không gửi thừa
        public class OrderRequestDTO
        {
            public int CustomerId { get; set; } // Tạm thời bắt gửi lên, sau này có Token JWT sẽ lấy ngầm
            public int StoreId { get; set; }
            public string OrderType { get; set; } = "delivery";
            public string? DeliveryAddress { get; set; }
            public string? PaymentMethod { get; set; }
            public List<OrderDetailRequestDTO> Items { get; set; } = new List<OrderDetailRequestDTO>();
        }

        public class OrderDetailRequestDTO
        {
            public int ProductVariantId { get; set; }
            public int Quantity { get; set; }
            public string? Toppings { get; set; }
            public string? Note { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO request)
        {
            // 1. Ngoại lệ 1: Kiểm tra Cửa hàng có tồn tại và đang mở cửa không?
            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null || !store.IsActive)
            {
                return BadRequest("Lỗi: Cửa hàng này không tồn tại hoặc hiện đang đóng cửa!");
            }

            // 2. Khởi tạo cái "Vỏ" Hóa đơn
            var newOrder = new Order
            {
                InvoiceCode = "TCH" + DateTime.Now.ToString("yyMMddHHmmss"), // Mã dạng TCH260403...
                CustomerId = request.CustomerId,
                StoreId = request.StoreId,
                OrderType = request.OrderType,
                DeliveryAddress = request.DeliveryAddress,
                PaymentMethod = request.PaymentMethod,
                Status = "pending", // Đơn mới luôn là pending
                CreatedAt = DateTime.Now,
                OrderDetails = new List<OrderDetail>() // Chuẩn bị sẵn giỏ để đựng món
            };

            decimal totalAmount = 0;

            // 3. Xử lý từng món trong giỏ hàng khách gửi lên
            foreach (var item in request.Items)
            {
                // Tìm món ăn trong DB kèm theo thông tin gốc của nó
                var variant = await _context.ProductVariants
                                    .Include(v => v.Product)
                                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);

                // Ngoại lệ 2: Kiểm tra món ăn có bị khóa/hết hàng không?
                if (variant == null || variant.Product == null || !variant.Product.IsActive)
                {
                    return BadRequest($"Lỗi: Món ăn (Size ID: {item.ProductVariantId}) đã ngừng bán!");
                }

                decimal itemTotal = variant.Price * item.Quantity;

                // Tính tiền Topping (Giống hệt logic cũ của chúng ta)
                if (!string.IsNullOrEmpty(item.Toppings))
                {
                    try
                    {
                        var toppingNames = JsonSerializer.Deserialize<List<string>>(item.Toppings);
                        if (toppingNames != null && toppingNames.Any())
                        {
                            var toppingsInDb = await _context.Toppings
                                .Where(t => toppingNames.Contains(t.Name))
                                .ToListAsync();

                            decimal toppingsPrice = toppingsInDb.Sum(t => t.Price);
                            itemTotal += (toppingsPrice * item.Quantity);
                        }
                    }
                    catch
                    {
                        return BadRequest("Lỗi: Định dạng Topping không hợp lệ.");
                    }
                }

                totalAmount += itemTotal;

                // Gói món ăn bỏ vào Hóa đơn, CHỐT GIÁ tại thời điểm này
                newOrder.OrderDetails.Add(new OrderDetail
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    PriceAtTime = variant.Price,
                    Toppings = item.Toppings,
                    Note = item.Note
                });
            }

            // 4. Chốt tổng tiền an toàn từ Backend
            newOrder.TotalAmount = totalAmount;

            // 5. Lưu tất cả vào Database
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đặt hàng thành công!",
                invoiceCode = newOrder.InvoiceCode,
                totalAmount = newOrder.TotalAmount
            });
        }
    }
}