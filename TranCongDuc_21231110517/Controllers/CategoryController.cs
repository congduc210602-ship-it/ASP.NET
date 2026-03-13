using Microsoft.AspNetCore.Mvc;
using TranCongDuc_21231110517.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TranCongDuc_21231110517.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private static List<Category> categories = new List<Category>
        {
            new Category {Id =1, Name ="Laptop"},
            new Category {Id =2, Name ="Điện thoại"}
        };
        // GET: api/<CategoryController>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(categories);
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public ActionResult<Category> Get(int id)
        {
            var category = categories.FirstOrDefault(c=> c.Id==id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        // POST api/<CategoryController>
        [HttpPost]
        public ActionResult Post([FromBody] Category newCategory)
        {
            categories.Add(newCategory);
            //trả về 201 Created
            return CreatedAtAction(nameof(Get),new {id=newCategory.Id},newCategory);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
