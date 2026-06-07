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

        // POST: api/Booking  (إنشاء طلب حجز) with robust error handling
        [HttpPost]
        public async Task<ActionResult<Booking>> Create([FromBody] Booking booking)
        {
            try
            {
                if (booking == null) return BadRequest("Invalid booking data.");
                // Validate required fields
                if (string.IsNullOrWhiteSpace(booking.BusinessName) || string.IsNullOrWhiteSpace(booking.UserEmail))
                    return BadRequest("BusinessName and UserEmail must be provided.");
                if (string.IsNullOrWhiteSpace(booking.Service))
                    return BadRequest("Service description is required.");
                // Default status is pending approval
                booking.Status = "pending";
                booking.DateCreated = DateTime.UtcNow;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                // Log exception to console (or proper logger in production)
                Console.WriteLine($"Error creating booking: {ex.Message}");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Booking            (جلب كل الحجوزات أو تصفية)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll([FromQuery] string? email, [FromQuery] int? userId, [FromQuery] string? businessName)
        {
            var query = _context.Bookings.AsQueryable();
            
            if (!string.IsNullOrEmpty(email))
            {
                var normalizedEmail = email.Trim().ToLower();
                query = query.Where(b => b.UserEmail.ToLower() == normalizedEmail);
            }
            else if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    var normalizedEmail = user.Email.Trim().ToLower();
                    query = query.Where(b => b.UserEmail.ToLower() == normalizedEmail);
                }
                else
                {
                    return Ok(new List<Booking>());
                }
            }

            if (!string.IsNullOrEmpty(businessName))
            {
                query = query.Where(b => b.BusinessName.ToLower() == businessName.Trim().ToLower());
            }

            var list = await query.OrderByDescending(b => b.DateCreated).ToListAsync();
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
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] object statusObj)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            // Extract string from object if needed (to handle different JSON formats)
            string status = statusObj?.ToString() ?? "";
            if (status.StartsWith("\"") && status.EndsWith("\""))
                status = status.Substring(1, status.Length - 2);

            // Loyalty points: if status switches to completed, award 100 points
            if (booking.Status != "completed" && status == "completed")
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == booking.UserEmail.ToLower());
                if (user != null)
                {
                    user.Points += 100;
                }
            }

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
