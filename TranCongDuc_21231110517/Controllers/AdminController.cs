using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;

namespace TranCongDuc_21231110517.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Bơm IWebHostEnvironment để lấy đường dẫn thư mục lưu ảnh
        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. LẤY DANH SÁCH TẤT CẢ ĐƠN HÀNG ĐỂ DUYỆT
        // ==========================================
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.ProductVariant!)
                        .ThenInclude(pv => pv.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        // ==========================================
        // 2. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // ==========================================
        public class UpdateStatusDto
        {
            public string Status { get; set; } = string.Empty;
            // Các trạng thái: pending, preparing, delivering, completed, cancelled
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");

            order.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công!", newStatus = order.Status });
        }

        // ==========================================
        // 3. BÁO CÁO DOANH THU THỐNG KÊ
        // ==========================================
        [HttpGet("reports/revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Chỉ tính tiền những đơn đã giao thành công (completed)
            var query = _context.Orders.Where(o => o.Status == "completed");

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            var totalRevenue = await query.SumAsync(o => o.TotalAmount);
            var totalOrders = await query.CountAsync();

            return Ok(new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        // ==========================================
        // 4. UPLOAD HÌNH ẢNH (MÓN ĂN / BANNER)
        // ==========================================
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn một file ảnh.");

            // Lấy đường dẫn thư mục wwwroot, nếu không có thì tự tạo
            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadsFolder = Path.Combine(webRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file ngẫu nhiên để không bị trùng (tránh việc upload 2 file cùng tên)
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về URL để bạn copy dán vào cột Avatar của Products hoặc Banners
            string imageUrl = "/images/" + uniqueFileName;
            return Ok(new { message = "Upload thành công!", url = imageUrl });
        }
    }
}