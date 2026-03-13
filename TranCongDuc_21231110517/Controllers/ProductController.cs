using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranCongDuc_21231110517.Models;
namespace TranCongDuc_21231110517.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1,Name= "Iphone 17",Price=225000000,Description="Hot",CategoryId=2},
            new Product { Id = 2,Name= "Iphone 16 Pro Max",Price=295000000,Description="Hot",CategoryId=2},
            new Product { Id = 3,Name= "Iphone 17 Air",Price=230000000,Description="Hot",CategoryId=2},
            new Product { Id = 4,Name= "ThinkPad",Price=150000000,Description="Hot",CategoryId=1}
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            return Ok(products); // Ok() tương đương với HTTP status 200
        }

        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }
            return Ok(product);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Product newProduct)
        {
            products.Add(newProduct);
            // Trả về 201 Created
            return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
        }
    }
}
