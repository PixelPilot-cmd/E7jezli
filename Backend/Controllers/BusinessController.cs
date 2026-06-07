using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E7jezli.Server.Data;
using E7jezli.Server.Models;

namespace E7jezli.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Business
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Business>>> GetBusinesses()
        {
            return await _context.Businesses.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        // POST: api/Business
        [HttpPost]
        public async Task<ActionResult<Business>> PostBusiness(Business business)
        {
            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBusinesses", new { id = business.Id }, business);
        }

        // DELETE: api/Business/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Business/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBusinessStatus(int id, [FromBody] string status)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Business/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Business>> GetBusiness(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound("المؤسسة غير موجودة.");
            }
            return Ok(business);
        }
    }
}
