using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;

namespace TranCongDuc_21231110517.Controllers.Auth
{
    [Route("api/customer/auth")]
    [ApiController]
    public class CustomerAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomerAuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. DTO cho Đăng ký
        public class RegisterRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? Address { get; set; }
        }

        // 2. DTO cho Đăng nhập
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        // ===================================
        // API 1: ĐĂNG KÝ (REGISTER)
        // ===================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra email hoặc sđt đã tồn tại chưa
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email || c.Phone == request.Phone);

            if (existingCustomer != null)
            {
                return BadRequest("Lỗi: Email hoặc Số điện thoại này đã được đăng ký!");
            }

            // Mã hóa mật khẩu trước khi lưu vào Database
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newCustomer = new Customer
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Password = passwordHash,
                Address = request.Address,
                Points = 0
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        // ===================================
        // API 2: ĐĂNG NHẬP (LOGIN)
        // ===================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            // Kiểm tra Email và giải mã đối chiếu Mật khẩu
            if (customer == null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.Password))
            {
                return Unauthorized("Lỗi: Sai email hoặc mật khẩu!");
            }

            // Nếu đúng, tiến hành tạo Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            // Gói thông tin của khách hàng vào bên trong Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                    new Claim(ClaimTypes.Name, customer.Name),
                    new Claim(ClaimTypes.Email, customer.Email!),
                    new Claim(ClaimTypes.Role, "Customer")
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token có hạn 7 ngày
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = jwtString,
                customer = new
                {
                    id = customer.Id,
                    name = customer.Name,
                    email = customer.Email,
                    points = customer.Points
                }
            });
        }
    }
}