using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E7jezli.Server.Data;
using E7jezli.Server.Models;

namespace E7jezli.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Booking  (إنشاء طلب حجز)
        [HttpPost]
        public async Task<ActionResult<Booking>> Create([FromBody] Booking booking)
        {
            if (booking == null) return BadRequest("Invalid booking data.");
            // Validate required fields
            if (booking.BusinessId == 0 || booking.UserId == 0)
                return BadRequest("BusinessId and UserId must be provided.");
            if (string.IsNullOrWhiteSpace(booking.Service))
                return BadRequest("Service description is required.");
            // الوضع الافتراضي هو انتظار موافقة الشريك
            booking.Status = "Pending";
            booking.BookingDate = DateTime.UtcNow;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        // GET: api/Booking            (جلب كل الحجوزات أو تصفية)
        // يمكن تمرير businessId أو userId كمعاملات استعلام
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll([FromQuery] int? businessId, [FromQuery] int? userId)
        {
            var query = _context.Bookings.AsQueryable();
            if (businessId.HasValue) query = query.Where(b => b.BusinessId == businessId.Value);
            if (userId.HasValue) query = query.Where(b => b.UserId == userId.Value);
            var list = await query.OrderByDescending(b => b.BookingDate).ToListAsync();
            return Ok(list);
        }

        // GET: api/Booking/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetById(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        // PATCH: api/Booking/{id}/status   (موافقة أو رفض الشريك)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            booking.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Booking/{id}   (إلغاء من قبل المستخدم أو حذف من قبل الشريك)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
