using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
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
                CreatedAt = DateTime.UtcNow,
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

                // Tính tiền Topping
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

        // ==========================================
        // API MỚI: XEM LỊCH SỬ ĐƠN HÀNG
        // ==========================================
        // GET: api/customer/orders/history/1

        [Authorize]
        [HttpGet("history/{customerId}")]
        public async Task<IActionResult> GetOrderHistory(int customerId)
        {
            // Tìm tất cả đơn hàng của ông khách này, sắp xếp đơn mới nhất lên đầu
            var orders = await _context.Orders
                .Include(o => o.Store) // Lấy thông tin chi nhánh
                .Include(o => o.OrderDetails!) // Lấy danh sách món
                    .ThenInclude(od => od.ProductVariant!)
                        .ThenInclude(pv => pv.Product) // Lấy tên món ăn gốc
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.InvoiceCode,
                    o.OrderType,
                    o.Status,
                    o.TotalAmount,
                    o.CreatedAt,
                    StoreName = o.Store != null ? o.Store.Name : "",
                    // Định dạng lại chi tiết món ăn cho App dễ hiển thị
                    Items = o.OrderDetails!.Select(od => new
                    {
                        ProductName = od.ProductVariant!.Product!.Name,
                        Size = od.ProductVariant.Size,
                        Quantity = od.Quantity,
                        Price = od.PriceAtTime,
                        Toppings = od.Toppings
                    })
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { message = "Khách hàng này chưa có lịch sử đặt hàng." });
            }

            return Ok(orders);
        }
    }
}