using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E7jezli.Server.Data;
using E7jezli.Server.Models;

namespace E7jezli.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings([FromQuery] string? email)
        {
            var query = _context.Bookings.AsQueryable();
            if (!string.IsNullOrEmpty(email))
            {
                var normalizedEmail = email.Trim().ToLower();
                query = query.Where(b => _context.Users.Any(u => u.Id == b.UserId && u.Email.ToLower() == normalizedEmail));
            }
            return await query.OrderByDescending(b => b.BookingDate).ToListAsync();
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            return booking;
        }

        // POST: api/Bookings
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(Booking booking)
        {
            booking.BookingDate = DateTime.UtcNow;
            booking.Status = "pending"; // الحالة الافتراضية عند الحجز الجديد
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        // PATCH: api/Bookings/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            // نظام نقاط الولاء: إذا تم قبول الحجز وكان حالته السابقة غير مقبولة
            if (booking.Status != "approved" && newStatus == "approved")
            {
                var user = await _context.Users.FindAsync(booking.UserId);
                if (user != null)
                {
                    user.Points += 50; // منح الزبون 50 نقطة ولاء حقيقية
                }
            }

            booking.Status = newStatus;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
