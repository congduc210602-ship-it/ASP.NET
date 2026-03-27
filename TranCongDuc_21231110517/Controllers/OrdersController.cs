using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;
using System.Text.Json;
namespace TranCongDuc_21231110517.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // Lấy hóa đơn kèm theo danh sách chi tiết món
            return await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return order;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id) return BadRequest();
            _context.Entry(order).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        /*[HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // Tự động sinh mã hóa đơn nếu chưa có
            if (string.IsNullOrEmpty(order.InvoiceCode))
            {
                order.InvoiceCode = "HD" + DateTime.Now.Ticks.ToString().Substring(10);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }*/
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // 1. Tự động sinh mã hóa đơn
            if (string.IsNullOrEmpty(order.InvoiceCode))
            {
                order.InvoiceCode = "HD" + DateTime.Now.Ticks.ToString().Substring(10);
            }

            decimal finalTotalAmount = 0;

            // 2. Chặn gian lận: Tự động tính toán lại giá tiền dựa trên Database
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                foreach (var detail in order.OrderDetails)
                {
                    // Tra cứu giá gốc của Size món ăn
                    var variant = await _context.ProductVariants.FindAsync(detail.ProductVariantId);
                    if (variant == null)
                    {
                        return BadRequest($"Lỗi: Không tìm thấy Size món ăn có ID = {detail.ProductVariantId}");
                    }

                    // Ép giá bán bằng đúng giá niêm yết trong DB
                    detail.PriceAtTime = variant.Price;

                    // Bắt đầu tính tiền = (Giá size * Số lượng)
                    decimal itemTotal = variant.Price * detail.Quantity;

                    // 3. Xử lý tính thêm tiền Topping (nếu khách có chọn)
                    if (!string.IsNullOrEmpty(detail.Toppings))
                    {
                        try
                        {
                            // Chuyển chuỗi '["Trân châu", "Thạch"]' thành mảng
                            var toppingNames = JsonSerializer.Deserialize<List<string>>(detail.Toppings);
                            if (toppingNames != null && toppingNames.Any())
                            {
                                // Lấy giá của các topping này từ DB
                                var toppingsInDb = await _context.Toppings
                                    .Where(t => toppingNames.Contains(t.Name))
                                    .ToListAsync();

                                // Cộng tiền Topping (Tổng tiền topping * Số lượng ly)
                                decimal toppingsPrice = toppingsInDb.Sum(t => t.Price);
                                itemTotal += (toppingsPrice * detail.Quantity);
                            }
                        }
                        catch
                        {
                            return BadRequest("Định dạng Toppings không hợp lệ. Phải là mảng JSON (vd: [\"Tên topping\"])");
                        }
                    }

                    // Cộng dồn tiền của món này vào Tổng bill
                    finalTotalAmount += itemTotal;
                }
            }

            // 4. Gán tổng tiền tính được cho Hóa đơn (Ghi đè giá trị client gửi)
            order.TotalAmount = finalTotalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}