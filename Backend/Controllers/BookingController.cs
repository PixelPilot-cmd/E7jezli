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

        // POST: api/Booking  (إنشاء طلب حجز مع فحص التضارب والسعة)
        [HttpPost]
        public async Task<ActionResult<Booking>> Create([FromBody] Booking booking)
        {
            try
            {
                if (booking == null) return BadRequest("بيانات الحجز غير صحيحة.");
                
                // Validate required fields
                if (booking.BusinessId <= 0 || string.IsNullOrWhiteSpace(booking.UserEmail))
                    return BadRequest("BusinessId و UserEmail مطلوبان.");
                if (string.IsNullOrWhiteSpace(booking.Service))
                    return BadRequest("وصف الخدمة مطلوب.");
                if (booking.StartTime >= booking.EndTime)
                    return BadRequest("وقت البدء يجب أن يكون قبل وقت الانتهاء.");

                // Get business details
                var business = await _context.Businesses.FindAsync(booking.BusinessId);
                if (business == null)
                    return NotFound("المؤسسة غير موجودة.");

                // Check capacity
                if (booking.NumberOfPeople > business.Capacity)
                    return BadRequest($"عدد الأشخاص يتجاوز السعة القصوى لهذه المؤسسة ({business.Capacity}).");

                // Check for booking conflicts based on service type
                var hasConflict = await CheckBookingConflict(booking.BusinessId, booking.StartTime, booking.EndTime, business.ServiceType);
                if (hasConflict)
                    return BadRequest("عذراً، هذا الموعد محجوز بالفعل. يرجى اختيار وقت آخر.");

                // Set booking details
                booking.BusinessName = business.Name;
                booking.BusinessImage = business.ImageUrl;
                booking.Status = "pending";
                booking.DateCreated = DateTime.UtcNow;

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking: {ex.Message}");
                return StatusCode(500, $"خطأ في السيرفر: {ex.Message}");
            }
        }

        // GET: api/Booking            (جلب كل الحجوزات أو تصفية)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll([FromQuery] string? email, [FromQuery] int? businessId)
        {
            var query = _context.Bookings.AsQueryable();
            
            if (!string.IsNullOrEmpty(email))
            {
                var normalizedEmail = email.Trim().ToLower();
                query = query.Where(b => b.UserEmail.ToLower() == normalizedEmail);
            }

            if (businessId.HasValue)
            {
                query = query.Where(b => b.BusinessId == businessId.Value);
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

            // Extract string from object if needed
            string status = statusObj?.ToString() ?? "";
            if (status.StartsWith("\"") && status.EndsWith("\""))
                status = status.Substring(1, status.Length - 2);

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

        // Helper method to check for booking conflicts
        private async Task<bool> CheckBookingConflict(int businessId, DateTime startTime, DateTime endTime, string serviceType)
        {
            // If the booking doesn't specify a time or uses default values, do not conflict
            if (startTime == DateTime.MinValue || endTime == DateTime.MinValue)
            {
                return false;
            }

            var existingBookings = await _context.Bookings
                .Where(b => b.BusinessId == businessId && 
                           b.Status != "cancelled" && 
                           b.Status != "rejected")
                .ToListAsync();

            foreach (var existing in existingBookings)
            {
                if (existing.StartTime == DateTime.MinValue || existing.EndTime == DateTime.MinValue)
                    continue;

                // For wedding halls and hotels, they are booked per day.
                // If they are on the same day, they conflict, regardless of the hours!
                if (serviceType == "wedding_hall" || serviceType == "hotel")
                {
                    if (startTime.Date == existing.StartTime.Date)
                        return true;
                }
                // For other services (restaurants, gyms, salons, fields, etc.), they conflict if the time slots overlap
                else
                {
                    // Check for time overlap: (StartA < EndB) and (EndA > StartB)
                    if (startTime < existing.EndTime && endTime > existing.StartTime)
                        return true;
                }
            }

            return false;
        }
    }
}
