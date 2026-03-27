using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranCongDuc_21231110517.Data;
using TranCongDuc_21231110517.Models;

namespace TranCongDuc_21231110517.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToppingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ToppingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topping>>> GetToppings()
        {
            return await _context.Toppings.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Topping>> GetTopping(int id)
        {
            var topping = await _context.Toppings.FindAsync(id);
            if (topping == null) return NotFound();
            return topping;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopping(int id, Topping topping)
        {
            if (id != topping.Id) return BadRequest();
            _context.Entry(topping).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Toppings.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Topping>> PostTopping(Topping topping)
        {
            _context.Toppings.Add(topping);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetTopping", new { id = topping.Id }, topping);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopping(int id)
        {
            var topping = await _context.Toppings.FindAsync(id);
            if (topping == null) return NotFound();
            _context.Toppings.Remove(topping);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}